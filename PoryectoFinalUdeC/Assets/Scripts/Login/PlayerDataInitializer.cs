using UnityEngine;

public class PlayerDataInitializer : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerData playerData = player.GetComponent<PlayerData>();
            if (playerData != null)
            {
                playerData.UsuarioId = PlayerPrefs.GetInt("UsuarioId");
                playerData.Curso = PlayerPrefs.GetString("Curso");
                playerData.RolId = PlayerPrefs.GetInt("RolId");
                playerData.NombreUsuario = PlayerPrefs.GetString("NombreUsuario");
                playerData.TeacherId = PlayerPrefs.GetInt("selectedTeacherId");

                Debug.Log("Datos del jugador inicializados correctamente.");
            }
            else
            {
                Debug.LogError("El objeto 'Player' no tiene el componente 'PlayerData'.");
            }
        }
        else
        {
            Debug.LogError("No se encontró el objeto 'Player' en la escena.");
        }
    }
}
