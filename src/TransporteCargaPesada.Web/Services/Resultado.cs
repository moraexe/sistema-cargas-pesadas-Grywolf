using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Services;

public enum TipoResultado
{
    Exito,
    NoEncontrado,
    Invalido,
    Conflicto,
}

/// <summary>
/// Resultado de una operación de negocio. Los controladores lo traducen a HTTP:
/// Exito → 200/201/204, NoEncontrado → 404, Invalido → 400, Conflicto → 409.
/// </summary>
public sealed class Resultado<T>
{
    public TipoResultado Tipo { get; private init; }
    public T? Valor { get; private init; }
    public string? Mensaje { get; private init; }
    public IDictionary<string, string[]>? Errores { get; private init; }

    public static Resultado<T> Ok(T valor) => new() { Tipo = TipoResultado.Exito, Valor = valor };

    public static Resultado<T> NoEncontrado(string mensaje) =>
        new() { Tipo = TipoResultado.NoEncontrado, Mensaje = mensaje };

    public static Resultado<T> Invalido(ErroresValidacion errores) =>
        new() { Tipo = TipoResultado.Invalido, Errores = errores.ComoDiccionario() };

    public static Resultado<T> Invalido(string campo, string mensaje)
    {
        var errores = new ErroresValidacion();
        errores.Agregar(campo, mensaje);
        return Invalido(errores);
    }

    public static Resultado<T> Conflicto(string mensaje) =>
        new() { Tipo = TipoResultado.Conflicto, Mensaje = mensaje };
}
