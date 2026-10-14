namespace TransporteCargaPesada.Web.Services.Reglas;

/// <summary>Acumula errores de validación por campo (nombres en camelCase, igual que en el JSON).</summary>
public sealed class ErroresValidacion
{
    private readonly Dictionary<string, List<string>> _errores = new();

    public bool HayErrores => _errores.Count > 0;

    public bool TieneError(string campo) => _errores.ContainsKey(campo);

    public void Agregar(string campo, string mensaje)
    {
        if (!_errores.TryGetValue(campo, out var lista))
        {
            lista = new List<string>();
            _errores[campo] = lista;
        }
        lista.Add(mensaje);
    }

    public IDictionary<string, string[]> ComoDiccionario() =>
        _errores.ToDictionary(e => e.Key, e => e.Value.ToArray());
}
