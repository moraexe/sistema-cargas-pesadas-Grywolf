// Utilidades compartidas por las páginas. La interfaz NO contiene reglas de negocio:
// envía los datos a la API REST (/api) y muestra los errores que esta devuelve.
'use strict';

const Api = {
    /** Llama a la API y devuelve { ok, estado, datos } sin lanzar excepciones. */
    async solicitar(metodo, ruta, cuerpo) {
        const opciones = { method: metodo, headers: { Accept: 'application/json' } };
        if (cuerpo !== undefined) {
            opciones.headers['Content-Type'] = 'application/json';
            opciones.body = JSON.stringify(cuerpo);
        }
        let respuesta;
        try {
            respuesta = await fetch('/api' + ruta, opciones);
        } catch {
            return { ok: false, estado: 0, datos: { title: 'No se pudo conectar con la API.' } };
        }
        const texto = await respuesta.text();
        let datos = null;
        try { datos = texto ? JSON.parse(texto) : null; } catch { datos = { title: texto }; }
        return { ok: respuesta.ok, estado: respuesta.status, datos };
    },
    get(ruta) { return this.solicitar('GET', ruta); },
    post(ruta, cuerpo) { return this.solicitar('POST', ruta, cuerpo); },
    put(ruta, cuerpo) { return this.solicitar('PUT', ruta, cuerpo); },
    eliminar(ruta) { return this.solicitar('DELETE', ruta); },
};

const Ui = {
    esc(valor) {
        if (valor === null || valor === undefined) return '';
        return String(valor).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    },

    numero(valor, decimales = 2) {
        if (valor === null || valor === undefined) return '—';
        return Number(valor).toLocaleString('es-CO', { maximumFractionDigits: decimales });
    },

    /** Muestra un mensaje de éxito o el ProblemDetails devuelto por la API. */
    mensaje(contenedor, tipo, titulo, problema) {
        const el = document.getElementById(contenedor);
        let html = `<strong>${Ui.esc(titulo)}</strong>`;
        if (problema) {
            if (problema.status) html += ` <span class="badge text-bg-secondary">HTTP ${problema.status}</span>`;
            if (problema.title && problema.title !== titulo) html += `<div>${Ui.esc(problema.title)}</div>`;
            if (problema.detail) html += `<div>${Ui.esc(problema.detail)}</div>`;
            if (problema.errors) {
                html += '<ul class="mb-0 mt-1">';
                for (const [campo, mensajes] of Object.entries(problema.errors)) {
                    for (const m of mensajes) html += `<li><code>${Ui.esc(campo)}</code>: ${Ui.esc(m)}</li>`;
                }
                html += '</ul>';
            }
        }
        el.innerHTML = `<div class="alert alert-${tipo} alert-dismissible fade show" role="alert">${html}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button></div>`;
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    },

    error(contenedor, respuesta) {
        Ui.mensaje(contenedor, 'danger', 'La operación no se realizó.', respuesta.datos ?? { status: respuesta.estado });
    },

    limpiarMensaje(contenedor) { document.getElementById(contenedor).innerHTML = ''; },

    /** Marca en rojo los campos que la API reportó con error. */
    marcarErrores(form, errores) {
        Ui.limpiarErrores(form);
        if (!errores) return;
        for (const [campo, mensajes] of Object.entries(errores)) {
            const control = form.elements[campo];
            if (!control || !control.classList) continue;
            control.classList.add('is-invalid');
            const ayuda = document.createElement('div');
            ayuda.className = 'invalid-feedback';
            ayuda.textContent = mensajes.join(' ');
            control.insertAdjacentElement('afterend', ayuda);
        }
    },

    limpiarErrores(form) {
        form.querySelectorAll('.is-invalid').forEach(c => c.classList.remove('is-invalid'));
        form.querySelectorAll('.invalid-feedback').forEach(c => c.remove());
    },

    /**
     * Convierte el formulario en un objeto JSON según data-tipo:
     * "numero" → número o null, "id" → entero o null, "bool" → checkbox; resto → texto.
     */
    leerFormulario(form) {
        const datos = {};
        for (const control of form.querySelectorAll('[name]')) {
            if (control.dataset.ignorar !== undefined) continue;
            const tipo = control.dataset.tipo;
            const valor = control.value;
            if (tipo === 'bool') datos[control.name] = control.checked;
            else if (tipo === 'numero') datos[control.name] = valor === '' ? null : Number(valor);
            else if (tipo === 'id') datos[control.name] = valor === '' ? null : parseInt(valor, 10);
            else datos[control.name] = valor;
        }
        return datos;
    },

    llenarFormulario(form, objeto) {
        for (const control of form.querySelectorAll('[name]')) {
            const valor = objeto[control.name];
            if (control.dataset.tipo === 'bool') control.checked = !!valor;
            else control.value = valor === null || valor === undefined ? '' : valor;
        }
    },

    /**
     * Llena un <select> con categorías de un tipo. Solo ofrece categorías activas, salvo la
     * actualmente asignada (para poder conservarla al editar).
     */
    llenarSelectCategorias(select, categorias, { textoVacio = null, valorActual = null } = {}) {
        let html = textoVacio !== null ? `<option value="">${Ui.esc(textoVacio)}</option>` : '';
        for (const c of categorias) {
            if (!c.activo && c.id !== valorActual) continue;
            const sufijo = c.activo ? '' : ' (inactiva)';
            html += `<option value="${c.id}">${Ui.esc(c.nombre)} [${Ui.esc(c.codigo)}]${sufijo}</option>`;
        }
        select.innerHTML = html;
        select.value = valorActual ?? '';
    },

    badgeActivo(activo) {
        return activo ? '<span class="badge text-bg-success">Activo</span>' : '<span class="badge text-bg-secondary">Inactivo</span>';
    },
};
