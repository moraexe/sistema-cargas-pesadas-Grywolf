# 07 · Registro de ejecución

Registro generado a partir de los **reportes reales** de ejecución guardados en [`docs/evidencias`](evidencias/):
archivos TRX de xUnit, JSON, JUnit y texto de Newman, y JSON más capturas PNG de la prueba de interfaz. No contiene
resultados supuestos: un caso sin ejecución en un ciclo aparece como "—".

## 1. Entorno de ejecución

| ID | Descripción |
|---|---|
| E-01 | Windows 11 Pro 10.0.26300 · .NET SDK 10.0.400 · ASP.NET Core 10.0.11 · EF Core / Microsoft.Data.Sqlite 10.0.13 · ejecución local |
| E-01/xUnit | WebApplicationFactory (TestServer en memoria) + SQLite temporal en %TEMP% · xUnit 2.9.3 |
| E-01/Newman | Aplicación en `http://localhost:5080` (Development) con la base separada `App_Data/newman-evidencia.db` · Newman 6.2.3 · Node.js 24.20.0 |
| E-01/UI | La misma instancia que Newman · Microsoft Edge 155.0.4283.45 headless controlado por CDP |

## 2. Ciclos de ejecución

| Ciclo | Fecha (hora Colombia) | Herramienta | Casos aplicables | Aprobados | Fallidos | Bloqueados | Evidencia | Nota |
|---|---|---|---|---|---|---|---|---|
| X1 | 14/10/26, 11:12 a. m. | xUnit | 82 | 5 | 1 | 76 | `xunit-ejecucion-1.trx` | Primera ejecución. |
| X2 | 14/10/26, 11:13 a. m. | xUnit | 82 | 34 | 48 | 0 | `xunit-ejecucion-2.trx` | Tras corregir INC-002 (testware). |
| X3 | 14/10/26, 11:15 a. m. | xUnit | 82 | 82 | 0 | 0 | `xunit-ejecucion-3.trx` | Tras corregir DEF-001. |
| N1 | 14/10/26, 11:20 a. m. | Newman | 77 | 68 | 9 | 0 | `newman-ejecucion-1.json` | Primera ejecución de la colección. (103 solicitudes; 446/455 aserciones) |
| N2 | 14/10/26, 11:20 a. m. | Newman | 77 | 77 | 0 | 0 | `newman-ejecucion-2.json` | Tras corregir INC-005 (testware). (103 solicitudes; 455/455 aserciones) |
| N3 | 14/10/26, 11:21 a. m. | Newman | 77 | 77 | 0 | 0 | `newman-ejecucion-3.json` | Repetición sobre la misma base (reproducibilidad). (103 solicitudes; 455/455 aserciones) |
| U1 | 14/10/26, 11:22 a. m. | UI | 8 | 8 | 0 | 0 | `ui-ejecucion-1/resultados-ui.json` | Primera ejecución de la interfaz. |
| X4 | 14/10/26, 11:30 a. m. | xUnit | 82 | 82 | 0 | 0 | `xunit-ejecucion-4.trx` | Regresión final. |
| N4 | 14/10/26, 11:30 a. m. | Newman | 77 | 77 | 0 | 0 | `newman-ejecucion-4.json` | Regresión final. (103 solicitudes; 455/455 aserciones) |
| U2 | 14/10/26, 11:30 a. m. | UI | 8 | 8 | 0 | 0 | `ui/resultados-ui.json` | Regresión final (marca Griwolf). |

- Los conteos son **por caso**; un caso con varias variantes cuenta como fallido o bloqueado si lo está alguna de ellas. Por
  prueba xUnit (109 ejecuciones por ciclo): X1 = 5 aprobadas, 1 fallida y 103 bloqueadas; X2 = 41 aprobadas y 68 fallidas; X3 y X4 = 109 aprobadas.
- En X1 los casos bloqueados no llegaron a ejecutarse contra el sistema por un error del testware (INC-002); el único fallo real
  fue CP-INT-004 (DEF-001).
- Hubo dos intentos de ejecución xUnit que no ejecutaron ninguna prueba porque Windows Smart App Control bloqueó la DLL de pruebas (INC-004),
  uno entre X2 y X3 y otro antes de X4. No se cuentan como ciclos y sus archivos TRX vacíos se descartaron.

## 3. Registro por caso (regresión final: X4, N4, U2)

| ID | Fecha | Entorno | Herramienta | Resultado esperado | Resultado obtenido | Estado | Evidencia | Incidencia | Observaciones |
|---|---|---|---|---|---|---|---|---|---|
| CP-CAT-001 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; lista JSON no vacía con id, tipoCategoria, codigo, nombre y activo. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-CAT-002 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; todos los elementos tienen tipoCategoria = tipo en mayúsculas. | xUnit: 4/4 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 3 solicitud(es); 12/12 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-003 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 ProblemDetails con error en tipoCategoria y la lista de valores permitidos. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-004 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; todas las categorías devueltas tienen el estado solicitado. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-005 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 201; Location /api/categorias/{id}; tipoCategoria y codigo en mayúsculas sin espacios; activo = true. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 201 en 6 solicitud(es); 36/36 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-006 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con el mismo id y código. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-CAT-007 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails; detail menciona el id. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-008 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con el nombre nuevo y activo = true. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-009 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con errores en tipoCategoria, codigo y nombre. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 7/7 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-010 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en tipoCategoria. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-011 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 ProblemDetails; detail menciona el código. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-012 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 201 Created. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 201 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-013 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409; detail con el número de usos y la sugerencia de desactivar; la categoría sigue existiendo. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-014 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con activo = false; la carga conserva su referencia. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-015 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 Conflict. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-016 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 204; un GET posterior devuelve 404. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 204, 404 en 2 solicitud(es); 6/6 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAT-017 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en codigo. | xUnit: 4/4 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAT-018 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con VEHICULO, TIPO_CARGA, TIPO_ENTREGA, ESTADO_CARGA, UNIDAD_MEDIDA y TIPO_OBJETO. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-PAR-001 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; hay parámetros con ambito GENERAL (categoriaId null) y CATEGORIA. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-PAR-002 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; todos los elementos cumplen el filtro. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-PAR-003 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 201; valor canónico (1500.50, 42, true, 2026-12-31); esObligatorio = false; ambito GENERAL. | xUnit: 5/5 variante(s) cumplen todas las aserciones<br>Newman: HTTP 201 en 1 solicitud(es); 6/6 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-PAR-004 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con el mismo código. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-PAR-005 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-006 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con valor 20.5 y FechaActualizacion ≥ la anterior. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-PAR-007 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en valor que menciona el tipo de dato. | xUnit: 7/7 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 4 solicitud(es); 24/24 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-008 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en tipoDato. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-009 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con errores en codigo, nombre, tipoDato y valor. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 8/8 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-010 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 Conflict. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-011 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409; el parámetro sigue existiendo. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-012 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 en cada variante. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-013 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en categoriaId. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-014 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en categoriaId. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-015 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en valor; el límite no cambia. | xUnit: 2/2 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-PAR-016 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 204; un GET posterior devuelve 404. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 204 en 2 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-PAR-017 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 201; ambito CATEGORIA; categoriaTipo VEHICULO. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 201 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAR-001 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; lista JSON (incluye la carga creada en Postman). | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-CAR-002 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 201; Location; codigo en mayúsculas; estadoCodigo PENDIENTE; cantidadObjetos 0. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 201 en 2 solicitud(es); 11/11 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAR-003 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con tipoCargaNombre, tipoEntregaNombre y estadoNombre. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAR-004 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-005 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 con los datos nuevos; fechaRegistro sin cambios. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-CAR-006 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con errores en codigo, descripcion, origen, destino, pesoTotalKg, tipoCargaId y tipoEntregaId. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 11/11 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-007 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en pesoTotalKg. | xUnit: 3/3 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 2 solicitud(es); 10/10 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-008 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en volumenTotalM3. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-009 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en destino u origen según la variante. | xUnit: 3/3 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-010 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409; detail menciona el código. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-011 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400; errors.tipoCargaId explica que la categoría es VEHICULO y no puede usarse como TIPO_CARGA. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 6/6 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-012 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en el campo correspondiente. | xUnit: 3/3 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 2 solicitud(es); 10/10 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-013 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en tipoEntregaId. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-014 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400; el mensaje indica "inactiva". | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 6/6 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-015 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en pesoTotalKg que cita PESO_MAXIMO_CARGA_KG; con peso = límite → 201. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-016 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 que cita el parámetro de capacidad; con peso = capacidad → 201. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-017 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en volumenTotalM3. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-018 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en pesoTotalKg; el cuerpo no contiene "System.". | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-019 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 ProblemDetails con errores (clave cuerpo o el campo). | xUnit: 3/3 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-020 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-021 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 204; un GET posterior devuelve 404. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 204, 404 en 2 solicitud(es); 6/6 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-CAR-022 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409; la carga y los objetos siguen existiendo. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-023 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 con 2999; 200 con 3000 (valor límite). | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-024 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-CAR-025 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200 OK; la categoría inactiva se conserva. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 2/2 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-OBJ-001 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; lista JSON. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-OBJ-002 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 201; pesoTotalKg = cantidad × peso; la carga muestra cantidadObjetos y pesoObjetosKg. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 201, 200 en 2 solicitud(es); 9/9 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-OBJ-003 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 2/2 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-OBJ-004 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-005 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; solo los objetos de la carga indicada. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 2 solicitud(es); 7/7 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-005 | X1: bloqueado (testware); N1: aserción falsa por tipo de variable (testware); aprobado en la regresión final. |
| CP-OBJ-006 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 404 ProblemDetails. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 404 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-007 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 200; pesoTotalKg recalculado (4500 / 3000). | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 200 en 1 solicitud(es); 3/3 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-OBJ-008 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en cargaId. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-009 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en cantidad. | xUnit: 2/2 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-010 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en el campo. | xUnit: 2/2 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-011 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con errores en cargaId, nombre, cantidad y pesoUnitarioKg. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 8/8 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-012 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400; el mensaje menciona TIPO_OBJETO. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-013 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 cuando se supera; 201 cuando completa exactamente el peso. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-014 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 400 con error en cantidad. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 400 en 1 solicitud(es); 5/5 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-OBJ-015 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 204; un GET posterior devuelve 404. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 204, 404 en 2 solicitud(es); 6/6 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002 | X1: bloqueado (testware); aprobado en la regresión final. |
| CP-OBJ-016 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | 409 Conflict. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 409 en 1 solicitud(es); 4/4 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-002, INC-003 / DEF-001 | X1: bloqueado (testware); X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-INT-001 | 14/10/26, 11:30 a. m. | E-01 | xUnit | Con la segunda instancia, GET /api/cargas/{id} → 200 con el mismo código y 1 objeto; el número de categorías no cambia. | xUnit: 1/1 variante(s) cumplen todas las aserciones | **Aprobado** | `xunit-ejecucion-4.trx` | — | — |
| CP-INT-002 | 14/10/26, 11:30 a. m. | E-01 | xUnit | Borrar la categoría referenciada → DbUpdateException; borrar una carga con un objeto → DbUpdateException. | xUnit: 1/1 variante(s) cumplen todas las aserciones | **Aprobado** | `xunit-ejecucion-4.trx` | — | — |
| CP-INT-003 | 14/10/26, 11:30 a. m. | E-01 | xUnit | SqliteException "CHECK constraint failed" en los tres casos. | xUnit: 1/1 variante(s) cumplen todas las aserciones | **Aprobado** | `xunit-ejecucion-4.trx` | — | — |
| CP-INT-004 | 14/10/26, 11:30 a. m. | E-01 | xUnit + Newman | Carga llena → objeto extra 409; eliminar la categoría o la carga con dependencias → 409; eliminar en orden (objetos, carga, categoría) → 204. | xUnit: 1/1 variante(s) cumplen todas las aserciones<br>Newman: HTTP 204 en 7 solicitud(es); 14/14 aserciones | **Aprobado** | `xunit-ejecucion-4.trx`<br>`newman-ejecucion-4.json` | INC-003 / DEF-001 | X1: fallido por DEF-001; X2: fallido por DEF-001; aprobado en la regresión final. |
| CP-INT-005 | 14/10/26, 11:30 a. m. | E-01 | xUnit | 500 application/problem+json sin el mensaje de la excepción, "Data Source", "Password", el nombre de la excepción ni la pila de llamadas. | xUnit: 1/1 variante(s) cumplen todas las aserciones | **Aprobado** | `xunit-ejecucion-4.trx` | — | — |
| CP-INT-006 | 14/10/26, 11:30 a. m. | E-01 | xUnit | La ruta del archivo está en %TEMP% y no es transporte.db. | xUnit: 1/1 variante(s) cumplen todas las aserciones | **Aprobado** | `xunit-ejecucion-4.trx` | — | — |
| CP-UI-001 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | Los indicadores son numéricos y coinciden con la API. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-001-inicio.png` | — | — |
| CP-UI-002 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | La tabla muestra solo filas VEHICULO. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-002-filtro-categorias.png` | — | — |
| CP-UI-003 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | Cada selector solo contiene ids del tipo correspondiente. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-003-formulario-carga.png` | — | — |
| CP-UI-004 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | Alerta con HTTP 400; los campos codigo, descripcion, origen, destino, pesoTotalKg, tipoCargaId y tipoEntregaId se marcan como inválidos. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-004-validacion-carga.png` | — | — |
| CP-UI-005 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | Mensaje de éxito; la carga aparece en la tabla. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-005-registro-carga.png` | — | — |
| CP-UI-006 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | El primero se registra; el segundo muestra una alerta HTTP 409 con el peso disponible. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-006-objeto-supera-peso.png` | — | — |
| CP-UI-007 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | Se muestra la confirmación y luego una alerta HTTP 409. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-007-eliminar-carga-con-objetos.png` | — | — |
| CP-UI-008 | 14/10/26, 11:30 a. m. | E-01 | CDP/Edge | La tabla muestra PESO_MAXIMO_CARGA_KG con la etiqueta Obligatorio y el ámbito VEHICULO / TRACTOCAMION. | UI: comportamiento verificado en el DOM | **Aprobado** | `ui/CP-UI-008-parametros.png` | — | — |

## 4. Historial por caso y ciclo

Convenciones: ✅ aprobado · ❌ fallido · ⛔ bloqueado · — no aplica o no existía.

| ID | X1 | X2 | X3 | N1 | N2 | N3 | U1 | X4 | N4 | U2 |
|---|---|---|---|---|---|---|---|---|---|---|
| CP-CAT-001 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-002 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-003 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-004 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-005 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-006 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-007 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-008 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-009 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-010 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-011 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-012 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-013 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-014 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-015 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-016 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-017 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAT-018 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-001 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-002 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-003 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-004 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-005 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-006 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-007 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-008 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-009 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-010 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-011 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-012 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-013 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-014 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-015 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-016 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-PAR-017 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-001 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-002 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-003 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-004 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-005 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-006 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-007 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-008 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-009 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-010 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-011 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-012 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-013 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-014 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-015 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-016 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-017 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-018 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-019 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-020 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-021 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-022 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-023 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-024 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-CAR-025 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-001 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-002 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-003 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-004 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-005 | ⛔ | ✅ | ✅ | ❌ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-006 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-007 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-008 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-009 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-010 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-011 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-012 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-013 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-014 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-015 | ⛔ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-OBJ-016 | ⛔ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-INT-001 | ✅ | ✅ | ✅ | — | — | — | — | ✅ | — | — |
| CP-INT-002 | ✅ | ✅ | ✅ | — | — | — | — | ✅ | — | — |
| CP-INT-003 | ✅ | ✅ | ✅ | — | — | — | — | ✅ | — | — |
| CP-INT-004 | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — |
| CP-INT-005 | ✅ | ✅ | ✅ | — | — | — | — | ✅ | — | — |
| CP-INT-006 | ✅ | ✅ | ✅ | — | — | — | — | ✅ | — | — |
| CP-UI-001 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-002 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-003 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-004 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-005 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-006 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-007 | — | — | — | — | — | — | ✅ | — | — | ✅ |
| CP-UI-008 | — | — | — | — | — | — | ✅ | — | — | ✅ |
