namespace TransporteCargaPesada.Web.Services.Reglas;

/// <summary>
/// Lista centralizada de tipos de categoría permitidos (RN-07). Cualquier validación de
/// "tipo de categoría" en la API, la interfaz o las pruebas se basa en esta clase.
/// </summary>
public static class TiposCategoria
{
    public const string Vehiculo = "VEHICULO";
    public const string TipoCarga = "TIPO_CARGA";
    public const string TipoEntrega = "TIPO_ENTREGA";
    public const string EstadoCarga = "ESTADO_CARGA";
    public const string UnidadMedida = "UNIDAD_MEDIDA";
    public const string TipoObjeto = "TIPO_OBJETO";

    /// <summary>Código y descripción de cada tipo, en el orden en que se muestran.</summary>
    public static readonly IReadOnlyList<(string Codigo, string Descripcion)> Todos =
    [
        (Vehiculo, "Tipo de vehículo"),
        (TipoCarga, "Tipo de carga"),
        (TipoEntrega, "Tipo de entrega"),
        (EstadoCarga, "Estado de la carga"),
        (UnidadMedida, "Unidad de medida"),
        (TipoObjeto, "Tipo de objeto transportado"),
    ];

    public static bool EsValido(string? tipo) =>
        tipo is not null && Todos.Any(t => t.Codigo == tipo);

    public static string ListaTexto => string.Join(", ", Todos.Select(t => t.Codigo));

    /// <summary>Código del estado que se asigna a una carga cuando no se indica uno.</summary>
    public const string CodigoEstadoInicial = "PENDIENTE";
}
