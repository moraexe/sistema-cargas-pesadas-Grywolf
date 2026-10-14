namespace TransporteCargaPesada.Web.Models;

/// <summary>
/// Configuración del sistema. Si <see cref="CategoriaId"/> es null es un parámetro general;
/// en caso contrario pertenece a la categoría indicada.
/// </summary>
public class Parametro
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>ENTERO, DECIMAL, TEXTO, BOOLEANO o FECHA.</summary>
    public string TipoDato { get; set; } = string.Empty;

    /// <summary>Valor almacenado en forma canónica (cultura invariante).</summary>
    public string Valor { get; set; } = string.Empty;

    public string? UnidadMedida { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public bool EsObligatorio { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaActualizacion { get; set; }
}
