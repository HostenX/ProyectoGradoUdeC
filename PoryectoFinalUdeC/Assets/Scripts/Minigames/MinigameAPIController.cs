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
    private string apiUrl = "https://localhost:7193/api/resultados/registrar"; // Reemplazar con la URL real

    public IEnumerator EnviarResultado(int usuarioId, int minijuegoId, int puntaje, int tiempo)
    {
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
            }
            else
            {
                Debug.LogError("Error al enviar resultado: " + request.error);
                Debug.LogError("Respuesta del servidor: " + request.downloadHandler.text);
            }
        }
    }
}
