using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Services;

public class CategoriaService(AppDbContext db)
{
    public static IReadOnlyList<TipoCatalogoResponse> ListarTipos() =>
        TiposCategoria.Todos.Select(t => new TipoCatalogoResponse(t.Codigo, t.Descripcion)).ToList();

    public async Task<Resultado<List<CategoriaResponse>>> ListarAsync(string? tipoCategoria, bool? activo)
    {
        var consulta = db.Categorias.AsNoTracking();

        if (tipoCategoria is not null)
        {
            var tipo = ReglasValidacion.NormalizarCodigo(tipoCategoria);
            if (!TiposCategoria.EsValido(tipo))
                return Resultado<List<CategoriaResponse>>.Invalido("tipoCategoria",
                    $"El tipo de categoría '{tipoCategoria}' no es válido. Valores permitidos: {TiposCategoria.ListaTexto}.");
            consulta = consulta.Where(c => c.TipoCategoria == tipo);
        }
        if (activo is not null)
            consulta = consulta.Where(c => c.Activo == activo);

        var lista = await consulta.OrderBy(c => c.TipoCategoria).ThenBy(c => c.Nombre).ToListAsync();
        return Resultado<List<CategoriaResponse>>.Ok(lista.Select(ARespuesta).ToList());
    }

    public async Task<Resultado<CategoriaResponse>> ObtenerAsync(int id)
    {
        var categoria = await db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return categoria is null
            ? Resultado<CategoriaResponse>.NoEncontrado($"No existe la categoría con id {id}.")
            : Resultado<CategoriaResponse>.Ok(ARespuesta(categoria));
    }

    public async Task<Resultado<CategoriaResponse>> CrearAsync(CategoriaRequest solicitud)
    {
        var (errores, datos) = Validar(solicitud);
        if (errores.HayErrores) return Resultado<CategoriaResponse>.Invalido(errores);

        if (await ExisteCodigoAsync(datos.Tipo!, datos.Codigo!, excluirId: null))
            return Resultado<CategoriaResponse>.Conflicto(
                $"Ya existe una categoría con el código {datos.Codigo} en el tipo {datos.Tipo}.");

        var categoria = new Categoria
        {
            TipoCategoria = datos.Tipo!,
            Codigo = datos.Codigo!,
            Nombre = datos.Nombre!,
            Descripcion = datos.Descripcion,
            Activo = solicitud.Activo ?? true,
            FechaCreacion = DateTime.UtcNow,
        };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        return Resultado<CategoriaResponse>.Ok(ARespuesta(categoria));
    }

    public async Task<Resultado<CategoriaResponse>> ActualizarAsync(int id, CategoriaRequest solicitud)
    {
        var categoria = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null) return Resultado<CategoriaResponse>.NoEncontrado($"No existe la categoría con id {id}.");

        var (errores, datos) = Validar(solicitud);
        if (errores.HayErrores) return Resultado<CategoriaResponse>.Invalido(errores);

        // RN-14: cambiar el tipo de una categoría en uso rompería la coherencia de quienes la referencian.
        if (datos.Tipo != categoria.TipoCategoria && await ContarUsosAsync(id) is var usos && usos.Total > 0)
            return Resultado<CategoriaResponse>.Conflicto(
                $"No se puede cambiar el tipo de la categoría {categoria.Codigo} porque está en uso ({usos.Descripcion}).");

        if (await ExisteCodigoAsync(datos.Tipo!, datos.Codigo!, excluirId: id))
            return Resultado<CategoriaResponse>.Conflicto(
                $"Ya existe una categoría con el código {datos.Codigo} en el tipo {datos.Tipo}.");

        categoria.TipoCategoria = datos.Tipo!;
        categoria.Codigo = datos.Codigo!;
        categoria.Nombre = datos.Nombre!;
        categoria.Descripcion = datos.Descripcion;
        categoria.Activo = solicitud.Activo ?? categoria.Activo;
        await db.SaveChangesAsync();
        return Resultado<CategoriaResponse>.Ok(ARespuesta(categoria));
    }

    /// <summary>
    /// Política RN-14: una categoría referenciada por cargas, objetos o parámetros no se elimina;
    /// se debe desactivar (activo = false) para conservar la integridad histórica.
    /// </summary>
    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        var categoria = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria is null) return Resultado<bool>.NoEncontrado($"No existe la categoría con id {id}.");

        var usos = await ContarUsosAsync(id);
        if (usos.Total > 0)
            return Resultado<bool>.Conflicto(
                $"La categoría {categoria.Codigo} está en uso ({usos.Descripcion}) y no puede eliminarse. Desactívela (activo = false) en su lugar.");

        db.Categorias.Remove(categoria);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true);
    }

    private sealed record Usos(int Cargas, int Objetos, int Parametros)
    {
        public int Total => Cargas + Objetos + Parametros;
        public string Descripcion => $"{Cargas} carga(s), {Objetos} objeto(s), {Parametros} parámetro(s)";
    }

    private async Task<Usos> ContarUsosAsync(int id) => new(
        await db.Cargas.CountAsync(c => c.TipoCargaId == id || c.TipoVehiculoId == id || c.TipoEntregaId == id || c.EstadoId == id),
        await db.Objetos.CountAsync(o => o.CategoriaId == id),
        await db.Parametros.CountAsync(p => p.CategoriaId == id));

    private Task<bool> ExisteCodigoAsync(string tipo, string codigo, int? excluirId) =>
        db.Categorias.AnyAsync(c => c.TipoCategoria == tipo && c.Codigo == codigo && c.Id != excluirId);

    private sealed record DatosCategoria(string? Tipo, string? Codigo, string? Nombre, string? Descripcion);

    private static (ErroresValidacion, DatosCategoria) Validar(CategoriaRequest s)
    {
        var errores = new ErroresValidacion();
        var datos = new DatosCategoria(
            ReglasValidacion.NormalizarCodigo(s.TipoCategoria),
            ReglasValidacion.NormalizarCodigo(s.Codigo),
            ReglasValidacion.Limpiar(s.Nombre),
            ReglasValidacion.Limpiar(s.Descripcion));

        if (datos.Tipo is null)
            errores.Agregar("tipoCategoria", "El campo tipoCategoria es obligatorio.");
        else if (!TiposCategoria.EsValido(datos.Tipo))
            errores.Agregar("tipoCategoria",
                $"El tipo de categoría '{datos.Tipo}' no es válido. Valores permitidos: {TiposCategoria.ListaTexto}.");

        ReglasValidacion.Codigo(errores, "codigo", datos.Codigo, ReglasValidacion.PatronCodigoCatalogo(),
            ReglasValidacion.FormatoCodigoCatalogo);
        ReglasValidacion.Texto(errores, "nombre", datos.Nombre, obligatorio: true, ReglasValidacion.MaxNombre, minimo: 2);
        ReglasValidacion.Texto(errores, "descripcion", datos.Descripcion, obligatorio: false, ReglasValidacion.MaxDescripcion);
        return (errores, datos);
    }

    public static CategoriaResponse ARespuesta(Categoria c) =>
        new(c.Id, c.TipoCategoria, c.Codigo, c.Nombre, c.Descripcion, c.Activo, c.FechaCreacion);
}
