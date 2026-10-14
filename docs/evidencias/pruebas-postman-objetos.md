# Informe de Pruebas de API · Módulo de Objetos, Relaciones y Reglas de Negocio
**Responsable (Integrante 3):** Pruebas de Postman  
**Rama de trabajo:** `test/postman-objetos`  
**Colección Postman asociada:** `postman/TransporteCargaPesada_Objetos.postman_collection.json` (autónoma) y carpeta `04 · Objetos` + `05 · Integridad y relaciones` de `postman/TransporteCargaPesada.postman_collection.json`  
**Entorno:** `postman/TransporteCargaPesada.postman_environment.json` (`baseUrl = http://localhost:5080`)  

---

## 1. Verificación de Endpoints y Códigos HTTP

Como Responsable de Postman para el Integrante 3, se verificó el comportamiento funcional, los contratos de datos y los códigos de estado HTTP en las operaciones CRUD, relaciones y reglas de integridad que involucran a la entidad **Objeto**.

### 1.1. Matriz de Endpoints Verificados

| Método | Endpoint | Descripción | Códigos HTTP Verificados | Reglas asociadas |
|---|---|---|---|---|
| `GET` | `/api/objetos` | Lista todos los objetos registrados en el sistema | `200 OK` | RF-002 |
| `GET` | `/api/objetos?cargaId={id}` | Filtra y lista los objetos vinculados a una carga | `200 OK` | RF-002, RF-005 |
| `GET` | `/api/objetos/{id}` | Consulta el detalle de un objeto por su ID | `200 OK`, `404 Not Found` | RF-002, RN-15 |
| `POST` | `/api/objetos` | Registra un nuevo objeto en una carga | `201 Created`, `400 Bad Request`, `409 Conflict` | RF-002, RN-02, RN-04, RN-05, RN-06, RN-07, RN-09, RN-18 |
| `PUT` | `/api/objetos/{id}` | Actualiza un objeto o lo reasigna a otra carga | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` | RF-002, RN-02, RN-04, RN-05, RN-06, RN-07, RN-09, RN-18 |
| `DELETE` | `/api/objetos/{id}` | Elimina físicamente un objeto | `204 No Content`, `404 Not Found` | RF-002 |
| `GET` | `/api/cargas/{id}/objetos` | Consulta los objetos pertenecientes a una carga | `200 OK`, `404 Not Found` | RF-005, RN-15 |

### 1.2. Endpoints de Relaciones y Reglas de Negocio Verificados

Para comprobar la integridad referencial y las dependencias de negocio entre Objetos, Cargas y Categorías, se verificaron los siguientes endpoints bajo escenarios de colisión:

| Método | Endpoint | Escenario de Integridad / Regla | Código HTTP | Regla |
|---|---|---|---|---|
| `DELETE` | `/api/cargas/{id}` | Intento de eliminar una carga que posee objetos asociados | `409 Conflict` | RN-13 |
| `PUT` | `/api/cargas/{id}` | Intento de reducir `pesoTotalKg` de la carga por debajo de la suma de pesos de sus objetos | `409 Conflict` | RN-18 |
| `DELETE` | `/api/categorias/{id}` | Intento de eliminar una categoría asignada a un objeto o carga activa | `409 Conflict` | RN-12, RN-14 |
| `PUT` | `/api/categorias/{id}` | Intento de cambiar el `tipoCategoria` de una categoría en uso | `409 Conflict` | RN-14 |
| `PUT` | `/api/categorias/{id}` | Desactivar una categoría en uso (`activo: false`) | `200 OK` | RN-14 |
| `POST` | `/api/cargas` | Intento de crear carga con categoría inactiva | `400 Bad Request` | RN-08, RN-14 |
| `PUT` | `/api/cargas/{id}` | Actualizar carga conservando su categoría inactiva histórica | `200 OK` | RN-08, RN-14 |

---

## 2. Diseño de Pruebas Positivas y Negativas

Se aplicaron técnicas formales de diseño:
1. **Partición de Equivalencia:** División de entradas en clases válidas (cargas existentes, pesos positivos, categorías de tipo `TIPO_OBJETO`) e inválidas (cargas inexistentes, categorías de tipo `VEHICULO`, textos vacíos).
2. **Análisis de Valores Límite:** Cantidad mínima permitida (1 vs. 0 y negativos), límites de capacidad de peso (suma de pesos exactamente igual a la capacidad de la carga vs. capacidad + 1 kg).
3. **Tablas de Decisión y Escenarios:** Coherencia relacional entre cargas, objetos y categorías en transiciones de estado.

### 2.1. Matriz de Casos de Prueba (CP-OBJ y Reglas Relacionales)

| Caso de Prueba | Tipo | Descripción y Datos de Prueba | Resultado Esperado | Resultado Obtenido | Estado |
|---|---|---|---|---|---|
| **CP-OBJ-001** | Positiva | Listar todos los objetos existentes | HTTP 200, Array JSON | HTTP 200 OK, Lista con propiedades correctas | **Aprobado** |
| **CP-OBJ-002** | Positiva | Crear objeto válido con cálculo de peso (`cantidad: 2`, `pesoUnitarioKg: 1000`) | HTTP 201 Created, `Location`, `pesoTotalKg: 2000` | HTTP 201 Created, ID generado, carga actualiza `cantidadObjetos` | **Aprobado** |
| **CP-OBJ-003** | Positiva | Consultar objeto recién creado por ID | HTTP 200, Datos coinciden con la creación | HTTP 200 OK | **Aprobado** |
| **CP-OBJ-004** | Negativa | Consultar objeto inexistente (`ID: 999999999`) | HTTP 404 ProblemDetails | HTTP 404 Not Found, esquema ProblemDetails | **Aprobado** |
| **CP-OBJ-005a** | Positiva | Consultar objetos de una carga mediante subrecurso (`/api/cargas/{id}/objetos`) | HTTP 200, objetos asociados | HTTP 200 OK, solo objetos de la carga | **Aprobado** |
| **CP-OBJ-005b** | Positiva | Filtrar objetos por query param (`/api/objetos?cargaId={id}`) | HTTP 200, lista filtrada | HTTP 200 OK, filtrado exacto | **Aprobado** |
| **CP-OBJ-006** | Negativa | Consultar objetos de una carga inexistente (`ID: 999999999`) | HTTP 404 ProblemDetails | HTTP 404 Not Found | **Aprobado** |
| **CP-OBJ-007** | Positiva | Actualizar objeto modificando cantidad a 3 (`pesoTotalKg` pasa a 3000) | HTTP 200, `pesoTotalKg: 3000` | HTTP 200 OK, recálculo matemático consistente | **Aprobado** |
| **CP-OBJ-008** | Negativa | Crear objeto asignándolo a `cargaId` inexistente | HTTP 400 Bad Request, error en `cargaId` | HTTP 400 Bad Request (RN-06, RN-09) | **Aprobado** |
| **CP-OBJ-009** | Negativa | Crear objeto con cantidad cero (`cantidad: 0`) o negativa | HTTP 400 Bad Request, error en `cantidad` | HTTP 400 Bad Request (RN-05) | **Aprobado** |
| **CP-OBJ-010** | Negativa | Crear objeto con `pesoUnitarioKg: -10` | HTTP 400 Bad Request, error en peso | HTTP 400 Bad Request (RN-04) | **Aprobado** |
| **CP-OBJ-011** | Negativa | Crear objeto con campos obligatorios vacíos (nombre en blanco, sin carga) | HTTP 400 Bad Request, errores en campos requeridos | HTTP 400 Bad Request (RN-02) | **Aprobado** |
| **CP-OBJ-012** | Negativa | Asignar categoría tipo `VEHICULO` como `categoriaId` de un objeto | HTTP 400 Bad Request, mensaje menciona `TIPO_OBJETO` | HTTP 400 Bad Request (RN-07) | **Aprobado** |
| **CP-OBJ-013** | Negativa | Crear objeto cuyo peso sumado excede la capacidad declarada de la carga | HTTP 409 Conflict, mención a peso total declarado | HTTP 409 Conflict (RN-18) | **Aprobado** |
| **CP-OBJ-014** | Negativa | Crear objeto enviando string en campo numérico (`cantidad: "dos"`) | HTTP 400 Bad Request | HTTP 400 Bad Request (RN-05, RN-15) | **Aprobado** |
| **CP-OBJ-015a** | Positiva | Eliminar objeto existente por ID (`DELETE /api/objetos/{id}`) | HTTP 204 No Content | HTTP 204 No Content | **Aprobado** |
| **CP-OBJ-015b** | Negativa | Verificar que el objeto eliminado ya no existe (`GET /api/objetos/{id}`) | HTTP 404 Not Found | HTTP 404 Not Found | **Aprobado** |
| **CP-OBJ-016** | Negativa | Mover objeto a una carga destino que no tiene capacidad suficiente | HTTP 409 Conflict | HTTP 409 Conflict (RN-18) | **Aprobado** |
| **CP-CAR-022** | Negativa | Intentar eliminar carga que tiene objetos asociados | HTTP 409 Conflict | HTTP 409 Conflict (RN-13) | **Aprobado** |
| **CP-CAR-023** | Negativa | Reducir peso de carga por debajo del peso acumulado de sus objetos | HTTP 409 Conflict | HTTP 409 Conflict (RN-18) | **Aprobado** |
| **CP-CAT-013** | Negativa | Eliminar categoría en uso por cargas u objetos | HTTP 409 Conflict | HTTP 409 Conflict (RN-14) | **Aprobado** |
| **CP-CAT-015** | Negativa | Modificar el tipo de una categoría en uso | HTTP 409 Conflict | HTTP 409 Conflict (RN-14) | **Aprobado** |
| **CP-CAT-014** | Positiva | Desactivar categoría en uso (`activo: false`) | HTTP 200 OK | HTTP 200 OK (RN-14) | **Aprobado** |
| **CP-CAR-014** | Negativa | Crear entidad referenciando categoría inactiva | HTTP 400 Bad Request | HTTP 400 Bad Request (RN-08, RN-14) | **Aprobado** |
| **CP-CAR-025** | Positiva | Actualizar entidad preservando categoría inactiva ya asignada | HTTP 200 OK | HTTP 200 OK (RN-08, RN-14) | **Aprobado** |

---

## 3. Automatización de Comprobaciones en Postman

Cada solicitud en la colección cuenta con scripts de prueba automatizados en JavaScript (`pm.test`), distribuidos en validaciones globales y validaciones a nivel de endpoint.

### 3.1. Validaciones Globales (Colección)

Se ejecutan automáticamente en **todas** las peticiones:
1. **Tiempo de respuesta:**
   ```javascript
   pm.test("[General] Tiempo de respuesta menor a 3000 ms", () => {
       pm.expect(pm.response.responseTime).to.be.below(3000);
   });
   ```
2. **Formato estándar ProblemDetails para errores (HTTP ≥ 400):**
   ```javascript
   if (pm.response.code >= 400) {
       pm.test("[General] Error en formato ProblemDetails (application/problem+json)", () => {
           pm.expect(pm.response.headers.get("Content-Type")).to.include("application/problem+json");
           const j = pm.response.json();
           pm.expect(j.status).to.eql(pm.response.code);
           pm.expect(j.title).to.be.a("string");
           pm.expect(j.traceId).to.be.a("string");
       });
       pm.test("[General] La respuesta no expone detalles internos", () => {
           pm.expect(pm.response.text()).to.not.match(/Exception|StackTrace|Data Source|System\.|\\.cs:line/);
       });
   }
   ```

### 3.2. Validaciones Específicas de Objetos

Ejemplo en `CP-OBJ-002` (Crear objeto válido):
```javascript
pm.test("HTTP 201", () => pm.response.to.have.status(201));
pm.test("Devuelve un id numérico generado", () => {
    pm.expect(pm.response.json().id).to.be.a("number").and.above(0);
});
pm.test("Encabezado Location apunta al recurso", () => {
    pm.expect(pm.response.headers.get("Location")).to.include("/" + pm.response.json().id);
});
pm.collectionVariables.set("objetoId", pm.response.json().id);
pm.test("Contiene propiedades obligatorias", () => {
    const j = pm.response.json();
    ["id","cargaId","cargaCodigo","nombre","cantidad","pesoUnitarioKg","pesoTotalKg","categoriaId"]
        .forEach(p => pm.expect(j).to.have.property(p));
});
pm.test("Peso total calculado y carga correcta", () => {
    const j = pm.response.json();
    pm.expect(j.pesoTotalKg).to.eql(2000);
    pm.expect(j.cargaId).to.eql(Number(pm.collectionVariables.get("cargaId")));
});
```

Ejemplo en `CP-OBJ-013` (Exceso de peso declarado - Regla RN-18):
```javascript
pm.test("HTTP 409", () => pm.response.to.have.status(409));
pm.test("El detalle menciona 'peso total declarado'", () => {
    pm.expect(pm.response.json().detail).to.include("peso total declarado");
});
```

Ejemplo en `CP-CAR-022` (Bloqueo de eliminación con objetos asociados - Regla RN-13):
```javascript
pm.test("HTTP 409", () => pm.response.to.have.status(409));
pm.test("El detalle indica que la carga tiene objetos", () => {
    pm.expect(pm.response.json().detail).to.include("objetos asociados");
});
```

---

## 4. Registro de Evidencias, Fallos y Defectos Reproducibles

Durante el ciclo de pruebas de la API se detectaron e investigaron fallos observables, clasificándolos según ISO/IEC/IEEE 29119 e ISTQB en defectos de software e incidencias del testware.

### 4.1. Defecto de Software Identificado: DEF-001 (Incidencia INC-003)

- **Título:** Respuestas de error 400, 404 y 409 no cumplían el encabezado `application/problem+json`.
- **Severidad:** Media (incumplimiento contractual de API REST RFC 7807).
- **Prioridad:** Alta (afectaba transversalmente a todos los endpoints de error de la API).
- **Impacto en Objetos:** Afectó a los casos negativos `CP-OBJ-004`, `CP-OBJ-006`, `CP-OBJ-008`, `CP-OBJ-009`, `CP-OBJ-010`, `CP-OBJ-011`, `CP-OBJ-012`, `CP-OBJ-013`, `CP-OBJ-014`, `CP-OBJ-016` y los de integridad relacional `CP-CAR-022`, `CP-CAR-023`, `CP-CAT-013`.
- **Pasos de Reproducción:**
  1. Iniciar la API REST (`dotnet run --project src/TransporteCargaPesada.Web --launch-profile http`).
  2. Enviar petición GET para un objeto inexistente: `curl -i http://localhost:5080/api/objetos/999999`.
  3. Revisar el encabezado `Content-Type` de la respuesta recibida.
- **Resultado Esperado:** `HTTP/1.1 404 Not Found` con encabezado `Content-Type: application/problem+json; charset=utf-8`.
- **Resultado Observado:** `HTTP/1.1 404 Not Found` con encabezado `Content-Type: application/json; charset=utf-8`.
- **Causa Raíz:** En `src/TransporteCargaPesada.Web/Controllers/ApiControllerBase.cs` existía el atributo decorador `[Produces("application/json")]`, el cual sobreescribía la negociación de contenido de los resultados `ProblemDetails` generados por el framework.
- **Corrección Aplicada:** Se retiró el atributo `[Produces("application/json")]` de la clase base de controladores.
- **Verificación:** Ejecución de regresión automatizada comprobó que el 100% de las respuestas de error retornan `application/problem+json`.

### 4.2. Incidencia de Automatización en Postman: INC-005

- **Título:** 9 aserciones con falsos negativos por discrepancia de tipos de datos en scripts de Postman.
- **Impacto en Objetos:** Fallaron aserciones en `CP-OBJ-001`, `CP-OBJ-002` y `CP-OBJ-005`.
- **Pasos de Reproducción:**
  1. Guardar un identificador en una variable de colección declarada como `type: "string"` mediante `pm.collectionVariables.set("objetoId", pm.response.json().id);`.
  2. Comparar el identificador numérico retornado en la respuesta con la variable: `pm.expect(j.id).to.eql(pm.collectionVariables.get("objetoId"))`.
- **Resultado Observado:** Postman reportó error de aserción: `AssertionError: expected 4 to deeply equal '4'`.
- **Causa Raíz:** La variable devolvía tipo cadena (`"4"`), mientras que el DTO JSON retornaba un entero (`4`). La comparación estricta de Chai (`eql`) fallaba a pesar de que la API funcionaba correctamente.
- **Corrección Aplicada:** Se normalizó la lectura de variables en las aserciones numéricas utilizando conversión explícita: `Number(pm.collectionVariables.get("...Id"))`.
- **Verificación:** En los ciclos de regresión N2, N3 y N4 se obtuvo el 100% de aserciones aprobadas sin falsos negativos.

### 4.3. Resumen de Evidencias de Ejecución (Newman / Postman)

En la ejecución final de regresión (`docs/evidencias/newman-ejecucion-4.txt` y `newman-ejecucion-4.json`):
- **Total de solicitudes en la suite:** 103 solicitudes ejecutadas.
- **Solicitudes del módulo de Objetos e Integridad Relacional:** 27 solicitudes ejecutadas directamente.
- **Aserciones automatizadas evaluadas en el módulo:** > 115 aserciones ejecutadas.
- **Aserciones fallidas:** 0.
- **Tiempos de respuesta:** promedio de 12 ms por solicitud (máximo registrado: 35 ms, ampliamente inferior al umbral de 3000 ms).
- **Estado general:** **100% Aprobado**.

---

## 5. Instrucciones para Ejecutar las Pruebas

### 5.1. Ejecución mediante la Aplicación Postman
1. Iniciar la API REST (`dotnet run --project src/TransporteCargaPesada.Web --launch-profile http`).
2. Abrir Postman e importar:
   - Colección: `postman/TransporteCargaPesada_Objetos.postman_collection.json`
   - Entorno: `postman/TransporteCargaPesada.postman_environment.json`
3. Seleccionar el entorno **Transporte Carga Pesada · Local**.
4. Abrir **Collection Runner**, seleccionar la colección y ejecutar las carpetas en orden secuencial.

### 5.2. Ejecución mediante Newman (CLI)
```bash
npx newman run postman/TransporteCargaPesada_Objetos.postman_collection.json \
  -e postman/TransporteCargaPesada.postman_environment.json
```
