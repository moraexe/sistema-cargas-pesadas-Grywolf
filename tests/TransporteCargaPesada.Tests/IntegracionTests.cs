using System.Net;
using System.Net.Http.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransporteCargaPesada.Tests.Infraestructura;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.DTOs;

namespace TransporteCargaPesada.Tests;

/// <summary>Pruebas de integración API + EF Core + SQLite.</summary>
[Trait("Entidad", "Integracion")]
public class IntegracionTests
{
    [Fact(DisplayName = "CP-INT-001 Los datos persisten en SQLite después de reiniciar la aplicación")]
    [Trait("Tipo", "Integracion")]
    public async Task PersistenciaTrasReinicio()
    {
        var ruta = FabricaApi.NuevaRutaBaseDatos();
        CargaResponse creada;
        int categoriasIniciales;

        using (var primeraEjecucion = FabricaApi.SobreBaseDatos(ruta, eliminarAlFinal: false))
        {
            var api = new ApiPruebas(primeraEjecucion.CreateClient());
            creada = await api.CrearCargaAsync();
            await api.CrearObjetoAsync(creada.Id);
            categoriasIniciales = (await api.Cliente.GetFromJsonAsync<List<CategoriaResponse>>("/api/categorias"))!.Count;
        }

        using var segundaEjecucion = FabricaApi.SobreBaseDatos(ruta, eliminarAlFinal: true);
        var cliente = segundaEjecucion.CreateClient();
        var recuperada = await cliente.GetFromJsonAsync<CargaResponse>($"/api/cargas/{creada.Id}");
        Assert.Equal(creada.Codigo, recuperada!.Codigo);
        Assert.Equal(1, recuperada.CantidadObjetos);

        // La siembra de datos de ejemplo no se repite ni borra datos al reiniciar.
        var categorias = await cliente.GetFromJsonAsync<List<CategoriaResponse>>("/api/categorias");
        Assert.Equal(categoriasIniciales, categorias!.Count);
    }

    [Fact(DisplayName = "CP-INT-002 SQLite impide borrar una categoría referenciada (clave foránea Restrict)")]
    [Trait("Tipo", "Integracion")]
    public async Task IntegridadReferencialEnBaseDeDatos()
    {
        using var fabrica = new FabricaApi();
        var api = new ApiPruebas(fabrica.CreateClient());
        var carga = await api.CrearCargaAsync();

        // Se omite la capa de servicios a propósito: la base de datos debe proteger la integridad por sí misma.
        await using var db = fabrica.CrearContexto();
        var tipoCarga = await db.Categorias.SingleAsync(c => c.Id == carga.TipoCargaId);
        db.Categorias.Remove(tipoCarga);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        await using var db2 = fabrica.CrearContexto();
        var cargaDb = await db2.Cargas.SingleAsync(c => c.Id == carga.Id);
        db2.Cargas.Remove(cargaDb);
        await db2.Database.ExecuteSqlRawAsync(
            "INSERT INTO Objetos (CargaId, Nombre, Cantidad, PesoUnitarioKg) VALUES ({0}, 'Objeto directo', 1, '10')", carga.Id);
        await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }

    [Fact(DisplayName = "CP-INT-003 SQLite rechaza pesos negativos y cantidades en cero (restricciones CHECK)")]
    [Trait("Tipo", "Integracion")]
    public async Task RestriccionesCheck()
    {
        using var fabrica = new FabricaApi();
        var api = new ApiPruebas(fabrica.CreateClient());
        var carga = await api.CrearCargaAsync();
        var objeto = await api.CrearObjetoAsync(carga.Id);

        await using var db = fabrica.CrearContexto();
        var errorPeso = await Assert.ThrowsAsync<SqliteException>(() =>
            db.Database.ExecuteSqlRawAsync("UPDATE Cargas SET PesoTotalKg = '-5' WHERE Id = {0}", carga.Id));
        Assert.Contains("CHECK", errorPeso.Message);

        var errorCantidad = await Assert.ThrowsAsync<SqliteException>(() =>
            db.Database.ExecuteSqlRawAsync("UPDATE Objetos SET Cantidad = 0 WHERE Id = {0}", objeto.Id));
        Assert.Contains("CHECK", errorCantidad.Message);

        var errorTipo = await Assert.ThrowsAsync<SqliteException>(() =>
            db.Database.ExecuteSqlRawAsync("UPDATE Categorias SET TipoCategoria = 'COLOR' WHERE Id = {0}", carga.TipoCargaId));
        Assert.Contains("CHECK", errorTipo.Message);
    }

    [Fact(DisplayName = "CP-INT-004 Flujo completo: categoría → carga → objetos → eliminación ordenada respetando dependencias")]
    [Trait("Tipo", "Integracion")]
    public async Task FlujoCompleto()
    {
        using var fabrica = new FabricaApi();
        var api = new ApiPruebas(fabrica.CreateClient());
        var cliente = api.Cliente;

        var tipoObjeto = await api.CrearCategoriaAsync("TIPO_OBJETO");
        var carga = await api.CrearCargaAsync(c => c["pesoTotalKg"] = 8000m);
        var o1 = await api.CrearObjetoAsync(carga.Id, o => o["categoriaId"] = tipoObjeto.Id);
        var o2 = await api.CrearObjetoAsync(carga.Id, o => { o["cantidad"] = 1; o["pesoUnitarioKg"] = 5000m; });

        var resumen = await cliente.GetFromJsonAsync<CargaResponse>($"/api/cargas/{carga.Id}");
        Assert.Equal(2, resumen!.CantidadObjetos);
        Assert.Equal(8000m, resumen.PesoObjetosKg);

        // La carga está llena: un objeto más se rechaza.
        await (await cliente.PostAsJsonAsync("/api/objetos", ApiPruebas.CuerpoObjeto(carga.Id)))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);

        await (await cliente.DeleteAsync($"/api/categorias/{tipoObjeto.Id}")).DetalleProblemaAsync(HttpStatusCode.Conflict);
        await (await cliente.DeleteAsync($"/api/cargas/{carga.Id}")).DetalleProblemaAsync(HttpStatusCode.Conflict);

        await (await cliente.DeleteAsync($"/api/objetos/{o1.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await cliente.DeleteAsync($"/api/objetos/{o2.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await cliente.DeleteAsync($"/api/cargas/{carga.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await cliente.DeleteAsync($"/api/categorias/{tipoObjeto.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "CP-INT-005 Un error inesperado devuelve 500 sin exponer excepciones ni cadenas de conexión")]
    [Trait("Tipo", "Integracion")]
    public async Task ErrorInternoSinDetalles()
    {
        using var fabrica = new FabricaApi();
        var cliente = fabrica.CreateClient();
        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/cargas");
        solicitud.Headers.Add(FiltroErrorSimulado.Encabezado, "1");

        var respuesta = await cliente.SendAsync(solicitud);
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.InternalServerError);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":500", cuerpo);
        Assert.DoesNotContain("Fallo simulado", cuerpo);
        Assert.DoesNotContain("Data Source", cuerpo);
        Assert.DoesNotContain("Password", cuerpo);
        Assert.DoesNotContain("InvalidOperationException", cuerpo);
        Assert.DoesNotContain(" at ", cuerpo);
    }

    [Fact(DisplayName = "CP-INT-006 Las pruebas usan una base de datos SQLite distinta de la base de la aplicación")]
    [Trait("Tipo", "Integracion")]
    public void BaseDeDatosSeparada()
    {
        using var fabrica = new FabricaApi();
        _ = fabrica.CreateClient();
        // Contexto resuelto desde el contenedor de la aplicación: es el que usan los controladores.
        using var alcance = fabrica.Services.CreateScope();
        var db = alcance.ServiceProvider.GetRequiredService<AppDbContext>();
        var ruta = new SqliteConnectionStringBuilder(db.Database.GetConnectionString()).DataSource;
        Assert.StartsWith(Path.GetTempPath(), ruta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("transporte.db", ruta);
        Assert.True(File.Exists(ruta));
    }
}
