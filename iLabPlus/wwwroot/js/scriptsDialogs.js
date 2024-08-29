

const CallAsyncFetch = async (url, data) => {
    // alert("Pruebo el Fetch antes del try");

    try {
        // alert("Pruebo el Fetch en el try");

        const options = {
            method: 'POST',
            body: data,
        };

        const response = await fetch(url, options);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const jsonData = await response.json();
        return jsonData;
    } catch (error) {
        console.log("Error", error);
    }
};

function CallAsyncAjax(url, data) {
    return new Promise(function (resolve, reject) {


        setTimeout(function () {
            $.ajax({
                type: 'POST',
                url: url,
                //async: false,
                data: data,
                processData: false,
                contentType: false,
                success: function (data) {
                    resolve(data);
                },
                always: function (data) { },
                error: function (xhr, status, error) {
                    console.log("Error", xhr, xhr.responseText, status, error);
                }
            });
        }, 20);


    });
}


/***************************************************************************
    DIALOGOS GENERALES
***************************************************************************/

let DialogConfirm = function DialogConfirmFunc(Texto) {
    return new Promise((resolve, reject) => {

        $("#modalCenterWarning .modal-body").html(Texto);
        $('#modalCenterWarning').modal('show');
        $('#modal-ok').off('click').on('click', function () {
            resolve(true);
        });
        $('#modal-cancel').off('click').on('click', function () {
            resolve(false);
        });

        //resolve(true);
        //resolve(false);
        //resolve("texto");
        //reject("Rejected");
    });
};

let DialogInfo = function DialogInfoFunc(Texto) {

   // return new Promise((resolve, reject) => {

        $("#modalInfo .modal-body").text(Texto);
        $('#modalInfo').modal('show');
        //$('#modal-ok').off('click').on('click', function () {
        //    resolve(true);
        //});


    //});
};

let N_ModalConfirmation = function N_ModalConfirmationFunc(Texto) {
    return new Promise((resolve, reject) => {
        // Elimina el diálogo anterior del DOM
        $('#modalConfirmation').remove();

        // Carga la vista parcial usando fetch
        fetch(`/Dialogs/ModalConfirmation?Texto=${Texto}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                // Agrega el diálogo al DOM y lo muestra
                $("body").append(data);
                $("#modalConfirmation .modal-body").html(Texto);

                // Reemplaza esta línea:
                $('#modalConfirmation').modal('show');

                // Con el nuevo código de Bootstrap 5 para mostrar el modal
                //var myModal = new bootstrap.Modal(document.getElementById('modalConfirmation'));
                //myModal.show();

                // Manejadores de eventos para los botones OK y Cancelar
                $('#modalConfirmation-ok').off('click').on('click', function (evt) {
                    evt.preventDefault();
                    $('#modalConfirmation').modal('hide');
                    resolve(true);
                });

                $('#modalConfirmation-cancel').off('click').on('click', function () {
                    $('#modalConfirmation').modal('hide');
                    resolve(false);
                });

                // Manejador para cerrar el modal cuando se presiona Enter
                $('#modalConfirmation').on('keydown', function (e) {
                    if (e.keyCode === 13) {
                        e.preventDefault();
                        $('#modalConfirmation').modal('hide');
                        resolve(true);
                    }
                });

                // Limpieza después de ocultar el modal
                $('#modalConfirmation').on('hidden.bs.modal', function () {
                    $('#modalConfirmation').remove();
                });
            })
            .catch(error => {
                console.error('Error en la solicitud fetch:', error);
                reject(error);
            });
    });



};

let N_ModalConfirmationOptions = function N_ModalConfirmationOptionsFunc(Texto, TxtOpt1, TxtOpt2, Opt1Active, Opt1TxtTooltip, Opt2Active, Opt2TxtTooltip) {
    return new Promise((resolve, reject) => {
        // Elimina el diálogo anterior del DOM
        $('#modalConfirmationOptions').remove();

        // Carga la vista parcial usando fetch
        fetch(`/Dialogs/ModalConfirmationOptions?Texto=${Texto}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                // Agrega el diálogo al DOM y lo muestra
                $("body").append(data);
                //$("#modalConfirmationOptions .modal-body").html(Texto);
                $("#modal-bodyLabel").text(Texto);

                $("#modalConfirmationOptions-ok1").text(TxtOpt1);
                $("#modalConfirmationOptions-ok2").text(TxtOpt2);
                if (Opt1Active == false) {
                    if (Opt1TxtTooltip != "") {
                        $("#modalConfirmationOptions-ok1").attr({
                            "data-bs-toggle": "tooltip",
                            "data-bs-placement": "bottom",
                            "title": Opt1TxtTooltip
                        });
                    }

                    $("#modalConfirmationOptions-ok1").prop("disabled", true);
                }
                if (Opt2Active == false) {
                    if (Opt2TxtTooltip != "") {
                        $("#modalConfirmationOptions-ok2").attr({
                            "data-bs-toggle": "tooltip",
                            "data-bs-placement": "bottom",
                            "title": Opt2TxtTooltip
                        });
                    }

                    $("#modalConfirmationOptions-ok2").prop("disabled", true);
                }                

                $('#modalConfirmationOptions').modal('show');

                // Manejadores de eventos para los botones OK y Cancelar
                $('#modalConfirmationOptions-ok1').off('click').on('click', function (evt) {
                    evt.preventDefault();
                    $('#modalConfirmationOptions').modal('hide');
                    resolve(1);
                });
                $('#modalConfirmationOptions-ok2').off('click').on('click', function (evt) {
                    evt.preventDefault();
                    $('#modalConfirmationOptions').modal('hide');
                    resolve(2);
                });


                $('#modalConfirmationOptions-cancel').off('click').on('click', function () {
                    $('#modalConfirmationOptions').modal('hide');
                    resolve(false);
                });

                // Manejador para cerrar el modal cuando se presiona Enter
                $('#modalConfirmationOptions').on('keydown', function (e) {
                    if (e.keyCode === 13) {
                        e.preventDefault();
                        $('#modalConfirmationOptions').modal('hide');
                        resolve(true);
                    }
                });

                // Limpieza después de ocultar el modal
                $('#modalConfirmationOptions').on('hidden.bs.modal', function () {
                    $('#modalConfirmationOptions').remove();
                });
            })
            .catch(error => {
                console.error('Error en la solicitud fetch:', error);
                reject(error);
            });
    });



};


/***************************************************************************
    DIALOGOS PANTALLAS
***************************************************************************/
let N_DialogImplosionArt = function N_DialogImplosionArtFunc(Guid) {
    return new Promise((resolve, reject) => {
        // Elimina el diálogo anterior del DOM
        $('#modalImplosionArt').remove();

        // Carga la vista parcial usando fetch
        fetch(`/Articulos/DialogImplosionArt?GuidArt=${Guid}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                // Agrega el diálogo al DOM y lo muestra
                $("body").append(data);
                $('#modalImplosionArt').modal('show');

                // Manejador de evento para el botón OK
                $('#modalImplosionArt-ok').off('click').on('click', () => {
                    
                });

                // Manejador de evento para el botón Cancelar
                $('#modalImplosionArt-cancel').off('click').on('click', () => {
                  
                });

                // Limpieza después de ocultar el modal
                $('#modalImplosionArt').on('hidden.bs.modal', function () {
                    $('#modalImplosionArt').remove();
                });
            })
            .catch(error => {
                console.error('Error en la solicitud fetch:', error);
                reject(error);
            });
    });
};


//const validarEmails = () => {
//    var regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

//    var remitenteInput = document.getElementById('Remitente');
//    var destinatarioInput = document.getElementById('Destinatario');
//    var ccoInput = document.getElementById('CCO');

//    var remitenteLabel = document.querySelector('label[for="Remitente"]');
//    var destinatarioLabel = document.querySelector('label[for="Destinatario"]');
//    var ccoLabel = document.querySelector('label[for="CCO"]');

//    var esValido = true;


//    if (!regexEmail.test(remitenteInput.value)) {
//        remitenteInput.classList.add('invalido');
//        remitenteLabel.classList.add('label-invalido');
//        esValido = false;
//    } else {
//        remitenteInput.classList.remove('invalido');
//        remitenteLabel.classList.remove('label-invalido');
//    }

//    if (!regexEmail.test(destinatarioInput.value)) {
//        destinatarioInput.classList.add('invalido');
//        destinatarioLabel.classList.add('label-invalido');
//        esValido = false;
//    } else {
//        destinatarioInput.classList.remove('invalido');
//        destinatarioLabel.classList.remove('label-invalido');
//    }

//    if (ccoInput.value && !regexEmail.test(ccoInput.value)) {
//        ccoInput.classList.add('invalido');
//        ccoLabel.classList.add('label-invalido');
//        esValido = false;
//    } else {
//        ccoInput.classList.remove('invalido');
//        ccoLabel.classList.remove('label-invalido');
//    }

//    return esValido;
//}

const validarEmails = () => {
    var regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    var remitenteInput = document.getElementById('Remitente');
    var destinatarioInput = document.getElementById('Destinatario');
    var ccoInput = document.getElementById('CCO');

    // Cambia los selectores para que coincidan con los nuevos span en lugar de las etiquetas label
    var remitenteSpan = document.getElementById('basic-addon1');
    var destinatarioSpan = document.getElementById('basic-addon2');
    var ccoSpan = document.getElementById('basic-addon3');

    var esValido = true;

    if (!regexEmail.test(remitenteInput.value)) {
        remitenteInput.classList.add('invalido');
        remitenteSpan.classList.add('label-invalido'); // Cambiado de Label a Span
        esValido = false;
    } else {
        remitenteInput.classList.remove('invalido');
        remitenteSpan.classList.remove('label-invalido'); // Cambiado de Label a Span
    }

    if (!regexEmail.test(destinatarioInput.value)) {
        destinatarioInput.classList.add('invalido');
        destinatarioSpan.classList.add('label-invalido'); // Cambiado de Label a Span
        esValido = false;
    } else {
        destinatarioInput.classList.remove('invalido');
        destinatarioSpan.classList.remove('label-invalido'); // Cambiado de Label a Span
    }

    if (ccoInput.value && !regexEmail.test(ccoInput.value)) {
        ccoInput.classList.add('invalido');
        ccoSpan.classList.add('label-invalido'); // Cambiado de Label a Span
        esValido = false;
    } else {
        ccoInput.classList.remove('invalido');
        ccoSpan.classList.remove('label-invalido'); // Cambiado de Label a Span
    }

    return esValido;
}



let N_DialogCorreo = function N_DialogCorreoFunc(Guid) {
    return new Promise((resolve, reject) => {
        // Elimina el diálogo anterior del DOM
        $('#modalCorreoSaliente').remove();

        // Carga la vista parcial usando fetch
        fetch(`/CorreosSalientes/DialogCorreo?Guid=${Guid}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                // Agrega el diálogo al DOM y lo muestra
                $("body").append(data);
                $('#modalCorreoSaliente').modal('show');

                $('#modalCorreoSaliente-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    // Validaciones
                    if (!validarEmails()) {
                        return; // Detiene la ejecución si la validación falla
                    }

                    try {

                        var Spinner = Rats.UI.LoadAnimation.start();
                        var form = $("#FormCorreosSalientesCreateEdit");
                        var formData = new FormData(form[0]);

                        // Aca añado el manejo de los archivos adjuntos
                        var archivos = document.getElementById('Adjuntos').files;
                        for (var i = 0; i < archivos.length; i++) {
                            formData.append('adjuntos', archivos[i]);
                        }

                        // Deshabilita el botón OK para evitar envíos múltiples
                        $("#modalCorreoSaliente-ok").prop("disabled", true);

                        const response = await fetch('/CorreosSalientes/CreateMail', {
                            method: 'POST',
                            body: formData
                        });

                        // Verifica el tipo de contenido de la respuesta
                        const contentType = response.headers.get("content-type");
                        if (contentType && contentType.includes("application/json")) {
                            const result = await response.json();
                            $('#modalCorreoSaliente').modal('hide');
                            resolve(result);
                        } 

                        Spinner.stop();
                    } catch (error) {
                        Spinner.stop();
                        console.error('Error en la solicitud:', error);
                        reject(error);
                    }
                });


                // Manejadores para los botones Cancelar y cerrar el modal
                $('#modalCorreoSaliente-cancel').off('click').on('click', () => {
                    $('#modalCorreoSaliente').modal('hide');
                });

                $('#modalCorreoSaliente').on('hidden.bs.modal', () => {
                    $('#modalCorreoSaliente').remove();
                });
            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });
    });
};



///////////////////////////////////////////////////////////////// SCRIPTS CLIEMTES /////////////////////////////////////////////////////////////////////////////


let N_DialogCliente = function N_DialogClienteFunc(Guid) {
    return new Promise((resolve, reject) => {
        // Elimina el diálogo anterior del DOM
        $('#modalCliente').remove();

        // Carga la vista parcial usando fetch
        fetch(`/Clientes/DialogCliente?Guid=${Guid}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                // Agrega el diálogo al DOM y lo muestra
                $("body").append(data);
                $('#modalCliente').modal('show');

                $('#modalCliente-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    // Lógica de validación
                    if (!$("#Cliente").val().trim()) {
                        Procesar = false;
                        $("#ClienteLabel").addClass("LabelRed");
                    } else {
                        $("#ClienteLabel").removeClass("LabelRed");
                    }

                    // Validación para ProNombre
                    if (!$("#CliNombre").val().trim()) {
                        Procesar = false;
                        $("#CliNombreLabel").addClass("LabelRed");
                    } else {
                        $("#CliNombreLabel").removeClass("LabelRed");
                    }

                    // Validación para ProRazon
                    if (!$("#CliRazon").val().trim()) {
                        Procesar = false;
                        $("#CliRazonLabel").addClass("LabelRed");
                    } else {
                        $("#CliRazonLabel").removeClass("LabelRed");
                    }

                    // Validación de Tarifa Venta
                    const comboBoxCliTarifaVenta = wijmo.Control.getControl('#CliTarifaVenta'); // Obtener el ComboBox
                    if (comboBoxCliTarifaVenta.selectedValue === null || comboBoxCliTarifaVenta.selectedValue === "") { // Verificar si se seleccionó un valor
                        Procesar = false;
                        $("#CliTarifaVentaLabel").addClass("LabelRed");
                    } else {
                        $("#CliTarifaVentaLabel").removeClass("LabelRed");
                    }


                    if (!Procesar) {
                        var TabsObj = wijmo.Control.getControl('.TabsCliente');
                        TabsObj.selectedIndex = 0;
                        return;
                    }



                    try {
                        var Spinner = Rats.UI.LoadAnimation.start();
                        var form = $("#FormClientesCreateEdit");
                        var formData = new FormData(form[0]);

                        // Agrega campos adicionales al FormData si es necesario
                        formData.append("Cliente", $("#Cliente").val());
                        formData.append("IsoUser", $("#IsoUser").val());
                        formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                        formData.append("IsoFecMod", $("#IsoFecMod").val());

                        // Deshabilita el botón OK para evitar envíos múltiples
                        $("#modalCliente-ok").prop("disabled", true);

                        const response = await fetch('/Clientes/CreateEdit', {
                            method: 'POST',
                            body: formData
                        });

                        // Verifica el tipo de contenido de la respuesta
                        const contentType = response.headers.get("content-type");
                        if (contentType && contentType.includes("application/json")) {
                            const result = await response.json();

                            if (result === "EXIST") {
                                $("#ClienteLabel").addClass("LabelRed");
                                $("#modalCliente-ok").prop("disabled", false);
                            } else {
                                $('#modalCliente').modal('hide');
                                resolve(result);
                            }
                        } else {
                            // Si no es JSON, trata la respuesta como texto
                            const textResult = await response.text();
                            if (textResult === "EXIST") {
                                $("#ClienteLabel").addClass("LabelRed");
                                $("#modalCliente-ok").prop("disabled", false);
                            }
                        }
                        Spinner.stop();
                    } catch (error) {
                        Spinner.stop();
                        console.error('Error en la solicitud:', error);
                        reject(error);
                    }
                });


                // Manejadores para los botones Cancelar y cerrar el modal
                $('#modalCliente-cancel').off('click').on('click', () => {
                    $('#modalCliente').modal('hide');
                });

                $('#modalCliente').on('hidden.bs.modal', () => {
                    $('#modalCliente').remove();
                });
            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });
    });
};


let N_DialogClienteCopiar = function N_DialogClienteFunc(Cliente) {
    return new Promise((resolve, reject) => {

        // Elimina el diálogo anterior del DOM (si existe)
        $('#modalClienteNew').remove();

        // Carga la vista parcial
        fetch(`/Clientes/_DialogClienteCopiar?Cliente=${Cliente}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);
                $('#modalClienteNew').modal('show');

                $('#modalClienteNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var ClienteNew = $('#ClienteNew').val();

                    // Validaciones (puedes agregar más según tus necesidades)
                    if (ClienteNew === "" || ClienteNew === null) {
                        Procesar = false;
                        $("#ClienteNewLabel").addClass("LabelRed");
                    } else {
                        $("#ClienteNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("ClienteOld", Cliente);
                        data.append("ClienteNew", ClienteNew);

                        try {
                            $("#modalClienteNew-ok").prop("disabled", true);

                            const response = await fetch('/Clientes/Cliente_Copiar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#ClienteNewLabel").addClass("LabelRed");
                                $("#ClienteStatusLabel").css("display", "block");
                                $("#ClienteStatusLabel").text(result.message);
                                $("#modalClienteNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalClienteNew').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            console.error('Error al copiar cliente:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalClienteNew-cancel').off('click').on('click', () => {
                    $('#modalClienteNew').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalClienteNew').on('hidden.bs.modal', () => {
                    $('#modalClienteNew').remove();
                });
            })
            .catch(error => {
                console.error('Error al cargar la vista parcial:', error);
                reject(error);
            });
    });
};



let N_DialogClienteRenombrar = function (Cliente) {
    return new Promise((resolve, reject) => {

        // Eliminar el diálogo anterior del DOM si existe
        $('#modalClienteRenombrar').remove();
        var script = document.getElementById("modalClienteRenombrar_js");
        if (script) $(script).remove(); // Eliminar el script anterior si existe

        // Cargar la vista parcial
        fetch(`/Clientes/_DialogClienteRenombrar?Cliente=${Cliente}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);

                $('#modalClienteNewTitle').text("Renombrar Cliente"); // Asegura que el título se establezca correctamente

                $('#modalClienteRenombrar').modal('show');

                $('#modalClienteNew-ok').off('click').on('click', async (evt) => { // Asegúrate de que el botón 'ok' tenga el ID correcto
                    evt.preventDefault();

                    let Procesar = true;
                    var ClienteNew = $('#ClienteNew').val();

                    // Validaciones
                    if (ClienteNew === "" || ClienteNew === null) {
                        Procesar = false;
                        $("#ClienteNewLabel").addClass("LabelRed");
                    } else {
                        $("#ClienteNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("ClienteOld", Cliente);
                        data.append("ClienteNew", ClienteNew);

                        try {
                            $("#modalClienteNew-ok").prop("disabled", true); // Deshabilitar el botón 'ok'

                            const response = await fetch('/Clientes/Cliente_Renombrar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#ClienteNewLabel").addClass("LabelRed");
                                $("#ClienteStatusLabel").css("display", "block");
                                $("#ClienteStatusLabel").text(result.message);
                                $("#modalClienteNew-ok").prop("disabled", false); // Habilitar el botón si hay error
                                return;
                            }

                            $('#modalClienteRenombrar').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            console.error('Error al renombrar cliente:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalClienteNew-cancel').off('click').on('click', () => { // Asegúrate de que el botón 'cancel' tenga el ID correcto
                    $('#modalClienteRenombrar').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalClienteRenombrar').on('hidden.bs.modal', () => {
                    $('#modalClienteRenombrar').remove();
                    var script = document.getElementById("modalClienteRenombrar_js");
                    if (script) $(script).remove();
                });
            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });
    });
};


//////////////////////////////////////////////////////////////// SCRIPTS VENDEDOR ///////////////////////////////////////////////////////////////////////////////////////

let N_DialogVendedor = function N_DialogVendedorFunc(Guid) {
    return new Promise((resolve, reject) => {

        // Elimina el diálogo anterior del DOM
        $('#modalVendedor').remove();
        // Carga la vista parcial usando fetch
        fetch(`/Vendedores/DialogVendedor?Guid=${Guid}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                // Agrega el diálogo al DOM y lo muestra
                $("body").append(data);
                $('#modalVendedor').modal('show');

                $('#modalVendedor-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    if ($("#Vendedor").val() === "") {
                        Procesar = false;
                        $("#VendedorLabel").addClass("LabelRed");
                    } else {
                        $("#VendedorLabel").removeClass("LabelRed");
                    }
                    if ($("#VenNombre").val() === "") {
                        Procesar = false;
                        $("#VenNombreLabel").addClass("LabelRed");
                    } else {
                        $("#VenNombreLabel").removeClass("LabelRed");
                    }

                    if (!Procesar) {
                        var TabsObj = wijmo.Control.getControl('.TabsVendedor');
                        TabsObj.selectedIndex = 0;
                        return;
                    }

                    try {
                        var Spinner = Rats.UI.LoadAnimation.start();

                        var form = $("#FormVendedorCreateEdit");
                        var formData = new FormData(form[0]);

                        formData.append("Vendedor", $("#Vendedor").val());
                        formData.append("IsoUser", $("#IsoUser").val());
                        formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                        formData.append("IsoFecMod", $("#IsoFecMod").val());

                        // Deshabilita el botón OK para evitar envíos múltiples
                        $("#modalVendedor-ok").prop("disabled", true);

                        const response = await fetch('/Vendedores/CreateEdit', {
                            method: 'POST',
                            body: formData
                        });

                        const contentType = response.headers.get("content-type");
                        if (contentType && contentType.includes("application/json")) {
                            const result = await response.json();

                            if (result === "EXIST") {
                                $("#VendedorLabel").addClass("LabelRed");
                                $("#modalVendedor-ok").prop("disabled", false);
                            } else {
                                $('#modalVendedor').modal('hide');
                                resolve(result);
                            }
                        } else {
                            // Si no es JSON, trata la respuesta como texto
                            const textResult = await response.text();
                            if (textResult === "EXIST") {
                                $("#VendedorLabel").addClass("LabelRed");
                                $("#modalVendedor-ok").prop("disabled", false);
                            }
                        }
                        Spinner.stop();
                    } catch (error) {
                        Spinner.stop();
                        console.error('Error en la solicitud:', error);
                        reject(error);
                    }
                });

                // Manejadores para los botones Cancelar y cerrar el modal
                $('#modalVendedor-cancel').off('click').on('click', () => {
                    $('#modalVendedor').modal('hide');
                });

                $('#modalVendedor').on('hidden.bs.modal', () => {
                    $('#modalVendedor').remove();
                });
            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });
    });
};


let N_DialogVendedorCopiar = function N_DialogVendedorCopiarFunc(Vendedor) {
    return new Promise((resolve, reject) => {
        try {
            // Elimina el diálogo anterior del DOM (si existe)
            $('#modalVendedorNew').remove();

            // Carga la vista parcial
            fetch(`/Vendedores/_DialogVendedorCopiar?Vendedor=${Vendedor}`)
                .then(response => response.text())
                .then(data => {
                    $("body").append(data);
                    $('#modalVendedorNew').modal('show');

                    $('#modalVendedorNew-ok').off('click').on('click', async (evt) => {
                        evt.preventDefault();

                        let Procesar = true;
                        var VendedorNew = $('#VendedorNew').val();

                        // Validaciones
                        if (VendedorNew === "" || VendedorNew === null) {
                            Procesar = false;
                            $("#VendedorNewLabel").addClass("LabelRed");
                        } else {
                            $("#VendedorNewLabel").removeClass("LabelRed");
                        }

                        if (Procesar) {
                            try {
                                var data = new FormData();
                                data.append("VendedorOld", Vendedor);
                                data.append("VendedorNew", VendedorNew);

                                $("#modalVendedorNew-ok").prop("disabled", true);

                                const response = await fetch('/Vendedores/Vendedor_Copiar', {
                                    method: 'POST',
                                    body: data
                                });

                                if (!response.ok) {
                                    throw new Error(`HTTP error! status: ${response.status}`);
                                }

                                const result = await response.json();

                                if (!result.success) {
                                    $("#VendedorNewLabel").addClass("LabelRed");
                                    $("#VendedorStatusLabel").css("display", "block");
                                    $("#VendedorStatusLabel").text(result.message);
                                    $("#modalVendedorNew-ok").prop("disabled", false);
                                    return;
                                }

                                $('#modalVendedorNew').modal('hide');
                                resolve(result.data);

                            } catch (error) {
                                console.error('Error al copiar vendedor:', error);
                                reject(error);
                            }
                        }
                    });

                    // Manejador de evento 'cancel'
                    $('#modalVendedorNew-cancel').off('click').on('click', () => {
                        $('#modalVendedorNew').modal('hide');
                    });

                    // Manejador de evento 'hidden'
                    $('#modalVendedorNew').on('hidden.bs.modal', () => {
                        $('#modalVendedorNew').remove();
                    });
                })
                .catch(error => {
                    console.error('Error al cargar la vista parcial:', error);
                    reject(error);
                });
        } catch (error) {
            console.error('Error en N_DialogVendedorCopiar:', error);
            reject(error);
        }
    });
};


let N_DialogOperario = async function N_DialogOperarioFunc(Guid) {
    try {
        // Elimina del DOM si ya existiera
        $('#modalOperario').remove();


        // Carga la vista parcial con fetch
        let response = await fetch("/OperariosEXT/DialogOperario?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abre el diálogo
        $('#modalOperario').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalOperario-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let Procesar = true;

                // Lógica de validación
                var ComboOpeTipo = wijmo.Control.getControl('#OpeTipo');
                Procesar = Procesar && (ComboOpeTipo.selectedValue !== "" && ComboOpeTipo.selectedValue != null);
                $("#OpeTipoLabel").toggleClass("LabelRed", !Procesar);

                Procesar = Procesar && $("#Operario").val() !== "";
                $("#OperarioLabel").toggleClass("LabelRed", !Procesar);

                Procesar = Procesar && $("#OpeNombre").val() !== "";
                $("#OpeNombreLabel").toggleClass("LabelRed", !Procesar);

                if (!Procesar) {
                    var TabsObj = wijmo.Control.getControl('.TabsOperario');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                // Procesamiento del formulario
                var Spinner = Rats.UI.LoadAnimation.start();
                var form = $("#FormOperarioCreateEdit");
                var formData = new FormData(form[0]);

                formData.append("Operario", $("#Operario").val());
                formData.append("IsoUser", $("#IsoUser").val());
                formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                formData.append("IsoFecMod", $("#IsoFecMod").val());
                // Agrega aquí todos los campos adicionales que necesites

                try {
                    let saveResponse = await fetch("/OperariosEXT/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');

                    let res = await saveResponse.text();
                    Spinner.stop();

                    if (res === "EXIST") {
                        $("#OperarioLabel").addClass("LabelRed");
                        $("#modalOperario-ok").prop("disabled", false);
                    } else {
                        $('#modalOperario').modal('hide');
                        let operarioData = JSON.parse(res);
                        resolve(operarioData); // Resuelve la promesa con el resultado
                    }
                } catch (error) {
                    Spinner.stop();
                    console.error('Error en el proceso de guardado:', error);
                    reject(error); // Rechaza la promesa en caso de error
                }
            });

            $('#modalOperario-cancel').off('click').on('click', function () {
                $('#modalOperario').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalOperario').on('hidden.bs.modal', function () {
                $('#modalOperario').remove();
            });
        });

    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error; // Lanza el error para manejarlo en el catch donde se llama a N_DialogOperario
    }
};


/////////////////////////////////////////////////////////////////// SCRIPTS EMPLEADOS //////////////////////////////////////////////////////////////////////////////////////

let N_DialogEmpleado = async function N_DialogEmpleadoFunc(Guid) {
    try {
        // Elimina del DOM si ya existiera
        $('#modalEmpleado').remove();

        // Carga la vista parcial con fetch
        let response = await fetch("/Empleados/DialogEmpleado?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abre el diálogo
        $('#modalEmpleado').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalEmpleado-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let formData = new FormData(document.getElementById('FormEmpleadoCreateEdit')); // Preparar formData

                let Procesar = true;

                if (croppieInstance) {
                    try {
                        let blob = await croppieInstance.result({ type: 'blob', format: 'jpeg', quality: 1 });
                        // Agrega el blob de la imagen recortada a formData
                        formData.append('EmpImagenRecortada', blob, 'imagenRecortada.jpg');
                    } catch (error) {
                        Procesar = false; // Salir si hay error al procesar la imagen
                    }
                }


                // Lógica de validación
                var ComboOpeTipo = wijmo.Control.getControl('#EmpTipo');
                Procesar = Procesar && (ComboOpeTipo.selectedValue !== "" && ComboOpeTipo.selectedValue != null);
                $("#EmpTipoLabel").toggleClass("LabelRed", !Procesar);

                Procesar = Procesar && $("#Empleado").val() !== "";
                $("#EmpleadoLabel").toggleClass("LabelRed", !Procesar);

                Procesar = Procesar && $("#EmpNombre").val() !== "";
                $("#EmpNombreLabel").toggleClass("LabelRed", !Procesar);

                if (!Procesar) {
                    var TabsObj = wijmo.Control.getControl('.TabsEmpleado');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                // Procesamiento del formulario
                var form = $("#FormEmpleadoCreateEdit");
                var password = $("#EmpPassword").val();


                if (Procesar) {
                    formData.append("Empleado", $("#Empleado").val());
                    formData.append("IsoUser", $("#IsoUser").val());
                    formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                    formData.append("IsoFecMod", $("#IsoFecMod").val());
                    formData.append("EmpPassword", password);
                    console.log("Dentro del if(Procesar")

                    var Spinner = Rats.UI.LoadAnimation.start();

                try {
                    let saveResponse = await fetch("/Empleados/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });


                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');
                    let res = await saveResponse.text();
                    Spinner.stop();

                    if (res === "EXIST") {
                        $("#EmpleadoLabel").addClass("LabelRed");
                        $("#modalEmpleado-ok").prop("disabled", false);
                    } else {
                        $('#modalEmpleado').modal('hide');
                        let empleadoData = JSON.parse(res);
                        resolve(empleadoData); // Resuelve la promesa con el resultado
                    }
                } catch (error) {
                    Spinner.stop();
                    reject(error); // Rechaza la promesa en caso de error
                    }
                };
            });

            $('#modalEmpleado-cancel').off('click').on('click', function () {
                $('#modalEmpleado').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalEmpleado').on('hidden.bs.modal', function () {
                $('#modalEmpleado').remove();
            });
        });

    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error; // Lanza el error para manejarlo en el catch donde se llama a N_DialogOperario
    }
};


let N_DialogEmpleadoCopiar = function (Empleado) {
    return new Promise((resolve, reject) => {
        $('#modalEmpleadoNew').remove();

        fetch(`/Empleados/_DialogEmpleadoCopiar?Empleado=${Empleado}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);
                $('#modalEmpleadoNew').modal('show');

                $('#modalEmpleadoNew').on('keypress', function (e) {
                    if (e.which === 13) {
                        e.preventDefault();
                        $('#modalEmpleadoNew-ok').click();
                    }
                });

                $('#modalEmpleadoNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var EmpleadoNew = $('#EmpleadoNew').val();

                    if (EmpleadoNew === "" || EmpleadoNew === null) {
                        Procesar = false;
                        $("#EmpleadoNewLabel").addClass("LabelRed");
                    } else {
                        $("#EmpleadoNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("EmpleadoOld", Empleado);
                        data.append("EmpleadoNew", EmpleadoNew);

                        try {
                            $("#modalEmpleadoNew-ok").prop("disabled", true);

                            const response = await fetch('/Empleados/Empleado_Copiar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#EmpleadoNewLabel").addClass("LabelRed");
                                $("#EmpleadoStatusLabel").css("display", "block");
                                $("#EmpleadoStatusLabel").text(result.message);
                                $("#modalEmpleadoNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalEmpleadoNew').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            console.error('Error al copiar empleado:', error);
                            reject(error);
                        }
                    }
                });

                $('#modalEmpleadoNew-cancel').off('click').on('click', () => {
                    $('#modalEmpleadoNew').modal('hide');
                });

                $('#modalEmpleadoNew').on('hidden.bs.modal', () => {
                    $('#modalEmpleadoNew').remove();
                });
            })
            .catch(error => {
                console.error('Error al cargar la vista parcial:', error);
                reject(error);
            });
    });
};

let N_DialogEmpleadoRenombrar = function (Empleado) {
    return new Promise((resolve, reject) => {
        $('#modalEmpleadoRenombrar').remove();

        fetch(`/Empleados/_DialogEmpleadoRenombrar?Empleado=${Empleado}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);
                $('#modalEmpleadoRenombrar').modal('show');

                $('#modalEmpleadoRenombrar').on('keypress', function (e) {
                    if (e.which === 13) {
                        e.preventDefault();
                        $('#modalEmpleadoNew-ok').click();
                    }
                });

                $('#modalEmpleadoNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var EmpleadoNew = $('#EmpleadoNew').val();

                    if (EmpleadoNew === "" || EmpleadoNew === null) {
                        Procesar = false;
                        $("#EmpleadoNewLabel").addClass("LabelRed");
                    } else {
                        $("#EmpleadoNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("EmpleadoOld", Empleado);
                        data.append("EmpleadoNew", EmpleadoNew);

                        try {
                            $("#modalEmpleadoNew-ok").prop("disabled", true);

                            const response = await fetch('/Empleados/Empleado_Renombrar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#EmpleadoNewLabel").addClass("LabelRed");
                                $("#EmpleadoStatusLabel").css("display", "block");
                                $("#EmpleadoStatusLabel").text(result.message);
                                $("#modalEmpleadoNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalEmpleadoRenombrar').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            console.error('Error al renombrar empleado:', error);
                            reject(error);
                        }
                    }
                });

                $('#modalEmpleadoNew-cancel').off('click').on('click', () => {
                    $('#modalEmpleadoRenombrar').modal('hide');
                });

                $('#modalEmpleadoRenombrar').on('hidden.bs.modal', () => {
                    $('#modalEmpleadoRenombrar').remove();
                });
            })
            .catch(error => {
                console.error('Error al cargar la vista parcial:', error);
                reject(error);
            });
    });
};


let N_DialogFase = async function N_DialogFaseFunc(Guid) {
    try {
        $('#modalFase').remove();

        let response = await fetch("/Fases/DialogFase?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        $('#modalFase').modal('show');

        $('body').on('shown.bs.modal', '#modalFase', function () {
            var inputColor = $('#Color');
            if (inputColor.val()) {
                inputColor.css('background-color', inputColor.val());
            }
        });

        return new Promise((resolve, reject) => {
            $('#modalFase-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let colorInput = $('#Color');
                let isValidHex = /^#[A-Fa-f0-9]{6}$/.test(colorInput.val());
                if (!isValidHex) {
                    alert("No es un color en hexadecimal.");
                    colorInput.css('border', '2px solid red');
                    return false; // Paro el proceso aquí si el color no es válido
                }

                let formData = new FormData(document.getElementById('FormFaseCreateEdit'));
                let faseValue = $('#Fase').val();
                let intExtValue = $('#IntExt').val();

                let Procesar = faseValue !== "" && faseValue != null && intExtValue !== "";

                if (!Procesar) {
                    $("#FaseLabel, #IntExtLabel").addClass("LabelRed");
                    var TabsObj = wijmo.Control.getControl('.TabsFase');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                formData.append("Fase", faseValue);
                formData.append("IntExt", intExtValue);
                formData.append("IsoUser", $("#IsoUser").val());
                formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                formData.append("IsoFecMod", $("#IsoFecMod").val());

                try {
                    let saveResponse = await fetch("/Fases/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');
                    let res = await saveResponse.json();

                    if (res.success) {
                        $('#modalFase').modal('hide');
                        let dataToAdd = {
                            Guid: res.Guid,
                            Empresa: formData.get("Empresa"),
                            Fase: formData.get("Fase"),
                            IntExt: formData.get("IntExt") === "true",
                            Color: formData.get("Color"),
                            Descripcion: formData.get("Descripcion"),
                            IsoUser: formData.get("IsoUser"),
                            IsoFecAlt: formData.get("IsoFecAlt"),
                            IsoFecMod: formData.get("IsoFecMod")
                        };
                        N_Grid_Data_Add(wijmo.Control.getControl("#gridFases"), dataToAdd);
                        resolve(res);
                    } else {
                        $("#FaseLabel, #IntExtLabel").addClass("LabelRed");
                        $("#modalFase-ok").prop("disabled", false);
                    }
                } catch (error) {
                    reject(error);
                }
            });

            $('#modalFase-cancel').off('click').on('click', function () {
                $('#modalFase').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalFase').on('hidden.bs.modal', function () {
                $('#modalFase').remove();
            });
        });

    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error;
    }
};



let N_DialogDoctor = function (guid = null) {
    return new Promise(async (resolve, reject) => {
        try {
            $('#modalDoctor').remove();
            let url = "/Doctores/DialogDoctor" + (guid ? "?guid=" + guid : "");
            let response = await fetch(url);
            if (!response.ok) throw new Error('Error al cargar la vista parcial');
            let data = await response.text();
            $("body").append(data);
            $('#modalDoctor').modal('show');

            $('#modalDoctor-ok').off('click').on('click', async function (evt) {

                evt.preventDefault();
                var form = $('#FormDoctorCreateEdit')[0];

                if (!form.checkValidity()) {
                    form.reportValidity();
                    return;
                }


                var formData = new FormData(form);
                formData.append("IsoUser", $("#IsoUser").val());
                formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                formData.append("IsoFecMod", $("#IsoFecMod").val());

                formData.append("Doctor", $("#Doctor").val());
                formData.append("DirDireccion", $("#DirDireccion").val());
                formData.append("DirDP", $("#DirDP").val());
                formData.append("DirPoblacion", $("#DirPoblacion").val());
                formData.append("DirProvincia", $("#DirProvincia").val());
                formData.append("Telefono1", $("#Telefono1").val());
                formData.append("Observ", $("#Observ").val());
                formData.append("Activo", $("#Activo").val());

                try {
                    let saveResponse = await fetch("/Doctores/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    let res = await saveResponse.json();
                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');

                    console.log('respuesta completa del servidor: ', res);

                    if (res.success) {
                        $('#modalDoctor').modal('hide');
                        let dataToAdd = {
                            Guid: res.Guid,
                            Doctor: res.Doctor,
                            Nombre: res.Nombre,
                            NumColegiado: res.NumColegiado,
                            Activo: res.Activo,
                            NIF: res.NIF,
                            Mail: res.Mail,
                            DirDireccion: res.DirDireccion,
                            DirDP: res.DirDP,
                            DirPoblacion: res.DirPoblacion,
                            DirProvincia: res.DirProvincia,
                            Telefono1: res.Telefono1,
                            Observ: res.Observ,
                            IsoUser: res.IsoUser,
                            IsoFecAlt: res.IsoFecAlt,
                            IsoFecMod: res.IsoFecMod
                        };
                        N_Grid_Data_Add(wijmo.Control.getControl("#gridDoctores"), dataToAdd);
                        resolve(res); // Resuelve la promesa con el resultado
                    } else {
                        throw new Error('Error en la respuesta del servidor: ' + res.message);
                    }
                } catch (error) {
                    reject(error); // Rechaza la promesa con el error
                }
            });

            $('#modalDoctor').on('hidden.bs.modal', function () {
                $('#modalDoctor').remove();
                reject('Modal cerrado por el usuario');
            });

        } catch (error) {
            console.error('Error al abrir el diálogo de doctor:', error);
            reject(error);
        }
    });
}





let N_DialogControlHorarioAdmin = async function N_DialogControlHorarioAdminFunc(Guid) {
    try {
        // Elimina del DOM si ya existiera
        $('#modalControlHorarioAdmin').remove();

        // Carga la vista parcial con fetch
        let response = await fetch(`/ControlHorario/DialogControlHorarioAdmin?Guid=${Guid}`);
        if (!response.ok) {
            throw new Error('Error al cargar la vista parcial');
        }
        let data = await response.text();
        $("body").append(data);

        // Formatear la fecha inmediatamente después de añadir el modal al DOM
        var fechaRaw = $('#Fecha').val();
        var fechaFormateada = formatearFecha(fechaRaw);
        $('#Fecha').val(fechaFormateada);

        // Abre el diálogo
        $('#modalControlHorarioAdmin').modal('show');


        return new Promise((resolve, reject) => {
            $('#modalControlHorarioAdmin-ok').off('click').on('click', async (evt) => {
                evt.preventDefault();

                if (!validarHorarios()) {
                    return; // Detiene la ejecución si la validación falla
                }
                //console.log(fechaFormateada);
                // Validaciones para cada campo del formulario
                let Procesar = true;
                Procesar = Procesar && $("#Empleado").val() !== "";
                $("#EmpleadoLabel").toggleClass("LabelRed", !Procesar);

                Procesar = Procesar && $("#Empresa").val() !== "";
                $("#EmpresaLabel").toggleClass("LabelRed", !Procesar);


                if (!Procesar) {
                    let TabsObj = wijmo.Control.getControl('.TabsControlHorarioAdmin');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                // Crea un FormData a partir del formulario
                var formData = new FormData(document.getElementById("FormControlHorarioAdminRegistroControlHorario"));

                try {
                    const saveResponse = await fetch('/ControlHorario/UpdateRegistroHorario', {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) {
                        throw new Error(`HTTP error! status: ${saveResponse.status}`);
                    }

                    const result = await saveResponse.json();
                    //console.log("Resultado recibido:", result);
                    if (result.success) {
                        
                        $('#modalControlHorarioAdmin').modal('hide');

                        resolve(result.data); // Resuelve la promesa con los datos actualizados
                    } else {
                        alert(result.message); // Manejo de error
                    }
                } catch (error) {
                    console.error('Error en la solicitud:', error);
                    reject(error); // Rechaza la promesa en caso de error
                }
            });

            $('#modalControlHorarioAdmin-cancel').off('click').on('click', () => {
                $('#modalControlHorarioAdmin').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalControlHorarioAdmin').on('hidden.bs.modal', () => {
                $('#modalControlHorarioAdmin').remove();
            });
        });
    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error;
    }
};


//////////////////////////////////////////////////////////////////// SCRIPTS TarifasVenta ////////////////////////////////////////////////////////////////////////////////////

let N_DialogTarifa = async function N_DialogTarifaFunc(Guid) {
    try {
        // Elimina del DOM si ya existía
        $('#modalTarifa').remove();

        // Carga la vista parcial con fetch
        let response = await fetch("/TarifasVenta/DialogTarifa?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abre el diálogo
        $('#modalTarifa').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalTarifa-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let Procesar = true;
                // Lógica de validación
                if ($("#Tarifa").val() === "") {
                    Procesar = false;
                    $("#TarifaLabel").addClass("LabelRed");
                } else {
                    $("#TarifaLabel").removeClass("LabelRed");
                }
                if ($("#TarDescripcion").val() === "") {
                    Procesar = false;
                    $("#TarDescripcionLabel").addClass("LabelRed");
                } else {
                    $("#TarDescripcionLabel").removeClass("LabelRed");
                }

                if (!Procesar) {
                    var TabsObj = wijmo.Control.getControl('.TabsTarifa');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                // Procesamiento del formulario
                var Spinner = Rats.UI.LoadAnimation.start();
                var form = $("#FormTarifaCreateEdit");
                var formData = new FormData(form[0]);

                formData.append("Tarifa", $("#Tarifa").val());
                formData.append("IsoUser", $("#IsoUser").val());
                formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                formData.append("IsoFecMod", $("#IsoFecMod").val());

                try {
                    let saveResponse = await fetch("/TarifasVenta/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');

                    let result = await saveResponse.text();
                    Spinner.stop();

                    if (result === "EXIST") {
                        $("#TarifaLabel").addClass("LabelRed");
                        $("#modalTarifa-ok").prop("disabled", false);
                    } else {
                        $('#modalTarifa').modal('hide');
                        let tarifasData = JSON.parse(result);
                        resolve(tarifasData); // Resuelve la promesa con el resultado
                    }
                } catch (error) {
                    Spinner.stop();
                    console.error('Error en el proceso de guardado:', error);
                    reject(error); // Rechaza la promesa en caso de error
                }
            });

            $('#modalTarifa-cancel').off('click').on('click', function () {
                $('#modalTarifa').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalTarifa').on('hidden.bs.modal', function () {
                $('#modalTarifa').remove();
            });
        });

    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error; // Lanza el error para manejarlo en el catch donde se llama a N_DialogTarifa
    }
};


let N_DialogTarifaCopiar = function N_DialogTarifaCopiarFunc(Tarifa) {
    return new Promise((resolve, reject) => {
        try {
            // Elimina el diálogo anterior del DOM (si existe)
            $('#modalTarifaNew').remove();

            // Carga la vista parcial
            fetch(`/TarifasVenta/_DialogTarifaCopiar?Tarifa=${Tarifa}`)
                .then(response => response.text())
                .then(data => {
                    $("body").append(data);
                    $('#modalTarifaNew').modal('show');

                    $('#modalTarifaNew-ok').off('click').on('click', async (evt) => {
                        evt.preventDefault();

                        let Procesar = true;
                        var TarifaNew = $('#TarifaNew').val();

                        // Validaciones
                        if (TarifaNew === "" || TarifaNew === null) {
                            Procesar = false;
                            $("#TarifaNewLabel").addClass("LabelRed");
                        } else {
                            $("#TarifaNewLabel").removeClass("LabelRed");
                        }

                        if (Procesar) {
                            try {
                                var data = new FormData();
                                data.append("TarifaOld", Tarifa);
                                data.append("TarifaNew", TarifaNew);

                                $("#modalTarifaNew-ok").prop("disabled", true);

                                const response = await fetch('/TarifasVenta/Tarifa_Copiar', {
                                    method: 'POST',
                                    body: data
                                });

                                if (!response.ok) {
                                    throw new Error(`HTTP error! status: ${response.status}`);
                                }

                                const result = await response.json();

                                if (!result.success) {
                                    $("#TarifaNewLabel").addClass("LabelRed");
                                    $("#TarifaStatusLabel").css("display", "block");
                                    $("#TarifaStatusLabel").text(result.message);
                                    $("#modalTarifaNew-ok").prop("disabled", false);
                                    return;
                                }

                                $('#modalTarifaNew').modal('hide');
                                resolve(result.data);

                            } catch (error) {
                                console.error('Error al copiar tarifa:', error);
                                reject(error);
                            }
                        }
                    });

                    // Manejador de evento 'cancel'
                    $('#modalTarifaNew-cancel').off('click').on('click', () => {
                        $('#modalTarifaNew').modal('hide');
                    });

                    // Manejador de evento 'hidden'
                    $('#modalTarifaNew').on('hidden.bs.modal', () => {
                        $('#modalTarifaNew').remove();
                    });
                })
                .catch(error => {
                    console.error('Error al cargar la vista parcial:', error);
                    reject(error);
                });
        } catch (error) {
            console.error('Error en N_DialogTarifaCopiar:', error);
            reject(error);
        }
    });
};



//////////////////////////////////////////////////////////////////// SCRIPTS PROVEEDORES ////////////////////////////////////////////////////////////////////////////////////////////

let N_DialogProveedor = async function N_DialogProveedorFunc(Guid) {
    try {
        // Elimina del DOM si ya existía
        $('#modalProveedor').remove();

        // Carga la vista parcial con fetch
        let response = await fetch("/Proveedores/DialogProveedor?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abre el diálogo
        $('#modalProveedor').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalProveedor-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let Procesar = true;
                // Lógica de validación
                if (!$("#Proveedor").val().trim()) {
                    Procesar = false;
                    $("#ProveedorLabel").addClass("LabelRed");
                } else {
                    $("#ProveedorLabel").removeClass("LabelRed");
                }

                // Validación para ProNombre
                if (!$("#ProNombre").val().trim()) {
                    Procesar = false;
                    $("#ProNombreLabel").addClass("LabelRed");
                } else {
                    $("#ProNombreLabel").removeClass("LabelRed");
                }

                // Validación para ProRazon
                if (!$("#ProRazon").val().trim()) {
                    Procesar = false;
                    $("#ProRazonLabel").addClass("LabelRed");
                } else {
                    $("#ProRazonLabel").removeClass("LabelRed");
                }


                if (!Procesar) {
                    var TabsObj = wijmo.Control.getControl('.TabsProveedor');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                // Procesamiento del formulario
                var Spinner = Rats.UI.LoadAnimation.start();
                var form = $("#FormProveedorCreateEdit");
                var formData = new FormData(form[0]);

                // Agrega campos al formData
                formData.append("Proveedor", $("#Proveedor").val());
                formData.append("IsoUser", $("#IsoUser").val());
                formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                formData.append("IsoFecMod", $("#IsoFecMod").val());

                try {
                    let saveResponse = await fetch("/Proveedores/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');

                    let result = await saveResponse.text();
                    Spinner.stop();

                    if (result === "EXIST") {
                        $("#ProveedorLabel").addClass("LabelRed");
                        $("#modalProveedor-ok").prop("disabled", false);
                    } else {
                        $('#modalProveedor').modal('hide');
                        let dialogProveedor = JSON.parse(result);
                        resolve(dialogProveedor); // Resuelve la promesa con el resultado
                    }
                } catch (error) {
                    Spinner.stop();
                    console.error('Error en el proceso de guardado:', error);
                    reject(error); // Rechaza la promesa en caso de error
                }
            });

            $('#modalProveedor-cancel').off('click').on('click', function () {
                $('#modalProveedor').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalProveedor').on('hidden.bs.modal', function () {
                $('#modalProveedor').remove();
            });
        });

    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error; // Lanza el error para manejarlo en el catch donde se llama a N_DialogProveedor
    }
};


let N_DialogProveedorCopiar = function N_DialogProveedorCopiarFunc(Proveedor) {
    return new Promise((resolve, reject) => {
        // Elimina el diálogo anterior del DOM (si existe)
        $('#modalProveedorNew').remove();

        // Carga la vista parcial
        fetch(`/Proveedores/_DialogProveedorCopiar?Proveedor=${Proveedor}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);
                $('#modalProveedorNew').modal('show');

                $('#modalProveedorNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var ProveedorNew = $('#ProveedorNew').val();

                    // Validaciones
                    if (ProveedorNew === "" || ProveedorNew === null) {
                        Procesar = false;
                        $("#ProveedorNewLabel").addClass("LabelRed");
                    } else {
                        $("#ProveedorNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("ProveedorOld", Proveedor);
                        data.append("ProveedorNew", ProveedorNew);

                        try {
                            $("#modalProveedorNew-ok").prop("disabled", true);

                            const response = await fetch('/Proveedores/Proveedor_Copiar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#ProveedorNewLabel").addClass("LabelRed");
                                $("#ProveedorStatusLabel").css("display", "block");
                                $("#ProveedorStatusLabel").text(result.message);
                                $("#modalProveedorNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalProveedorNew').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            console.error('Error al copiar proveedor:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalProveedorNew-cancel').off('click').on('click', () => {
                    $('#modalProveedorNew').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalProveedorNew').on('hidden.bs.modal', () => {
                    $('#modalProveedorNew').remove();
                });
            })
            .catch(error => {
                console.error('Error al cargar la vista parcial:', error);
                reject(error);
            });
    });
};

let N_DialogProveedorRenombrar = function (Proveedor) {
    return new Promise((resolve, reject) => {
        // Eliminar el diálogo anterior del DOM si existe
        $('#modalProveedorRenombrar').remove();
        var script = document.getElementById("modalProveedorRenombrar_js");
        if (script) $(script).remove(); // Eliminar el script anterior si existe

        // Cargar la vista parcial
        fetch(`/Proveedores/_DialogProveedorRenombrar?Proveedor=${Proveedor}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);

                $('#modalProveedorNewTitle').text("Renombrar Proveedor");

                $('#modalProveedorRenombrar').modal('show');

                $('#modalProveedorNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var ProveedorNew = $('#ProveedorNew').val();

                    // Validaciones
                    if (ProveedorNew === "" || ProveedorNew === null) {
                        Procesar = false;
                        $("#ProveedorNewLabel").addClass("LabelRed");
                    } else {
                        $("#ProveedorNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("ProveedorOld", Proveedor);
                        data.append("ProveedorNew", ProveedorNew);

                        try {
                            $("#modalProveedorNew-ok").prop("disabled", true);

                            const response = await fetch('/Proveedores/Proveedor_Renombrar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#ProveedorNewLabel").addClass("LabelRed");
                                $("#ProveedorStatusLabel").css("display", "block");
                                $("#ProveedorStatusLabel").text(result.message);
                                $("#modalProveedorNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalProveedorRenombrar').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            console.error('Error al renombrar proveedor:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalProveedorNew-cancel').off('click').on('click', () => {
                    $('#modalProveedorRenombrar').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalProveedorRenombrar').on('hidden.bs.modal', () => {
                    $('#modalProveedorRenombrar').remove();
                    var script = document.getElementById("modalProveedorRenombrar_js");
                    if (script) $(script).remove();
                });
            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });
    });
};


let N_DialogDivisa = async function N_DialogDivisaFunc(Guid) {
    try {
        // Elimina del DOM si ya existía
        $('#modalDivisa').remove();

        // Carga la vista parcial con fetch
        let response = await fetch("/Divisas/DialogDivisa?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abre el diálogo
        $('#modalDivisa').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalDivisa-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let Procesar = true;
                if ($("#Divisa").val() == "") {
                    Procesar = false;
                    $("#DivisaLabel").addClass("LabelRed");
                } else {
                    $("#DivisaLabel").removeClass("LabelRed");
                }
                if ($("#DivNombre").val() == "") {
                    Procesar = false;
                    $("#DivNombreLabel").addClass("LabelRed");
                } else {
                    $("#DivNombreLabel").removeClass("LabelRed");
                }
                if ($("#DivSimbolo").val() == "") {
                    Procesar = false;
                    $("#DivSimboloLabel").addClass("LabelRed");
                } else {
                    $("#DivSimboloLabel").removeClass("LabelRed");
                }
                if ($("#DivCambio").val() == "") {
                    Procesar = false;
                    $("#DivCambioLabel").addClass("LabelRed");
                } else {
                    $("#DivCambioLabel").removeClass("LabelRed");
                }

                if (!Procesar) {
                    var TabsObj = wijmo.Control.getControl('.TabsDivisa');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                // Procesamiento del formulario
                var Spinner = Rats.UI.LoadAnimation.start();
                var form = $("#FormDivisaCreateEdit");
                var formData = new FormData(form[0]);

                // Agrega campos al formData
                formData.append("Divisa", $("#Divisa").val());
                formData.append("IsoUser", $("#IsoUser").val());
                formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                formData.append("IsoFecMod", $("#IsoFecMod").val());

                try {
                    let saveResponse = await fetch("/Divisas/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');

                    let result = await saveResponse.text();
                    Spinner.stop();

                    if (result !== "EXIST") {
                        $('#modalDivisa').modal('hide');
                        let resDialogDivisa = JSON.parse(result);
                        resolve(resDialogDivisa); // Resuelve la promesa con el resultado

                    }

                } catch (error) {
                    Spinner.stop();
                    console.error('Error en el proceso de guardado:', error);
                    reject(error); // Rechaza la promesa en caso de error
                }
            });

            $('#modalDivisa-cancel').off('click').on('click', function () {
                $('#modalDivisa').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalDivisa').on('hidden.bs.modal', function () {
                $('#modalDivisa').remove();
            });
        });

    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error; // Lanza el error para manejarlo en el catch donde se llama a N_DialogDivisa
    }
};



let N_DialogAlmacenes = async function N_DialogAlmacenesFunc(Guid) {

    try {

        // Eliminar del DOM si previamente existía
        $('#modalAlmacenes').remove();

        // Cargar vista parcial
        let response = await fetch("/Stocks/DialogAlmacenes");
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();

        $("body").append(data);

        // Abrir el dialogo
        $('#modalAlmacenes').modal('show');

        return new Promise((resolve, reject) => {

            $('#modalAlmacenes-ok').off('click').on('click', async (evt) => {
                evt.preventDefault();

                try {

                    let fetchResponse = await fetch("/Stocks/GetAlmacenes");
                    if (!fetchResponse.ok) throw new Error('Error al obtener almacenes');
                    let almacenes = await fetchResponse.json();

                    var ComboAlmacen = wijmo.Control.getControl('#ComboAlmacen');
                    ComboAlmacen.itemSource = alamacenes;
                    resolve(null);

                } catch (error) {

                    console.error('Error en la obtencion de almacenes: ', error);
                    reject(error);

                } finally {

                    $('#modalAlmacenes').modal('hide');

                }
            });

            $('#modalAlmacenes-cancel').off('click').on('click', function () {
                reject('Operación cancelada por el usuario');
            });

            $('#modalAlmacenes').on('hidden.bs.modal', function () {
                $('#modalAlmacenes').remove();
            });

        });

    } catch (error) {
        console.error('Error al cargar la vista parcial: ', error);
        throw error;
    }
};




//////////////////////////////////////////////////// ARTICULOS SCRIPTS /////////////////////////////////////////////////////////////////////////////////


let N_DialogArticuloRenombrar = function N_DialogArticuloFunc(Articulo) {
    return new Promise((resolve, reject) => {

        // Elimina el diálogo anterior del DOM
        $('#modalArtNew').remove();
        var script = document.getElementById("modalArtNew_js");
        $(script).remove();

        // Carga la vista parcial
        fetch(`/Articulos/_DialogArtRENCOP?Articulo=${Articulo}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);

                $('#modalArtNewTitle').text("Renombrar Atículo");

                $('#modalArtNew').modal('show');

                $('#modalArtNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var ArticuloNew = $('#ArticuloNew').val();

                    // Validaciones
                    if (ArticuloNew === "" || ArticuloNew === null) {
                        Procesar = false;
                        $("#ArticuloNewLabel").addClass("LabelRed");
                    } else {
                        $("#ArticuloNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("ArticuloOld", Articulo);
                        data.append("ArticuloNew", ArticuloNew);

                        try {
                            $("#modalArtNew-ok").prop("disabled", true);

                            const response = await fetch('/Articulos/Articulo_Renombrar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#ArticuloNewLabel").addClass("LabelRed");
                                $("#ArticuloStatusLabel").css("display", "block");
                                $("#ArticuloStatusLabel").text(result.message);
                                $("#modalArtNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalArtNew').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            //console.error('Error al renombrar pedido:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalArtNew-cancel').off('click').on('click', () => {
                    $('#modalArtNew').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalArtNew').on('hidden.bs.modal', () => {
                    $('#modalArtNew').remove();
                    var script = document.getElementById("modalArtNew_js");
                    $(script).remove();
                });

            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });

    });
};


let N_DialogArticuloCopiar = function N_DialogArticuloFunc(Articulo) {
    return new Promise((resolve, reject) => {

        // Elimina el diálogo anterior del DOM
        $('#modalArtNew').remove();
        var script = document.getElementById("modalArtNew_js");
        $(script).remove();

        // Carga la vista parcial
        fetch(`/Articulos/_DialogArtRENCOP?Articulo=${Articulo}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);

                $('#modalArtNewTitle').text("Copiar Atículo");

                $('#modalArtNew').modal('show');

                $('#modalArtNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var ArticuloNew = $('#ArticuloNew').val();

                    // Validaciones
                    if (ArticuloNew === "" || ArticuloNew === null) {
                        Procesar = false;
                        $("#ArticuloNewLabel").addClass("LabelRed");
                    } else {
                        $("#ArticuloNewLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("ArticuloOld", Articulo);
                        data.append("ArticuloNew", ArticuloNew);

                        try {
                            $("#modalArtNew-ok").prop("disabled", true);

                            const response = await fetch('/Articulos/Articulo_Copiar', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();

                            if (!result.success) {
                                $("#ArticuloNewLabel").addClass("LabelRed");
                                $("#ArticuloStatusLabel").css("display", "block");
                                $("#ArticuloStatusLabel").text(result.message);
                                $("#modalArtNew-ok").prop("disabled", false);
                                return;
                            }

                            $('#modalArtNew').modal('hide');
                            resolve(result.data);

                        } catch (error) {
                            //console.error('Error al renombrar pedido:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalArtNew-cancel').off('click').on('click', () => {
                    $('#modalArtNew').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalArtNew').on('hidden.bs.modal', () => {
                    $('#modalArtNew').remove();
                    var script = document.getElementById("modalArtNew_js");
                    $(script).remove();
                });

            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });

    });
};



let N_DialogStocks = async function N_DialogStocksFunc(Guid) {
    try {

        // Elimina del DOM si ya existía
        $('#modalStock').remove();

        // Carga la vista parcial
        let response = await fetch(`/Stocks/DialogStocks?Guid=${Guid}`);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();

        $("body").append(data);
        $('#modalStock').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalStock-ok').off('click').on('click', async function (evt) {
                evt.preventDefault();

                let Procesar = true;
                // Aquí iría tu lógica de validación...

                if (Procesar) {
                    try {
                        var Spinner = Rats.UI.LoadAnimation.start();

                        var form = $("#FormStockCreateEdit");
                        var formData = new FormData(form[0]);
                        // Agregar campos adicionales si es necesario
                        formData.append("Divisa", $("#Divisa").val());
                        formData.append("IsoUser", $("#IsoUser").val());
                        formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                        formData.append("IsoFecMod", $("#IsoFecMod").val());

                        $("#modalStock-ok").prop("disabled", true);

                        let saveResponse = await fetch("/Stocks/CreateEdit", {
                            method: 'POST',
                            body: formData
                        });
                        if (!saveResponse.ok) throw new Error('Error en la solicitud de Create/Edit');

                        let result = await saveResponse.json();

                        Spinner.stop();
                        if (result === "EXIST") {
                            $("#modalStockLabel").addClass("LabelRed");
                            $("#modalStock-ok").prop("disabled", false);
                        } else {
                            $('#modalStock').modal('hide');
                            resolve(result);
                        }
                    } catch (error) {
                        Spinner.stop();
                        console.error('Error en el proceso:', error);
                        reject(error);
                    }
                } else {
                    var TabsObj = wijmo.Control.getControl('.TabsStock');
                    TabsObj.selectedIndex = 0;
                }
            });

            $('#modalStock-cancel').off('click').on('click', function () {
                $('#modalStock').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalStock').on('hidden.bs.modal', function () {
                $('#modalStock').remove();
            });
        });
    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error; // Lanza el error para manejarlo en un nivel superior si es necesario
    }
};


let N_DialogBanco = async function N_DialogBancoFunc(Guid) {
    try {

        alert("Entrando en N_DialogBanco")

        // Eliminar del DOM si previamente existía
        $('#modalBanco').remove();

        // Cargar vista parcial
        let response = await fetch("/Contabilidad/DialogBanco?Guid=" + Guid);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abrir el diálogo
        $('#modalBanco').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalBanco-ok').off('click').on('click', async (evt) => {
                evt.preventDefault();

                let Procesar = true;
                if ($("#Banco").val() == "") {
                    Procesar = false;
                    $("#BancoLabel").addClass("LabelRed");
                } else {
                    $("#BancoLabel").removeClass("LabelRed");
                }
                if ($("#BcoNombre").val() == "") {
                    Procesar = false;
                    $("#BcoNombreLabel").addClass("LabelRed");
                } else {
                    $("#BcoNombreLabel").removeClass("LabelRed");
                }

                if (!Procesar) {
                    var TabsObj = wijmo.Control.getControl('.TabsBanco');
                    TabsObj.selectedIndex = 0;
                    return;
                }

                try {
                    var Spinner = Rats.UI.LoadAnimation.start();
                    var form = $("#FormBancoCreateEdit");
                    var formData = new FormData(form[0]);
                    formData.append("Banco", $("#Banco").val());
                    formData.append("IsoUser", $("#IsoUser").val());
                    formData.append("IsoFecAlt", $("#IsoFecAlt").val());
                    formData.append("IsoFecMod", $("#IsoFecMod").val());

                    let saveResponse = await fetch("/Contabilidad/CreateEdit_BANCO", {
                        method: 'POST',
                        body: formData
                    });

                    const contentType = saveResponse.headers.get("content-type");
                    if (contentType && contentType.includes("application/json")) {
                        const result = await saveResponse.json();

                        if (result === "EXIST") {
                            $("#BancoLabel").addClass("LabelRed");
                            $("#modalBanco-ok").prop("disabled", false);
                        } else {
                            $('#modalBanco').modal('hide');
                            resolve(result);
                        }
                    } else {
                        const textResult = await saveResponse.text();
                        if (textResult === "EXIST") {
                            $("#BancoLabel").addClass("LabelRed");
                            $("#modalBanco-ok").prop("disabled", false);
                        }
                    }
                    Spinner.stop();
                } catch (error) {
                    Spinner.stop();
                    console.error('Error en el proceso de guardado:', error);
                    reject(error);
                }
            });

            $('#modalBanco-cancel').off('click').on('click', function () {
                $('#modalBanco').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalBanco').on('hidden.bs.modal', function () {
                $('#modalBanco').remove();
            });
        });

    } catch (error) {

        console.error('Error al cargar la vista parcial:', error);
        throw error;

    }
};



let N_DialogValsys = async function N_DialogValsysFunc(Guid) {
    try {
        // Eliminar del DOM si previamente existía
        $('#modalValsys').remove();

        // Cargar vista parcial con fetch
        let response = await fetch(`/Valsys/DialogValsys?Guid=${Guid}`);
        if (!response.ok) throw new Error('Error al cargar la vista parcial');
        let data = await response.text();
        $("body").append(data);

        // Abrir el diálogo
        $('#modalValsys').modal('show');

        return new Promise((resolve, reject) => {
            $('#modalValsys-ok').off('click').on('click', async (evt) => {
                evt.preventDefault();

                let Procesar = true;
                var ComboIndice = wijmo.Control.getControl('#Indice');
                if (!ComboIndice.selectedValue) {
                    Procesar = false;
                    $("#IndiceLabel").addClass("LabelRed");
                } else {
                    $("#IndiceLabel").removeClass("LabelRed");
                }
                if ($("#Clave").val() === "") {
                    Procesar = false;
                    $("#ClaveLabel").addClass("LabelRed");
                } else {
                    $("#ClaveLabel").removeClass("LabelRed");
                }

                if (!Procesar) return;

                try {
                    var form = $("#FormValsysCreateEdit")[0];
                    var formData = new FormData(form);
                    formData.append("Indice", ComboIndice.selectedValue);
                    formData.append("Clave", $("#Clave").val());

                    let saveResponse = await fetch("/Valsys/CreateEdit", {
                        method: 'POST',
                        body: formData
                    });

                    if (!saveResponse.ok) throw new Error('Error en el proceso de guardado');
                    let result = await saveResponse.text();

                    if (result !== "EXIST") {
                        $('#modalValsys').modal('hide');
                        resolve(result);
                    } else {
                        $("#ClaveLabel").addClass("LabelRed");
                        $("#modalValsys-ok").prop("disabled", false);
                    }
                } catch (error) {
                    console.error('Error en el proceso de guardado:', error);
                    reject(error);
                }
            });

            $('#modalValsys-cancel').off('click').on('click', function () {
                $('#modalValsys').modal('hide');
                reject('Operación cancelada por el usuario');
            });

            $('#modalValsys').on('hidden.bs.modal', function () {
                $('#modalValsys').remove();
            });
        });
    } catch (error) {
        console.error('Error al cargar la vista parcial:', error);
        throw error;
    }
};


let N_DialogArtMet = function N_DialogArtMetFunc(Guid,Articulo) {
    return new Promise((resolve, reject) => {


        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalArtMet').remove();
        var script = document.getElementById("modalArtMet_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Articulos/DialogArtMet?Guid=" + Guid + "&Articulo=" + Articulo, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalArtMet').modal('show');
            $('#modalArtMet-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;
                ////var ComboIndice = wijmo.Control.getControl('#Indice');
                ////if (ComboIndice.selectedValue == "" || ComboIndice.selectedValue == null) {
                ////    Procesar = false;
                ////    $("#IndiceLabel").addClass("LabelRed");
                ////} else {
                ////    $("#IndiceLabel").removeClass("LabelRed");
                ////}
                var ComboMetal = wijmo.Control.getControl('#Metal');

                if (ComboMetal.selectedValue == "" || ComboMetal.selectedValue == null) {
                    Procesar = false;
                    $("#MetalLabel").addClass("LabelRed");
                } else {
                    $("#MetalLabel").removeClass("LabelRed");
                }


                if (Procesar == false) {
                    //return false;
                } else {
                    //var Spinner = Rats.UI.LoadAnimation.start();

                    var form = $("#FormArtMetCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });
                    data.append("Articulo", Articulo);
                    data.append("Metal", ComboMetal.selectedValue);
                    data.append("Liga", $("#Liga").val());

                    $("#modalArtMet-ok").prop("disabled", true);
                    CallAsyncAjax("/Articulos/CreateEdit_MET", data).then((res) => {

                        //Spinner.stop();

                        if (res == "EXIST") {
                            $("#MetalLabel").addClass("LabelRed");
                            $("#modalArtMet-ok").prop("disabled", false);
                        } else {
                            $("#modalArtMet").modal("hide");
                            resolve(res);
                        }



                    }).catch((error) => {
                        //Spinner.stop();
                    });

                }

            });

            $('#modalArtMet-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalArtMet').on('hidden.bs.modal', function () {
                $('#modalArtMet').remove();
                var script = document.getElementById("modalArtMet_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogArtComp = function N_DialogArtCompFunc(Guid, Articulo) {
    return new Promise((resolve, reject) => {


        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalArtComp').remove();
        var script = document.getElementById("modalArtComp_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Articulos/DialogArtComp?Guid=" + Guid + "&Articulo=" + Articulo, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalArtComp').modal('show');
            $('#modalArtComp-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;
                var ComboComponente = wijmo.Control.getControl('#ComboComponente');
                if (ComboComponente.selectedValue == "" || ComboComponente.selectedValue == null) {
                    Procesar = false;
                    $("#ComponenteLabel").addClass("LabelRed");
                } else {
                    $("#ComponenteLabel").removeClass("LabelRed");
                }


                if (Procesar == false) {
                    //return false;
                } else {
                    //var Spinner = Rats.UI.LoadAnimation.start();

                    var form = $("#FormArtCompCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Articulo", Articulo);
                    var ComboComponente = wijmo.Control.getControl('#ComboComponente');
                    data.append("Componente", ComboComponente.selectedValue);
                    


                    $("#modalArtComp-ok").prop("disabled", true);
                    CallAsyncAjax("/Articulos/CreateEdit_COMP", data).then((res) => {

                        //Spinner.stop();

                        if (res == "EXIST") {
                            $("#ComponenteLabel").addClass("LabelRed");
                            $("#modalArtComp-ok").prop("disabled", false);
                        } else {
                            $("#modalArtComp").modal("hide");
                            resolve(res);
                        }



                    }).catch((error) => {
                        //Spinner.stop();
                    });

                }

            });

            $('#modalArtComp-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalArtComp').on('hidden.bs.modal', function () {
                $('#modalArtComp').remove();
                var script = document.getElementById("modalArtComp_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogArtCompPIE = function N_DialogArtCompPIEFunc(Guid, Articulo) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalArtPIE').remove();
        var script = document.getElementById("modalArtPIE_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Articulos/DialogArtPIE?Guid=" + Guid + "&Articulo=" + Articulo, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalArtPIE').modal('show');
            $('#modalArtPIE-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;
                var ComboComponente = wijmo.Control.getControl('#ComboComponente');
                if (ComboComponente.selectedValue == "" || ComboComponente.selectedValue == null) {
                    Procesar = false;
                    $("#ComponenteLabel").addClass("LabelRed");
                } else {
                    $("#ComponenteLabel").removeClass("LabelRed");
                }

                if (Procesar == false) {
                    //return false;
                } else {
                    //var Spinner = Rats.UI.LoadAnimation.start();

                    var form = $("#FormArtPIECreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Articulo", Articulo);
                    var ComboComponente = wijmo.Control.getControl('#ComboComponente');
                    data.append("Componente", ComboComponente.selectedValue);
                    var ComboTipoPrecio = wijmo.Control.getControl('#ComboTipoPrecio');
                    data.append("TipoPrecio", ComboTipoPrecio.selectedValue);


                    $("#modalArtPIE-ok").prop("disabled", true);
                    CallAsyncAjax("/Articulos/CreateEdit_PIE", data).then((res) => {

                        //Spinner.stop();

                        if (res == "EXIST") {
                            $("#ComponenteLabel").addClass("LabelRed");
                            $("#modalArtPIE-ok").prop("disabled", false);
                        } else {
                            $("#modalArtPIE").modal("hide");
                            resolve(res);
                        }

                    }).catch((error) => {
                        //Spinner.stop();
                    });

                }

            });

            $('#modalArtPIE-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalArtPIE').on('hidden.bs.modal', function () {
                $('#modalArtPIE').remove();
                var script = document.getElementById("modalArtPIE_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogArtMo = function N_DialogArtMoFunc(Guid, Articulo) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalArtMo').remove();
        var script = document.getElementById("modalArtMo_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Articulos/DialogArtMo?Guid=" + Guid + "&Articulo=" + Articulo, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalArtMo').modal('show');
            $('#modalArtMo-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;

                var ComboCodMO = wijmo.Control.getControl('#CodMO');
                if (ComboCodMO.selectedValue == "" || ComboCodMO.selectedValue == null) {
                    Procesar = false;
                    $("#CodMOLabel").addClass("LabelRed");
                } else {
                    $("#CodMOLabel").removeClass("LabelRed");
                }

                var ComboMOExtInt = wijmo.Control.getControl('#MOExtInt');
                if (ComboMOExtInt.selectedValue == "" || ComboMOExtInt.selectedValue == null) {
                    Procesar = false;
                    $("#MOExtIntLabel").addClass("LabelRed");
                } else {
                    $("#MOExtIntLabel").removeClass("LabelRed");
                }

                var ComboMOUniMed = wijmo.Control.getControl('#MOUniMed');
                if (ComboMOUniMed.selectedValue == "" || ComboMOUniMed.selectedValue == null) {
                    Procesar = false;
                    $("#MOUniMedLabel").addClass("LabelRed");
                } else {
                    $("#MOUniMedLabel").removeClass("LabelRed");
                }


                if (Procesar == false) {
                    //return false;
                } else {
                    //var Spinner = Rats.UI.LoadAnimation.start();

                    var form = $("#FormArtMoCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Articulo", Articulo);
                    data.append("CodMO",    ComboCodMO.selectedValue);


                    $("#modalArtMo-ok").prop("disabled", true);
                    CallAsyncAjax("/Articulos/CreateEdit_MO", data).then((res) => {

                        //Spinner.stop();

                        if (res == "EXIST") {
                            $("#CodMOLabel").addClass("LabelRed");
                            $("#modalArtMo-ok").prop("disabled", false);
                        } else {
                            $("#modalArtMo").modal("hide");
                            resolve(res);
                        }



                    }).catch((error) => {
                        //Spinner.stop();
                    });

                }

            });

            $('#modalArtMo-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalArtMo').on('hidden.bs.modal', function () {
                $('#modalArtMo').remove();
                var script = document.getElementById("modalArtMo_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogSimuladorPrecios = function N_DialogSimuladorPreciosFunc(GuidArt, SelectArticulo, SelectOroFino, SelectKilates, SelectColorOro, SelectTarifa) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalPrecios').remove();
        var script = document.getElementById("modalPrecios_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Articulos/DialogSimuladorPrecios?GuidArt=" + GuidArt + "&SelectArticulo=" + SelectArticulo +
            "&SelectOroFino=" + SelectOroFino + "&SelectKilates=" + SelectKilates + "&SelectColorOro=" + SelectColorOro + "&SelectTarifa=" + SelectTarifa, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalPrecios').modal('show');

            $('#modalPrecios-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                $("#modalPrecios").modal("hide");

                //debug
                resolve(false);
            });



            $('#modalPrecios-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });
            $('#modalPrecios').on('hidden.bs.modal', function () {
                $('#modalPrecios').remove();
                var script = document.getElementById("modalPrecios_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogFacturaNew = function N_DialogFacturaNewFunc(Cliente) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalFacNew').remove();
        var script = document.getElementById("modalFacNew_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Facturas/_DialogFacNew?Cliente=" + Cliente, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalFacNew').modal('show');
            $('#modalFacNew-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;

                var ComboFacNewClientes = wijmo.Control.getControl('#ComboFacNewClientes');
                var ComboFacNewTipo = wijmo.Control.getControl('#ComboFacNewTipo');

                if (ComboFacNewClientes.selectedValue == "" || ComboFacNewClientes.selectedValue == null) {
                    Procesar = false;
                    $("#CodFacNewClienteLabel").addClass("LabelRed");
                } else {
                    $("#CodFacNewClienteLabel").removeClass("LabelRed");
                }

                if (ComboFacNewTipo.selectedValue == "" || ComboFacNewTipo.selectedValue == null) {
                    Procesar = false;
                    $("#CodFacNewTipoLabel").addClass("LabelRed");
                } else {
                    $("#CodFacNewTipoLabel").removeClass("LabelRed");
                }


                if (Procesar == false) {
                    //return false;
                } else {

                    var form = $("#FormFacNewCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Cliente", ComboFacNewClientes.selectedValue);
                    data.append("TipoFac", ComboFacNewTipo.selectedValue);


                    $("#modalFacNew-ok").prop("disabled", true);
                    CallAsyncAjax("/Facturas/Create_Factura", data).then((res) => {

                        //console.log("Create_Pedido: " + res);

                        resolve(res);
                        //Spinner.stop();

                        //if (res == "EXIST") {
                        //    $("#CodMOLabel").addClass("LabelRed");
                        //    $("#modalPedNew-ok").prop("disabled", false);
                        //} else {
                        //    $("#modalPedNew").modal("hide");
                        //    resolve(res);
                        //}
                    }).catch((error) => {
                        //Spinner.stop();
                    });

                }

            });

            $('#modalFacNew-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalFacNew').on('hidden.bs.modal', function () {
                $('#modalFacNew').remove();
                var script = document.getElementById("modalFacNew_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogPedidoNew = function N_DialogPedidoNewFunc(Cliente) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalPedNew').remove();
        var script = document.getElementById("modalPedNew_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Pedidos/_DialogPedNew?Cliente=" + Cliente, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalPedNew').modal('show');
            $('#modalPedNew-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;

                var ComboPedNewClientes = wijmo.Control.getControl('#ComboPedNewClientes');
                var ComboPedNewTipo     = wijmo.Control.getControl('#ComboPedNewTipo');

                if (ComboPedNewClientes.selectedValue == "" || ComboPedNewClientes.selectedValue == null) {
                    Procesar = false;
                    $("#CodPedNewClienteLabel").addClass("LabelRed");
                } else {
                    $("#CodPedNewClienteLabel").removeClass("LabelRed");
                }

                if (ComboPedNewTipo.selectedValue == "" || ComboPedNewTipo.selectedValue == null) {
                    Procesar = false;
                    $("#CodPedNewTipoLabel").addClass("LabelRed");
                } else {
                    $("#CodPedNewTipoLabel").removeClass("LabelRed");
                }


                if (Procesar == false) {
                    //return false;
                } else {

                    var form = $("#FormPedNewCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Cliente", ComboPedNewClientes.selectedValue);
                    data.append("TipoPed", ComboPedNewTipo.selectedValue);


                    $("#modalPedNew-ok").prop("disabled", true);
                    CallAsyncAjax("/Pedidos/Create_Pedido", data).then((res) => {


                        resolve(res);
                       
                    }).catch((error) => {
                    });

                }

            });

            $('#modalPedNew-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalPedNew').on('hidden.bs.modal', function () {
                $('#modalPedNew').remove();
                var script = document.getElementById("modalPedNew_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogPedidoRen = function N_DialogPedidoRenFunc(Cliente, Pedido) {
    return new Promise((resolve, reject) => {

        // Elimina el diálogo anterior del DOM
        $('#modalPedNew').remove();
        var script = document.getElementById("modalPedNew_js");
        $(script).remove();

        // Carga la vista parcial
        fetch(`/Pedidos/_DialogPedRen?Cliente=${Cliente}`)
            .then(response => response.text())
            .then(data => {
                $("body").append(data);
                // Abre el diálogo
                $('#modalPedNew').modal('show');

                $('#modalPedNew-ok').off('click').on('click', async (evt) => {
                    evt.preventDefault();

                    let Procesar = true;
                    var ComboPedNewClientes = wijmo.Control.getControl('#ComboPedNewClientes');

                    // Validaciones
                    if (ComboPedNewClientes.selectedValue === "" || ComboPedNewClientes.selectedValue === null) {
                        Procesar = false;
                        $("#CodPedNewClienteLabel").addClass("LabelRed");
                    } else {
                        $("#CodPedNewClienteLabel").removeClass("LabelRed");
                    }

                    if (Procesar) {
                        var data = new FormData();
                        data.append("Pedido", Pedido); 
                        data.append("Cliente", ComboPedNewClientes.selectedValue);

                        //console.log("Enviando solicitud para renombrar pedido:", Pedido, "Nuevo Cliente:", ComboPedNewClientes.selectedValue);

                        try {
                            $("#modalPedNew-ok").prop("disabled", true);

                            const response = await fetch('/Pedidos/Rename_Pedido', {
                                method: 'POST',
                                body: data
                            });

                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }

                            const result = await response.json();
                            $('#modalPedNew').modal('hide');
                            resolve(result);

                        } catch (error) {
                            console.error('Error al renombrar pedido:', error);
                            reject(error);
                        }
                    }
                });

                // Manejador de evento 'cancel'
                $('#modalPedNew-cancel').off('click').on('click', () => {
                    $('#modalPedNew').modal('hide');
                });

                // Manejador de evento 'hidden'
                $('#modalPedNew').on('hidden.bs.modal', () => {
                    $('#modalPedNew').remove();
                    var script = document.getElementById("modalPedNew_js");
                    $(script).remove();
                });

            })
            .catch(error => {
                console.error('Error al obtener la vista parcial:', error);
                reject(error);
            });

    });
};


let N_DialogPedLinMedidas = function N_DialogPedLinMedidasFunc(Medidas, QtyLin) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalPedlinMed').remove();
        var script = document.getElementById("modalPedlinMed_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Pedidos/_DialogPedLinMedidas?Medidas=" + Medidas + "&QtyLin=" + QtyLin, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalPedlinMed').modal('show');
            $('#modalPedlinMed-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                $("#modalPedNew-ok").prop("disabled", true);
                //console.log("modalPedlinMed ok: " + CadMed);

                var CadMed = $("#CadenaFinalMedidas").val();

                resolve(CadMed);
                $("#modalPedlinMed").modal("hide");

            });

            $('#modalPedlinMed-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalPedlinMed').on('hidden.bs.modal', function () {
                $('#modalPedlinMed').remove();
                var script = document.getElementById("modalPedlinMed_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogPedLinChangeColor = function N_DialogPedLinChangeColorFunc(Pedido, PedLinea, SecuenciaComp, Componente, CompoQtyTotal) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalPedlinChgColor').remove();
        var script = document.getElementById("modalPedlinChgColor_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Pedidos/_DialogPedLinChangeColor?Pedido=" + Pedido + "&PedLinea=" + PedLinea + "&SecuenciaComp=" + SecuenciaComp + "&Componente=" + Componente + "&CompoQtyTotal=" + CompoQtyTotal, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalPedlinChgColor').modal('show');
            $('#modalPedlinChgColor-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                $("#modalPedNew-ok").prop("disabled", true);
                //console.log("modalPedlinChgColor ok: " + CadMed);

                var CadMed = $("#CadenaFinalMedidas").val();

                resolve(CadMed);
                $("#modalPedlinChgColor").modal("hide");

            });

            $('#modalPedlinChgColor-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalPedlinChgColor').on('hidden.bs.modal', function () {
                $('#modalPedlinChgColor').remove();
                var script = document.getElementById("modalPedlinChgColor_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogAlbaranNew = function N_DialogAlbaranNewFunc(Cliente) {
    return new Promise((resolve, reject) => {

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalAlbNew').remove();
        var script = document.getElementById("modalAlbNew_js");
        $(script).remove();

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Albaranes/_DialogAlbNew?Cliente=" + Cliente, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalAlbNew').modal('show');
            $('#modalAlbNew-ok').off('click').on('click', function (evt) {
                evt.preventDefault();

                let Procesar = true;

                var ComboPedNewClientes = wijmo.Control.getControl('#ComboAlbNewClientes');
                var ComboPedNewTipo = wijmo.Control.getControl('#ComboAlbNewTipo');

                if (ComboPedNewClientes.selectedValue == "" || ComboPedNewClientes.selectedValue == null) {
                    Procesar = false;
                    $("#CodAlbNewClienteLabel").addClass("LabelRed");
                } else {
                    $("#CodAlbNewClienteLabel").removeClass("LabelRed");
                }

                if (ComboPedNewTipo.selectedValue == "" || ComboPedNewTipo.selectedValue == null) {
                    Procesar = false;
                    $("#CodAlbNewTipoLabel").addClass("LabelRed");
                } else {
                    $("#CodAlbNewTipoLabel").removeClass("LabelRed");
                }


                if (Procesar == false) {
                    //return false;
                } else {

                    var form = $("#FormAlbNewCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Cliente", ComboPedNewClientes.selectedValue);
                    data.append("TipoAlb", ComboPedNewTipo.selectedValue);


                    $("#modalAlbNew-ok").prop("disabled", true);
                    CallAsyncAjax("/Albaranes/Create_Albaran", data).then((res) => {

                        //console.log("Create_Pedido: " + res);

                        resolve(res);
                        //Spinner.stop();

                        //if (res == "EXIST") {
                        //    $("#CodMOLabel").addClass("LabelRed");
                        //    $("#modalPedNew-ok").prop("disabled", false);
                        //} else {
                        //    $("#modalPedNew").modal("hide");
                        //    resolve(res);
                        //}
                    }).catch((error) => {
                        //Spinner.stop();
                    });

                }

            });

            $('#modalAlbNew-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalAlbNew').on('hidden.bs.modal', function () {
                $('#modalAlbNew').remove();
                var script = document.getElementById("modalAlbNew_js");
                $(script).remove();
            })

        });

    });



};

let N_DialogSelPedLin = function N_DialogSelPedLinFunc(ListSelectPedRowsGuidParam, FiltroMrp, FiltroAlbaran) {
    return new Promise((resolve, reject) => {

        //console.log(ListSelectPedRowsGuidParam);

        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalSelectPedLin').remove();


        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Pedidos/DialogSelPedLin?ListSelectPedRowsGuidParam=" + ListSelectPedRowsGuidParam + "&FiltroMrp=" + FiltroMrp + "&FiltroAlbaran=" + FiltroAlbaran, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalSelectPedLin').modal('show');

            $('#modalSelectPedLin-ok').off('click').on('click', function () {
                //evt.preventDefault();
                var theGridPedLin = wijmo.Control.getControl("#gridSelectPedLin");
                if (theGridPedLin) {

                    ListSelectPedLineas = [];
                    var cntSelect = 0;
                    for (var i = 0; i < theGridPedLin.rows.length; i++) {
                        if (theGridPedLin.rows[i].isSelected) {
                            cntSelect++;
                            //ListSelectPedLineas.push(theGrid.rows[i].dataItem.Guid);
                            ListSelectPedLineas.push(theGridPedLin.rows[i].dataItem);
                        }
                    }

                    if (cntSelect > 0) {
                        resolve(ListSelectPedLineas);
                    }
                }

                //resolve(null);
            });

            $('#modalSelectPedLin-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalSelectPedLin').on('hidden.bs.modal', function () {
                $('#modalSelectPedLin').remove();
            })

        });

    });



};

let N_DialogFixing = function N_DialogFixingFunc(Guid) {

    return new Promise((resolve, reject) => {
        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalFixing').remove();

        // alert("Antes de se carga");

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/Fixing/DialogFixing?Guid=" + Guid, function (data) {
            // alert("Se detuvo antes de append");
            $("body").append(data);
            // alert("A pasado append");
            // SE ABRE EL DIALOGO
            $('#modalFixing').modal('show');
            $('#modalFixing-ok').off('click').on('click', function (evt) {
                // alert("Ejecturamos desde el script");
                evt.preventDefault();

                //alert("Estamos justo despues del prevent default");
                let Procesar = true;

                var ComboTipoMetal = wijmo.Control.getControl('#ComboTipoMetal');
                var TipoMetal = ComboTipoMetal.selectedItem;
                let oroFinoVenta = $('#OroFinoVenta').val().replace(",", "."); // Sacá el valor una sola vez
                oroFinoVenta = parseFloat(oroFinoVenta);

                if (TipoMetal == "") {
                    Procesar = false;
                    $("#TipoMetalLabel").addClass("LabelRed");
                } else {
                    if (TipoMetal == "ORO") {
                        // Unificá las condiciones
                        if (isNaN(oroFinoVenta) || oroFinoVenta <= 0) {
                            Procesar = false;
                            $('#OroFinoVentaError').text("El campo Oro Fino Venta debe ser mayor que 0.");
                            $("#OroFinoVentaLabel").addClass("LabelRed");
                        } else {
                            $("#OroFinoVentaLabel").removeClass("LabelRed");
                        }
                    } else {
                        if ($("#Metal").val() == "") {
                            Procesar = false;
                            $("#MetalLabel").addClass("LabelRed");
                        }
                    }
                }



                if (Procesar == false) {
                    //return false;
                    //resolve(null);
                } else {
                    var Spinner = Rats.UI.LoadAnimation.start();

                    var form = $("#FormFixingCreateEdit");
                    var data = new FormData();
                    $.each(form.serializeArray(), function (key, input) {
                        data.append(input.name, input.value);
                    });

                    data.append("Metal", $("#Metal").val());
                    data.append("IsoUser", $("#IsoUser").val());
                    data.append("IsoFecAlt", $("#IsoFecAlt").val());
                    data.append("IsoFecMod", $("#IsoFecMod").val());

                    $("#modalFixing-ok").prop("disabled", true);

                    CallAsyncFetch("/Fixing/CreateEdit", data).then((res) => {
                        Spinner.stop();

                        if (res.message == "EXIST") {
                            // alert("Si res == Exist")
                            //alert("Estoy saliendo porque voy a entrar a N_ModalInfo, aunque aun no entré...")
                            //N_ModalInfo("Ya existe un registro para este metal en la fecha seleccionada.").then((res) => {
                            //    $("#FixingLabel").addClass("LabelRed");
                            //    $("#modalFixing-ok").prop("disabled", false);
                            //});

                            $("#MetalLabel").addClass("LabelRed");
                        } else {
                            $("#modalFixing").modal("hide");
                            resolve(res);
                        }

                    }).catch((error) => {
                        Spinner.stop();
                    });


                }

            });

            $('#modalFixing-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalFixing').on('hidden.bs.modal', function () {
                $('#modalFixing').remove();
            })

        });

    });



};

let N_DialogLeyAntiFraude = function N_DialogLeyAntiFraudeFunc(Guid) {

    return new Promise((resolve, reject) => {
        // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA: TB programado en el evento hidden.bs.modal
        $('#modalLA').remove();

        // alert("Antes de se carga");

        // SE CARGA PRIMERO LA VISTA PARCIAL
        $.get("/LeyAntiFraude/DialogLeyAntiFraude?Guid=" + Guid, function (data) {

            $("body").append(data);

            // SE ABRE EL DIALOGO
            $('#modalLA').modal('show');


            $('#modalLA-cancel').off('click').on('click', function () {
                /*resolve(false);*/
            });

            $('#modalLA').on('hidden.bs.modal', function () {
                $('#modalLA').remove();
            })

        });

    });
};



/***************************************************************************
    MRP Y OTROS
***************************************************************************/
function MRPGenerar(Parametros, IdGrid) {

    //console.log("BtnPrintOpc: " + Opc + "    Params: " + Parametros + "    IdGrid: " + IdGrid);

    /************************************************************************************************/
    /*Parametro: SelPedLin = Seleccion necesaria de lineas de pedidos seleccionados en la grid      */
    /************************************************************************************************/
    if (Parametros == "SelPedLin") {
        ListSelectPedRowsGuid = [];

        var theGrid = wijmo.Control.getControl("#" + IdGrid);
        if (theGrid !== null) {
            var sel = theGrid.selection,
                ecv = theGrid.collectionView;

            if (sel._row >= 0) {

                for (var i = sel.bottomRow; i >= sel.topRow; i--) {
                    if (theGrid.rows[i].dataItem) {
                        ListSelectPedRowsGuid.push(theGrid.rows[i].dataItem.Guid);
                    }
                }

                let ListSelectPedRowsGuidParam = ListSelectPedRowsGuid.join("**");

                N_DialogSelPedLin(ListSelectPedRowsGuidParam,true, false).then((SelectPedLin) => {
                    var Empresa = "";
                    var ListSelectPedLineas = [];
                    for (var i = 0; i < SelectPedLin.length; i++) {
                        ListSelectPedLineas.push(SelectPedLin[i].Guid);
                        Empresa = SelectPedLin[i].Empresa;
                    }


                    var Spinner = Rats.UI.LoadAnimation.start();
                    var data = new FormData();
                    data.append("ListSelectPedLineas", ListSelectPedLineas);

                    $.ajax({
                        type: 'POST',
                        url: "/Pedidos/MRPGenerar",
                        data: data,
                        processData: false,
                        contentType: false,
                        success: function (res) {
                            Spinner.stop();

                            if (res != null) {

                                N_Grid_Data_Refresh_List(wijmo.Control.getControl("#gridPedidos"), res);

                            }
                            
                        },
                        always: function (data) { },
                        error: function (xhr, status, error) {
                            Spinner.stop();
                            console.log("Error", xhr, xhr.responseText, status, error);
                        }
                    });

                    //N_Grid_Data_Refresh(wijmo.Control.getControl("#gridImplosionArt"), res);
                }).catch((error) => {

                });

            } else {
                DialogInfo("Debe seleccionar uno o varios pedido.");
            }

        }


    }
    /************************************************************************************************/



}

function AlbaranesImprimir(IdGrid) {

    var theGrid = wijmo.Control.getControl("#" + IdGrid);
    if (theGrid !== null) {
        var sel = theGrid.selection,
            ecv = theGrid.collectionView;

        if (sel._row >= 0) {

            var apiURL              = "https://www.roistechbo.es/WebApi/api/report";
            let PoolListados = [];

            var Empresa             = "";
            let ListSelectRows      = [];
            let ListSelectRowsTipos = [];
            let FactTotal           = 0;
            for (var i = sel.topRow; i <= sel.bottomRow; i++) {
                if (theGrid.rows[i].dataItem) {
                    
                    //ListSelectRows.push(theGrid.rows[i].dataItem);
                    
                    let item = theGrid.rows[i].dataItem;

                    Empresa = item.Empresa;

                    //console.log(theGrid.rows[i].dataItem);

                    var KeyAlb                  = encodeURIComponent(item.Albaran);
                    var KeyTipoVenta            = item.AlbTipoVenta;

                    switch (KeyTipoVenta) {
                        case "E":
                            KeyTipoVenta = "Etiqueta";
                            break;
                        case "P":
                            KeyTipoVenta = "Peso";
                            break;
                        case "H":
                            KeyTipoVenta = "Hechura";
                            break;
                        case "PH":
                            KeyTipoVenta = "PesoHechura";
                            break;
                        default:
                            KeyTipoVenta = "Etiqueta";
                    }

                    //var KeyAlbTotal             = item.TotalAlb;
                    //var KeyAlbTotalIva          = item.TotalAlbIVA;
                    //var KeyAlbTotalDtoCial      = item.TotalDtoCial;
                    //var KeyAlbTotalDtoRappel    = item.TotalDtoRappel;


                    var reportPath      = "ReportsRoot/iLabPlus/Albaranes.flxr";
                    var Listado         = "Albaran_" + KeyTipoVenta;
                    var reportName      = Listado;
                    var reportExport    = Listado + "-" + Date.now();
                    var CadParams       = "&parameters.KeyEmp=" + Empresa + "&parameters.KeyAlb=" + KeyAlb;
                    var _tmpURL         = apiURL + "/" + reportPath + "/" + reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;

                    PoolListados.push(_tmpURL);

                }
            }

            ReportPrint_ExportPDF_PoolListados(PoolListados);


            //var apiURL = "https://www.roistechbo.es/WebApi/api/report";
            //let PoolListados = [];
            //if (ListSelectRows.length > 0) {
            //    for (let i = 0; i < ListSelectRows.length; i++) {

            //        var KeyAlb          = ListSelectRows[i].Albaran;
            //        var KeyTipoVenta    = ListSelectRows[i].AlbTipoVenta;
            //        //console.log(KeyAlb + "   " + KeyTipoVenta);

            //        var reportPath      = "ReportsRoot/iLabPlus/Comercial.flxr";
            //        var Listado         = "Albaran_" + KeyTipoVenta;
            //        var reportName      = Listado;
            //        var reportExport    = Listado + "-" + Date.now();
            //        var CadParams       = "&parameters.KeyEmp=" + Empresa + "&parameters.KeyAlb=" + KeyAlb;
            //        var _tmpURL         = apiURL + "/" + reportPath + "/" + reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;
            //        PoolListados.push(_tmpURL);

            //    }

            //    console.log(PoolListados);

            //    //ReportPrint_ExportPDF_PoolListados(PoolListados);

            //} else {

            //}

        }
    }



}

function MRPGenerarImprimir(IdGrid) {

    var theGrid = wijmo.Control.getControl("#" + IdGrid);
    if (theGrid !== null) {
        var sel = theGrid.selection,
            ecv = theGrid.collectionView;

        if (sel._row >= 0) {

            var Empresa = "";
            let ListSelectPedRowsMrp = [];
            for (var i = sel.topRow; i <= sel.bottomRow; i++) {
                if (theGrid.rows[i].dataItem) {
                    ListSelectPedRowsMrp.push(theGrid.rows[i].dataItem.PedMRP);
                    Empresa = theGrid.rows[i].dataItem.Empresa;
                }
            }


            let UniqListSelectPedRowsMrp = [...new Set(ListSelectPedRowsMrp)];  // Elimina valores repetidos
            UniqListSelectPedRowsMrp = UniqListSelectPedRowsMrp.filter(n => n); // Elimina entradas en blanco
            //console.log(UniqListSelectPedRowsMrp);

            
            var apiURL      = "https://www.roistechbo.es/WebApi/api/report";
            let PoolListados = [];
            if (UniqListSelectPedRowsMrp.length > 0) {
                for (let i = 0; i < UniqListSelectPedRowsMrp.length; i++) {
                    var KeyMrp = UniqListSelectPedRowsMrp[i];

                    //Print_MRP(Empresa, KeyMrp, "Mrp_PiedrasFornituras");
                    //Print_MRP(Empresa, KeyMrp, "Mrp_PiedrasFornituras_Etq");

                    var reportPath      = "ReportsRoot/iLabPlus/Mrp.flxr";
                    var Listado         =   "Mrp_PiedrasFornituras";
                    var reportName      =   Listado;
                    var reportExport    =   Listado + "-" + Date.now();
                    var CadParams       =   "&parameters.KeyEmp=" + Empresa +
                                            "&parameters.KeyMrp=" + KeyMrp;
                    var _tmpURL = apiURL + "/" + reportPath + "/" + reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;
                    PoolListados.push(_tmpURL);

                    var reportPath      =   "ReportsRoot/iLabPlus/Mrp.flxr";
                    var Listado         =   "Mrp_PiedrasFornituras_Etq";
                    var reportName      =   Listado;
                    var reportExport    =   Listado + "-" + Date.now();
                    var CadParams       =   "&parameters.KeyEmp=" + Empresa +
                                            "&parameters.KeyMrp=" + KeyMrp;
                    var _tmpURL = apiURL + "/" + reportPath + "/" + reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;
                    PoolListados.push(_tmpURL);

                    var reportPath      =   "ReportsRoot/iLabPlus/Mrp.flxr";
                    var Listado         =   "Mrp_Secciones";
                    var reportName      =   Listado;
                    var reportExport    =   Listado + "-" + Date.now();
                    var CadParams       =   "&parameters.KeyEmp=" + Empresa +
                                            "&parameters.KeyMrp=" + KeyMrp;
                    var _tmpURL = apiURL + "/" + reportPath + "/" + reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;
                    PoolListados.push(_tmpURL);

                    var reportPath      =   "ReportsRoot/iLabPlus/Mrp.flxr";
                    var Listado         =   "Mrp_Sobres";
                    var reportName      =   Listado;
                    var reportExport    =   Listado + "-" + Date.now();
                    var CadParams       =   "&parameters.KeyEmp=" + Empresa +
                                            "&parameters.KeyMrp=" + KeyMrp;
                    var _tmpURL = apiURL + "/" + reportPath + "/" + reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;
                    PoolListados.push(_tmpURL);
                } 

                ReportPrint_ExportPDF_PoolListados(PoolListados);

            } else {
                DialogInfo("Los pedidos seleccionados no tienen MRP generado.");
            }

        }
    }



}

function Print_MRP(Empresa, KeyMrp, Listado) {

    var reportPath = "ReportsRoot/iLabPlus/Mrp.flxr";
    //var reportPath = "ReportsRoot/iLabPlus/" + Listado + ".flxr";
    var reportName = Listado;

    //var reportExport = "PedEtq3x4-" + Date.now();
    var reportExport = Listado + "-" + Date.now();


    var CadParams = "&parameters.KeyEmp="       + Empresa +
                    "&parameters.KeyMrp="       + KeyMrp
        ;

    ReportPrint_ExportPDF("PDF", "https://www.roistechbo.es/WebApi/api/report", reportPath, reportName, reportExport, CadParams);

    //ReportPrint_ToPrinter("PDF", "https://www.roistechbo.es/WebApi/api/report", reportPath, reportName, reportExport, CadParams);

    // NO VA MIRAR COMO SE SACA U PREVIEW
    //var viewName = window.location.pathname.split('/').pop(); // Obtiene el nombre de la vista
    //ReportPrint_PreviewFR("https://www.roistechbo.es/WebApi/api/report",'/' + viewName + '/ReportPreviewfr', reportPath, reportName, "")
}

function PedLinAlbaranCrear(Parametros, IdGrid) {

    /************************************************************************************************/
    /*Parametro: SelPedLin = Seleccion necesaria de lineas de pedidos seleccionados en la grid      */
    /************************************************************************************************/
    if (Parametros == "SelPedLin") {
        ListSelectPedRowsGuid = [];

        var theGrid = wijmo.Control.getControl("#" + IdGrid);
        if (theGrid !== null) {
            var sel = theGrid.selection,
                ecv = theGrid.collectionView;

            if (sel._row >= 0) {

                for (var i = sel.bottomRow; i >= sel.topRow; i--) {
                    if (theGrid.rows[i].dataItem) {
                        ListSelectPedRowsGuid.push(theGrid.rows[i].dataItem.Guid);
                    }
                }

                let ListSelectPedRowsGuidParam = ListSelectPedRowsGuid.join("**");

                N_DialogSelPedLin(ListSelectPedRowsGuidParam, false, true).then((SelectPedLin) => {
                    var Empresa = "";
                    var ListSelectPedLineas = [];
                    for (var i = 0; i < SelectPedLin.length; i++) {
                        ListSelectPedLineas.push(SelectPedLin[i].Guid);
                        Empresa = SelectPedLin[i].Empresa;
                    }


                    var Spinner = Rats.UI.LoadAnimation.start();
                    var data = new FormData();
                    data.append("ListSelectPedLineas", ListSelectPedLineas);

                    $.ajax({
                        type: 'POST',
                        url: "/Albaranes/PedLinAlbaranCrear",
                        data: data,
                        processData: false,
                        contentType: false,
                        success: function (res) {
                            Spinner.stop();

                            if (res != null) {
                                N_Grid_Data_Refresh_List(wijmo.Control.getControl("#gridPedidos"), res);
                            }
                        },
                        always: function (data) { },
                        error: function (xhr, status, error) {
                            Spinner.stop();
                            console.log("Error", xhr, xhr.responseText, status, error);
                        }
                    });

                    //N_Grid_Data_Refresh(wijmo.Control.getControl("#gridImplosionArt"), res);
                }).catch((error) => {

                });

            } else {
                DialogInfo("Debe seleccionar uno o varios pedido.");
            }

        }

    }
    /************************************************************************************************/

}

function PedLinFacturaCrear(Parametros, IdGrid) {

    /************************************************************************************************/
    /*Parametro: SelPedLin = Seleccion necesaria de lineas de pedidos seleccionados en la grid      */
    /************************************************************************************************/
    if (Parametros == "SelPedLin") {
        ListSelectPedRowsGuid = [];

        var theGrid = wijmo.Control.getControl("#" + IdGrid);
        if (theGrid !== null) {
            var sel = theGrid.selection,
                ecv = theGrid.collectionView;

            if (sel._row >= 0) {

                for (var i = sel.bottomRow; i >= sel.topRow; i--) {
                    if (theGrid.rows[i].dataItem) {
                        ListSelectPedRowsGuid.push(theGrid.rows[i].dataItem.Guid);
                    }
                }

                let ListSelectPedRowsGuidParam = ListSelectPedRowsGuid.join("**");

                N_DialogSelPedLin(ListSelectPedRowsGuidParam, false, true).then((SelectPedLin) => {
                    var Empresa = "";
                    var ListSelectPedLineas = [];
                    for (var i = 0; i < SelectPedLin.length; i++) {
                        ListSelectPedLineas.push(SelectPedLin[i].Guid);
                        Empresa = SelectPedLin[i].Empresa;
                    }


                    var Spinner = Rats.UI.LoadAnimation.start();
                    var data = new FormData();
                    data.append("ListSelectPedLineas", ListSelectPedLineas);

                    $.ajax({
                        type: 'POST',
                        url: "/Facturas/PedLinFacturaCrear",
                        data: data,
                        processData: false,
                        contentType: false,
                        success: function (res) {
                            Spinner.stop();

                            if (res != null) {
                                N_Grid_Data_Refresh_List(wijmo.Control.getControl("#gridPedidos"), res);
                            }
                        },
                        always: function (data) { },
                        error: function (xhr, status, error) {
                            Spinner.stop();
                            console.log("Error", xhr, xhr.responseText, status, error);
                        }
                    });

                    //N_Grid_Data_Refresh(wijmo.Control.getControl("#gridImplosionArt"), res);
                }).catch((error) => {

                });

            } else {
                DialogInfo("Debe seleccionar uno o varios pedido.");
            }

        }

    }
    /************************************************************************************************/

}

function DescargarFacturaE(Guid) {

    const url = `/Facturas/DescargarFacturaE?Guid=${Guid}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {

            const XsigBlob      = b64toBlob(data.XsigfileBlob);
            const XsigUrl       = window.URL.createObjectURL(XsigBlob);
            const XsigA         = document.createElement('a');
            XsigA.href          = XsigUrl;
            XsigA.download      = data.XsigfileName; // Establecer el nombre del archivo
            document.body.appendChild(XsigA);
            XsigA.click();
            window.URL.revokeObjectURL(XsigUrl);

            setTimeout(() => {
                XsigA.remove(); // Eliminar el elemento <a> del DOM después de la descarga
            }, 100);

            
        })
        .catch(error => {
            console.error('Error fetch:', error);
        });

}

function b64toBlob(b64Data, contentType = '', sliceSize = 512) {
    const byteCharacters = atob(b64Data);
    const byteArrays = [];
    for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        const slice = byteCharacters.slice(offset, offset + sliceSize);
        const byteNumbers = new Array(slice.length);
        for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }
    const blob = new Blob(byteArrays, { type: contentType });
    return blob;
}


async function N_DialogCalendarioCrearEvento(Guid, infoDataClick, calendarInstance, HannaParameters) {

    return new Promise((resolve, reject) => {

        try {
            // SE ELIMINA DEL DOM: POR SI PREVIAMENTE YA EXISTIERA
            $('#modalFecha').remove();

            fetch(`/Calendario/_ModalEvento`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la solicitud');
                }
                return response.text();
            })
            .then(data => {
                
                $("body").append(data);
      
                $('#modalFecha').on('hidden.bs.modal', function () {
                    $('#modalFecha').remove();
                })

                $('#modalFecha').modal('show');

                if (HannaParameters != null && HannaParameters != undefined) {

                    $('#inputFecha').val(HannaParameters.Fecha);
                    $('#inputHoraInicio').val(HannaParameters.HoraIni);
                    $('#inputHoraFin').val(HannaParameters.HoraFin);
                    $('#inputTitulo').val(HannaParameters.Titulo);
                    $('#inputDetalle').val(HannaParameters.Detalle);
                    $('#inputAlcance').val(HannaParameters.Alcance);

                } else {

                    if (Guid == "NEW") {
                        // EVENTO CREAR
                        $('#inputHoraInicio').val("08:00");
                        $('#inputHoraFin').val("08:30");

                        const fechaOriginal = infoDataClick.dateStr;
                        const fechaFormateada = formatDate(fechaOriginal);
                        $('#inputFecha').val(fechaFormateada);
                                                
                    } else {
                        // EVENTO EDITAR
                        try {

                            fetch(`/Calendario/ObtenerDetallesEvento?guidEvento=${Guid}`)
                                .then(responseD => {
                                    if (!responseD.ok) {
                                        throw new Error('Error en la solicitud');
                                    }
                                    return responseD.text();
                                })
                                .then(data => {
                                    let datajSON = JSON.parse(data);

                                    $('#inputTitulo').val(datajSON.Titulo);
                                    $('#inputFecha').val(datajSON.Fecha);
                                    $('#inputDetalle').val(datajSON.Detalle);
                                    $('#inputHoraInicio').val(datajSON.HoraInicio);
                                    $('#inputHoraFin').val(datajSON.HoraFin);
                                    $('#inputGuid').val(Guid);
                                    $('#inputAlcance').val(datajSON.Alcance);
                                    $('#btnGuardarEvento').text('Editar'); // Para mostrar EDITAR
                                    $('#btnEliminarEvento').show(); // Para mostrar ELIMINAR

                                    // Parte para obtener el tipo de usuario: ADMIN o USUARIO
                                    const usuarioTipo = fetchTipoUsuario();
                                    if (usuarioTipo === "ADMIN") {
                                        $('#inputAlcance').show(); // Mostramos el select para los ADMIN
                                    } else {
                                        $('#inputAlcance').hide(); // Lo ocultamos para otros tipos de usuario
                                    }
                                })
                                .catch(errorD => {
                                    throw new Error('No se pudo cargar el contenido ObtenerDetallesEvento.');
                                    reject(errorD);
                                });



                        } catch (error) {
                            DialogInfo("Ha habido un error al buscar los detallles.");
                        }
                    }
                }

                setTimeout(function () {
                    $('#inputTitulo').focus();
                }, 500);

                $('#btnCerrarEvento').off('click').on('click', function () {
                    resolve(false);
                });


                // $('#btnGuardarEvento').click(async function () {    // ESTO FUNCIONABA ASI TAL CUAL!
                //$(document).on('click', '#btnGuardarEvento', async function () {
                $('#btnGuardarEvento').off('click').on('click', async function (evt) {

                    const guidEvento = Guid;
                    const alcance = $('#inputAlcance').val();
                    
                    const titulo = $('#inputTitulo').val();
                    if (!titulo) {
                        DialogInfo('El título del evento no puede estar vacío.');
                        return;
                    }

                    const fecha = $('#inputFecha').val();
                    if (!fecha) {
                        DialogInfo('La fecha del evento no puede estar vacía.');
                        return;
                    }

                    const horaInicio = $('#inputHoraInicio').val(); // AGREGADO AHORA

                    // Nuevo código para convertir la fecha
                    const [anio, mes, dia] = fecha.split("-");
                    const fechaSeleccionada = new Date(anio, mes - 1, dia);
                    fechaSeleccionada.setHours(0, 0, 0, 0);

       
                    const fechaHoy = new Date();
                    fechaHoy.setHours(0, 0, 0, 0);
                    fechaHoy.setFullYear(anio, mes - 1, dia);

                    if (fechaSeleccionada.getFullYear() === fechaHoy.getFullYear() &&
                        fechaSeleccionada.getMonth() === fechaHoy.getMonth() &&
                        fechaSeleccionada.getDate() === fechaHoy.getDate()) {

                        const ahora = new Date();
                        const horaInicio = $('#inputHoraInicio').val().split(":");
                        const horaEvento = new Date(fechaSeleccionada);
                        horaEvento.setHours(horaInicio[0], horaInicio[1]);

                        if (ahora > horaEvento) {
                            const confirmar = window.confirm("Estás creando un evento que ya debería haber empezado. ¿Querés hacerlo de todos modos?");
                            if (!confirmar) {
                                DialogInfo("Optaste por cancelar la creación del evento.");
                                return;
                            }
                        }
                    } else {
                        //console.log("No entró al bloque de la misma fecha");
                    }

                    // const horaInicio = $('#inputHoraInicio').val();
                    const horaFin = $('#inputHoraFin').val();
                    if (horaInicio >= horaFin) {
                        DialogInfo("La hora de fin debe ser posterior a la hora de inicio.");
                        return;
                    }

                    if (!alcance) {
                        DialogInfo("Debes seleccionar un alcance para el evento.");
                        return;
                    }

                    let usuariosSeleccionados = $('#inputUsuarios').val();
                    if ((alcance === "Varios usuarios" || alcance === "Observador") && !usuariosSeleccionados.length) {
                        DialogInfo("Debes seleccionar al menos un usuario.");
                        return;
                    }

                    let evento = {
                        Guid: guidEvento,
                        Titulo: $('#inputTitulo').val(),
                        Fecha: $('#inputFecha').val(),
                        Detalle: $('#inputDetalle').val(),
                        HoraInicio: $('#inputHoraInicio').val(),
                        HoraFin: $('#inputHoraFin').val(),
                        Alcance: alcance,
                        RecibirMail: $('#inputRecibirRecordatorios').is(':checked')
                    };

                    if (alcance === "Varios usuarios" || alcance === "Observador") {
                        evento.Usuarios = $('#inputUsuarios').val();
                    }

                                        

                    try {
                        let response;

                        if (guidEvento != "NEW") {

                            fetch('/Calendario/ActualizarEvento',
                                {
                                    headers: {
                                        'Content-Type': 'application/json'
                                    },
                                    method: "POST",
                                    body: JSON.stringify(evento)
                                })
                                .then(function (resAE) {
                                    $('#modalFecha').modal('hide');
                                    if (calendarInstance != null) {
                                        calendarInstance.refetchEvents();
                                    }

                                    if (resAE.ok) {
                                        resolve(resAE.json());
                                    } else if (resAE.status === 400) {
                                        DialogInfo("Error en el proceso.");
                                        resAE.text().then(function (errorMessage) {
                                            console.log(errorMessage);
                                            resolve(errorMessage);
                                        });

                                    } else { // Otros errores del servidor
                                        DialogInfo("Error en el proceso.");
                                        resAE.text().then(function (errorMessage) {
                                            console.log(errorMessage);
                                            resolve(errorMessage);
                                        });
                                    }
                                })
                                .catch(function (resAE) {
                                    DialogInfo("Algo salió mal al intentar actualizar.");
                                })



                        } else {

                            fetch('/Calendario/AgregarEvento',
                                {
                                    headers: {     
                                        'Content-Type': 'application/json'
                                    },
                                    method: "POST",
                                    body: JSON.stringify(evento)
                                })
                                .then(function (resAE) {
                                    $('#modalFecha').modal('hide');

                                    if (calendarInstance != null) {
                                        calendarInstance.refetchEvents();    
                                    }


                                    if (resAE.ok) { 
                                        resolve(resAE.json());
                                    } else if (resAE.status === 400) {
                                        DialogInfo("Error en el proceso.");
                                        resAE.text().then(function (errorMessage) {
                                            console.log(errorMessage);
                                            resolve(errorMessage);
                                        });

                                    } else { // Otros errores del servidor
                                        DialogInfo("Error en el proceso.");
                                        resAE.text().then(function (errorMessage) {
                                            console.log(errorMessage);
                                            resolve(errorMessage);
                                        });
                                    }
                                })
                                .catch(function (resAEC) {
                                    DialogInfo("Algo salió mal al intentar actualizar.");
                                    
                                })

                        }

                        

                    } catch (error) {
                        console.error("Error al procesar la respuesta: ", error);
                        DialogInfo("Hubo un error, vuelve a intentarlo");
                    }


                });

                $('#btnEliminarEvento').off('click').on('click', async function (evt) {
                    const guidEvento = {guid: Guid};  // Obtengo el Guid del campo oculto
                    try {
                        const response = await fetch('/Calendario/EliminarEvento', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify(guidEvento)
                        });

                        if (response.ok) {
                            $('#modalFecha').modal('hide');
                            calendarInstance.refetchEvents();  // <-- Añado esta línea para actualizar los eventos
                        } else {
                            DialogInfo("Algo salió mal al intentar actualizar.");
                        }
                    } catch (error) {
                        DialogInfo("Hubo un error en la actualización: " + error);
                    }
                });




                async function fetchTipoUsuario() {
                    // Acá hago el fetch al servidor para obtner el tipo de usuario
                    const response = await fetch('/Usuarios/ObtenerTipoUsuario');
                    if (response.ok) {
                        const data = await response.json();
                        return data.UsuarioTipo
                    } else {
                        return null;
                    }
                }

                async function cargarUsuarios() {
               
                    try {
                        const response = await fetch(`/Calendario/ObtenerUsuarios`);
                        if (response.ok) {
                            const listaCombinada = await response.json();
                            let opciones = listaCombinada
                                .filter(item => {
                                    // Excluye el usuario actual solo si el alcance es "Observador"
                                    if ($('#inputAlcance').val() === 'Observador' || $('#inputAlcance').val() === 'Varios usuarios') {
                                        return item.NombreCompleto !== SessionUsuarioNombre;
                                    }
                                    return true;
                                })
                                .map(item => `<option value="${item.Guid}">${item.NombreCompleto}</option>`)
                                .join('');
                            $('#inputUsuarios').html(opciones);
                        } else {
                            DialogInfo("No se pudieron cargar los usuarios y empleados");
                        }
                    } catch (error) {
                        DialogInfo("Algo salió mal al cargar los usuarios y empleados!");
                    }


                }


                $(document).on('change', '#inputAlcance', function () {
                    const valorSeleccionado = $(this).val();
                    if (valorSeleccionado === "Varios usuarios" || valorSeleccionado === "Observador") {
                        cargarUsuarios();
                        $('#inputUsuarios').show();
                        $('#inputUsuarios').attr('multiple', 'multiple');
                    } else {
                        $('#inputUsuarios').hide();
                    }
                });
                

            })
            .catch(error => {
                    
            throw new Error('No se pudo cargar el contenido del modal.');
            reject(error);
            });



        } catch (error) {

            console.error("Error al cargar el modal: ", error);

        }

    });



}

function formatDate(originalDate) {
    // Primero, verificamos si la fecha contiene información de hora buscando 'T'
    if (originalDate.includes('T')) {
        // Si la tiene, la dividimos para obtener solo la parte de la fecha
        originalDate = originalDate.split('T')[0];
    }
    // El formato original de la fecha es 'YYYY-MM-DD', así que lo dividimos por '-'
    const parts = originalDate.split('-');
    // Reorganizamos las partes a 'DD-MM-YYYY'
    const formattedDate = `${parts[2]}-${parts[1]}-${parts[0]}`;
    return formattedDate;
}

function formatDate(originalDate) {
    // Primero, verificamos si la fecha contiene información de hora buscando 'T'
    if (originalDate.includes('T')) {
        // Si la tiene, la dividimos para obtener solo la parte de la fecha
        originalDate = originalDate.split('T')[0];
    }
    // El formato original de la fecha es 'YYYY-MM-DD', así que lo dividimos por '-'
    const parts = originalDate.split('-');
    // Reorganizamos las partes a 'DD-MM-YYYY'
    const formattedDate = `${parts[2]}-${parts[1]}-${parts[0]}`;
    return formattedDate;
}


///////////////////////////////////////////////////// Script de CONTACTOS //////////////////////////////////////////////////////////////////////////////////////////


function N_DialogContacto(guid = null) {
    return new Promise((resolve, reject) => {
        $('#modalContacto').remove(); // Elimina el diálogo anterior del DOM, si existe

        let url = '/Contactos/DialogContacto';
        if (guid) {
            url += `?id=${guid}`; // Ajusta la URL para cargar los datos del contacto si se está editando
        }

        fetch(url) // Carga la vista parcial para un nuevo contacto o uno existente
            .then(response => response.text())
            .then(html => {

                $('body').append(html);

                $('#modalContacto').modal('show');

                $('#modalContacto-ok').off('click').on('click', async () => { // Configura el evento click del botón de guardado

                    let formData = new FormData(document.getElementById('FormContactosCreateEdit'));

                    if (guid) {
                        formData.append('Guid', guid); // Incluye el GUID en la petición si estás editando
                    }

                    try {
                        const response = await fetch('/Contactos/CrearEditarContacto', {
                            method: 'POST',
                            body: formData
                        });

                        if (!response.ok) {
                            throw new Error('Error en la respuesta del servidor');
                        }

                        const data = await response.json();
                        console.log(data);

                        if (data.success) {
                            $('#modalContacto').modal('hide');
                            // Si la acción fue crear un nuevo contacto, lo añadimos al listado directamente
                            if (data.contacto) {
                                agregarNuevoContacto(data.contacto); // Añade el contacto al listado
                            } else if (data.contactos) {
                                // Cuando es una actualización de contactos
                                actualizarListadoContactos(data.contactos);
                            }
                           // actualizarListadoContactos(data.contactos); // Añade o actualiza el contacto en el listado
                            } else {
                                alert(data.message);
                            }
                    } catch (error) {
                        console.error('Error:', error);
                        alert('Error al crear/editar el contacto: ' + error.message);
                    }
                });
            })
            .catch(error => {
                console.error('Error al cargar el diálogo de contacto:', error);
                reject(error);
            });
    });
}



function actualizarListadoContactos(contactos) {
    const lista = $('.list-group');
    lista.empty(); // Limpiamos la lista actual

    contactos.forEach(contacto => {
        // Aquí construyes el HTML para cada contacto y lo agregas a la lista
        const contactoHTML = `
            <li class="list-group-item list-group-item-border" data-letra="${contacto.Nombre[0].toUpperCase()}" data-contactoNombre="${contacto.Nombre}">
                <a href="#" onclick="mostrarDetalle('${contacto.Guid}')" style="display: block; text-decoration: none; color: inherit;">
                    <div class="contact-name">${contacto.Nombre}</div>
                    <div class="contact-details">
                        <span class="contact-info"><small>${contacto.TelefonoMovilPersonal}</small></span>
                        <br>
                        <span class="contact-info"><small>${contacto.Email}</small></span>
                    </div>
                </a>
            </li>
        `;
        lista.append(contactoHTML); // Agregar el HTML del contacto a la lista
    });
}



function agregarNuevoContacto(contacto) {
    // Crear el elemento HTML para el nuevo contacto
    const nuevoContactoHtml = $(`
        <li class="list-group-item" style="background-color: #F3F3F4; margin-bottom: 5px; border-left: 4px solid #1B6EC2;">
            <a href="#" onclick="mostrarDetalle('${contacto.Guid}')" style="display: block; text-decoration: none; color: inherit;">
                <div class="contact-name">${contacto.Nombre}</div>
                <div class="contact-details">
                    <span class="contact-info"><small>${contacto.TelefonoMovilPersonal ? contacto.TelefonoMovilPersonal : ''}</small></span>
                    <br>
                    <span class="contact-info"><small>${contacto.Email}</small></span>
                </div>
            </a>
        </li>
    `);

    // Encontrar el lugar correcto para insertar el nuevo contacto
    let insertado = false;
    $('.list-group-item').each(function () {
        const nombreActual = $(this).find('.contact-name').text().toLowerCase();
        if (nombreActual > contacto.Nombre.toLowerCase()) {
            $(this).before(nuevoContactoHtml);
            insertado = true;
            return false; // Salir del bucle each
        }
    });

    // Si no se insertó en el bucle, agregar al final
    if (!insertado) {
        $('.list-group').append(nuevoContactoHtml);
    }
}


function mostrarTodosLosContactos() {
    // Desactivar todos los botones del abecedario
    document.querySelectorAll('.btn-abecedario').forEach(boton => boton.classList.remove('active'));
    // Mostrar todos los contactos
    const elementosConDataLetra = document.querySelectorAll('[data-letra]');
    elementosConDataLetra.forEach(elemento => elemento.style.display = 'block');
}

function filtrarPorLetra(letraSeleccionada) {

    const inputBuscar = document.getElementById('buscar');
    inputBuscar.value = "";

    // Obtener todos los botones del abecedario
    const todosLosBotones = document.querySelectorAll('.btn-abecedario');
    // Obtener el botón específico de la letra seleccionada
    const botonActual = Array.from(todosLosBotones).find(boton => boton.textContent === letraSeleccionada);
    // Analizar si el objeto Button de letraSeleccionada está en azul, desactivarlo y mostrarTodosLosContactos()
    // sino ejecutar el else

    // Verificar si el botón ya está activo
    if (botonActual.classList.contains('active')) {
        // Si el botón ya está activo, desactivarlo y mostrar todos los contactos
        botonActual.classList.remove('active');
        mostrarTodosLosContactos();
    } else {
        // Activar el botón actual y desactivar todos los demás
        todosLosBotones.forEach(boton => boton.classList.remove('active'));
        botonActual.classList.add('active');

        // Mostrar solo los contactos que coinciden con la letra seleccionada
        const elementosConDataLetra = document.querySelectorAll('[data-letra]');
        elementosConDataLetra.forEach(elemento => {
            if (elemento.getAttribute('data-letra') === letraSeleccionada) {
                elemento.style.display = 'block';
            } else {
                elemento.style.display = 'none';
            }
        });
    }
}



function buscarContactos() {
    // Obtiene el valor actual del campo de búsqueda
    const busqueda = document.getElementById('buscar').value;

    // Seleccionar todos los elementos que tienen el atributo data-letra
    // (Si estás filtrando por nombres en 'buscarContactos', quizás quieras seleccionar por un atributo más específico o relevante)
    var elementosConDataLetra = document.querySelectorAll('[data-contactoNombre]');

    // Mostrar todos los elementos antes de aplicar el nuevo filtro
    // (Opcional, dependiendo de cómo quieras que funcione tu UI)
    elementosConDataLetra.forEach(function (elemento) {
        elemento.style.display = 'block';
    });

    // Filtrar elementos basados en la búsqueda
    elementosConDataLetra.forEach(function (elemento) {
        // Obtener el nombre del contacto del atributo data-contactoNombre y convertir a minúsculas para la comparación
        var nombreContacto = elemento.getAttribute('data-contactoNombre').toLowerCase();
        var busquedaMinusc = busqueda.toLowerCase();

        // Si el nombre del contacto no incluye la cadena de búsqueda, ocultar el elemento
        if (!nombreContacto.includes(busquedaMinusc)) {
            elemento.style.display = 'none';
        }
    });
}
