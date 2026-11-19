using TerraNova.Api.Models;
using TerraNova.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuracion propia de cada PC (cadena de conexion, correo). Este archivo NO se sube a GitHub:
// cada compañero crea el suyo copiando appsettings.Local.example.json
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TerraNovaDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));

builder.Services.AddTransient<ServicioEmail>();
builder.Services.AddSingleton<ServicioImagenes>();

builder.Services.AddCors(
    opciones => {
        opciones.AddPolicy("NuevaPolitica", app => {
            app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    }
    );

var app = builder.Build();

// Primera ejecucion en una PC nueva: crea la base de datos con BaseDatos/ScriptCompleto.sql
try
{
    InicializadorBaseDatos.Ejecutar(builder.Configuration.GetConnectionString("CadenaSql"), app.Logger);
}
catch (Exception ex)
{
    app.Logger.LogError("No se pudo conectar/crear la base de datos: {Error}. " +
        "Revise la cadena 'CadenaSql' en appsettings.Local.json (ver README.md).", ex.Message);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");

// Sirve las imagenes subidas: http://<api>/imagenes/trabajadores/xxx.jpg
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
