# 08 · Registro de incidencias y defectos

## 1. Terminología

Se usa la cadena causal de la terminología de pruebas (ISO/IEC/IEEE 29119-1 e ISTQB):

| Término | Definición aplicada en este proyecto | Ejemplo |
|---|---|---|
| **Error humano** | Acción humana equivocada (al programar, configurar, escribir pruebas u operar). | Agregar `[Produces("application/json")]` sin prever su efecto sobre las respuestas de error. |
| **Defecto de software** | Imperfección en el código del **sistema bajo prueba** causada por un error humano. | DEF-001: el atributo reemplaza el Content-Type de los errores. |
| **Fallo observado** | Desviación visible entre el resultado esperado y el obtenido durante una ejecución. | La respuesta 404 llega con `application/json` en lugar de `application/problem+json`. |
| **Incidencia registrada** | Cualquier evento que requiere investigación. Puede deberse a un defecto del sistema, a un error del testware, al entorno o a la operación. | INC-001 a INC-005. |

> Que una prueba falle **no implica** un defecto del sistema. Cada incidencia se analizó para clasificar su causa raíz.
> Solo una de las cinco incidencias resultó ser un defecto de software.

## 2. Resumen

| ID | Fecha | Clasificación | Defecto | Casos afectados | Severidad | Prioridad | Estado |
|---|---|---|---|---|---|---|---|
| INC-001 | 14/10/2026 | Error humano de operación | No | Prueba de humo (antes del ciclo X1) | Baja | Baja | Cerrada |
| INC-002 | 14/10/2026 | Error humano en el testware (xUnit) | No | 76 casos bloqueados en X1 | Media | Alta | Cerrada |
| INC-003 | 14/10/2026 | **Defecto de software** | **DEF-001** | 48 casos (1 en X1, 48 en X2) | Media | Alta | Cerrada (verificada) |
| INC-004 | 14/10/2026 | Entorno (política de Windows) | No | Ejecución xUnit completa (2 intentos) | Media | Media | Cerrada con solución temporal |
| INC-005 | 14/10/2026 | Error humano en el testware (Postman) | No | 9 casos en N1 | Baja | Alta | Cerrada |

**Defectos confirmados del sistema: 1 (DEF-001). Defectos abiertos: 0.**

## 3. Defectos de software

### DEF-001 · Las respuestas de error 400/404/409 no usan `application/problem+json`

| Campo | Valor |
|---|---|
| Identificador | DEF-001 (incidencia origen: INC-003) |
| Descripción | Las respuestas de error generadas por los controladores (400, 404 y 409) tenían `Content-Type: application/json; charset=utf-8` en lugar de `application/problem+json`. El código de estado y el cuerpo ProblemDetails eran correctos; solo el tipo de contenido incumplía el formato documentado (RF-010, RN-15). |
| Casos asociados | CP-INT-004 (ciclo X1) y 48 casos en el ciclo X2, entre ellos CP-CAT-003, CP-CAR-011, CP-PAR-007 y CP-OBJ-013. La lista completa está en el historial de [07-registro-de-ejecucion.md](07-registro-de-ejecucion.md). |
| Pasos de reproducción | 1. Ejecutar la aplicación (`dotnet run --project src/TransporteCargaPesada.Web --launch-profile http`). 2. Ejecutar `curl -i http://localhost:5080/api/cargas/999`. 3. Observar el encabezado `Content-Type`. |
| Resultado esperado | `HTTP 404` con `Content-Type: application/problem+json`. |
| Resultado observado | `HTTP 404` con `Content-Type: application/json; charset=utf-8`. En xUnit X2: 68 aserciones fallidas con `Expected: "application/problem+json" / Actual: "application/json"`. |
| Evidencia | `docs/evidencias/xunit-ejecucion-1.trx` (CP-INT-004) y `docs/evidencias/xunit-ejecucion-2.trx` (68 ejecuciones fallidas con el mismo mensaje). También la salida de curl contra la aplicación en ejecución antes de la corrección (`Content-Type: application/json; charset=utf-8`) y después (`application/problem+json; charset=utf-8`). |
| Análisis de causa raíz | `ApiControllerBase` tenía el atributo `[Produces("application/json")]`. Ese atributo es un filtro de resultado que **reemplaza** el `ContentTypes` de todo `ObjectResult`, incluidos los errores, que se habían configurado como `application/problem+json`. El error 500 no se veía afectado porque lo escribe el middleware de excepciones, fuera del pipeline MVC; por eso CP-INT-005 se aprobó en todos los ciclos. |
| Error humano que lo originó | El desarrollador agregó `[Produces]` para documentar el tipo de las respuestas exitosas sin prever su efecto sobre los errores. |
| Corrección | Se eliminó `[Produces("application/json")]` de `Controllers/ApiControllerBase.cs` y se agregó un comentario que remite a DEF-001. |
| Severidad | **Media**: no hay pérdida de datos ni respuestas incorrectas en código o cuerpo, pero se incumple el contrato de la API, y un cliente que distinga los errores por el tipo de contenido los interpretaría mal. |
| Prioridad | **Alta**: afectaba a todas las respuestas de error (más de la mitad de los casos). |
| Estado | **Cerrado**: corregido y verificado. |
| Repetición de pruebas | X3: 82/82 casos y 109/109 ejecuciones aprobadas. Regresión: N2, N3 y N4 (la prueba general de Postman exige `application/problem+json` en todo código ≥ 400: 455/455 aserciones) y X4 (109/109). |

## 4. Incidencias

### INC-001 · La aplicación no arrancó en la prueba de humo

| Campo | Valor |
|---|---|
| Descripción | Al iniciar la aplicación por primera vez se produjo `SQLite Error 1: 'no such table: Categorias'` y el proceso terminó con el código 82. |
| Caso asociado | Prueba de humo previa a los ciclos (criterio de entrada 3). |
| Pasos de reproducción | 1. `dotnet ef migrations add InicialTransporte`. 2. `dotnet run --no-build` (sin recompilar). |
| Resultado esperado | La aplicación aplica la migración, siembra los datos y responde en el puerto 5080. |
| Resultado observado | El registro mostró "No migrations were found in assembly" y luego la excepción de tabla inexistente. |
| Evidencia | Salida de la consola de la ejecución (registrada en la sesión de trabajo). |
| Análisis | **Error humano de operación**: se ejecutó con `--no-build` un binario compilado antes de crear la migración. No es un defecto del código; con el binario actualizado la migración se aplica correctamente. Como mejora se identificó que el arranque falla sin un mensaje explicativo si el binario no contiene migraciones; se deja como recomendación. |
| Severidad / Prioridad | Baja / Baja |
| Estado | Cerrada. Se eliminó el archivo de base vacío, se recompiló y la prueba de humo respondió 200, 400, 404 y 409 según lo esperado. |

### INC-002 · 103 ejecuciones xUnit bloqueadas en el ciclo X1

| Campo | Valor |
|---|---|
| Descripción | 103 de las 109 ejecuciones xUnit no llegaron a ejecutarse: `Class fixture type 'FabricaApi' may only define a single public constructor.` |
| Casos asociados | Todos los de las clases `CategoriasTests`, `ParametrosTests`, `CargasTests` y `ObjetosTests` (76 casos). |
| Pasos de reproducción | Ejecutar `dotnet test` con la versión inicial de `Infraestructura/FabricaApi.cs`, que tenía dos constructores públicos. |
| Resultado esperado | Las pruebas se ejecutan contra el sistema. |
| Resultado observado | Bloqueadas en la inicialización del fixture. |
| Evidencia | `docs/evidencias/xunit-ejecucion-1.trx` |
| Análisis | **Error humano en el testware**: xUnit exige un único constructor público en los `IClassFixture`. No hay defecto en el sistema. |
| Corrección | El constructor con parámetros pasó a ser privado y se agregó el método de fábrica `FabricaApi.SobreBaseDatos(...)`. |
| Severidad / Prioridad | Media (bloquea la ejecución) / Alta |
| Estado | Cerrada. En X2 no hubo ejecuciones bloqueadas. |

### INC-003 · Content-Type incorrecto en las respuestas de error

Esta incidencia se confirmó como defecto del sistema: ver **DEF-001**.

### INC-004 · Windows Smart App Control bloquea la DLL de pruebas

| Campo | Valor |
|---|---|
| Descripción | `dotnet test` terminó sin ejecutar pruebas: `Catastrophic failure: System.IO.FileLoadException: Could not load file or assembly '...TransporteCargaPesada.Tests.dll'. An Application Control policy has blocked this file. (0x800711C7)`. |
| Casos asociados | Todos los casos xUnit, en dos intentos: entre X2 y X3, y antes de X4. |
| Pasos de reproducción | Con Smart App Control activado, compilar el proyecto de pruebas (sin firma) y ejecutar `dotnet test`. El bloqueo depende de la reputación del binario y no es determinista: el mismo binario se cargó a las 11:13 y se bloqueó a las 11:14. |
| Resultado esperado | testhost carga la DLL de pruebas. |
| Resultado observado | Code Integrity bloqueó la carga (eventos 3033 y 3077 del registro `Microsoft-Windows-CodeIntegrity/Operational`: "did not meet the Enterprise signing level requirements"). |
| Evidencia | El registro de eventos de Windows y la salida de la consola. Los TRX vacíos de esos intentos se descartaron porque no contenían ejecuciones. |
| Análisis | **Entorno**: es una política de seguridad del sistema operativo, no un defecto del sistema ni del testware. **No se modificó la configuración de seguridad del equipo.** |
| Solución temporal | Recompilar el proyecto de pruebas con metadatos de compilación nuevos (`dotnet build tests/TransporteCargaPesada.Tests -p:SourceRevisionId=<marca-de-tiempo>`) genera un binario distinto, que Windows evaluó y permitió. Ver la sección "Solución de problemas" del README. |
| Severidad / Prioridad | Media / Media |
| Estado | Cerrada con solución temporal. Puede repetirse en equipos con Smart App Control activado. |

### INC-005 · 9 aserciones falsas en la primera ejecución de Newman

| Campo | Valor |
|---|---|
| Descripción | En N1 fallaron 9 de 455 aserciones con mensajes como `expected 20 to deeply equal '20'` y `expected [1, 2, 3, 4] to include '3'`. |
| Casos asociados | CP-CAT-001, CP-CAT-006, CP-PAR-002, CP-PAR-004, CP-CAR-001, CP-CAR-005, CP-OBJ-001, CP-OBJ-002 y CP-OBJ-005. |
| Pasos de reproducción | Ejecutar la versión inicial de la colección. Las variables de colección declaradas como `type: string` devuelven los identificadores como texto. |
| Resultado esperado | Las aserciones comparan el id numérico de la respuesta con el id guardado. |
| Resultado observado | Comparación número contra texto, por lo que la aserción falló aunque el sistema respondió correctamente. Todos los códigos HTTP coincidieron con lo esperado. |
| Evidencia | `docs/evidencias/newman-ejecucion-1.txt` y `newman-ejecucion-1.json` |
| Análisis | **Error humano en el testware** (scripts de Postman). Es un falso negativo: no hay defecto en el sistema. |
| Corrección | Las comparaciones usan `Number(pm.collectionVariables.get("...Id"))`. |
| Severidad / Prioridad | Baja / Alta (impide confiar en los resultados de la colección) |
| Estado | Cerrada. N2, N3 y N4: 455/455 aserciones aprobadas. |

## 5. Lecciones aprendidas

1. Analizar cada fallo antes de clasificarlo: 4 de las 5 incidencias no eran defectos del sistema.
2. Verificar los encabezados además del código HTTP: DEF-001 no se habría detectado comprobando solo el estado y el cuerpo.
3. Combinar herramientas: xUnit detectó DEF-001 y la verificación general de Postman lo vigila en cada solicitud desde entonces.
4. Ejecutar primero el ciclo completo después de corregir el testware, para tener una visión completa de los defectos antes de modificar el sistema (X1 → X2 → corrección → X3).
