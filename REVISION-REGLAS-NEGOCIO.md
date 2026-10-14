# Revisión de restricciones y reglas de negocio (RN-01 … RN-19)

**Fecha:** 14/10/2026
**Rama:** `Modelo-de-negocio-y-datos`
**Alcance:** capa de validación de los servicios (`CargaService`, `ObjetoService`, `CategoriaService`, `ParametroService`,
`ValidadorReferencias`) y de `Services/Reglas/` (`ReglasValidacion`, `TiposCategoria`, `TiposDato`, `ParametrosSistema`).
**Base:** commit `d577320` · contrastado con `docs/02-requisitos-y-reglas.md` (RN-01…RN-19) y `docs/06-especificacion-de-pruebas.md`.

---

## 1. Resumen

| | Resultado |
|---|---|
| Reglas de negocio verificadas | **19/19 conformes** |
| Suite xUnit ejecutada | **109/109 superadas** (`dotnet test`) |
| Defectos funcionales | 0 |
| Observaciones de adherencia/estilo | 6 |

La lógica de negocio está **centralizada** en los servicios y en `Services/Reglas/`; la API y la interfaz no la duplican.
Cada regla tiene su criterio de aceptación cubierto por al menos un caso automatizado. Lo verifiqué ejecutando la suite completa.

---

## 2. Verificación ejecutada (evidencia)

Comando: `dotnet test` (repo `sistema-cargas-pesadas-Grywolf`, solución completa)

```
Correctas! - Con error: 0, Superado: 109, Omitido: 0, Total: 109, Duración: 6 s
```

Cobertura por entidad (clases de prueba): `CargasTests`, `ObjetosTests`, `CategoriasTests`, `ParametrosTests`, `IntegracionTests`.
Todas las reglas con criterio negativo (400/409) tienen su caso `[Trait("Tipo","Negativa")]` asociado y pasan.

---

## 3. Dónde vive cada regla (trazabilidad código ↔ RN)

| Regla | Implementación central | Caso(s) |
|---|---|---|
| RN-01 Código de carga único | `CargaService` + `IX_Cargas_Codigo` (índice único) | CP-CAR-010 |
| RN-02 Campos obligatorios no vacíos | `ReglasValidacion.Limpiar/Texto` | CP-CAR-006, CP-OBJ-011, CP-CAT-009, CP-PAR-009 |
| RN-03 Origen/destino 3-100 y distintos | `CargaService.ValidarAsync` | CP-CAR-009 |
| RN-04 Peso > 0, volumen no negativo | `ReglasValidacion.Positivo` + CHECK | CP-CAR-007/008, CP-OBJ-010 |
| RN-05 Cantidad de objeto > 0 | `ReglasValidacion.Positivo` + CHECK | CP-OBJ-009, CP-OBJ-014 |
| RN-06 Ids relacionados existen | `ValidadorReferencias`, `ObjetoService` | CP-CAR-013, CP-OBJ-008, CP-PAR-013 |
| RN-07 Tipo de categoría corresponde a su uso | `ValidadorReferencias.ValidarCategoriaAsync` | CP-CAR-011/012, CP-OBJ-012, CP-PAR-014 |
| RN-08 No referenciar categorías inactivas (conserva la ya asignada) | `ValidadorReferencias` + `valorActual` | CP-CAR-013/014/025 |
| RN-09 Objeto no en carga inexistente | `ObjetoService` + FK | CP-OBJ-008 |
| RN-10 Valor respeta el tipo de dato | `TiposDato.TryNormalizar` | CP-PAR-003/007/008 |
| RN-11 Código de categoría único por tipo | índice `(TipoCategoria, Codigo)` | CP-CAT-011/012 |
| RN-12 Eliminación respeta dependencias | FK `Restrict` | CP-INT-002/004 |
| RN-13 No eliminar carga con objetos | `CargaService.EliminarAsync` | CP-CAR-022 |
| RN-14 Categoría en uso: no se elimina ni cambia de tipo | `CategoriaService` + `ContarUsosAsync` | CP-CAT-013/014/015 |
| RN-15 Códigos HTTP coherentes | `ApiControllerBase.Responder` | Todos |
| RN-16 No exponer detalles internos | `RespuestasError` + `AllowInputFormatterExceptionMessages=false` | CP-INT-005, CP-CAR-018 |
| RN-17 Límites configurables peso/volumen | `CargaService` lee `PESO_MAXIMO_CARGA_KG`, `VOLUMEN_MAXIMO_CARGA_M3`, `CAPACIDAD_KG_*` | CP-CAR-015/016/017, CP-PAR-015 |
| RN-18 Peso de objetos ≤ peso declarado | `ObjetoService.ValidarPesoAsync`, `CargaService.ActualizarAsync` | CP-OBJ-013/016, CP-CAR-023 |

---

## 4. Análisis de las decisiones de validación

### 4.1 Doble línea de defensa (RN-04, RN-05, RN-10, RN-17)
Las validaciones de los servicios devuelven mensajes claros (400/409) y las restricciones `CHECK`/índices de SQLite actúan como
respaldo si se omite la API. Correcto: el mensaje de negocio lo da el servicio; la BD garantiza el invariante.

### 4.2 Políticas de "no eliminar, desactivar" (RN-14, RN-19)
- **Categorías en uso (RN-14):** no se borran ni cambian de tipo si `ContarUsosAsync` > 0; se desactivan para conservar el historial.
- **Parámetros obligatorios (RN-19):** no se eliminan, no se desactivan y no cambian código/tipo (los usa el motor de reglas).
Ambas son coherentes con el modelo de "integridad histórica" y tienen casos negativos dedicados.

### 4.3 Coherencia de tipo fuera de la BD (RN-07)
`ValidadorReferencias.ValidarCategoriaAsync` compara `TipoCategoria` porque SQLite no permite FK condicionadas a otra columna.
Recibe `valorActual` para **conservar** una categoría que estaba asignada y luego se desactivó (RN-08, CP-CAR-025). Decisión acertada.

### 4.4 Semántica de códigos HTTP (D-02, D-03, RN-15)
- Referencia inexistente en el cuerpo → **400** (dato inválido); el recurso de la ruta inexistente → **404**.
- Conflicto con otra entidad (superar peso de la carga) → **409**; valor inválido por configuración (límite) → **400**.
Traducción centralizada en `ApiControllerBase.Responder`. Coherente y verificada por toda la suite.

### 4.5 Límites configurables (RN-17)
`CargaService` consulta los parámetros activos al vuelo (`LimiteGeneralAsync`, `CAPACIDAD_KG_*` del vehículo). El criterio
"superar → 400, igual al límite → 201" se cumple (CP-CAR-015, CP-OBJ-013 con valor justo permitido). Buen diseño: reglas
académicas parametrizadas, no hardcodeadas.

---

## 5. Observaciones (adherencia a `.clinerules` y estilo)

> Ninguna corrige un fallo: la suite 109/109 pasa y las 19 reglas se cumplen. Son mejoras de adherencia y robustez.

### OBS-RN-01 · `CancellationToken` ausente en toda la API async  *(regla: coding-style)*
Igual que en la revisión de entidades: ningún `async` propaga `CancellationToken`. Impacto bajo; cambio en cascada si se aborda.

### OBS-RN-02 · `ObjetoService` muta el DTO de request  *(regla: coding-style · immutability)*
`ObjetoService.ValidarAsync` reasigna `s.Nombre/Descripcion/Observaciones`. Es el único servicio que muta la entrada; los otros tres
usan variables locales. Unificar el estilo reduce sorpresas.

### OBS-RN-03 · Patrón poco legible en `CategoriaService.ActualizarAsync`  *(estilo)*
`... && await ContarUsosAsync(id) is var usos && usos.Total > 0`. Extraer la llamada a una variable local antes del `if`.

### OBS-RN-04 · `Positivo` confunde mensaje cuando el valor falta  *(robustez de mensajes)*
En `ReglasValidacion.Positivo`, si `valor` es `null` y `obligatorio=true` el mensaje es "es obligatorio"; correcto. Pero cuando
`obligatorio=false` y el valor es `null`, la función retorna sin error (correcto), mientras que un `0` devuelve "debe ser mayor que
cero". Comportamiento correcto; se documenta solo para dejar clara la diferencia entre "ausente" y "cero".

### OBS-RN-05 · RN-18 valida peso pero no volumen  *(decisión de negocio)*
La suma de volúmenes de los objetos no se contrasta con `VolumenTotalM3` de la carga. Probablemente intencional (D-01 solo menciona
peso). Requiere decisión del equipo y un caso nuevo si se implementa.

### OBS-RN-06 · Mensajes de validación en español incrustados en código  *(mantenibilidad)*
Los textos de error están en el código fuente (capa de servicios). Es adecuado para el alcance académico; en un sistema
multi-idioma convendría externalizarlos a recursos. Nota prospectiva, no un defecto.

---

## 6. Recomendación

Las **19 reglas de negocio están correctamente implementadas, centralizadas y verificadas** (109/109 tests). Para la entrega
académica **no requiere cambios**. Las observaciones OBS-RN-01/02/03 son de estilo/adherencia a `.clinerules` (compartidas con la
revisión de entidades) y OBS-RN-04/06 son notas prospectivas; **OBS-RN-05** (validación de volumen en RN-18) es la única que
implicaría una decisión funcional del equipo y un caso nuevo en `docs/06`. Cualquier ajuste debe reverificarse con `dotnet test`.


| RN-19 Parámetros obligatorios inmutables | `ParametroService` | CP-PAR-011/012 |
