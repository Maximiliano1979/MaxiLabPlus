let stockMinMaxChart;
let registrosPorPagina = 15;
let porcentajeAvisoStock = 25; // Inicializa la variable aquí para que sea global
let inicioActual = 1; // Almacena la página actual
let finActual = registrosPorPagina; // Almacena el fin actual


function inicioGraficaStocks() {

    cargarRangoStocks();
    cargarDatosStocks(inicioActual, finActual); // Carga inicial de datos



    const btnBuscar = document.getElementById('btnBuscarArticulo');
    const inputBuscar = document.getElementById('inputBuscarArticulo');

    const inputPorcentajeAviso = document.getElementById('inputPorcentajeAviso'); // Capturo porcentaje agregado por el usuario.

    // Evento de cambio en el input del porcentaje
    inputPorcentajeAviso.addEventListener('keypress', function (e) {
        porcentajeAvisoStock = parseInt(inputPorcentajeAviso.value);
        if (e.key === 'Enter') {
            cargarDatosStocks(inicioActual, finActual); // Recarga los datos con el nuevo porcentaje

        }
    });


    // Evento de clic en el botón de búsqueda
    btnBuscar.addEventListener('click', function () {
        buscarArticulo(inputBuscar.value);
    });

    // Evento de presionar tecla en el campo de entrada
    inputBuscar.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            buscarArticulo(inputBuscar.value);
        }
    });

    function cargarDatosStocks(inicio, fin) {
        inicioActual = inicio;
        finActual = fin; 
        fetch(`/Estadisticas/ObtenerStocks?inicio=${inicio}&fin=${fin}`)
            .then(response => response.json())
            .then(data => {
                // console.log("Datos recibidos:", data);
                crearGraficoStocks(data.Datos);
            })
            .catch(error => console.error("Error al obtener datos de stocks: ", error));
    }


    function cargarRangoStocks() {
        fetch('/Estadisticas/ObtenerTotalStocks')
            .then(response => response.json())
            .then(data => {
                const totalRegistros = data.Total;
                const selectRangoStocks = document.getElementById('selectRangoStocks');

                for (let i = 0; i < totalRegistros; i += 15) {
                    const inicio = i + 1;
                    const fin = Math.min(inicio + 14, totalRegistros);
                    const option = new Option(`Del ${inicio} al ${fin}`, `${inicio}-${fin}`);
                    selectRangoStocks.appendChild(option);
                }
            })
            .catch(error => console.error("Error al obtener el total de stocks: ", error));

        const selectRangoStocks = document.getElementById('selectRangoStocks');
        selectRangoStocks.addEventListener('change', (event) => {
            const [inicio, fin] = event.target.value.split('-').map(Number);
            cargarDatosStocks(inicio, fin);
        });
    }



    function crearGraficoStocks(datos) {
        if (stockMinMaxChart) {
            stockMinMaxChart.destroy();
        }

        const ctxStockMinMax = document.getElementById('stockMinMaxChart').getContext('2d');

     
        stockMinMaxChart = new Chart(ctxStockMinMax, {
            type: 'bar',
            data: {
                labels: datos.map(d => d.Articulo),
                datasets: [
                    {
                        label: 'Stock Mínimo',
                        data: datos.map(d => d.StockMinimo),
                        backgroundColor: 'rgba(168, 167, 159, 0.8)'
                    },
                    {
                        label: 'Stock Físico',
                        data: datos.map(d => d.StockActual),
                        backgroundColor: datos.map(d => {
                            const porcentajeDelMinimo = (d.StockActual / d.StockMinimo) * 100;

                            if (d.StockActual < d.StockMinimo) {
                                return 'rgba(242, 9, 9, 0.8)'; // Rojo si está por debajo del mínimo
                            } else if (porcentajeDelMinimo <= (100 + porcentajeAvisoStock) && porcentajeDelMinimo >= (100 - porcentajeAvisoStock)) {
                                return 'rgba(242, 236, 79, 0.8)'; // Amarillo si está cerca del mínimo (dentro del 15%)
                            } else {
                                return 'rgba(54, 162, 235, 0.6)'; // Azul si está fuera del rango del 15%
                            }
                        }),
                        barPercentage: 1.0,
                        categoryPercentage: 1.0
                    },
                    {
                        label: 'Stock Máximo',
                        data: datos.map(d => d.StockMaximo),
                        backgroundColor: 'rgba(75, 192, 192, 0.8)'
                    }
                ]
            },
            options: {
                scales: {
                    x: {
                        stacked: false
                    },
                    y: {
                        stacked: false
                    }
                },
                
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            let label = data.datasets[tooltipItem.datasetIndex].label || '';
                            if (label) {
                                label += ': ';
                            }
                            label += tooltipItem.yLabel;
                            return label;
                        }
                        
                    }
                },
                plugins: {
                    legend: {
                        display: false // Aquí se ocultan las leyendas
                    }
                }
                
            }
        });
    }


    function buscarArticulo(articulo) {
        fetch(`/Estadisticas/BuscarStocks?articulo=${encodeURIComponent(articulo)}`)
            .then(response => response.json())
            .then(data => {
                // console.log("Datos recibidos:", data);
                crearGraficoStocks(data.Datos);
            })
            .catch(error => console.error("Error al buscar datos de stocks: ", error));
    }

}


