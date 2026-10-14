using Microsoft.AspNetCore.Mvc;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Services;

namespace TransporteCargaPesada.Web.Controllers;

[Route("api/categorias")]
public class CategoriasController(CategoriaService servicio) : ApiControllerBase
{
    /// <summary>Lista las categorías, filtrando opcionalmente por tipo y por estado activo.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? tipoCategoria, [FromQuery] bool? activo) =>
        Responder(await servicio.ListarAsync(tipoCategoria, activo), lista => Ok(lista));

    /// <summary>Lista los tipos de categoría permitidos.</summary>
    [HttpGet("tipos")]
    public ActionResult<IReadOnlyList<TipoCatalogoResponse>> ListarTipos() => Ok(CategoriaService.ListarTipos());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id) =>
        Responder(await servicio.ObtenerAsync(id), categoria => Ok(categoria));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CategoriaRequest datos) =>
        Responder(await servicio.CrearAsync(datos), categoria => CreatedAtAction(nameof(Obtener), new { id = categoria.Id }, categoria));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CategoriaRequest datos) =>
        Responder(await servicio.ActualizarAsync(id, datos), categoria => Ok(categoria));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id) =>
        Responder(await servicio.EliminarAsync(id), _ => NoContent());
}
