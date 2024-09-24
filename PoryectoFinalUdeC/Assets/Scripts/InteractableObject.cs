using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // El método Interact ahora recibe un color como parámetro
    public void Interact(Color color)
    {
        // Aplicar el color recibido al SpriteRenderer del objeto interactuable
        GetComponent<SpriteRenderer>().color = color;
        Debug.Log("Interacción con el objeto: color aplicado " + color);
    }
}
