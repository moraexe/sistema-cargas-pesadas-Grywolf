using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransporteCargaPesada.Web.Data;

namespace TransporteCargaPesada.Tests.Infraestructura;

/// <summary>
/// Levanta la aplicación real en memoria (API + EF Core + SQLite) contra una base de datos SQLite
/// de pruebas, separada de la base de la aplicación (App_Data/transporte.db).
/// Cada instancia usa su propio archivo en %TEMP%, que se elimina al terminar.
/// </summary>
public class FabricaApi : WebApplicationFactory<Program>
{
    private readonly bool _eliminarAlFinal;

    public string RutaBaseDatos { get; }

    /// <summary>Constructor usado por xUnit (IClassFixture exige un único constructor público).</summary>
    public FabricaApi() : this(NuevaRutaBaseDatos(), eliminarAlFinal: true) { }

    private FabricaApi(string rutaBaseDatos, bool eliminarAlFinal)
    {
        RutaBaseDatos = rutaBaseDatos;
        _eliminarAlFinal = eliminarAlFinal;
    }

    /// <summary>Crea una fábrica sobre un archivo concreto; permite simular un reinicio con la misma base.</summary>
    public static FabricaApi SobreBaseDatos(string rutaBaseDatos, bool eliminarAlFinal) => new(rutaBaseDatos, eliminarAlFinal);

    public static string NuevaRutaBaseDatos()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "TransporteCargaPesada.Tests");
        Directory.CreateDirectory(carpeta);
        return Path.Combine(carpeta, $"pruebas-{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuracion) =>
            configuracion.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={RutaBaseDatos}",
            }));
        builder.ConfigureTestServices(servicios =>
            servicios.Configure<MvcOptions>(opciones => opciones.Filters.Add(new FiltroErrorSimulado())));
    }

    /// <summary>Contexto directo sobre la base de pruebas, para verificar restricciones a nivel de SQLite.</summary>
    public AppDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={RutaBaseDatos}").Options);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || !_eliminarAlFinal) return;

        SqliteConnection.ClearAllPools();
        foreach (var archivo in new[] { RutaBaseDatos, RutaBaseDatos + "-wal", RutaBaseDatos + "-shm" })
        {
            try { File.Delete(archivo); } catch (IOException) { /* el archivo temporal se limpiará después */ }
        }
    }
}
