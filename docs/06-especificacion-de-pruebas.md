# 06 · Especificación de pruebas

Especificación de casos de prueba basada en la estructura de ISO/IEC/IEEE 29119-3. Los casos se diseñaron con partición de
equivalencia, análisis de valores límite, tablas de decisión y escenarios (ver [05-plan-de-pruebas.md](05-plan-de-pruebas.md)).

**Trazabilidad con el código de prueba**

- xUnit: el `DisplayName` de cada prueba empieza con el ID del caso (`tests/TransporteCargaPesada.Tests/*Tests.cs`).
  Un caso con varias variantes se implementa como `[Theory]`.
- Postman: el nombre de cada solicitud empieza con el ID del caso (`postman/TransporteCargaPesada.postman_collection.json`).
  Un caso puede incluir varias solicitudes (por ejemplo, eliminar y verificar).
- Interfaz: script CDP que identifica cada resultado con el ID del caso.

**Criterio de aprobación general:** el caso se aprueba si en **todas** sus variantes y herramientas el código HTTP y el contenido
coinciden con el resultado esperado. En las respuestas de error se exige además formato ProblemDetails
(`application/problem+json`) sin detalles internos. Esta comprobación general la aplica Postman a todas las solicitudes con código ≥ 400,
y xUnit a todas las respuestas 400, 404 y 409.

## Resumen

| Grupo | Casos | Funcionales | Negativos | Integración | Interfaz | Prioridad alta |
|---|---|---|---|---|---|---|
| Categorías | 18 | 10 | 8 | 0 | 0 | 10 |
| Parámetros | 17 | 7 | 10 | 0 | 0 | 7 |
| Cargas | 25 | 6 | 19 | 0 | 0 | 17 |
| Objetos | 16 | 6 | 10 | 0 | 0 | 10 |
| Integración | 6 | 0 | 0 | 6 | 0 | 5 |
| Interfaz web | 8 | 0 | 0 | 0 | 8 | 3 |
| **Total** | **90** | **29** | **47** | **6** | **8** | **52** |

## Matriz de trazabilidad (requisito → casos)

| Requisito / regla | Casos |
|---|---|
| RF-001 | CP-CAR-001, CP-CAR-002, CP-CAR-003, CP-CAR-004, CP-CAR-005, CP-CAR-020, CP-CAR-021 |
| RF-002 | CP-OBJ-001, CP-OBJ-002, CP-OBJ-003, CP-OBJ-004, CP-OBJ-007, CP-OBJ-015 |
| RF-003 | CP-CAT-001, CP-CAT-005, CP-CAT-006, CP-CAT-007, CP-CAT-008, CP-CAT-016, CP-CAT-018 |
| RF-004 | CP-PAR-001, CP-PAR-003, CP-PAR-004, CP-PAR-005, CP-PAR-006, CP-PAR-016, CP-PAR-017 |
| RF-005 | CP-OBJ-005, CP-OBJ-006 |
| RF-006 | CP-CAT-002, CP-CAT-003, CP-CAT-004 |
| RF-007 | CP-PAR-002 |
| RF-008 | CP-UI-001, CP-UI-002, CP-UI-003, CP-UI-004, CP-UI-005, CP-UI-006, CP-UI-007, CP-UI-008 |
| RF-009 | CP-INT-001 |
| RNF-001 | CP-INT-005 |
| RNF-002 | CP-INT-006 |
| RNF-003 | CP-INT-002, CP-INT-003 |
| RN-01 | CP-PAR-010, CP-CAR-010 |
| RN-02 | CP-CAT-009, CP-CAT-017, CP-PAR-009, CP-CAR-006, CP-OBJ-011 |
| RN-03 | CP-CAR-009 |
| RN-04 | CP-CAR-007, CP-CAR-008, CP-OBJ-010, CP-INT-003 |
| RN-05 | CP-OBJ-009, CP-OBJ-014, CP-INT-003 |
| RN-06 | CP-PAR-013, CP-CAR-013, CP-OBJ-008 |
| RN-07 | CP-CAT-010, CP-CAT-018, CP-PAR-014, CP-CAR-011, CP-CAR-012, CP-OBJ-012, CP-INT-003 |
| RN-08 | CP-CAR-011, CP-CAR-013, CP-CAR-014, CP-CAR-025 |
| RN-09 | CP-OBJ-008 |
| RN-10 | CP-PAR-003, CP-PAR-007, CP-PAR-008 |
| RN-11 | CP-CAT-011, CP-CAT-012, CP-CAT-017 |
| RN-12 | CP-CAT-013, CP-CAT-016, CP-CAR-021, CP-INT-002, CP-INT-004 |
| RN-13 | CP-CAR-022, CP-INT-004 |
| RN-14 | CP-CAT-013, CP-CAT-014, CP-CAT-015, CP-CAR-014, CP-CAR-025, CP-INT-004 |
| RN-15 | CP-CAT-007, CP-PAR-005, CP-CAR-004, CP-CAR-018, CP-CAR-019, CP-CAR-020, CP-CAR-024, CP-OBJ-004, CP-OBJ-014 |
| RN-16 | CP-CAR-018, CP-INT-005 |
| RN-17 | CP-PAR-014, CP-PAR-015, CP-CAR-015, CP-CAR-016, CP-CAR-017 |
| RN-18 | CP-CAR-023, CP-OBJ-013, CP-OBJ-016, CP-INT-004 |
| RN-19 | CP-PAR-011, CP-PAR-012, CP-PAR-016 |

## Categorías

### CP-CAT-001 · Listar categorías

| Campo | Valor |
|---|---|
| ID | CP-CAT-001 |
| Nombre | Listar categorías |
| Requisito relacionado | RF-003 |
| Objetivo | Verificar la consulta general del catálogo. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | — |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias`.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; lista JSON no vacía con id, tipoCategoria, codigo, nombre y activo. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-002 · Filtrar categorías por tipo

| Campo | Valor |
|---|---|
| ID | CP-CAT-002 |
| Nombre | Filtrar categorías por tipo |
| Requisito relacionado | RF-006 |
| Objetivo | Verificar que el filtro devuelve solo categorías del tipo solicitado, sin distinguir mayúsculas. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipo ∈ {VEHICULO, TIPO_CARGA, TIPO_ENTREGA, tipo_carga} |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias?tipoCategoria={tipo}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; todos los elementos tienen tipoCategoria = tipo en mayúsculas. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-003 · Filtrar por un tipo inexistente

| Campo | Valor |
|---|---|
| ID | CP-CAT-003 |
| Nombre | Filtrar por un tipo inexistente |
| Requisito relacionado | RF-006 |
| Objetivo | Verificar la validación del parámetro de filtro. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipoCategoria = AVION |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias?tipoCategoria=AVION` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 ProblemDetails con error en tipoCategoria y la lista de valores permitidos. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-004 · Filtrar categorías por estado activo

| Campo | Valor |
|---|---|
| ID | CP-CAT-004 |
| Nombre | Filtrar categorías por estado activo |
| Requisito relacionado | RF-006 |
| Objetivo | Verificar el filtro activo=true/false. |
| Precondiciones | Existe al menos una categoría inactiva (xUnit la crea). |
| Datos de entrada | activo = true / false |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias?activo={true\|false}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; todas las categorías devueltas tienen el estado solicitado. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-005 · Crear categoría válida

| Campo | Valor |
|---|---|
| ID | CP-CAT-005 |
| Nombre | Crear categoría válida |
| Requisito relacionado | RF-003 |
| Objetivo | Verificar la creación y la normalización del código y el tipo. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipoCategoria "vehiculo", codigo " volqueta_xxx " (minúsculas y espacios), nombre |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/categorias` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 201; Location /api/categorias/{id}; tipoCategoria y codigo en mayúsculas sin espacios; activo = true. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-006 · Consultar categoría por id

| Campo | Valor |
|---|---|
| ID | CP-CAT-006 |
| Nombre | Consultar categoría por id |
| Requisito relacionado | RF-003 |
| Objetivo | Verificar la consulta individual. |
| Precondiciones | Categoría creada en el caso. |
| Datos de entrada | id existente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con el mismo id y código. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-007 · Consultar categoría inexistente

| Campo | Valor |
|---|---|
| ID | CP-CAT-007 |
| Nombre | Consultar categoría inexistente |
| Requisito relacionado | RF-003, RN-15 |
| Objetivo | Verificar la respuesta ante un id inexistente. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | id = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias/999999` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails; detail menciona el id. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-008 · Actualizar categoría

| Campo | Valor |
|---|---|
| ID | CP-CAT-008 |
| Nombre | Actualizar categoría |
| Requisito relacionado | RF-003 |
| Objetivo | Verificar la actualización y que activo omitido conserva su valor. |
| Precondiciones | Categoría creada en el caso. |
| Datos de entrada | nombre nuevo, sin "activo" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/categorias/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con el nombre nuevo y activo = true. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-009 · Crear categoría con campos obligatorios vacíos

| Campo | Valor |
|---|---|
| ID | CP-CAT-009 |
| Nombre | Crear categoría con campos obligatorios vacíos |
| Requisito relacionado | RN-02 |
| Objetivo | Verificar la validación de campos obligatorios vacíos o con espacios. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipoCategoria "", codigo "   ", nombre "" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/categorias` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con errores en tipoCategoria, codigo y nombre. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-010 · Crear categoría con tipo no permitido

| Campo | Valor |
|---|---|
| ID | CP-CAT-010 |
| Nombre | Crear categoría con tipo no permitido |
| Requisito relacionado | RN-07 |
| Objetivo | Verificar la validación centralizada de TipoCategoria. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipoCategoria = COLOR |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/categorias` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en tipoCategoria. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-011 · Código duplicado en el mismo tipo

| Campo | Valor |
|---|---|
| ID | CP-CAT-011 |
| Nombre | Código duplicado en el mismo tipo |
| Requisito relacionado | RN-11 |
| Objetivo | Verificar la unicidad del código dentro del tipo. |
| Precondiciones | Existe VEHICULO/TRACTOCAMION (o la categoría creada por Postman). |
| Datos de entrada | VEHICULO + código existente en minúsculas |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/categorias` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 ProblemDetails; detail menciona el código. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-012 · Mismo código en otro tipo

| Campo | Valor |
|---|---|
| ID | CP-CAT-012 |
| Nombre | Mismo código en otro tipo |
| Requisito relacionado | RN-11 |
| Objetivo | Verificar que la unicidad es por tipo y no global. |
| Precondiciones | Existe TIPO_CARGA/GENERAL. |
| Datos de entrada | TIPO_OBJETO + código GENERAL |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/categorias` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 201 Created. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-013 · Eliminar categoría en uso

| Campo | Valor |
|---|---|
| ID | CP-CAT-013 |
| Nombre | Eliminar categoría en uso |
| Requisito relacionado | RN-12, RN-14 |
| Objetivo | Verificar la política de eliminación de categorías referenciadas. |
| Precondiciones | Categoría referenciada por al menos una carga. |
| Datos de entrada | id de una categoría en uso |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/categorias/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409; detail con el número de usos y la sugerencia de desactivar; la categoría sigue existiendo. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-014 · Desactivar categoría en uso

| Campo | Valor |
|---|---|
| ID | CP-CAT-014 |
| Nombre | Desactivar categoría en uso |
| Requisito relacionado | RN-14 |
| Objetivo | Verificar la alternativa de desactivación que conserva el historial. |
| Precondiciones | Categoría referenciada por una carga. |
| Datos de entrada | mismos datos con activo = false |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/categorias/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con activo = false; la carga conserva su referencia. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-015 · Cambiar el tipo de una categoría en uso

| Campo | Valor |
|---|---|
| ID | CP-CAT-015 |
| Nombre | Cambiar el tipo de una categoría en uso |
| Requisito relacionado | RN-14 |
| Objetivo | Verificar que no se rompe la coherencia de tipo de los registros existentes. |
| Precondiciones | Categoría TIPO_CARGA referenciada por una carga. |
| Datos de entrada | tipoCategoria = VEHICULO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/categorias/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 Conflict. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-016 · Eliminar categoría sin uso

| Campo | Valor |
|---|---|
| ID | CP-CAT-016 |
| Nombre | Eliminar categoría sin uso |
| Requisito relacionado | RF-003, RN-12 |
| Objetivo | Verificar la eliminación física cuando no hay dependencias. |
| Precondiciones | Categoría creada en el caso, sin referencias. |
| Datos de entrada | id sin uso |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/categorias/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 204; un GET posterior devuelve 404. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-017 · Código de categoría con formato inválido

| Campo | Valor |
|---|---|
| ID | CP-CAT-017 |
| Nombre | Código de categoría con formato inválido |
| Requisito relacionado | RN-02, RN-11 |
| Objetivo | Verificar el formato del código (partición de equivalencia). |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | codigo ∈ {"1ABC", "CON ESPACIO", "CON-GUION", "X", "1 CODIGO-MAL"} |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/categorias` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en codigo. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAT-018 · Consultar los tipos de categoría permitidos

| Campo | Valor |
|---|---|
| ID | CP-CAT-018 |
| Nombre | Consultar los tipos de categoría permitidos |
| Requisito relacionado | RF-003, RN-07 |
| Objetivo | Verificar que la lista centralizada se expone a los clientes. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | — |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/categorias/tipos`.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con VEHICULO, TIPO_CARGA, TIPO_ENTREGA, ESTADO_CARGA, UNIDAD_MEDIDA y TIPO_OBJETO. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

## Parámetros

### CP-PAR-001 · Listar parámetros

| Campo | Valor |
|---|---|
| ID | CP-PAR-001 |
| Nombre | Listar parámetros |
| Requisito relacionado | RF-004 |
| Objetivo | Verificar la consulta y la distinción entre parámetros generales y de categoría. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | — |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/parametros`.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; hay parámetros con ambito GENERAL (categoriaId null) y CATEGORIA. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-002 · Filtrar parámetros por categoría y estado

| Campo | Valor |
|---|---|
| ID | CP-PAR-002 |
| Nombre | Filtrar parámetros por categoría y estado |
| Requisito relacionado | RF-007 |
| Objetivo | Verificar los filtros categoriaId y activo. |
| Precondiciones | Existe un parámetro asociado a la categoría y uno inactivo. |
| Datos de entrada | categoriaId de un VEHICULO; activo = false |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/parametros?categoriaId={id}&activo={bool}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; todos los elementos cumplen el filtro. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-003 · Crear parámetro con valor compatible

| Campo | Valor |
|---|---|
| ID | CP-PAR-003 |
| Nombre | Crear parámetro con valor compatible |
| Requisito relacionado | RF-004, RN-10 |
| Objetivo | Verificar la creación y la normalización por tipo de dato. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | DECIMAL " 1500.50 ", ENTERO "42", BOOLEANO "TRUE", FECHA "2026-12-31", TEXTO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 201; valor canónico (1500.50, 42, true, 2026-12-31); esObligatorio = false; ambito GENERAL. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-004 · Consultar parámetro por id

| Campo | Valor |
|---|---|
| ID | CP-PAR-004 |
| Nombre | Consultar parámetro por id |
| Requisito relacionado | RF-004 |
| Objetivo | Verificar la consulta individual. |
| Precondiciones | Parámetro creado en el caso. |
| Datos de entrada | id existente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/parametros/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con el mismo código. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-005 · Consultar parámetro inexistente

| Campo | Valor |
|---|---|
| ID | CP-PAR-005 |
| Nombre | Consultar parámetro inexistente |
| Requisito relacionado | RF-004, RN-15 |
| Objetivo | Verificar la respuesta ante un id inexistente. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | id = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/parametros/999999` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-006 · Actualizar parámetro

| Campo | Valor |
|---|---|
| ID | CP-PAR-006 |
| Nombre | Actualizar parámetro |
| Requisito relacionado | RF-004 |
| Objetivo | Verificar la actualización del valor y de FechaActualizacion. |
| Precondiciones | Parámetro creado en el caso. |
| Datos de entrada | valor "20.5" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/parametros/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con valor 20.5 y FechaActualizacion ≥ la anterior. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-007 · Valor incompatible con el tipo de dato

| Campo | Valor |
|---|---|
| ID | CP-PAR-007 |
| Nombre | Valor incompatible con el tipo de dato |
| Requisito relacionado | RN-10 |
| Objetivo | Verificar que no se aceptan textos arbitrarios en parámetros tipados. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | ENTERO "abc" y "12.5"; DECIMAL "12,5" y "doce"; BOOLEANO "si"; FECHA "31/12/2026" y "2026-02-30" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en valor que menciona el tipo de dato. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-008 · Tipo de dato no permitido

| Campo | Valor |
|---|---|
| ID | CP-PAR-008 |
| Nombre | Tipo de dato no permitido |
| Requisito relacionado | RN-10 |
| Objetivo | Verificar la lista de tipos de dato. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipoDato = MONEDA |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en tipoDato. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-009 · Parámetro con campos obligatorios vacíos

| Campo | Valor |
|---|---|
| ID | CP-PAR-009 |
| Nombre | Parámetro con campos obligatorios vacíos |
| Requisito relacionado | RN-02 |
| Objetivo | Verificar los campos obligatorios. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | codigo "", nombre " " |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con errores en codigo, nombre, tipoDato y valor. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-010 · Código de parámetro duplicado

| Campo | Valor |
|---|---|
| ID | CP-PAR-010 |
| Nombre | Código de parámetro duplicado |
| Requisito relacionado | RN-01 (análoga) |
| Objetivo | Verificar la unicidad global del código. |
| Precondiciones | Existe PESO_MAXIMO_CARGA_KG. |
| Datos de entrada | codigo = peso_maximo_carga_kg |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 Conflict. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-011 · Eliminar parámetro obligatorio

| Campo | Valor |
|---|---|
| ID | CP-PAR-011 |
| Nombre | Eliminar parámetro obligatorio |
| Requisito relacionado | RN-19 |
| Objetivo | Verificar la política de eliminación de parámetros en uso. |
| Precondiciones | PESO_MAXIMO_CARGA_KG es obligatorio. |
| Datos de entrada | id de PESO_MAXIMO_CARGA_KG |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/parametros/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409; el parámetro sigue existiendo. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-012 · Desactivar o cambiar un parámetro obligatorio

| Campo | Valor |
|---|---|
| ID | CP-PAR-012 |
| Nombre | Desactivar o cambiar un parámetro obligatorio |
| Requisito relacionado | RN-19 |
| Objetivo | Verificar que un parámetro obligatorio conserva su código, su tipo y su estado activo. |
| Precondiciones | VOLUMEN_MAXIMO_CARGA_M3 / PESO_MAXIMO_CARGA_KG obligatorios. |
| Datos de entrada | activo = false; tipoDato = ENTERO; codigo = OTRO_CODIGO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/parametros/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 en cada variante. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-013 · Parámetro asociado a una categoría inexistente

| Campo | Valor |
|---|---|
| ID | CP-PAR-013 |
| Nombre | Parámetro asociado a una categoría inexistente |
| Requisito relacionado | RN-06 |
| Objetivo | Verificar la referencia a la categoría. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | categoriaId = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en categoriaId. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-014 · Capacidad asociada a una categoría que no es VEHICULO

| Campo | Valor |
|---|---|
| ID | CP-PAR-014 |
| Nombre | Capacidad asociada a una categoría que no es VEHICULO |
| Requisito relacionado | RN-07, RN-17 |
| Objetivo | Verificar que CAPACIDAD_KG_* solo se asocia a VEHICULO. |
| Precondiciones | Existe una categoría TIPO_CARGA. |
| Datos de entrada | codigo CAPACIDAD_KG_xxx, categoriaId de TIPO_CARGA |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en categoriaId. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-015 · Parámetro de límite con valor ≤ 0

| Campo | Valor |
|---|---|
| ID | CP-PAR-015 |
| Nombre | Parámetro de límite con valor ≤ 0 |
| Requisito relacionado | RN-17 |
| Objetivo | Verificar los valores límite de los parámetros de límite. |
| Precondiciones | PESO_MAXIMO_CARGA_KG existe. |
| Datos de entrada | valor "0" y "-100" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/parametros/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en valor; el límite no cambia. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-016 · Eliminar parámetro no obligatorio

| Campo | Valor |
|---|---|
| ID | CP-PAR-016 |
| Nombre | Eliminar parámetro no obligatorio |
| Requisito relacionado | RF-004, RN-19 |
| Objetivo | Verificar la eliminación permitida. |
| Precondiciones | Parámetro no obligatorio creado en el caso. |
| Datos de entrada | id no obligatorio |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/parametros/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 204; un GET posterior devuelve 404. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-PAR-017 · Crear parámetro asociado a una categoría

| Campo | Valor |
|---|---|
| ID | CP-PAR-017 |
| Nombre | Crear parámetro asociado a una categoría |
| Requisito relacionado | RF-004 |
| Objetivo | Verificar el ámbito CATEGORIA y los datos de la categoría en la respuesta. |
| Precondiciones | Existe una categoría VEHICULO. |
| Datos de entrada | codigo CAPACIDAD_KG_xxx, categoriaId de un VEHICULO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/parametros` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 201; ambito CATEGORIA; categoriaTipo VEHICULO. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

## Cargas

### CP-CAR-001 · Listar cargas

| Campo | Valor |
|---|---|
| ID | CP-CAR-001 |
| Nombre | Listar cargas |
| Requisito relacionado | RF-001 |
| Objetivo | Verificar la consulta general. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | — |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/cargas`.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; lista JSON (incluye la carga creada en Postman). |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-002 · Crear carga válida

| Campo | Valor |
|---|---|
| ID | CP-CAR-002 |
| Nombre | Crear carga válida |
| Requisito relacionado | RF-001 |
| Objetivo | Verificar la creación, la normalización del código y el estado por defecto. |
| Precondiciones | Categorías válidas de TIPO_CARGA y TIPO_ENTREGA. |
| Datos de entrada | codigo en minúsculas, origen/destino distintos, pesoTotalKg 10000 (xUnit) o 4000 (Postman), sin estadoId |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 201; Location; codigo en mayúsculas; estadoCodigo PENDIENTE; cantidadObjetos 0. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-003 · Consultar carga por id

| Campo | Valor |
|---|---|
| ID | CP-CAR-003 |
| Nombre | Consultar carga por id |
| Requisito relacionado | RF-001 |
| Objetivo | Verificar la consulta con nombres de las clasificaciones. |
| Precondiciones | Carga creada en el caso. |
| Datos de entrada | id existente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con tipoCargaNombre, tipoEntregaNombre y estadoNombre. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-004 · Consultar carga inexistente

| Campo | Valor |
|---|---|
| ID | CP-CAR-004 |
| Nombre | Consultar carga inexistente |
| Requisito relacionado | RF-001, RN-15 |
| Objetivo | Verificar la respuesta ante un id inexistente. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | id = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/cargas/999999` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-005 · Actualizar carga

| Campo | Valor |
|---|---|
| ID | CP-CAR-005 |
| Nombre | Actualizar carga |
| Requisito relacionado | RF-001 |
| Objetivo | Verificar la actualización del destino, el estado y el vehículo. |
| Precondiciones | Carga creada en el caso. |
| Datos de entrada | destino Pereira, estadoId EN_TRANSITO (o el estado de Postman), tipoVehiculoId |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 con los datos nuevos; fechaRegistro sin cambios. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-006 · Crear carga con campos obligatorios vacíos

| Campo | Valor |
|---|---|
| ID | CP-CAR-006 |
| Nombre | Crear carga con campos obligatorios vacíos |
| Requisito relacionado | RN-02 |
| Objetivo | Verificar que se informan todos los campos obligatorios. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | codigo "", descripcion "   ", origen "", destino null; sin peso ni categorías |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con errores en codigo, descripcion, origen, destino, pesoTotalKg, tipoCargaId y tipoEntregaId. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-007 · Peso total negativo o cero

| Campo | Valor |
|---|---|
| ID | CP-CAR-007 |
| Nombre | Peso total negativo o cero |
| Requisito relacionado | RN-04 |
| Objetivo | Valores límite inferiores del peso. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | pesoTotalKg ∈ {-1, 0, -0.01} |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en pesoTotalKg. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-008 · Volumen negativo

| Campo | Valor |
|---|---|
| ID | CP-CAR-008 |
| Nombre | Volumen negativo |
| Requisito relacionado | RN-04 |
| Objetivo | Verificar la validación del volumen. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | volumenTotalM3 = -5 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en volumenTotalM3. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-009 · Origen y destino inválidos

| Campo | Valor |
|---|---|
| ID | CP-CAR-009 |
| Nombre | Origen y destino inválidos |
| Requisito relacionado | RN-03 |
| Objetivo | Verificar que el origen y el destino son válidos y distintos. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | Bogotá/bogotá; "AB"/Cali; Cali/"  "; Cali/cali |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en destino u origen según la variante. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-010 · Código de carga duplicado

| Campo | Valor |
|---|---|
| ID | CP-CAR-010 |
| Nombre | Código de carga duplicado |
| Requisito relacionado | RN-01 |
| Objetivo | Verificar la unicidad del código sin distinguir mayúsculas. |
| Precondiciones | Carga existente creada en el caso. |
| Datos de entrada | codigo existente (en minúsculas en xUnit) |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409; detail menciona el código. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-011 · Categoría VEHICULO usada como TIPO_CARGA

| Campo | Valor |
|---|---|
| ID | CP-CAR-011 |
| Nombre | Categoría VEHICULO usada como TIPO_CARGA |
| Requisito relacionado | RN-07, RN-08 |
| Objetivo | Regla clave: un registro VEHICULO no puede usarse como TIPO_CARGA. |
| Precondiciones | Existe una categoría VEHICULO. |
| Datos de entrada | tipoCargaId = id de una categoría VEHICULO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400; errors.tipoCargaId explica que la categoría es VEHICULO y no puede usarse como TIPO_CARGA. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-012 · Categoría de tipo incorrecto en vehículo, entrega o estado

| Campo | Valor |
|---|---|
| ID | CP-CAR-012 |
| Nombre | Categoría de tipo incorrecto en vehículo, entrega o estado |
| Requisito relacionado | RN-07 |
| Objetivo | Tabla de decisión campo × tipo de categoría. |
| Precondiciones | Categorías de varios tipos. |
| Datos de entrada | tipoVehiculoId=TIPO_CARGA; tipoEntregaId=ESTADO_CARGA; estadoId=TIPO_ENTREGA |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en el campo correspondiente. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-013 · Carga con categoría inexistente

| Campo | Valor |
|---|---|
| ID | CP-CAR-013 |
| Nombre | Carga con categoría inexistente |
| Requisito relacionado | RN-06, RN-08 |
| Objetivo | Verificar la existencia de la categoría referenciada. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | tipoEntregaId = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en tipoEntregaId. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-014 · Carga nueva con categoría inactiva

| Campo | Valor |
|---|---|
| ID | CP-CAR-014 |
| Nombre | Carga nueva con categoría inactiva |
| Requisito relacionado | RN-08, RN-14 |
| Objetivo | Verificar que las categorías desactivadas no se asignan a registros nuevos. |
| Precondiciones | Categoría TIPO_CARGA desactivada. |
| Datos de entrada | tipoCargaId = categoría inactiva |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400; el mensaje indica "inactiva". |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-015 · Peso que supera PESO_MAXIMO_CARGA_KG

| Campo | Valor |
|---|---|
| ID | CP-CAR-015 |
| Nombre | Peso que supera PESO_MAXIMO_CARGA_KG |
| Requisito relacionado | RN-17 |
| Objetivo | Valores límite del parámetro configurable de peso. |
| Precondiciones | PESO_MAXIMO_CARGA_KG activo (40000 de ejemplo). |
| Datos de entrada | pesoTotalKg = límite + 0.01 (xUnit) o + 1 (Postman); además, peso = límite |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en pesoTotalKg que cita PESO_MAXIMO_CARGA_KG; con peso = límite → 201. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-016 · Peso que supera la capacidad del vehículo

| Campo | Valor |
|---|---|
| ID | CP-CAR-016 |
| Nombre | Peso que supera la capacidad del vehículo |
| Requisito relacionado | RN-17 |
| Objetivo | Verificar los parámetros de categoría CAPACIDAD_KG_*. |
| Precondiciones | Vehículo con parámetro CAPACIDAD_KG_* (CAMION_RIGIDO = 17000 o el creado por Postman = 5000). |
| Datos de entrada | pesoTotalKg = capacidad + 1; además, peso = capacidad |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 que cita el parámetro de capacidad; con peso = capacidad → 201. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-017 · Volumen que supera VOLUMEN_MAXIMO_CARGA_M3

| Campo | Valor |
|---|---|
| ID | CP-CAR-017 |
| Nombre | Volumen que supera VOLUMEN_MAXIMO_CARGA_M3 |
| Requisito relacionado | RN-17 |
| Objetivo | Verificar el parámetro configurable de volumen. |
| Precondiciones | VOLUMEN_MAXIMO_CARGA_M3 activo. |
| Datos de entrada | volumenTotalM3 = límite + 1 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en volumenTotalM3. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-018 · Tipo de dato incorrecto en el JSON

| Campo | Valor |
|---|---|
| ID | CP-CAR-018 |
| Nombre | Tipo de dato incorrecto en el JSON |
| Requisito relacionado | RN-15, RN-16 |
| Objetivo | Verificar el manejo de tipos incorrectos sin exponer detalles .NET. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | "pesoTotalKg": "mil", "tipoCargaId": "uno" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en pesoTotalKg; el cuerpo no contiene "System.". |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-019 · JSON mal formado o cuerpo vacío

| Campo | Valor |
|---|---|
| ID | CP-CAR-019 |
| Nombre | JSON mal formado o cuerpo vacío |
| Requisito relacionado | RN-15 |
| Objetivo | Verificar la robustez ante cuerpos inválidos. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | JSON truncado; "esto no es json"; cuerpo vacío |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 ProblemDetails con errores (clave cuerpo o el campo). |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-020 · Actualizar carga inexistente

| Campo | Valor |
|---|---|
| ID | CP-CAR-020 |
| Nombre | Actualizar carga inexistente |
| Requisito relacionado | RF-001, RN-15 |
| Objetivo | Verificar el 404 en PUT. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | cuerpo válido |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/cargas/999999` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-021 · Eliminar carga sin objetos

| Campo | Valor |
|---|---|
| ID | CP-CAR-021 |
| Nombre | Eliminar carga sin objetos |
| Requisito relacionado | RF-001, RN-12 |
| Objetivo | Verificar la eliminación permitida. |
| Precondiciones | Carga sin objetos. |
| Datos de entrada | id existente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 204; un GET posterior devuelve 404. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-022 · Eliminar carga con objetos

| Campo | Valor |
|---|---|
| ID | CP-CAR-022 |
| Nombre | Eliminar carga con objetos |
| Requisito relacionado | RN-13 |
| Objetivo | Verificar que no se elimina una carga con objetos accidentalmente. |
| Precondiciones | Carga con al menos un objeto. |
| Datos de entrada | id de una carga con objetos |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409; la carga y los objetos siguen existiendo. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-023 · Reducir peso por debajo del peso de los objetos

| Campo | Valor |
|---|---|
| ID | CP-CAR-023 |
| Nombre | Reducir peso por debajo del peso de los objetos |
| Requisito relacionado | RN-18 |
| Objetivo | Verificar la coherencia del peso al actualizar la carga. |
| Precondiciones | Carga con objetos de 3000 kg. |
| Datos de entrada | pesoTotalKg = 2999; luego 3000 (xUnit) |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 con 2999; 200 con 3000 (valor límite). |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-024 · Eliminar carga inexistente

| Campo | Valor |
|---|---|
| ID | CP-CAR-024 |
| Nombre | Eliminar carga inexistente |
| Requisito relacionado | RN-15 |
| Objetivo | Verificar el 404 en DELETE. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | id inexistente o ya eliminado |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Baja |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-CAR-025 · Actualizar carga conservando una categoría que pasó a inactiva

| Campo | Valor |
|---|---|
| ID | CP-CAR-025 |
| Nombre | Actualizar carga conservando una categoría que pasó a inactiva |
| Requisito relacionado | RN-08, RN-14 |
| Objetivo | Verificar la integridad histórica: lo que ya estaba asignado se conserva. |
| Precondiciones | Carga creada con una categoría que luego se desactivó. |
| Datos de entrada | mismo tipoCargaId (inactivo), descripción nueva |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/cargas/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200 OK; la categoría inactiva se conserva. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

## Objetos

### CP-OBJ-001 · Listar objetos

| Campo | Valor |
|---|---|
| ID | CP-OBJ-001 |
| Nombre | Listar objetos |
| Requisito relacionado | RF-002 |
| Objetivo | Verificar la consulta general. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | — |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/objetos`.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; lista JSON. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-002 · Crear objeto válido

| Campo | Valor |
|---|---|
| ID | CP-OBJ-002 |
| Nombre | Crear objeto válido |
| Requisito relacionado | RF-002 |
| Objetivo | Verificar la creación, el cálculo del peso y el reflejo en la carga. |
| Precondiciones | Carga existente con capacidad disponible. |
| Datos de entrada | cantidad 2, pesoUnitarioKg 1500 (xUnit) o 1000 (Postman), categoriaId TIPO_OBJETO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 201; pesoTotalKg = cantidad × peso; la carga muestra cantidadObjetos y pesoObjetosKg. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-003 · Consultar objeto por id

| Campo | Valor |
|---|---|
| ID | CP-OBJ-003 |
| Nombre | Consultar objeto por id |
| Requisito relacionado | RF-002 |
| Objetivo | Verificar la consulta individual. |
| Precondiciones | Objeto existente. |
| Datos de entrada | id existente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/objetos/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-004 · Consultar objeto inexistente

| Campo | Valor |
|---|---|
| ID | CP-OBJ-004 |
| Nombre | Consultar objeto inexistente |
| Requisito relacionado | RF-002, RN-15 |
| Objetivo | Verificar el 404. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | id = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/objetos/999999` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-005 · Consultar objetos de una carga

| Campo | Valor |
|---|---|
| ID | CP-OBJ-005 |
| Nombre | Consultar objetos de una carga |
| Requisito relacionado | RF-005 |
| Objetivo | Verificar la relación carga → objetos y el filtro ?cargaId. |
| Precondiciones | Dos cargas con objetos distintos. |
| Datos de entrada | id de cada carga |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/cargas/{id}/objetos y /api/objetos?cargaId={id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; solo los objetos de la carga indicada. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-006 · Objetos de una carga inexistente

| Campo | Valor |
|---|---|
| ID | CP-OBJ-006 |
| Nombre | Objetos de una carga inexistente |
| Requisito relacionado | RF-005 |
| Objetivo | Verificar el 404 del endpoint de relación. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | id inexistente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/cargas/999999/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 404 ProblemDetails. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-007 · Actualizar objeto

| Campo | Valor |
|---|---|
| ID | CP-OBJ-007 |
| Nombre | Actualizar objeto |
| Requisito relacionado | RF-002 |
| Objetivo | Verificar la actualización y el recálculo del peso. |
| Precondiciones | Objeto existente. |
| Datos de entrada | cantidad 3 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/objetos/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 200; pesoTotalKg recalculado (4500 / 3000). |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-008 · Objeto vinculado a una carga inexistente

| Campo | Valor |
|---|---|
| ID | CP-OBJ-008 |
| Nombre | Objeto vinculado a una carga inexistente |
| Requisito relacionado | RN-06, RN-09 |
| Objetivo | Verificar que un objeto no pertenece a una carga inexistente. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | cargaId = 999999 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en cargaId. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-009 · Cantidad cero o negativa

| Campo | Valor |
|---|---|
| ID | CP-OBJ-009 |
| Nombre | Cantidad cero o negativa |
| Requisito relacionado | RN-05 |
| Objetivo | Valores límite de la cantidad. |
| Precondiciones | Carga existente. |
| Datos de entrada | cantidad ∈ {0, -3} |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en cantidad. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-010 · Peso o volumen unitario negativos

| Campo | Valor |
|---|---|
| ID | CP-OBJ-010 |
| Nombre | Peso o volumen unitario negativos |
| Requisito relacionado | RN-04 |
| Objetivo | Verificar las validaciones físicas. |
| Precondiciones | Carga existente. |
| Datos de entrada | pesoUnitarioKg = -10; volumenUnitarioM3 = -10 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en el campo. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-011 · Objeto con campos obligatorios vacíos

| Campo | Valor |
|---|---|
| ID | CP-OBJ-011 |
| Nombre | Objeto con campos obligatorios vacíos |
| Requisito relacionado | RN-02 |
| Objetivo | Verificar los campos obligatorios. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | solo nombre "  " |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con errores en cargaId, nombre, cantidad y pesoUnitarioKg. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-012 · Objeto con categoría de tipo incorrecto

| Campo | Valor |
|---|---|
| ID | CP-OBJ-012 |
| Nombre | Objeto con categoría de tipo incorrecto |
| Requisito relacionado | RN-07 |
| Objetivo | Verificar que solo se aceptan categorías TIPO_OBJETO. |
| Precondiciones | Existe una categoría VEHICULO. |
| Datos de entrada | categoriaId = VEHICULO |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400; el mensaje menciona TIPO_OBJETO. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-013 · Objetos que superan el peso declarado

| Campo | Valor |
|---|---|
| ID | CP-OBJ-013 |
| Nombre | Objetos que superan el peso declarado |
| Requisito relacionado | RN-18 |
| Objetivo | Valores límite de la capacidad de la carga. |
| Precondiciones | Carga de 5000 kg con 3000 kg en objetos (xUnit) o de 4000 kg con 3000 kg (Postman). |
| Datos de entrada | 1 × 2000.01 kg (xUnit) o 1 × 1001 kg (Postman); además, 1 × 2000 kg |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 cuando se supera; 201 cuando completa exactamente el peso. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-014 · Cantidad con tipo de dato incorrecto

| Campo | Valor |
|---|---|
| ID | CP-OBJ-014 |
| Nombre | Cantidad con tipo de dato incorrecto |
| Requisito relacionado | RN-05, RN-15 |
| Objetivo | Verificar el manejo de un entero recibido como decimal o texto. |
| Precondiciones | Carga existente. |
| Datos de entrada | cantidad 2.5; cantidad "dos" |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `POST /api/objetos` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 400 con error en cantidad. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-015 · Eliminar objeto

| Campo | Valor |
|---|---|
| ID | CP-OBJ-015 |
| Nombre | Eliminar objeto |
| Requisito relacionado | RF-002 |
| Objetivo | Verificar la eliminación. |
| Precondiciones | Objeto existente. |
| Datos de entrada | id existente |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `DELETE /api/objetos/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 204; un GET posterior devuelve 404. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Funcional |
| Herramientas | xUnit, Postman/Newman |

### CP-OBJ-016 · Mover objeto a una carga sin capacidad

| Campo | Valor |
|---|---|
| ID | CP-OBJ-016 |
| Nombre | Mover objeto a una carga sin capacidad |
| Requisito relacionado | RN-18 |
| Objetivo | Verificar RN-18 cuando un objeto cambia de carga. |
| Precondiciones | Objeto de 3000 kg; carga destino de 1000 kg. |
| Datos de entrada | cargaId = carga destino |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `PUT /api/objetos/{id}` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 409 Conflict. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Media |
| Tipo de prueba | Negativa |
| Herramientas | xUnit, Postman/Newman |

## Integración

### CP-INT-001 · Persistencia después de reiniciar la aplicación

| Campo | Valor |
|---|---|
| ID | CP-INT-001 |
| Nombre | Persistencia después de reiniciar la aplicación |
| Requisito relacionado | RF-009 |
| Objetivo | Verificar que SQLite conserva los datos y que la siembra no se repite. |
| Precondiciones | Archivo SQLite temporal compartido por dos instancias sucesivas. |
| Datos de entrada | Carga y objeto creados en la primera instancia |
| Pasos | 1. Iniciar la instancia 1 de la aplicación sobre el archivo F.<br>2. Crear una carga y un objeto; contar las categorías.<br>3. Detener la instancia 1.<br>4. Iniciar la instancia 2 sobre el mismo archivo F.<br>5. Consultar la carga y contar de nuevo las categorías. |
| Resultado esperado | Con la segunda instancia, GET /api/cargas/{id} → 200 con el mismo código y 1 objeto; el número de categorías no cambia. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Integración |
| Herramientas | xUnit |

### CP-INT-002 · Integridad referencial en SQLite

| Campo | Valor |
|---|---|
| ID | CP-INT-002 |
| Nombre | Integridad referencial en SQLite |
| Requisito relacionado | RN-12, RNF-003 |
| Objetivo | Verificar que las FK Restrict protegen los datos aunque se omita la API. |
| Precondiciones | Carga creada por la API. |
| Datos de entrada | DbContext directo |
| Pasos | 1. Crear una carga por la API.<br>2. Con un DbContext directo, eliminar su categoría TipoCarga y guardar.<br>3. Insertar un objeto por SQL y eliminar la carga con EF.<br>4. Verificar que ambas operaciones fallan. |
| Resultado esperado | Borrar la categoría referenciada → DbUpdateException; borrar una carga con un objeto → DbUpdateException. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Integración |
| Herramientas | xUnit |

### CP-INT-003 · Restricciones CHECK en SQLite

| Campo | Valor |
|---|---|
| ID | CP-INT-003 |
| Nombre | Restricciones CHECK en SQLite |
| Requisito relacionado | RN-04, RN-05, RN-07, RNF-003 |
| Objetivo | Verificar las restricciones de la base de datos. |
| Precondiciones | Carga y objeto creados por la API. |
| Datos de entrada | UPDATE con peso "-5", cantidad 0 y TipoCategoria "COLOR" |
| Pasos | 1. Crear una carga y un objeto por la API.<br>2. Ejecutar UPDATE directos con valores inválidos.<br>3. Verificar la excepción CHECK en cada uno. |
| Resultado esperado | SqliteException "CHECK constraint failed" en los tres casos. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Media |
| Tipo de prueba | Integración |
| Herramientas | xUnit |

### CP-INT-004 · Flujo completo con eliminación ordenada

| Campo | Valor |
|---|---|
| ID | CP-INT-004 |
| Nombre | Flujo completo con eliminación ordenada |
| Requisito relacionado | RN-12, RN-13, RN-14, RN-18 |
| Objetivo | Verificar las reglas en conjunto a lo largo del ciclo de vida. |
| Precondiciones | API en ejecución con datos de ejemplo sembrados. |
| Datos de entrada | Categoría, carga de 8000 kg y dos objetos (3000 y 5000 kg) |
| Pasos | 1. Crear una categoría TIPO_OBJETO, una carga y dos objetos que completan el peso.<br>2. Verificar el resumen de la carga.<br>3. Intentar agregar otro objeto (409).<br>4. Intentar eliminar la categoría y la carga (409).<br>5. Eliminar los objetos, la carga y la categoría (204). |
| Resultado esperado | Carga llena → objeto extra 409; eliminar la categoría o la carga con dependencias → 409; eliminar en orden (objetos, carga, categoría) → 204. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes y herramientas. |
| Prioridad | Alta |
| Tipo de prueba | Integración |
| Herramientas | xUnit, Postman/Newman |

### CP-INT-005 · Error inesperado sin detalles internos

| Campo | Valor |
|---|---|
| ID | CP-INT-005 |
| Nombre | Error inesperado sin detalles internos |
| Requisito relacionado | RN-16, RNF-001 |
| Objetivo | Verificar el manejador global de excepciones. |
| Precondiciones | Filtro de prueba que lanza una excepción con datos sensibles (encabezado X-Simular-Error: 1). |
| Datos de entrada | Encabezado X-Simular-Error: 1 |
| Pasos | 1. Preparar las precondiciones (xUnit crea sus datos con códigos únicos).<br>2. Enviar `GET /api/cargas` con los datos de entrada.<br>3. Verificar el código HTTP, el Content-Type y el cuerpo de la respuesta. |
| Resultado esperado | 500 application/problem+json sin el mensaje de la excepción, "Data Source", "Password", el nombre de la excepción ni la pila de llamadas. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Integración |
| Herramientas | xUnit |

### CP-INT-006 · Base de datos de pruebas separada

| Campo | Valor |
|---|---|
| ID | CP-INT-006 |
| Nombre | Base de datos de pruebas separada |
| Requisito relacionado | RNF-002 |
| Objetivo | Verificar que las pruebas no usan la base de la aplicación. |
| Precondiciones | Fábrica de pruebas iniciada. |
| Datos de entrada | AppDbContext resuelto del contenedor de la aplicación |
| Pasos | 1. Iniciar la aplicación de prueba.<br>2. Resolver AppDbContext desde su contenedor de servicios.<br>3. Leer la cadena de conexión y comparar la ruta. |
| Resultado esperado | La ruta del archivo está en %TEMP% y no es transporte.db. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Integración |
| Herramientas | xUnit |

## Interfaz web

### CP-UI-001 · La página de inicio muestra los indicadores leídos de la API

| Campo | Valor |
|---|---|
| ID | CP-UI-001 |
| Nombre | La página de inicio muestra los indicadores leídos de la API |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. Abrir /, esperar a que carguen los indicadores y comparar el total de categorías con GET /api/categorias.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | Los indicadores son numéricos y coinciden con la API. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Media |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-002 · Filtrar categorías por tipo en la interfaz

| Campo | Valor |
|---|---|
| ID | CP-UI-002 |
| Nombre | Filtrar categorías por tipo en la interfaz |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. Abrir /Categorias y elegir VEHICULO en el filtro.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | La tabla muestra solo filas VEHICULO. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Media |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-003 · El formulario de carga solo ofrece categorías del tipo correcto

| Campo | Valor |
|---|---|
| ID | CP-UI-003 |
| Nombre | El formulario de carga solo ofrece categorías del tipo correcto |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. Abrir /Cargas y comparar las opciones de cada selector con /api/categorias?tipoCategoria=.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | Cada selector solo contiene ids del tipo correspondiente. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-004 · El formulario de carga vacío muestra los errores de la API

| Campo | Valor |
|---|---|
| ID | CP-UI-004 |
| Nombre | El formulario de carga vacío muestra los errores de la API |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. Abrir /Cargas y pulsar Registrar sin datos.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | Alerta con HTTP 400; los campos codigo, descripcion, origen, destino, pesoTotalKg, tipoCargaId y tipoEntregaId se marcan como inválidos. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-005 · Registrar una carga desde el formulario

| Campo | Valor |
|---|---|
| ID | CP-UI-005 |
| Nombre | Registrar una carga desde el formulario |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. Completar el formulario con datos válidos y pulsar Registrar.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | Mensaje de éxito; la carga aparece en la tabla. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Alta |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-006 · Objeto que supera el peso declarado (409) en la interfaz

| Campo | Valor |
|---|---|
| ID | CP-UI-006 |
| Nombre | Objeto que supera el peso declarado (409) en la interfaz |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. En /Objetos?cargaId= registrar 2500 kg y luego 600 kg en una carga de 3000 kg.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | El primero se registra; el segundo muestra una alerta HTTP 409 con el peso disponible. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Media |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-007 · Eliminar una carga con objetos (409) en la interfaz

| Campo | Valor |
|---|---|
| ID | CP-UI-007 |
| Nombre | Eliminar una carga con objetos (409) en la interfaz |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. En /Cargas pulsar Eliminar y confirmar.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | Se muestra la confirmación y luego una alerta HTTP 409. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Media |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |

### CP-UI-008 · La página de parámetros muestra ámbito y obligatoriedad

| Campo | Valor |
|---|---|
| ID | CP-UI-008 |
| Nombre | La página de parámetros muestra ámbito y obligatoriedad |
| Requisito relacionado | RF-008 |
| Objetivo | Verificar que la interfaz consume la API y muestra sus resultados. |
| Precondiciones | Aplicación en ejecución; navegador Edge headless controlado por CDP. |
| Datos de entrada | Datos tecleados en el formulario |
| Pasos | 1. Abrir /Parametros.<br>2. Verificar el resultado en el DOM y guardar una captura. |
| Resultado esperado | La tabla muestra PESO_MAXIMO_CARGA_KG con la etiqueta Obligatorio y el ámbito VEHICULO / TRACTOCAMION. |
| Criterio de aprobación | Resultado observado = resultado esperado en todas las variantes. |
| Prioridad | Baja |
| Tipo de prueba | Interfaz |
| Herramientas | CDP (Edge headless) |
