using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class RegistroResultadoDTO
{
    public int UsuarioEstudianteId;
    public int MinijuegoId;
    public int PuntajeObtenido;
    public int TiempoEjecucion;
}

public class MinigameAPIController : MonoBehaviour
{
    private string apiUrl = "https://gamificationudecapi.azurewebsites.net/api/resultados/registrar"; // Reemplazar con la URL real
    private PlayerData playerData;

    private void Start()
    {
        playerData = FindObjectOfType<PlayerData>();
    }

    public IEnumerator EnviarResultado(int usuarioId, int minijuegoId, int puntaje, int tiempo)
    {
        // Buscar PlayerData aquí, no solo en Start
        if (playerData == null)
        {
            playerData = FindObjectOfType<PlayerData>();
        }

        RegistroResultadoDTO resultado = new RegistroResultadoDTO
        {
            UsuarioEstudianteId = usuarioId,
            MinijuegoId = minijuegoId,
            PuntajeObtenido = puntaje,
            TiempoEjecucion = tiempo
        };

        string jsonData = JsonUtility.ToJson(resultado);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Resultado enviado con éxito: " + request.downloadHandler.text);

                // Siempre usar el usuarioId de PlayerData como prioridad
                if (playerData == null)
                {
                    playerData = FindObjectOfType<PlayerData>();
                }

                if (playerData != null)
                {
                    usuarioId = playerData.UsuarioId;
                    Debug.Log($"Usando UsuarioID de PlayerData: {usuarioId}");
                }
                else
                {
                    Debug.LogError("No se encontró PlayerData, usando usuarioId proporcionado");
                }
                if (playerData != null)
                {
                    Debug.Log($"Añadiendo {puntaje} puntos al jugador. Puntaje anterior: {playerData.PuntajeActual}");
                    playerData.AgregarPuntaje(puntaje);
                    Debug.Log($"Nuevo puntaje del jugador: {playerData.PuntajeActual}");
                }
                else
                {
                    Debug.LogError("No se pudo encontrar el PlayerData para actualizar el puntaje");
                }
            }
            else
            {
                Debug.LogError("Error al enviar resultado: " + request.error);
                Debug.LogError("Respuesta del servidor: " + request.downloadHandler.text);
            }
        }
    }

}
