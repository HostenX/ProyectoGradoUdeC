mergeInto(LibraryManager.library, {
    SpeakText: function(text, rate, pitch, volume, lang) {
        var textString = UTF8ToString(text);
        var langString = UTF8ToString(lang);
        
        if ('speechSynthesis' in window) {
            // Detener cualquier síntesis actual
            window.speechSynthesis.cancel();
            
            // Crear un nuevo objeto de enunciado
            var utterance = new SpeechSynthesisUtterance(textString);
            
            // Configurar las opciones de voz
            utterance.rate = rate;
            utterance.pitch = pitch;
            utterance.volume = volume;
            utterance.lang = langString;
            
            // Seleccionar una voz en español si está disponible
            var voices = window.speechSynthesis.getVoices();
            for (var i = 0; i < voices.length; i++) {
                if (voices[i].lang.indexOf(langString.split('-')[0]) !== -1) {
                    utterance.voice = voices[i];
                    break;
                }
            }
            
            // Iniciar la síntesis
            window.speechSynthesis.speak(utterance);
        } else {
            console.log("Web Speech API no está soportada en este navegador");
        }
    },
    
    StopSpeaking: function() {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
        }
    },
    
    IsSpeaking: function() {
        if ('speechSynthesis' in window) {
            return window.speechSynthesis.speaking;
        }
        return false;
    }
});