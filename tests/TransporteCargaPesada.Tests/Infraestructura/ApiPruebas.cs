using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TransporteCargaPesada.Web.DTOs;

namespace TransporteCargaPesada.Tests.Infraestructura;

/// <summary>Datos y operaciones reutilizables para preparar los escenarios de prueba a través de la API.</summary>
public class ApiPruebas(HttpClient cliente)
{
    public HttpClient Cliente { get; } = cliente;

    public static string CodigoCarga() => $"T-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";

    public static string CodigoCatalogo(string prefijo) => $"{prefijo}_{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    /// <summary>Busca una categoría sembrada por su tipo y código (no se depende de identificadores fijos).</summary>
    public async Task<int> CategoriaIdAsync(string tipo, string codigo)
    {
        var lista = await Cliente.GetFromJsonAsync<List<CategoriaResponse>>($"/api/categorias?tipoCategoria={tipo}");
        return lista!.Single(c => c.Codigo == codigo).Id;
    }

    public async Task<ParametroResponse> ParametroAsync(string codigo)
    {
        var lista = await Cliente.GetFromJsonAsync<List<ParametroResponse>>("/api/parametros");
        return lista!.Single(p => p.Codigo == codigo);
    }

    public async Task<Dictionary<string, object?>> CuerpoCargaAsync() => new()
    {
        ["codigo"] = CodigoCarga(),
        ["descripcion"] = "Carga de prueba automatizada",
        ["origen"] = "Bogotá",
        ["destino"] = "Cali",
        ["pesoTotalKg"] = 10000m,
        ["volumenTotalM3"] = 20m,
        ["tipoCargaId"] = await CategoriaIdAsync("TIPO_CARGA", "GENERAL"),
        ["tipoVehiculoId"] = null,
        ["tipoEntregaId"] = await CategoriaIdAsync("TIPO_ENTREGA", "DIRECTA"),
        ["observaciones"] = "Creada por xUnit",
    };

    public async Task<CargaResponse> CrearCargaAsync(Action<Dictionary<string, object?>>? ajustar = null)
    {
        var cuerpo = await CuerpoCargaAsync();
        ajustar?.Invoke(cuerpo);
        var respuesta = await Cliente.PostAsJsonAsync("/api/cargas", cuerpo);
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        return (await respuesta.Content.ReadFromJsonAsync<CargaResponse>())!;
    }

    public static Dictionary<string, object?> CuerpoObjeto(int cargaId) => new()
    {
        ["cargaId"] = cargaId,
        ["nombre"] = "Motor industrial",
        ["descripcion"] = "Objeto de prueba",
        ["cantidad"] = 2,
        ["pesoUnitarioKg"] = 1500m,
        ["volumenUnitarioM3"] = 3m,
        ["categoriaId"] = null,
    };

    public async Task<ObjetoResponse> CrearObjetoAsync(int cargaId, Action<Dictionary<string, object?>>? ajustar = null)
    {
        var cuerpo = CuerpoObjeto(cargaId);
        ajustar?.Invoke(cuerpo);
        var respuesta = await Cliente.PostAsJsonAsync("/api/objetos", cuerpo);
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        return (await respuesta.Content.ReadFromJsonAsync<ObjetoResponse>())!;
    }

    public async Task<CategoriaResponse> CrearCategoriaAsync(string tipo, string? codigo = null)
    {
        var respuesta = await Cliente.PostAsJsonAsync("/api/categorias", new
        {
            tipoCategoria = tipo,
            codigo = codigo ?? CodigoCatalogo("PRUEBA"),
            nombre = "Categoría de prueba",
            descripcion = "Creada por xUnit",
        });
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.Created);
        return (await respuesta.Content.ReadFromJsonAsync<CategoriaResponse>())!;
    }

    /// <summary>Envía un cuerpo crudo (para JSON mal formado o tipos incorrectos).</summary>
    public Task<HttpResponseMessage> EnviarCrudoAsync(HttpMethod metodo, string ruta, string json) =>
        Cliente.SendAsync(new HttpRequestMessage(metodo, ruta) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
}

public static class ExtensionesRespuesta
{
    public static async Task AsegurarEstadoAsync(this HttpResponseMessage respuesta, HttpStatusCode esperado)
    {
        if (respuesta.StatusCode != esperado)
        {
            var cuerpo = await respuesta.Content.ReadAsStringAsync();
            Assert.Fail($"Se esperaba HTTP {(int)esperado} y se obtuvo {(int)respuesta.StatusCode}. Cuerpo: {cuerpo}");
        }
    }

    public static async Task<JsonElement> JsonAsync(this HttpResponseMessage respuesta) =>
        await respuesta.Content.ReadFromJsonAsync<JsonElement>();

    /// <summary>Verifica una respuesta 400 con formato ValidationProblemDetails y devuelve los campos con error.</summary>
    public static async Task<IReadOnlyDictionary<string, string[]>> ErroresValidacionAsync(this HttpResponseMessage respuesta)
    {
        await respuesta.AsegurarEstadoAsync(HttpStatusCode.BadRequest);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var json = await respuesta.JsonAsync();
        Assert.Equal(400, json.GetProperty("status").GetInt32());
        Assert.True(json.TryGetProperty("errors", out var errores), "La respuesta 400 debe incluir 'errors'.");
        return errores.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.EnumerateArray().Select(m => m.GetString()!).ToArray());
    }

    /// <summary>Verifica una respuesta ProblemDetails (404/409) y devuelve el campo 'detail'.</summary>
    public static async Task<string> DetalleProblemaAsync(this HttpResponseMessage respuesta, HttpStatusCode esperado)
    {
        await respuesta.AsegurarEstadoAsync(esperado);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var json = await respuesta.JsonAsync();
        Assert.Equal((int)esperado, json.GetProperty("status").GetInt32());
        return json.GetProperty("detail").GetString()!;
    }
}
