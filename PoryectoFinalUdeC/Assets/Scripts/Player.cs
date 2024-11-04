using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float speed;
    public Animator animator;

    private bool isFlipped = false; // Track the flip state
    private InteractableObject currentInteractable; // Variable para guardar el objeto interactuable actual
    public bool isMinigameActive = false; // Bandera para controlar si el minijuego está activo

    private void Update()
    {
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

            if (Input.GetKeyDown(KeyCode.Z)) // Interactuar con el objeto al presionar "Z"
            {
                Interact();
            }
        }
    }

    void Interact()
    {
        Vector3 facingDir = new Vector3(animator.GetFloat("horizontal"), animator.GetFloat("vertical"), 0f);
        Debug.DrawRay(transform.position, facingDir, Color.red, 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDir, 1f);

        if (hit.collider != null && hit.collider.CompareTag("Interactable"))
        {
            currentInteractable = hit.collider.GetComponent<InteractableObject>();

            if (currentInteractable != null)
            {
                Debug.Log("Se interactuó con un objeto");
                // Cargar la escena del minijuego de manera aditiva
                LoadMinigameScene(currentInteractable.minigameSceneName);
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
}
