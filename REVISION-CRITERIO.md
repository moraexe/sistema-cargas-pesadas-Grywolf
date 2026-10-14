# Revisión de código · Cargas, Objetos, Categorías y Parámetros

**Fecha:** 14/10/2026
**Rama:** `Modelo-de-negocio-y-datos`
**Alcance:** API REST + servicios de `TransporteCargaPesada.Web` (entidades Carga, Objeto, Categoria, Parametro).
**Base:** commit `d577320` · se contrastó el código con `docs/02-requisitos-y-reglas.md` (RN-01…RN-19) y con las reglas de `.clinerules/rules/csharp/`.

---

## 1. Resumen

La revisión **no detectó defectos funcionales**. La implementación cumple las reglas de negocio RN-01…RN-19 y respeta la
arquitectura en capas (Controllers → Services → `Services/Reglas/`, con `Data/AppDbContext` y `DbSeeder`). Se registran
**5 observaciones menores** de adherencia a `.clinerules` y de estilo, ninguna de las cuales rompe el comportamiento actual.

| | Resultado |
|---|---|
| Entidades revisadas | 4 (Carga, Objeto, Categoria, Parametro) |
| Reglas de negocio verificadas | 19/19 conformes |
| Defectos funcionales | 0 |
| Observaciones de adherencia/estilo | 5 |

---

## 2. Arquitectura observada

```
Controllers  ──▶  ApiControllerBase.Responder  ──▶  RespuestasError (ProblemDetails, RFC 9457)
    │                    traduce Resultado<T> → 200/201/204/400/404/409/500
    ▼
Services     ──▶  CargaService · ObjetoService · CategoriaService · ParametroService
    │                  └─▶ ValidadorReferencias (RN-06/07/08, referencias a categorías)
    ▼                       └─▶ Services/Reglas/ (ReglasValidacion, TiposCategoria,
Models       ──▶  Carga · Objeto · Categoria · Parametro                         TiposDato, ParametrosSistema, ErroresValidacion)
    ▲
Data         ──▶  AppDbContext (CHECK + índices únicos + FK Restrict) · DbSeeder · InicializacionBaseDatos
```

Patrones correctamente aplicados: resultado de operación `Resultado<T>` con traducción centralizada a HTTP, validación en el
boundary (DTOs), errores sin detalles internos, fechas en UTC, restricciones CHECK e índices únicos también a nivel de base de datos.


---

## 3. Conformidad por entidad

### 3.1 Categorías (`CategoriaService`)
| Regla | Estado |
|---|---|
| RF-003 CRUD + normalización a mayúsculas (`NormalizarCodigo`) | OK |
| RN-07 tipo válido contra `TiposCategoria` | OK |
| RN-11 código único **por tipo** (índice compuesto `TipoCategoria,Codigo`) | OK |
| RN-14 categoría en uso no se elimina ni cambia de tipo (409); se desactiva | OK (`ContarUsosAsync`) |
| RF-006 filtros `?tipoCategoria=&activo=`, con 400 si el tipo no existe | OK |

### 3.2 Parámetros (`ParametroService`)
| Regla | Estado |
|---|---|
| RF-004 CRUD + ámbito `GENERAL`/`CATEGORIA` en la respuesta | OK |
| RN-10 valor canónico según `TipoDato` (`TiposDato.TryNormalizar`) | OK |
| D-05 código único **global** (índice único simple) | OK |
| RN-17 límites (`PESO_MAXIMO_CARGA_KG`, `CAPACIDAD_KG_*`): numéricos, >0, generales sin categoría, capacidad ligada a VEHICULO | OK |
| RN-19 obligatorio no se elimina/desactiva ni cambia código/tipo (409) | OK |

### 3.3 Cargas (`CargaService`)
| Regla | Estado |
|---|---|
| RN-01 código único · RN-02/03 formato y obligatorios · origen ≠ destino | OK |
| RN-04/05 peso y volumen estrictamente positivos | OK |
| CA-001.5 sin `estadoId` al crear → `PENDIENTE` (o 400 explicativo) | OK |
| RN-17 límites generales y por capacidad de vehículo | OK |
| RN-18 peso declarado cubre objetos ya registrados (409 al reducir por debajo) | OK |
| RN-13 carga con objetos no se elimina (409) | OK |

### 3.4 Objetos (`ObjetoService`)
| Regla | Estado |
|---|---|
| RF-002 CRUD + `pesoTotalKg = cantidad × pesoUnitarioKg` | OK |
| RN-06/09 pertenece a una carga existente (400) | OK |
| RN-07 categoría opcional de tipo `TIPO_OBJETO` | OK |

---

## 4. Observaciones (adherencia a `.clinerules` y estilo)

> Ninguna altera el comportamiento actual; son mejoras de adherencia y legibilidad. Quedan documentadas para decidir si se aplican.

### OBS-01 · `CancellationToken` ausente en toda la API async  *(regla: coding-style)*
Ningún método `async` de servicios ni controladores recibe ni propaga `CancellationToken`. La regla pide pasarlo por las APIs
públicas. Impacto: bajo (no hay cancelación real hoy). Requeriría un cambio en cascada por los 4 servicios y sus tests.

### OBS-02 · `ObjetoService` muta el DTO de request  *(regla: coding-style · immutability)*
En `ObjetoService.ValidarAsync` (líneas 71-73) se reasigna `s.Nombre`, `s.Descripcion`, `s.Observaciones` sobre el objeto de
entrada. La regla indica **no mutar los modelos de entrada**. Los otros tres servicios usan variables locales; este es el único
inconsistente. Se sugiere usar variables locales como en `CargaService`/`CategoriaService`/`ParametroService`.

### OBS-03 · Patrón poco legible en `CategoriaService.ActualizarAsync`  *(estilo)*
Línea 73: `if (datos.Tipo != categoria.TipoCategoria && await ContarUsosAsync(id) is var usos && usos.Total > 0)`.
Es válido pero mezcla comparación y asignación por patrón dentro de la condición. Extraer `var usos = await ContarUsosAsync(id);`
antes del `if` mejora la legibilidad.

### OBS-04 · Servicios sin interfaz  *(regla: patterns · service boundaries)*
Los servicios se registran como clases concretas (`AddScoped<CargaService>()`, etc.). La guía sugiere depender de interfaces en los
límites de servicio. No es un defecto (las pruebas usan `WebApplicationFactory`, no mocks), pero dificulta unit tests con sustitutos.

### OBS-05 · RN-18 valida peso pero no volumen  *(decisión de negocio)*
La regla verifica que la suma de pesos de los objetos no supere `PesoTotalKg` de la carga, pero **no** hay validación análoga de
volumen contra `VolumenTotalM3`. Probablemente intencional (D-01 solo menciona peso). Se documenta para que el equipo decida si
procede añadir la validación de volumen (implicaría un caso nuevo en `docs/06`).


---

## 5. Recomendación

Mantener el código tal como está para la entrega actual (cumple RN-01…RN-19 y pasa la regresión 90/90 documentada). Si se decide
mejorar la adherencia a `.clinerules`, el orden sugerido por relación esfuerzo/beneficio es: **OBS-02 → OBS-03 → OBS-01 → OBS-04**,
dejando **OBS-05** para una decisión funcional del equipo. Cualquier cambio debe reverificarse con `dotnet build` y `dotnet test`.

| RN-18 suma de pesos no supera el declarado (incluye mover de carga) | OK |
