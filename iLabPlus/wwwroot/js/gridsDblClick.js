window.addEventListener('load', OnLoadCommon);

let theGridDBL;

/***** onload() ******/
function OnLoadCommon() {

    $('.GridEvents').each(function () {

        theGridDBL = wijmo.Control.getControl("#" + this.id);

        theGridDBL.addEventListener(theGridDBL.hostElement, 'dblclick', function (e) {
            var ht = theGridDBL.hitTest(e);
            if (ht.panel === theGridDBL.cells) {
                let GridId = theGridDBL._orgAtts.id.nodeValue;
                let Guid = theGridDBL.collectionView.currentItem.Guid;

                switch (GridId) {
                    case 'gridContaBancos':
                        N_DialogBanco(Guid).then((res) => {
                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridContaBancos"), res);
                        }).catch((error) => {
                        });
                        break;

                    case 'gridStocks':
                        N_DialogStocks(Guid).then((res) => {
                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridStocks"), res);
                        }).catch((error) => {
                        });
                        break;

                    case 'gridControlHorarioAdmin':
                        N_DialogControlHorarioAdmin(Guid).then((res) => {
                            console.log("Este es el resultado de res de gridsDblClick.js: " + res);
                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridControlHorarioAdmin"), res);
                        }).catch((error) => {
                        });
                        break;

                    case 'gridDivisas':

                        N_DialogDivisa(Guid).then((res) => {
                            formatearFechas(res);
                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridDivisas"), res);
                        }).catch((error) => {
                        });

                        break;

                    case 'gridClientes':

                        N_DialogCliente(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridClientes"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridContactos':

                        N_DialogContacto(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridContactos"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridEmpleados':

                        N_DialogEmpleado(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridEmpleados"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridFases':

                        N_DialogFase(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridFases"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridDoctores':

                        N_DialogDoctor(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridDoctores"), res);

                        }).catch((error) => {

                        });

                        break;



                    case 'gridTarifas':

                        N_DialogTarifa(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridTarifas"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridProveedores':

                        N_DialogProveedor(Guid).then((res) => {
                            console.log("Este es el resultado de res de gridsDblClick.js: " + res);

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridProveedores"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridVendedores':

                        N_DialogVendedor(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridVendedores"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridOperariosEXT':

                        N_DialogOperario(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridOperariosEXT"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridValsys':

                        N_DialogValsys(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridValsys"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridArticulos':

                        window.location.href = "/Articulos/Ficha/" + Guid;

                        break;

                    case 'gridPedidos':

                        window.location.href = "/Pedidos/Ficha/" + Guid;

                        break;

                    case 'gridFacturas':

                        window.location.href = "/Facturas/Ficha/" + Guid;

                        break;

                    case 'gridAlbaranes':

                        window.location.href = "/Albaranes/Ficha/" + Guid;

                        break;

                    case 'gridFixing':

                        N_DialogFixing(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridFixing"), res);

                        }).catch((error) => {

                        });

                        break;

                    case 'gridLeyAntiFraude':

                        N_DialogLeyAntiFraude(Guid).then((res) => {


                        }).catch((error) => {

                        });

                        break;

                    default:
                        console.log("grid dblclick: " + GridId);
                        break;
                };

            }
        });

    });




}









