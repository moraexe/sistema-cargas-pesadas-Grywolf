namespace TransporteCargaPesada.Web.Services.Reglas;

/// <summary>
/// Códigos de parámetros que el sistema utiliza en sus reglas (RN-17). Los valores son reglas
/// académicas configurables, no límites legales.
/// </summary>
public static class ParametrosSistema
{
    /// <summary>Peso total máximo de una carga (parámetro general, numérico, kg).</summary>
    public const string PesoMaximoCargaKg = "PESO_MAXIMO_CARGA_KG";

    /// <summary>Volumen total máximo de una carga (parámetro general, numérico, m³).</summary>
    public const string VolumenMaximoCargaM3 = "VOLUMEN_MAXIMO_CARGA_M3";

    /// <summary>
    /// Prefijo de los parámetros de capacidad por tipo de vehículo. Deben asociarse a una
    /// categoría VEHICULO; por ejemplo CAPACIDAD_KG_TRACTOCAMION.
    /// </summary>
    public const string PrefijoCapacidadVehiculoKg = "CAPACIDAD_KG_";

    /// <summary>Parámetros generales de límite: numéricos, mayores que cero y sin categoría.</summary>
    public static readonly IReadOnlySet<string> LimitesGenerales =
        new HashSet<string> { PesoMaximoCargaKg, VolumenMaximoCargaM3 };

    public static bool EsCapacidadVehiculo(string codigo) =>
        codigo.StartsWith(PrefijoCapacidadVehiculoKg, StringComparison.Ordinal);

    public static bool EsLimite(string codigo) => LimitesGenerales.Contains(codigo) || EsCapacidadVehiculo(codigo);
}
