window.addEventListener('load', OnLoadCommon);

let theGridC;

/***** onload() ******/
function OnLoadCommon() {

    var menu = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar'.split(','),

        itemClicked: function (s, e) {

            let GridId  = theGridC._orgAtts.id.nodeValue;
            let Guid    = theGridC.collectionView.currentItem.Guid;

            // EDITAR
            if (menu.selectedIndex == 0) {


                switch (GridId) {
                    case 'gridBancos':
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


                    case 'gridControlHorarioAdmin':
                        N_DialogControlHorarioAdmin(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridControlHorarioAdmin"), res);
                        }).catch((error) => {

                        });
                        break;

                    case 'gridDivisas':
                        N_DialogDivisa(Guid).then((res) => {
                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridDivisas"), res);
                        }).catch((error) => {

                        });
                        break;

                    case 'gridContactos':

                        N_DialogContacto(Guid).then((res) => {

                            N_Grid_Data_Refresh(wijmo.Control.getControl("#gridContactos"), res);

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


                    default:
                        console.log("grid event UPDATE sub: " + GridId);
                        break;
                };

            }

            // ELIMINAR
            if (menu.selectedIndex == 1) {

                switch (GridId) {
                    case 'gridContaBancos':

                        N_ModalConfirmation('¿ Desea eliminar el banco seleccionado?').then((res) => {
                            if (res == true) {
                                var Spinner = Rats.UI.LoadAnimation.start();
                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/Contabilidad/Delete_BANCO",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });
                            }
                        }).catch((error) => {
                        });

                        break;

                    case 'gridFases':

                        N_ModalConfirmation('¿ Desea eliminar la fase seleccionada?').then((res) => {
                            if (res == true) {
                                var Spinner = Rats.UI.LoadAnimation.start();
                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/Fases/Delete_Fase",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });
                            }
                        }).catch((error) => {
                        });

                        break;

                    case 'gridDoctores':

                        N_ModalConfirmation('¿ Desea eliminar al doctor seleccionado?').then((res) => {
                            if (res == true) {

                                var Spinner = Rats.UI.LoadAnimation.start();
                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/Doctores/Delete_Doctor",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });
                            }
                        }).catch((error) => {
                        });

                        break;

                    case 'gridControlHorarioAdmin':

                        N_ModalConfirmation('¿ Desea eliminar el registro horario seleccionado?').then((res) => {
                            if (res == true) {
                                var Spinner = Rats.UI.LoadAnimation.start();
                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/ControlHorario/DeleteRegistroHorario",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });
                            }
                        }).catch((error) => {
                        });

                        break;

                    case 'gridStocks':

                        N_ModalConfirmation('¿ Desea eliminar el stock seleccionad8?').then((res) => {
                            if (res == true) {
                                var Spinner = Rats.UI.LoadAnimation.start();
                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/Stocks/Delete_STOCKS",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });
                            }
                        }).catch((error) => {
                        });

                        break;

                    case 'gridDivisas':

                        N_ModalConfirmation('¿ Desea eliminar la divisa seleccionada?').then((res) => {
                            if (res == true) {
                                var Spinner = Rats.UI.LoadAnimation.start();
                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/Divisas/Delete_DIVISA",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });
                            }
                        }).catch((error) => {
                        });

                        break;


                    case 'gridFixing':

                        N_ModalConfirmation('¿ Desea eliminar el registro seleccionado?').then((res) => {

                            if (res == true) {

                                var Spinner = Rats.UI.LoadAnimation.start();

                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/Fixing/DeleteFixing",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });


                            }

                        }).catch((error) => {

                        });

                        break;

                    case 'gridOperariosEXT':

                        N_ModalConfirmation('¿ Desea eliminar el operario seleccionado?').then((res) => {

                            if (res == true) {

                                var Spinner = Rats.UI.LoadAnimation.start();

                                var data = new FormData();
                                data.append("Guid", Guid);

                                $.ajax({
                                    type: 'POST',
                                    url: "/OperariosEXT/Delete_Operario",
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    success: function (data) {
                                        Spinner.stop();
                                        if (data != null) {
                                            N_Grid_Data_Delete(theGridC, Guid);
                                        }
                                    },
                                    always: function (data) { },
                                    error: function (xhr, status, error) {
                                        Spinner.stop();
                                        console.log("Error", xhr, xhr.responseText, status, error);
                                    }
                                });


                            }

                        }).catch((error) => {

                        });

                        break;

                    case 'gridValsys':


                        break;


                    default:
                        //console.log("grid event DELETE sub: " + GridId);
                        break;
                };
            }
            
        }
    });


    menu.id = 'ctx-menu';

    var menuArticulos = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Simulador de Precios,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<span class=\"fal fa-copy\"></span>&nbsp;&nbsp;Copiar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Implosión,<hr class="opciones-separador">,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Historificar(OUT)/Act.'.split(','),

        itemClicked: function (s, e) {

            let GridId  = theGridC._orgAtts.id.nodeValue;
           

            // SIMULADOR DE PRECIOS
            if (menuArticulos.selectedIndex == 0) {
                N_DialogSimuladorPrecios(Guid).then((res) => {
                }).catch((error) => {
                });
            }

            // EDITAR ART
            if (menuArticulos.selectedIndex == 1) {
                window.location.href = "/Articulos/Ficha/" + Guid;
            }

            // ELIMINAR ART
            if (menuArticulos.selectedIndex == 2) {
                var selectedGuids = getSelectedArticulosGuids();
                if (selectedGuids.length === 0) {
                    DialogInfo("Por favor, seleccione al menos un artículo.");
                    return;
                }

                var message = selectedGuids.length === 1
                    ? '¿Está seguro de que desea eliminar el artículo seleccionado?'
                    : `¿Está seguro de que desea eliminar los ${selectedGuids.length} artículos seleccionados?`;

                N_ModalConfirmation(message).then(async (res) => {
                    if (res) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        try {
                            const response = await fetch("/Articulos/EliminarMultiplesArticulos", {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json',
                                },
                                body: JSON.stringify(selectedGuids)
                            });

                            const result = await response.json();
                            Spinner.stop();

                            if (result.success) {
                                CargaDatosArticulos();
                            } else {
                                DialogInfo("Error al eliminar los artículos: " + result.message);
                            }
                        } catch (error) {
                            Spinner.stop();
                            console.error("Error:", error);
                            DialogInfo("Error al eliminar los artículos: " + error.message);
                        }
                    }
                }).catch((error) => {
                    DialogInfo("Ocurrió un error: " + error.message);
                });
            }

            // RENOMBRAR ART
            if (menuArticulos.selectedIndex == 3) {

                let articuloId = theGridC.collectionView.currentItem.Articulo;
                N_DialogArticuloRenombrar(articuloId).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridArticulos"), res);
                }).catch((error) => {
                });
            }

            // COPIAR ART
            if (menuArticulos.selectedIndex == 4) {

                let articuloId = theGridC.collectionView.currentItem.Articulo;
                N_DialogArticuloCopiar(articuloId).then((res) => {
                    RepositionArt = res.Guid;
                    CargaDatos();
                }).catch((error) => {
                });

            }

            // IMPLOSION ART
            if (menuArticulos.selectedIndex == 5) {
                N_DialogImplosionArt(Guid).then((res) => {

                }).catch((error) => {

                });
            }

            // HISTORIFICAR OUT
            if (menuArticulos.selectedIndex == 7) {
                let selectedGuids = getSelectedArticulosGuids();
                if (selectedGuids.length === 0) {
                    DialogInfo("Por favor, seleccione al menos un artículo.");
                    return;
                }

                var message = selectedGuids.length === 1
                    ? '¿Desea Historificar(Out)/Act el artículo seleccionado?'
                    : `¿Desea Historificar(Out)/Act los ${selectedGuids.length} artículos seleccionados?`;

                N_ModalConfirmation(message).then(async (res) => {
                    if (res) {
                        try {
                            var Spinner = Rats.UI.LoadAnimation.start();
                            const response = await fetch('/Articulos/HistorificarArticulos', {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json',
                                },
                                body: JSON.stringify(selectedGuids)
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const text = await response.text();
                            let data;
                            try {
                                data = JSON.parse(text);
                            } catch (e) {
                                console.error('Error parsing JSON:', text);
                                throw new Error('La respuesta del servidor no es un JSON válido');
                            }

                            if (data.success) {
                                // DialogInfo(data.message);
                                CargaDatosArticulos();
                            } else {
                                throw new Error(data.message || 'Error desconocido');
                            }
                        } catch (error) {
                            console.error('Error completo:', error);
                            DialogInfo("Ocurrió un error al historificar los artículos: " + error.message);
                        } finally {
                            Spinner.stop();
                        }
                    }
                });
            }


        }
    });
    menuArticulos.id = 'ctx-menu';

    var menuClientes = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<span class=\"fal fa-copy\"></span>&nbsp;&nbsp;Copiar'.split(','),

        itemClicked: function (s, e) {

            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR CLIENTE
            if (menuClientes.selectedIndex == 0) {
                N_DialogCliente(Guid).then((res) => {

                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridClientes"), res);

                }).catch((error) => {

                });
            }

            // ELIMINAR CLIENTE
            if (menuClientes.selectedIndex == 1) {
                N_ModalConfirmation('¿ Desea eliminar el cliente seleccionado?').then(async (res) => {
                    if (res) { // Simplificado: res == true no es necesario
                        var Spinner = Rats.UI.LoadAnimation.start();

                        try {
                            const response = await fetch("/Clientes/Delete_CLIENTE", {
                                method: 'POST',
                                body: new URLSearchParams({ Guid: Guid }), // Usamos URLSearchParams para enviar datos como formulario
                                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                            });

                            if (!response.ok) {
                                throw new Error(`Error HTTP: ${response.status} ${response.statusText}`);
                            }

                            const data = await response.text(); // La respuesta debería ser solo "OK" o un mensaje de error
                            if (data === "OK") {
                                N_Grid_Data_Delete(theGridC, Guid);
                            } else {
                                DialogInfo(data);
                            }
                        } catch (error) {
                            console.error('Error al eliminar el cliente:', error);
                            // Manejo el error aquí, por ejemplo:
                            DialogInfo("Error al eliminar el cliente.");
                        } finally {
                            Spinner.stop();
                        }
                    }
                }).catch((error) => {
                    console.error('Error al eliminar el cliente:', error);
                    DialogInfo("Error al eliminar el cliente. Por favor, inténtelo de nuevo más tarde.");

                });
            }

            // RENOMBRAR CLIENTE
            if (menuClientes.selectedIndex == 2) {
                let clienteId = theGridC.collectionView.currentItem.Cliente; // Obtener el ID del cliente a renombrar
                N_DialogClienteRenombrar(clienteId).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridClientes"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // COPIAR CLIENTE
            if (menuClientes.selectedIndex == 3) {

                let clienteId = theGridC.collectionView.currentItem.Cliente;
                N_DialogClienteCopiar(clienteId).then((res) => {
                    N_Grid_Data_Add(wijmo.Control.getControl("#gridClientes"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });

            }
            

        }
    });
    menuClientes.id = 'ctx-menu';

    var menuProveedores = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<span class=\"fal fa-copy\"></span>&nbsp;&nbsp;Copiar'.split(','),

        itemClicked: function (s, e) {
            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR PROVEEDOR
            if (menuProveedores.selectedIndex == 0) {
                N_DialogProveedor(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridProveedores"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // ELIMINAR PROVEEDOR
            if (menuProveedores.selectedIndex == 1) {
                N_ModalConfirmation('¿Desea eliminar el proveedor seleccionado?').then(async (res) => {
                    if (res) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        try {
                            const response = await fetch("/Proveedores/Delete_PROVEEDOR", {
                                method: 'POST',
                                body: new URLSearchParams({ Guid: Guid }),
                                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                            });

                            if (!response.ok) {
                                throw new Error(`Error HTTP: ${response.status} ${response.statusText}`);
                            }

                            const data = await response.text();
                            if (data === "OK") {
                                N_Grid_Data_Delete(theGridC, Guid);
                            } else {
                                DialogInfo(data);
                            }
                        } catch (error) {
                            console.error('Error al eliminar el proveedor:', error);
                            DialogInfo("Error al eliminar el proveedor.");
                        } finally {
                            Spinner.stop();
                        }
                    }
                }).catch((error) => {
                    console.error('Error al eliminar el proveedor:', error);
                    DialogInfo("Error al eliminar el proveedor. Por favor, inténtelo de nuevo más tarde.");
                });
            }

            // RENOMBRAR PROVEEDOR
            if (menuProveedores.selectedIndex == 2) {
                let proveedorId = theGridC.collectionView.currentItem.Proveedor;
                N_DialogProveedorRenombrar(proveedorId).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridProveedores"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // COPIAR PROVEEDOR
            if (menuProveedores.selectedIndex == 3) {
                let proveedorId = theGridC.collectionView.currentItem.Proveedor;
                N_DialogProveedorCopiar(proveedorId).then((res) => {
                    N_Grid_Data_Add(wijmo.Control.getControl("#gridProveedores"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }
        }
    });
    menuProveedores.id = 'ctx-menu';

    var menuCOMP = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Ficha Artículo'.split(','),

        itemClicked: function (s, e) {

            let GridId  = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;
            //console.log(theGridC.collectionView.currentItem);

            if (menuCOMP.selectedIndex == 0) {
                //window.location.href = "/Articulos/Ficha/" + Guid;
                var base_url = window.location.origin;
                window.open(base_url + "/Articulos/Ficha/" + Guid, '_blank');
                theGridC.select(-1, -1);
            }

            //// ELIMINAR ART
            //if (menuCOMP.selectedIndex == 1) {

            //}
        }
    });
    menuCOMP.id = 'ctx-menu';

    var menuPIE = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Ficha Artículo'.split(','),

        itemClicked: function (s, e) {

            var theGridCArtPIE = wijmo.Control.getControl("#gridArticuloPIE");
            let GridId = theGridCArtPIE._orgAtts.id.nodeValue;
            let Guid = theGridCArtPIE.collectionView.currentItem.Guid;

            if (menuPIE.selectedIndex == 0) {
                //window.location.href = "/Articulos/Ficha/" + Guid;
                var base_url = window.location.origin;
                window.open(base_url + "/Articulos/Ficha/" + Guid, '_blank');
                theGridCArtPIE.select(-1, -1);
            }

        }
    });
    menuPIE.id = 'ctx-menu';

    var menuPlanesMrp = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Eliminar'.split(','),

        itemClicked: function (s, e) {

            N_ModalConfirmation('¿ Desea eliminar el plan MRP seleccionado?').then((res) => {

                if (res == true) {

                    var Spinner = Rats.UI.LoadAnimation.start();

                    var theGridCPlanesMrp    = wijmo.Control.getControl("#gridPlanesFab");
                    let GridId              = theGridCPlanesMrp._orgAtts.id.nodeValue;
                    let Guid                = theGridCPlanesMrp.collectionView.currentItem.Guid;

                    var data = new FormData();
                    data.append("Guid", Guid);

                    $.ajax({
                        type: 'POST',
                        url: "/Mrp/Delete_PlanMRP",
                        data: data,
                        processData: false,
                        contentType: false,
                        success: function (data) {
                            Spinner.stop();
                            if (data != null) {
                                N_Grid_Data_Delete(theGridC, Guid);
                            }
                        },
                        always: function (data) { },
                        error: function (xhr, status, error) {
                            Spinner.stop();
                            console.log("Error", xhr, xhr.responseText, status, error);
                        }
                    });

                }

            }).catch((error) => {

            });

        }
    });
    menuPlanesMrp.id = 'ctx-menu';

    var menuPedidos = new wijmo.input.Menu(document.createElement('div'), {        
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<hr class="opciones-separador">,<span class=\"fal fa-industry-alt\"></span>&nbsp;&nbsp;MRP Generar,<span class=\"fal fa-print\"></span>&nbsp;&nbsp;MRP Imprimir,<hr class="opciones-separador">,<span class=\"fal fa-file-invoice\"></span>&nbsp;&nbsp;Albaran Crear,<span class=\"fal fa-euro-sign\"></span>&nbsp;&nbsp;Factura Crear'.split(','),

        itemClicked: function (s, e) {

            let GridId  = theGridC._orgAtts.id.nodeValue;
            let Guid    = theGridC.collectionView.currentItem.Guid;

            //console.log(menuPedidos.selectedIndex);


            // EDITAR
            if (menuPedidos.selectedIndex == 0) {
                window.location.href = "/Pedidos/Ficha/" + Guid;
            }

            // ELIMINAR
            if (menuPedidos.selectedIndex == 1) {

                N_ModalConfirmation('¿ Desea eliminar el pedido seleccionado?').then((res) => {

                    if (res == true) {

                        var Spinner = Rats.UI.LoadAnimation.start();

                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Pedidos/Delete_PEDIDO",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }

                }).catch((error) => {

                });

            }


            /////////////////////////////////////////////////////////////// RENOMBRAR ///////////////////////////////////////////////////////////////////////////////

            // RENOMBRAR
            if (menuPedidos.selectedIndex == 2) {

                let pedidoId = theGridC.collectionView.currentItem.Pedido;
                let clienteId = theGridC.collectionView.currentItem.Cliente; 

                N_DialogPedidoRen(clienteId, pedidoId).then((res) => {

                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridPedidos"), res);

                }).catch((error) => {

                });
            }

            // MRP GENERAR
            if (menuPedidos.selectedIndex == 4) {
                MRPGenerar('SelPedLin', 'gridPedidos');
            }

            // MRP IMPRIMIR
            if (menuPedidos.selectedIndex == 5) {
                MRPGenerarImprimir('gridPedidos');
            }

            // ALBARAN CREAR
            if (menuPedidos.selectedIndex == 7) {
                PedLinAlbaranCrear('SelPedLin', 'gridPedidos');
            }

            // FACTURA CREAR
            if (menuPedidos.selectedIndex == 8) {
                PedLinFacturaCrear('SelPedLin', 'gridPedidos');
            }

        }
    });
    menuPedidos.id = 'ctx-menu';

    var menuAlbaranes = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<hr class="opciones-separador">,<span class=\"fal fa-print\"></span>&nbsp;&nbsp;Albaran Imprimir,<hr class="opciones-separador">,<span class=\"fal fa-euro-sign\"></span>&nbsp;&nbsp;Facturar'.split(','),

        itemClicked: function (s, e) {

            let GridId  = theGridC._orgAtts.id.nodeValue;
            let Guid    = theGridC.collectionView.currentItem.Guid;

            // EDITAR
            if (menuAlbaranes.selectedIndex == 0) {
                window.location.href = "/Albaranes/Ficha/" + Guid;
            }

            // ELIMINAR
            if (menuAlbaranes.selectedIndex == 1) {

                N_ModalConfirmation('¿ Desea eliminar el pedido seleccionado?').then((res) => {

                    if (res == true) {

                        var Spinner = Rats.UI.LoadAnimation.start();

                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Albaranes/Delete_ALBARAN",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }

                }).catch((error) => {

                });

            }

            // RENOMBRAR
            if (menuAlbaranes.selectedIndex == 2) {

            }

            // ALBARAN IMPRIMIR
            if (menuAlbaranes.selectedIndex == 4) {
                AlbaranesImprimir('gridAlbaranes');
            }


            // FACTURAR
            if (menuAlbaranes.selectedIndex == 6) {
                alert("facturar");
            }

        }
    });
    menuAlbaranes.id = 'ctx-menu';

    var menuControlHorarioAdmin = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar'.split(','),

        itemClicked: function (s, e) {

            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR
            if (menuControlHorarioAdmin.selectedIndex == 0) {
                // window.location.href = "/ControlHorario/DialogControlHorarioAdmin?Guid=" + Guid;
                N_DialogControlHorarioAdmin(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridControlHorarioAdmin"), res);
                }).catch((error) => {

                });
            }

            // ELIMINAR
            if (menuControlHorarioAdmin.selectedIndex == 1) {

                N_ModalConfirmation('¿ Desea eliminar el registro horario seleccionado?').then((res) => {
                    if (res == true) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/ControlHorario/DeleteRegistroHorario",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }
                }).catch((error) => {
                });

                //break;

                //N_ModalConfirmation('¿ Desea eliminar el pedido seleccionado?').then((res) => {

                //    if (res == true) {

                //        var Spinner = Rats.UI.LoadAnimation.start();

                //        var data = new FormData();
                //        data.append("Guid", Guid);

                //        $.ajax({
                //            type: 'POST',
                //            url: "/Albaranes/Delete_ALBARAN",
                //            data: data,
                //            processData: false,
                //            contentType: false,
                //            success: function (data) {
                //                Spinner.stop();
                //                if (data != null) {
                //                    N_Grid_Data_Delete(theGridC, Guid);
                //                }
                //            },
                //            always: function (data) { },
                //            error: function (xhr, status, error) {
                //                Spinner.stop();
                //                console.log("Error", xhr, xhr.responseText, status, error);
                //            }
                //        });
                //    }

                //}).catch((error) => {

                //});

            }

            // ALBARAN IMPRIMIR
            //if (menuAlbaranes.selectedIndex == 4) {
            //    AlbaranesImprimir('gridAlbaranes');
            //}


            // FACTURAR
            //if (menuAlbaranes.selectedIndex == 6) {
            //    alert("facturar");
            //}

        }
    });
    menuControlHorarioAdmin.id = 'ctx-menu';

    var menuFacturas = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        //itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<hr class="opciones-separador">,<span class=\"fal fa-file-invoice\"></span>&nbsp;&nbsp;FacturaE,<span class=\"fal fa-file-pdf\"></span>&nbsp;&nbsp;Pdf,<hr class="opciones-separador">,<span class=\"fal fa-file-pdf\"></span>&nbsp;&nbsp;Enviar'.split(','),
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<hr class="opciones-separador">,<span class=\"fal fa-file-certificate\"></span>&nbsp;&nbsp;Firmar Digitalmente,<span class=\"fal fa-envelope\"></span>&nbsp;&nbsp;Enviar a cliente,<hr class="opciones-separador">,<span class=\"fal fa-file-certificate\"></span>&nbsp;&nbsp;Descargar FacturaE,<span class=\"fal fa-file-pdf\"></span>&nbsp;&nbsp;Descargar PDF'.split(','),

        itemClicked: function (s, e) {

            let GridId      = theGridC._orgAtts.id.nodeValue;
            let Guid        = theGridC.collectionView.currentItem.Guid;
            let FacFirmada  = theGridC.collectionView.currentItem.FacFirmada;

            // EDITAR
            if (menuFacturas.selectedIndex == 0) {
                window.location.href = "/Facturas/Ficha/" + Guid;
            }

            // ELIMINAR
            if (menuFacturas.selectedIndex == 1) {

                N_ModalConfirmation('¿ Desea eliminar la factura seleccionada?').then((res) => {

                    if (res == true) {

                        var Spinner = Rats.UI.LoadAnimation.start();

                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Facturas/Delete_FACTURA",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }

                }).catch((error) => {

                });

            }

            // RENOMBRAR
            if (menuFacturas.selectedIndex == 2) {

            }

            // 3 SEPARATOR


            // FIRMAR : FACTURAE
            if (menuFacturas.selectedIndex == 4) {

                if (FacFirmada != null) {
                    DialogInfo("Factura ya firmada digitalmente");    
                } else {

                    //N_ModalConfirmation('¿Desea firmar la factura (generar FacturaE) ?').then((res) => {
                    N_ModalConfirmationOptions('¿Desea firmar la factura (generar FacturaE) ?', "Firmar", "Firmar y enviar", true, "", true, "").then((res) => {

                        if (res == 1 || res == 2) {

                            var Spinner = Rats.UI.LoadAnimation.start();

                            var data = new FormData();
                            data.append("Guid", Guid);
                            data.append("Tipo", res);

                            fetch('/Facturas/FirmarFacturaE', {
                                method: 'POST',
                                body: data
                            })
                                .then(response => {
                                    Spinner.stop();
                                    if (response.ok) {
                                        return response.json();
                                    } else {
                                        console.error(`Error del servidor: ${response.status} ${response.statusText}`);
                                        return response.text().then(text => { throw new Error(text) });
                                    }
                                })
                                .then(data => {
                                    //console.log(data.status);
                                    //console.log(data.result);

                                    if (data.status == "ok") {
                                        N_Grid_Data_Refresh(wijmo.Control.getControl("#gridFacturas"), data.result);

                                        DescargarFacturaE(data.result.Guid);

                                    } else {
                                        DialogInfo(data.result);
                                    }

                                })
                                .catch((error) => {
                                    Spinner.stop();
                                    console.error('Error fetch:', error);
                                }
                            );


                        }

                    }).catch((error) => {

                    });
                }



            }

            // ENVIAR
            if (menuFacturas.selectedIndex == 5) {
                var Opt2Active     = true;
                var Opt2TxtTooltip  = "";
                if (FacFirmada != null) {
                    Opt2Active = false;
                    Opt2TxtTooltip = "Factura ya firmada digitalmente";
                }

                N_ModalConfirmationOptions('¿Desea enviar la factura?', "Enviar", "Firmar y enviar", true, "", Opt2Active, Opt2TxtTooltip).then((res) => {

                    if (res == 1 || res == 2) {

                        var Spinner = Rats.UI.LoadAnimation.start();

                        var data = new FormData();
                        data.append("Guid", Guid);
                        data.append("Tipo", res);

                        fetch('/Facturas/EnviarFactura', {
                            method: 'POST',
                            body: data
                        })
                            .then(response => {
                                Spinner.stop();
                                if (response.ok) {
                                    return response.json();
                                } else {
                                    console.error(`Error del servidor: ${response.status} ${response.statusText}`);
                                    return response.text().then(text => { throw new Error(text) });
                                }
                            })
                            .then(data => {
                                //console.log(data.status);
                                //console.log(data.result);

                                if (data.status == "ok") {
                                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridFacturas"), data.result);
                                } else {
                                    DialogInfo(data.result);
                                }

                            })
                            .catch((error) => {
                                Spinner.stop();
                                console.error('Error fetch:', error);
                            }
                            );


                    }

                }).catch((error) => {

                });

            }

            // 6 SEPARATOR

            // DESCARGAR FACTURAE
            if (menuFacturas.selectedIndex == 7) {
                alert("DESCARGAR FACTURAE");
            }

            // DESCARGAR PDF
            if (menuFacturas.selectedIndex == 8) {
                alert("DESCARGAR PDF");
            }

        }
    });
    menuFacturas.id = 'ctx-menu';

    var menuRemesas = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',        
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<hr class="opciones-separador">,<span class=\"fal fa-file-code\"></span>&nbsp;&nbsp;Descargar Fichero SEPA'.split(','),

        itemClicked: function (s, e) {

            let GridId  = theGridC._orgAtts.id.nodeValue;
            let Guid    = theGridC.collectionView.currentItem.Guid;
            let Estado  = theGridC.collectionView.currentItem.RemEstado;

            // EDITAR
            if (menuRemesas.selectedIndex == 0) {

                if (Estado == "CON") {
                    DialogInfo('No se puede modificar una Remesa ya contabilizada.');

                } else {
                    alert("editar");
                }

            }

            // ELIMINAR
            if (menuRemesas.selectedIndex == 1) {


            }

            // 2 SEPARATOR            

            // GENERAR FICHERO SEPA
            if (menuRemesas.selectedIndex == 3) {
                alert("GENERAR FICHERO SEPA");

            }



        }
    });
    menuRemesas.id = 'ctx-menu';

    var menuFases = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar'.split(','),

        itemClicked: function (s, e) {

            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR
            if (menuFases.selectedIndex == 0) {
                // window.location.href = "/ControlHorario/DialogControlHorarioAdmin?Guid=" + Guid;
                N_DialogFase(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridFases"), res);
                }).catch((error) => {

                });
            }

            // ELIMINAR
            if (menuFases.selectedIndex == 1) {

                N_ModalConfirmation('¿ Desea eliminar la fase seleccionada?').then((res) => {
                    if (res == true) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Fases/Delete_Fase",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }
                }).catch((error) => {
                });

            }

        }
    });
    menuFases.id = 'ctx-menu';

    var menuDoctores = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar'.split(','),

        itemClicked: function (s, e) {

            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR
            if (menuDoctores.selectedIndex == 0) {
                // window.location.href = "/ControlHorario/DialogControlHorarioAdmin?Guid=" + Guid;
                N_DialogDoctor(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridDoctores"), res);
                }).catch((error) => {

                });
            }

            // ELIMINAR
            if (menuDoctores.selectedIndex == 1) {

                N_ModalConfirmation('¿ Desea eliminar el doctor seleccionado?').then((res) => {
                    if (res == true) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Doctores/Delete_Doctor",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }
                }).catch((error) => {
                });

            }

        }
    });
    menuDoctores.id = 'ctx-menu';

    var menuTarifas = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-copy\"></span>&nbsp;&nbsp;Copiar'.split(','),

        itemClicked: function (s, e) {
            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR
            if (menuTarifas.selectedIndex == 0) {
                N_DialogTarifa(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridTarifas"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // ELIMINAR
            if (menuTarifas.selectedIndex == 1) {
                N_ModalConfirmation('¿Desea eliminar la tarifa seleccionada?').then((res) => {
                    if (res == true) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/TarifasVenta/Delete_TARIFA",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // COPIAR
            if (menuTarifas.selectedIndex == 2) {
                let tarifaId = theGridC.collectionView.currentItem.Tarifa;
                N_DialogTarifaCopiar(tarifaId).then((res) => {
                    N_Grid_Data_Add(wijmo.Control.getControl("#gridTarifas"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }
        }
    });
    menuTarifas.id = 'ctx-menu';

    var menuVendedores = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-copy\"></span>&nbsp;&nbsp;Copiar'.split(','),

        itemClicked: function (s, e) {
            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR VENDEDOR
            if (menuVendedores.selectedIndex == 0) {
                N_DialogVendedor(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridVendedores"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // ELIMINAR VENDEDOR
            if (menuVendedores.selectedIndex == 1) {
                N_ModalConfirmation('¿Desea eliminar el vendedor seleccionado?').then((res) => {
                    if (res == true) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Vendedores/Delete_VENDEDOR",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // COPIAR VENDEDOR
            if (menuVendedores.selectedIndex == 2) {
                let vendedorId = theGridC.collectionView.currentItem.Vendedor;
                N_DialogVendedorCopiar(vendedorId).then((res) => {
                    N_Grid_Data_Add(wijmo.Control.getControl("#gridVendedores"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }
        }
    });
    menuVendedores.id = 'ctx-menu';

    var menuEmpleados = new wijmo.input.Menu(document.createElement('div'), {
        dropDownCssClass: 'context-menu',
        itemsSource: '<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Editar,<span class=\"fal fa-trash-alt\"></span>&nbsp;&nbsp;Eliminar,<span class=\"fal fa-edit\"></span>&nbsp;&nbsp;Renombrar,<span class=\"fal fa-copy\"></span>&nbsp;&nbsp;Copiar'.split(','),
        itemClicked: function (s, e) {
            let GridId = theGridC._orgAtts.id.nodeValue;
            let Guid = theGridC.collectionView.currentItem.Guid;

            // EDITAR EMPLEADO
            if (menuEmpleados.selectedIndex == 0) {
                N_DialogEmpleado(Guid).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridEmpleados"), res);
                }).catch((error) => {

                });
            }

            // ELIMINAR EMPLEADO
            if (menuEmpleados.selectedIndex == 1) {
                N_ModalConfirmation('¿ Desea eliminar el empleado seleccionado?').then((res) => {
                    if (res == true) {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var data = new FormData();
                        data.append("Guid", Guid);

                        $.ajax({
                            type: 'POST',
                            url: "/Empleados/Delete_Empleado",
                            data: data,
                            processData: false,
                            contentType: false,
                            success: function (data) {
                                Spinner.stop();
                                if (data != null) {
                                    N_Grid_Data_Delete(theGridC, Guid);
                                }
                            },
                            always: function (data) { },
                            error: function (xhr, status, error) {
                                Spinner.stop();
                                console.log("Error", xhr, xhr.responseText, status, error);
                            }
                        });
                    }
                }).catch((error) => {
                });
            }

            // RENOMBRAR EMPLEADO
            if (menuEmpleados.selectedIndex == 2) {
                let empleadoId = theGridC.collectionView.currentItem.Empleado;
                N_DialogEmpleadoRenombrar(empleadoId).then((res) => {
                    N_Grid_Data_Refresh(wijmo.Control.getControl("#gridEmpleados"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }

            // COPIAR EMPLEADO
            if (menuEmpleados.selectedIndex == 3) {
                let empleadoId = theGridC.collectionView.currentItem.Empleado;
                N_DialogEmpleadoCopiar(empleadoId).then((res) => {
                    N_Grid_Data_Add(wijmo.Control.getControl("#gridEmpleados"), res);
                }).catch((error) => {
                    // Manejar errores aquí
                });
            }
        }
    });



    $('.GridEvents').each(function () {

        theGridC = wijmo.Control.getControl("#" + this.id);

        if (this.id == "gridArticuloPIE") {
            var theGridArtPIE = wijmo.Control.getControl("#gridArticuloPIE");
            theGridArtPIE.hostElement.addEventListener('contextmenu', function (e) {  // select the cell under the menu
                var ht = theGridArtPIE.hitTest(e);
                if (ht.panel === theGridArtPIE.cells) {
                    theGridArtPIE.select(ht.range);
                }
                console.log(theGridArtPIE);
                let GridId = theGridArtPIE._orgAtts.id.nodeValue;
                menuPIE.show(e); // show the menu

                e.preventDefault();
            });

        } else {
            theGridC.hostElement.addEventListener('contextmenu', function (e) {  // select the cell under the menu
                var ht = theGridC.hitTest(e);
                if (ht.panel === theGridC.cells) {

                    // CONTROL PARA QUE NO DESMARQUE LOS ROWS SELECCIONADOS CUANDO EL MOUSE ESTA ENCIMA DE ELLOS
                    // SI EL MOUSE ESTA ENCIMA DE OTRO REGISTRO = MARCAR ESE NUEVO REGISTRO
                    var selection = theGridC.selection;
                    var row = theGridC.hitTest(e.clientX, e.clientY).row;
                    if (selection.containsRow(row)) {
                        //console.log('El mouse está encima de un/os registro/s seleccionado');
                    } else {
                        //console.log('El mouse NOOOOOOOO está encima de un/os registro/s seleccionado');
                        theGridC.select(ht.range);
                    }


                }

                let GridId = theGridC._orgAtts.id.nodeValue;
                //

                switch (GridId) {

                    //case "gridArticulos":
                    //    menuArticulos.show(e);
                    //    $('#_dropdown').on('mouseleave', function (event) {
                    //        menuArticulos.hide();
                    //    });

                    //    break;

                    case "gridArticulos":
                        var ht = theGridC.hitTest(e);
                        if (ht.panel === theGridC.cells) {
                            // Si el clic no está en una selección existente, selecciona la nueva celda
                            if (!theGridC.selection.contains(ht.row, ht.col)) {
                                if (!e.ctrlKey && !e.shiftKey) {
                                    theGridC.selection.clear();
                                }
                                theGridC.select(ht.row, ht.col, !e.ctrlKey);
                            }
                        }
                        menuArticulos.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuArticulos.hide();
                        });

                        break;

                    case "gridControlHorarioAdmin":
                        menuControlHorarioAdmin.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuControlHorarioAdmin.hide();
                        });

                        break;

                    case "gridArticuloCOMP":
                        menuCOMP.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuCOMP.hide();
                        });

                        break;

                    case "gridArticuloPIE":
                        menuPIE.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuPIE.hide();
                        });

                        break;

                    case "gridPlanesFab":
                        menuPlanesMrp.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuPlanesMrp.hide();
                        });

                        break;

                    case "gridPedidos":
                        menuPedidos.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuPedidos.hide();
                        });

                        break;

                    case "gridAlbaranes":
                        menuAlbaranes.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuAlbaranes.hide();
                        });

                        break;

                    case "gridFacturas":
                        menuFacturas.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuFacturas.hide();
                        });

                        break;

                    case "gridContaRemesas":
                        menuRemesas.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuRemesas.hide();
                        });

                        break;

                    case "gridFases":
                        menuFases.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuFases.hide();
                        });

                        break;

                    case "gridDoctores":
                        menuDoctores.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuDoctores.hide();
                        });

                        break;

                    case "gridClientes":
                        menuClientes.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuClientes.hide();
                        });

                        break;

                    case "gridProveedores":
                        menuProveedores.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuProveedores.hide();
                        });
                        break;

                    case "gridTarifas":
                        menuTarifas.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuTarifas.hide();
                        });
                        break;

                    case "gridVendedores":
                        menuVendedores.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuVendedores.hide();
                        });
                        break;

                    case "gridEmpleados":
                        menuEmpleados.show(e);
                        $('#_dropdown').on('mouseleave', function (event) {
                            menuEmpleados.hide();
                        });
                        break;

                    default:
                        menu.show(e); // show the menu
                        $('#_dropdown').on('mouseleave', function (event) {
                            menu.hide();
                        });
                }



                e.preventDefault();
            });
        }

    });

}
