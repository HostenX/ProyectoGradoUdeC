using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Necesario para trabajar con TextMeshPro

public class DregDropWeight : MonoBehaviour
{
    private Camera minigameCamera;
    private Rigidbody2D rb;
    public LayerMask targetLayer;
    public float dynamicMass;
    private TextMeshPro textMesh; // Referencia al TextMeshPro del hijo

    private void Awake()
    {
        // Buscar la cámara con la etiqueta específica
        GameObject cameraObject = GameObject.FindWithTag("MinigameCamera");
        if (cameraObject != null)
        {
            minigameCamera = cameraObject.GetComponent<Camera>();
        }

        if (minigameCamera == null)
        {
            Debug.LogError("No camera with the tag 'MinigameCamera' found or it is missing the Camera component.");
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on this GameObject.");
        }
    }

    private void Start()
    {
        // Buscar el componente TextMeshPro en el hijo
        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("No TextMeshPro component found in the children of this GameObject.");
        }
        else
        {
            UpdateWeightText(); // Actualizar el texto con el valor de dynamicMass
        }
    }

    public void OnMouseDrag()
    {
        if (minigameCamera == null) return;

        rb.bodyType = RigidbodyType2D.Static;

        // Convertir la posición del mouse a coordenadas del mundo
        Vector3 mousePosition = minigameCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z; // Mantener la profundidad original del objeto
        transform.position = mousePosition; // Mover el objeto directamente a la posición del mouse
    }

    public void OnMouseUp()
    {
        if (rb == null) return;

        Collider2D targetCollider = Physics2D.OverlapPoint(transform.position, targetLayer);
        if (targetCollider != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = dynamicMass/2;
            Debug.Log("Objeto dentro del collider del objetivo. Rigidbody cambiado a dinámico.");
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Static;
            transform.rotation = Quaternion.identity;
            Debug.Log("Objeto soltado fuera del objetivo. Rigidbody cambiado a estático y rotación restablecida.");
        }
    }

    private void UpdateWeightText()
    {
        if (textMesh != null)
        {
            textMesh.text = $"{dynamicMass:F1}"; // Actualizar el texto con el peso (formateado a 1 decimal)
        }
    }
}
