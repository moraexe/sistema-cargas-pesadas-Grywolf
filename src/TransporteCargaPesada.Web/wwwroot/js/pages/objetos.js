'use strict';

(function () {
    const form = document.getElementById('formObjeto');
    const filtro = document.getElementById('filtroCarga');
    let cargas = [];
    let tiposObjeto = [];
    let objetos = [];

    async function cargarListas() {
        const [rc, rt] = await Promise.all([Api.get('/cargas'), Api.get('/categorias?tipoCategoria=TIPO_OBJETO')]);
        if (!rc.ok) { Ui.error('mensajes', rc); return; }
        cargas = rc.datos;
        tiposObjeto = rt.ok ? rt.datos : [];

        const opciones = cargas.map(c => `<option value="${c.id}">${Ui.esc(c.codigo)} · ${Ui.esc(c.origen)} → ${Ui.esc(c.destino)}</option>`).join('');
        const seleccionado = filtro.value || new URLSearchParams(location.search).get('cargaId') || '';
        filtro.innerHTML = '<option value="">Todas las cargas</option>' + opciones;
        filtro.value = seleccionado;
        form.cargaId.innerHTML = '<option value="">— Seleccione —</option>' + opciones;
    }

    async function listar() {
        const cargaId = filtro.value;
        // Con filtro se usa el endpoint de relación GET /api/cargas/{id}/objetos.
        const r = cargaId ? await Api.get(`/cargas/${cargaId}/objetos`) : await Api.get('/objetos');
        if (!r.ok) { Ui.error('mensajes', r); return; }
        objetos = r.datos;

        const resumen = document.getElementById('resumenCarga');
        const carga = cargas.find(c => c.id === Number(cargaId));
        resumen.innerHTML = carga
            ? `Peso de objetos: <strong>${Ui.numero(objetos.reduce((s, o) => s + o.pesoTotalKg, 0))} kg</strong> de
               <strong>${Ui.numero(carga.pesoTotalKg)} kg</strong> declarados para ${Ui.esc(carga.codigo)}.`
            : '';

        const cuerpo = document.getElementById('tablaObjetos');
        if (objetos.length === 0) {
            cuerpo.innerHTML = '<tr><td colspan="8" class="text-muted">No hay objetos registrados.</td></tr>';
            return;
        }
        cuerpo.innerHTML = objetos.map(o => `
            <tr>
                <td>${Ui.esc(o.cargaCodigo)}</td>
                <td><strong>${Ui.esc(o.nombre)}</strong><div class="small text-muted">${Ui.esc(o.descripcion)}</div></td>
                <td class="text-end">${Ui.numero(o.cantidad, 0)}</td>
                <td class="text-end">${Ui.numero(o.pesoUnitarioKg)}</td>
                <td class="text-end">${Ui.numero(o.pesoTotalKg)}</td>
                <td class="text-end">${Ui.numero(o.volumenUnitarioM3)}</td>
                <td class="small">${Ui.esc(o.categoriaNombre ?? '—')}</td>
                <td class="acciones">
                    <button class="btn btn-sm btn-outline-primary" data-editar="${o.id}">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" data-eliminar="${o.id}">Eliminar</button>
                </td>
            </tr>`).join('');
    }

    function modoCreacion() {
        form.reset();
        Ui.limpiarErrores(form);
        document.getElementById('objetoId').value = '';
        document.getElementById('tituloFormulario').textContent = 'Registrar objeto';
        document.getElementById('btnGuardar').textContent = 'Registrar';
        document.getElementById('btnCancelar').classList.add('d-none');
        Ui.llenarSelectCategorias(form.categoriaId, tiposObjeto, { textoVacio: '— Sin clasificar —' });
        form.cargaId.value = filtro.value;
    }

    function modoEdicion(objeto) {
        Ui.limpiarErrores(form);
        Ui.llenarSelectCategorias(form.categoriaId, tiposObjeto, { textoVacio: '— Sin clasificar —', valorActual: objeto.categoriaId });
        Ui.llenarFormulario(form, objeto);
        document.getElementById('objetoId').value = objeto.id;
        document.getElementById('tituloFormulario').textContent = `Editar objeto ${objeto.nombre}`;
        document.getElementById('btnGuardar').textContent = 'Guardar cambios';
        document.getElementById('btnCancelar').classList.remove('d-none');
        form.scrollIntoView({ behavior: 'smooth' });
    }

    form.addEventListener('submit', async e => {
        e.preventDefault();
        const id = document.getElementById('objetoId').value;
        const datos = Ui.leerFormulario(form);
        const r = id ? await Api.put(`/objetos/${id}`, datos) : await Api.post('/objetos', datos);
        if (!r.ok) {
            Ui.marcarErrores(form, r.datos?.errors);
            Ui.error('mensajes', r);
            return;
        }
        Ui.mensaje('mensajes', 'success', id ? `Objeto ${r.datos.nombre} actualizado.` : `Objeto ${r.datos.nombre} registrado (id ${r.datos.id}).`);
        modoCreacion();
        await cargarListas();
        await listar();
    });

    document.getElementById('tablaObjetos').addEventListener('click', async e => {
        const editar = e.target.dataset.editar;
        const eliminar = e.target.dataset.eliminar;
        if (editar) {
            const objeto = objetos.find(o => o.id === Number(editar));
            if (objeto) modoEdicion(objeto);
        }
        if (eliminar) {
            const objeto = objetos.find(o => o.id === Number(eliminar));
            if (!confirm(`¿Eliminar el objeto ${objeto?.nombre}?`)) return;
            const r = await Api.eliminar(`/objetos/${eliminar}`);
            if (!r.ok) { Ui.error('mensajes', r); return; }
            Ui.mensaje('mensajes', 'success', `Objeto ${objeto?.nombre} eliminado.`);
            await listar();
        }
    });

    filtro.addEventListener('change', () => { form.cargaId.value = filtro.value; listar(); });
    document.getElementById('btnCancelar').addEventListener('click', modoCreacion);

    (async () => {
        await cargarListas();
        modoCreacion();
        await listar();
    })();
})();
