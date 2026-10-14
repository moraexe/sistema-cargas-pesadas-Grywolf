using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Services;

/// <summary>
/// Validación centralizada de referencias a categorías (RN-06, RN-07, RN-08).
/// </summary>
public class ValidadorReferencias(AppDbContext db)
{
    /// <summary>
    /// Comprueba que <paramref name="id"/> exista, sea del <paramref name="tipoEsperado"/> y esté activa.
    /// Una categoría inactiva solo se acepta si ya era el valor actual del registro (conserva el historial).
    /// </summary>
    /// <returns>La categoría válida, o null si no se indicó o es inválida (el error queda en <paramref name="errores"/>).</returns>
    public async Task<Categoria?> ValidarCategoriaAsync(ErroresValidacion errores, string campo, int? id,
        string tipoEsperado, bool obligatorio, int? valorActual = null)
    {
        if (id is null)
        {
            if (obligatorio) errores.Agregar(campo, $"El campo {campo} es obligatorio.");
            return null;
        }

        var categoria = await db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null)
        {
            errores.Agregar(campo, $"No existe una categoría con id {id}.");
            return null;
        }
        if (categoria.TipoCategoria != tipoEsperado)
        {
            errores.Agregar(campo,
                $"La categoría {id} ({categoria.Codigo}) es de tipo {categoria.TipoCategoria} y no puede usarse como {tipoEsperado}.");
            return null;
        }
        if (!categoria.Activo && id != valorActual)
        {
            errores.Agregar(campo, $"La categoría {id} ({categoria.Codigo}) está inactiva y no puede asignarse.");
            return null;
        }
        return categoria;
    }
}
