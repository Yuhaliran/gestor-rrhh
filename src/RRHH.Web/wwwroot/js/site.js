// Confirmación antes de enviar un formulario con data-confirmar (por ejemplo, eliminar)
document.addEventListener('submit', (evento) => {
    const mensaje = evento.target.dataset.confirmar;
    if (mensaje && !confirm(mensaje)) {
        evento.preventDefault();
    }
});

// Listas en cascada (RN6): al elegir una opción en un select con data-cascada-destino, se cargan
// las opciones del select destino desde data-cascada-url ({id} se reemplaza por el valor elegido).
// Los selects que dependen del destino se vacían, porque su padre cambió.
async function cargarCascada(origen) {
    const destino = document.querySelector(origen.dataset.cascadaDestino);
    vaciar(destino);
    if (!origen.value) {
        return;
    }
    const respuesta = await fetch(origen.dataset.cascadaUrl.replace('{id}', encodeURIComponent(origen.value)));
    if (!respuesta.ok) {
        return;
    }
    for (const opcion of await respuesta.json()) {
        destino.add(new Option(opcion.nombre, opcion.id));
    }
}

function vaciar(select) {
    select.length = 1;   // queda la opción vacía («Elegí…»)
    if (select.dataset.cascadaDestino) {
        vaciar(document.querySelector(select.dataset.cascadaDestino));
    }
}

document.querySelectorAll('select[data-cascada-destino]').forEach((select) => {
    select.addEventListener('change', () => cargarCascada(select));
});
