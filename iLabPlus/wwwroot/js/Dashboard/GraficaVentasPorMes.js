var misVentasPorMes;

async function cargarVentasPorMes(year = new Date().getFullYear()) {
    try {
        const response = await fetch('/Dashboard/ObtenerVentasPorMes?year=' + year);
        if (!response.ok) {
            throw new Error('Error al obtener datos de ventas por mes');
        }

        const datosVentas = await response.json();
        mostrarGraficoVentasPorMes(datosVentas);
    } catch (error) {
        console.error('Error al cargar ventas por mes:', error);
    }
}


function mostrarGraficoVentasPorMes(datos) {
    try {
        const nombreMes = obtenerNombreMes();
        const ctx = document.getElementById('ventasPorMesCanvas').getContext('2d');

        // Genera una lista de etiquetas para todos los días del mes
        const diasMes = new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0).getDate();
        const diasLabels = Array.from({ length: diasMes }, (_, i) => i + 1);

        // Inicializa un arreglo con 0 para todos los días del mes
        const datosMes = new Array(diasMes).fill(0);

       
        // Asigna los datos de ventas a los días correspondientes
        datos.forEach(item => {
            if (item.Dia <= diasMes) {
                datosMes[item.Dia - 1] = item.TotalVentas;
            }
        });
        
        if (misVentasPorMes == undefined) {
            // Si misVentasPorMes no existe, crea un nuevo gráfico
            misVentasPorMes = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: diasLabels,
                    datasets: [{
                        label: `Ventas diarias del Mes de ${nombreMes}`,
                        data: datosMes,
                        backgroundColor: 'rgba(36, 103, 174, 1)'
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true,
                            afterDataLimits: axis => {
                                // da error
                                //const maxDataValue = Math.max(...misVentasPorMes.data.datasets[0].data);
                                //axis.max = maxDataValue + maxDataValue * 0.05;
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
                                    return `Día ${tooltipItems[0].label}`;
                                }
                            }
                        },
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
            // Si misVentasPorMes ya existe, actualiza los datos y redibuja
            misVentasPorMes.data.labels = diasLabels;
            misVentasPorMes.data.datasets[0].data = datosMes;
            misVentasPorMes.options.scales.y.afterDataLimits = axis => {
                const maxDataValue = Math.max(...misVentasPorMes.data.datasets[0].data);
                axis.max = maxDataValue + maxDataValue * 0.05;
            };
            misVentasPorMes.update(); // Actualiza el gráfico
        }

    } catch (error) {
        console.error('Error al mostrar gráfico de ventas por mes:', error);
    }
}


$(document).ready(function () {

    var currentYear = new Date().getFullYear();
    cargarVentasPorMes(currentYear);


    $('#selectorAnio').change(function () {
        var selectedYear = $(this).val();
        cargarVentasPorMes(selectedYear);

    });
});



