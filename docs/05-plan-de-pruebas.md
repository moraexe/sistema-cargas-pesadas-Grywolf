# 05 · Plan de pruebas

Plan basado en la estructura de ISO/IEC/IEEE 29119-3 (plan, especificación, registro, incidencias e informe) y adaptado al
alcance de un proyecto académico. **No se afirma conformidad total ni certificación** con la norma.

| Dato | Valor |
|---|---|
| Identificador | PP-TCP-001 |
| Sistema bajo prueba | Sistema de transporte de carga pesada (API REST + interfaz web + SQLite) |
| Versión del software | Código del repositorio al 14/10/2026 (.NET 10, migración `InicialTransporte`) |
| Responsable | Equipo del proyecto académico (asistido por Claude Code) |
| Documentos relacionados | 01 a 04 (base de prueba), 06 (especificación), 07 (registro), 08 (incidencias), 09 (informe) |

## 1. Objetivos

1. Verificar que el CRUD de cargas, objetos, categorías y parámetros cumple RF-001 a RF-007.
2. Comprobar que las 19 reglas de negocio (RN-01 a RN-19) se aplican y producen los códigos HTTP documentados.
3. Demostrar mediante pruebas negativas que el sistema rechaza datos inválidos de forma controlada, en especial las categorías de
   tipo incorrecto (un `VEHICULO` usado como `TIPO_CARGA`).
4. Verificar la integración API ↔ EF Core ↔ SQLite: persistencia, integridad referencial y restricciones.
5. Verificar que la interfaz web consume la API y muestra sus validaciones.
6. Detectar, registrar y dar seguimiento a los defectos hasta su cierre.

## 2. Alcance

### 2.1. Elementos que se prueban

| Elemento | Detalle |
|---|---|
| API `/api/cargas` | Incluye `/api/cargas/{id}/objetos` |
| API `/api/objetos` | Incluye el filtro `?cargaId=` |
| API `/api/categorias` | Incluye `/tipos` y los filtros `tipoCategoria` y `activo` |
| API `/api/parametros` | Incluye `/tipos-dato` y los filtros `categoriaId` y `activo` |
| Reglas de negocio | RN-01 a RN-19 |
| Formato de errores | ProblemDetails, sin detalles internos |
| Base de datos SQLite | Migración, FK, CHECK, persistencia y siembra |
| Interfaz web | Páginas de inicio, cargas, objetos, categorías y parámetros |

### 2.2. Elementos excluidos

| Elemento | Motivo |
|---|---|
| Autenticación y autorización | No existen en el sistema (exclusión del alcance). |
| Rendimiento, carga y estrés | No es objetivo de la asignatura; solo se verifica un umbral simple de 3 s por solicitud. |
| Concurrencia | Supuesto S-05: un solo usuario. |
| Compatibilidad entre navegadores | Solo se usa Microsoft Edge (Chromium). |
| Accesibilidad formal (WCAG) | Fuera de alcance. |
| Seguridad avanzada (inyección, XSS exhaustivo) | Fuera de alcance; la interfaz escapa el HTML que muestra y EF Core usa consultas parametrizadas. |
| Ejecución desde Visual Studio | Visual Studio no está instalado en el entorno de ejecución; queda como verificación manual pendiente. |

## 3. Estrategia

| Aspecto | Decisión |
|---|---|
| Enfoque | Basado en requisitos y en riesgos. Cada RF y RN tiene al menos un caso positivo o negativo trazable. |
| Técnicas de diseño | Partición de equivalencia (válidos e inválidos por campo), análisis de valores límite (peso = 0, peso = límite, límite + 0,01), tablas de decisión (tipo de categoría × campo), transición de estado (activa → inactiva) y pruebas basadas en escenarios (flujo completo). |
| Niveles | Sistema (API en caja negra), integración (API + EF Core + SQLite) y aceptación de interfaz (UI sobre la API). |
| Tipos | Funcional, negativa, integración e interfaz. |
| Automatización | Todas las pruebas son automatizadas. xUnit y Postman se complementan: xUnit trabaja en memoria con una base temporal; Postman/Newman trabaja contra la aplicación en ejecución por HTTP real. |
| Datos de prueba | Cada ejecución crea sus propios datos con un código único (GUID o sufijo de tiempo). No se depende de identificadores fijos; los datos sembrados se buscan por código. |
| Independencia de la base de datos | xUnit usa un archivo SQLite temporal por clase. Newman se ejecutó contra una base separada (`App_Data/newman-evidencia.db`, eliminada después). La colección también elimina todo lo que crea. |
| Tratamiento de fallos | Ante un fallo se analiza la causa (sistema, testware, entorno u operación), se registra la incidencia con su evidencia, se corrige y se repiten las pruebas afectadas (repetición y regresión completa). |

## 4. Tipos y niveles de prueba

| Tipo | Nivel | Herramienta | Prefijos de los casos |
|---|---|---|---|
| Funcional (positiva) | Sistema | xUnit + Postman | CP-CAT, CP-PAR, CP-CAR, CP-OBJ |
| Negativa | Sistema | xUnit + Postman | CP-CAT, CP-PAR, CP-CAR, CP-OBJ |
| Integración | Integración | xUnit (`IntegracionTests`) | CP-INT |
| Interfaz | Aceptación | Script CDP sobre Edge headless | CP-UI |

## 5. Herramientas

| Herramienta | Versión | Uso |
|---|---|---|
| .NET SDK | 10.0.400 | Compilación y ejecución |
| ASP.NET Core / EF Core / Microsoft.Data.Sqlite | 10.0.11 (runtime) / 10.0.13 / 10.0.13 | Sistema bajo prueba |
| xUnit | 2.9.3 (runner VS 3.1.4, Test SDK 17.14.1) | Pruebas automatizadas |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.13 | Servidor de prueba en memoria (`WebApplicationFactory`) |
| Postman Collection | v2.1 | Diseño de las pruebas de API |
| Newman | 6.2.3 (Node.js 24.20.0) | Ejecución de la colección por línea de comandos |
| Microsoft Edge | 155.0.4283.45 (headless, CDP) | Pruebas de interfaz y capturas |

## 6. Entorno

| Elemento | Valor |
|---|---|
| Sistema operativo | Windows 11 Pro 10.0.26300 |
| Aplicación | `dotnet run --project src/TransporteCargaPesada.Web --launch-profile http` → `http://localhost:5080`, entorno Development |
| Base de datos de la aplicación | `src/TransporteCargaPesada.Web/App_Data/transporte.db` |
| Base de datos de xUnit | `%TEMP%\TransporteCargaPesada.Tests\pruebas-<guid>.db` (una por clase, se elimina al terminar) |
| Base de datos de Newman y UI | `App_Data/newman-evidencia.db` (variable `ConnectionStrings__DefaultConnection`; se eliminó al terminar) |
| Restricción conocida | Smart App Control de Windows está activo y puede bloquear las DLL de prueba sin firma (INC-004) |

## 7. Riesgos

| ID | Riesgo | Probabilidad | Impacto | Mitigación |
|---|---|---|---|---|
| R-01 | Que la interfaz o las pruebas dupliquen reglas y se desalineen del servidor | Media | Alto | Reglas centralizadas en `Services`; la UI solo muestra los errores de la API. |
| R-02 | Que las pruebas contaminen la base de la aplicación | Media | Medio | Base temporal en xUnit; base separada en Newman; limpieza en la colección. |
| R-03 | Dependencia del orden en la colección Postman | Alta | Medio | Carpetas numeradas y variables encadenadas; se ejecuta en Collection Runner en orden. |
| R-04 | Resultados falsos (falsos positivos o negativos) por errores del testware | Media | Alto | Analizar cada fallo antes de clasificarlo; registrar los errores del testware como incidencias (INC-002, INC-005). |
| R-05 | Bloqueos del entorno (políticas de Windows, puertos ocupados) | Media | Medio | Documentar en el README y registrar como incidencia de entorno (INC-004). |
| R-06 | Ambigüedad en los requisitos (por ejemplo, el código para un peso excedido) | Media | Medio | Decisiones D-01 a D-07 documentadas en 02. |
| R-07 | Presentar resultados esperados como observados | Baja | Alto | El registro de ejecución se genera a partir de los reportes reales (TRX, JSON de Newman y JSON de la UI). |

## 8. Criterios de entrada

1. La solución compila sin errores ni advertencias.
2. La migración se aplica sobre una base vacía y la siembra se completa.
3. Prueba rápida de humo: `GET /api/categorias/tipos` responde 200.
4. Los casos de 06 están especificados y trazados a los requisitos.

## 9. Criterios de salida

1. El 100 % de los casos diseñados y automatizables se ejecutó al menos una vez sobre la versión final.
2. Porcentaje de aprobación de la última ejecución ≥ 95 %, sin casos fallidos de prioridad Alta.
3. No quedan defectos abiertos de severidad Alta o Crítica.
4. Cada incidencia está clasificada (defecto, testware, entorno u operación) y cerrada o justificada.
5. El informe 09 diferencia los resultados observados de las actividades pendientes.

**Tratamiento de casos bloqueados:** un caso bloqueado (no pudo ejecutarse por causas ajenas al sistema) cuenta como **diseñado
pero no ejecutado** en ese ciclo. No suma al denominador del porcentaje de aprobación, pero sí reduce el porcentaje de ejecución.

## 10. Métricas

- Porcentaje de ejecución = casos ejecutados / casos diseñados × 100.
- Porcentaje de aprobación = casos aprobados / casos ejecutados × 100. Si no se ejecutó ningún caso, se informa "N/A" (no se divide por cero).
- Un caso con varias variantes (Theory de xUnit o varias solicitudes de Postman) se aprueba solo si **todas** sus variantes se aprueban.
- Un caso cubierto por varias herramientas se aprueba solo si se aprueba en **todas** las que lo ejecutaron en el ciclo.

## 11. Secuencia de ejecución

| Paso | Actividad | Herramienta | Resultado esperado |
|---|---|---|---|
| 1 | Compilar la solución | `dotnet build` | 0 errores |
| 2 | Prueba de humo de la API | curl | Respuestas 200, 400, 404 y 409 coherentes |
| 3 | Ciclo xUnit (funcional, negativo e integración) | `dotnet test` | Reporte TRX en `docs/evidencias` |
| 4 | Analizar los fallos, registrar incidencias, corregir y repetir | — | 07 y 08 actualizados |
| 5 | Levantar la aplicación con una base separada | `dotnet run` | API en el puerto 5080 |
| 6 | Ciclo Postman/Newman | `npx newman run` | Reportes JSON, JUnit y texto |
| 7 | Ciclo de interfaz | Script CDP sobre Edge | Capturas PNG y JSON de resultados |
| 8 | Regresión final después de la última modificación del código | xUnit + Newman + UI | 100 % aprobado |
| 9 | Elaborar el informe | — | 09 |
