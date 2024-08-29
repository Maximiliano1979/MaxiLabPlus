
let N_ModalImport = function N_ModalImportFunc() {

    return new Promise(async (resolve, reject) => {
        try {
            // Eliminamos del DOM el modal anterior por si ya existiera
            $('#modalImportClientes').remove();
            // Cargamos la vista parcial desde el controlador ImportExport
            let response = await fetch("/ImportExport/ImportGetPartial");
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            let data = await response.text();
            //console.log(data);
            // Agrego el HTML al body de la página
            $("body").append(data);

            // Muestro el modal
            $('#modalImportClientes').modal('show');

            //$('#modalImportClientes').on('hidden.bs.modal', function () {
            //    $('#modalImportClientes').remove();
            //}) 

        } catch (error) {
            // Manejo de errores al cargar la vista parcial
            console.error('Error al cargar la vista parcial:', error);
            reject(error);
        }
    });
};

