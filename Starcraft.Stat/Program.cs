using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
        
Log.Information("Starting Web Application");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();

var services = builder.Services;

services.AddHostedService<WebhookService>();

services.AddHttpClient("telegramWebHook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

var connectionString = builder.Configuration.GetConnectionString("StarcraftDbContext");
services.AddNpgsql<StarcraftDbContext>(connectionString);

services.AddScoped<IBotHandleService, BotHandleService>();
services.AddScoped<IStatisticsService, StatisticsService>();
services.AddScoped<IGameService, GameService>();

services.AddControllers()
    .AddNewtonsoftJson()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AddGameRequestValidator>());

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddHostedService<MigrationsService>();

var app = builder.Build();

app.UseForwardedHeaders(new()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Log.Information("Starcraft started");

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints =>
{
    var token = botConfig.BotToken;
    endpoints.MapControllerRoute("telegramWebHook",
        $"bot/{token}",
        new { controller = "Webhook", action = "Post" });
    endpoints.MapControllers();
});

app.MapControllers();
app.Run();