

/***************************************************************************
    FUNCIONESDE AUDIO
***************************************************************************/
function SintetizarMsgToVoz(Mensaje) {

    if ('speechSynthesis' in window) {

        var synthesis = window.speechSynthesis;

        var Audio = new SpeechSynthesisUtterance(Mensaje);

        synthesis.speak(Audio);

    } else {
        lert('Tu navegador no es compatible con la síntesis de voz.');
    }

}








