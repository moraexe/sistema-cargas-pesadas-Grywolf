namespace TransporteCargaPesada.Web.Models;

/// <summary>
/// Registro de catálogo genérico. <see cref="TipoCategoria"/> determina su finalidad
/// (VEHICULO, TIPO_CARGA, TIPO_ENTREGA, ESTADO_CARGA, UNIDAD_MEDIDA, TIPO_OBJETO).
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string TipoCategoria { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
}
