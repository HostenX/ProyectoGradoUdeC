using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private string loginUrl = "https://localhost:7193/api/Usuario/login";

    void Start()
    {
        passwordField.contentType = TMP_InputField.ContentType.Password;
        loginButton.onClick.AddListener(() => StartCoroutine(LoginUser()));
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

