using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TransporteCargaPesada.Web.Data;

public static class InicializacionBaseDatos
{
    /// <summary>
    /// Obtiene la cadena de conexión "DefaultConnection" y, si la ruta del archivo SQLite es relativa,
    /// la resuelve respecto de la carpeta del proyecto (ContentRoot), así funciona igual desde
    /// Visual Studio y desde la línea de comandos. Crea la carpeta si no existe.
    /// </summary>
    public static string ResolverCadenaConexion(IConfiguration configuracion, string contentRoot)
    {
        var cadena = configuracion.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        var builder = new SqliteConnectionStringBuilder(cadena);
        if (!string.IsNullOrEmpty(builder.DataSource) && builder.DataSource != ":memory:" && !Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRoot, builder.DataSource));

        var carpeta = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);

        return builder.ToString();
    }

    /// <summary>Aplica las migraciones pendientes y siembra datos de ejemplo si la base está vacía.</summary>
    public static void InicializarBaseDatos(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        DbSeeder.Sembrar(db);
    }
}
