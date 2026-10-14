# 02 · Requisitos y reglas de negocio

Cada requisito y cada regla tienen un identificador único y criterios de aceptación verificables. La columna
**Casos** relaciona cada elemento con los casos de [06-especificacion-de-pruebas.md](06-especificacion-de-pruebas.md).

## 1. Requisitos funcionales

| ID | Requisito | Criterios de aceptación | Casos |
|---|---|---|---|
| RF-001 | Gestionar cargas (crear, consultar, listar, actualizar, eliminar). | CA-001.1 `POST /api/cargas` válido → 201 con `Location`. CA-001.2 `GET /api/cargas/{id}` → 200 o 404. CA-001.3 `PUT` → 200 con datos nuevos. CA-001.4 `DELETE` → 204 si no tiene objetos. CA-001.5 Sin `estadoId`, la carga se crea como `PENDIENTE`. | CP-CAR-001 a CP-CAR-005, CP-CAR-020, CP-CAR-021, CP-CAR-024 |
| RF-002 | Gestionar objetos de una carga. | CA-002.1 CRUD completo con 201/200/204/404. CA-002.2 La respuesta calcula `pesoTotalKg = cantidad × pesoUnitarioKg`. CA-002.3 La carga refleja `cantidadObjetos` y `pesoObjetosKg`. | CP-OBJ-001 a CP-OBJ-004, CP-OBJ-007, CP-OBJ-015 |
| RF-003 | Gestionar categorías (catálogo genérico tipado). | CA-003.1 CRUD completo. CA-003.2 El código y el tipo se normalizan a mayúsculas. CA-003.3 `GET /api/categorias/tipos` devuelve la lista centralizada. | CP-CAT-001, CP-CAT-005 a CP-CAT-008, CP-CAT-016, CP-CAT-018 |
| RF-004 | Gestionar parámetros con tipo de dato. | CA-004.1 CRUD completo. CA-004.2 El valor se guarda en forma canónica (por ejemplo `TRUE` → `true`). CA-004.3 La respuesta indica el ámbito `GENERAL` o `CATEGORIA`. | CP-PAR-001, CP-PAR-003 a CP-PAR-006, CP-PAR-016, CP-PAR-017 |
| RF-005 | Consultar los objetos de una carga. | CA-005.1 `GET /api/cargas/{id}/objetos` devuelve solo los objetos de esa carga. CA-005.2 Carga inexistente → 404. CA-005.3 `GET /api/objetos?cargaId=` filtra igual. | CP-OBJ-005, CP-OBJ-006 |
| RF-006 | Filtrar categorías por tipo y estado. | CA-006.1 `?tipoCategoria=X` devuelve solo el tipo X (sin distinguir mayúsculas). CA-006.2 Tipo no permitido → 400. CA-006.3 `?activo=true/false` filtra por estado. | CP-CAT-002, CP-CAT-003, CP-CAT-004 |
| RF-007 | Filtrar parámetros por categoría y estado. | CA-007.1 `?categoriaId=` y `?activo=` filtran correctamente. | CP-PAR-002 |
| RF-008 | Interfaz web de demostración que consume la API. | CA-008.1 Páginas de inicio, cargas, objetos, categorías y parámetros. CA-008.2 Formularios que solo ofrecen categorías del tipo correcto. CA-008.3 Muestra los errores de la API por campo. CA-008.4 No accede directamente a SQLite. | CP-UI-001 a CP-UI-008 |
| RF-009 | Persistencia en SQLite. | CA-009.1 Los datos sobreviven a un reinicio. CA-009.2 La inicialización no borra datos y siembra ejemplos solo si la base está vacía. | CP-INT-001 |
| RF-010 | Respuestas de error uniformes. | CA-010.1 Todo error usa ProblemDetails (`application/problem+json`) con `status`, `title` y `traceId`. CA-010.2 Los errores 400 incluyen `errors` por campo (nombres en camelCase). | Verificación general de Postman en todas las solicitudes; xUnit en todos los casos negativos |

## 2. Requisitos no funcionales

| ID | Requisito | Criterio de aceptación | Casos |
|---|---|---|---|
| RNF-001 | Seguridad de la información de errores. | No se exponen excepciones, pilas de llamadas, nombres de tipos .NET ni cadenas de conexión. | CP-INT-005, CP-CAR-018 y la prueba general de Postman |
| RNF-002 | Base de datos de pruebas aislada. | Las pruebas automatizadas usan un archivo SQLite temporal distinto de `App_Data/transporte.db`. | CP-INT-006 |
| RNF-003 | Integridad en la base de datos. | Las claves foráneas (Restrict) y las restricciones CHECK protegen los datos aunque se omita la API. | CP-INT-002, CP-INT-003 |
| RNF-004 | Tiempo de respuesta en el entorno local. | Cada solicitud de la colección responde en menos de 3000 ms. | Prueba general de Postman |
| RNF-005 | Ejecución en Visual Studio. | La solución `.sln` se abre y ejecuta con el perfil `http` (puerto 5080). | Verificación manual pendiente (ver informe) |
| RNF-006 | Reproducibilidad de la colección. | La colección no depende de identificadores fijos, elimina lo que crea y puede ejecutarse varias veces. | Ejecuciones Newman 2, 3 y 4 |

## 3. Reglas de negocio

Las reglas están centralizadas en `src/TransporteCargaPesada.Web/Services` (servicios) y `Services/Reglas`
(tipos permitidos, validaciones y parámetros del sistema). La interfaz y las pruebas no duplican la lógica:
la interfaz llama a la API y las pruebas verifican las respuestas.

| ID | Regla | Implementación | Criterio de aceptación | Casos |
|---|---|---|---|---|
| RN-01 | El código de carga es único. | `CargaService` + índice único `IX_Cargas_Codigo`. | Duplicado (sin importar mayúsculas) → 409. | CP-CAR-010 |
| RN-02 | Los campos obligatorios no pueden ir vacíos ni contener solo espacios. | `ReglasValidacion.Limpiar/Texto`. | 400 con un error por cada campo faltante. | CP-CAR-006, CP-OBJ-011, CP-CAT-009, CP-PAR-009 |
| RN-03 | El origen y el destino tienen entre 3 y 100 caracteres y son distintos. | `CargaService.ValidarAsync`. | Origen = destino (sin distinguir mayúsculas) o menos de 3 caracteres → 400. | CP-CAR-009 |
| RN-04 | El peso y el volumen no son negativos; el peso es mayor que 0. | `ReglasValidacion.Positivo` + CHECK en SQLite. | Valores ≤ 0 → 400. | CP-CAR-007, CP-CAR-008, CP-OBJ-010, CP-INT-003 |
| RN-05 | La cantidad de un objeto es mayor que 0. | `ReglasValidacion.Positivo` + CHECK. | 0 o negativa → 400; decimal → 400. | CP-OBJ-009, CP-OBJ-014 |
| RN-06 | Los identificadores relacionados existen. | `ValidadorReferencias`, `ObjetoService`. | Id inexistente en el cuerpo → 400 en ese campo. | CP-CAR-013, CP-OBJ-008, CP-PAR-013 |
| RN-07 | El tipo de categoría corresponde a su uso. | `ValidadorReferencias.ValidarCategoriaAsync` compara `TipoCategoria`. | `VEHICULO` en `tipoCargaId` → 400 con mensaje explicativo. | CP-CAR-011, CP-CAR-012, CP-OBJ-012, CP-PAR-014 |
| RN-08 | Una carga no referencia categorías inexistentes ni inactivas. | `ValidadorReferencias`. | Categoría inactiva en una carga nueva → 400; si ya estaba asignada, se conserva al actualizar. | CP-CAR-013, CP-CAR-014, CP-CAR-025 |
| RN-09 | Un objeto no pertenece a una carga inexistente. | `ObjetoService` + FK. | `cargaId` inexistente → 400. | CP-OBJ-008 |
| RN-10 | El valor de un parámetro respeta su tipo de dato. | `TiposDato.TryNormalizar`. | ENTERO, DECIMAL (punto decimal), BOOLEANO (true/false) y FECHA (yyyy-MM-dd); los valores incompatibles → 400. | CP-PAR-003, CP-PAR-007, CP-PAR-008 |
| RN-11 | El código de categoría es único dentro de su tipo. | Índice único (`TipoCategoria`, `Codigo`). | Duplicado en el mismo tipo → 409; el mismo código en otro tipo → 201. | CP-CAT-011, CP-CAT-012 |
| RN-12 | La eliminación respeta las dependencias. | FK `Restrict` en todas las relaciones. | Ninguna eliminación deja referencias huérfanas. | CP-INT-002, CP-INT-004 |
| RN-13 | No se elimina una carga con objetos. | `CargaService.EliminarAsync`. | 409; la carga y los objetos se conservan. | CP-CAR-022 |
| RN-14 | Política de categorías en uso: **no se eliminan, se desactivan**. Tampoco cambian de tipo. | `CategoriaService`. | Eliminar una categoría en uso → 409; desactivarla → 200; cambiar su tipo → 409. Una categoría inactiva no se ofrece para registros nuevos. | CP-CAT-013, CP-CAT-014, CP-CAT-015 |
| RN-15 | Códigos HTTP coherentes. | `ApiControllerBase.Responder`. | 200, 201, 204, 400, 404, 409 y 500 según el catálogo de endpoints. | Todos |
| RN-16 | No se exponen detalles internos. | `RespuestasError` + `AllowInputFormatterExceptionMessages = false`. | Ver RNF-001. | CP-INT-005, CP-CAR-018 |
| RN-17 | Límites configurables de peso y volumen. | `CargaService` lee `PESO_MAXIMO_CARGA_KG`, `VOLUMEN_MAXIMO_CARGA_M3` y `CAPACIDAD_KG_*` de la categoría del vehículo (solo los activos). | Superar el límite → 400; igual al límite → 201. Los límites deben ser numéricos y mayores que 0. | CP-CAR-015, CP-CAR-016, CP-CAR-017, CP-PAR-015 |
| RN-18 | El peso de los objetos no supera el peso declarado de la carga. | `ObjetoService.ValidarPesoAsync`, `CargaService.ActualizarAsync`. | Agregar, mover o aumentar un objeto por encima del peso declarado → 409; reducir el peso de la carga por debajo del de sus objetos → 409. | CP-OBJ-013, CP-OBJ-016, CP-CAR-023 |
| RN-19 | Política de parámetros en uso: los **obligatorios** no se eliminan, no se desactivan y no cambian su código ni su tipo de dato. Los no obligatorios se pueden eliminar o desactivar. | `ParametroService`. | 409 en cada caso prohibido. | CP-PAR-011, CP-PAR-012 |

## 4. Decisiones ante ambigüedades

| ID | Ambigüedad | Decisión |
|---|---|---|
| D-01 | ¿Peso cero es válido? | No: una carga o un objeto sin peso no tiene sentido operativo. El peso debe ser mayor que 0; el volumen es opcional, pero si se indica también debe ser mayor que 0. |
| D-02 | ¿Qué código HTTP usar para referencias inexistentes en el cuerpo? | 400 (dato inválido en el campo). Se reserva 404 para el recurso de la ruta. |
| D-03 | ¿400 o 409 cuando se supera el peso de la carga con objetos? | 409: la solicitud es válida por sí sola, pero entra en conflicto con el estado de otra entidad. Superar un parámetro de límite es 400, porque el valor es inválido según la configuración. |
| D-04 | `PUT` parcial o total. | Total: se reemplazan todos los campos editables. Las excepciones son `activo` y `esObligatorio`: si se omiten conservan su valor. En las cargas, `estadoId` es obligatorio al actualizar. |
| D-05 | Unicidad de códigos de parámetro. | Global (un mismo código no se repite aunque cambie la categoría). |
| D-06 | Unidad de los pesos. | Siempre kilogramos (S-02); `UnidadMedida` del parámetro es informativa. |
| D-07 | Categorías del sistema (por ejemplo `PENDIENTE`). | Pueden editarse; si `PENDIENTE` no existe o está inactiva, crear una carga sin `estadoId` devuelve 400 explicativo. |
