using Microsoft.EntityFrameworkCore;
using Npgsql;
using TripAnalytics.API.Data;
using TripAnalytics.API.Repositories;
using TripAnalytics.API.Repositories.Interfaces;
using TripAnalytics.API.Services;
using TripAnalytics.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var dataSource = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection"))
    .Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<GeoJsonService>();
builder.Services.AddScoped<TripAggregatorService>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<IZoneService, ZoneService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    var geoJsonService = scope.ServiceProvider.GetRequiredService<GeoJsonService>();
    var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "nyc-zip-code-tabulation-areas-polygons.geojson");
    await geoJsonService.LoadAndSaveAsync(filePath);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var geoJsonService = scope.ServiceProvider.GetRequiredService<GeoJsonService>();
    var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "nyc-zip-code-tabulation-areas-polygons.geojson");

    Console.WriteLine($"GeoJSON path: {filePath}");
    Console.WriteLine($"File exists: {File.Exists(filePath)}");
    await geoJsonService.LoadAndSaveAsync(filePath);
    Console.WriteLine("GeoJSON load completed.");

    var aggregatorService = scope.ServiceProvider.GetRequiredService<TripAggregatorService>();
    var csvPath = Path.Combine(AppContext.BaseDirectory, "Resources", "yellow_tripdata_2015-01.csv");
    await aggregatorService.AggregateAndSaveAsync(csvPath, filePath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors();
app.Run();
