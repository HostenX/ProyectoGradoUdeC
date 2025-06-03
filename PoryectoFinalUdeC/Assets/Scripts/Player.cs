using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float speed;
    public Animator animator;
    private bool isFlipped = false; // Track the flip state
    private InteractableObject currentInteractable; // Variable para guardar el objeto interactuable actual
    public bool isMinigameActive = false; // Bandera para controlar si el minijuego está activo
    private PlayerData playerData;
    private string apiUrl = "https://gamificationudecapi.azurewebsites.net/api/personajes/actualizar-puntaje"; // Endpoint de la API
    public GameObject introUIPanel; // Asignar el panel desde el editor
    public float introDuration = 45f; // Duración de la intro
    private bool introActive = true;
    public GameObject outroUIPanel; // Arrástralo desde el editor
    private bool isOutroActive = false;

    public int puntaje { get; set; }

    private Collider2D[] playerColliders; // Referencia a los colliders del jugador

    private void Start()
    {
        // Obtener todos los colliders 2D del jugador
        playerColliders = GetComponents<Collider2D>();
        // Buscar PlayerData de manera más robusta
        playerData = FindObjectOfType<PlayerData>();

        if (playerData == null)
        {
            Debug.LogError("No se encontró PlayerData en la escena!");
            // Puedes crear uno temporalmente para pruebas
            // GameObject tempObj = new GameObject("TempPlayerData");
            // playerData = tempObj.AddComponent<PlayerData>();
        }
        else
        {
            Debug.Log("PlayerData encontrado correctamente");
        }
        // Mostrar la pantalla de introducción
        if (introUIPanel != null)
        {
            introUIPanel.SetActive(true);
            isMinigameActive = true; // Bloquea el movimiento
            StartCoroutine(EsconderIntroDespuesDeTiempo(introDuration));
        }
    }

    private void Update()
    {
        if (introActive && Input.anyKeyDown)
        {
            introUIPanel.SetActive(false);
            isMinigameActive = false;
            introActive = false;
        }

        if (isOutroActive && Input.anyKeyDown)
        {
            Application.Quit();
            Debug.Log("El juego se cerraría aquí (solo funciona en build).");
        }


        // Solo permitimos movimiento si no hay un minijuego activo
        if (!isMinigameActive)
        {
            // Get input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Create direction vector
            Vector2 direction = new Vector2(horizontal, vertical).normalized;

            // Animate player
            AnimateMovement(direction);

            // Apply movement
            transform.position += new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Z)) // Interactuar con la tecla "Z"
            {
                Interact();
            }
        }
    }


    void Interact()
    {
        Vector3 facingDir = new Vector3(animator.GetFloat("horizontal"), animator.GetFloat("vertical"), 0f);
        Debug.DrawRay(transform.position, facingDir, Color.red, 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDir, 5f);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Interactable")) // Es un objeto interactivo
            {
                InteractableObject currentInteractable = hit.collider.GetComponent<InteractableObject>();

                if (currentInteractable != null)
                {
                    Debug.Log("Se interactuó con un objeto interactivo.");
                    LoadMinigameScene(currentInteractable.minigameSceneName);
                }
            }
            else if (hit.collider.CompareTag("NPC")) // Es un NPC
            {
                NPC npc = hit.collider.GetComponent<NPC>();

                if (npc != null)
                {
                    Debug.Log("Se interactuó con un NPC.");
                    npc.StartDialog(); // Iniciar el diálogo del NPC
                }
            }
            else if (hit.collider.CompareTag("EndObject")) // Finaliza el minijuego
            {
                Debug.Log("Se interactuó con un objeto de finalización.");
                EnviarPuntajePersonaje();
                MostrarOutro();
            }
        }
    }


    // Método para cargar la escena del minijuego
    void LoadMinigameScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Desactivar el movimiento mientras el minijuego esté activo
            isMinigameActive = true;

            // Desactivar los colliders 2D
            SetPlayerCollidersActive(false);

            // Guardar la posición actual del jugador
            Vector3 playerPosition = transform.position;

            // Cargar la escena del minijuego de manera aditiva
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            // Esperar un frame para asegurar que la nueva escena está cargada
            StartCoroutine(PositionMinigameCameraAndObjects(playerPosition));
        }
        else
        {
            Debug.LogError("El nombre de la escena del minijuego no es válido.");
        }
    }

    // Corrutina para posicionar la cámara y objetos del minijuego en la ubicación del jugador
    IEnumerator PositionMinigameCameraAndObjects(Vector3 playerPosition)
    {
        yield return null; // Esperar un frame para que la escena del minijuego se cargue

        // Posicionar la cámara del minijuego
        GameObject minigameCamera = GameObject.FindWithTag("MinigameCamera");
        if (minigameCamera != null)
        {
            minigameCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, minigameCamera.transform.position.z);
            Debug.Log("La cámara del minijuego ha sido posicionada en la ubicación del jugador.");
        }
        else
        {
            Debug.LogWarning("No se encontró la cámara del minijuego.");
        }

        // Posicionar los objetos del minijuego
        GameObject[] minigameObjects = GameObject.FindGameObjectsWithTag("MinigameObject"); // Supongamos que los objetos del minijuego tienen este tag
        foreach (GameObject minigameObject in minigameObjects)
        {
            minigameObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, minigameObject.transform.position.z);
        }

        Debug.Log("Los objetos del minijuego han sido posicionados en la ubicación del jugador.");
    }

    void AnimateMovement(Vector2 direction)
    {
        if (animator != null)
        {
            if (direction.magnitude > 0)
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("horizontal", direction.x);
                animator.SetFloat("vertical", direction.y);

                // Control animation speed based on the player's speed
                animator.speed = speed;

                // Rotate Animation - only when moving horizontally
                if (direction.x != 0)
                {
                    bool shouldFlip = direction.x < 0;

                    // Flip only when the direction has changed
                    if (shouldFlip != isFlipped)
                    {
                        transform.rotation = Quaternion.Euler(new Vector3(0f, shouldFlip ? 180f : 0f, 0f));
                        isFlipped = shouldFlip;
                    }
                }
            }
            else
            {
                animator.SetBool("isMoving", false);
                // Set animation speed back to 1 (default) when not moving
                animator.speed = 1;
            }
        }
    }

    // Método para desactivar los colliders 2D del jugador
    public void SetPlayerCollidersActive(bool isActive)
    {
        if (playerColliders != null)
        {
            foreach (Collider2D collider in playerColliders)
            {
                collider.enabled = isActive;
            }
        }
    }

    private void EnviarPuntajePersonaje()
    {
        if (playerData != null)
        {
            StartCoroutine(ActualizarPuntajeAPI(playerData.UsuarioId, playerData.PuntajeActual));
        }
        else
        {
            Debug.Log("Algo esta mal");
        }
    }

    private IEnumerator ActualizarPuntajeAPI(int usuarioId, int nuevoPuntaje)
    {
        // Construir la URL correctamente según el endpoint
        string url = $"https://gamificationudecapi.azurewebsites.net/api/Personaje/ActualizarPuntaje/{usuarioId}";

        // Crear el cuerpo de la solicitud en formato JSON
        string jsonBody = nuevoPuntaje.ToString(); // Tu API espera directamente el número en el body
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        // Log de lo que estamos enviando
        Debug.Log($"[API REQUEST] Enviando petición a: {url}");
        Debug.Log($"[API REQUEST] Body enviado: {jsonBody}");
        Debug.Log($"[API REQUEST] UsuarioID: {usuarioId}, Puntaje: {nuevoPuntaje}");

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Opcional: Agregar timeout
            request.timeout = 10;

            yield return request.SendWebRequest();

            // Log detallado de la respuesta
            Debug.Log($"[API RESPONSE] Estado: {request.responseCode}");
            Debug.Log($"[API RESPONSE] Error: {request.error}");
            Debug.Log($"[API RESPONSE] Contenido: {request.downloadHandler.text}");
            Debug.Log($"[API RESPONSE] Headers: {request.GetResponseHeaders()}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[API SUCCESS] Puntaje actualizado correctamente en la API.");
                Debug.Log($"[API SUCCESS] Respuesta completa: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError("[API ERROR] Fallo al actualizar puntaje");
                Debug.LogError($"[API ERROR] Código: {request.responseCode}");
                Debug.LogError($"[API ERROR] Mensaje: {request.error}");
                Debug.LogError($"[API ERROR] Respuesta: {request.downloadHandler.text}");

                // Para errores HTTP específicos
                if (request.responseCode == 404)
                {
                    Debug.LogError("[API ERROR 404] Recurso no encontrado - Verifica el usuarioId");
                }
                else if (request.responseCode == 400)
                {
                    Debug.LogError("[API ERROR 400] Bad Request - Verifica los datos enviados");
                }
                else if (request.responseCode == 500)
                {
                    Debug.LogError("[API ERROR 500] Error interno del servidor");
                }
            }
        }
    }

    IEnumerator EsconderIntroDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (introUIPanel != null)
        {
            introUIPanel.SetActive(false);
            isMinigameActive = false;
            introActive = false;
        }
    }

    public void MostrarOutro()
{
    if (outroUIPanel != null)
    {
        outroUIPanel.SetActive(true);
        isOutroActive = true;
        isMinigameActive = true; // Bloquea movimiento
    }
}


}
