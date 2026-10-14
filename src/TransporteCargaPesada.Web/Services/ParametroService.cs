using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Services;

public class ParametroService(AppDbContext db)
{
    public const string AmbitoGeneral = "GENERAL";
    public const string AmbitoCategoria = "CATEGORIA";

    public static IReadOnlyList<TipoCatalogoResponse> ListarTiposDato() =>
        TiposDato.Todos.Select(t => new TipoCatalogoResponse(t.Codigo, t.Descripcion)).ToList();

    public async Task<List<ParametroResponse>> ListarAsync(int? categoriaId, bool? activo)
    {
        var consulta = db.Parametros.AsNoTracking().Include(p => p.Categoria).AsQueryable();
        if (categoriaId is not null) consulta = consulta.Where(p => p.CategoriaId == categoriaId);
        if (activo is not null) consulta = consulta.Where(p => p.Activo == activo);
        var lista = await consulta.OrderBy(p => p.Codigo).ToListAsync();
        return lista.Select(ARespuesta).ToList();
    }

    public async Task<Resultado<ParametroResponse>> ObtenerAsync(int id)
    {
        var parametro = await db.Parametros.AsNoTracking().Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
        return parametro is null
            ? Resultado<ParametroResponse>.NoEncontrado($"No existe el parámetro con id {id}.")
            : Resultado<ParametroResponse>.Ok(ARespuesta(parametro));
    }

    public async Task<Resultado<ParametroResponse>> CrearAsync(ParametroRequest solicitud)
    {
        var esObligatorio = solicitud.EsObligatorio ?? false;
        var activo = solicitud.Activo ?? true;

        var (errores, datos) = await ValidarAsync(solicitud, actual: null);
        if (esObligatorio && !activo)
            errores.Agregar("activo", "Un parámetro obligatorio no puede crearse inactivo.");
        if (errores.HayErrores) return Resultado<ParametroResponse>.Invalido(errores);

        if (await db.Parametros.AnyAsync(p => p.Codigo == datos.Codigo))
            return Resultado<ParametroResponse>.Conflicto($"Ya existe un parámetro con el código {datos.Codigo}.");

        var parametro = new Parametro { EsObligatorio = esObligatorio, Activo = activo };
        Aplicar(parametro, datos);
        db.Parametros.Add(parametro);
        await db.SaveChangesAsync();
        return await ObtenerAsync(parametro.Id);
    }

    public async Task<Resultado<ParametroResponse>> ActualizarAsync(int id, ParametroRequest solicitud)
    {
        var parametro = await db.Parametros.FirstOrDefaultAsync(p => p.Id == id);
        if (parametro is null) return Resultado<ParametroResponse>.NoEncontrado($"No existe el parámetro con id {id}.");

        var (errores, datos) = await ValidarAsync(solicitud, actual: parametro);
        if (errores.HayErrores) return Resultado<ParametroResponse>.Invalido(errores);

        var esObligatorio = solicitud.EsObligatorio ?? parametro.EsObligatorio;
        var activo = solicitud.Activo ?? parametro.Activo;

        // RN-19: un parámetro obligatorio conserva su código, su tipo y su estado activo.
        if (parametro.EsObligatorio)
        {
            if (datos.Codigo != parametro.Codigo)
                return Resultado<ParametroResponse>.Conflicto($"El parámetro {parametro.Codigo} es obligatorio: no se puede cambiar su código.");
            if (datos.TipoDato != parametro.TipoDato)
                return Resultado<ParametroResponse>.Conflicto($"El parámetro {parametro.Codigo} es obligatorio: no se puede cambiar su tipo de dato.");
            if (!esObligatorio)
                return Resultado<ParametroResponse>.Conflicto($"El parámetro {parametro.Codigo} es obligatorio: no se puede marcar como no obligatorio.");
            if (!activo)
                return Resultado<ParametroResponse>.Conflicto($"El parámetro {parametro.Codigo} es obligatorio: no se puede desactivar.");
        }
        else if (esObligatorio && !activo)
        {
            return Resultado<ParametroResponse>.Invalido("activo", "Un parámetro obligatorio no puede estar inactivo.");
        }

        if (await db.Parametros.AnyAsync(p => p.Codigo == datos.Codigo && p.Id != id))
            return Resultado<ParametroResponse>.Conflicto($"Ya existe un parámetro con el código {datos.Codigo}.");

        Aplicar(parametro, datos);
        parametro.EsObligatorio = esObligatorio;
        parametro.Activo = activo;
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    /// <summary>
    /// Política RN-19: los parámetros obligatorios (usados por las reglas del sistema) no se eliminan;
    /// los demás se eliminan físicamente o pueden desactivarse.
    /// </summary>
    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        var parametro = await db.Parametros.FirstOrDefaultAsync(p => p.Id == id);
        if (parametro is null) return Resultado<bool>.NoEncontrado($"No existe el parámetro con id {id}.");
        if (parametro.EsObligatorio)
            return Resultado<bool>.Conflicto($"El parámetro {parametro.Codigo} es obligatorio y no puede eliminarse.");

        db.Parametros.Remove(parametro);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true);
    }

    private sealed record DatosParametro(string Codigo, string Nombre, string? Descripcion, string TipoDato,
        string Valor, string? UnidadMedida, int? CategoriaId);

    private async Task<(ErroresValidacion, DatosParametro)> ValidarAsync(ParametroRequest s, Parametro? actual)
    {
        var errores = new ErroresValidacion();
        var codigo = ReglasValidacion.NormalizarCodigo(s.Codigo);
        var nombre = ReglasValidacion.Limpiar(s.Nombre);
        var descripcion = ReglasValidacion.Limpiar(s.Descripcion);
        var tipoDato = ReglasValidacion.NormalizarCodigo(s.TipoDato);
        var valor = ReglasValidacion.Limpiar(s.Valor);
        var unidad = ReglasValidacion.Limpiar(s.UnidadMedida);

        ReglasValidacion.Codigo(errores, "codigo", codigo, ReglasValidacion.PatronCodigoCatalogo(), ReglasValidacion.FormatoCodigoCatalogo);
        ReglasValidacion.Texto(errores, "nombre", nombre, obligatorio: true, ReglasValidacion.MaxNombre, minimo: 2);
        ReglasValidacion.Texto(errores, "descripcion", descripcion, obligatorio: false, ReglasValidacion.MaxDescripcion);
        ReglasValidacion.Texto(errores, "unidadMedida", unidad, obligatorio: false, ReglasValidacion.MaxUnidadMedida);

        if (tipoDato is null)
            errores.Agregar("tipoDato", "El campo tipoDato es obligatorio.");
        else if (!TiposDato.EsValido(tipoDato))
            errores.Agregar("tipoDato", $"El tipo de dato '{tipoDato}' no es válido. Valores permitidos: {TiposDato.ListaTexto}.");

        // RN-10: el valor debe ser compatible con el tipo de dato declarado.
        ReglasValidacion.Texto(errores, "valor", valor, obligatorio: true, ReglasValidacion.MaxValorParametro);
        if (valor is not null && TiposDato.EsValido(tipoDato) && !errores.TieneError("valor"))
        {
            if (TiposDato.TryNormalizar(tipoDato!, valor, out var normalizado, out var mensaje))
                valor = normalizado;
            else
                errores.Agregar("valor", mensaje);
        }

        Categoria? categoria = null;
        if (s.CategoriaId is not null)
        {
            categoria = await db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == s.CategoriaId);
            if (categoria is null)
                errores.Agregar("categoriaId", $"No existe una categoría con id {s.CategoriaId}.");
            else if (!categoria.Activo && s.CategoriaId != actual?.CategoriaId)
                errores.Agregar("categoriaId", $"La categoría {categoria.Id} ({categoria.Codigo}) está inactiva y no puede asignarse.");
        }

        // RN-17: los parámetros de límite usados por las reglas tienen restricciones adicionales.
        if (codigo is not null && !errores.TieneError("codigo") && ParametrosSistema.EsLimite(codigo))
        {
            if (tipoDato is not null && TiposDato.EsValido(tipoDato) && !TiposDato.EsNumerico(tipoDato))
                errores.Agregar("tipoDato", $"El parámetro {codigo} es un límite y debe ser de tipo {TiposDato.Decimal} o {TiposDato.Entero}.");
            else if (!errores.TieneError("valor") && TiposDato.EsNumerico(tipoDato) && TiposDato.ComoDecimal(valor!) <= 0)
                errores.Agregar("valor", $"El parámetro {codigo} es un límite y su valor debe ser mayor que cero.");

            if (ParametrosSistema.LimitesGenerales.Contains(codigo) && s.CategoriaId is not null)
                errores.Agregar("categoriaId", $"El parámetro {codigo} es general del sistema y no puede asociarse a una categoría.");
            if (ParametrosSistema.EsCapacidadVehiculo(codigo))
            {
                if (s.CategoriaId is null)
                    errores.Agregar("categoriaId", $"El parámetro {codigo} debe asociarse a una categoría de tipo {TiposCategoria.Vehiculo}.");
                else if (categoria is not null && categoria.TipoCategoria != TiposCategoria.Vehiculo)
                    errores.Agregar("categoriaId",
                        $"El parámetro {codigo} debe asociarse a una categoría de tipo {TiposCategoria.Vehiculo}; la categoría {categoria.Id} es de tipo {categoria.TipoCategoria}.");
            }
        }

        var datos = errores.HayErrores
            ? null!
            : new DatosParametro(codigo!, nombre!, descripcion, tipoDato!, valor!, unidad, s.CategoriaId);
        return (errores, datos);
    }

    private static void Aplicar(Parametro p, DatosParametro d)
    {
        p.Codigo = d.Codigo;
        p.Nombre = d.Nombre;
        p.Descripcion = d.Descripcion;
        p.TipoDato = d.TipoDato;
        p.Valor = d.Valor;
        p.UnidadMedida = d.UnidadMedida;
        p.CategoriaId = d.CategoriaId;
        p.FechaActualizacion = DateTime.UtcNow;
    }

    private static ParametroResponse ARespuesta(Parametro p) => new(
        p.Id, p.Codigo, p.Nombre, p.Descripcion, p.TipoDato, p.Valor, p.UnidadMedida,
        p.CategoriaId is null ? AmbitoGeneral : AmbitoCategoria,
        p.CategoriaId, p.Categoria?.TipoCategoria, p.Categoria?.Codigo,
        p.EsObligatorio, p.Activo, p.FechaActualizacion);
}
