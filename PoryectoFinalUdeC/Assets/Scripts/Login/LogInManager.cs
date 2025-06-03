using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private string loginUrl = "https://gamificationudecapi.azurewebsites.net/api/Usuario/login";

    // Importamos la función JavaScript que verificará el localStorage
    [DllImport("__Internal")]
    private static extern string GetLocalStorageItem(string key);

    void Start()
    {
        // Configuración inicial de la interfaz
        passwordField.contentType = TMP_InputField.ContentType.Password;
        loginButton.onClick.AddListener(() => StartCoroutine(LoginUser()));

        // Intentar auto-login al iniciar
        StartCoroutine(CheckForAutoLogin());
    }

    IEnumerator CheckForAutoLogin()
    {
        // Solo en WebGL podemos acceder al localStorage
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Intentamos obtener los datos de usuario del localStorage
            string userData = GetLocalStorageItem("user");

            if (!string.IsNullOrEmpty(userData))
            {
                Debug.Log("Datos encontrados en localStorage: " + userData);

                bool loginSuccess = false;

                // Movemos el try-catch fuera de la lógica de yield
                try
                {
                    // Parseamos los datos JSON
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(userData);

                    // Guardamos los datos del usuario en PlayerPrefs
                    PlayerPrefs.SetString("Token", userInfo.token);
                    PlayerPrefs.SetInt("UsuarioId", userInfo.id);
                    PlayerPrefs.SetString("NombreUsuario", userInfo.nombreUsuario);
                    PlayerPrefs.SetInt("RolId", userInfo.rolId);
                    PlayerPrefs.SetString("Curso", userInfo.curso); // Añadir esta línea

                    // No tenemos información sobre el curso en los datos de localStorage
                    PlayerPrefs.Save();

                    feedbackText.text = "Sesión recuperada automáticamente.";
                    Debug.Log("Auto-login exitoso para: " + userInfo.nombreUsuario);

                    loginSuccess = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error al procesar datos de localStorage: " + e.Message);
                    feedbackText.text = "Error al recuperar sesión.";
                }

                if (loginSuccess)
                {
                    // Esperamos un momento para mostrar el mensaje antes de cambiar de escena
                    yield return new WaitForSeconds(1.0f);

                    // Cargamos la escena siguiente
                    SceneManager.LoadScene("TeacherList");
                }
            }
            else
            {
                Debug.Log("No se encontraron datos de usuario en localStorage.");
            }
        }
        else
        {
            Debug.Log("Auto-login solo disponible en WebGL.");
        }
    }

    IEnumerator LoginUser()
    {
        string username = usernameField.text;
        string password = passwordField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Por favor, completa todos los campos.";
            Debug.Log("Error: Campos vacíos en el formulario.");
            yield break;
        }

        string hashedPassword = ComputeSha256Hash(password);
        Debug.Log("Contraseña encriptada: " + hashedPassword);

        var loginData = new { NombreUsuario = username, Contrasena = hashedPassword };
        string jsonData = JsonConvert.SerializeObject(loginData);
        Debug.Log("Datos JSON enviados: " + jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Respuesta recibida: " + request.downloadHandler.text);
            var response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
            if (response.success)
            {
                feedbackText.text = "Login exitoso.";
                Debug.Log("Login exitoso.");
                PlayerPrefs.SetString("Token", response.token);
                PlayerPrefs.SetInt("UsuarioId", response.usuarioId);
                PlayerPrefs.SetString("Curso", response.curso);
                PlayerPrefs.SetInt("RolId", response.rolId);
                PlayerPrefs.SetString("NombreUsuario", response.nombreUsuario);
                PlayerPrefs.Save();

                SceneManager.LoadScene("TeacherList");
            }
            else
            {
                feedbackText.text = "Error: " + response.message;
                Debug.Log("Error en login: " + response.message);
            }
        }
        else
        {
            feedbackText.text = "Error de conexión: " + request.error;
            Debug.LogError("Error en la solicitud: " + request.error);
            Debug.LogError("Código de respuesta HTTP: " + request.responseCode);
            Debug.LogError("Respuesta del servidor: " + request.downloadHandler.text);
        }
    }

    private string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}

public class LoginResponse
{
    public bool success;
    public string message;
    public int usuarioId;
    public string curso;
    public int rolId;
    public string nombreUsuario;
    public string token;
}

// Clase para deserializar los datos del localStorage
public class UserInfo
{
    public string nombreUsuario;
    public int rolId;
    public string token;
    public int id;
    public string curso; // Añadir esta propiedad
}