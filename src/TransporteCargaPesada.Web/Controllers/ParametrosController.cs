using Microsoft.AspNetCore.Mvc;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Services;

namespace TransporteCargaPesada.Web.Controllers;

[Route("api/parametros")]
public class ParametrosController(ParametroService servicio) : ApiControllerBase
{
    /// <summary>Lista los parámetros, filtrando opcionalmente por categoría y por estado activo.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ParametroResponse>>> Listar([FromQuery] int? categoriaId, [FromQuery] bool? activo) =>
        await servicio.ListarAsync(categoriaId, activo);

    /// <summary>Lista los tipos de dato permitidos.</summary>
    [HttpGet("tipos-dato")]
    public ActionResult<IReadOnlyList<TipoCatalogoResponse>> ListarTiposDato() => Ok(ParametroService.ListarTiposDato());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id) =>
        Responder(await servicio.ObtenerAsync(id), parametro => Ok(parametro));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ParametroRequest datos) =>
        Responder(await servicio.CrearAsync(datos), parametro => CreatedAtAction(nameof(Obtener), new { id = parametro.Id }, parametro));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ParametroRequest datos) =>
        Responder(await servicio.ActualizarAsync(id, datos), parametro => Ok(parametro));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id) =>
        Responder(await servicio.EliminarAsync(id), _ => NoContent());
}
