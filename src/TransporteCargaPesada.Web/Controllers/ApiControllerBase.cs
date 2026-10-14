using Microsoft.AspNetCore.Mvc;
using TransporteCargaPesada.Web.Infrastructure;
using TransporteCargaPesada.Web.Services;

namespace TransporteCargaPesada.Web.Controllers;

/// <summary>
/// Traduce los <see cref="Resultado{T}"/> de los servicios a respuestas HTTP coherentes (RN-15).
/// </summary>
/// <remarks>
/// No se usa [Produces("application/json")]: ese filtro reemplaza el ContentType de todas las respuestas,
/// incluidos los errores, que deben salir como application/problem+json (defecto DEF-001).
/// </remarks>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Responder<T>(Resultado<T> resultado, Func<T, IActionResult> siExito) => resultado.Tipo switch
    {
        TipoResultado.Exito => siExito(resultado.Valor!),
        TipoResultado.NoEncontrado => Problema(StatusCodes.Status404NotFound, RespuestasError.TituloNoEncontrado, resultado.Mensaje),
        TipoResultado.Conflicto => Problema(StatusCodes.Status409Conflict, RespuestasError.TituloConflicto, resultado.Mensaje),
        _ => new BadRequestObjectResult(RespuestasError.Validacion(HttpContext, resultado.Errores!))
        {
            ContentTypes = { "application/problem+json" },
        },
    };

    private ObjectResult Problema(int estado, string titulo, string? detalle) =>
        new(RespuestasError.Problema(HttpContext, estado, titulo, detalle))
        {
            StatusCode = estado,
            ContentTypes = { "application/problem+json" },
        };
}
