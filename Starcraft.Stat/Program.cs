using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models;
using Starcraft.Stat.Services;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
        
Log.Information("Starting Web Application");

var builder = WebApplication.CreateBuilder(args);

var botConfigurationSection = builder.Configuration.GetSection(nameof(BotConfiguration));

builder.Services.Configure<BotConfiguration>(botConfigurationSection);

var services = builder.Services;

services.AddSingleton(Log.Logger);
services.AddHostedService<WebhookService>();

var botConfig = botConfigurationSection.Get<BotConfiguration>()!;
services.AddHttpClient("telegramWebHook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

var connectionString = builder.Configuration.GetConnectionString("StarcraftDbContext")!;
services.AddNpgsql<StarcraftDbContext>(connectionString);

services.AddScoped<IBotHandleService, BotHandleService>();
services.AddScoped<IStatisticsService, StatisticsService>();
services.AddScoped<IGameService, GameService>();

services.AddControllers()
    .AddNewtonsoftJson();
services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

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

app.MapControllerRoute("telegramWebHook", $"bot/{botConfig.BotToken}", new { controller = "Webhook", action = "Post" });

app.MapControllers();
app.Run();