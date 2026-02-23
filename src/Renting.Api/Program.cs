using Microsoft.EntityFrameworkCore;
using Renting.Infrastructure.Persistence;
using Renting.Infrastructure.Repositories;
using Renting.Application.Interfaces;
using MediatR;
using Microsoft.OpenApi.Models;
using Renting.Api.Middleware;
using Renting.Api.HealthChecks;
using Renting.Application.Commands.RentVehicle;
using System;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ⚠️ IMPORTANTE: Configurar Npgsql para usar UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("RentingDatabase");
var useInMemory = builder.Configuration.GetValue<bool?>("UseInMemoryDatabase") 
                  ?? string.IsNullOrEmpty(connectionString);

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ Tratar fechas sin timezone como UTC
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Renting API", 
        Version = "v1",
        Description = "API para gestión de alquiler de vehículos"
    });
});

// DbContext
if (useInMemory)
{
    Console.WriteLine("🔧 Using In-Memory Database");
    builder.Services.AddDbContext<RentingDbContext>(options =>
        options.UseInMemoryDatabase("RentingDb"));
}
else
{
    Console.WriteLine($"🔧 Using PostgreSQL: {connectionString}");
    builder.Services.AddDbContext<RentingDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// AutoMapper & MediatR
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblies(typeof(RentVehicleCommand).Assembly);
});

// Repositories
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Renting API v1");
    c.RoutePrefix = string.Empty;
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
