using System.Net;
using System.Net.Http.Json;
using TransporteCargaPesada.Tests.Infraestructura;
using TransporteCargaPesada.Web.DTOs;

namespace TransporteCargaPesada.Tests;

[Trait("Entidad", "Objetos")]
public class ObjetosTests(FabricaApi fabrica) : IClassFixture<FabricaApi>
{
    private readonly ApiPruebas _api = new(fabrica.CreateClient());
    private HttpClient Cliente => _api.Cliente;

    private async Task<HttpResponseMessage> PostObjetoAsync(int cargaId, Action<Dictionary<string, object?>> ajustar)
    {
        var cuerpo = ApiPruebas.CuerpoObjeto(cargaId);
        ajustar(cuerpo);
        return await Cliente.PostAsJsonAsync("/api/objetos", cuerpo);
    }

    [Fact(DisplayName = "CP-OBJ-001 Listar objetos devuelve 200 y una lista")]
    [Trait("Tipo", "Funcional")]
    public async Task ListarObjetos()
    {
        var respuesta = await Cliente.GetAsync("/api/objetos");
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        Assert.NotNull(await respuesta.Content.ReadFromJsonAsync<List<ObjetoResponse>>());
    }

    [Fact(DisplayName = "CP-OBJ-002 Crear objeto válido devuelve 201 con peso total calculado")]
    [Trait("Tipo", "Funcional")]
    public async Task CrearObjeto()
    {
        var carga = await _api.CrearCargaAsync();
        var maquinaria = await _api.CategoriaIdAsync("TIPO_OBJETO", "MAQUINARIA");
        var respuesta = await PostObjetoAsync(carga.Id, o => o["categoriaId"] = maquinaria);

        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        var objeto = await respuesta.Content.ReadFromJsonAsync<ObjetoResponse>();
        Assert.True(objeto!.Id > 0);
        Assert.Equal(carga.Id, objeto.CargaId);
        Assert.Equal(carga.Codigo, objeto.CargaCodigo);
        Assert.Equal(3000m, objeto.PesoTotalKg);
        Assert.Equal("Maquinaria", objeto.CategoriaNombre);
        Assert.EndsWith($"/api/objetos/{objeto.Id}", respuesta.Headers.Location!.ToString());

        var cargaActualizada = await Cliente.GetFromJsonAsync<CargaResponse>($"/api/cargas/{carga.Id}");
        Assert.Equal(1, cargaActualizada!.CantidadObjetos);
        Assert.Equal(3000m, cargaActualizada.PesoObjetosKg);
    }

    [Fact(DisplayName = "CP-OBJ-003 Consultar objeto por id devuelve 200")]
    [Trait("Tipo", "Funcional")]
    public async Task ConsultarPorId()
    {
        var carga = await _api.CrearCargaAsync();
        var creado = await _api.CrearObjetoAsync(carga.Id);
        var objeto = await Cliente.GetFromJsonAsync<ObjetoResponse>($"/api/objetos/{creado.Id}");
        Assert.Equal(creado.Nombre, objeto!.Nombre);
    }

    [Fact(DisplayName = "CP-OBJ-004 Consultar objeto inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task ConsultarInexistente() =>
        await (await Cliente.GetAsync("/api/objetos/999999")).DetalleProblemaAsync(HttpStatusCode.NotFound);

    [Fact(DisplayName = "CP-OBJ-005 Consultar los objetos de una carga devuelve solo los de esa carga")]
    [Trait("Tipo", "Funcional")]
    public async Task ObjetosDeCarga()
    {
        var cargaA = await _api.CrearCargaAsync();
        var cargaB = await _api.CrearCargaAsync();
        await _api.CrearObjetoAsync(cargaA.Id);
        await _api.CrearObjetoAsync(cargaA.Id, o => o["nombre"] = "Repuestos");
        await _api.CrearObjetoAsync(cargaB.Id);

        var objetos = await Cliente.GetFromJsonAsync<List<ObjetoResponse>>($"/api/cargas/{cargaA.Id}/objetos");
        Assert.Equal(2, objetos!.Count);
        Assert.All(objetos, o => Assert.Equal(cargaA.Id, o.CargaId));

        var filtrados = await Cliente.GetFromJsonAsync<List<ObjetoResponse>>($"/api/objetos?cargaId={cargaB.Id}");
        Assert.Single(filtrados!);
    }

    [Fact(DisplayName = "CP-OBJ-006 Consultar objetos de una carga inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task ObjetosDeCargaInexistente() =>
        await (await Cliente.GetAsync("/api/cargas/999999/objetos")).DetalleProblemaAsync(HttpStatusCode.NotFound);

    [Fact(DisplayName = "CP-OBJ-007 Actualizar objeto devuelve 200 con los nuevos datos")]
    [Trait("Tipo", "Funcional")]
    public async Task ActualizarObjeto()
    {
        var carga = await _api.CrearCargaAsync();
        var creado = await _api.CrearObjetoAsync(carga.Id);
        var cuerpo = ApiPruebas.CuerpoObjeto(carga.Id);
        cuerpo["cantidad"] = 3;
        cuerpo["nombre"] = "Motor industrial reforzado";

        var respuesta = await Cliente.PutAsJsonAsync($"/api/objetos/{creado.Id}", cuerpo);
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        var objeto = await respuesta.Content.ReadFromJsonAsync<ObjetoResponse>();
        Assert.Equal(3, objeto!.Cantidad);
        Assert.Equal(4500m, objeto.PesoTotalKg);
    }

    [Fact(DisplayName = "CP-OBJ-008 Objeto vinculado a una carga inexistente devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CargaInexistente()
    {
        var errores = await (await PostObjetoAsync(999999, _ => { })).ErroresValidacionAsync();
        Assert.Contains("cargaId", errores.Keys);
    }

    [Theory(DisplayName = "CP-OBJ-009 Cantidad cero o negativa devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task CantidadNoPositiva(int cantidad)
    {
        var carga = await _api.CrearCargaAsync();
        var errores = await (await PostObjetoAsync(carga.Id, o => o["cantidad"] = cantidad)).ErroresValidacionAsync();
        Assert.Contains("cantidad", errores.Keys);
    }

    [Theory(DisplayName = "CP-OBJ-010 Peso o volumen unitario negativos devuelven 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("pesoUnitarioKg")]
    [InlineData("volumenUnitarioM3")]
    public async Task ValoresFisicosNegativos(string campo)
    {
        var carga = await _api.CrearCargaAsync();
        var errores = await (await PostObjetoAsync(carga.Id, o => o[campo] = -10m)).ErroresValidacionAsync();
        Assert.Contains(campo, errores.Keys);
    }

    [Fact(DisplayName = "CP-OBJ-011 Objeto con campos obligatorios vacíos devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CamposObligatoriosVacios()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/objetos", new { nombre = "  " })).ErroresValidacionAsync();
        foreach (var campo in new[] { "cargaId", "nombre", "cantidad", "pesoUnitarioKg" })
            Assert.Contains(campo, errores.Keys);
    }

    [Fact(DisplayName = "CP-OBJ-012 Objeto con categoría de tipo incorrecto (VEHICULO) devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CategoriaTipoIncorrecto()
    {
        var carga = await _api.CrearCargaAsync();
        var tractocamion = await _api.CategoriaIdAsync("VEHICULO", "TRACTOCAMION");
        var errores = await (await PostObjetoAsync(carga.Id, o => o["categoriaId"] = tractocamion)).ErroresValidacionAsync();
        Assert.Contains("TIPO_OBJETO", errores["categoriaId"][0]);
    }

    [Fact(DisplayName = "CP-OBJ-013 Objetos que superan el peso declarado de la carga devuelven 409")]
    [Trait("Tipo", "Negativa")]
    public async Task SuperaPesoDeclarado()
    {
        var carga = await _api.CrearCargaAsync(c => c["pesoTotalKg"] = 5000m);
        await _api.CrearObjetoAsync(carga.Id); // 3000 kg

        var detalle = await (await PostObjetoAsync(carga.Id, o => { o["cantidad"] = 1; o["pesoUnitarioKg"] = 2000.01m; }))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);
        Assert.Contains("peso total declarado", detalle);

        // Valor límite: completar exactamente el peso declarado sí se permite.
        await (await PostObjetoAsync(carga.Id, o => { o["cantidad"] = 1; o["pesoUnitarioKg"] = 2000m; }))
            .AsegurarEstadoAsync(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "CP-OBJ-014 Cantidad con tipo de dato incorrecto (decimal o texto) devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CantidadTipoIncorrecto()
    {
        var carga = await _api.CrearCargaAsync();
        var json = $$"""{"cargaId":{{carga.Id}},"nombre":"Caja","cantidad":2.5,"pesoUnitarioKg":10}""";
        var errores = await (await _api.EnviarCrudoAsync(HttpMethod.Post, "/api/objetos", json)).ErroresValidacionAsync();
        Assert.Contains("cantidad", errores.Keys);

        json = $$"""{"cargaId":{{carga.Id}},"nombre":"Caja","cantidad":"dos","pesoUnitarioKg":10}""";
        errores = await (await _api.EnviarCrudoAsync(HttpMethod.Post, "/api/objetos", json)).ErroresValidacionAsync();
        Assert.Contains("cantidad", errores.Keys);
    }

    [Fact(DisplayName = "CP-OBJ-015 Eliminar objeto devuelve 204 y luego 404")]
    [Trait("Tipo", "Funcional")]
    public async Task EliminarObjeto()
    {
        var carga = await _api.CrearCargaAsync();
        var objeto = await _api.CrearObjetoAsync(carga.Id);
        await (await Cliente.DeleteAsync($"/api/objetos/{objeto.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await Cliente.GetAsync($"/api/objetos/{objeto.Id}")).AsegurarEstadoAsync(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "CP-OBJ-016 Mover un objeto a una carga sin capacidad disponible devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task MoverObjetoSinCapacidad()
    {
        var origen = await _api.CrearCargaAsync();
        var destino = await _api.CrearCargaAsync(c => c["pesoTotalKg"] = 1000m);
        var objeto = await _api.CrearObjetoAsync(origen.Id); // 3000 kg

        var cuerpo = ApiPruebas.CuerpoObjeto(destino.Id);
        await (await Cliente.PutAsJsonAsync($"/api/objetos/{objeto.Id}", cuerpo)).DetalleProblemaAsync(HttpStatusCode.Conflict);
    }
}
