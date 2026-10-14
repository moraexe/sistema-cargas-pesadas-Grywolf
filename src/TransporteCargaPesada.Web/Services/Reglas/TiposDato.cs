using System.Globalization;

namespace TransporteCargaPesada.Web.Services.Reglas;

/// <summary>Tipos de dato permitidos para los parámetros y su validación (RN-10).</summary>
public static class TiposDato
{
    public const string Entero = "ENTERO";
    public const string Decimal = "DECIMAL";
    public const string Texto = "TEXTO";
    public const string Booleano = "BOOLEANO";
    public const string Fecha = "FECHA";

    public static readonly IReadOnlyList<(string Codigo, string Descripcion)> Todos =
    [
        (Entero, "Número entero, por ejemplo 50"),
        (Decimal, "Número decimal con punto, por ejemplo 40000.5"),
        (Texto, "Texto libre de hasta 200 caracteres"),
        (Booleano, "true o false"),
        (Fecha, "Fecha con formato yyyy-MM-dd, por ejemplo 2026-01-31"),
    ];

    private const NumberStyles EstiloDecimal = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public static bool EsValido(string? tipo) =>
        tipo is not null && Todos.Any(t => t.Codigo == tipo);

    public static string ListaTexto => string.Join(", ", Todos.Select(t => t.Codigo));

    public static bool EsNumerico(string? tipo) => tipo is Entero or Decimal;

    /// <summary>
    /// Valida <paramref name="valor"/> contra <paramref name="tipoDato"/> y devuelve su forma canónica
    /// (cultura invariante), que es la que se almacena.
    /// </summary>
    public static bool TryNormalizar(string tipoDato, string valor, out string normalizado, out string mensajeError)
    {
        normalizado = valor;
        mensajeError = string.Empty;
        var v = valor.Trim();

        switch (tipoDato)
        {
            case Entero:
                if (long.TryParse(v, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var entero))
                {
                    normalizado = entero.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
                mensajeError = $"El valor '{valor}' no es un número entero válido para el tipo ENTERO.";
                return false;

            case Decimal:
                if (decimal.TryParse(v, EstiloDecimal, CultureInfo.InvariantCulture, out var dec))
                {
                    normalizado = dec.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
                mensajeError = $"El valor '{valor}' no es un número válido para el tipo DECIMAL (use punto como separador decimal).";
                return false;

            case Booleano:
                if (bool.TryParse(v, out var b))
                {
                    normalizado = b ? "true" : "false";
                    return true;
                }
                mensajeError = $"El valor '{valor}' no es válido para el tipo BOOLEANO; use true o false.";
                return false;

            case Fecha:
                if (DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                {
                    normalizado = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return true;
                }
                mensajeError = $"El valor '{valor}' no es una fecha válida para el tipo FECHA (formato yyyy-MM-dd).";
                return false;

            case Texto:
                normalizado = v;
                return true;

            default:
                mensajeError = $"Tipo de dato desconocido: {tipoDato}.";
                return false;
        }
    }

    public static decimal? ComoDecimal(string valor) =>
        decimal.TryParse(valor, EstiloDecimal, CultureInfo.InvariantCulture, out var d) ? d : null;
}
