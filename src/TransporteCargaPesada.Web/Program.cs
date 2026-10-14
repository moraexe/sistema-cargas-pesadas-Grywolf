using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.Infrastructure;
using TransporteCargaPesada.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Base de datos SQLite. La cadena se resuelve al crear el contexto, así las pruebas pueden
// sustituirla por una base de datos propia (ConnectionStrings:DefaultConnection).
builder.Services.AddDbContext<AppDbContext>((proveedor, opciones) =>
    opciones.UseSqlite(InicializacionBaseDatos.ResolverCadenaConexion(
        proveedor.GetRequiredService<IConfiguration>(),
        proveedor.GetRequiredService<IWebHostEnvironment>().ContentRootPath)));

builder.Services.AddScoped<ValidadorReferencias>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ParametroService>();
builder.Services.AddScoped<CargaService>();
builder.Services.AddScoped<ObjetoService>();

builder.Services.AddRouting(opciones => opciones.LowercaseUrls = true);
builder.Services.AddControllers()
    // Evita que los mensajes del deserializador (con nombres de tipos .NET) lleguen al cliente (RN-16).
    .AddJsonOptions(opciones => opciones.AllowInputFormatterExceptionMessages = false)
    .ConfigureApiBehaviorOptions(opciones => opciones.InvalidModelStateResponseFactory = RespuestasError.DesdeModelState);
builder.Services.Configure<MvcOptions>(opciones =>
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddRazorPages();

var app = builder.Build();

app.InicializarBaseDatos();

// Siempre se usa el manejador propio (también en Development) para no exponer pilas de llamadas.
app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = RespuestasError.ManejarExcepcionAsync });

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

/// <summary>Visible para WebApplicationFactory en las pruebas de integración.</summary>
public partial class Program;
