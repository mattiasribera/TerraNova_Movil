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
builder.Services.AddSwaggerGen(opciones =>
{
    // Boton "Authorize" en Swagger para probar la API con el token del Login
    opciones.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pegue el token que devuelve POST /api/Usuario/Login"
    });
    opciones.AddSecurityRequirement(documento => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });
});

builder.Services.AddDbContext<TerraNovaDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));

builder.Services.AddTransient<ServicioEmail>();
builder.Services.AddSingleton<ServicioImagenes>();

// ===== Seguridad: autenticacion con token JWT =====
builder.Services.AddSingleton<ServicioToken>();
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
// La validacion usa la misma clave con la que ServicioToken firma los tokens
builder.Services.AddOptions<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
        Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .Configure<ServicioToken>((opciones, tokens) =>
    {
        opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = ServicioToken.Emisor,
            ValidateAudience = true,
            ValidAudience = ServicioToken.Emisor,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = tokens.Clave,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

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
    if (InicializadorBaseDatos.Ejecutar(builder.Configuration.GetConnectionString("CadenaSql"), app.Logger))
    {
        // Base recien creada: se llena con datos de demostracion (agentes, propiedades con foto, ventas, visitas)
        using var Alcance = app.Services.CreateScope();
        var Resumen = GeneradorDemo.Generar(Alcance.ServiceProvider.GetRequiredService<TerraNovaDbContext>(), CompletarFotos: true);
        app.Logger.LogWarning("Datos de demostración: {Resumen}", Resumen);
    }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
