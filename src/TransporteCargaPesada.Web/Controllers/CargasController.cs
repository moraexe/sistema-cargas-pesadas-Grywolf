using Microsoft.AspNetCore.Mvc;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Services;

namespace TransporteCargaPesada.Web.Controllers;

[Route("api/cargas")]
public class CargasController(CargaService servicio) : ApiControllerBase
{
    /// <summary>Lista todas las cargas.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CargaResponse>>> Listar() => await servicio.ListarAsync();

    /// <summary>Obtiene una carga por id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id) =>
        Responder(await servicio.ObtenerAsync(id), carga => Ok(carga));

    /// <summary>Lista los objetos de una carga.</summary>
    [HttpGet("{id:int}/objetos")]
    public async Task<IActionResult> ListarObjetos(int id) =>
        Responder(await servicio.ListarObjetosAsync(id), objetos => Ok(objetos));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CargaRequest datos) =>
        Responder(await servicio.CrearAsync(datos), carga => CreatedAtAction(nameof(Obtener), new { id = carga.Id }, carga));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CargaRequest datos) =>
        Responder(await servicio.ActualizarAsync(id, datos), carga => Ok(carga));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id) =>
        Responder(await servicio.EliminarAsync(id), _ => NoContent());
}
