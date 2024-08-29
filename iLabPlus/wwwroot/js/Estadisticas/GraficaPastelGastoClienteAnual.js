let myPieChart;

// document.addEventListener('DOMContentLoaded', function () {

async function inicioGraficaPastelGastoClienteAnual() {
  
    const anioActual = new Date().getFullYear();
    await obtenerClientes(anioActual);

    document.getElementById('selectAnioPastel').addEventListener('change', async (event) => {
        const anioSeleccionado = event.target.value;
        await obtenerClientes(anioSeleccionado); // Asegúrate de que esta función complete antes de proceder.
        const selectRango = document.getElementById('selectRangoClientesPastel');
        if (selectRango.options.length > 0) {
            selectRango.selectedIndex = 0; // Selecciona el primer rango por defecto
            selectRango.dispatchEvent(new Event('change')); // Dispara el evento de cambio manualmente
        }
    });


    const selectRangoClientesPastel = document.getElementById('selectRangoClientesPastel');
    selectRangoClientesPastel.addEventListener('change', (event) => {
        const rangoSeleccionado = event.target.value;
        const [inicio, fin] = rangoSeleccionado.split('-').map(num => parseInt(num, 10));
        const selectAnioPastel = document.getElementById('selectAnioPastel');
        actualizarGraficoPastel(selectAnioPastel.value, inicio, fin);
    });

    if (!myPieChart) {
        const ctxPie = document.getElementById('myPieChart').getContext('2d');
        myPieChart = new Chart(ctxPie, {
            type: 'pie',
            data: {
                labels: [],
                datasets: [{ data: [] }]
            },
            options: {
                aspectRatio: 1.35,
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            let label = data.labels[tooltipItem.index] || '';
                            if (label) {
                                label += ': ';
                            }
                            label += parseFloat(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]).toLocaleString('es-ES', {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 2
                            });
                            return label;
                        }
                    }
                },
                plugins: {
                    datalabels: {
                        formatter: (value, ctx) => {
                            let datasets = ctx.chart.data.datasets[0];
                            let total = datasets.data.reduce((total, value) => total + value, 0);
                            let percentage = ((value / total) * 100).toFixed(2) + '%';
                            return percentage;
                        },
                        color: '#fff',
                        anchor: 'end',
                        align: 'start',
                        offset: 10
                    },
                },
            },
            plugins: [ChartDataLabels] // Asegurate de que ChartDataLabels está definido e importado
        });
    }




}

const obtenerClientes = async (anio) => {
    try {
        const response = await fetch(`/Estadisticas/TotalClientesConPedidos?anio=${anio}`);
        if (!response.ok) {
            throw new Error('Ocurrió un error al obtener los clientes.');
        }
        const { totalClientesConPedidos } = await response.json();

        // Verificar que totalClientes es un número
        if (typeof totalClientesConPedidos !== 'number') {
            console.error('totalClientes no es un número:', totalClientesConPedidos);
            return; // Si no es un número, no podemos continuar
        }

        const selectRangoClientesPastel = document.getElementById('selectRangoClientesPastel');
        if (!selectRangoClientesPastel) {
            console.error('El selectRangoClientesPastel no existe en el DOM aún');
            return; // Si no existe el elemento, no podemos continuar
        }

        selectRangoClientesPastel.innerHTML = ''; // Limpiamos las opciones anteriores para evitar duplicados

        // Llenar el select con rangos de clientes
        for (let i = 0; i < totalClientesConPedidos; i += 10) {
            const inicio = i + 1;
            const fin = Math.min(inicio + 9, totalClientesConPedidos);

            const optionElement = document.createElement('option');
            optionElement.value = `${inicio}-${fin}`;
            optionElement.text = `Clientes del ${inicio} al ${fin}`;
            selectRangoClientesPastel.appendChild(optionElement);
        }

        // Asegurarse de seleccionar el primer rango por defecto
        if (selectRangoClientesPastel.options.length > 0) {
            selectRangoClientesPastel.selectedIndex = 0; // Selecciona el primer rango por defecto
            selectRangoClientesPastel.dispatchEvent(new Event('change')); // Dispara el evento de cambio manualmente
        }

    } catch (error) {
        console.error("Algo salió realmente mal: ", error);
    }
}


const actualizarGraficoPastel = async (anio, inicio, fin) => {

    if (isNaN(inicio) || fin === undefined) {
        console.error('Los parámetros de inicio o fin son inválidos:', inicio, fin);
        return; // No continuar si los parámetros no son válidos
    }

    try {
        const response = await fetch(`/Estadisticas/ObtenerDatosGraficaPastel?anio=${anio}&inicio=${inicio}&fin=${fin}`);
        if (!response.ok) {
            throw new Error('Ocurrio un error con el fetch');
        }

        const data = await response.json();

        // console.log("Este es el console de data: ", data);
        const etiquetas = data.DatosPastel.map(d => `${d.ClienteId} - ${d.ClienteNombre}`);
        const totales = data.DatosPastel.map(d => parseFloat(d.Total));

        // Genero colores intercalados
        const colores = [];
        for (let i = 0; i < etiquetas.length; i++) {
            if (i % 2 === 0) {
                colores.push('rgba(104, 174, 255, 1)');
            } else {
                colores.push('rgba(255, 159, 64, 1)');
            }
        }

        myPieChart.data.labels = etiquetas;
        myPieChart.data.datasets[0].data = totales;
        myPieChart.data.datasets[0].backgroundColor = colores;
        myPieChart.update();
        llenarTablaPastel();

    } catch (error) {

        console.error("Algo salio mal con la carga del grafico de pastel: ", error)

    }
}



function llenarTablaPastel() {
    const tbody = document.getElementById('tablaPastel').getElementsByTagName('tbody')[0];
    tbody.innerHTML = '';

    myPieChart.data.labels.forEach((etiqueta, index) => {
        const fila = tbody.insertRow();
        const celdaCliente = fila.insertCell();
        const celdaGasto = fila.insertCell();

        celdaCliente.textContent = etiqueta.split(' - ')[1];
        celdaGasto.textContent = parseFloat(myPieChart.data.datasets[0].data[index]).toLocaleString('es-ES', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    });
}

document.addEventListener('DOMContentLoaded', inicioGraficaPastelGastoClienteAnual);
