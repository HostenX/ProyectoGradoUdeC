using System.Collections.Generic;
using UnityEngine;

public class BookGenerator : MonoBehaviour
{
    [SerializeField] private GameObject bookPrefab; // Prefab del objeto "Book"
    [SerializeField] private Transform spawnContainer; // Contenedor para los libros
    public List<string> numbers; // Lista de números para los libros
    public Vector2 spawnAreaSize = new Vector2(200, 500); // Área de generación (en unidades de UI o mundo)
    public float minSpacing = 60f; // Distancia mínima entre libros
    public Color gizmoColor = new Color(0, 1, 0, 0.25f); // Color del gizmo

    public void GenerateBooks()
    {
        if (numbers == null || numbers.Count == 0)
        {
            Debug.LogError("La lista de números está vacía o no está asignada.");
            return;
        }

        // Limpiar objetos previos
        foreach (Transform child in spawnContainer)
        {
            Destroy(child.gameObject);
        }

        List<Vector2> usedPositions = new List<Vector2>();

        // Generar un libro por cada número
        foreach (string number in numbers)
        {
            Vector2 spawnPosition;
            int attempts = 0;
            const int maxAttempts = 100;

            // Encontrar una posición válida con suficiente espacio
            do
            {
                float randomX = Random.Range(-spawnAreaSize.x, spawnAreaSize.x);
                spawnPosition = new Vector2(randomX, -30f);
                attempts++;
            }
            while (!IsPositionValid(spawnPosition, usedPositions) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("No se pudo encontrar una posición válida para un libro después de varios intentos.");
                continue;
            }

            // Generar el libro
            GameObject newBook = Instantiate(bookPrefab, spawnContainer);

            // Configurar el número en el OptionController
            OptionController optionController = newBook.GetComponent<OptionController>();
            if (optionController != null)
            {
                optionController.Initialize(number);  // Pasar el valor como string
            }
            else
            {
                Debug.LogError("El prefab no tiene el script OptionController.");
            }

            // Asignar la posición al libro
            RectTransform bookRectTransform = newBook.GetComponent<RectTransform>();
            if (bookRectTransform != null)
            {
                bookRectTransform.anchoredPosition = spawnPosition;
            }
            else
            {
                newBook.transform.localPosition = new Vector3(spawnPosition.x, spawnPosition.y, 0);
            }

            usedPositions.Add(spawnPosition);
            newBook.name = $"Book_{number}";
        }
    }

    private bool IsPositionValid(Vector2 newPosition, List<Vector2> usedPositions)
    {
        foreach (Vector2 usedPosition in usedPositions)
        {
            if (Vector2.Distance(newPosition, usedPosition) < minSpacing)
            {
                return false;
            }
        }
        return true;
    }

    // Método para dibujar el área de generación en la escena
    private void OnDrawGizmosSelected()
    {
        if (spawnContainer == null) return;

        Gizmos.color = gizmoColor;

        // Dibujar el rectángulo en la posición del contenedor
        Vector3 center = spawnContainer.position;
        Gizmos.DrawCube(center, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 1));
    }
}
