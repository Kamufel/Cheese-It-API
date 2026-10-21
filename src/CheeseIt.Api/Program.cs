using System.Text.Json.Serialization;
using CheeseIt.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CheeseItDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Convenient for local development. TODO: replace with EF Core migrations
    // (dotnet ef migrations add InitialCreate) once the model is stable.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CheeseItDbContext>().Database.EnsureCreated();
}

app.MapControllers();
app.Run();

// Makes the entry point visible to WebApplicationFactory in the test project.
public partial class Program;
