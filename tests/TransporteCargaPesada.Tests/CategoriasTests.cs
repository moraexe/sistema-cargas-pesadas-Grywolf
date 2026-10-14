using System.Net;
using System.Net.Http.Json;
using TransporteCargaPesada.Tests.Infraestructura;
using TransporteCargaPesada.Web.DTOs;

namespace TransporteCargaPesada.Tests;

[Trait("Entidad", "Categorias")]
public class CategoriasTests(FabricaApi fabrica) : IClassFixture<FabricaApi>
{
    private readonly ApiPruebas _api = new(fabrica.CreateClient());
    private HttpClient Cliente => _api.Cliente;

    [Fact(DisplayName = "CP-CAT-001 Listar categorías devuelve 200 y una lista")]
    [Trait("Tipo", "Funcional")]
    public async Task ListarCategorias()
    {
        var respuesta = await Cliente.GetAsync("/api/categorias");
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        var lista = await respuesta.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        Assert.NotEmpty(lista!);
    }

    [Theory(DisplayName = "CP-CAT-002 Filtrar por tipo devuelve solo categorías de ese tipo")]
    [Trait("Tipo", "Funcional")]
    [InlineData("VEHICULO")]
    [InlineData("TIPO_CARGA")]
    [InlineData("TIPO_ENTREGA")]
    [InlineData("tipo_carga")]
    public async Task FiltrarPorTipo(string tipo)
    {
        var lista = await Cliente.GetFromJsonAsync<List<CategoriaResponse>>($"/api/categorias?tipoCategoria={tipo}");
        Assert.NotEmpty(lista!);
        Assert.All(lista!, c => Assert.Equal(tipo.ToUpperInvariant(), c.TipoCategoria));
    }

    [Fact(DisplayName = "CP-CAT-003 Filtrar por tipo inexistente devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task FiltrarPorTipoInvalido()
    {
        var errores = await (await Cliente.GetAsync("/api/categorias?tipoCategoria=AVION")).ErroresValidacionAsync();
        Assert.Contains("tipoCategoria", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAT-004 Filtrar por estado activo devuelve solo categorías con ese estado")]
    [Trait("Tipo", "Funcional")]
    public async Task FiltrarPorActivo()
    {
        var categoria = await _api.CrearCategoriaAsync("UNIDAD_MEDIDA");
        await Cliente.PutAsJsonAsync($"/api/categorias/{categoria.Id}", new
        {
            categoria.TipoCategoria, categoria.Codigo, categoria.Nombre, activo = false,
        });

        var inactivas = await Cliente.GetFromJsonAsync<List<CategoriaResponse>>("/api/categorias?activo=false");
        Assert.Contains(inactivas!, c => c.Id == categoria.Id);
        Assert.All(inactivas!, c => Assert.False(c.Activo));

        var activas = await Cliente.GetFromJsonAsync<List<CategoriaResponse>>("/api/categorias?activo=true");
        Assert.DoesNotContain(activas!, c => c.Id == categoria.Id);
    }

    [Fact(DisplayName = "CP-CAT-005 Crear categoría válida devuelve 201, Location y código en mayúsculas")]
    [Trait("Tipo", "Funcional")]
    public async Task CrearCategoria()
    {
        var codigo = ApiPruebas.CodigoCatalogo("volqueta").ToLowerInvariant();
        var respuesta = await Cliente.PostAsJsonAsync("/api/categorias",
            new { tipoCategoria = "vehiculo", codigo = $"  {codigo} ", nombre = "Volqueta", descripcion = "Vehículo de prueba" });

        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        var creada = await respuesta.Content.ReadFromJsonAsync<CategoriaResponse>();
        Assert.True(creada!.Id > 0);
        Assert.Equal("VEHICULO", creada.TipoCategoria);
        Assert.Equal(codigo.ToUpperInvariant(), creada.Codigo);
        Assert.True(creada.Activo);
        Assert.EndsWith($"/api/categorias/{creada.Id}", respuesta.Headers.Location!.ToString());
    }

    [Fact(DisplayName = "CP-CAT-006 Consultar categoría por id devuelve 200")]
    [Trait("Tipo", "Funcional")]
    public async Task ConsultarPorId()
    {
        var creada = await _api.CrearCategoriaAsync("TIPO_OBJETO");
        var consultada = await Cliente.GetFromJsonAsync<CategoriaResponse>($"/api/categorias/{creada.Id}");
        Assert.Equal(creada.Codigo, consultada!.Codigo);
    }

    [Fact(DisplayName = "CP-CAT-007 Consultar categoría inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task ConsultarInexistente()
    {
        var detalle = await (await Cliente.GetAsync("/api/categorias/999999")).DetalleProblemaAsync(HttpStatusCode.NotFound);
        Assert.Contains("999999", detalle);
    }

    [Fact(DisplayName = "CP-CAT-008 Actualizar categoría devuelve 200 con los nuevos datos")]
    [Trait("Tipo", "Funcional")]
    public async Task ActualizarCategoria()
    {
        var creada = await _api.CrearCategoriaAsync("TIPO_ENTREGA");
        var respuesta = await Cliente.PutAsJsonAsync($"/api/categorias/{creada.Id}",
            new { tipoCategoria = "TIPO_ENTREGA", codigo = creada.Codigo, nombre = "Entrega nocturna", descripcion = "Actualizada" });

        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        var actualizada = await respuesta.Content.ReadFromJsonAsync<CategoriaResponse>();
        Assert.Equal("Entrega nocturna", actualizada!.Nombre);
        Assert.True(actualizada.Activo); // activo omitido conserva el valor actual
    }

    [Fact(DisplayName = "CP-CAT-009 Crear categoría con campos obligatorios vacíos devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CrearConCamposVacios()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/categorias",
            new { tipoCategoria = "", codigo = "   ", nombre = "" })).ErroresValidacionAsync();
        Assert.Contains("tipoCategoria", errores.Keys);
        Assert.Contains("codigo", errores.Keys);
        Assert.Contains("nombre", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAT-010 Crear categoría con tipo no permitido devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CrearConTipoInvalido()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/categorias",
            new { tipoCategoria = "COLOR", codigo = "ROJO", nombre = "Rojo" })).ErroresValidacionAsync();
        Assert.Contains("tipoCategoria", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAT-011 Código duplicado en el mismo tipo devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task CodigoDuplicadoMismoTipo()
    {
        var respuesta = await Cliente.PostAsJsonAsync("/api/categorias",
            new { tipoCategoria = "VEHICULO", codigo = "tractocamion", nombre = "Duplicado" });
        var detalle = await respuesta.DetalleProblemaAsync(HttpStatusCode.Conflict);
        Assert.Contains("TRACTOCAMION", detalle);
    }

    [Fact(DisplayName = "CP-CAT-012 El mismo código en otro tipo de categoría se permite (201)")]
    [Trait("Tipo", "Funcional")]
    public async Task MismoCodigoEnOtroTipo()
    {
        // GENERAL existe como TIPO_CARGA; se crea como TIPO_OBJETO.
        var respuesta = await Cliente.PostAsJsonAsync("/api/categorias",
            new { tipoCategoria = "TIPO_OBJETO", codigo = "GENERAL", nombre = "Objeto general" });
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "CP-CAT-013 Eliminar categoría en uso devuelve 409 y no la elimina")]
    [Trait("Tipo", "Negativa")]
    public async Task EliminarCategoriaEnUso()
    {
        var id = await _api.CategoriaIdAsync("TIPO_CARGA", "GENERAL");
        await _api.CrearCargaAsync(); // garantiza que GENERAL está en uso
        var detalle = await (await Cliente.DeleteAsync($"/api/categorias/{id}")).DetalleProblemaAsync(HttpStatusCode.Conflict);
        Assert.Contains("en uso", detalle);
        await (await Cliente.GetAsync($"/api/categorias/{id}")).AsegurarEstadoAsync(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "CP-CAT-014 Desactivar categoría en uso devuelve 200 y conserva la carga")]
    [Trait("Tipo", "Funcional")]
    public async Task DesactivarCategoriaEnUso()
    {
        var categoria = await _api.CrearCategoriaAsync("TIPO_CARGA");
        var carga = await _api.CrearCargaAsync(c => c["tipoCargaId"] = categoria.Id);

        var respuesta = await Cliente.PutAsJsonAsync($"/api/categorias/{categoria.Id}",
            new { categoria.TipoCategoria, categoria.Codigo, categoria.Nombre, activo = false });
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        Assert.False((await respuesta.Content.ReadFromJsonAsync<CategoriaResponse>())!.Activo);

        var cargaConsultada = await Cliente.GetFromJsonAsync<CargaResponse>($"/api/cargas/{carga.Id}");
        Assert.Equal(categoria.Id, cargaConsultada!.TipoCargaId);
    }

    [Fact(DisplayName = "CP-CAT-015 Cambiar el tipo de una categoría en uso devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task CambiarTipoCategoriaEnUso()
    {
        var categoria = await _api.CrearCategoriaAsync("TIPO_CARGA");
        await _api.CrearCargaAsync(c => c["tipoCargaId"] = categoria.Id);

        var respuesta = await Cliente.PutAsJsonAsync($"/api/categorias/{categoria.Id}",
            new { tipoCategoria = "VEHICULO", categoria.Codigo, categoria.Nombre });
        await respuesta.DetalleProblemaAsync(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "CP-CAT-016 Eliminar categoría sin uso devuelve 204 y luego 404")]
    [Trait("Tipo", "Funcional")]
    public async Task EliminarCategoriaSinUso()
    {
        var categoria = await _api.CrearCategoriaAsync("UNIDAD_MEDIDA");
        await (await Cliente.DeleteAsync($"/api/categorias/{categoria.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await Cliente.GetAsync($"/api/categorias/{categoria.Id}")).AsegurarEstadoAsync(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "CP-CAT-017 Código de categoría con formato inválido devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("1ABC")]
    [InlineData("CON ESPACIO")]
    [InlineData("CON-GUION")]
    [InlineData("X")]
    public async Task CodigoFormatoInvalido(string codigo)
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/categorias",
            new { tipoCategoria = "VEHICULO", codigo, nombre = "Formato" })).ErroresValidacionAsync();
        Assert.Contains("codigo", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAT-018 Consultar los tipos de categoría permitidos devuelve la lista centralizada")]
    [Trait("Tipo", "Funcional")]
    public async Task ListarTipos()
    {
        var tipos = await Cliente.GetFromJsonAsync<List<TipoCatalogoResponse>>("/api/categorias/tipos");
        Assert.Equal(["VEHICULO", "TIPO_CARGA", "TIPO_ENTREGA", "ESTADO_CARGA", "UNIDAD_MEDIDA", "TIPO_OBJETO"],
            tipos!.Select(t => t.Codigo));
    }
}
