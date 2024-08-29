let entregasSemanales;

async function cargarEntregasSemana(year) {

    const response = await fetch('/Dashboard/ObtenerEntregasSemana?year=' + year);
    if (!response.ok) {
        console.error('Error al obtener entregas de la semana');
        return;
    }

    const entregasSemana = await response.json();

    mostrarGraficoEntregasSemana(entregasSemana, year);
}


function mostrarGraficoEntregasSemana(entregas, year) {

    const canvas = document.getElementById('graficoEntregasSemana');
    if (!canvas) {
        console.error('Elemento canvas no encontrado');
        return;
    }

    // Destruir el gráfico existente si ya está definido
    if (entregasSemanales) {
        entregasSemanales.destroy();
    }

    const ctx = canvas.getContext('2d');

    const hoy = new Date();
    let inicioSemana = new Date(year, hoy.getMonth(), hoy.getDate());
    inicioSemana.setDate(inicioSemana.getDate() - inicioSemana.getDay() + 1); // Ajusto al lunes más cercano

    const datosGrafico = [];
    const colores = [];

    for (let i = 0; i < 7; i++) {
        const fecha = new Date(inicioSemana);
        fecha.setDate(fecha.getDate() + i);

        // Corrección aquí: Asegurándose de que la comparación sea con el día correcto
        const diaEntrega = entregas.find(e => e.DiaSemana === fecha.getDay());

        // Acá creo una fecha solo con año, mes y día para la comparación
        const fechaComparacion = new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate());
        const hoyComparacion = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());


        datosGrafico.push({
            dia: fecha.getDate(),
            cantidad: diaEntrega ? diaEntrega.CantidadEntregas : 0
        });

        if (fechaComparacion < hoyComparacion) {
            colores.push('#BBBBBB'); //Dias pasado
        } else if (fechaComparacion.getTime() === hoyComparacion.getTime()) {
            colores.push('rgba(0, 128, 0, 1)'); // Dia actual
        } else {
            colores.push('rgba(36, 103, 174, 9)') // Dias futuros
        }

    }

    entregasSemanales = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: datosGrafico.map(d => d.dia),
            datasets: [{
                label: 'Entregas semanales',
                data: datosGrafico.map(d => d.cantidad),
                backgroundColor: colores
            }]
        },
        // Configuraciones adicionales...
        options: {
            plugins: {
                legend: {
                    display: false, // Configuro la leyenda
                    labels: {
                        usePointStyle: true, // Usa un estilo de punto en lugar de un cuadrado
                        boxWidth: 0, // Establece el ancho del cuadro a 0 para ocultarlo
                    },
                    onClick: (e) => {
                        if (e && e.stopPropagation) {
                            e.stopPropagation();
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        title: function (tooltipItems) {
                            // Personalizo el título del tooltip
                            return `Día ${tooltipItems[0].label}`;
                        },
                        label: function (tooltipItem) {
                            // Modificar el texto que aparece para cada punto de datos
                            return `Entregas: ${tooltipItem.raw}`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    afterDataLimits: axis => {
                        if (!entregasSemanales || !entregasSemanales.data.datasets[0].data) return;
                        const maxDataValue = Math.max(...entregasSemanales.data.datasets[0].data);
                        axis.max = maxDataValue + maxDataValue * 0.05;
                    }

                }
            }
        }
    });
}



$(document).ready(function () {
    var currentYear = new Date().getFullYear();
    cargarEntregasSemana(currentYear);

    $('#selectorAnio').change(function () {
        var selectedYear = $(this).val();
        cargarEntregasSemana(selectedYear);

    });
});
