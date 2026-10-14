namespace TransporteCargaPesada.Web.DTOs;

/// <summary>Cuerpo de POST/PUT de cargas. PUT reemplaza todos los campos editables.</summary>
public class CargaRequest
{
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Origen { get; set; }
    public string? Destino { get; set; }
    public decimal? PesoTotalKg { get; set; }
    public decimal? VolumenTotalM3 { get; set; }
    public int? TipoCargaId { get; set; }
    public int? TipoVehiculoId { get; set; }
    public int? TipoEntregaId { get; set; }

    /// <summary>Si se omite al crear se asigna el estado PENDIENTE; al actualizar es obligatorio.</summary>
    public int? EstadoId { get; set; }

    public string? Observaciones { get; set; }
}

public record CargaResponse(
    int Id,
    string Codigo,
    string Descripcion,
    string Origen,
    string Destino,
    decimal PesoTotalKg,
    decimal? VolumenTotalM3,
    int TipoCargaId,
    string? TipoCargaNombre,
    int? TipoVehiculoId,
    string? TipoVehiculoNombre,
    int TipoEntregaId,
    string? TipoEntregaNombre,
    int EstadoId,
    string? EstadoCodigo,
    string? EstadoNombre,
    DateTime FechaRegistro,
    string? Observaciones,
    int CantidadObjetos,
    decimal PesoObjetosKg);
