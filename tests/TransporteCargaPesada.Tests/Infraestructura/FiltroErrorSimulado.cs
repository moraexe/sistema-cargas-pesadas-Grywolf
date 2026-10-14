using Microsoft.AspNetCore.Mvc.Filters;

namespace TransporteCargaPesada.Tests.Infraestructura;

/// <summary>
/// Solo existe en las pruebas: si la solicitud trae el encabezado <c>X-Simular-Error: 1</c>, lanza una
/// excepción con información sensible para comprobar que el cliente no la recibe (RN-16).
/// </summary>
public class FiltroErrorSimulado : IActionFilter
{
    public const string Encabezado = "X-Simular-Error";
    public const string MensajeSensible = "Fallo simulado. Data Source=C:/secreto/produccion.db;Password=1234";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers[Encabezado] == "1")
            throw new InvalidOperationException(MensajeSensible);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
