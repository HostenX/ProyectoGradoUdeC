using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameController : MonoBehaviour
{
    public string sceneName;
    private Player player;  // Referencia al jugador
    [SerializeField] private InteractiveTextManager textManager;
    [SerializeField] private GetVariables getVariables;
    [SerializeField] private BalanceWeightController balanceController;

    public Dictionary<Wire, GameObject> wireConnections = new Dictionary<Wire, GameObject>();
    public int totalConnectionsNeeded; // Total de conexiones correctas necesarias para ganar

    private void Start()
    {
        // Buscar el objeto del jugador en la escena principal
        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogError("No se encontr� el jugador en la escena principal.");
        }
        
    }

    // M�todo que se llamar� cuando se presione el bot�n "Completo"
    public void OnEquationComplete()
    {
        // Obtener el texto compilado desde InteractiveTextManager
        string textoCompilado = textManager.CompileText();

        // Evaluar la ecuaci�n con el texto compilado
        bool esValida = EvaluarEcuacion(textoCompilado);

        if (esValida)
        {
            // Log de resultado
            Debug.Log("Minijuego completado.");
            // Manejar el resultado del minijuego
            HandleMinigameCompletion(esValida);  // Pasamos el resultado de la evaluaci�n (true o false)
        }
        else
        {
            Debug.Log("Respuesta incorrecta, intentalo nuevamente");
        }
        
    }

    public void OnTextComplete() //Boton de minijuego "Completar Texto" terminado
    {
        // Obtener el texto compilado desde InteractiveTextManager
        string textoCompilado = textManager.CompileText();

        // Evaluar la ecuaci�n con el texto compilado
        bool esValida = EvaluarTexto(textoCompilado);

        if (esValida)
        {
            // Log de resultado
            Debug.Log("Minijuego completado.");
            // Manejar el resultado del minijuego
            HandleMinigameCompletion(esValida);  // Pasamos el resultado de la evaluaci�n (true o false)
        }
        else
        {
            Debug.Log("Respuesta incorrecta, intentalo nuevamente");
        }

    }

    public void OnBalanceComplete()
    {
        // Evaluar la ecuaci�n con el texto compilado
        bool esValida = EvaluarDesigualdad(balanceController.leftSide.TotalWeight, balanceController.rightSide.TotalWeight, balanceController.currentInequality);

        if (esValida)
        {
            // Log de resultado
            Debug.Log("Minijuego completado.");
            // Manejar el resultado del minijuego
            HandleMinigameCompletion(esValida);  // Pasamos el resultado de la evaluaci�n (true o false)
        }
        else
        {
            Debug.Log("Respuesta incorrecta, intentalo nuevamente");
        }
    }
   
    // M�todo que se llamar� cuando se presione el bot�n "Incompleto"
    public void OnIncompleteButtonPressed()
    {
        Debug.Log("Minijuego no completado.");
        HandleMinigameCompletion(false);  // Pasamos false indicando que no se complet�
    }


    //========================================== Evaluacoones============================================


    // M�todo que maneja el resultado del minijuego y cierra la escena
    void HandleMinigameCompletion(bool isCompleted)
    {
        // Aqu� puedes a�adir l�gica adicional seg�n el estado del minijuego
        // Por ejemplo, podr�as actualizar variables del juego, registrar en base de datos, etc.

        // Verificar si se tiene referencia al jugador y cambiar el estado del minijuego
        if (player != null)
        {
            player.isMinigameActive = false;
            player.SetPlayerCollidersActive(true);
        }

        // Cerrar la escena del minijuego cargada aditivamente
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public bool EvaluarEcuacion(string ecuacion)
    {
        try
        {
            // Split the equation into left and right parts by '='
            string[] partes = ecuacion.Split('=');

            // If there aren't exactly two parts, it's invalid
            if (partes.Length != 2)
            {
                return false;
            }

            // Left side and right side of the equation
            string ladoIzquierdo = partes[0].Trim();
            string ladoDerecho = partes[1].Trim();

            // Convert the expected result (right side) to a number
            float resultadoEsperado;
            if (!float.TryParse(ladoDerecho, out resultadoEsperado))
            {
                return false;
            }

            // Use DataTable.Compute to evaluate the left side expression
            var resultadoLadoIzquierdo = new System.Data.DataTable().Compute(ladoIzquierdo, null);

            // Convert the result to float
            float resultadoCalculado = Convert.ToSingle(resultadoLadoIzquierdo);

            // Compare the calculated result with the expected result
            return Mathf.Approximately(resultadoCalculado, resultadoEsperado);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al evaluar la ecuaci�n: " + ex.Message);
            return false;
        }
    }

    public bool EvaluarDesigualdad(float valorIzquierdo, float valorDerecho, string desigualdad)
    {
        switch (desigualdad)
        {
            case "<":
                return valorIzquierdo < valorDerecho;
            case ">":
                return valorIzquierdo > valorDerecho;
            case "=":
                return Mathf.Approximately(valorIzquierdo, valorDerecho);
            default:
                Debug.LogError("Desigualdad no reconocida.");
                return false;
        }
    }
    public bool EvaluarTexto (string texto)
    {
        if (getVariables == null)
        {
            Debug.LogError("GetVariables no est� asignado. No se puede evaluar el texto.");
            return false;
        }

        string respuestaCorrecta = getVariables.respuestaCorrecta;

        if (string.IsNullOrEmpty(respuestaCorrecta))
        {
            Debug.LogError("No se ha definido una respuesta correcta en GetVariables.");
            return false;
        }

        // Comparar ignorando may�sculas/min�sculas y espacios adicionales
        return string.Equals(texto.Trim(), respuestaCorrecta.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void EvaluarConectar()
    {
        int conexionesCorrectas = 0;

        foreach (var conexion in wireConnections)
        {
            Wire pregunta = conexion.Key;
            GameObject respuestaObjeto = conexion.Value;

            // Verifica si el objeto conectado es un Wire
            Wire respuesta = respuestaObjeto.GetComponent<Wire>();

            if (respuesta != null)
            {
                // Verifica si la conexi�n es v�lida
                if (string.Equals(pregunta.AnswerText, respuesta.AnswerText, StringComparison.OrdinalIgnoreCase))
                {
                    conexionesCorrectas++;
                }
            }
        }

        // Comparar conexiones correctas con las necesarias
        if (conexionesCorrectas >= totalConnectionsNeeded)
        {
            Debug.Log("�Todas las conexiones son correctas! Minijuego completado.");
            HandleMinigameCompletion(true);
        }
        else
        {
            Debug.Log($"Conexiones correctas: {conexionesCorrectas} de {totalConnectionsNeeded}. Intenta de nuevo.");
        }
    }


}