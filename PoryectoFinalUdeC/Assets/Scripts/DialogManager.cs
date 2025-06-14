using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    private Queue<string> sentences;

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
        sentences = new Queue<string>();
        dialogPanel.SetActive(false);
    }

    private void Update()
    {
        // Actualizar estado de habla desde JavaScript (solo en WebGL)
#if UNITY_WEBGL && !UNITY_EDITOR
        isSpeaking = IsSpeaking();
#endif

        if (Input.GetKeyDown(KeyCode.Space) && !isSpeaking)
        {
            DisplayNextSentence();
        }
    }

    public void StartDialog(string[] dialogLines)
    {
        dialogPanel.SetActive(true);
        sentences.Clear();
        foreach (string sentence in dialogLines)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Si estamos hablando, parar primero
        if (isSpeaking)
        {
            StopSpeech();
        }

        if (sentences.Count == 0)
        {
            EndDialog();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogText.text = sentence;

        if (useTTS)
        {
            SpeakTextUsingAPI(sentence);
        }
    }

    public void EndDialog()
    {
        dialogPanel.SetActive(false);
        StopSpeech();
    }

    private void SpeakTextUsingAPI(string textToSpeak)
    {
        isSpeaking = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Usar la implementaci�n JavaScript
        SpeakText(textToSpeak, speechRate, speechPitch, speechVolume, speechLanguage);
#else
        // Implementaci�n para otras plataformas usando el TTS del sistema
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
    // Implementaci�n para PC/Mac/Mobile usando TTS nativo cuando sea posible
    IEnumerator SpeakTextNative(string text)
    {
        // Diferentes implementaciones seg�n la plataforma
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
        
        // Esperar un tiempo estimado (aproximado) para que termine de hablar
        // (2 caracteres por segundo es una aproximaci�n)
        float estimatedDuration = (text.Length / 2.0f) / speechRate;
        yield return new WaitForSeconds(estimatedDuration);
        
        // Liberar recursos
        tts.Call("shutdown");
        
#elif UNITY_IOS
        // iOS TTS (requiere plugin)
        // Este es un c�digo de ejemplo que requerir�a un plugin espec�fico para iOS
        // iOSSpeech.Speak(text, speechRate, speechPitch, speechLanguage);
        yield return new WaitForSeconds(text.Length * 0.05f);
        
#elif UNITY_STANDALONE_WIN
        // Windows TTS mediante SAPI (requiere interoperabilidad)
        // Aqu� podr�as usar System.Speech si tienes acceso a .NET Framework
        yield return new WaitForSeconds(text.Length * 0.05f);
        
#else
        // Fallback para otras plataformas
        Debug.Log("TTS no disponible en esta plataforma. Texto: " + text);
        yield return new WaitForSeconds(text.Length * 0.05f);
#endif

        isSpeaking = false;
    }
#endif
}