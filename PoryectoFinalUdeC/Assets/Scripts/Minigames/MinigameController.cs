using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameController : MonoBehaviour
{
    public string sceneName;
    private Player player;  // Referencia al jugador

    private void Start()
    {
        // Buscar el objeto del jugador en la escena principal
        player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogError("No se encontró el jugador en la escena principal.");
        }
    }

    // Método que se llamará cuando se presione el botón "Completo"
    public void OnCompleteButtonPressed()
    {
        Debug.Log("Minijuego completado.");
        HandleMinigameCompletion(true);  // Pasamos true indicando que se completó
    }

    // Método que se llamará cuando se presione el botón "Incompleto"
    public void OnIncompleteButtonPressed()
    {
        Debug.Log("Minijuego no completado.");
        HandleMinigameCompletion(false);  // Pasamos false indicando que no se completó
    }

    // Método que maneja el resultado del minijuego y cierra la escena
    void HandleMinigameCompletion(bool isCompleted)
    {
        // Aquí puedes añadir lógica adicional según el estado del minijuego
        // Por ejemplo, podrías actualizar variables del juego, registrar en base de datos, etc.

        // Verificar si se tiene referencia al jugador y cambiar el estado del minijuego
        if (player != null)
        {
            player.isMinigameActive = false;
        }

        // Cerrar la escena del minijuego cargada aditivamente
        SceneManager.UnloadSceneAsync(sceneName);
    }
}
