const filasPorPaginaProductos = 15;
let paginaActualProductos = 1;
let datosProductoGlobal = [];
let graficoProductosMasVendidos;




function inicioProductosMasVendidos() {

    if (!graficoProductosMasVendidos) {
        const ctxProductos = document.getElementById('graficoProductosMasVendidos').getContext('2d');
        graficoProductosMasVendidos = new Chart(ctxProductos, {
            type: 'bar',
            data: {
                labels: [], // Las etiquetas se llenarán dinámicamente
                datasets: [{
                    label: 'Totales de los más vendidos en unidades',
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
                                return " " + context.parsed.y + "(importe)";
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


    document.getElementById('selectAnioProductos').addEventListener('change', function () {

        actualizarGraficoProductosMasVendidos(this.value);

    });

    cargarAniosDisponibles().then(anios => {
        if (anios.length > 0) {
            
            const primerAnio = anios[0]; // Tomamos el primer año de la lista
            actualizarGraficoProductosMasVendidos(primerAnio); // Actualizamos el gráfico con el primer año
        }

    });

}



function actualizarGraficoProductosMasVendidos(anio) {
    fetch(`/Estadisticas/ObtenerProductosMasVendidos?anio=${anio}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al obtener los productos más vendidos');
            }
            return response.json();
        })
        .then(data => {

            datosProductoGlobal = data.Productos;
            //console.log("Datos cargados en datosProductoGlobal:", datosProductoGlobal);

            graficoProductosMasVendidos.data.labels = datosProductoGlobal.map(p => p.Descripcion);
            graficoProductosMasVendidos.data.datasets[0].data = datosProductoGlobal.map(p => p.TotalVendido);
            graficoProductosMasVendidos.update();

            llenarTablaProductosMasVendidos();
            actualizarEstadoBotonesPaginacionProductos(); // Llama a esta función después de actualizar datosProductoGlobal
        })
        .catch(error => console.error('Error al actualizar el grafico de productos más vendidos: ', error));
}


function llenarTablaProductosMasVendidos() {
    const tbody = document.getElementById('tablaProductosMasVendidos').getElementsByTagName('tbody')[0];
    tbody.innerHTML = '';

    // Calcular el inicio y fin del subconjunto de productos a mostrar
    const inicio = (paginaActualProductos - 1) * filasPorPaginaProductos;
    const fin = Math.min(inicio + filasPorPaginaProductos, graficoProductosMasVendidos.data.labels.length);

    for (let i = inicio; i < fin; i++) {
        const articulo = datosProductoGlobal[i].Articulo;
        const producto = graficoProductosMasVendidos.data.labels[i];
        const totalVendido = graficoProductosMasVendidos.data.datasets[0].data[i];

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

function obtenerTotalPaginasProductos() {
    return Math.ceil(datosProductoGlobal.length / filasPorPaginaProductos);
}

function actualizarEstadoBotonesPaginacionProductos() {
    const totalPaginas = obtenerTotalPaginasProductos();
    // console.log(datosProductoGlobal.length, filasPorPaginaProductos, totalPaginas);

    const btnAnterior = document.getElementById('btnAnteriorProductos');
    const btnSiguiente = document.getElementById('btnSiguienteProductos');

    btnAnterior.disabled = paginaActualProductos === 1;
    btnSiguiente.disabled = paginaActualProductos >= totalPaginas;

    // Ajustar los colores según el estado del botón
    btnAnterior.className = btnAnterior.disabled ? 'btn-custom disabled' : 'btn-custom enabled';
    btnSiguiente.className = btnSiguiente.disabled ? 'btn-custom disabled' : 'btn-custom enabled';
}


function cambiarPaginaProductos(incremento) {
    paginaActualProductos += incremento;
    llenarTablaProductosMasVendidos();
    actualizarEstadoBotonesPaginacionProductos();
}

