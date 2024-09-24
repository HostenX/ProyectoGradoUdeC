using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
    public float speed;
    public Animator animator;

    private bool isFlipped = false; // Track the flip state
    private string colorApiUrl = "http://localhost:5238/api/color"; // URL de la API para obtener color
    private string interactionApiUrl = "http://localhost:5238/api/interaction"; // URL de la API para registrar interacciones

    private InteractableObject currentInteractable; // Variable para guardar el objeto interactuable actual

    private void Update()
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
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
                // Obtener el color desde la API y pasarlo al objeto interactuable
                StartCoroutine(GetColorFromApi());

                // Registrar la interacci�n
                StartCoroutine(RegisterInteraction());
            }
        }
    }

    // M�todo para obtener el color desde la API y pasarlo al objeto interactuable
    IEnumerator GetColorFromApi()
    {
        UnityWebRequest www = UnityWebRequest.Get(colorApiUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Deserializar la respuesta JSON en un objeto ColorData
            ColorData colorData = JsonUtility.FromJson<ColorData>(www.downloadHandler.text);

            // Agregar '#' al valor de colorHex si es necesario
            string colorHex = colorData.colorHex.StartsWith("#") ? colorData.colorHex : "#" + colorData.colorHex;

            // Si el color es v�lido, lo pasamos al m�todo Interact del objeto interactuable
            if (ColorUtility.TryParseHtmlString(colorHex, out Color newColor))
            {
                currentInteractable.Interact(newColor); // Pasar el color al objeto interactuable
            }
            else
            {
                Debug.LogError("El color recibido no es v�lido: " + colorHex);
            }
        }
        else
        {
            Debug.LogError("Error al obtener el color: " + www.error);
        }
    }

    // M�todo para registrar la interacci�n en la base de datos a trav�s de la API
    IEnumerator RegisterInteraction()
    {
        UnityWebRequest www = UnityWebRequest.PostWwwForm(interactionApiUrl, "");  // Hacer una solicitud POST
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Interacci�n registrada exitosamente.");
        }
        else
        {
            Debug.LogError("Error al registrar la interacci�n: " + www.error);
        }
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

// Clase que representa la estructura de la respuesta JSON
[System.Serializable]
public class ColorData
{
    public int id;
    public string colorHex;
}
