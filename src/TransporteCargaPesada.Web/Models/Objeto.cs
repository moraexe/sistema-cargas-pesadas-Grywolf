namespace TransporteCargaPesada.Web.Models;

/// <summary>Mercancía, máquina o equipo físico que forma parte de una carga.</summary>
public class Objeto
{
    public int Id { get; set; }

    public int CargaId { get; set; }
    public Carga? Carga { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Cantidad { get; set; }
    public decimal PesoUnitarioKg { get; set; }
    public decimal? VolumenUnitarioM3 { get; set; }

    /// <summary>Categoría opcional de tipo TIPO_OBJETO.</summary>
    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string? Observaciones { get; set; }
}
