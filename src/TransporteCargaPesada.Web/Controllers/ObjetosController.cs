using Microsoft.AspNetCore.Mvc;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Services;

namespace TransporteCargaPesada.Web.Controllers;

[Route("api/objetos")]
public class ObjetosController(ObjetoService servicio) : ApiControllerBase
{
    /// <summary>Lista los objetos; opcionalmente solo los de una carga.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ObjetoResponse>>> Listar([FromQuery] int? cargaId) =>
        await servicio.ListarAsync(cargaId);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id) =>
        Responder(await servicio.ObtenerAsync(id), objeto => Ok(objeto));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ObjetoRequest datos) =>
        Responder(await servicio.CrearAsync(datos), objeto => CreatedAtAction(nameof(Obtener), new { id = objeto.Id }, objeto));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ObjetoRequest datos) =>
        Responder(await servicio.ActualizarAsync(id, datos), objeto => Ok(objeto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id) =>
        Responder(await servicio.EliminarAsync(id), _ => NoContent());
}
