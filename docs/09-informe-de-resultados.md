# 09 · Informe de resultados de pruebas

| Dato | Valor |
|---|---|
| Identificador | IR-TCP-001 |
| Plan asociado | PP-TCP-001 ([05-plan-de-pruebas.md](05-plan-de-pruebas.md)) |
| Período de ejecución | 14/10/2026, de 11:12 a 11:31 (hora de Colombia) |
| Sistema | Transporte de carga pesada (Griwolf Carga Pesada S.A.S., empresa ficticia): API REST + interfaz web + SQLite |
| Fuente de las cifras | [07-registro-de-ejecucion.md](07-registro-de-ejecucion.md), generado a partir de los TRX, JSON y capturas de `docs/evidencias` |

## 1. Resumen ejecutivo

Se diseñaron **90 casos de prueba** (29 funcionales, 47 negativos, 6 de integración y 8 de interfaz), trazados a 10 requisitos
funcionales, 6 no funcionales y 19 reglas de negocio. Se ejecutaron en **10 ciclos** con tres herramientas complementarias:
xUnit (109 ejecuciones por ciclo), Postman/Newman (103 solicitudes y 455 aserciones por ciclo) y la interfaz web automatizada.

En la **regresión final** (ciclos X4, N4 y U2) se ejecutaron **90 de 90 casos (100 %)** y se aprobaron **90 de 90 (100 %)**.

Durante las pruebas se registraron **5 incidencias**. Solo una correspondía a un **defecto real del sistema (DEF-001)**: las
respuestas de error se enviaban con `application/json` en lugar de `application/problem+json`. Se corrigió y se verificó. Las otras
cuatro incidencias se debieron a dos errores del testware, un error de operación y una política del entorno (Windows Smart App Control).
**No quedan defectos abiertos.**

La regla de negocio destacada del proyecto, que un registro `VEHICULO` no se puede usar como `TIPO_CARGA` (CP-CAR-011), se verificó
en xUnit, en Postman y desde la interfaz (CP-UI-003).

## 2. Objetivos y alcance ejecutado

| Objetivo del plan | Resultado |
|---|---|
| 1. CRUD de las cuatro entidades (RF-001 a RF-007) | Cumplido: todos los casos aprobados. |
| 2. Reglas RN-01 a RN-19 con los códigos HTTP documentados | Cumplido: cada regla tiene al menos un caso aprobado (matriz en 06). |
| 3. Pruebas negativas (incluido VEHICULO como TIPO_CARGA) | Cumplido: 47 casos negativos aprobados. |
| 4. Integración API ↔ EF Core ↔ SQLite | Cumplido: CP-INT-001 a CP-INT-006 aprobados. |
| 5. Interfaz web que consume la API | Cumplido: CP-UI-001 a CP-UI-008 aprobados, con capturas. |
| 6. Registro y seguimiento de defectos | Cumplido: 1 defecto detectado, corregido y verificado. |

Fuera del alcance ejecutado: ver la sección 18 (Limitaciones).

## 3. Entorno

| Elemento | Valor |
|---|---|
| Sistema operativo | Windows 11 Pro 10.0.26300 (Smart App Control activado) |
| Aplicación | `http://localhost:5080`, entorno Development, perfil `http` |
| Base de datos de la aplicación | `App_Data/transporte.db` (no se usó en las pruebas automatizadas) |
| Base de datos de xUnit | Archivo SQLite temporal por clase de prueba en `%TEMP%` |
| Base de datos de Newman y UI | `App_Data/newman-evidencia.db`, creada vacía para la evidencia y eliminada al terminar |

## 4. Herramientas y versiones

| Herramienta | Versión |
|---|---|
| .NET SDK / ASP.NET Core runtime | 10.0.400 / 10.0.11 |
| Entity Framework Core, Microsoft.Data.Sqlite | 10.0.13 |
| SQLitePCLRaw (motor SQLite empaquetado) | 2.1.13 |
| xUnit / xunit.runner.visualstudio / Microsoft.NET.Test.Sdk | 2.9.3 / 3.1.4 / 17.14.1 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.13 |
| Newman / Node.js | 6.2.3 / 24.20.0 |
| Postman Collection | Formato v2.1 |
| Microsoft Edge (headless, CDP) | 155.0.4283.45 |
| Bootstrap (interfaz) | 5.3.3 |

## 5. Casos diseñados

| Grupo | Casos | Funcionales | Negativos | Integración | Interfaz |
|---|---|---|---|---|---|
| Categorías | 18 | 10 | 8 | — | — |
| Parámetros | 17 | 7 | 10 | — | — |
| Cargas | 25 | 6 | 19 | — | — |
| Objetos | 16 | 6 | 10 | — | — |
| Integración | 6 | — | — | 6 | — |
| Interfaz web | 8 | — | — | — | 8 |
| **Total** | **90** | **29** | **47** | **6** | **8** |

Cobertura por herramienta: 82 casos en xUnit, 77 en Postman/Newman (todos también cubiertos por xUnit, lo que da doble verificación
por HTTP real) y 8 en la interfaz.

## 6 a 10. Casos ejecutados, aprobados, fallidos, bloqueados y no ejecutados

### Regresión final (versión entregada)

| Indicador | Valor |
|---|---|
| Casos diseñados | 90 |
| Casos ejecutados | 90 |
| Casos aprobados | 90 |
| Casos fallidos | 0 |
| Casos bloqueados | 0 |
| Casos no ejecutados | 0 |

### Historial por ciclo (conteo por caso)

| Ciclo | Herramienta | Aplicables | Aprobados | Fallidos | Bloqueados | % ejecución | % aprobación |
|---|---|---|---|---|---|---|---|
| X1 | xUnit | 82 | 5 | 1 | 76 | 7,3 % | 83,3 % |
| X2 | xUnit | 82 | 34 | 48 | 0 | 100 % | 41,5 % |
| X3 | xUnit | 82 | 82 | 0 | 0 | 100 % | 100 % |
| N1 | Newman | 77 | 68 | 9 | 0 | 100 % | 88,3 % |
| N2 | Newman | 77 | 77 | 0 | 0 | 100 % | 100 % |
| N3 | Newman | 77 | 77 | 0 | 0 | 100 % | 100 % |
| U1 | UI | 8 | 8 | 0 | 0 | 100 % | 100 % |
| X4 | xUnit | 82 | 82 | 0 | 0 | 100 % | 100 % |
| N4 | Newman | 77 | 77 | 0 | 0 | 100 % | 100 % |
| U2 | UI | 8 | 8 | 0 | 0 | 100 % | 100 % |

Lectura del historial:

- **X1**: los 76 casos bloqueados no se ejecutaron contra el sistema (INC-002, testware). Cuentan como no ejecutados en el porcentaje de
  ejecución (6/82) y no entran en el de aprobación (5/6).
- **X2**: es la primera visión completa del sistema. Los 48 fallos tienen una sola causa (DEF-001).
- **N1**: los 9 fallos son falsos negativos del testware (INC-005); los 103 códigos HTTP coincidieron con lo esperado.
- **N3**: repite N2 sobre la misma base y demuestra que la colección es reproducible (no depende de identificadores fijos y limpia sus datos).

## 11 y 12. Porcentajes de ejecución y aprobación

Fórmulas del plan:

- Porcentaje de ejecución = casos ejecutados / casos diseñados × 100 = 90 / 90 × 100 = **100 %**
- Porcentaje de aprobación = casos aprobados / casos ejecutados × 100 = 90 / 90 × 100 = **100 %**

Tratamiento de los bloqueados: un caso bloqueado no se considera ejecutado, así que reduce el porcentaje de ejecución y no entra en el
denominador del de aprobación. Si en un ciclo no se ejecuta ningún caso, el porcentaje de aprobación se informa como "N/A" y no se
divide por cero. Así se trataron los dos intentos anulados por INC-004, que no se cuentan como ciclos.

## 13. Incidencias

| ID | Clasificación | Defecto | Estado |
|---|---|---|---|
| INC-001 | Error humano de operación (`--no-build`) | No | Cerrada |
| INC-002 | Error humano en el testware (fixture xUnit) | No | Cerrada |
| INC-003 | Defecto de software | DEF-001 | Cerrada y verificada |
| INC-004 | Entorno (Smart App Control) | No | Cerrada con solución temporal |
| INC-005 | Error humano en el testware (variables de Postman) | No | Cerrada |

Detalle completo en [08-incidencias-y-defectos.md](08-incidencias-y-defectos.md).

## 14. Defectos confirmados

| ID | Descripción | Severidad | Prioridad | Detectado en | Corregido | Verificado en |
|---|---|---|---|---|---|---|
| DEF-001 | Las respuestas 400/404/409 llevaban `Content-Type: application/json` en lugar de `application/problem+json` (causa: `[Produces]` en `ApiControllerBase`) | Media | Alta | X1 (CP-INT-004) y X2 (48 casos) | Sí | X3, X4, N2, N3, N4 |

## 15. Clasificación por severidad

| Severidad | Defectos detectados | Abiertos |
|---|---|---|
| Crítica | 0 | 0 |
| Alta | 0 | 0 |
| Media | 1 | 0 |
| Baja | 0 | 0 |

## 16. Resultados por entidad (regresión final)

| Entidad / grupo | Diseñados | Aprobados | Fallidos | Afectados por DEF-001 durante el proceso |
|---|---|---|---|---|
| Categorías | 18 | 18 | 0 | 8 |
| Parámetros | 17 | 17 | 0 | 10 |
| Cargas | 25 | 25 | 0 | 19 |
| Objetos | 16 | 16 | 0 | 10 |
| Integración | 6 | 6 | 0 | 1 |
| Interfaz web | 8 | 8 | 0 | 0 |
| **Total** | **90** | **90** | **0** | **48** |

## 17. Evidencias

| Evidencia | Ubicación |
|---|---|
| Reportes xUnit (TRX) de los ciclos X1 a X4 | `docs/evidencias/xunit-ejecucion-{1..4}.trx` |
| Reportes Newman de los ciclos N1 a N4: consola, JSON completo (solicitudes y respuestas) y JUnit | `docs/evidencias/newman-ejecucion-{1..4}.{txt,json,xml}` |
| Interfaz U1: capturas y resultados | `docs/evidencias/ui-ejecucion-1/` |
| Interfaz U2 (final): capturas y resultados | `docs/evidencias/ui/` (por ejemplo, `CP-UI-004-validacion-carga.png` y `CP-UI-006-objeto-supera-peso.png`) |
| Registro por caso y ciclo | [07-registro-de-ejecucion.md](07-registro-de-ejecucion.md) |

**Verificación adicional de las instrucciones del README (no cuenta como ciclo formal):** sobre una copia limpia del proyecto,
sin `bin`, `obj` ni base de datos, se ejecutaron `dotnet restore` y `dotnet build` (0 advertencias, 0 errores) y luego
`dotnet run --launch-profile http`. Se comprobó que la base se creó y se sembró (2 cargas) y que `npx newman run` con la colección y el entorno
obtuvo 103 solicitudes y 455/455 aserciones aprobadas. La salida de esta verificación solo se observó en consola y no se guardó como archivo de evidencia.

## 18. Limitaciones

1. **Visual Studio no está instalado** en el entorno de ejecución. La solución se compiló y ejecutó con la CLI de .NET (`dotnet build`,
   `dotnet run`, `dotnet test`), que usa el mismo MSBuild. Queda **pendiente** verificar manualmente que se abre y ejecuta en Visual Studio
   (RNF-005). Un proyecto .NET 10 requiere Visual Studio 2026 o posterior.
2. **No se usó la aplicación de escritorio de Postman.** La colección se ejecutó con Newman, el motor de línea de comandos de Postman,
   con la misma colección y el mismo entorno. La importación en la aplicación de Postman no se verificó manualmente.
3. xUnit usa `TestServer` en memoria; las pruebas por HTTP real contra Kestrel las cubren Newman y la interfaz.
4. La interfaz se probó solo en Microsoft Edge (Chromium), en modo headless.
5. No se hicieron pruebas de rendimiento, concurrencia ni seguridad avanzada (excluidas en el plan). El umbral de 3 s por solicitud
   solo es una verificación básica.
6. Los valores de peso, volumen y capacidad son **académicos y configurables**; no representan límites legales.
7. Smart App Control puede bloquear la DLL de pruebas en otros equipos (INC-004).
8. La referencia a ISO/IEC/IEEE 29119 se limita a la estructura de los documentos (plan, especificación, registro, incidencias e informe) y a
   su terminología. **No se afirma conformidad total ni certificación.**

## 19. Conclusiones

- El sistema cumple los criterios de salida del plan: 100 % de ejecución, 100 % de aprobación, ningún caso de prioridad alta fallido
  y ningún defecto abierto.
- Las reglas centralizadas en los servicios se comportaron igual en la API, en la interfaz y en ambas herramientas de prueba.
- Las pruebas negativas fueron las más valiosas: el único defecto real apareció al verificar el formato de las respuestas de error.
- Distinguir la causa de cada fallo (sistema, testware, entorno u operación) evitó registrar como defectos lo que eran errores del
  testware: de 5 incidencias, solo 1 era un defecto.

## 20. Recomendaciones

1. Ejecutar `dotnet test` y la colección con Newman en un pipeline de integración continua en cada cambio, conservando los reportes.
2. Firmar los binarios de prueba o ejecutar las pruebas en un equipo o contenedor sin Smart App Control para evitar INC-004.
3. Verificar en Visual Studio 2026 la apertura y ejecución de la solución (pendiente de RNF-005).
4. Agregar una máquina de estados para `ESTADO_CARGA` (supuesto S-03) y casos de transición de estado.
5. Mostrar un mensaje explícito al arrancar si el ensamblado no contiene migraciones (mejora derivada de INC-001).
6. Si el proyecto crece: paginación en los listados, autenticación y pruebas de concurrencia.
