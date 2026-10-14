namespace TransporteCargaPesada.Web.Models;

/// <summary>Servicio de transporte de carga pesada entre un origen y un destino.</summary>
public class Carga
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public decimal PesoTotalKg { get; set; }
    public decimal? VolumenTotalM3 { get; set; }

    public int TipoCargaId { get; set; }
    public Categoria? TipoCarga { get; set; }

    public int? TipoVehiculoId { get; set; }
    public Categoria? TipoVehiculo { get; set; }

    public int TipoEntregaId { get; set; }
    public Categoria? TipoEntrega { get; set; }

    public int EstadoId { get; set; }
    public Categoria? Estado { get; set; }

    public DateTime FechaRegistro { get; set; }
    public string? Observaciones { get; set; }

    public List<Objeto> Objetos { get; set; } = new();
}
