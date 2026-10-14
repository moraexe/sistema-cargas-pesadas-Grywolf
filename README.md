# Sistema de transporte de carga pesada

Proyecto académico para la asignatura de **Pruebas de Software**. Es una API REST con interfaz web para una empresa ficticia
de transporte terrestre de carga pesada (**Griwolf Carga Pesada S.A.S.**), acompañada de un programa de pruebas inspirado en
ISO/IEC/IEEE 29119: plan, especificación, registro de ejecución, incidencias e informe.

- **Cargas**: servicios de transporte (origen → destino, peso y volumen declarados).
- **Objetos**: mercancías que forman parte de una carga.
- **Categorías**: catálogo genérico **tipado** (VEHICULO, TIPO_CARGA, TIPO_ENTREGA, ESTADO_CARGA, UNIDAD_MEDIDA, TIPO_OBJETO).
  Una categoría `VEHICULO` no puede usarse como `TIPO_CARGA`.
- **Parámetros**: configuraciones con tipo de dato (ENTERO, DECIMAL, TEXTO, BOOLEANO, FECHA). Pueden ser generales o
  pertenecer a una categoría, y algunos fijan límites que usan las reglas de negocio.

## Resultados (14/10/2026)

| | Resultado |
|---|---|
| Casos diseñados | 90 (29 funcionales, 47 negativos, 6 de integración, 8 de interfaz) |
| Regresión final | 90/90 ejecutados y 90/90 aprobados |
| xUnit | 109/109 ejecuciones aprobadas |
| Postman/Newman | 103 solicitudes y 455/455 aserciones aprobadas |
| Interfaz (Edge headless) | 8/8 aprobados |
| Defectos encontrados | 1 (DEF-001, severidad media), corregido y verificado |

Detalle en [docs/09-informe-de-resultados.md](docs/09-informe-de-resultados.md).

## Tecnologías

C# · ASP.NET Core Web API + Razor Pages (.NET 10 LTS) · Entity Framework Core 10 · SQLite · xUnit · Postman/Newman · Bootstrap 5.

## Estructura

```text
├── TransporteCargaPesada.sln
├── dotnet-tools.json                     # herramienta local dotnet-ef (solo para crear migraciones nuevas)
├── src/TransporteCargaPesada.Web/
│   ├── Controllers/                      # API REST /api/... (traduce resultados de servicio a HTTP)
│   ├── Services/                         # Reglas de negocio centralizadas
│   │   └── Reglas/                       # Tipos de categoría, tipos de dato, parámetros del sistema, validaciones
│   ├── Models/                           # Entidades EF Core
│   ├── DTOs/                             # Contratos de entrada y salida
│   ├── Data/                             # DbContext, migraciones, siembra de datos de ejemplo
│   ├── Infrastructure/                   # Formato de errores (ProblemDetails) y manejador de excepciones
│   ├── Pages/ y wwwroot/js/              # Interfaz web (consume la API con fetch)
│   └── App_Data/transporte.db            # Base SQLite de la aplicación (se crea al arrancar)
├── tests/TransporteCargaPesada.Tests/    # Pruebas xUnit (funcionales, negativas, de integración)
├── postman/                              # Colección Postman v2.1 y entorno
└── docs/                                 # Documentación 01–09 y evidencias de ejecución
```

## Requisitos

- **.NET SDK 10.0** (probado con 10.0.400).
- Opcional: **Visual Studio 2026** (o posterior) con la carga de trabajo "Desarrollo de ASP.NET y web". Visual Studio 2022 no admite .NET 10.
- Opcional: **Postman** (aplicación) o **Node.js 18+** para ejecutar la colección con Newman.

## Ejecutar la aplicación

### Desde Visual Studio

1. Abra `TransporteCargaPesada.sln`.
2. Elija `TransporteCargaPesada.Web` como proyecto de inicio y el perfil **http**.
3. Pulse F5. Se abre el navegador en `http://localhost:5080`.

> En el entorno donde se construyó el proyecto no había Visual Studio instalado. La ejecución desde Visual Studio no se verificó
> y queda como pendiente (ver el informe, sección 18).

### Desde la línea de comandos

```bash
dotnet restore
dotnet run --project src/TransporteCargaPesada.Web --launch-profile http
```

- Interfaz web: <http://localhost:5080>
- API: <http://localhost:5080/api/cargas>, `/api/objetos`, `/api/categorias`, `/api/parametros`

**Puerto:** 5080 (HTTP). Se configura en `src/TransporteCargaPesada.Web/Properties/launchSettings.json`. El perfil `https`
usa además `https://localhost:7080`.

### Base de datos

- Archivo: `src/TransporteCargaPesada.Web/App_Data/transporte.db` (cadena `DefaultConnection` en `appsettings.json`).
- Al arrancar se aplican las migraciones. Los **datos de ejemplo** (18 categorías, 8 parámetros, 2 cargas y 3 objetos) se insertan
  **solo si la base está vacía**: los datos persisten entre reinicios.
- Para empezar de cero: detenga la aplicación y elimine `App_Data/transporte.db*`.
- Para usar otra base sin tocar la de la aplicación:
  `ConnectionStrings__DefaultConnection="Data Source=App_Data/otra.db"` como variable de entorno.

## Pruebas automatizadas (xUnit)

```bash
dotnet test
```

- Cada clase de prueba levanta la aplicación en memoria (`WebApplicationFactory`) con **su propia base SQLite temporal** en
  `%TEMP%\TransporteCargaPesada.Tests\`. **Nunca** usa `App_Data/transporte.db` (lo verifica CP-INT-006).
- El nombre de cada prueba empieza con el ID del caso de [docs/06](docs/06-especificacion-de-pruebas.md), por ejemplo
  `CP-CAR-011 Categoría VEHICULO usada como TIPO_CARGA devuelve 400`.
- Reporte TRX: `dotnet test --logger "trx;LogFileName=resultado.trx" --results-directory docs/evidencias`

## Pruebas de API con Postman

Archivos:

- `postman/TransporteCargaPesada.postman_collection.json` (colección v2.1, 103 solicitudes)
- `postman/TransporteCargaPesada.postman_environment.json` (entorno con `baseUrl = http://localhost:5080`)

### Importar y ejecutar en Postman

1. Inicie la aplicación (ver arriba).
2. En Postman: **Import** → seleccione los dos archivos de la carpeta `postman/`.
3. Elija el entorno **Transporte Carga Pesada · Local** en la esquina superior derecha.
4. Abra la colección → **Run** (Collection Runner) → mantenga el orden de las carpetas → **Run**.

La colección:

- Está organizada en carpetas ordenadas (`00 · Preparación` … `06 · Eliminación y limpieza`) y debe ejecutarse **completa y en orden**,
  porque las solicitudes reutilizan los identificadores guardados en variables por las anteriores.
- No depende de identificadores fijos: crea sus propias categorías, parámetros, cargas y objetos con un sufijo único por ejecución
  y **elimina solo lo que creó**. Puede ejecutarse varias veces seguidas.
- Cada solicitud indica el ID del caso, su descripción y el resultado esperado, y lleva scripts que validan el código HTTP, la estructura
  y los tipos del JSON, los ids generados y los mensajes de error. Un script de la colección verifica además en **todas** las respuestas
  de error el formato ProblemDetails y que no se expongan detalles internos.

### Ejecutar con Newman (línea de comandos)

```bash
npx newman run postman/TransporteCargaPesada.postman_collection.json \
  -e postman/TransporteCargaPesada.postman_environment.json
```

## Documentación

| Documento | Contenido |
|---|---|
| [01-modelo-de-negocio.md](docs/01-modelo-de-negocio.md) | Negocio, actores, procesos, entidades, catálogos, parámetros, supuestos y exclusiones |
| [02-requisitos-y-reglas.md](docs/02-requisitos-y-reglas.md) | RF, RNF y reglas RN-01 a RN-19 con criterios de aceptación y decisiones |
| [03-modelo-de-datos.md](docs/03-modelo-de-datos.md) | Diagrama ER (Mermaid), claves, relaciones, índices y restricciones |
| [04-catalogo-de-endpoints.md](docs/04-catalogo-de-endpoints.md) | Endpoints, cuerpos, respuestas y formato de errores |
| [05-plan-de-pruebas.md](docs/05-plan-de-pruebas.md) | Objetivos, alcance, estrategia, riesgos y criterios |
| [06-especificacion-de-pruebas.md](docs/06-especificacion-de-pruebas.md) | 90 casos con trazabilidad |
| [07-registro-de-ejecucion.md](docs/07-registro-de-ejecucion.md) | Resultados reales por caso y por ciclo |
| [08-incidencias-y-defectos.md](docs/08-incidencias-y-defectos.md) | 5 incidencias clasificadas y 1 defecto (DEF-001) |
| [09-informe-de-resultados.md](docs/09-informe-de-resultados.md) | Métricas, conclusiones, limitaciones y recomendaciones |
| [docs/evidencias/](docs/evidencias/) | TRX de xUnit, reportes de Newman (txt, json, xml) y capturas de la interfaz |

## Formato de errores de la API

Todos los errores usan ProblemDetails (`application/problem+json`):

```json
{
  "title": "Uno o más datos no son válidos.",
  "status": 400,
  "errors": { "tipoCargaId": ["La categoría 1 (TRACTOCAMION) es de tipo VEHICULO y no puede usarse como TIPO_CARGA."] },
  "traceId": "00-..."
}
```

400 = datos inválidos · 404 = recurso inexistente · 409 = duplicado o conflicto de integridad · 500 = error inesperado
(sin detalles internos).

## Solución de problemas

| Problema | Solución |
|---|---|
| `An Application Control policy has blocked this file` al ejecutar `dotnet test` | Windows **Smart App Control** bloqueó la DLL de pruebas, que no está firmada (incidencia INC-004). En este entorno funcionó recompilarla con metadatos nuevos: `dotnet build tests/TransporteCargaPesada.Tests -p:SourceRevisionId=<marca-de-tiempo>` y luego `dotnet test --no-build`. También puede ejecutar las pruebas en un equipo sin esa política. |
| El puerto 5080 está ocupado | Cambie `applicationUrl` en `launchSettings.json` y el valor de `baseUrl` en el entorno de Postman. |
| `no such table` al arrancar | Recompile antes de ejecutar (no use `--no-build` con un binario anterior a la migración) y elimine `App_Data/transporte.db*` si quedó vacío. |
| Crear una migración nueva | `dotnet tool restore` y luego `dotnet ef migrations add <Nombre> --project src/TransporteCargaPesada.Web --output-dir Data/Migrations` |

## Alcance y aclaraciones

- Los límites de peso y volumen (por ejemplo `PESO_MAXIMO_CARGA_KG = 40000`) son **reglas académicas configurables**, no límites legales.
- Sin autenticación, GPS, facturación, pagos ni integraciones externas (exclusiones del alcance).
- La referencia a ISO/IEC/IEEE 29119 se limita a la estructura y la terminología de la documentación de pruebas; no implica
  conformidad total ni certificación.
