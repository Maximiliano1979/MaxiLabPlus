async function cargarEntregasPendSemana(year) {

    const response = await fetch('/Dashboard/ObtenerEntregasPendSemana?year=' + year);
    if (!response.ok) {
        console.error('Error al obtener entregas de la semana');
        return;
    }

    const entregasPendSemana = await response.json();
    
    mostrarGraficoEntregasPendSemana(entregasPendSemana);
}

var chart;

function mostrarGraficoEntregasPendSemana(entregasPendSemana) {
    
    var Porcentaje = entregasPendSemana.Porcentaje.toFixed(0);

    var options = {
        series: [Porcentaje],
        chart: {
            type: 'radialBar',
            offsetY: -20,
            sparkline: {
                enabled: true
            }
        },
        plotOptions: {
            radialBar: {
                startAngle: -90,
                endAngle: 90,
                track: {
                    background: "#e7e7e7",
                    strokeWidth: '97%',
                    margin: 5, // margin is in pixels
                    dropShadow: {
                        enabled: true,
                        top: 2,
                        left: 0,
                        color: '#999',
                        opacity: 1,
                        blur: 2
                    }
                },
                dataLabels: {
                    name: {
                        show: false
                    },
                    value: {
                        offsetY: -2,
                        fontSize: '22px'
                    }
                }
            }
        },
        grid: {
            padding: {
                top: 20
            }
        },
        fill: {
            type: 'gradient',
            gradient: {
                shade: 'light',
                shadeIntensity: 0.4,
                inverseColors: false,
                opacityFrom: 1,
                opacityTo: 1,
                stops: [0, 50, 53, 91]
            },
        },
        labels: ['Average Results'],
    };

    if (chart) {
        // Si existe, actualizar los datos y renderizar nuevamente
        chart.updateSeries([Porcentaje]);
    } else {
        // Si no existe, crear un nuevo objeto ApexCharts
        chart = new ApexCharts(document.querySelector("#graficoEntregasPendSemana"), options);
        chart.render();
    }

}



$(document).ready(function () {

    var currentYear = new Date().getFullYear();
    cargarEntregasPendSemana(currentYear);

    // Manejador del evento change para el selector de año
    $('#selectorAnio').change(function () {
        var selectedYear = $(this).val();
        cargarEntregasPendSemana(selectedYear);
    });
});
