# Revisión · Preparación de datos iniciales y casos de prueba relacionados

**Fecha:** 14/10/2026
**Rama:** `Modelo-de-negocio-y-datos`
**Alcance:** `Data/DbSeeder.cs` (datos de ejemplo), `Data/InicializacionBaseDatos.cs` (siembra condicional) y la
**consistencia** entre lo sembrado y los casos de prueba que dependen de esos datos (`docs/06`, tests xUnit).
**Base:** commit `d577320` · contrastado con `docs/03-modelo-de-datos.md` (§6) y `docs/06-especificacion-de-pruebas.md`.

---

## 1. Resumen

| | Resultado |
|---|---|
| Datos sembrados | 18 categorías · 8 parámetros · 2 cargas · 3 objetos |
| Siembra condicional | **Solo si `Categorias` está vacío**; nunca borra ni sobrescribe |
| Casos que dependen del seed | CP-INT-001 y todas las precondiciones "con datos de ejemplo sembrados" |
| Dependencias test → seed verificadas | 7 categorías + 2 parámetros referenciados por código, **todos existen** |
| Suite xUnit ejecutada | **109/109 superadas** (`dotnet test`) |
| Defectos | 0 |
| Observaciones | 4 |

Los datos iniciales son **correctos, coherentes con las reglas y suficientes** para los casos que los consumen. La verificación la
hice ejecutando la suite completa: si faltara un dato sembrado que un test busca por código, esos casos fallarían.

---

## 2. Cómo se siembran

`DbSeeder.Sembrar(db)` se invoca desde `InicializacionBaseDatos.InicializarBaseDatos` tras `Database.Migrate()`, al arrancar:

1. **Guardia de entrada:** `if (db.Categorias.Any()) return;` → siembra una sola vez. Los datos **persisten entre reinicios**
   (CP-INT-001 lo verifica: el conteo de categorías no cambia tras reiniciar).
2. Inserta **18 categorías** (`AddRange` + `SaveChanges`) para obtener sus `Id` autogenerados.
3. Inserta **8 parámetros**, referenciando los `Id` de categoría cuando aplica (`CAPACIDAD_KG_*` → vehículo, `TEMPERATURA_MAXIMA_C`
   → REFRIGERADA, `REQUIERE_ESCOLTA` → SOBREDIMENSIONADA).
4. Inserta **2 cargas** con sus **objetos** anidados (carga 1 con 2 objetos, carga 2 con 1 objeto).

Orden correcto: primero las categorías (padres), porque parámetros y cargas apuntan a sus `Id`. Sin errores de FK.

---

## 3. Datos sembrados (inventario)

**Categorías por tipo (18):** VEHICULO (TRACTOCAMION, CAMION_RIGIDO, CAMA_BAJA) · TIPO_CARGA (GENERAL, REFRIGERADA,
SOBREDIMENSIONADA) · TIPO_ENTREGA (DIRECTA, PROGRAMADA) · ESTADO_CARGA (**PENDIENTE**, EN_TRANSITO, ENTREGADA) ·
UNIDAD_MEDIDA (KILOGRAMO, TONELADA, METRO_CUBICO) · TIPO_OBJETO (MAQUINARIA, REPUESTOS, EQUIPO_ELECTRICO, MATERIAL_CONSTRUCCION).

**Parámetros (8):**

| Código | Tipo | Valor | Categoría | Obligatorio |
|---|---|---|---|---|
| PESO_MAXIMO_CARGA_KG | DECIMAL | 40000 | — | Sí |
| VOLUMEN_MAXIMO_CARGA_M3 | DECIMAL | 90 | — | Sí |
| UNIDAD_PESO_PREDETERMINADA | TEXTO | KILOGRAMO | — | Sí |
| CAPACIDAD_KG_TRACTOCAMION | DECIMAL | 34000 | TRACTOCAMION | No |
| CAPACIDAD_KG_CAMION_RIGIDO | DECIMAL | 17000 | CAMION_RIGIDO | No |
| TEMPERATURA_MAXIMA_C | DECIMAL | 4 | REFRIGERADA | No |
| REQUIERE_ESCOLTA | BOOLEANO | true | SOBREDIMENSIONADA | No |
| MAX_OBJETOS_SUGERIDOS | ENTERO | 50 | — | No |

**Cargas (2):** `CP-2026-0001` (Bogotá→Medellín, 28 000 kg, 60 m³, 2 objetos) y `CP-2026-0002` (Barranquilla→Bucaramanga,

---

## 4. Consistencia entre los datos sembrados y los casos de prueba

Muchos casos **no crean sus propias categorías/parámetros base**: los buscan por código entre los sembrados
(`ApiPruebas.CategoriaIdAsync(tipo, codigo)` y `ParametroAsync(codigo)`). Verifiqué que **todo lo que los tests piden existe**
en el seed (si no, esos `[Fact]`/`[Theory]` lanzarían y la suite fallaría; pasó 109/109):

| Buscado en tests (tipo, código) | Existe en seed | Casos que lo usan (ejemplo) |
|---|---|---|
| TIPO_CARGA / GENERAL | Sí | CP-CAR-001… (carga base), CP-CAT-013 |
| TIPO_ENTREGA / DIRECTA | Sí | CP-CAR-* (carga base) |
| ESTADO_CARGA / EN_TRANSITO | Sí | CP-CAR-005 |
| VEHICULO / TRACTOCAMION | Sí | CP-CAR-005, CP-CAR-016 |
| VEHICULO / CAMION_RIGIDO | Sí | CP-PAR-* (filtro) |
| VEHICULO / CAMA_BAJA | Sí | CP-PAR-017 |
| TIPO_OBJETO / MAQUINARIA | Sí | CP-OBJ-002 |
| TIPO_CARGA / REFRIGERADA | Sí | CP-PAR-014 |
| Parámetro PESO_MAXIMO_CARGA_KG | Sí | CP-PAR-011/012/015 |
| Parámetro CAPACIDAD_KG_CAMION_RIGIDO | Sí | CP-PAR-* (filtro) |

Además, la precondición declarada en `docs/06` para la mayoría de casos es *"API en ejecución con datos de ejemplo sembrados"*,
y CP-INT-001 valida de forma explícita que la siembra **no se repite ni borra** al reiniciar. La política de tests (códigos
únicos con sufijo, `FabricaApi` con BD temporal en `%TEMP%`) evita choques con el seed y con ejecuciones repetidas (RNF-006).

---

## 5. Observaciones (adherencia a `.clinerules` y buenas prácticas)

> El seed funciona y es coherente (109/109). Son mejoras de robustez/mantenibilidad, no fallos.

### OBS-DAT-01 · El seed no valida reglas de negocio al insertar  *(robustez)*
`DbSeeder` construye las entidades y llama `SaveChanges` **directamente**, sin pasar por los servicios. No ejecuta `RN-18`
(peso de objetos ≤ peso declarado) ni las validaciones de `ReglasValidacion`; depende de que los valores literales estén bien
hoy. Si alguien edita el seed y pone, por ejemplo, un peso de carga menor que la suma de sus objetos, el arranque fallaría con una
`DbUpdateException` opaca en lugar de un mensaje claro. Considerar una aserción/comentario de coherencia o reutilizar el cálculo
de `CargaService.PesoObjetos` como salvaguarda.

### OBS-DAT-02 · Valores "mágicos" hardcodeados sin constante  *(mantenibilidad)*
Los 8 parámetros usan literales (`"40000"`, `"90"`, `"34000"`…). Está bien para datos de ejemplo, pero los códigos de límite ya
tienen constante en `ParametrosSistema`; los valores no. No es un defecto; se documenta como deuda menor de legibilidad.

### OBS-DAT-03 · CP-INT-001 aserta persistencia pero no el contenido exacto  *(cobertura)*
El caso verifica que el conteo de categorías no cambia tras reiniciar, pero no que los datos sembrados concretos (p. ej. que
`TRACTOCAMION` siga activo) se conservan. La cobertura actual es suficiente; se anota como posible endurecimiento.

### OBS-DAT-04 · Seed acoplado a la migración única  *(escalabilidad)*
Al sembrar por `Id` de categoría recién creada, el seed asume un esquema estable. Si en el futuro hay varias migraciones y datos
históricos, convendría condicionar la siembra por migración aplicada o por bandera de versión. Nota prospectiva.


---

## 6. Recomendación

La preparación de datos iniciales es **correcta y coherente**: siembra una sola vez, persiste entre reinicios, respeta el orden
padre→hijo y cubre todo lo que los casos de prueba consumen por código (verificado con `dotnet test` 109/109). Para la entrega
académica **no requiere cambios**. Las observaciones son mejoras de robustez/mantenibilidad: la más útil es **OBS-DAT-01**
(añadir una salvaguarda de coherencia en el seed para que un futuro edit no rompa el arranque con un error opaco); las demás
(OBS-DAT-02/03/04) son deudas menores o notas prospectivas. Cualquier ajuste debe reverificarse con `dotnet test`.

15 000 kg, 1 objeto). Coherencia RN-18: la suma de pesos de los objetos (24 000 y 12 000) **no supera** el peso declarado.
