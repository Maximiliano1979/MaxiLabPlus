var ventasPorMesesAnio;

async function cargarVentasPorMesesDelAnio(year) {
    try {

        const response = await fetch('/Dashboard/ObtenerVentasPorMesesDelAnio?year=' + year);
        if (!response.ok) {
            throw new Error('Error al obtener datos de ventas mensuales');
        }

        const datosVentas = await response.json();
        mostrarGraficoVentasPorMeses(datosVentas);
    } catch (error) {
        console.error('Error al cargar ventas mensuales:', error);
    }
}

function mostrarGraficoVentasPorMeses(datos) {
    try {

        const ctx = document.getElementById('ventasPorMesesCanvas').getContext('2d');

        const mesesLabels = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
        const datosMeses = new Array(12).fill(0);

        datos.forEach(item => {
            if (item.Mes <= 12) {
                datosMeses[item.Mes - 1] = item.TotalVentas;
            }
        });

        if (ventasPorMesesAnio == undefined) {
            // Si ventasPorMesesAnio no existe, crea un nuevo gráfico
            ventasPorMesesAnio = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: mesesLabels,
                    datasets: [{
                        label: 'Ventas Mensuales del Año',
                        data: datosMeses,
                        backgroundColor: 'rgba(36, 103, 174, 1)'
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true,
                            //afterDataLimits: axis => {
                            //    const maxDataValue = Math.max(...ventasPorMesesAnio.data.datasets[0].data);
                            //    axis.max = maxDataValue + maxDataValue * 0.05;
                            //}
                        }
                    },
                    plugins: {
                        legend: {
                            display: true,
                            onClick: (e) => {
                                if (e && e.stopPropagation) {
                                    e.stopPropagation();
                                }
                            }
                        }
                    }
                }
            });
        } else {
            // Si ventasPorMesesAnio ya existe, actualiza los datos y redibuja
            ventasPorMesesAnio.data.datasets[0].data = datosMeses;
            ventasPorMesesAnio.options.scales.y.afterDataLimits = axis => {
                const maxDataValue = Math.max(...ventasPorMesesAnio.data.datasets[0].data);
                axis.max = maxDataValue + maxDataValue * 0.05;
            };
            ventasPorMesesAnio.update(); // Actualiza el gráfico
        }

    } catch (error) {
        console.error('Error al mostrar gráfico de ventas mensuales:', error);
    }
}


$(document).ready(function () {

    var currentYear = new Date().getFullYear();
    cargarVentasPorMesesDelAnio(currentYear);

    $('#selectorAnio').change(function () {
        var selectedYear = $(this).val();
        cargarVentasPorMesesDelAnio(selectedYear);

    });
});



