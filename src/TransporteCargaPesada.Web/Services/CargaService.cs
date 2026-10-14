using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Data;
using TransporteCargaPesada.Web.DTOs;
using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Services;

public class CargaService(AppDbContext db, ValidadorReferencias referencias)
{
    public async Task<List<CargaResponse>> ListarAsync()
    {
        var cargas = await ConsultaCompleta().OrderBy(c => c.Id).ToListAsync();
        return cargas.Select(ARespuesta).ToList();
    }

    public async Task<Resultado<CargaResponse>> ObtenerAsync(int id)
    {
        var carga = await ConsultaCompleta().FirstOrDefaultAsync(c => c.Id == id);
        return carga is null
            ? Resultado<CargaResponse>.NoEncontrado($"No existe la carga con id {id}.")
            : Resultado<CargaResponse>.Ok(ARespuesta(carga));
    }

    public async Task<Resultado<List<ObjetoResponse>>> ListarObjetosAsync(int id)
    {
        if (!await db.Cargas.AnyAsync(c => c.Id == id))
            return Resultado<List<ObjetoResponse>>.NoEncontrado($"No existe la carga con id {id}.");

        var objetos = await db.Objetos.AsNoTracking()
            .Include(o => o.Carga).Include(o => o.Categoria)
            .Where(o => o.CargaId == id).OrderBy(o => o.Id).ToListAsync();
        return Resultado<List<ObjetoResponse>>.Ok(objetos.Select(ObjetoService.ARespuesta).ToList());
    }

    public async Task<Resultado<CargaResponse>> CrearAsync(CargaRequest solicitud)
    {
        var (errores, datos) = await ValidarAsync(solicitud, actual: null);
        if (errores.HayErrores) return Resultado<CargaResponse>.Invalido(errores);

        if (await db.Cargas.AnyAsync(c => c.Codigo == datos.Codigo))
            return Resultado<CargaResponse>.Conflicto($"Ya existe una carga con el código {datos.Codigo}.");

        var carga = new Carga { FechaRegistro = DateTime.UtcNow };
        Aplicar(carga, datos);
        db.Cargas.Add(carga);
        await db.SaveChangesAsync();
        return await ObtenerAsync(carga.Id);
    }

    public async Task<Resultado<CargaResponse>> ActualizarAsync(int id, CargaRequest solicitud)
    {
        var carga = await db.Cargas.Include(c => c.Objetos).FirstOrDefaultAsync(c => c.Id == id);
        if (carga is null) return Resultado<CargaResponse>.NoEncontrado($"No existe la carga con id {id}.");

        var (errores, datos) = await ValidarAsync(solicitud, actual: carga);
        if (errores.HayErrores) return Resultado<CargaResponse>.Invalido(errores);

        if (await db.Cargas.AnyAsync(c => c.Codigo == datos.Codigo && c.Id != id))
            return Resultado<CargaResponse>.Conflicto($"Ya existe una carga con el código {datos.Codigo}.");

        // RN-18: el peso declarado debe cubrir el peso de los objetos ya registrados.
        var pesoObjetos = PesoObjetos(carga.Objetos);
        if (datos.PesoTotalKg < pesoObjetos)
            return Resultado<CargaResponse>.Conflicto(
                $"El peso total ({Kg(datos.PesoTotalKg)} kg) es menor que el peso de los objetos registrados ({Kg(pesoObjetos)} kg).");

        Aplicar(carga, datos);
        await db.SaveChangesAsync();
        return await ObtenerAsync(id);
    }

    /// <summary>RN-13: una carga con objetos asociados no se elimina; primero deben eliminarse sus objetos.</summary>
    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        var carga = await db.Cargas.FirstOrDefaultAsync(c => c.Id == id);
        if (carga is null) return Resultado<bool>.NoEncontrado($"No existe la carga con id {id}.");

        var objetos = await db.Objetos.CountAsync(o => o.CargaId == id);
        if (objetos > 0)
            return Resultado<bool>.Conflicto(
                $"La carga {carga.Codigo} tiene {objetos} objeto(s) asociado(s) y no puede eliminarse. Elimine primero sus objetos.");

        db.Cargas.Remove(carga);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true);
    }

    private sealed record DatosCarga(
        string Codigo, string Descripcion, string Origen, string Destino, decimal PesoTotalKg, decimal? VolumenTotalM3,
        int TipoCargaId, int? TipoVehiculoId, int TipoEntregaId, int EstadoId, string? Observaciones);

    private async Task<(ErroresValidacion, DatosCarga)> ValidarAsync(CargaRequest s, Carga? actual)
    {
        var errores = new ErroresValidacion();
        var codigo = ReglasValidacion.NormalizarCodigo(s.Codigo);
        var descripcion = ReglasValidacion.Limpiar(s.Descripcion);
        var origen = ReglasValidacion.Limpiar(s.Origen);
        var destino = ReglasValidacion.Limpiar(s.Destino);
        var observaciones = ReglasValidacion.Limpiar(s.Observaciones);

        // RN-01, RN-02, RN-03
        ReglasValidacion.Codigo(errores, "codigo", codigo, ReglasValidacion.PatronCodigoCarga(), ReglasValidacion.FormatoCodigoCarga);
        ReglasValidacion.Texto(errores, "descripcion", descripcion, obligatorio: true, ReglasValidacion.MaxDescripcion, minimo: 3);
        ReglasValidacion.Texto(errores, "origen", origen, obligatorio: true, ReglasValidacion.MaxUbicacion, ReglasValidacion.MinUbicacion);
        ReglasValidacion.Texto(errores, "destino", destino, obligatorio: true, ReglasValidacion.MaxUbicacion, ReglasValidacion.MinUbicacion);
        if (origen is not null && destino is not null && string.Equals(origen, destino, StringComparison.OrdinalIgnoreCase))
            errores.Agregar("destino", "El destino no puede ser igual al origen.");
        ReglasValidacion.Texto(errores, "observaciones", observaciones, obligatorio: false, ReglasValidacion.MaxObservaciones);

        // RN-04
        ReglasValidacion.Positivo(errores, "pesoTotalKg", s.PesoTotalKg, obligatorio: true, ReglasValidacion.MaxPesoTecnicoKg);
        ReglasValidacion.Positivo(errores, "volumenTotalM3", s.VolumenTotalM3, obligatorio: false, ReglasValidacion.MaxVolumenTecnicoM3);

        // RN-06, RN-07, RN-08
        await referencias.ValidarCategoriaAsync(errores, "tipoCargaId", s.TipoCargaId, TiposCategoria.TipoCarga, true, actual?.TipoCargaId);
        var vehiculo = await referencias.ValidarCategoriaAsync(errores, "tipoVehiculoId", s.TipoVehiculoId, TiposCategoria.Vehiculo, false, actual?.TipoVehiculoId);
        await referencias.ValidarCategoriaAsync(errores, "tipoEntregaId", s.TipoEntregaId, TiposCategoria.TipoEntrega, true, actual?.TipoEntregaId);

        var estadoId = s.EstadoId;
        if (estadoId is null && actual is null)
        {
            estadoId = await db.Categorias
                .Where(c => c.TipoCategoria == TiposCategoria.EstadoCarga && c.Codigo == TiposCategoria.CodigoEstadoInicial && c.Activo)
                .Select(c => (int?)c.Id).FirstOrDefaultAsync();
            if (estadoId is null)
                errores.Agregar("estadoId",
                    $"Indique el estado: el estado inicial {TiposCategoria.CodigoEstadoInicial} no existe o está inactivo.");
        }
        else
        {
            await referencias.ValidarCategoriaAsync(errores, "estadoId", estadoId, TiposCategoria.EstadoCarga, true, actual?.EstadoId);
        }

        // RN-17: límites configurables (solo si el valor ya es válido por sí mismo).
        if (!errores.TieneError("pesoTotalKg"))
        {
            var limitePeso = await LimiteGeneralAsync(ParametrosSistema.PesoMaximoCargaKg);
            if (limitePeso is not null && s.PesoTotalKg > limitePeso)
                errores.Agregar("pesoTotalKg",
                    $"El peso total ({Kg(s.PesoTotalKg!.Value)} kg) supera el máximo configurado ({Kg(limitePeso.Value)} kg) en el parámetro {ParametrosSistema.PesoMaximoCargaKg}.");

            if (vehiculo is not null)
            {
                var capacidades = await db.Parametros.AsNoTracking()
                    .Where(p => p.CategoriaId == vehiculo.Id && p.Activo && p.Codigo.StartsWith(ParametrosSistema.PrefijoCapacidadVehiculoKg))
                    .ToListAsync();
                foreach (var capacidad in capacidades)
                {
                    var valor = TiposDato.ComoDecimal(capacidad.Valor);
                    if (valor is not null && s.PesoTotalKg > valor)
                        errores.Agregar("pesoTotalKg",
                            $"El peso total ({Kg(s.PesoTotalKg!.Value)} kg) supera la capacidad del vehículo {vehiculo.Codigo} ({Kg(valor.Value)} kg) configurada en el parámetro {capacidad.Codigo}.");
                }
            }
        }
        if (s.VolumenTotalM3 is not null && !errores.TieneError("volumenTotalM3"))
        {
            var limiteVolumen = await LimiteGeneralAsync(ParametrosSistema.VolumenMaximoCargaM3);
            if (limiteVolumen is not null && s.VolumenTotalM3 > limiteVolumen)
                errores.Agregar("volumenTotalM3",
                    $"El volumen total ({Kg(s.VolumenTotalM3.Value)} m3) supera el máximo configurado ({Kg(limiteVolumen.Value)} m3) en el parámetro {ParametrosSistema.VolumenMaximoCargaM3}.");
        }

        var datos = errores.HayErrores
            ? null!
            : new DatosCarga(codigo!, descripcion!, origen!, destino!, s.PesoTotalKg!.Value, s.VolumenTotalM3,
                s.TipoCargaId!.Value, s.TipoVehiculoId, s.TipoEntregaId!.Value, estadoId!.Value, observaciones);
        return (errores, datos);
    }

    private async Task<decimal?> LimiteGeneralAsync(string codigo)
    {
        var parametro = await db.Parametros.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigo && p.Activo && p.CategoriaId == null);
        return parametro is null ? null : TiposDato.ComoDecimal(parametro.Valor);
    }

    private static void Aplicar(Carga carga, DatosCarga d)
    {
        carga.Codigo = d.Codigo;
        carga.Descripcion = d.Descripcion;
        carga.Origen = d.Origen;
        carga.Destino = d.Destino;
        carga.PesoTotalKg = d.PesoTotalKg;
        carga.VolumenTotalM3 = d.VolumenTotalM3;
        carga.TipoCargaId = d.TipoCargaId;
        carga.TipoVehiculoId = d.TipoVehiculoId;
        carga.TipoEntregaId = d.TipoEntregaId;
        carga.EstadoId = d.EstadoId;
        carga.Observaciones = d.Observaciones;
    }

    private IQueryable<Carga> ConsultaCompleta() => db.Cargas.AsNoTracking()
        .Include(c => c.TipoCarga).Include(c => c.TipoVehiculo).Include(c => c.TipoEntrega).Include(c => c.Estado)
        .Include(c => c.Objetos);

    internal static decimal PesoObjetos(IEnumerable<Objeto> objetos) => objetos.Sum(o => o.Cantidad * o.PesoUnitarioKg);

    internal static string Kg(decimal valor) => valor.ToString("0.###", CultureInfo.InvariantCulture);

    private static CargaResponse ARespuesta(Carga c) => new(
        c.Id, c.Codigo, c.Descripcion, c.Origen, c.Destino, c.PesoTotalKg, c.VolumenTotalM3,
        c.TipoCargaId, c.TipoCarga?.Nombre,
        c.TipoVehiculoId, c.TipoVehiculo?.Nombre,
        c.TipoEntregaId, c.TipoEntrega?.Nombre,
        c.EstadoId, c.Estado?.Codigo, c.Estado?.Nombre,
        c.FechaRegistro, c.Observaciones,
        c.Objetos.Count, PesoObjetos(c.Objetos));
}
