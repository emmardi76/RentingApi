using Microsoft.EntityFrameworkCore;
using Renting.Infrastructure.Persistence;
using Renting.Infrastructure.Repositories;
using Renting.Application.Interfaces;
using MediatR;
using Microsoft.OpenApi.Models;
using Renting.Api.Middleware;
using Renting.Api.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("RentingDatabase")
    ?? builder.Configuration["ConnectionStrings:RentingDatabase"]
    ?? "Host=postgres;Database=renting_db;Username=renting_user;Password=renting_pass";

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Renting API", Version = "v1" });
});

// DbContext
builder.Services.AddDbContext<RentingDbContext>(options =>
    options.UseNpgsql(connectionString));

// AutoMapper & MediatR
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddMediatR(typeof(Program).Assembly);

// Repositories (interfaces in Application, implementations in Infrastructure)
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();

// Health checks: usar el healthcheck personalizado que consulta RentingDbContext
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgresql");


var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Renting API v1"));
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
