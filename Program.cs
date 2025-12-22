using EvChargingSystem.API.Data; // Äîäàéòå öåé ðÿäîê
using Microsoft.EntityFrameworkCore; // Äîäàéòå öåé ðÿäîê
using Npgsql; // Ìîæå çíàäîáèòèñÿ, ÿêùî âèíèêíóòü ïðîáëåìè
using Npgsql.EntityFrameworkCore.PostgreSQL;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString) // Âèêîðèñòîâóºìî ïðîâàéäåð Npgsql äëÿ PostgreSQL
);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Дозволяємо Swagger у будь-якому середовищі (і в Dev, і в Production на Render)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Це зробить Swagger головною сторінкою (без /swagger в кінці)
});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

