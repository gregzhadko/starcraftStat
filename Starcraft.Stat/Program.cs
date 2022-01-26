using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
Console.WriteLine($"Token: {botConfig.BotToken}");
Console.WriteLine($"Host Address: {botConfig.HostAddress}");

var services = builder.Services;

// There are several strategies for completing asynchronous tasks during startup.
// Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
// We are going to use IHostedService to add and later remove Webhook
services.AddHostedService<ConfigureWebhook>();

// Register named HttpClient to get benefits of IHttpClientFactory
// and consume it with ITelegramBotClient typed client.
// More read:
//  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
//  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
services.AddHttpClient("telegramWebHook")
    .AddTypedClient<ITelegramBotClient>(httpClient
        => new TelegramBotClient(botConfig.BotToken, httpClient));

services.AddDbContext<StarcraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StarcraftDbContext")));

services.AddScoped<IBotHandleService, BotHandleService>();
services.AddScoped<IStatisticsService, StatisticsService>();
services.AddScoped<IGameService, GameService>();

// Add services to the container.

services.AddControllers()
    .AddNewtonsoftJson()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AddGameRequestValidator>());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine("Starcraft started");

app.UseHttpsRedirection();
//app.UseAuthorization();
app.UseRouting();
app.UseCors();

//app.MapControllers();

app.UseEndpoints(endpoints =>
{
    // Configure custom endpoint per Telegram API recommendations:
    // https://core.telegram.org/bots/api#setwebhook
    // If you'd like to make sure that the Webhook request comes from Telegram, we recommend
    // using a secret path in the URL, e.g. https://www.example.com/<token>.
    // Since nobody else knows your bot's token, you can be pretty sure it's us.
    var token = botConfig.BotToken;
    endpoints.MapControllerRoute("telegramWebHook",
        $"bot/{token}",
        new { controller = "Webhook", action = "Post" });
    endpoints.MapControllers();
});

app.MapControllers();

//TODO: make it async

ApplyMigrations(app);

app.Run();

void ApplyMigrations(IHost host)
{
    Console.WriteLine("Start migration");
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StarcraftDbContext>();

    Console.WriteLine("Ensure created...");
    db.Database.EnsureCreated();
    var races = db.Races.ToArray();
    foreach (var race in races)
    {
        Console.WriteLine(race.Name);
    }
}