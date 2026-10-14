using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Data;

/// <summary>
/// Inserta los datos de ejemplo una sola vez: solo si la tabla Categorias está vacía.
/// Nunca elimina ni sobrescribe datos existentes, así que la información persiste entre reinicios.
/// </summary>
public static class DbSeeder
{
    public static void Sembrar(AppDbContext db)
    {
        if (db.Categorias.Any()) return;

        var ahora = DateTime.UtcNow;
        Categoria Cat(string tipo, string codigo, string nombre, string descripcion) =>
            new() { TipoCategoria = tipo, Codigo = codigo, Nombre = nombre, Descripcion = descripcion, Activo = true, FechaCreacion = ahora };

        var tractocamion = Cat(TiposCategoria.Vehiculo, "TRACTOCAMION", "Tractocamión", "Unidad tractora con semirremolque.");
        var camionRigido = Cat(TiposCategoria.Vehiculo, "CAMION_RIGIDO", "Camión rígido", "Camión de chasis rígido de dos o tres ejes.");
        var camaBaja = Cat(TiposCategoria.Vehiculo, "CAMA_BAJA", "Cama baja", "Semirremolque de plataforma baja para maquinaria.");
        var general = Cat(TiposCategoria.TipoCarga, "GENERAL", "Carga general", "Mercancía empacada o paletizada.");
        var refrigerada = Cat(TiposCategoria.TipoCarga, "REFRIGERADA", "Carga refrigerada", "Mercancía que requiere cadena de frío.");
        var sobredimensionada = Cat(TiposCategoria.TipoCarga, "SOBREDIMENSIONADA", "Carga sobredimensionada", "Carga que excede las dimensiones estándar.");
        var directa = Cat(TiposCategoria.TipoEntrega, "DIRECTA", "Entrega directa", "Entrega sin paradas intermedias.");
        var programada = Cat(TiposCategoria.TipoEntrega, "PROGRAMADA", "Entrega programada", "Entrega en fecha y franja acordadas.");
        var pendiente = Cat(TiposCategoria.EstadoCarga, TiposCategoria.CodigoEstadoInicial, "Pendiente", "Registrada, aún no despachada.");
        var enTransito = Cat(TiposCategoria.EstadoCarga, "EN_TRANSITO", "En tránsito", "Despachada y en ruta.");
        var entregada = Cat(TiposCategoria.EstadoCarga, "ENTREGADA", "Entregada", "Recibida por el destinatario.");
        var kilogramo = Cat(TiposCategoria.UnidadMedida, "KILOGRAMO", "Kilogramo", "Unidad de masa (kg).");
        var tonelada = Cat(TiposCategoria.UnidadMedida, "TONELADA", "Tonelada", "1 000 kg.");
        var metroCubico = Cat(TiposCategoria.UnidadMedida, "METRO_CUBICO", "Metro cúbico", "Unidad de volumen (m³).");
        var maquinaria = Cat(TiposCategoria.TipoObjeto, "MAQUINARIA", "Maquinaria", "Maquinaria pesada o industrial.");
        var repuestos = Cat(TiposCategoria.TipoObjeto, "REPUESTOS", "Repuestos", "Piezas y repuestos industriales.");
        var equipoElectrico = Cat(TiposCategoria.TipoObjeto, "EQUIPO_ELECTRICO", "Equipo eléctrico", "Transformadores, motores y tableros.");
        var materialConstruccion = Cat(TiposCategoria.TipoObjeto, "MATERIAL_CONSTRUCCION", "Material de construcción", "Acero, cemento, prefabricados.");

        db.Categorias.AddRange(tractocamion, camionRigido, camaBaja, general, refrigerada, sobredimensionada,
            directa, programada, pendiente, enTransito, entregada, kilogramo, tonelada, metroCubico,
            maquinaria, repuestos, equipoElectrico, materialConstruccion);
        db.SaveChanges();

        Parametro Par(string codigo, string nombre, string descripcion, string tipoDato, string valor,
            string? unidad, Categoria? categoria, bool obligatorio) =>
            new()
            {
                Codigo = codigo, Nombre = nombre, Descripcion = descripcion, TipoDato = tipoDato, Valor = valor,
                UnidadMedida = unidad, CategoriaId = categoria?.Id, EsObligatorio = obligatorio, Activo = true,
                FechaActualizacion = ahora,
            };

        db.Parametros.AddRange(
            Par(ParametrosSistema.PesoMaximoCargaKg, "Peso máximo por carga",
                "Regla académica configurable: peso total máximo de una carga.", TiposDato.Decimal, "40000", "kg", null, true),
            Par(ParametrosSistema.VolumenMaximoCargaM3, "Volumen máximo por carga",
                "Regla académica configurable: volumen total máximo de una carga.", TiposDato.Decimal, "90", "m3", null, true),
            Par("UNIDAD_PESO_PREDETERMINADA", "Unidad de peso predeterminada",
                "Unidad en que se expresan los pesos del sistema (informativo).", TiposDato.Texto, "KILOGRAMO", null, null, true),
            Par("CAPACIDAD_KG_TRACTOCAMION", "Capacidad del tractocamión",
                "Peso máximo de una carga asignada a un tractocamión (valor académico).", TiposDato.Decimal, "34000", "kg", tractocamion, false),
            Par("CAPACIDAD_KG_CAMION_RIGIDO", "Capacidad del camión rígido",
                "Peso máximo de una carga asignada a un camión rígido (valor académico).", TiposDato.Decimal, "17000", "kg", camionRigido, false),
            Par("TEMPERATURA_MAXIMA_C", "Temperatura máxima de carga refrigerada",
                "Temperatura máxima de conservación (informativo).", TiposDato.Decimal, "4", "°C", refrigerada, false),
            Par("REQUIERE_ESCOLTA", "Requiere vehículo escolta",
                "Indica si la carga sobredimensionada requiere escolta (informativo).", TiposDato.Booleano, "true", null, sobredimensionada, false),
            Par("MAX_OBJETOS_SUGERIDOS", "Objetos sugeridos por carga",
                "Cantidad de líneas de objetos recomendada por carga (informativo).", TiposDato.Entero, "50", null, null, false));

        var carga1 = new Carga
        {
            Codigo = "CP-2026-0001",
            Descripcion = "Traslado de maquinaria industrial para planta de producción",
            Origen = "Bogotá",
            Destino = "Medellín",
            PesoTotalKg = 28000m,
            VolumenTotalM3 = 60m,
            TipoCargaId = sobredimensionada.Id,
            TipoVehiculoId = camaBaja.Id,
            TipoEntregaId = programada.Id,
            EstadoId = pendiente.Id,
            FechaRegistro = ahora,
            Observaciones = "Dato de ejemplo.",
            Objetos =
            [
                new Objeto { Nombre = "Excavadora CAT 320", Descripcion = "Excavadora hidráulica", Cantidad = 1, PesoUnitarioKg = 21000m, VolumenUnitarioM3 = 45m, CategoriaId = maquinaria.Id },
                new Objeto { Nombre = "Motor diésel", Descripcion = "Motor de repuesto", Cantidad = 2, PesoUnitarioKg = 1500m, VolumenUnitarioM3 = 3m, CategoriaId = repuestos.Id },
            ],
        };
        var carga2 = new Carga
        {
            Codigo = "CP-2026-0002",
            Descripcion = "Transformador eléctrico para subestación",
            Origen = "Barranquilla",
            Destino = "Bucaramanga",
            PesoTotalKg = 15000m,
            TipoCargaId = general.Id,
            TipoVehiculoId = tractocamion.Id,
            TipoEntregaId = directa.Id,
            EstadoId = enTransito.Id,
            FechaRegistro = ahora,
            Observaciones = "Dato de ejemplo.",
            Objetos =
            [
                new Objeto { Nombre = "Transformador 500 kVA", Cantidad = 1, PesoUnitarioKg = 12000m, VolumenUnitarioM3 = 18m, CategoriaId = equipoElectrico.Id },
            ],
        };
        db.Cargas.AddRange(carga1, carga2);
        db.SaveChanges();
    }
}
