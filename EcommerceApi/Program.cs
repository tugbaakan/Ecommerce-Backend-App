using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using StackExchange.Redis;
using EcommerceApi.Services;
using System.Text.Json.Serialization;
using DotNetEnv;

// Load .env file from the correct path
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
{
    throw new FileNotFoundException("The .env file is missing. Please create one based on .env.example");
}
Env.Load(envPath);

// Validate required environment variables
var requiredEnvVars = new[]
{
    "DB_HOST",
    "DB_PORT",
    "DB_NAME",
    "DB_USERNAME",
    "DB_PASSWORD",
    "REDIS_HOST",
    "REDIS_PORT"
};

var missingVars = requiredEnvVars.Where(var => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(var))).ToList();
if (missingVars.Any())
{
    throw new Exception($"Missing required environment variables: {string.Join(", ", missingVars)}");
}

var builder = WebApplication.CreateBuilder(args);

// Configure logging
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
Directory.CreateDirectory(logsPath); // Ensure logs directory exists

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFile(Path.Combine(logsPath, "app-{Date}.log"));

// Build connection strings from environment variables
var postgresConnectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                             $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                             $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                             $"Username={Environment.GetEnvironmentVariable("DB_USERNAME")};" +
                             $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

var redisConnectionString = $"{Environment.GetEnvironmentVariable("REDIS_HOST")}:{Environment.GetEnvironmentVariable("REDIS_PORT")}";

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with environment variable connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

// Configure Redis with environment variable connection string
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// Add Redis Cache Service
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Create database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

app.Run();
