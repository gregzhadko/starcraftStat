using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StarcraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StarcraftDbContext")));

builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// Add services to the container.

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AddGameRequestValidator>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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