using SapApi.Models;
using Microsoft.OpenApi.Models; // ← NECESARIO para Swagger

var builder = WebApplication.CreateBuilder(args);

// Configuración
builder.Services.Configure<HanaConfig>(builder.Configuration.GetSection("hanaconf"));
builder.Services.Configure<OpcionesConfig>(builder.Configuration.GetSection("opciones"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SAP API", Version = "v1" });
});

var app = builder.Build();

// Middleware de Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
