async function cargarYMostrarGraficoEntregasSemana(year) {
    try {
        const response = await fetch('/Dashboard/ObtenerInfoEntregasSemana?year=' + year);
        if (!response.ok) throw new Error('Error al obtener datos');

        const { TotalEntregas, EntregasRealizadas } = await response.json();

        console.log(TotalEntregas, EntregasRealizadas); // Línea para depuración


        const porcentajeRealizado = Math.round((EntregasRealizadas / TotalEntregas) * 100);

        mostrarGraficoEntregasSemana(porcentajeRealizado);
    } catch (error) {
        console.error('Error al cargar el gráfico de entregas de la semana:', error);
    }
}

function mostrarGraficoEntregasSemana(porcentaje) {
    var options = {
        series: [porcentaje],
        chart: {
            // height: 350,
            height: 131, // Altura del gráfico
            width: 193,  // Anchura del gráfico
            type: 'radialBar',
            // offsetY: -10
        },
        plotOptions: {
            radialBar: {
                startAngle: -135,
                endAngle: 135,
                dataLabels: {
                    name: {
                        fontSize: '16px',
                        color: undefined,
                        offsetY: 120
                    },
                    value: {
                        offsetY: 76,
                        fontSize: '22px',
                        color: undefined,
                        formatter: function (val) {
                            return val + "%";
                        }
                    }
                }
            }
        },
        fill: {
            type: 'gradient',
            gradient: {
                shade: 'dark',
                shadeIntensity: 0.15,
                inverseColors: false,
                opacityFrom: 1,
                opacityTo: 1,
                stops: [0, 50, 65, 91]
            },
        },
        stroke: {
            dashArray: 4
        },
        labels: ['Entregas Realizadas'],
     
    };

    var chart = new ApexCharts(document.querySelector("#graficoEntregasSemana"), options);
    chart.render();
}


// Cargar el gráfico cuando la página esté lista
//document.addEventListener('DOMContentLoaded', function () {
//    var currentYear = new Date().getFullYear();
//    cargarYMostrarGraficoEntregasSemana(currentYear);
//});


//$(document).ready(function () {
//    // Inicialización con el año actual
//    var currentYear = new Date().getFullYear();
//    cargarYMostrarGraficoEntregasSemana(currentYear);
//    // ... otras inicializaciones ...

//    // Manejador del evento change para el selector de año
//    $('#selectorAnio').change(function () {
//        var selectedYear = $(this).val();
//        cargarYMostrarGraficoEntregasSemana(selectedYear);
//        // ... otras llamadas a funciones con el año seleccionado ...
//    });
//});


