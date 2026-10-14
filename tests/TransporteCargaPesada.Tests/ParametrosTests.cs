using System.Net;
using System.Net.Http.Json;
using TransporteCargaPesada.Tests.Infraestructura;
using TransporteCargaPesada.Web.DTOs;

namespace TransporteCargaPesada.Tests;

[Trait("Entidad", "Parametros")]
public class ParametrosTests(FabricaApi fabrica) : IClassFixture<FabricaApi>
{
    private readonly ApiPruebas _api = new(fabrica.CreateClient());
    private HttpClient Cliente => _api.Cliente;

    private static object Parametro(string tipoDato, string valor, string? codigo = null, int? categoriaId = null,
        bool? esObligatorio = null, bool? activo = null) => new
    {
        codigo = codigo ?? ApiPruebas.CodigoCatalogo("PAR"),
        nombre = "Parámetro de prueba",
        descripcion = "Creado por xUnit",
        tipoDato,
        valor,
        unidadMedida = "u",
        categoriaId,
        esObligatorio,
        activo,
    };

    private async Task<ParametroResponse> CrearAsync(object cuerpo)
    {
        var respuesta = await Cliente.PostAsJsonAsync("/api/parametros", cuerpo);
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        return (await respuesta.Content.ReadFromJsonAsync<ParametroResponse>())!;
    }

    [Fact(DisplayName = "CP-PAR-001 Listar parámetros devuelve 200 con parámetros generales y por categoría")]
    [Trait("Tipo", "Funcional")]
    public async Task ListarParametros()
    {
        var lista = await Cliente.GetFromJsonAsync<List<ParametroResponse>>("/api/parametros");
        Assert.Contains(lista!, p => p.Ambito == "GENERAL" && p.CategoriaId == null);
        Assert.Contains(lista!, p => p.Ambito == "CATEGORIA" && p.CategoriaId != null);
    }

    [Fact(DisplayName = "CP-PAR-002 Filtrar parámetros por categoría y por estado activo")]
    [Trait("Tipo", "Funcional")]
    public async Task FiltrarParametros()
    {
        var tractocamion = await _api.CategoriaIdAsync("VEHICULO", "TRACTOCAMION");
        var porCategoria = await Cliente.GetFromJsonAsync<List<ParametroResponse>>($"/api/parametros?categoriaId={tractocamion}");
        Assert.NotEmpty(porCategoria!);
        Assert.All(porCategoria!, p => Assert.Equal(tractocamion, p.CategoriaId));

        var inactivo = await CrearAsync(Parametro("TEXTO", "x", activo: false));
        var inactivos = await Cliente.GetFromJsonAsync<List<ParametroResponse>>("/api/parametros?activo=false");
        Assert.Contains(inactivos!, p => p.Id == inactivo.Id);
        Assert.All(inactivos!, p => Assert.False(p.Activo));
    }

    [Theory(DisplayName = "CP-PAR-003 Crear parámetro con valor compatible con su tipo devuelve 201 y valor normalizado")]
    [Trait("Tipo", "Funcional")]
    [InlineData("DECIMAL", " 1500.50 ", "1500.50")]
    [InlineData("ENTERO", "42", "42")]
    [InlineData("BOOLEANO", "TRUE", "true")]
    [InlineData("FECHA", "2026-12-31", "2026-12-31")]
    [InlineData("TEXTO", "Valor libre", "Valor libre")]
    public async Task CrearParametroValido(string tipoDato, string valor, string esperado)
    {
        var creado = await CrearAsync(Parametro(tipoDato.ToLowerInvariant(), valor));
        Assert.Equal(tipoDato, creado.TipoDato);
        Assert.Equal(esperado, creado.Valor);
        Assert.False(creado.EsObligatorio);
        Assert.True(creado.Activo);
        Assert.Equal("GENERAL", creado.Ambito);
    }

    [Fact(DisplayName = "CP-PAR-004 Consultar parámetro por id devuelve 200")]
    [Trait("Tipo", "Funcional")]
    public async Task ConsultarPorId()
    {
        var creado = await CrearAsync(Parametro("ENTERO", "7"));
        var consultado = await Cliente.GetFromJsonAsync<ParametroResponse>($"/api/parametros/{creado.Id}");
        Assert.Equal(creado.Codigo, consultado!.Codigo);
    }

    [Fact(DisplayName = "CP-PAR-005 Consultar parámetro inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task ConsultarInexistente() =>
        await (await Cliente.GetAsync("/api/parametros/999999")).DetalleProblemaAsync(HttpStatusCode.NotFound);

    [Fact(DisplayName = "CP-PAR-006 Actualizar parámetro devuelve 200 y actualiza FechaActualizacion")]
    [Trait("Tipo", "Funcional")]
    public async Task ActualizarParametro()
    {
        var creado = await CrearAsync(Parametro("DECIMAL", "10"));
        var respuesta = await Cliente.PutAsJsonAsync($"/api/parametros/{creado.Id}", Parametro("DECIMAL", "20.5", creado.Codigo));
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<ParametroResponse>();
        Assert.Equal("20.5", actualizado!.Valor);
        Assert.True(actualizado.FechaActualizacion >= creado.FechaActualizacion);
    }

    [Theory(DisplayName = "CP-PAR-007 Valor incompatible con el tipo de dato devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("ENTERO", "abc")]
    [InlineData("ENTERO", "12.5")]
    [InlineData("DECIMAL", "12,5")]
    [InlineData("DECIMAL", "doce")]
    [InlineData("BOOLEANO", "si")]
    [InlineData("FECHA", "31/12/2026")]
    [InlineData("FECHA", "2026-02-30")]
    public async Task ValorIncompatible(string tipoDato, string valor)
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/parametros", Parametro(tipoDato, valor))).ErroresValidacionAsync();
        Assert.Contains("valor", errores.Keys);
        Assert.Contains(tipoDato, errores["valor"][0]);
    }

    [Fact(DisplayName = "CP-PAR-008 Tipo de dato no permitido devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task TipoDatoInvalido()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/parametros", Parametro("MONEDA", "100"))).ErroresValidacionAsync();
        Assert.Contains("tipoDato", errores.Keys);
    }

    [Fact(DisplayName = "CP-PAR-009 Parámetro con campos obligatorios vacíos devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CamposObligatoriosVacios()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/parametros", new { codigo = "", nombre = " " })).ErroresValidacionAsync();
        Assert.Contains("codigo", errores.Keys);
        Assert.Contains("nombre", errores.Keys);
        Assert.Contains("tipoDato", errores.Keys);
        Assert.Contains("valor", errores.Keys);
    }

    [Fact(DisplayName = "CP-PAR-010 Código de parámetro duplicado devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task CodigoDuplicado() =>
        await (await Cliente.PostAsJsonAsync("/api/parametros", Parametro("DECIMAL", "1", "peso_maximo_carga_kg")))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);

    [Fact(DisplayName = "CP-PAR-011 Eliminar parámetro obligatorio devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task EliminarObligatorio()
    {
        var parametro = await _api.ParametroAsync("PESO_MAXIMO_CARGA_KG");
        Assert.True(parametro.EsObligatorio);
        await (await Cliente.DeleteAsync($"/api/parametros/{parametro.Id}")).DetalleProblemaAsync(HttpStatusCode.Conflict);
        await (await Cliente.GetAsync($"/api/parametros/{parametro.Id}")).AsegurarEstadoAsync(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "CP-PAR-012 Desactivar o cambiar el tipo de un parámetro obligatorio devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task ModificarObligatorio()
    {
        var p = await _api.ParametroAsync("VOLUMEN_MAXIMO_CARGA_M3");
        await (await Cliente.PutAsJsonAsync($"/api/parametros/{p.Id}", Parametro("DECIMAL", p.Valor, p.Codigo, activo: false)))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);
        await (await Cliente.PutAsJsonAsync($"/api/parametros/{p.Id}", Parametro("ENTERO", "90", p.Codigo)))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);
        await (await Cliente.PutAsJsonAsync($"/api/parametros/{p.Id}", Parametro("DECIMAL", p.Valor, "OTRO_CODIGO")))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "CP-PAR-013 Parámetro asociado a una categoría inexistente devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CategoriaInexistente()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/parametros", Parametro("TEXTO", "x", categoriaId: 999999)))
            .ErroresValidacionAsync();
        Assert.Contains("categoriaId", errores.Keys);
    }

    [Fact(DisplayName = "CP-PAR-014 Capacidad de vehículo asociada a una categoría que no es VEHICULO devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CapacidadConCategoriaIncorrecta()
    {
        var refrigerada = await _api.CategoriaIdAsync("TIPO_CARGA", "REFRIGERADA");
        var errores = await (await Cliente.PostAsJsonAsync("/api/parametros",
            Parametro("DECIMAL", "1000", ApiPruebas.CodigoCatalogo("CAPACIDAD_KG"), refrigerada))).ErroresValidacionAsync();
        Assert.Contains("categoriaId", errores.Keys);
    }

    [Theory(DisplayName = "CP-PAR-015 Parámetro de límite con valor menor o igual a cero devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("0")]
    [InlineData("-100")]
    public async Task LimiteNoPositivo(string valor)
    {
        var p = await _api.ParametroAsync("PESO_MAXIMO_CARGA_KG");
        var errores = await (await Cliente.PutAsJsonAsync($"/api/parametros/{p.Id}", Parametro("DECIMAL", valor, p.Codigo)))
            .ErroresValidacionAsync();
        Assert.Contains("valor", errores.Keys);
    }

    [Fact(DisplayName = "CP-PAR-016 Eliminar parámetro no obligatorio devuelve 204 y luego 404")]
    [Trait("Tipo", "Funcional")]
    public async Task EliminarNoObligatorio()
    {
        var creado = await CrearAsync(Parametro("BOOLEANO", "false"));
        await (await Cliente.DeleteAsync($"/api/parametros/{creado.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await Cliente.GetAsync($"/api/parametros/{creado.Id}")).AsegurarEstadoAsync(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "CP-PAR-017 Crear parámetro asociado a una categoría devuelve ámbito CATEGORIA")]
    [Trait("Tipo", "Funcional")]
    public async Task CrearParametroDeCategoria()
    {
        var camaBaja = await _api.CategoriaIdAsync("VEHICULO", "CAMA_BAJA");
        var creado = await CrearAsync(Parametro("DECIMAL", "45000", ApiPruebas.CodigoCatalogo("CAPACIDAD_KG"), camaBaja));
        Assert.Equal("CATEGORIA", creado.Ambito);
        Assert.Equal("VEHICULO", creado.CategoriaTipo);
        Assert.Equal("CAMA_BAJA", creado.CategoriaCodigo);
    }
}
