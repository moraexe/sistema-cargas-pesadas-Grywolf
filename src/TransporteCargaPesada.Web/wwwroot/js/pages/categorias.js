'use strict';

(function () {
    const form = document.getElementById('formCategoria');
    const filtroTipo = document.getElementById('filtroTipo');
    const filtroActivo = document.getElementById('filtroActivo');
    let categorias = [];

    async function cargarTipos() {
        // Los tipos permitidos vienen de la API (lista centralizada en el servidor).
        const r = await Api.get('/categorias/tipos');
        if (!r.ok) { Ui.error('mensajes', r); return; }
        const opciones = r.datos.map(t => `<option value="${t.codigo}">${Ui.esc(t.codigo)} · ${Ui.esc(t.descripcion)}</option>`).join('');
        filtroTipo.innerHTML = '<option value="">Todos los tipos</option>' + opciones;
        form.tipoCategoria.innerHTML = '<option value="">— Seleccione —</option>' + opciones;
    }

    async function listar() {
        const filtros = new URLSearchParams();
        if (filtroTipo.value) filtros.set('tipoCategoria', filtroTipo.value);
        if (filtroActivo.value) filtros.set('activo', filtroActivo.value);
        const r = await Api.get('/categorias' + (filtros.size ? `?${filtros}` : ''));
        if (!r.ok) { Ui.error('mensajes', r); return; }
        categorias = r.datos;
        const cuerpo = document.getElementById('tablaCategorias');
        if (categorias.length === 0) {
            cuerpo.innerHTML = '<tr><td colspan="5" class="text-muted">No hay categorías para el filtro seleccionado.</td></tr>';
            return;
        }
        cuerpo.innerHTML = categorias.map(c => `
            <tr>
                <td><span class="badge text-bg-light border">${Ui.esc(c.tipoCategoria)}</span></td>
                <td><code>${Ui.esc(c.codigo)}</code></td>
                <td>${Ui.esc(c.nombre)}<div class="small text-muted">${Ui.esc(c.descripcion)}</div></td>
                <td>${Ui.badgeActivo(c.activo)}</td>
                <td class="acciones">
                    <button class="btn btn-sm btn-outline-primary" data-editar="${c.id}">Editar</button>
                    <button class="btn btn-sm btn-outline-secondary" data-alternar="${c.id}">${c.activo ? 'Desactivar' : 'Activar'}</button>
                    <button class="btn btn-sm btn-outline-danger" data-eliminar="${c.id}">Eliminar</button>
                </td>
            </tr>`).join('');
    }

    function modoCreacion() {
        form.reset();
        Ui.limpiarErrores(form);
        document.getElementById('categoriaId').value = '';
        form.tipoCategoria.value = filtroTipo.value;
        document.getElementById('tituloFormulario').textContent = 'Registrar categoría';
        document.getElementById('btnGuardar').textContent = 'Registrar';
        document.getElementById('btnCancelar').classList.add('d-none');
    }

    function modoEdicion(categoria) {
        Ui.limpiarErrores(form);
        Ui.llenarFormulario(form, categoria);
        document.getElementById('categoriaId').value = categoria.id;
        document.getElementById('tituloFormulario').textContent = `Editar categoría ${categoria.codigo}`;
        document.getElementById('btnGuardar').textContent = 'Guardar cambios';
        document.getElementById('btnCancelar').classList.remove('d-none');
        form.scrollIntoView({ behavior: 'smooth' });
    }

    form.addEventListener('submit', async e => {
        e.preventDefault();
        const id = document.getElementById('categoriaId').value;
        const datos = Ui.leerFormulario(form);
        const r = id ? await Api.put(`/categorias/${id}`, datos) : await Api.post('/categorias', datos);
        if (!r.ok) {
            Ui.marcarErrores(form, r.datos?.errors);
            Ui.error('mensajes', r);
            return;
        }
        Ui.mensaje('mensajes', 'success', id ? `Categoría ${r.datos.codigo} actualizada.` : `Categoría ${r.datos.codigo} registrada (id ${r.datos.id}).`);
        modoCreacion();
        await listar();
    });

    document.getElementById('tablaCategorias').addEventListener('click', async e => {
        const { editar, alternar, eliminar } = e.target.dataset;
        const categoria = categorias.find(c => c.id === Number(editar ?? alternar ?? eliminar));
        if (!categoria) return;

        if (editar) modoEdicion(categoria);

        if (alternar) {
            const r = await Api.put(`/categorias/${categoria.id}`, { ...categoria, activo: !categoria.activo });
            if (!r.ok) { Ui.error('mensajes', r); return; }
            Ui.mensaje('mensajes', 'success', `Categoría ${categoria.codigo} ${r.datos.activo ? 'activada' : 'desactivada'}.`);
            await listar();
        }

        if (eliminar) {
            if (!confirm(`¿Eliminar la categoría ${categoria.codigo}?`)) return;
            const r = await Api.eliminar(`/categorias/${categoria.id}`);
            if (!r.ok) { Ui.error('mensajes', r); return; }
            Ui.mensaje('mensajes', 'success', `Categoría ${categoria.codigo} eliminada.`);
            await listar();
        }
    });

    filtroTipo.addEventListener('change', () => { listar(); if (!document.getElementById('categoriaId').value) form.tipoCategoria.value = filtroTipo.value; });
    filtroActivo.addEventListener('change', listar);
    document.getElementById('btnCancelar').addEventListener('click', modoCreacion);

    (async () => {
        await cargarTipos();
        modoCreacion();
        await listar();
    })();
})();
