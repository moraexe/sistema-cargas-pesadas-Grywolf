namespace TransporteCargaPesada.Web.DTOs;

/// <summary>Cuerpo de POST/PUT de objetos.</summary>
public class ObjetoRequest
{
    public int? CargaId { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int? Cantidad { get; set; }
    public decimal? PesoUnitarioKg { get; set; }
    public decimal? VolumenUnitarioM3 { get; set; }

    /// <summary>Categoría opcional de tipo TIPO_OBJETO.</summary>
    public int? CategoriaId { get; set; }

    public string? Observaciones { get; set; }
}

public record ObjetoResponse(
    int Id,
    int CargaId,
    string? CargaCodigo,
    string Nombre,
    string? Descripcion,
    int Cantidad,
    decimal PesoUnitarioKg,
    decimal? VolumenUnitarioM3,
    decimal PesoTotalKg,
    int? CategoriaId,
    string? CategoriaNombre,
    string? Observaciones);
