let myAnnualChart;

function inicioGraficaAnualGasto() {
    const container = document.getElementById('myAnnualBarChartWrapper');

    if (!container) {
        console.error('El contenedor del gráfico no se encontró');
        return;
    }

    // Asegurarse de que el contenedor tenga dimensiones
    container.style.height = '400px';
    container.style.width = '100%';

    if (!myAnnualChart) {
        const canvas = document.createElement('canvas');
        canvas.id = 'myAnnualBarChart';
        container.innerHTML = '';
        container.appendChild(canvas);
        const ctxAnnual = canvas.getContext('2d');

        myAnnualChart = new Chart(ctxAnnual, {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Gasto Total por Año',
                    backgroundColor: 'rgba(104, 174, 255, 1)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1,
                    data: []
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: {
                        display: true
                    }
                }
            }
        });
    }

    actualizarGraficoAnual();
}

const actualizarGraficoAnual = async () => {
    try {
        const response = await fetch('/Estadisticas/ObtenerTotalGastosPorAnio');
        if (!response.ok) {
            throw new Error('Ocurrió un error al cargar los gastos por año.');
        }
        const { GastosPorAnio } = await response.json();

        const etiquetas = GastosPorAnio.map(d => d.Anio.toString());
        const totales = GastosPorAnio.map(d => d.TotalGasto);

        myAnnualChart.data.labels = etiquetas;
        myAnnualChart.data.datasets[0].data = totales;
        myAnnualChart.update();

        llenarTablaGraficoAnual();
    } catch (error) {
        console.error("Hubo un problema al cargar los gastos por año: ", error);
    }
}

function llenarTablaGraficoAnual() {
    const tbody = document.getElementById('tablaGraficoAnual').getElementsByTagName('tbody')[0];
    tbody.innerHTML = '';
    myAnnualChart.data.labels.forEach((anio, index) => {
        const fila = tbody.insertRow();
        const celdaAnio = fila.insertCell();
        const celdaGasto = fila.insertCell();
        celdaAnio.textContent = anio;
        celdaGasto.textContent = parseFloat(myAnnualChart.data.datasets[0].data[index]).toLocaleString('es-ES', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    });
}

document.addEventListener('DOMContentLoaded', inicioGraficaAnualGasto);