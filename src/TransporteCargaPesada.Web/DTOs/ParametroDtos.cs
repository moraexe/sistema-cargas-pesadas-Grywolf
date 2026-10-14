namespace TransporteCargaPesada.Web.DTOs;

/// <summary>Cuerpo de POST/PUT de parámetros.</summary>
public class ParametroRequest
{
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }

    /// <summary>ENTERO, DECIMAL, TEXTO, BOOLEANO o FECHA.</summary>
    public string? TipoDato { get; set; }

    /// <summary>Siempre se envía como texto y se valida contra <see cref="TipoDato"/>.</summary>
    public string? Valor { get; set; }

    public string? UnidadMedida { get; set; }

    /// <summary>null = parámetro general del sistema.</summary>
    public int? CategoriaId { get; set; }

    /// <summary>Si se omite: false al crear; se conserva el valor actual al actualizar.</summary>
    public bool? EsObligatorio { get; set; }

    /// <summary>Si se omite: true al crear; se conserva el valor actual al actualizar.</summary>
    public bool? Activo { get; set; }
}

public record ParametroResponse(
    int Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    string TipoDato,
    string Valor,
    string? UnidadMedida,
    string Ambito,
    int? CategoriaId,
    string? CategoriaTipo,
    string? CategoriaCodigo,
    bool EsObligatorio,
    bool Activo,
    DateTime FechaActualizacion);
