using EvChargingSystem.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1. Налаштування контексту БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. БЕЗПЕЧНЕ ВИКОНАННЯ МІГРАЦІЙ
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("--> Database Migration Successful!");
    }
    catch (Exception ex)
    {
        // Якщо база не встигла підключитися, сервер просто піде далі, а не вимкнеться
        Console.WriteLine($"--> Error applying migrations: {ex.Message}");
    }
}

// 3. Налаштування Swagger (як головної сторінки)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; 
});

app.UseAuthorization();
app.MapControllers();

app.Run();
