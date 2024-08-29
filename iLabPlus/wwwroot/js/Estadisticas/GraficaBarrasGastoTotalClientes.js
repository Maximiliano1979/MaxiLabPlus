let myChart;
let lastLoadedYear; // Variable para trackear el último año cargado


async function inicioGraficasBarrasGastoTotalClientes() {

    const selectAnio = document.getElementById('selectAnio');
    const selectCliente = document.getElementById('selectClientes');

    // Cargar años disponibles
    await cargarAniosDisponibles();

    // Evento para cambio de año
    selectAnio.addEventListener('change', async () => {
        const anioSeleccionado = selectAnio.value;
        if (anioSeleccionado !== lastLoadedYear) {
            await actualizarDatosParaAnio(anioSeleccionado);
        }
    });

    // Evento para cambio de cliente
    selectCliente.addEventListener('change', async (event) => {
        const clienteSeleccionado = event.target.value;
        const labelText = event.target.options[event.target.selectedIndex].text;
        await actualizarGrafico(clienteSeleccionado, labelText, selectAnio.value);
    });

    if (!myChart) {
        const ctx = document.getElementById('myBarChart').getContext('2d');
        myChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
                datasets: [{
                    label: 'Gasto Total de Clientes',
                    backgroundColor: Array(12).fill('rgba(104, 174, 255, 1)'),
                    borderWidth: 1
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
                        onClick: (e) => {
                           // if (e && e.stopPropagation) {
                                e.stopPropagation();
                           // }
                        }
                    }
                }
            }
        });
    }

    // Carga inicial de datos
    await actualizarDatosParaAnio(selectAnio.value);

}

async function actualizarDatosParaAnio(anio) {

    lastLoadedYear = anio;
    const clientes = await cargarClientesConGastos(anio);

    const selectCliente = document.getElementById('selectClientes');
    selectCliente.innerHTML = ''; // Limpiar opciones existentes

    if (clientes.length > 0) {
        clientes.forEach(cliente => {
            const option = document.createElement('option');
            option.value = cliente.ClienteId;
            option.textContent = `${cliente.ClienteId} - ${cliente.CliNombre} (Gasto: ${cliente.TotalGasto.toFixed(2)})`;
            selectCliente.appendChild(option);
        });

        const primerCliente = clientes[0];
        selectCliente.value = primerCliente.ClienteId;
        await actualizarGrafico(primerCliente.ClienteId, `${primerCliente.ClienteId} - ${primerCliente.CliNombre}`, anio);

    } else {

        const option = document.createElement('option');
        option.value = "";
        option.textContent = "Sin clientes para este año";
        selectCliente.appendChild(option);
        await actualizarGrafico(null, 'Gasto Total de Clientes', anio);

    }
}


// Asegúrate de que esta función sea asíncrona
const actualizarGrafico = async (clienteId = null, labelText = 'Gasto Total de Clientes', anio = null) => {
    try {
        let url = '/Estadisticas/ObtenerDatosGrafica';
        if (clienteId) {
            url += `?clienteId=${clienteId}&anio=${anio}`;
        } else {
            url += `?anio=${anio}`;
        }

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error('Ocurrió un error con el fetch');
        }

        const data = await response.json();

        const nuevosDatos = Array(12).fill(0);

        data.Datos.forEach(d => {
            nuevosDatos[d.Mes - 1] = parseFloat(d.Total);
        });

        myChart.data.datasets[0].label = labelText;
        myChart.data.datasets[0].data = nuevosDatos;

        myChart.update();
        llenarTabla();

    } catch (error) {

        console.error("Error al actualizar el gráfico: ", error);
        myChart.data.datasets[0].label = 'No hay datos disponibles';
        myChart.data.datasets[0].data = Array(12).fill(0);
        myChart.update();
        llenarTabla();
    }
};

function llenarTabla() {
    const tbody = document.getElementById('datosTabla').getElementsByTagName('tbody')[0];
    tbody.innerHTML = ''; // Limpiamos el contenido del tbody

    myChart.data.labels.forEach((mes, index) => {
        const fila = tbody.insertRow();
        const celdaMes = fila.insertCell();
        const celdaGasto = fila.insertCell();

        celdaMes.textContent = mes;
        // Aquí aplicamos el formateo solo para mostrar en la tabla
        celdaGasto.textContent = parseFloat(myChart.data.datasets[0].data[index]).toLocaleString('es-ES', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    });
}


const cargarClientesConGastos = async (anio) => {
    try {
        let url = `/Estadisticas/ObtenerClientesConGastosPorAnio?anio=${anio}`;
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error('Ocurrió un error con el fetch');
        }

        const data = await response.json();

        return data.Clientes || [];
    } catch (error) {
        console.error("Hubo un problema con la carga de clientes con gastos: ", error);
        return [];
    }
};


async function cargarAniosDisponibles() {
    try {
        const response = await fetch('/Estadisticas/ObtenerAniosDisponibles');
        if (!response.ok) {
            throw new Error('Error al obtener los años disponibles');
        }
        const anios = await response.json();

        const selectores = [
            document.getElementById('selectAnio'),
            document.getElementById('selectAnioPastel'),
            document.getElementById('selectAnioProductos')
        ];

        selectores.forEach(selector => {
            if (selector) {
                selector.innerHTML = ''; // Limpiar opciones existentes
                anios.forEach(anio => {
                    const option = document.createElement('option');
                    option.value = anio;
                    option.textContent = anio;
                    selector.appendChild(option);
                });
                if (anios.length > 0) {
                    selector.value = anios[anios.length - 1]; // Seleccionar el año más reciente
                }
            }
        });

        // Disparar evento de cambio en selectAnio si existe
        const selectAnio = document.getElementById('selectAnio');
        if (selectAnio) {
            selectAnio.dispatchEvent(new Event('change'));
        }

        return anios;
    } catch (error) {
        console.error("Error al cargar los años disponibles:", error);
        return [];
    }
}