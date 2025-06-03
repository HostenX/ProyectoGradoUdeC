using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class SpeechReader : MonoBehaviour
{
    [SerializeField] private GetVariables getVariables;

    [Header("Text-to-Speech Settings")]
    public bool useTTS = true;
    [Range(0.1f, 2.0f)]
    public float speechRate = 1.0f;
    [Range(0.1f, 2.0f)]
    public float speechPitch = 1.0f;
    [Range(0.0f, 1.0f)]
    public float speechVolume = 1.0f;
    public string speechLanguage = "en-US";

    private bool isSpeaking = false;

    // Importar funciones de JavaScript para WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SpeakText(string text, float rate, float pitch, float volume, string lang);
    
    [DllImport("__Internal")]
    private static extern void StopSpeaking();
    
    [DllImport("__Internal")]
    private static extern bool IsSpeaking();
#endif

    void Start()
    {
        // Si no se asignó el componente GetVariables en el inspector, intentar encontrarlo
        if (getVariables == null)
        {
            getVariables = FindObjectOfType<GetVariables>();
            if (getVariables == null)
            {
                Debug.LogError("No se encontró el componente GetVariables");
                return;
            }
        }

        // Suscribirse al evento cuando se completen los datos del minijuego
        StartCoroutine(WaitForMinijuegoData());
    }

    void Update()
    {
        // Actualizar estado de habla desde JavaScript (solo en WebGL)
#if UNITY_WEBGL && !UNITY_EDITOR
        isSpeaking = IsSpeaking();
#endif

        // Añadir control opcional por tecla para repetir la lectura
        if (Input.GetKeyDown(KeyCode.R) && !isSpeaking)
        {
            ReadMinijuegoData();
        }
    }

    IEnumerator WaitForMinijuegoData()
    {
        // Esperar hasta que el minijuegoId sea diferente de 0 (valor por defecto)
        yield return new WaitUntil(() => getVariables.minijuegoId != 0);

        // Esperar un poco para asegurarse de que todos los datos estén cargados
        yield return new WaitForSeconds(1.0f);

        // Leer los datos según el tipo de minijuego
        ReadMinijuegoData();
    }

    public void ReadMinijuegoData()
    {
        if (!useTTS) return;

        // Detener cualquier síntesis anterior
        StopSpeech();

        string textToSpeak = "";

        switch (getVariables.tipoMinijuego.ToLower())
        {
            case "frase":
                textToSpeak = getVariables.respuestaCorrecta;
                break;

            case "conectar":
                for (int i = 0; i < getVariables.listaPreguntas.Count; i++)
                {
                    if (i > 0) textToSpeak += ". ";
                    textToSpeak += getVariables.listaPreguntas[i] +","+ getVariables.listaRespuestas[i];
                }
                break;



            default:
                Debug.LogWarning("Tipo de minijuego no reconocido para síntesis de voz: " + getVariables.tipoMinijuego);
                return;
        }

        SpeakTextUsingAPI(textToSpeak);
    }

    private void SpeakTextUsingAPI(string textToSpeak)
    {
        if (string.IsNullOrEmpty(textToSpeak)) return;

        isSpeaking = true;

        Debug.Log("Sintetizando: " + textToSpeak);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Usar la implementación JavaScript
        SpeakText(textToSpeak, speechRate, speechPitch, speechVolume, speechLanguage);
#else
        // Implementación para otras plataformas usando el TTS del sistema
        StartCoroutine(SpeakTextNative(textToSpeak));
#endif
    }

    private void StopSpeech()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StopSpeaking();
#endif
        isSpeaking = false;
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    // Implementación para PC/Mac/Mobile usando TTS nativo cuando sea posible
    IEnumerator SpeakTextNative(string text)
    {
        // Diferentes implementaciones según la plataforma
#if UNITY_ANDROID
        // Android TTS
        AndroidJavaObject tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", 
            new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"),
            new AndroidJavaObject("com.example.MyTTSListener"));
        
        // Esperar un poco para que se inicialice el TTS
        yield return new WaitForSeconds(0.5f);
        
        // Configurar el idioma
        AndroidJavaObject locale = new AndroidJavaObject("java.util.Locale", speechLanguage.Split('-')[0]);
        tts.Call<int>("setLanguage", locale);
        
        // Configurar tasa y tono
        tts.Call<int>("setSpeechRate", speechRate);
        tts.Call<int>("setPitch", speechPitch);
        
        // Hablar
        int result = tts.Call<int>("speak", text, 0, null);
        
        // Esperar un tiempo estimado para que termine de hablar
        float estimatedDuration = (text.Length / 2.0f) / speechRate;
        yield return new WaitForSeconds(estimatedDuration);
        
        // Liberar recursos
        tts.Call("shutdown");
        
#elif UNITY_IOS
        // iOS TTS (requiere plugin)
        yield return new WaitForSeconds(text.Length * 0.05f);
        
#elif UNITY_STANDALONE_WIN
        // Windows TTS mediante SAPI (requiere interoperabilidad)
        yield return new WaitForSeconds(text.Length * 0.05f);
        
#else
        // Fallback para otras plataformas
        Debug.Log("TTS no disponible en esta plataforma. Texto: " + text);
        yield return new WaitForSeconds(text.Length * 0.05f);
#endif

        isSpeaking = false;
    }
#endif

    // Métodos públicos para usar desde botones u otros scripts
    public void ReadCorrectAnswer()
    {
        if (getVariables != null && !string.IsNullOrEmpty(getVariables.respuestaCorrecta))
        {
            SpeakTextUsingAPI(getVariables.respuestaCorrecta);
        }
    }

    public void ReadPregunta()
    {
        if (getVariables != null && !string.IsNullOrEmpty(getVariables.pregunta))
        {
            SpeakTextUsingAPI(getVariables.pregunta);
        }
    }

    public void ReadConnections()
    {
        if (getVariables != null && getVariables.listaPreguntas != null && getVariables.listaRespuestas != null)
        {
            string connectionsText = "";
            for (int i = 0; i < Mathf.Min(getVariables.listaPreguntas.Count, getVariables.listaRespuestas.Count); i++)
            {
                if (i > 0) connectionsText += ". ";
                connectionsText += getVariables.listaPreguntas[i] + "," + getVariables.listaRespuestas[i];
            }
            SpeakTextUsingAPI(connectionsText);
        }
    }
}