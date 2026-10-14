'use strict';

(function () {
    const form = document.getElementById('formCarga');
    const catalogos = { TIPO_CARGA: [], VEHICULO: [], TIPO_ENTREGA: [], ESTADO_CARGA: [] };
    let cargas = [];

    async function cargarCatalogos() {
        const tipos = Object.keys(catalogos);
        const respuestas = await Promise.all(tipos.map(t => Api.get(`/categorias?tipoCategoria=${t}`)));
        respuestas.forEach((r, i) => { if (r.ok) catalogos[tipos[i]] = r.datos; });
    }

    /** Solo se ofrecen categorías del tipo correcto (RN-07); el servidor vuelve a validarlo. */
    function llenarSelects(actual = {}) {
        Ui.llenarSelectCategorias(form.tipoCargaId, catalogos.TIPO_CARGA, { textoVacio: '— Seleccione —', valorActual: actual.tipoCargaId });
        Ui.llenarSelectCategorias(form.tipoVehiculoId, catalogos.VEHICULO, { textoVacio: '— Sin asignar —', valorActual: actual.tipoVehiculoId });
        Ui.llenarSelectCategorias(form.tipoEntregaId, catalogos.TIPO_ENTREGA, { textoVacio: '— Seleccione —', valorActual: actual.tipoEntregaId });
        const textoEstado = actual.id ? '— Seleccione —' : '— Predeterminado (PENDIENTE) —';
        Ui.llenarSelectCategorias(form.estadoId, catalogos.ESTADO_CARGA, { textoVacio: textoEstado, valorActual: actual.estadoId });
    }

    async function listar() {
        const r = await Api.get('/cargas');
        if (!r.ok) { Ui.error('mensajes', r); return; }
        cargas = r.datos;
        const cuerpo = document.getElementById('tablaCargas');
        if (cargas.length === 0) {
            cuerpo.innerHTML = '<tr><td colspan="6" class="text-muted">No hay cargas registradas.</td></tr>';
            return;
        }
        cuerpo.innerHTML = cargas.map(c => `
            <tr>
                <td><strong>${Ui.esc(c.codigo)}</strong><div class="small text-muted">${Ui.esc(c.descripcion)}</div></td>
                <td>${Ui.esc(c.origen)} → ${Ui.esc(c.destino)}</td>
                <td class="text-end">${Ui.numero(c.pesoObjetosKg)} / ${Ui.numero(c.pesoTotalKg)}
                    <div class="small text-muted">${c.cantidadObjetos} objeto(s)</div></td>
                <td class="small">${Ui.esc(c.tipoCargaNombre)}<br>${Ui.esc(c.tipoVehiculoNombre ?? 'Sin vehículo')} · ${Ui.esc(c.tipoEntregaNombre)}</td>
                <td><span class="badge text-bg-info">${Ui.esc(c.estadoNombre)}</span></td>
                <td class="acciones">
                    <a class="btn btn-sm btn-outline-secondary" href="/Objetos?cargaId=${c.id}">Objetos</a>
                    <button class="btn btn-sm btn-outline-primary" data-editar="${c.id}">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" data-eliminar="${c.id}">Eliminar</button>
                </td>
            </tr>`).join('');
    }

    function modoCreacion() {
        form.reset();
        Ui.limpiarErrores(form);
        document.getElementById('cargaId').value = '';
        document.getElementById('tituloFormulario').textContent = 'Registrar carga';
        document.getElementById('btnGuardar').textContent = 'Registrar';
        document.getElementById('btnCancelar').classList.add('d-none');
        llenarSelects();
    }

    function modoEdicion(carga) {
        Ui.limpiarErrores(form);
        llenarSelects(carga);
        Ui.llenarFormulario(form, carga);
        document.getElementById('cargaId').value = carga.id;
        document.getElementById('tituloFormulario').textContent = `Editar carga ${carga.codigo}`;
        document.getElementById('btnGuardar').textContent = 'Guardar cambios';
        document.getElementById('btnCancelar').classList.remove('d-none');
        form.scrollIntoView({ behavior: 'smooth' });
    }

    form.addEventListener('submit', async e => {
        e.preventDefault();
        const id = document.getElementById('cargaId').value;
        const datos = Ui.leerFormulario(form);
        const r = id ? await Api.put(`/cargas/${id}`, datos) : await Api.post('/cargas', datos);
        if (!r.ok) {
            Ui.marcarErrores(form, r.datos?.errors);
            Ui.error('mensajes', r);
            return;
        }
        Ui.mensaje('mensajes', 'success', id ? `Carga ${r.datos.codigo} actualizada.` : `Carga ${r.datos.codigo} registrada (id ${r.datos.id}).`);
        modoCreacion();
        await listar();
    });

    document.getElementById('tablaCargas').addEventListener('click', async e => {
        const editar = e.target.dataset.editar;
        const eliminar = e.target.dataset.eliminar;
        if (editar) {
            const carga = cargas.find(c => c.id === Number(editar));
            if (carga) modoEdicion(carga);
        }
        if (eliminar) {
            const carga = cargas.find(c => c.id === Number(eliminar));
            if (!confirm(`¿Eliminar la carga ${carga?.codigo}?`)) return;
            const r = await Api.eliminar(`/cargas/${eliminar}`);
            if (!r.ok) { Ui.error('mensajes', r); return; }
            Ui.mensaje('mensajes', 'success', `Carga ${carga?.codigo} eliminada.`);
            await listar();
        }
    });

    document.getElementById('btnCancelar').addEventListener('click', modoCreacion);
    document.getElementById('btnRecargar').addEventListener('click', listar);

    (async () => {
        await cargarCatalogos();
        modoCreacion();
        await listar();
    })();
})();
