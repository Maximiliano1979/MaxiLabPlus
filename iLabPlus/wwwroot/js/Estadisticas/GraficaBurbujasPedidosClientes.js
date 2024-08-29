let datosBurbujaGlobal = [];
let paginaActual = 1;
const filasPorPagina = 15;
let factorDeEscala = 2;
let bubbleChart;
const coloresBurbuja = [
    'rgba(255, 99, 132, 0.6)',    // rojo
    'rgba(54, 162, 235, 0.6)',    // azul
    'rgba(255, 206, 86, 0.6)',    // amarillo
    'rgba(75, 192, 192, 0.6)',    // verde agua
    'rgba(153, 102, 255, 0.6)',   // violeta
    'rgba(255, 159, 64, 0.6)',    // naranja
    'rgba(201, 203, 207, 0.6)',   // gris
    'rgba(233, 30, 99, 0.6)',     // rosa fuerte
    'rgba(0, 188, 212, 0.6)',     // cian
    'rgba(205, 220, 57, 0.6)',    // lima
    'rgba(96, 125, 139, 0.6)',    // azul grisáceo
    'rgba(255, 87, 34, 0.6)',     // naranja oscuro
    'rgba(121, 85, 72, 0.6)',     // marrón
    'rgba(158, 158, 158, 0.6)',   // gris claro
    'rgba(0, 150, 136, 0.6)'      // verde teal
];



function inicioGraficaBurbujasPedidosClientes() {

    if (!bubbleChart) {
        const ctxBurbuja = document.getElementById('bubbleChart').getContext('2d');
        bubbleChart = new Chart(ctxBurbuja, {
            type: 'bubble',
            data: {
                datasets: [{
                    label: 'Pedidos por Cliente',
                    data: [],
                    //backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Cantidad de Pedidos'
                        },
                        min: 0,
                        suggestedMax: 100
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Valor Total de los Pedidos'
                        },
                        min: 0,
                        suggestedMax: 1000000
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const dataItem = context.raw; // En la versión 3, se usa 'context' y 'raw'
                                return [
                                    'Cliente: ' + dataItem.cliNombre,
                                    'Cantidad de Pedidos: ' + dataItem.x.toLocaleString('es-ES', { useGrouping: true }),
                                    'Valor Total: ' + dataItem.y.toLocaleString('es-ES', { maximumFractionDigits: 2 }),
                                    'Peso Total: ' + (dataItem.r ** 2 * factorDeEscala ** 2).toLocaleString('es-ES', { maximumFractionDigits: 2 })
                                ];
                            }
                        }
                    },
                    legend: {
                        //onClick: function (event, legendItem, legend) {
                        //    // Esto previene que el clic en la leyenda oculte el dataset
                        //    event.stopPropagation();
                        //}
                        display: false
                    }
                },
            }
        });
    }


    cargarDatosBurbuja().then(() => {
        llenarTablaBurbujas(datosBurbujaGlobal);
        actualizarEstadoBotones(); // Asegúrate de que esta línea está aquí
    });

    // Añadimos el event listener al select
    document.getElementById('FiltroAnioBubble').addEventListener('change', (event) => {
        const anioSeleccionado = event.target.value;
        const todosLosAnios = anioSeleccionado === '';
        const datosFiltrados = filtrarDatosPorAnio(anioSeleccionado, datosBurbujaGlobal);
        generarBurbujas(datosFiltrados, todosLosAnios); // Actualizamos el gráfico con los datos filtrados
    });

    const filtrarDatosPorAnio = (anio, datosBurbuja) => {
        if (anio === '') {
            return datosBurbuja; // Si es "Todos los Años", devolvemos todos los datos
        } else {
            return datosBurbuja.filter(item => item.Anio === parseInt(anio));
        }
    };



    // Listener para el evento change del select de años
    document.getElementById('selectAnio').addEventListener('change', (event) => {
        const anioSeleccionado = event.target.value;
        const inicioAnio = `${anioSeleccionado}-01-01`;
        const finAnio = `${anioSeleccionado}-12-31`;
        cargarClientesConGastos(inicioAnio, finAnio);
        const clienteSeleccionado = document.getElementById('selectClientes').value;
        const labelText = clienteSeleccionado
            ? document.getElementById('selectClientes').options[document.getElementById('selectClientes').selectedIndex].text
            : 'Gasto Total de Clientes';
        actualizarGrafico(clienteSeleccionado, labelText, anioSeleccionado);
    });

    // });

}


const cargarDatosBurbuja = async () => {
    try {
        const response = await fetch('/Estadisticas/ObtenerDatosParaGraficoBurbujaConAnios');
        if (!response.ok) {
            throw new Error('Problema al obtener los datos');
        }

        const resultado = await response.json();
        // console.log(resultado); // Esto mostrará la estructura de los datos recibidos.

        datosBurbujaGlobal = resultado.Datos;
        const aniosConGastos = resultado.Anios;

        actualizarSelectAnios(aniosConGastos);
        generarBurbujas(datosBurbujaGlobal, true);
        llenarTablaBurbujas(datosBurbujaGlobal);
        actualizarEstadoBotones();
    } catch (error) {
        console.error('Hubo un problema al cargar los datos para el gráfico de burbujas: ', error);
    }
};



const generarBurbujas = (datosBurbuja, todosLosAnios = false) => {

    // Comprobación de que bubbleChart está definido
    if (!bubbleChart) {
        console.error("El gráfico de burbujas no está definido aún.");
        return;
    }

    let datosParaGraficar = todosLosAnios ? sumarDatosClientes(datosBurbuja) : datosBurbuja;

    datosParaGraficar = datosParaGraficar.sort((a, b) => b.ValorTotal - a.ValorTotal).slice(0, 50);

    // Encontrar el valor máximo de ValorTotal
    const valorMaximo = Math.max(...datosParaGraficar.map(item => item.ValorTotal));
    const cantidadMaxima = Math.max(...datosParaGraficar.map(item => item.CantidadPedidos));
    const radioMaximo = 20;

    // const coloresPorBurbuja = datosFormateados.map(() => coloresBurbuja[Math.floor(Math.random() * coloresBurbuja.length)]);



    //if (todosLosAnios) {
    //    datosParaGraficar = sumarDatosClientes(datosBurbuja);

    //}
    //// console.log("Estos son datos para graficar luego del if: ", datosParaGraficar);


    const datosFormateados = datosParaGraficar.map((item) => ({
        x: item.CantidadPedidos,
        y: item.ValorTotal,
        r: Math.sqrt(item.ValorTotal / valorMaximo) * radioMaximo,
        cliNombre: item.CliNombre,
    }));

    const coloresPorBurbuja = datosFormateados.map(() =>
        coloresBurbuja[Math.floor(Math.random() * coloresBurbuja.length)]
    );

    // Asumiendo que ya he creado el gráfico y que está referenciado por la variable `bubbleChart`
    bubbleChart.data.datasets[0].data = datosFormateados;
    bubbleChart.data.datasets[0].backgroundColor = coloresPorBurbuja;

    // Ajustar las escalas
    bubbleChart.options.scales.x.suggestedMax = cantidadMaxima * 1.1;
    bubbleChart.options.scales.y.suggestedMax = valorMaximo * 1.1;

    // console.log(datosFormateados);
    bubbleChart.update();

    llenarTablaBurbujas(datosParaGraficar);
};



function obtenerTotalPaginas() {
    return Math.ceil(datosBurbujaGlobal.length / filasPorPagina);
}

function cambiarPaginaBurbujas(incremento) {
    paginaActual += incremento;
    llenarTablaBurbujas(datosBurbujaGlobal);
    actualizarEstadoBotones();
}

function llenarTablaBurbujas(datos) {
    const inicio = (paginaActual - 1) * filasPorPagina;
    const fin = inicio + filasPorPagina;

    const datosPaginados = datos.slice(inicio, fin);

    const tbody = document.getElementById('tablaBurbujas').getElementsByTagName('tbody')[0];
    tbody.innerHTML = '';

    datosPaginados.forEach(dato => {
        const fila = tbody.insertRow();
        const celdaCliente = fila.insertCell();
        const celdaCantidadPedidos = fila.insertCell();
        const celdaValorTotal = fila.insertCell();

        celdaCliente.textContent = dato.CliNombre;
        celdaCantidadPedidos.textContent = dato.CantidadPedidos;
        celdaValorTotal.textContent = dato.ValorTotal.toLocaleString('es-ES', {
            maximumFractionDigits: 2
        });
    });
}






const actualizarSelectAnios = (anios) => {
    const selectAnio = document.getElementById('FiltroAnioBubble');
    selectAnio.innerHTML = ''; // Limpo las opciones anteriores

    // Opción para "Todos los Años"
    selectAnio.add(new Option('Todos los Años', ''));

    // Añadimos una opción por cada año recibido
    anios.forEach(anio => {
        selectAnio.add(new Option(anio, anio));
    });
};


const escalarValorTotal = (valor, valorMaximo, radioMaximo) => {
    return (valor / valorMaximo) * radioMaximo;
};





const sumarDatosClientes = (datos) => {
    const datosSumados = {};

    // Sumo los datos por cliente
    datos.forEach(dato => {
        if (datosSumados[dato.ClienteId]) {
            datosSumados[dato.ClienteId].CantidadPedidos += dato.CantidadPedidos;
            datosSumados[dato.ClienteId].ValorTotal += dato.ValorTotal;
            datosSumados[dato.ClienteId].PesoTotal += dato.PesoTotal;
        } else {
            datosSumados[dato.ClienteId] = {
                ClienteId: dato.ClienteId,
                CliNombre: dato.CliNombre,
                CantidadPedidos: dato.CantidadPedidos,
                ValorTotal: dato.ValorTotal,
                PesoTotal: dato.PesoTotal
            };
        }
    });

    // Convierto el objeto en un array
    return Object.values(datosSumados);
};



function actualizarEstadoBotones() {
    const totalPaginas = obtenerTotalPaginas();
    const btnAnterior = document.getElementById('btnAnterior');
    const btnSiguiente = document.getElementById('btnSiguiente');

    btnAnterior.disabled = paginaActual === 1;
    btnSiguiente.disabled = paginaActual === totalPaginas;

    // Ajustar los colores según el estado del botón
    btnAnterior.className = btnAnterior.disabled ? 'btn-custom disabled' : 'btn-custom enabled';
    btnSiguiente.className = btnSiguiente.disabled ? 'btn-custom disabled' : 'btn-custom enabled';
}