using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddDbContext<StarcraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StarcraftDbContext")));

services.AddScoped<IStatisticsService, StatisticsService>();
services.AddScoped<IGameService, GameService>();

// Add services to the container.

services.AddControllers()
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
app.UseAuthorization();
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