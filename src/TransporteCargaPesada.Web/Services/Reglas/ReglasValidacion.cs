using System.Text.RegularExpressions;

namespace TransporteCargaPesada.Web.Services.Reglas;

/// <summary>
/// Reglas de validación de campos reutilizadas por todos los servicios (RN-02, RN-03, RN-04, RN-05).
/// Los límites técnicos evitan desbordamientos numéricos; no son reglas de negocio.
/// </summary>
public static partial class ReglasValidacion
{
    public const int MaxNombre = 100;
    public const int MaxDescripcion = 300;
    public const int MaxObservaciones = 500;
    public const int MinUbicacion = 3;
    public const int MaxUbicacion = 100;
    public const int MaxValorParametro = 200;
    public const int MaxUnidadMedida = 20;

    public const decimal MaxPesoTecnicoKg = 10_000_000m;
    public const decimal MaxVolumenTecnicoM3 = 100_000m;
    public const int MaxCantidadObjeto = 100_000;

    public const string FormatoCodigoCarga =
        "Use de 3 a 20 caracteres: letras mayúsculas, dígitos y guiones (ejemplo: CP-2026-0001).";

    public const string FormatoCodigoCatalogo =
        "Use de 2 a 50 caracteres: debe iniciar con letra y solo contener letras, dígitos y guion bajo (ejemplo: CAMION_RIGIDO).";

    [GeneratedRegex("^[A-Z0-9][A-Z0-9-]{2,19}$")]
    public static partial Regex PatronCodigoCarga();

    [GeneratedRegex("^[A-Z][A-Z0-9_]{1,49}$")]
    public static partial Regex PatronCodigoCatalogo();

    /// <summary>Recorta espacios y convierte cadenas vacías o de solo espacios en null.</summary>
    public static string? Limpiar(string? valor)
    {
        var v = valor?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    /// <summary>Los códigos se almacenan en mayúsculas para que la unicidad no dependa de mayúsculas/minúsculas.</summary>
    public static string? NormalizarCodigo(string? valor) => Limpiar(valor)?.ToUpperInvariant();

    public static void Texto(ErroresValidacion errores, string campo, string? valor, bool obligatorio,
        int maximo, int minimo = 1)
    {
        if (valor is null)
        {
            if (obligatorio) errores.Agregar(campo, $"El campo {campo} es obligatorio.");
            return;
        }
        if (valor.Length < minimo)
            errores.Agregar(campo, $"El campo {campo} debe tener al menos {minimo} caracteres.");
        if (valor.Length > maximo)
            errores.Agregar(campo, $"El campo {campo} no puede superar {maximo} caracteres.");
    }

    public static void Codigo(ErroresValidacion errores, string campo, string? valor, Regex patron, string formato)
    {
        if (valor is null)
        {
            errores.Agregar(campo, $"El campo {campo} es obligatorio.");
            return;
        }
        if (!patron.IsMatch(valor))
            errores.Agregar(campo, $"El campo {campo} tiene un formato inválido. {formato}");
    }

    /// <summary>Valida un número estrictamente positivo (RN-04, RN-05).</summary>
    public static void Positivo(ErroresValidacion errores, string campo, decimal? valor, bool obligatorio, decimal maximo)
    {
        if (valor is null)
        {
            if (obligatorio) errores.Agregar(campo, $"El campo {campo} es obligatorio.");
            return;
        }
        if (valor < 0)
            errores.Agregar(campo, $"El campo {campo} no puede ser negativo.");
        else if (valor == 0)
            errores.Agregar(campo, $"El campo {campo} debe ser mayor que cero.");
        else if (valor > maximo)
            errores.Agregar(campo, $"El campo {campo} no puede superar {maximo:0.##} (límite técnico).");
    }
}
