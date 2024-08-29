



function HannaProcesarPeticion(Texto) {

    var _data = new FormData();
    _data.append("Prompt", Texto);

    $.ajax({
        type: 'POST',
        url: '/HannaIA/HannaGenerateAnswer',
        async: true,
        data: _data,
        processData: false,
        contentType: false,
        success: function (res) {

            //Spinner.stop();
            if (res != null) {
                //console.clear();
                //console.log(res);
                $(".HannaIcon").removeClass("HannaIconListening");


                if (res != null) {
                    var jsonObject = JSON.parse(res);
                    //console.log(jsonObject);
                    var Intent = jsonObject.Intent;
        

                    if (Intent == "CrearEvento") {

                        N_DialogCalendarioCrearEvento("NEW", null, null, jsonObject).then((res) => {
                            var currentUrl = window.location.href;
                            if (currentUrl.includes("Calendario")) {
                                location.reload();
                            }
                        });   
                    }

                    if (Intent == "NoClasificado") {

                        toastId = toastr.success(jsonObject.MensajeTexto, '', { positionClass: 'toast-top-right-Nemesis', "timeOut": "5000", });

                    }


                }

                //if (res.Type == "Message") {
                //    toastr.success(res.Answer, '', { positionClass: 'toast-top-right-Nemesis' });
                //} else {
                //    toastr.warning(res.Answer, '', { positionClass: 'toast-top-right-Nemesis' });
                //}


                $('#TextoTest1').val("");
            } else {

            }

            //toastr.success('PROCESO COMPLETADO');
        },
        always: function (res) { },
        error: function (res) {
            console.log(res);
            //Spinner.stop();
        }
    });
}


/******************************************************************************/
/* FUNCIONES Y PROCESOS SPEECH RECOGNITION  */
/******************************************************************************/
//var ActivarHannaIA = true;

//if (ActivarHannaIA == true) {

//    $(".HannaIcon").addClass("HannaIconActive");
//}

var speechRecognition;

function HannaDesconectar() {
    $(".HannaIcon").removeClass("HannaIconActive");

    // Detener el reconocimiento de voz si está en progreso
    if (speechRecognition && speechRecognition.abort) {
        speechRecognition.stop();
        speechRecognition.abort();
    }

    // Eliminar todas las referencias al objeto de reconocimiento de voz
    speechRecognition = null;
}


function HannaConectar() {

    $(".HannaIcon").addClass("HannaIconActive");


    if ($(".HannaIcon").hasClass("HannaIconActive")) {

        //if ("webkitSpeechRecognition" in window) {


        if ("SpeechRecognition" in window || "webkitSpeechRecognition" in window) {

            //speechRecognition = new webkitSpeechRecognition();

            const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            const speechRecognition = new SpeechRecognition();

            // String for the Final Transcript
            let final_transcript = "";

            // Set the properties for the Speech Recognition object
            speechRecognition.continuous        = true;
            speechRecognition.interimResults    = true;
            speechRecognition.lang              = "es-ES";


            // Callback Function for the onStart Event
            speechRecognition.onstart = () => {
                // Show the Status Element
                if ($('#status').length) {
                    document.querySelector("#status").style.display = "block";
                    /*console.log("speechRecognition.start START");*/
                }

            };
            speechRecognition.onerror = () => {
                // Hide the Status Element
                if ($('#status').length) {
                    //alert("speechRecognition.onerror ERRORRRRRRRRR");
                    document.querySelector("#status").style.display = "none";
                    console.log("speechRecognition.onerror ERRORRRRRRRRR");
                }

            };
            speechRecognition.onend = () => {
                // Hide the Status Element
                if ($('#status').length) {
                    document.querySelector("#status").style.display = "none";
                    console.log("speechRecognition.onend onend");
                    
                }

                console.log("speechRecognition.onend onend");

                //if (speechRecognition) {
                //    console.log("LLAMAMOS A speechRecognition.start");
                //    speechRecognition.start(); // En local siempre pedira permisos, en HTTPS no los pide
                //}


            };

            var toastId;

            speechRecognition.onresult = (event) => {

                let interim_transcript = "";

                for (let i = event.resultIndex; i < event.results.length; ++i) {

                    if (event.results[i].isFinal) {
                        final_transcript = event.results[i][0].transcript;                  
                        if ($('#status').length) {
                            $(".HannaIcon").removeClass("HannaIconListening");
                            $('#TextoTest1').val(final_transcript);
                        }
                        
                        
                        setTimeout(function () {
                            toastr.clear(toastId);
                        }, 1000); 
                        toastId = undefined;

                        HannaProcesarPeticion(final_transcript);
                    } else {
                        interim_transcript += event.results[i][0].transcript;
                        $(".HannaIcon").addClass("HannaIconListening");
                    }
                }

                if ($('#interimW').length) {
                    document.querySelector("#interimW").innerHTML = interim_transcript;
                    $(".HannaIcon").addClass("HannaIconListening");
                }

                //console.log(toastId);
                if (interim_transcript != "") {
                    if (!toastId) {
                        // Si no hay un ID de notificación, significa que es la primera vez que se muestra
                        toastId = toastr.info(interim_transcript, '', { positionClass: 'toast-top-right-Nemesis', "timeOut": "0", });
                    } else {
                        if (toastId.length && toastId != null) {
                            // Si la notificación existe, actualizar su contenido
                            if (interim_transcript != "") {
                                toastId.find('.toast-message').text(interim_transcript); // Actualizar el texto del mensaje
                            }
                        }
                    }
                }




            };

            speechRecognition.start();

        } else {
            console.log("Speech Recognition Not Available");
            //alert("Speech Recognition Not Available");
        }

    }

}


