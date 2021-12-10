using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models;
using Starcraft.Stat.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();

builder.Services.AddDbContext<StarcraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StarcraftDbContext")));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// There are several strategies for completing asynchronous tasks during startup.
// Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
// We are going to use IHostedService to add and later remove Webhook
//builder.Services.AddHostedService<ConfigureWebhook>();

// Register named HttpClient to get benefits of IHttpClientFactory
// and consume it with ITelegramBotClient typed client.
// More read:
//  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
//  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
builder.Services.AddHttpClient("telegramWebHook")
    .AddTypedClient<ITelegramBotClient>(httpClient
        => new TelegramBotClient(botConfig.BotToken, httpClient));

// The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
// incoming webhook updates and send serialized responses back.
// Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
//   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-5.0#add-newtonsoftjson-based-json-format-support
//builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

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
        $"api/bot/{token}",
        new { controller = "Webhook", action = "Post" });
    endpoints.MapControllers();
});

app.MapControllers();

//TODO: make it async

//ApplyMigrations(app);

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
    return;
    
    //For the next runs we can use this code

    Console.WriteLine("Trying to get pending migrations");
    var pendingMigrations = db.Database.GetPendingMigrations().ToArray();
    if (!pendingMigrations.Any())
    {
        Console.WriteLine("No migrations to add");
        return;
    }

    try
    {
        Console.WriteLine("Started migration");
        db.Database.Migrate();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}