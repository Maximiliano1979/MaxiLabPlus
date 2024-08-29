let dashFechasPedidosEntregas;

async function cargarDatosGraficosPedidos(year = new Date().getFullYear()) {
    const response = await fetch('/Dashboard/ObtenerDatosPedidosPorDia?year=' + year);
    if (!response.ok) {
        console.error('Error al obtener datos de pedidos');
        return;
    }

    const datos = await response.json()
    mostrarGraficoPedidos(datos);
    dashFechasPedidosEntregas.update();


}

const obtenerNombreMes = () => {
    const meses = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];
    const fechaActual = new Date();
    return meses[fechaActual.getMonth()];
}

function mostrarGraficoPedidos(datos) {
    const nombreMes = obtenerNombreMes();
    const ctx = document.getElementById('pedidosPorDiaDashboard').getContext('2d');

    if (dashFechasPedidosEntregas) {
        dashFechasPedidosEntregas.destroy();
    }

    dashFechasPedidosEntregas = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: datos.map(item => item.Dia),
            datasets: [{
                label: `Cantidad de pedidos por dia en ${nombreMes}`,
                data: datos.map(item => item.CantidadPedidos),
                backgroundColor: 'rgba(36, 103, 174, 1)'
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    afterDataLimits: axis => {
                        if (!dashFechasPedidosEntregas || !dashFechasPedidosEntregas.data.datasets[0].data) return;
                        const maxDataValue = Math.max(...dashFechasPedidosEntregas.data.datasets[0].data);
                        // console.log(maxDataValue, maxDataValue + maxDataValue * 0.05);
                        axis.max = maxDataValue + maxDataValue * 0.05;
                    }
                },
                x: {
                    ticks: {
                        autoSkip: false
                    }
                }
               
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        title: function (tooltipItems) {
                            // Personalizo el título del tooltip
                            return `Día ${tooltipItems[0].label}`;
                        }
                    }
                },
                legend: {
                    display: true, // Configuro la leyenda
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

async function cargarEntregasHoy(year = new Date().getFullYear()) {
    try {
        const response = await fetch('/Dashboard/ObtenerEntregasHoy?year=' + year);
        if (!response.ok) {
            throw new Error('No se pudieron obtener las entregas de hoy');
        }
        const data = await response.json();

        const fechaActual = new Date();
        const opciones = { weekday: 'long', day: 'numeric' };
        const fechaFormateada = fechaActual.toLocaleDateString('es-ES', opciones);

        // Asegurándonos de que actualizamos correctamente cada div
        document.getElementById('leyendaDia').textContent = `ENTREGAS - ${fechaFormateada}`.toUpperCase();
        document.getElementById('entregasHoy').textContent = data.cantidad;
    } catch (error) {
        console.error('Error:', error);
    }
}



$(document).ready(function () {
    // Código existente para cargar datos iniciales
    var currentYear = new Date().getFullYear();
    cargarDatosGraficosPedidos(currentYear);
    cargarEntregasHoy(currentYear);
    

    // Manejador del evento change para el selector de año
    $('#selectorAnio').change(function () {
        var selectedYear = $(this).val();
        cargarDatosGraficosPedidos(selectedYear);
        cargarEntregasHoy(selectedYear);

    });

});

