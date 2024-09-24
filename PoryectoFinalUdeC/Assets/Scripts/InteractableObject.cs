using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // El m�todo Interact ahora recibe un color como par�metro
    public void Interact(Color color)
    {
        // Aplicar el color recibido al SpriteRenderer del objeto interactuable
        GetComponent<SpriteRenderer>().color = color;
        Debug.Log("Interacci�n con el objeto: color aplicado " + color);
    }
}
