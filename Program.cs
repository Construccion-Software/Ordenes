using Ordenes.Api.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotNetEnv;
using Ordenes.API.Repositories;
using Ordenes.Api.Services;
using Ordenes.Api.BusinessRules;
using Ordenes.Api.BusinessRules.Rules;
using System.Text.Json.Serialization;


// Cargar variables de entorno desde archivo .env (solo si no existen)
if (!File.Exists(".env.local"))
{
    Env.Load(".env");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
        ?? builder.Configuration["MongoDbSettings:ConnectionString"]
        ?? string.Empty;
    options.DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
        ?? builder.Configuration["MongoDbSettings:DatabaseName"]
        ?? string.Empty;
    options.OrdenesCollectionName = Environment.GetEnvironmentVariable("MONGODB_ORDENES_COLLECTION_NAME")
        ?? builder.Configuration["MongoDbSettings:OrdenesCollectionName"]
        ?? string.Empty;
    options.MedicamentosCollectionName = Environment.GetEnvironmentVariable("MONGODB_MEDICAMENTOS_COLLECTION_NAME")
        ?? builder.Configuration["MongoDbSettings:MedicamentosCollectionName"]
        ?? string.Empty;
    options.ProcedimientosCollectionName = Environment.GetEnvironmentVariable("MONGODB_PROCEDIMIENTOS_COLLECTION_NAME")
        ?? builder.Configuration["MongoDbSettings:ProcedimientosCollectionName"]
        ?? string.Empty;
    options.AyudasDiagnosticasCollectionName = Environment.GetEnvironmentVariable("MONGODB_AYUDAS_DIAGNOSTICAS_COLLECTION_NAME")
        ?? builder.Configuration["MongoDbSettings:AyudasDiagnosticasCollectionName"]
        ?? string.Empty;
});

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<OrdenRepository>();
builder.Services.AddScoped<MedicamentoRepository>();
builder.Services.AddScoped<ProcedimientoRepository>();
builder.Services.AddScoped<AyudaDiagnosticaRepository>();
builder.Services.AddScoped<IValidationRule, ItemsUnicosEnOrdenRule>();
builder.Services.AddScoped<IValidationRule, MedicamentosAsociadosRule>();
builder.Services.AddScoped<IValidationRule, ProcedimientosAsociadosRule>();
builder.Services.AddScoped<IValidationRule, AyudasDiagnosticasAsociadasRule>();
builder.Services.AddScoped<IValidationRule, HospitalizacionProcedimientosRule>();
builder.Services.AddScoped<OrdenValidator>();
builder.Services.AddScoped<OrdenService>();
builder.Services.AddScoped<MedicamentoService>();
builder.Services.AddScoped<ProcedimientoService>();
builder.Services.AddScoped<AyudaDiagnosticaService>();

// Agregar CORS para permitir comunicación entre microservicios
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Usar CORS
app.UseCors("AllowAll");

// Habilitar Swagger siempre (útil para testing)
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
