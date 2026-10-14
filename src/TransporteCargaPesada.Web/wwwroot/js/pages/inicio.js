'use strict';

(async function () {
    const [cargas, objetos, categorias, parametros] = await Promise.all([
        Api.get('/cargas'), Api.get('/objetos'), Api.get('/categorias'), Api.get('/parametros')]);

    if (![cargas, objetos, categorias, parametros].every(r => r.ok)) {
        Ui.mensaje('mensajes', 'danger', 'No se pudo cargar el resumen desde la API.');
        return;
    }

    document.getElementById('totalCargas').textContent = cargas.datos.length;
    document.getElementById('totalObjetos').textContent = objetos.datos.length;
    document.getElementById('totalCategorias').textContent = categorias.datos.length;
    document.getElementById('totalParametros').textContent = parametros.datos.length;

    const porEstado = {};
    for (const c of cargas.datos) porEstado[c.estadoNombre] = (porEstado[c.estadoNombre] ?? 0) + 1;
    const filasEstado = Object.entries(porEstado)
        .map(([estado, total]) => `<tr><td>${Ui.esc(estado)}</td><td class="text-end">${total}</td></tr>`).join('');
    document.getElementById('tablaEstados').innerHTML = filasEstado || '<tr><td colspan="2" class="text-muted">Sin cargas registradas.</td></tr>';

    const limites = parametros.datos.filter(p => p.codigo.startsWith('PESO_MAXIMO') || p.codigo.startsWith('VOLUMEN_MAXIMO') || p.codigo.startsWith('CAPACIDAD_KG_'));
    document.getElementById('tablaLimites').innerHTML = limites.map(p =>
        `<tr><td><code>${Ui.esc(p.codigo)}</code>${p.activo ? '' : ' <span class="badge text-bg-secondary">Inactivo</span>'}</td>
             <td class="text-end">${Ui.numero(p.valor)} ${Ui.esc(p.unidadMedida ?? '')}</td></tr>`).join('')
        || '<tr><td colspan="2" class="text-muted">Sin límites configurados.</td></tr>';
})();
