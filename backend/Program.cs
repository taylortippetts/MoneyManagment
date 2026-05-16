using Microsoft.EntityFrameworkCore;
using MoneyManager.API.Data;
using MoneyManager.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Money Manager API", Version = "v1" });
});

// Configure SQLite database
var connectionString = "Data Source=moneymanager.db";
builder.Services.AddDbContextFactory<MoneyManagerDbContext>(options =>
    options.UseSqlite(connectionString));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

// Register services
builder.Services.AddScoped<CsvImportService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var contextFactory = services.GetRequiredService<IDbContextFactory<MoneyManagerDbContext>>();
    
    try
    {
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
        throw;
    }
}

Console.WriteLine("Money Manager API starting on:");
Console.WriteLine("  http://localhost:5000");
Console.WriteLine("  https://localhost:5001");
Console.WriteLine("  Swagger UI: http://localhost:5000/swagger");

app.Run("http://localhost:5000");