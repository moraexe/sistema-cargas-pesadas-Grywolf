'use strict';

(function () {
    const form = document.getElementById('formParametro');
    const filtroCategoria = document.getElementById('filtroCategoria');
    const filtroActivo = document.getElementById('filtroActivo');
    let tiposDato = [];
    let categorias = [];
    let parametros = [];

    async function cargarListas() {
        const [rt, rc] = await Promise.all([Api.get('/parametros/tipos-dato'), Api.get('/categorias')]);
        if (!rt.ok || !rc.ok) { Ui.error('mensajes', rt.ok ? rc : rt); return; }
        tiposDato = rt.datos;
        categorias = rc.datos;
        form.tipoDato.innerHTML = '<option value="">— Seleccione —</option>' +
            tiposDato.map(t => `<option value="${t.codigo}">${Ui.esc(t.codigo)}</option>`).join('');
        filtroCategoria.innerHTML = '<option value="">Todas las categorías</option>' +
            categorias.map(c => `<option value="${c.id}">${Ui.esc(c.tipoCategoria)} / ${Ui.esc(c.codigo)}</option>`).join('');
    }

    function llenarCategorias(valorActual = null) {
        let html = '<option value="">— General del sistema —</option>';
        for (const c of categorias) {
            if (!c.activo && c.id !== valorActual) continue;
            html += `<option value="${c.id}">${Ui.esc(c.tipoCategoria)} / ${Ui.esc(c.codigo)}${c.activo ? '' : ' (inactiva)'}</option>`;
        }
        form.categoriaId.innerHTML = html;
        form.categoriaId.value = valorActual ?? '';
    }

    function mostrarAyudaTipo() {
        const tipo = tiposDato.find(t => t.codigo === form.tipoDato.value);
        document.getElementById('ayudaTipo').textContent = tipo ? tipo.descripcion : '';
    }

    async function listar() {
        const filtros = new URLSearchParams();
        if (filtroCategoria.value) filtros.set('categoriaId', filtroCategoria.value);
        if (filtroActivo.value) filtros.set('activo', filtroActivo.value);
        const r = await Api.get('/parametros' + (filtros.size ? `?${filtros}` : ''));
        if (!r.ok) { Ui.error('mensajes', r); return; }
        parametros = r.datos;
        const cuerpo = document.getElementById('tablaParametros');
        if (parametros.length === 0) {
            cuerpo.innerHTML = '<tr><td colspan="6" class="text-muted">No hay parámetros para el filtro seleccionado.</td></tr>';
            return;
        }
        cuerpo.innerHTML = parametros.map(p => `
            <tr>
                <td><code>${Ui.esc(p.codigo)}</code>${p.esObligatorio ? ' <span class="badge text-bg-warning">Obligatorio</span>' : ''}
                    <div class="small text-muted">${Ui.esc(p.nombre)}</div></td>
                <td class="small">${Ui.esc(p.tipoDato)}</td>
                <td class="text-end">${Ui.esc(p.valor)} ${Ui.esc(p.unidadMedida ?? '')}</td>
                <td class="small">${p.ambito === 'GENERAL' ? 'General' : `${Ui.esc(p.categoriaTipo)} / ${Ui.esc(p.categoriaCodigo)}`}</td>
                <td>${Ui.badgeActivo(p.activo)}</td>
                <td class="acciones">
                    <button class="btn btn-sm btn-outline-primary" data-editar="${p.id}">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" data-eliminar="${p.id}">Eliminar</button>
                </td>
            </tr>`).join('');
    }

    function modoCreacion() {
        form.reset();
        Ui.limpiarErrores(form);
        llenarCategorias();
        mostrarAyudaTipo();
        document.getElementById('parametroId').value = '';
        document.getElementById('tituloFormulario').textContent = 'Registrar parámetro';
        document.getElementById('btnGuardar').textContent = 'Registrar';
        document.getElementById('btnCancelar').classList.add('d-none');
    }

    function modoEdicion(parametro) {
        Ui.limpiarErrores(form);
        llenarCategorias(parametro.categoriaId);
        Ui.llenarFormulario(form, parametro);
        mostrarAyudaTipo();
        document.getElementById('parametroId').value = parametro.id;
        document.getElementById('tituloFormulario').textContent = `Editar parámetro ${parametro.codigo}`;
        document.getElementById('btnGuardar').textContent = 'Guardar cambios';
        document.getElementById('btnCancelar').classList.remove('d-none');
        form.scrollIntoView({ behavior: 'smooth' });
    }

    form.addEventListener('submit', async e => {
        e.preventDefault();
        const id = document.getElementById('parametroId').value;
        const datos = Ui.leerFormulario(form);
        const r = id ? await Api.put(`/parametros/${id}`, datos) : await Api.post('/parametros', datos);
        if (!r.ok) {
            Ui.marcarErrores(form, r.datos?.errors);
            Ui.error('mensajes', r);
            return;
        }
        Ui.mensaje('mensajes', 'success', id ? `Parámetro ${r.datos.codigo} actualizado.` : `Parámetro ${r.datos.codigo} registrado (id ${r.datos.id}).`);
        modoCreacion();
        await listar();
    });

    document.getElementById('tablaParametros').addEventListener('click', async e => {
        const { editar, eliminar } = e.target.dataset;
        const parametro = parametros.find(p => p.id === Number(editar ?? eliminar));
        if (!parametro) return;
        if (editar) modoEdicion(parametro);
        if (eliminar) {
            if (!confirm(`¿Eliminar el parámetro ${parametro.codigo}?`)) return;
            const r = await Api.eliminar(`/parametros/${parametro.id}`);
            if (!r.ok) { Ui.error('mensajes', r); return; }
            Ui.mensaje('mensajes', 'success', `Parámetro ${parametro.codigo} eliminado.`);
            await listar();
        }
    });

    form.tipoDato.addEventListener('change', mostrarAyudaTipo);
    filtroCategoria.addEventListener('change', listar);
    filtroActivo.addEventListener('change', listar);
    document.getElementById('btnCancelar').addEventListener('click', modoCreacion);

    (async () => {
        await cargarListas();
        modoCreacion();
        await listar();
    })();
})();
