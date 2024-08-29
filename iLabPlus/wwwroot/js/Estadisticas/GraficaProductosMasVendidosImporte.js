const filasPorPaginaProductosImporte = 15;
let paginaActualProductosImporte = 1;
let datosProductoGlobalImporte = [];
let graficoProductosMasVendidosImporte;



function inicioProductosMasVendidosImporte() {

    if (!graficoProductosMasVendidosImporte) {
        const ctxProductos = document.getElementById('graficoProductosMasVendidosImporte').getContext('2d');
        graficoProductosMasVendidosImporte = new Chart(ctxProductos, {
            type: 'bar',
            data: {
                labels: [], // Las etiquetas se llenarán dinámicamente
                datasets: [{
                    label: 'Totales de los más vendidos en euros',
                    backgroundColor: 'rgba(104, 174, 255, 1)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1,
                    data: [] // Los datos se llenarán dinámicamente
                }]
            },
            options: {
                scales: {
                    x: {
                        ticks: {
                            display: false // Oculta las leyendas del eje X
                        }
                    },
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    tooltip: {
                        enabled: true, // Asegura que los tooltips estén habilitados
                        callbacks: {
                            label: function (context) {
                                // Puedes personalizar el texto que aparece en el tooltip
                                return " " + context.parsed.y + " €";
                            }
                        }
                    },
                    legend: {
                        display: true, // Configuración de la leyenda
                        onClick: (e) => {
                            if (e && e.stopPropagation) {
                                e.stopPropagation();
                            }
                        }
                    }
                }
            }
        });
    }



    document.getElementById('selectAnioProductosImporte').addEventListener('change', function () {

        actualizarGraficoProductosMasVendidosImporte(this.value);

    });

    cargarAniosDisponiblesImporte().then(anios => {
        if (anios.length > 0) {
            const primerAnio = anios[0]; // Tomamos el primer año de la lista
            actualizarGraficoProductosMasVendidosImporte(primerAnio); // Actualizamos el gráfico con el primer año
        }

    });


};



const cargarAniosDisponiblesImporte = async () => {
    try {
        const response = await fetch('/Estadisticas/ObtenerAniosDisponibles');
        if (!response.ok) {
            throw new Error('Ocurrio un error al cargar los años disponibles.');
        }
        const anios = await response.json();
        const selectAnioProductosImporte = document.getElementById('selectAnioProductosImporte');

        selectAnioProductosImporte.innerHTML = '';

        anios.forEach(anio => {
            selectAnioProductosImporte.appendChild(new Option(anio, anio));
        });

        if (anios.length > 0) {
            actualizarGraficoProductosMasVendidosImporte(anios[0]);
        }

        return anios;

    } catch (error) {

        console.error("Hubo un problema al carga los años disponibles: ", error);

        return [];

    }
};


function actualizarGraficoProductosMasVendidosImporte(anio) {
    fetch(`/Estadisticas/ObtenerProductosMasVendidos?anio=${anio}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al obtener los productos más vendidos');
            }
            return response.json();
        })
        .then(data => {
            datosProductoGlobalImporte = data.Productos;

            // Los ordenare por TotalVendidoImporte en orden descendente
            datosProductoGlobalImporte.sort((a, b) => b.TotalVendidoImporte - a.TotalVendidoImporte); 

            graficoProductosMasVendidosImporte.data.labels = datosProductoGlobalImporte.map(p => p.Descripcion);

            graficoProductosMasVendidosImporte.data.datasets[0].data = datosProductoGlobalImporte.map(p => p.TotalVendidoImporte);
            graficoProductosMasVendidosImporte.update();

            llenarTablaProductosMasVendidosImporte();
            actualizarEstadoBotonesPaginacionProductosImporte(); // Llama a esta función después de actualizar datosProductoGlobalImporte
        })
        .catch(error => console.error('Error al actualizar el grafico de productos más vendidos: ', error));
}


function llenarTablaProductosMasVendidosImporte() {
    const tbody = document.getElementById('tablaProductosMasVendidosImporte').getElementsByTagName('tbody')[0];
    tbody.innerHTML = '';

    // Calcular el inicio y fin del subconjunto de productos a mostrar
    const inicio = (paginaActualProductosImporte - 1) * filasPorPaginaProductosImporte;
    const fin = Math.min(inicio + filasPorPaginaProductosImporte, graficoProductosMasVendidosImporte.data.labels.length);

    for (let i = inicio; i < fin; i++) {
        const articulo = datosProductoGlobalImporte[i].Articulo;
        const producto = graficoProductosMasVendidosImporte.data.labels[i];
        const totalVendido = graficoProductosMasVendidosImporte.data.datasets[0].data[i];

        const fila = tbody.insertRow();
        const celdaArticulo = fila.insertCell();
        const celdaProducto = fila.insertCell();
        const celdaTotalVendido = fila.insertCell();

        celdaArticulo.textContent = articulo;
        celdaProducto.textContent = producto;
        celdaTotalVendido.textContent = parseFloat(totalVendido).toLocaleString('es-ES', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }
}

function obtenerTotalPaginasProductosImporte() {
    return Math.ceil(datosProductoGlobalImporte.length / filasPorPaginaProductosImporte);
}

function actualizarEstadoBotonesPaginacionProductosImporte() {
    const totalPaginas = obtenerTotalPaginasProductosImporte();
    // console.log(datosProductoGlobalImporte.length, filasPorPaginaProductosImporte, totalPaginas);

    const btnAnterior = document.getElementById('btnAnteriorProductosImporte');
    const btnSiguiente = document.getElementById('btnSiguienteProductosImporte');

    btnAnterior.disabled = paginaActualProductosImporte === 1;
    btnSiguiente.disabled = paginaActualProductosImporte >= totalPaginas;

    // Ajustar los colores según el estado del botón
    btnAnterior.className = btnAnterior.disabled ? 'btn-custom disabled' : 'btn-custom enabled';
    btnSiguiente.className = btnSiguiente.disabled ? 'btn-custom disabled' : 'btn-custom enabled';
}


function cambiarPaginaProductosImporte(incremento) {
    paginaActualProductosImporte += incremento;
    llenarTablaProductosMasVendidosImporte();
    actualizarEstadoBotonesPaginacionProductosImporte();
}

