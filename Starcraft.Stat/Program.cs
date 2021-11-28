using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StarcraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StarcraftDbContext")));


// Add services to the container.

builder.Services.AddControllers();
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
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StarcraftDbContext>();

    var pendingMigrations = db.Database.GetPendingMigrations().ToArray();
    if (!pendingMigrations.Any())
    {
        return;
    }

    try
    {
        db.Database.Migrate();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}