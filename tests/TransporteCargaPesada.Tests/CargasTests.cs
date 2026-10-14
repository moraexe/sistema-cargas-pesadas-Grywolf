using System.Net;
using System.Net.Http.Json;
using TransporteCargaPesada.Tests.Infraestructura;
using TransporteCargaPesada.Web.DTOs;

namespace TransporteCargaPesada.Tests;

[Trait("Entidad", "Cargas")]
public class CargasTests(FabricaApi fabrica) : IClassFixture<FabricaApi>
{
    private readonly ApiPruebas _api = new(fabrica.CreateClient());
    private HttpClient Cliente => _api.Cliente;

    private async Task<HttpResponseMessage> PostCargaAsync(Action<Dictionary<string, object?>> ajustar)
    {
        var cuerpo = await _api.CuerpoCargaAsync();
        ajustar(cuerpo);
        return await Cliente.PostAsJsonAsync("/api/cargas", cuerpo);
    }

    [Fact(DisplayName = "CP-CAR-001 Listar cargas devuelve 200 y una lista")]
    [Trait("Tipo", "Funcional")]
    public async Task ListarCargas()
    {
        var respuesta = await Cliente.GetAsync("/api/cargas");
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        Assert.NotNull(await respuesta.Content.ReadFromJsonAsync<List<CargaResponse>>());
    }

    [Fact(DisplayName = "CP-CAR-002 Crear carga válida devuelve 201, Location y estado PENDIENTE por defecto")]
    [Trait("Tipo", "Funcional")]
    public async Task CrearCarga()
    {
        var cuerpo = await _api.CuerpoCargaAsync();
        cuerpo["codigo"] = ((string)cuerpo["codigo"]!).ToLowerInvariant();
        var respuesta = await Cliente.PostAsJsonAsync("/api/cargas", cuerpo);

        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        var carga = await respuesta.Content.ReadFromJsonAsync<CargaResponse>();
        Assert.True(carga!.Id > 0);
        Assert.Equal(((string)cuerpo["codigo"]!).ToUpperInvariant(), carga.Codigo);
        Assert.Equal("PENDIENTE", carga.EstadoCodigo);
        Assert.Equal(0, carga.CantidadObjetos);
        Assert.Equal(10000m, carga.PesoTotalKg);
        Assert.Equal("Carga general", carga.TipoCargaNombre);
        Assert.EndsWith($"/api/cargas/{carga.Id}", respuesta.Headers.Location!.ToString());
    }

    [Fact(DisplayName = "CP-CAR-003 Consultar carga por id devuelve 200 con sus clasificaciones")]
    [Trait("Tipo", "Funcional")]
    public async Task ConsultarPorId()
    {
        var creada = await _api.CrearCargaAsync();
        var carga = await Cliente.GetFromJsonAsync<CargaResponse>($"/api/cargas/{creada.Id}");
        Assert.Equal(creada.Codigo, carga!.Codigo);
        Assert.NotNull(carga.TipoEntregaNombre);
        Assert.NotNull(carga.EstadoNombre);
    }

    [Fact(DisplayName = "CP-CAR-004 Consultar carga inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task ConsultarInexistente() =>
        await (await Cliente.GetAsync("/api/cargas/999999")).DetalleProblemaAsync(HttpStatusCode.NotFound);

    [Fact(DisplayName = "CP-CAR-005 Actualizar carga (estado EN_TRANSITO y vehículo) devuelve 200")]
    [Trait("Tipo", "Funcional")]
    public async Task ActualizarCarga()
    {
        var creada = await _api.CrearCargaAsync();
        var cuerpo = await _api.CuerpoCargaAsync();
        cuerpo["codigo"] = creada.Codigo;
        cuerpo["destino"] = "Pereira";
        cuerpo["estadoId"] = await _api.CategoriaIdAsync("ESTADO_CARGA", "EN_TRANSITO");
        cuerpo["tipoVehiculoId"] = await _api.CategoriaIdAsync("VEHICULO", "TRACTOCAMION");

        var respuesta = await Cliente.PutAsJsonAsync($"/api/cargas/{creada.Id}", cuerpo);
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.OK);
        var carga = await respuesta.Content.ReadFromJsonAsync<CargaResponse>();
        Assert.Equal("Pereira", carga!.Destino);
        Assert.Equal("EN_TRANSITO", carga.EstadoCodigo);
        Assert.Equal("Tractocamión", carga.TipoVehiculoNombre);
        Assert.Equal(creada.FechaRegistro, carga.FechaRegistro);
    }

    [Fact(DisplayName = "CP-CAR-006 Crear carga con campos obligatorios vacíos devuelve 400 con cada campo")]
    [Trait("Tipo", "Negativa")]
    public async Task CamposObligatoriosVacios()
    {
        var errores = await (await Cliente.PostAsJsonAsync("/api/cargas",
            new { codigo = "", descripcion = "   ", origen = "", destino = (string?)null })).ErroresValidacionAsync();
        foreach (var campo in new[] { "codigo", "descripcion", "origen", "destino", "pesoTotalKg", "tipoCargaId", "tipoEntregaId" })
            Assert.Contains(campo, errores.Keys);
    }

    [Theory(DisplayName = "CP-CAR-007 Peso total negativo o cero devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(-0.01)]
    public async Task PesoNoPositivo(double peso)
    {
        var errores = await (await PostCargaAsync(c => c["pesoTotalKg"] = (decimal)peso)).ErroresValidacionAsync();
        Assert.Contains("pesoTotalKg", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAR-008 Volumen negativo devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task VolumenNegativo()
    {
        var errores = await (await PostCargaAsync(c => c["volumenTotalM3"] = -5m)).ErroresValidacionAsync();
        Assert.Contains("volumenTotalM3", errores.Keys);
    }

    [Theory(DisplayName = "CP-CAR-009 Origen y destino inválidos devuelven 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("Bogotá", "bogotá", "destino")]
    [InlineData("AB", "Cali", "origen")]
    [InlineData("Cali", "  ", "destino")]
    public async Task OrigenDestinoInvalidos(string origen, string destino, string campo)
    {
        var errores = await (await PostCargaAsync(c => { c["origen"] = origen; c["destino"] = destino; })).ErroresValidacionAsync();
        Assert.Contains(campo, errores.Keys);
    }

    [Fact(DisplayName = "CP-CAR-010 Código de carga duplicado devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task CodigoDuplicado()
    {
        var existente = await _api.CrearCargaAsync();
        var detalle = await (await PostCargaAsync(c => c["codigo"] = existente.Codigo.ToLowerInvariant()))
            .DetalleProblemaAsync(HttpStatusCode.Conflict);
        Assert.Contains(existente.Codigo, detalle);
    }

    [Fact(DisplayName = "CP-CAR-011 Categoría VEHICULO usada como TIPO_CARGA devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task VehiculoComoTipoCarga()
    {
        var tractocamion = await _api.CategoriaIdAsync("VEHICULO", "TRACTOCAMION");
        var errores = await (await PostCargaAsync(c => c["tipoCargaId"] = tractocamion)).ErroresValidacionAsync();
        Assert.Contains("tipoCargaId", errores.Keys);
        Assert.Contains("VEHICULO", errores["tipoCargaId"][0]);
        Assert.Contains("TIPO_CARGA", errores["tipoCargaId"][0]);
    }

    [Theory(DisplayName = "CP-CAR-012 Categoría de tipo incorrecto en vehículo, entrega o estado devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("tipoVehiculoId", "TIPO_CARGA", "GENERAL")]
    [InlineData("tipoEntregaId", "ESTADO_CARGA", "PENDIENTE")]
    [InlineData("estadoId", "TIPO_ENTREGA", "DIRECTA")]
    public async Task CategoriaTipoIncorrecto(string campo, string tipo, string codigo)
    {
        var id = await _api.CategoriaIdAsync(tipo, codigo);
        var errores = await (await PostCargaAsync(c => c[campo] = id)).ErroresValidacionAsync();
        Assert.Contains(campo, errores.Keys);
    }

    [Fact(DisplayName = "CP-CAR-013 Carga con categoría inexistente devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CategoriaInexistente()
    {
        var errores = await (await PostCargaAsync(c => c["tipoEntregaId"] = 999999)).ErroresValidacionAsync();
        Assert.Contains("tipoEntregaId", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAR-014 Carga nueva con categoría inactiva devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CategoriaInactiva()
    {
        var categoria = await _api.CrearCategoriaAsync("TIPO_CARGA");
        await Cliente.PutAsJsonAsync($"/api/categorias/{categoria.Id}",
            new { categoria.TipoCategoria, categoria.Codigo, categoria.Nombre, activo = false });

        var errores = await (await PostCargaAsync(c => c["tipoCargaId"] = categoria.Id)).ErroresValidacionAsync();
        Assert.Contains("inactiva", errores["tipoCargaId"][0]);
    }

    [Fact(DisplayName = "CP-CAR-015 Peso que supera PESO_MAXIMO_CARGA_KG devuelve 400; igual al límite devuelve 201")]
    [Trait("Tipo", "Negativa")]
    public async Task PesoMaximoConfigurable()
    {
        var limite = decimal.Parse((await _api.ParametroAsync("PESO_MAXIMO_CARGA_KG")).Valor, System.Globalization.CultureInfo.InvariantCulture);

        var errores = await (await PostCargaAsync(c => c["pesoTotalKg"] = limite + 0.01m)).ErroresValidacionAsync();
        Assert.Contains("PESO_MAXIMO_CARGA_KG", errores["pesoTotalKg"][0]);

        await (await PostCargaAsync(c => c["pesoTotalKg"] = limite)).AsegurarEstadoAsync(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "CP-CAR-016 Peso que supera la capacidad del vehículo (CAPACIDAD_KG_*) devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task CapacidadVehiculo()
    {
        var camionRigido = await _api.CategoriaIdAsync("VEHICULO", "CAMION_RIGIDO");
        var capacidad = decimal.Parse((await _api.ParametroAsync("CAPACIDAD_KG_CAMION_RIGIDO")).Valor, System.Globalization.CultureInfo.InvariantCulture);

        var errores = await (await PostCargaAsync(c => { c["tipoVehiculoId"] = camionRigido; c["pesoTotalKg"] = capacidad + 1; }))
            .ErroresValidacionAsync();
        Assert.Contains("CAPACIDAD_KG_CAMION_RIGIDO", errores["pesoTotalKg"][0]);

        await (await PostCargaAsync(c => { c["tipoVehiculoId"] = camionRigido; c["pesoTotalKg"] = capacidad; }))
            .AsegurarEstadoAsync(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "CP-CAR-017 Volumen que supera VOLUMEN_MAXIMO_CARGA_M3 devuelve 400")]
    [Trait("Tipo", "Negativa")]
    public async Task VolumenMaximoConfigurable()
    {
        var limite = decimal.Parse((await _api.ParametroAsync("VOLUMEN_MAXIMO_CARGA_M3")).Valor, System.Globalization.CultureInfo.InvariantCulture);
        var errores = await (await PostCargaAsync(c => c["volumenTotalM3"] = limite + 1)).ErroresValidacionAsync();
        Assert.Contains("volumenTotalM3", errores.Keys);
    }

    [Fact(DisplayName = "CP-CAR-018 Tipo de dato incorrecto en el JSON devuelve 400 sin detalles internos")]
    [Trait("Tipo", "Negativa")]
    public async Task TipoDatoIncorrecto()
    {
        var respuesta = await _api.EnviarCrudoAsync(HttpMethod.Post, "/api/cargas",
            """{"codigo":"T-TIPO","descripcion":"x","origen":"Bogotá","destino":"Cali","pesoTotalKg":"mil","tipoCargaId":"uno"}""");
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        var errores = await respuesta.ErroresValidacionAsync();
        Assert.Contains("pesoTotalKg", errores.Keys);
        Assert.DoesNotContain("System.", cuerpo);
    }

    [Theory(DisplayName = "CP-CAR-019 JSON mal formado o cuerpo vacío devuelve 400")]
    [Trait("Tipo", "Negativa")]
    [InlineData("""{"codigo": "T-MAL", "descripcion": """)]
    [InlineData("esto no es json")]
    [InlineData("")]
    public async Task JsonMalFormado(string json)
    {
        var errores = await (await _api.EnviarCrudoAsync(HttpMethod.Post, "/api/cargas", json)).ErroresValidacionAsync();
        Assert.NotEmpty(errores);
    }

    [Fact(DisplayName = "CP-CAR-020 Actualizar carga inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task ActualizarInexistente() =>
        await (await Cliente.PutAsJsonAsync("/api/cargas/999999", await _api.CuerpoCargaAsync()))
            .DetalleProblemaAsync(HttpStatusCode.NotFound);

    [Fact(DisplayName = "CP-CAR-021 Eliminar carga sin objetos devuelve 204 y luego 404")]
    [Trait("Tipo", "Funcional")]
    public async Task EliminarSinObjetos()
    {
        var carga = await _api.CrearCargaAsync();
        await (await Cliente.DeleteAsync($"/api/cargas/{carga.Id}")).AsegurarEstadoAsync(HttpStatusCode.NoContent);
        await (await Cliente.GetAsync($"/api/cargas/{carga.Id}")).AsegurarEstadoAsync(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "CP-CAR-022 Eliminar carga con objetos devuelve 409 y conserva carga y objetos")]
    [Trait("Tipo", "Negativa")]
    public async Task EliminarConObjetos()
    {
        var carga = await _api.CrearCargaAsync();
        var objeto = await _api.CrearObjetoAsync(carga.Id);

        var detalle = await (await Cliente.DeleteAsync($"/api/cargas/{carga.Id}")).DetalleProblemaAsync(HttpStatusCode.Conflict);
        Assert.Contains("objeto", detalle);
        await (await Cliente.GetAsync($"/api/cargas/{carga.Id}")).AsegurarEstadoAsync(HttpStatusCode.OK);
        await (await Cliente.GetAsync($"/api/objetos/{objeto.Id}")).AsegurarEstadoAsync(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "CP-CAR-023 Reducir el peso de la carga por debajo del peso de sus objetos devuelve 409")]
    [Trait("Tipo", "Negativa")]
    public async Task ReducirPesoBajoObjetos()
    {
        var carga = await _api.CrearCargaAsync();
        await _api.CrearObjetoAsync(carga.Id); // 2 × 1500 = 3000 kg

        var cuerpo = await _api.CuerpoCargaAsync();
        cuerpo["codigo"] = carga.Codigo;
        cuerpo["estadoId"] = carga.EstadoId;
        cuerpo["pesoTotalKg"] = 2999m;
        await (await Cliente.PutAsJsonAsync($"/api/cargas/{carga.Id}", cuerpo)).DetalleProblemaAsync(HttpStatusCode.Conflict);

        cuerpo["pesoTotalKg"] = 3000m;
        await (await Cliente.PutAsJsonAsync($"/api/cargas/{carga.Id}", cuerpo)).AsegurarEstadoAsync(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "CP-CAR-024 Eliminar carga inexistente devuelve 404")]
    [Trait("Tipo", "Negativa")]
    public async Task EliminarInexistente() =>
        await (await Cliente.DeleteAsync("/api/cargas/999999")).DetalleProblemaAsync(HttpStatusCode.NotFound);

    [Fact(DisplayName = "CP-CAR-025 Actualizar carga conserva una categoría que fue desactivada después de asignarla")]
    [Trait("Tipo", "Funcional")]
    public async Task ConservaCategoriaInactivaAsignada()
    {
        var categoria = await _api.CrearCategoriaAsync("TIPO_CARGA");
        var carga = await _api.CrearCargaAsync(c => c["tipoCargaId"] = categoria.Id);
        await Cliente.PutAsJsonAsync($"/api/categorias/{categoria.Id}",
            new { categoria.TipoCategoria, categoria.Codigo, categoria.Nombre, activo = false });

        var cuerpo = await _api.CuerpoCargaAsync();
        cuerpo["codigo"] = carga.Codigo;
        cuerpo["tipoCargaId"] = categoria.Id;
        cuerpo["estadoId"] = carga.EstadoId;
        cuerpo["descripcion"] = "Descripción actualizada";
        await (await Cliente.PutAsJsonAsync($"/api/cargas/{carga.Id}", cuerpo)).AsegurarEstadoAsync(HttpStatusCode.OK);
    }
}
