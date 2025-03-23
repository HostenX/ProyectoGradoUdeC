using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameController : MonoBehaviour
{
    public string sceneName;
    private Player player;
    [SerializeField] private InteractiveTextManager textManager;
    [SerializeField] private GetVariables getVariables;
    [SerializeField] private BalanceWeightController balanceController;
    [SerializeField] private Image errorBorder;
    [SerializeField] private Image successBorder;

    public Dictionary<Wire, GameObject> wireConnections = new Dictionary<Wire, GameObject>();
    public int totalConnectionsNeeded;
    public int puntosBase;
    public int penalidadPuntos;
    public MinigameAPIController apiController;
    private int usuarioId = 1; // Debe obtenerse dinámicamente según el jugador
    private float tiempoInicio;

    private void Start()
    {
        int puntosActuales = puntosBase;
        player = FindObjectOfType<Player>();
        getVariables = FindObjectOfType<GetVariables>();

        if (player == null)
        {
            Debug.LogError("No se encontró el jugador en la escena principal.");
        }

        if (getVariables != null)
        {
            InvokeRepeating(nameof(VerificarYAsignarPuntos), 0.1f, 0.5f);
        }
        else
        {
            Debug.LogError("No se encontró GetVariables en la escena.");
        }
        tiempoInicio = Time.time;
    }

    void VerificarYAsignarPuntos()
    {
        if (getVariables.puntosBase > 0) // Espera a que los puntos sean válidos
        {
            puntosBase = getVariables.puntosBase;
            penalidadPuntos = getVariables.penalidadPuntos;
            Debug.Log($"Puntos base asignados desde GetVariables: {puntosBase}, Penalidad: {penalidadPuntos}");
            CancelInvoke(nameof(VerificarYAsignarPuntos)); // Detener la verificación una vez asignados
        }
    }


    public void OnEquationComplete()
    {
        string textoCompilado = textManager.CompileText();
        bool esValida = EvaluarEcuacion(textoCompilado);
        HandleMinigameResult(esValida);
    }

    public void OnTextComplete()
    {
        string textoCompilado = textManager.CompileText();
        bool esValida = EvaluarTexto(textoCompilado);
        HandleMinigameResult(esValida);
    }

    public void OnBalanceComplete()
    {
        bool esValida = EvaluarDesigualdad(balanceController.leftSide.TotalWeight, balanceController.rightSide.TotalWeight, balanceController.currentInequality);
        HandleMinigameResult(esValida);
    }
   
    public void OnIncompleteButtonPressed()
    {
        Debug.Log("Minijuego no completado.");
        HandleMinigameResult(false);
    }

    private void HandleMinigameResult(bool isCorrect)
    {
        if (isCorrect)
        {
            Debug.Log("Minijuego completado.");
            StartCoroutine(ShowSuccessEffect());
            HandleMinigameCompletion(true);
        }
        else
        {
            Debug.Log("Respuesta incorrecta, intenta nuevamente");

            puntosBase = Mathf.Max(0, puntosBase - penalidadPuntos);
            Debug.Log($"Puntos reducidos. Puntos actuales: {puntosBase}");
            StartCoroutine(ShowErrorEffect());
        }
    }

    private IEnumerator ShowErrorEffect()
    {
        if (errorBorder != null)
        {
            errorBorder.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            errorBorder.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowSuccessEffect()
    {
        if (successBorder != null)
        {
            successBorder.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            successBorder.gameObject.SetActive(false);
        }
    }

    void HandleMinigameCompletion(bool isCompleted)
    {
        if (player != null)
        {
            player.isMinigameActive = false;
            player.SetPlayerCollidersActive(true);
        }

        int tiempoTotal = Mathf.RoundToInt(Time.time - tiempoInicio);
        if (tiempoTotal < 0) tiempoTotal = 0; // Seguridad para evitar valores negativos

        if (apiController != null)
        {
            StartCoroutine(apiController.EnviarResultado(usuarioId, getVariables.minijuegoId, puntosBase, tiempoTotal));
        }
        else
        {
            Debug.LogError("No se encontró el MinigameAPIController.");
        }

        SceneManager.UnloadSceneAsync(sceneName);
    }
    public bool EvaluarEcuacion(string ecuacion)
    {
        try
        {
            string[] partes = ecuacion.Split('=');
            if (partes.Length != 2) return false;

            string ladoIzquierdo = partes[0].Trim();
            string ladoDerecho = partes[1].Trim();

            if (!float.TryParse(ladoDerecho, out float resultadoEsperado)) return false;
            var resultadoLadoIzquierdo = new System.Data.DataTable().Compute(ladoIzquierdo, null);
            float resultadoCalculado = Convert.ToSingle(resultadoLadoIzquierdo);

            return Mathf.Approximately(resultadoCalculado, resultadoEsperado);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al evaluar la ecuación: " + ex.Message);
            return false;
        }
    }

    public bool EvaluarDesigualdad(float valorIzquierdo, float valorDerecho, string desigualdad)
    {
        switch (desigualdad)
        {
            case "<": return valorIzquierdo < valorDerecho;
            case ">": return valorIzquierdo > valorDerecho;
            case "=": return Mathf.Approximately(valorIzquierdo, valorDerecho);
            default:
                Debug.LogError("Desigualdad no reconocida.");
                return false;
        }
    }

    public bool EvaluarTexto(string texto)
    {
        if (getVariables == null)
        {
            Debug.LogError("GetVariables no está asignado. No se puede evaluar el texto.");
            return false;
        }
        
        string respuestaCorrecta = getVariables.respuestaCorrecta;
        if (string.IsNullOrEmpty(respuestaCorrecta))
        {
            Debug.LogError("No se ha definido una respuesta correcta en GetVariables.");
            return false;
        }
        
        return string.Equals(texto.Trim(), respuestaCorrecta.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void EvaluarConectar()
    {
        int conexionesCorrectas = 0;
        foreach (var conexion in wireConnections)
        {
            Wire pregunta = conexion.Key;
            GameObject respuestaObjeto = conexion.Value;
            Wire respuesta = respuestaObjeto.GetComponent<Wire>();

            if (respuesta != null && string.Equals(pregunta.AnswerText, respuesta.AnswerText, StringComparison.OrdinalIgnoreCase))
            {
                conexionesCorrectas++;
            }
        }

        if (conexionesCorrectas >= totalConnectionsNeeded)
        {
            Debug.Log("¡Todas las conexiones son correctas! Minijuego completado.");
            HandleMinigameResult(true);
        }
        else
        {
            Debug.Log($"Conexiones correctas: {conexionesCorrectas} de {totalConnectionsNeeded}. Intenta de nuevo.");
            StartCoroutine(ShowErrorEffect());
        }
    }
}
