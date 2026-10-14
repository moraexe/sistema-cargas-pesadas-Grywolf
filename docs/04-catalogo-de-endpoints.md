# 04 · Catálogo de endpoints

- URL base local: `http://localhost:5080` (perfil `http` de `Properties/launchSettings.json`). El perfil `https` usa además `https://localhost:7080`.
- Ruta base de la API: `/api`. Las rutas no distinguen mayúsculas de minúsculas.
- Formato: JSON con propiedades en camelCase. Las fechas se devuelven en UTC (ISO 8601 con `Z`).
- Los `{id}` son enteros (`{id:int}`). Un valor no numérico no coincide con ninguna ruta y devuelve 404 sin cuerpo.
- Autenticación: no aplica (exclusión del alcance).

## 1. Formato de errores

Todos los errores usan **ProblemDetails** (RFC 9457) con `Content-Type: application/problem+json`.

**400 · Validación** (`errors` por campo, con nombres en camelCase igual que en el JSON):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Uno o más datos no son válidos.",
  "status": 400,
  "errors": {
    "tipoCargaId": ["La categoría 1 (TRACTOCAMION) es de tipo VEHICULO y no puede usarse como TIPO_CARGA."],
    "pesoTotalKg": ["El campo pesoTotalKg debe ser mayor que cero."]
  },
  "traceId": "00-6b6e23ea9d31bcdf9e93433d1baec9ad-2ecb0c2b0269b22a-00"
}
```

Casos especiales de 400:

| Situación | Clave en `errors` | Mensaje |
|---|---|---|
| JSON mal formado | `cuerpo` | El cuerpo de la solicitud no es un JSON válido. |
| Cuerpo vacío | `cuerpo` | El cuerpo de la solicitud es obligatorio y debe ser un JSON válido. |
| Tipo de dato incorrecto (`"pesoTotalKg": "mil"`) | `pesoTotalKg` | El valor tiene un tipo de dato incorrecto o el JSON está mal formado en este campo. |
| Parámetro de consulta inválido (`?activo=quizas`) | `activo` | El valor 'quizas' no es válido para el parámetro activo. |

**404 y 409** (`detail` explica la causa):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflicto con el estado actual de los datos.",
  "status": 409,
  "detail": "La carga CP-2026-0001 tiene 2 objeto(s) asociado(s) y no puede eliminarse. Elimine primero sus objetos.",
  "traceId": "00-0f20c6fd91c4a432d8c3906097517aa1-400447be26fd9b8b-00"
}
```

**500**: `title` = "Error interno del servidor." y `detail` genérico. El detalle de la excepción queda solo en el log del servidor.

## 2. Códigos HTTP

| Código | Uso |
|---|---|
| 200 OK | Consulta o actualización exitosa. |
| 201 Created | Creación exitosa; incluye el encabezado `Location` y el recurso creado. |
| 204 No Content | Eliminación exitosa. |
| 400 Bad Request | Datos inválidos, campos obligatorios vacíos, tipo de categoría incorrecto, referencias inexistentes en el cuerpo, límites configurables superados, JSON mal formado o filtro inválido. |
| 404 Not Found | El recurso de la ruta no existe. |
| 409 Conflict | Código duplicado, eliminación con dependencias, cambio de tipo de una categoría en uso, operación prohibida sobre un parámetro obligatorio o peso de objetos que supera el peso declarado. |
| 500 Internal Server Error | Error inesperado (sin detalles internos). |

## 3. Cargas

| Método | Ruta | Descripción | Cuerpo | Respuestas | Reglas |
|---|---|---|---|---|---|
| GET | `/api/cargas` | Lista todas las cargas. | — | 200 `CargaResponse[]` | — |
| GET | `/api/cargas/{id}` | Obtiene una carga. | — | 200, 404 | — |
| GET | `/api/cargas/{id}/objetos` | Lista los objetos de una carga. | — | 200 `ObjetoResponse[]`, 404 | RF-005 |
| POST | `/api/cargas` | Crea una carga. | `CargaRequest` | 201, 400, 409 | RN-01 a RN-08, RN-17 |
| PUT | `/api/cargas/{id}` | Reemplaza una carga. | `CargaRequest` (con `estadoId`) | 200, 400, 404, 409 | RN-01 a RN-08, RN-17, RN-18 |
| DELETE | `/api/cargas/{id}` | Elimina una carga sin objetos. | — | 204, 404, 409 | RN-13 |

**CargaRequest**

```json
{
  "codigo": "CP-2026-0003",
  "descripcion": "Traslado de turbina",
  "origen": "Cali",
  "destino": "Pasto",
  "pesoTotalKg": 12500.5,
  "volumenTotalM3": 40,
  "tipoCargaId": 6,
  "tipoVehiculoId": 3,
  "tipoEntregaId": 8,
  "estadoId": 9,
  "observaciones": "Opcional"
}
```

| Campo | Obligatorio | Validación |
|---|---|---|
| codigo | Sí | 3 a 20 caracteres `A-Z 0-9 -`, se normaliza a mayúsculas, único. |
| descripcion | Sí | 3 a 300 caracteres. |
| origen, destino | Sí | 3 a 100 caracteres y distintos entre sí. |
| pesoTotalKg | Sí | > 0, ≤ `PESO_MAXIMO_CARGA_KG` y ≤ `CAPACIDAD_KG_*` del vehículo. Al actualizar, ≥ peso de los objetos. |
| volumenTotalM3 | No | > 0 y ≤ `VOLUMEN_MAXIMO_CARGA_M3`. |
| tipoCargaId | Sí | Categoría `TIPO_CARGA` existente y activa. |
| tipoVehiculoId | No | Categoría `VEHICULO` existente y activa. |
| tipoEntregaId | Sí | Categoría `TIPO_ENTREGA` existente y activa. |
| estadoId | Al crear: no (por defecto `PENDIENTE`). Al actualizar: sí. | Categoría `ESTADO_CARGA` existente y activa. |
| observaciones | No | ≤ 500 caracteres. |

Una categoría inactiva se acepta al actualizar solo si ya era la asignada (conserva el historial).

**CargaResponse**: `id`, `codigo`, `descripcion`, `origen`, `destino`, `pesoTotalKg`, `volumenTotalM3`, `tipoCargaId`, `tipoCargaNombre`,
`tipoVehiculoId`, `tipoVehiculoNombre`, `tipoEntregaId`, `tipoEntregaNombre`, `estadoId`, `estadoCodigo`, `estadoNombre`,
`fechaRegistro`, `observaciones`, `cantidadObjetos` y `pesoObjetosKg`.

## 4. Objetos

| Método | Ruta | Descripción | Cuerpo | Respuestas | Reglas |
|---|---|---|---|---|---|
| GET | `/api/objetos?cargaId={id}` | Lista los objetos; `cargaId` es opcional. | — | 200 | — |
| GET | `/api/objetos/{id}` | Obtiene un objeto. | — | 200, 404 | — |
| POST | `/api/objetos` | Crea un objeto. | `ObjetoRequest` | 201, 400, 409 | RN-02, RN-04 a RN-07, RN-09, RN-18 |
| PUT | `/api/objetos/{id}` | Reemplaza un objeto (puede moverlo a otra carga). | `ObjetoRequest` | 200, 400, 404, 409 | Ídem |
| DELETE | `/api/objetos/{id}` | Elimina un objeto. | — | 204, 404 | — |

**ObjetoRequest**

```json
{ "cargaId": 1, "nombre": "Excavadora", "descripcion": "Opcional", "cantidad": 1,
  "pesoUnitarioKg": 21000, "volumenUnitarioM3": 45, "categoriaId": 15, "observaciones": "Opcional" }
```

| Campo | Obligatorio | Validación |
|---|---|---|
| cargaId | Sí | Carga existente (si no existe → 400). |
| nombre | Sí | 2 a 100 caracteres. |
| cantidad | Sí | Entero > 0 (≤ 100 000). |
| pesoUnitarioKg | Sí | > 0. |
| volumenUnitarioM3 | No | > 0. |
| categoriaId | No | Categoría `TIPO_OBJETO` existente y activa. |

Regla RN-18: la suma de `cantidad × pesoUnitarioKg` de los objetos de la carga no puede superar `pesoTotalKg`; si la supera → 409.

**ObjetoResponse**: `id`, `cargaId`, `cargaCodigo`, `nombre`, `descripcion`, `cantidad`, `pesoUnitarioKg`, `volumenUnitarioM3`,
`pesoTotalKg` (calculado), `categoriaId`, `categoriaNombre` y `observaciones`.

## 5. Categorías

| Método | Ruta | Descripción | Cuerpo | Respuestas | Reglas |
|---|---|---|---|---|---|
| GET | `/api/categorias?tipoCategoria=&activo=` | Lista las categorías con filtros opcionales. | — | 200; 400 si el tipo no está permitido o `activo` no es booleano | RF-006 |
| GET | `/api/categorias/tipos` | Lista los tipos permitidos. | — | 200 `[{codigo, descripcion}]` | RN-07 |
| GET | `/api/categorias/{id}` | Obtiene una categoría. | — | 200, 404 | — |
| POST | `/api/categorias` | Crea una categoría. | `CategoriaRequest` | 201, 400, 409 | RN-02, RN-07, RN-11 |
| PUT | `/api/categorias/{id}` | Reemplaza una categoría (incluye activar o desactivar). | `CategoriaRequest` | 200, 400, 404, 409 | RN-11, RN-14 |
| DELETE | `/api/categorias/{id}` | Elimina una categoría sin uso. | — | 204, 404, 409 | RN-14 |

Ejemplos de filtros: `GET /api/categorias?tipoCategoria=VEHICULO`, `?tipoCategoria=TIPO_CARGA`, `?tipoCategoria=TIPO_ENTREGA`
y `?activo=false`. Un `tipoCategoria` vacío equivale a no filtrar.

**CategoriaRequest**

```json
{ "tipoCategoria": "VEHICULO", "codigo": "VOLQUETA", "nombre": "Volqueta", "descripcion": "Opcional", "activo": true }
```

`activo` es opcional: al crear vale true y al actualizar conserva el valor actual.

**Política de eliminación (RN-14):** si la categoría está referenciada por cargas, objetos o parámetros → 409, con el número de
usos y la sugerencia de desactivarla. Cambiar el `tipoCategoria` de una categoría en uso → 409.

## 6. Parámetros

| Método | Ruta | Descripción | Cuerpo | Respuestas | Reglas |
|---|---|---|---|---|---|
| GET | `/api/parametros?categoriaId=&activo=` | Lista los parámetros con filtros opcionales. | — | 200 | RF-007 |
| GET | `/api/parametros/tipos-dato` | Lista los tipos de dato permitidos. | — | 200 | RN-10 |
| GET | `/api/parametros/{id}` | Obtiene un parámetro. | — | 200, 404 | — |
| POST | `/api/parametros` | Crea un parámetro. | `ParametroRequest` | 201, 400, 409 | RN-10, RN-17 |
| PUT | `/api/parametros/{id}` | Reemplaza un parámetro. | `ParametroRequest` | 200, 400, 404, 409 | RN-10, RN-17, RN-19 |
| DELETE | `/api/parametros/{id}` | Elimina un parámetro no obligatorio. | — | 204, 404, 409 | RN-19 |

**ParametroRequest**

```json
{ "codigo": "CAPACIDAD_KG_CAMA_BAJA", "nombre": "Capacidad cama baja", "descripcion": "Opcional",
  "tipoDato": "DECIMAL", "valor": "45000", "unidadMedida": "kg", "categoriaId": 3,
  "esObligatorio": false, "activo": true }
```

| tipoDato | Valores válidos | Valores inválidos (→ 400) |
|---|---|---|
| ENTERO | `42`, `-7` | `abc`, `12.5` |
| DECIMAL | `40000`, `12.75` (punto decimal) | `12,5`, `doce` |
| BOOLEANO | `true`, `FALSE` | `si`, `1` |
| FECHA | `2026-12-31` | `31/12/2026`, `2026-02-30` |
| TEXTO | Cualquier texto de 1 a 200 caracteres | Vacío |

**ParametroResponse** agrega `ambito` (`GENERAL` / `CATEGORIA`), `categoriaTipo`, `categoriaCodigo` y `fechaActualizacion`.

## 7. Interfaz web (Razor Pages)

| Ruta | Página | Endpoints que consume |
|---|---|---|
| `/` | Inicio: indicadores, cargas por estado y límites configurados | GET de las cuatro entidades |
| `/Cargas` | CRUD de cargas | `/api/cargas`, `/api/categorias?tipoCategoria=` |
| `/Objetos?cargaId=` | CRUD de objetos y filtro por carga | `/api/objetos`, `/api/cargas/{id}/objetos`, `/api/categorias?tipoCategoria=TIPO_OBJETO` |
| `/Categorias` | CRUD, filtros, activar o desactivar | `/api/categorias`, `/api/categorias/tipos` |
| `/Parametros` | CRUD y filtros | `/api/parametros`, `/api/parametros/tipos-dato`, `/api/categorias` |

Las páginas usan `fetch` contra la API (`wwwroot/js`). No contienen reglas de negocio ni acceden a la base de datos.
