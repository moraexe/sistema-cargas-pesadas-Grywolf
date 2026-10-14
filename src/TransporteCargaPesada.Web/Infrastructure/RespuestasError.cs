using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TransporteCargaPesada.Web.Infrastructure;

/// <summary>
/// Formato único de errores de la API (RN-15, RN-16), basado en ProblemDetails (RFC 9457):
/// <code>{ "type", "title", "status", "detail"?, "errors"?, "traceId" }</code>
/// </summary>
public static class RespuestasError
{
    public const string TituloValidacion = "Uno o más datos no son válidos.";
    public const string TituloNoEncontrado = "Recurso no encontrado.";
    public const string TituloConflicto = "Conflicto con el estado actual de los datos.";
    public const string TituloErrorInterno = "Error interno del servidor.";

    /// <summary>Nombre de los parámetros [FromBody] de los controladores.</summary>
    public const string NombreParametroCuerpo = "datos";
    public const string CampoCuerpo = "cuerpo";

    private static readonly Dictionary<int, string> Tipos = new()
    {
        [400] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        [404] = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        [409] = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        [500] = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
    };

    public static string TraceId(HttpContext contexto) => Activity.Current?.Id ?? contexto.TraceIdentifier;

    public static ValidationProblemDetails Validacion(HttpContext contexto, IDictionary<string, string[]> errores)
    {
        var detalles = new ValidationProblemDetails(errores)
        {
            Type = Tipos[400],
            Title = TituloValidacion,
            Status = StatusCodes.Status400BadRequest,
        };
        detalles.Extensions["traceId"] = TraceId(contexto);
        return detalles;
    }

    public static ProblemDetails Problema(HttpContext contexto, int estado, string titulo, string? detalle)
    {
        var detalles = new ProblemDetails
        {
            Type = Tipos.GetValueOrDefault(estado),
            Title = titulo,
            Status = estado,
            Detail = detalle,
        };
        detalles.Extensions["traceId"] = TraceId(contexto);
        return detalles;
    }

    /// <summary>
    /// Reemplaza la respuesta automática de [ApiController] cuando el JSON está mal formado,
    /// tiene tipos incorrectos o un parámetro de consulta no se puede convertir. Los mensajes
    /// se traducen y no incluyen nombres de tipos .NET ni detalles internos.
    /// </summary>
    public static IActionResult DesdeModelState(ActionContext contexto)
    {
        var errores = new Dictionary<string, string[]>();
        var hayErroresDeCampo = false;

        foreach (var (clave, entrada) in contexto.ModelState)
        {
            if (entrada.Errors.Count == 0) continue;

            if (clave.StartsWith('$'))
            {
                var campo = clave.TrimStart('$', '.');
                if (string.IsNullOrEmpty(campo))
                {
                    errores[CampoCuerpo] = ["El cuerpo de la solicitud no es un JSON válido."];
                }
                else
                {
                    errores[campo] = ["El valor tiene un tipo de dato incorrecto o el JSON está mal formado en este campo."];
                    hayErroresDeCampo = true;
                }
            }
            else if (clave == NombreParametroCuerpo || clave.Length == 0)
            {
                errores.TryAdd(CampoCuerpo, ["El cuerpo de la solicitud es obligatorio y debe ser un JSON válido."]);
            }
            else
            {
                var valor = entrada.AttemptedValue;
                errores[clave] = [$"El valor '{valor}' no es válido para el parámetro {clave}."];
            }
        }

        // Si se identificó el campo exacto, el error genérico del cuerpo no aporta información.
        if (hayErroresDeCampo) errores.Remove(CampoCuerpo);

        return new BadRequestObjectResult(Validacion(contexto.HttpContext, errores))
        {
            ContentTypes = { "application/problem+json" },
        };
    }

    /// <summary>
    /// Manejador global de excepciones: registra el detalle en el log del servidor y responde al cliente
    /// sin pila de llamadas, mensajes de excepción ni cadenas de conexión.
    /// </summary>
    public static async Task ManejarExcepcionAsync(HttpContext contexto)
    {
        var excepcion = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = contexto.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TransporteCargaPesada.Errores");
        logger.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);

        if (!contexto.Request.Path.StartsWithSegments("/api"))
        {
            contexto.Response.Redirect("/Error");
            return;
        }

        // Respaldo ante condiciones de carrera: la base de datos rechazó un duplicado o una referencia.
        var (estado, titulo, detalle) = excepcion is DbUpdateException
            ? (StatusCodes.Status409Conflict, TituloConflicto, "La operación viola una restricción de integridad de los datos.")
            : (StatusCodes.Status500InternalServerError, TituloErrorInterno, "Ocurrió un error inesperado. Intente de nuevo o contacte al administrador.");

        contexto.Response.StatusCode = estado;
        await contexto.Response.WriteAsJsonAsync(Problema(contexto, estado, titulo, detalle),
            options: null, contentType: "application/problem+json");
    }
}
