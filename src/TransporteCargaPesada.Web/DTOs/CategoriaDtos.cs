namespace TransporteCargaPesada.Web.DTOs;

/// <summary>
/// Cuerpo de POST/PUT de categorías. Las propiedades son anulables para que el servicio detecte
/// los campos omitidos y los informe como obligatorios (RN-02).
/// </summary>
public class CategoriaRequest
{
    public string? TipoCategoria { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }

    /// <summary>Si se omite: true al crear; se conserva el valor actual al actualizar.</summary>
    public bool? Activo { get; set; }
}

public record CategoriaResponse(
    int Id,
    string TipoCategoria,
    string Codigo,
    string Nombre,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion);

public record TipoCatalogoResponse(string Codigo, string Descripcion);
