using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Services;

public class ObjetoService(AppDbContext db, ValidadorReferencias referencias)
{
    public async Task<List<ObjetoResponse>> ListarAsync(int? cargaId)
    {
        var consulta = ConsultaCompleta();
        if (cargaId is not null) consulta = consulta.Where(o => o.CargaId == cargaId);
        var objetos = await consulta.OrderBy(o => o.Id).ToListAsync();
        return objetos.Select(ARespuesta).ToList();
    }

    public async Task<Resultado<ObjetoResponse>> ObtenerAsync(int id)
    {
        var objeto = await ConsultaCompleta().FirstOrDefaultAsync(o => o.Id == id);
        return objeto is null
            ? Resultado<ObjetoResponse>.NoEncontrado($"No existe el objeto con id {id}.")
            : Resultado<ObjetoResponse>.Ok(ARespuesta(objeto));
    }

    public async Task<Resultado<ObjetoResponse>> CrearAsync(ObjetoRequest solicitud)
    {
        var (errores, carga) = await ValidarAsync(solicitud, actual: null);
        if (errores.HayErrores) return Resultado<ObjetoResponse>.Invalido(errores);

        var conflicto = await ValidarPesoAsync(carga!, solicitud, excluirObjetoId: null);
        if (conflicto is not null) return Resultado<ObjetoResponse>.Conflicto(conflicto);

        var objeto = new Objeto();
        Aplicar(objeto, solicitud);
        db.Objetos.Add(objeto);
        await db.SaveChangesAsync();
        return await ObtenerAsync(objeto.Id);
    }

    public async Task<Resultado<ObjetoResponse>> ActualizarAsync(int id, ObjetoRequest solicitud)
    {
        var objeto = await db.Objetos.FirstOrDefaultAsync(o => o.Id == id);
        if (objeto is null) return Resultado<ObjetoResponse>.NoEncontrado($"No existe el objeto con id {id}.");

        var (errores, carga) = await ValidarAsync(solicitud, actual: objeto);
        if (errores.HayErrores) return Resultado<ObjetoResponse>.Invalido(errores);

        var conflicto = await ValidarPesoAsync(carga!, solicitud, excluirObjetoId: id);
        if (conflicto is not null) return Resultado<ObjetoResponse>.Conflicto(conflicto);

        Aplicar(objeto, solicitud);
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        var objeto = await db.Objetos.FirstOrDefaultAsync(o => o.Id == id);
        if (objeto is null) return Resultado<bool>.NoEncontrado($"No existe el objeto con id {id}.");

        db.Objetos.Remove(objeto);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true);
    }

    private async Task<(ErroresValidacion, Carga?)> ValidarAsync(ObjetoRequest s, Objeto? actual)
    {
        var errores = new ErroresValidacion();
        s.Nombre = ReglasValidacion.Limpiar(s.Nombre);
        s.Descripcion = ReglasValidacion.Limpiar(s.Descripcion);
        s.Observaciones = ReglasValidacion.Limpiar(s.Observaciones);

        // RN-06, RN-09: el objeto debe pertenecer a una carga existente.
        Carga? carga = null;
        if (s.CargaId is null)
            errores.Agregar("cargaId", "El campo cargaId es obligatorio.");
        else
        {
            carga = await db.Cargas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == s.CargaId);
            if (carga is null) errores.Agregar("cargaId", $"No existe una carga con id {s.CargaId}.");
        }

        ReglasValidacion.Texto(errores, "nombre", s.Nombre, obligatorio: true, ReglasValidacion.MaxNombre, minimo: 2);
        ReglasValidacion.Texto(errores, "descripcion", s.Descripcion, obligatorio: false, ReglasValidacion.MaxDescripcion);
        ReglasValidacion.Texto(errores, "observaciones", s.Observaciones, obligatorio: false, ReglasValidacion.MaxObservaciones);

        // RN-04, RN-05
        ReglasValidacion.Positivo(errores, "cantidad", s.Cantidad, obligatorio: true, ReglasValidacion.MaxCantidadObjeto);
        ReglasValidacion.Positivo(errores, "pesoUnitarioKg", s.PesoUnitarioKg, obligatorio: true, ReglasValidacion.MaxPesoTecnicoKg);
        ReglasValidacion.Positivo(errores, "volumenUnitarioM3", s.VolumenUnitarioM3, obligatorio: false, ReglasValidacion.MaxVolumenTecnicoM3);

        // RN-07: solo categorías TIPO_OBJETO.
        await referencias.ValidarCategoriaAsync(errores, "categoriaId", s.CategoriaId, TiposCategoria.TipoObjeto,
            obligatorio: false, actual?.CategoriaId);

        return (errores, carga);
    }

    /// <summary>RN-18: la suma de pesos de los objetos no puede superar el peso total declarado de la carga.</summary>
    private async Task<string?> ValidarPesoAsync(Carga carga, ObjetoRequest s, int? excluirObjetoId)
    {
        var otros = await db.Objetos.AsNoTracking()
            .Where(o => o.CargaId == carga.Id && o.Id != excluirObjetoId).ToListAsync();
        var pesoOtros = CargaService.PesoObjetos(otros);
        var pesoNuevo = s.Cantidad!.Value * s.PesoUnitarioKg!.Value;

        if (pesoOtros + pesoNuevo <= carga.PesoTotalKg) return null;
        return $"El peso de los objetos ({CargaService.Kg(pesoOtros + pesoNuevo)} kg) superaría el peso total declarado " +
               $"de la carga {carga.Codigo} ({CargaService.Kg(carga.PesoTotalKg)} kg). Peso disponible: {CargaService.Kg(carga.PesoTotalKg - pesoOtros)} kg.";
    }

    private static void Aplicar(Objeto objeto, ObjetoRequest s)
    {
        objeto.CargaId = s.CargaId!.Value;
        objeto.Nombre = s.Nombre!;
        objeto.Descripcion = s.Descripcion;
        objeto.Cantidad = s.Cantidad!.Value;
        objeto.PesoUnitarioKg = s.PesoUnitarioKg!.Value;
        objeto.VolumenUnitarioM3 = s.VolumenUnitarioM3;
        objeto.CategoriaId = s.CategoriaId;
        objeto.Observaciones = s.Observaciones;
    }

    private IQueryable<Objeto> ConsultaCompleta() =>
        db.Objetos.AsNoTracking().Include(o => o.Carga).Include(o => o.Categoria);

    public static ObjetoResponse ARespuesta(Objeto o) => new(
        o.Id, o.CargaId, o.Carga?.Codigo, o.Nombre, o.Descripcion, o.Cantidad, o.PesoUnitarioKg, o.VolumenUnitarioM3,
        o.Cantidad * o.PesoUnitarioKg, o.CategoriaId, o.Categoria?.Nombre, o.Observaciones);
}
