# Revisión de base de datos · SQLite y relaciones

**Fecha:** 14/10/2026
**Rama:** `Modelo-de-negocio-y-datos`
**Alcance:** modelo de datos EF Core (SQLite), migración `InicialTransporte`, snapshot, inicialización y siembra
(`AppDbContext`, `DbSeeder`, `InicializacionBaseDatos`), y verificación de integridad referencial.
**Base:** commit `d577320` (repo interno) · contrastado con `docs/03-modelo-de-datos.md` y con los tests `CP-INT-001`…`CP-INT-006`.

---

## 1. Resumen

| | Resultado |
|---|---|
| Modelo ↔ migración ↔ snapshot | **Sincronizados** (sin *model drift*) |
| Relaciones (FK) | **7 FK, todas `Restrict`** — sin borrados en cascada accidentales |
| Restricciones CHECK | **7** como segunda línea de defensa |
| Índices únicos | 3 (RN-01, RN-11, unicidad de parámetro) |
| Tests de integración ejecutados | **6/6 superados** (`dotnet test`, filtro `IntegracionTests`) |
| Defectos funcionales | 0 |
| Observaciones de adherencia/estilo | 5 |

La integridad del esquema es sólida: la base de datos protege los datos **aunque se omita la API** (RNF-003). Lo verifiqué
ejecutando los tests, no solo leyendo el código.

---

## 2. Verificación ejecutada (evidencia)

Comando: `dotnet test --filter "FullyQualifiedName~IntegracionTests"`

```
Correctas! - Con error: 0, Superado: 6, Omitido: 0, Total: 6, Duración: 10 s
```

| Caso | Qué comprueba | Resultado |
|---|---|---|
| CP-INT-001 | Los datos persisten tras reiniciar; la siembra no se repite ni borra | OK |
| CP-INT-002 | SQLite impide borrar una categoría referenciada (FK `Restrict`); insertar objeto a carga borrada falla | OK |
| CP-INT-003 | CHECK rechaza peso negativo, cantidad 0 y tipo de categoría inválido (`SqliteException`) | OK |
| CP-INT-004 | Flujo categoría → carga → objetos → borrado ordenado respetando dependencias | OK |
| CP-INT-005 | Error 500 en `application/problem+json` sin filtrar excepción ni cadena de conexión | OK |
| CP-INT-006 | Las pruebas usan una BD SQLite temporal en `%TEMP%`, distinta de `transporte.db` | OK |

---

## 3. Modelo de datos y sincronización

`AppDbContext.OnModelCreating` declara longitudes, índices únicos, CHECK y FK. La migración
`20261014160045_InicialTransporte` y el snapshot `AppDbContextModelSnapshot` reproducen **exactamente** ese modelo:

- **CHECK `Categorias.TipoCategoria`** y **`Parametros.TipoDato`** se generan desde `TiposCategoria.Todos` / `TiposDato.Todos`
  (fuente única de verdad: añadir un tipo nuevo al catálogo se refleja solo en el CHECK).
- **`decimal` → TEXT con `CAST(... AS REAL)`** en los CHECK: correcto, porque SQLite no tiene `decimal` nativo.
- **Fechas en UTC**: `FechaUtcConverter` normaliza el `Kind` al guardar/leer (SQLite no conserva el offset).
- **Índices únicos**: `IX_Cargas_Codigo` (RN-01), `IX_Categorias_TipoCategoria_Codigo` (RN-11, compuesto),
  `IX_Parametros_Codigo` (unicidad global, D-05).

---

## 4. Relaciones entre entidades

Las 7 claves foráneas de la migración usan `ReferentialAction.Restrict` (nunca `Cascade`), de modo que **ninguna eliminación
deja referencias huérfanas** (RN-12) y **una carga con objetos no se borra por accidente** (RN-13):

| Relación | Columna | Cardinalidad | Delete |
|---|---|---|---|
| Carga → Categoría (`TipoCarga`) | TipoCargaId | N:1 | Restrict |
| Carga → Categoría (`TipoVehiculo`) | TipoVehiculoId (nullable) | N:0..1 | Restrict |
| Carga → Categoría (`TipoEntrega`) | TipoEntregaId | N:1 | Restrict |
| Carga → Categoría (`Estado`) | EstadoId | N:1 | Restrict |
| Objeto → Carga | CargaId | N:1 | Restrict |
| Objeto → Categoría (`TIPO_OBJETO`) | CategoriaId (nullable) | N:0..1 | Restrict |
| Parámetro → Categoría | CategoriaId (nullable) | N:0..1 | Restrict |

Decisiones de modelado acertadas y documentadas en `docs/03`:

- **Coherencia de tipo fuera de la BD (RN-07):** la FK solo garantiza que la categoría exista; que sea del tipo correcto se
  valida en `ValidadorReferencias`, porque SQLite no permite claves foráneas condicionadas a otra columna. Correcto.
- **Sin FK Parámetro ↔ Carga/Objeto:** las reglas consultan parámetros por código; una FK acoplaría configuración y datos. Correcto.
- **Defensa en capas:** la API impide borrar cargas con objetos (RN-13) y aun así CP-INT-002 confirma que SQLite lo bloquea por sí sola.


---

## 5. Inicialización, cadena de conexión y siembra

- **`InicializacionBaseDatos.ResolverCadenaConexion`:** lee `DefaultConnection`, y si la ruta del archivo es relativa la resuelve
  contra `ContentRoot`, creando la carpeta. Así funciona igual desde Visual Studio y desde la CLI. Correcto.
- **`AddDbContext` con resolución diferida:** la cadena se resuelve dentro del lambda, de modo que los tests pueden inyectar
  `ConnectionStrings:DefaultConnection` propia (ver `FabricaApi`). Habilita RNF-002 (BD de pruebas aislada).
- **`InicializarBaseDatos`:** `Database.Migrate()` + `DbSeeder.Sembrar(db)` al arrancar. La siembra inserta **solo si
  `Categorias` está vacía** y nunca borra nada, por lo que los datos persisten entre reinicios (CP-INT-001). Correcto.
- **`FabricaApi`:** cada clase usa un archivo SQLite propio en `%TEMP%\TransporteCargaPesada.Tests\`, con `ClearAllPools()` y
  borrado del `.db`/`-wal`/`-shm` al final (CP-INT-006). Correcto y limpio.

---

## 6. Observaciones (adherencia a `.clinerules` y buenas prácticas de BD)

> Ninguna corrige un fallo actual; son mejoras de robustez y adherencia, análogas a las del documento `REVISION-CRITERIO.md`.

### OBS-SQL-01 · `decimal` como texto: precisión dependiente de `decimal` de .NET  *(buena práctica)*
SQLite guarda los `decimal` como `TEXT` y EF los convierte. La comparación en CHECK usa `CAST(... AS REAL)` (coma flotante de 64
bits). Para los pesos académicos del dominio la precisión es suficiente, pero conviene tener presente que un valor con muchos
decimales podría compararse con precisión de doble en el CHECK. No es un defecto hoy; se documenta como límite conocido.

### OBS-SQL-02 · La unicidad de parámetro es global, no distingue ámbito  *(decisión, D-05)*
`IX_Parametros_Codigo` es único y simple: dos parámetros no pueden compartir `Codigo` aunque uno sea `GENERAL` y otro
`CATEGORIA`. Es lo que pide D-05, pero si en el futuro se quisieran parámetros homónimos por categoría haría falta un índice
único compuesto `(CategoriaId, Codigo)` con manejo de nulos. Solo una nota prospectiva.

### OBS-SQL-03 · Sin índices para consultas por fecha o estado  *(rendimiento, opcional)*
Los índices cubren unicidad, FK y filtros por categoría. No hay índices sobre `FechaRegistro` ni sobre campos que una futura
búsqueda por rango de fechas pudiera usar. Con el volumen académico actual es irrelevante; se menciona solo como referencia de
escalabilidad.

### OBS-SQL-04 · `PRAGMA foreign_keys` no se establece explícitamente  *(robustez)*
La integridad FK funciona porque EF Core habilita `foreign_keys=ON` por defecto al abrir la conexión SQLite (lo confirman
CP-INT-002 y los tests). No se declara de forma explícita en `AppDbContext`; si alguien reutilizara la cadena en otra herramienta
(por ejemplo un script ADO.NET suelto), SQLite tiene las FK **desactivadas** por defecto. Añadir `PRAGMA foreign_keys=ON` al
construir el `SqliteConnection` haría la garantía explícita e independiente de EF. Mejora de robustez, no un fallo actual.

### OBS-SQL-05 · `Migrate()` en cada arranque también corre en producción  *(operación)*
`InicializarBaseDatos` aplica migraciones automáticamente en todos los entornos. Es cómodo para el proyecto académico, pero en un
desproductivo lo habitual es aplicar migraciones como paso de despliegue (`dotnet ef database update`) y no al arrancar la app.
Válido para el alcance actual; se deja anotado para una eventual puesta en producción.


---

## 7. Recomendación

El modelo de datos es correcto y está verificado: modelo ↔ migración ↔ snapshot sincronizados, 7 FK `Restrict`, CHECK como
segunda línea de defensa e **6/6 tests de integración superados**. Para la entrega académica actual **no requiere cambios**.

Si se decide robustecer, el orden sugerido es: **OBS-SQL-04** (declarar `PRAGMA foreign_keys=ON` explícito, el de mayor valor
técnico) → **OBS-SQL-01/02** como notas de diseño → **OBS-SQL-03/05** solo si el proyecto crece o pasa a producción. Cualquier
cambio debe reverificarse con `dotnet build` y los tests `CP-INT-*`.

- **Índices no únicos** sobre todas las FK: sostienen `ContarUsosAsync` (RN-14) y los filtros `?categoriaId=`.
