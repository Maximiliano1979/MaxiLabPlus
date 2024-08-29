let graficoRupturaDeStocks;
let inicioActualRuptura = 1;
let registrosPorPaginaRuptura = 15;


/*document.addEventListener('DOMContentLoaded', function () {*/

function inicioRupturaDeStocks() {
    cargarRangoRupturaDeStocks();
    cargarDatosRupturaDeStocks();

    const selectRangoRupturaStocks = document.getElementById('selectRangoRupturaStocks');

    selectRangoRupturaStocks.addEventListener('change', function () {
        const rango = selectRangoRupturaStocks.value.split('-').map(Number);
        cargarDatosRupturaDeStocks(rango[0], rango[1]);
    });

    function cargarRangoRupturaDeStocks() {
        fetch('/Estadisticas/ObtenerTotalStocksRuptura')
            .then(response => response.json())
            .then(data => {
                const totalRegistros = data.Total;
                for (let i = 0; i < totalRegistros; i += 15) {
                    const inicio = i + 1;
                    const fin = Math.min(inicio + 14, totalRegistros);
                    const option = new Option(`Del ${inicio} al ${fin}`, `${inicio}-${fin}`);
                    selectRangoRupturaStocks.appendChild(option);
                }
            })
            .catch(error => console.error("Error al obtener el total de stocks de ruptura: ", error));
    }

    function cargarDatosRupturaDeStocks(inicio = 1, fin = 15) {
        fetch(`/Estadisticas/ObtenerRupturaDeStocks?inicio=${inicio}&fin=${fin}`)
            .then(response => response.json())
            .then(data => {
                crearGraficoRupturaDeStocks(data.Datos);
            })
            .catch(error => console.error("Error al obtener datos de ruptura de stocks: ", error));
    }

    function crearGraficoRupturaDeStocks(datos) {

        // console.log("Datos del gráfico:", datos);

        const canvas = document.getElementById('rupturaDeStocks');
        const ctx = canvas.getContext('2d');

        // Destruye el gráfico existente si ya hay uno
        if (graficoRupturaDeStocks) {
            graficoRupturaDeStocks.destroy();
        }


    graficoRupturaDeStocks = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: datos.map(d => d.Articulo),
            datasets: [{
                label: 'Stock Fisico',
                data: datos.map(d => d.StockActual),
                backgroundColor: 'rgba(54, 162, 235, 0.5)'
            }, {
                label: 'Stock Mínimo',
                data: datos.map(d => d.StockMinimo - d.StockActual),
                backgroundColor: 'rgba(255, 99, 132, 0.8)'
            }]
        },
        options: {
            scales: {
                x: {
                    stacked: true
                },
                y: {
                    stacked: true
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {

                            //console.log('Tooltip callback ejecutado', datos);
                            //console.log("Datos dentro del tooltip:", datos);

                            console.log("Callback del Tooltip ejecutado");


                            let label = context.dataset.label || '';
                            console.log(`Etiqueta actual del dataset: '${label}'`);

                            if (label) {
                                label += ': ';
                            }

                            if (label.trim() === 'Stock Mínimo:') {
                                // Suma de Stock Fisico y Stock Mínimo

                                console.log("Entrando en el bloque de Stock Mínimo");


                                const stockFisico = datos[context.dataIndex].StockActual;
                                const stockMinimo = datos[context.dataIndex].StockMinimo;
                                const sumaTotal = stockFisico + stockMinimo;

                                console.log(`Stock Fisico: ${stockFisico}, Stock Minimo: ${stockMinimo}, Suma: ${sumaTotal}`);

                                return label + stockMinimo;
                            } else {
                                // Para otros casos (Stock Fisico), mostramos el valor normal

                                console.log("Entrando en el bloque de otro dataset");

                                return label + context.parsed.y;
                            }
                        }
                    }
                }
            }
        }
    });
  }

}
/*});*/