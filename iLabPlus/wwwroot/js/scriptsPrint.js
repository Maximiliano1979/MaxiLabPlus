

/***************************************************************************
    GESTION DE IMPRESIONES
***************************************************************************/
function BtnPrintOpc(Listado, Parametros, IdGrid) {

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

                N_DialogSelPedLin(ListSelectPedRowsGuidParam,false, false).then((SelectPedLin) => {
                    var Empresa = "";
                    var ListSelectPedLineas = [];
                    for (var i = 0; i < SelectPedLin.length; i++) {
                        ListSelectPedLineas.push(SelectPedLin[i].Guid);
                        Empresa = SelectPedLin[i].Empresa;
                    }
                    Print_PedEtiquetas3x4(Empresa, ListSelectPedLineas, Listado);

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

function Print_PedEtiquetas3x4(Empresa, SelectPedLin, Listado) {
    
    var reportPath      = "ReportsRoot/iLabPlus/Pedidos.flxr";
    //var reportName      = "PedEtq3x4";
    var reportName      = Listado;

    //var reportExport = "PedEtq3x4-" + Date.now();
    var reportExport = Listado + "-" + Date.now();

    
    //var reportParams = [];
    //var params = {};
    //params["KeyEmp"]    = Empresa;
    ////params["KeyCli"]    = _Cliente;
    ////params["KeyPed"]    = _PedidoWeb;
    ////params["KeyPedCab"] = _Pedido;

    ////var CadFiltro = "8f0f2319-74ca-44f9-b9e8-08dafe13aea0,553ab89a-84cb-4f57-b705-08dafe13aea0,abd48a80-183b-46cb-b60a-08dafe13aea0,2b61fb4b-875a-498e-b605-08dafe13aea0,635e0a41-2dd3-4fff-b52e-08dafe13aea0,576ef08f-d325-4c00-b3a0-08dafe13aea0,4cac818e-3f4d-400c-af39-08dafe13aea0,79c35662-820a-427e-ae5c-08dafe13aea0,79c35662-820a-427e-ae5c-08dafe13aea0,xxxxxx,";
    ////CadFiltro = CadFiltro + "55979354-94d1-41e4-ae58-08dafe13aea0,c011bbd4-dd6f-4bb8-ae57-08dafe13aea0,a2c4388b-aa72-4cbb-ae56-08dafe13aea0,e0d28ea5-0d47-4e74-ae55-08dafe13aea0,fd3da197-42af-43d8-ae54-08dafe13aea0,";
    ////CadFiltro = CadFiltro + "5dff9725-0485-4485-ae53-08dafe13aea0,6430ec0c-77c8-47f7-ae52-08dafe13aea0,212c9395-2775-459d-ae51-08dafe13aea0,03b84704-da5b-4bf8-ae50-08dafe13aea0,ae7d4a45-42b6-4ce2-ae4f-08dafe13aea0,";
    ////CadFiltro = CadFiltro + "2409dafc-ad40-4391-ae4e-08dafe13aea0,";

    //var CadFiltro = SelectPedLin.join(',');
    //params["CadFiltroGuid"] = CadFiltro;
    //reportParams.push(params);
    //ReportPrintCommon("PDF", "https://www.roistechbo.es/WebApi/api/report", reportPath, reportName, reportExport, reportParams);


    var CadFiltro = SelectPedLin.join(',');

    CadParams = "&parameters.KeyEmp="           + Empresa       +
                "&parameters.CadFiltroGuid="    + CadFiltro
                ;


    ReportPrint_ExportPDF("PDF", "https://www.roistechbo.es/WebApi/api/report", reportPath, reportName, reportExport, CadParams);

    //ReportPrint_ToPrinter("PDF", "https://www.roistechbo.es/WebApi/api/report", reportPath, reportName, reportExport, CadParams);

    // NO VA MIRAR COMO SE SACA U PREVIEW
    //var viewName = window.location.pathname.split('/').pop(); // Obtiene el nombre de la vista
    //ReportPrint_PreviewFR("https://www.roistechbo.es/WebApi/api/report",'/' + viewName + '/ReportPreviewfr', reportPath, reportName, "")
}

function ReportPrint_ToPrinter(_tipoReport, apiURL, _prlPath, _reportName, reportExport, CadParams) {

    var Url = apiURL + "/" + _prlPath + "/" + _reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;

    var data = new FormData();
    data.append("Url",      Url);
    data.append("FileName", reportExport + ".pdf");

    var Spinner = Rats.UI.LoadAnimation.start();

    // LLAMA A FUNCION Print_Document Y LO GUARDA EN DIRECTORIO O....
    $.ajax({
        type: 'POST',
        url: "/PrintFuntions/Print_Document",
        data: data,
        processData: false,
        contentType: false,
        success: function (data) {
            Spinner.stop();

        },
        always: function (data) { },
        error: function (xhr, status, error) {
            Spinner.stop();
            console.log("Error", xhr, xhr.responseText, status, error);
        }
    });

    // CON ESTE TROZO LO QUE HACES ES DESCARGARLO COMO FICHERO
    //$.ajax({
    //    method: 'POST',
    //    url: "/PrintFuntions/Print_Document",        
    //    data: data,
    //    processData: false,
    //    contentType: false,
    //    xhrFields: {
    //        responseType: 'blob'
    //    },
    //    success: function (data) {
    //        var url = window.URL.createObjectURL(data);
    //        var link = document.createElement('a');
    //        link.href = url;
    //        link.download = 'xxxxxxxxx.pdf';
    //        link.click();
    //        Spinner.stop();
    //    },
    //    error: function () {
    //        Spinner.stop();
    //        console.log('Error al descargar el archivo PDF.');
    //    }
    //});




}

function ReportPrint_ExportPDF(_tipoReport, apiURL, _prlPath, _reportName, reportExport, CadParams) {

    var iLabSpinner = Rats.UI.LoadAnimation.start();
    setTimeout(function () {

        _tmpURL = apiURL + "/" + _prlPath + "/" + _reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport + CadParams;

        //console.log(_tmpURL);

        //alert(_reportName);
        // MUESTRA DIALOGO DE IMPRESION
        /*printJS(_tmpURL, 'image');*/
        //printJS(_tmpURL);

        printJS({
            printable: _tmpURL,
            onPrintDialogClose: () => {
                //alert("onPrintDialogClose");
            },
            onError: () => {
                alert("onError");
                // do your thing..
            }
        });


        // LO DESCARGA como fichero
        //window.open(_tmpURL, "_self");

        // LO DESCARGA EN OTRA VENTANA DELNAVEGADOR
        //window.open(_tmpURL, "_blank");

        iLabSpinner.stop();
    }, 50); // delay of 50 for spinner....

}

function ReportPrint_ExportPDF_PoolListados(PoolListados) {

    
    var iLabSpinner = Rats.UI.LoadAnimation.start();

    setTimeout(function () {

        //console.log("bucle PoolListados: ", PoolListados)

        //PoolListados.forEach(function (item) {
        //    console.log(item);
        //});

        var contador = 0;

        

        function imprimirPDF(_tmpURL) {

            printJS({
                printable: _tmpURL,
                onPrintDialogClose: () => {
                    iLabSpinner.stop();
                    contador++;
                    if (contador < PoolListados.length) {
                        imprimirPDF(PoolListados[contador]);
                    }
                },
                onError: (error) => {
                    alert("onError printJS: " + error);
                }
            });

        }

        // Iniciamos el primero y recursividad...
        imprimirPDF(PoolListados[contador]);


        iLabSpinner.stop();
    }, 50); // delay of 50 for spinner....

}

function ReportPrintCommon(_tipoReport, apiURL, _prlPath, _reportName, reportExport, _reportParams) {

    var iLabSpinner = Rats.UI.LoadAnimation.start();
    setTimeout(function () {


        _tmpURL = apiURL + "/" + _prlPath + "/" + _reportName + "/" + "$report/export?exportOptions.format=pdf&exportFileName=" + reportExport +
            "&parameters.KeyEmp=" + _reportParams[0].KeyEmp +
            "&parameters.CadFiltroGuid=" + _reportParams[0].CadFiltroGuid
            ;

        console.log(_tmpURL);

        window.open(_tmpURL, "_self");

        iLabSpinner.stop();
    }, 50); // delay of 50 for spinner....
}

function ReportPrint_PreviewFR(_URL, apiURL, _reportPath, _reportName, _reportParams) {

    console.log(apiURL);
    console.log(_URL);
    alert(_URL);

    var _Data = {
        'keyPar': JSON.stringify(_reportParams),
        reportURL: apiUrl,
        reportPath: _reportPath,
        reportName: _reportName
    };

    $.ajax({
        url: _URL,
        type: 'POST',
        data: _Data,
        datatype: 'json',
        success: function (data) {
            var newWin = open(_URL, '_blank', 'left = 300, top = 100, width = 1100, height = auto, toolbar = 0, resizable = 1, titlebar = 0, menubar=no, toolbar=no, location=no, directories=no, status=no');
            newWin.document.write(data);
        }
    });
}







