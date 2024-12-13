using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class BookGenerator : MonoBehaviour
{
    [SerializeField] private GameObject bookPrefab; // Prefab del objeto "Book"
    [SerializeField] private Transform spawnContainer; // Contenedor para los libros
    public List<string> numbers; // Lista de números para los libros
    public Vector2 spawnAreaSize; // Área de generación (en unidades de UI o mundo)
    public float minSpacing; // Distancia mínima entre libros
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

        // Lista de libros generados con posición y ancho
        List<(Vector2 position, float width)> usedBooks = new List<(Vector2, float)>();

        // Generar un libro por cada número
        foreach (string number in numbers)
        {
            Vector2 spawnPosition;
            float bookWidth = 0f;
            int attempts = 0;
            const int maxAttempts = 100;

            // Encontrar una posición válida con suficiente espacio
            do
            {
                float randomX = Random.Range(-spawnAreaSize.x, spawnAreaSize.x);
                float randomY = Random.Range(-spawnAreaSize.y, 15f) / 8;
                spawnPosition = new Vector2(randomX, randomY);
                attempts++;
            }
            while (!IsPositionValid(spawnPosition, usedBooks, bookWidth) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("No se pudo encontrar una posición válida para un libro después de varios intentos.");
                continue;
            }

            // Generar el libro
            GameObject newBook = Instantiate(bookPrefab, spawnContainer);

            // Configurar el número en el OptionController y ajustar el ancho del libro
            OptionController optionController = newBook.GetComponent<OptionController>();
            if (optionController != null)
            {
                optionController.Initialize(number); // Pasar el valor como string

                // Ajustar el ancho del libro según el texto
                RectTransform bookWidthTransform = newBook.GetComponent<RectTransform>();
                if (bookWidthTransform != null)
                {
                    int charCount = number.Length; // Contar caracteres del texto
                    float baseWidth = 32f;       // Ancho base para textos cortos
                    float extraWidthPerChar = 8f; // Incremento por cada carácter adicional
                    float maxWidth = 300f;        // Ancho máximo permitido (opcional)

                    // Calcular el nuevo ancho
                    bookWidth = Mathf.Min(baseWidth + (charCount - 4) * extraWidthPerChar, maxWidth);
                    if (bookWidth < baseWidth) bookWidth = baseWidth; // Asegurar ancho mínimo

                    // Aplicar el ancho ajustado
                    bookWidthTransform.sizeDelta = new Vector2(bookWidth, bookWidthTransform.sizeDelta.y);

                    // Ajustar el ancho del TextMeshPro para que coincida con el RectTransform del libro
                    TextMeshProUGUI textComponent = newBook.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        RectTransform textRectTransform = textComponent.GetComponent<RectTransform>();
                        if (textRectTransform != null)
                        {
                            textRectTransform.sizeDelta = new Vector2(bookWidth, textRectTransform.sizeDelta.y);
                            textRectTransform.anchoredPosition = new Vector2(0, textRectTransform.anchoredPosition.y);
                        }
                    }
                }
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

            // Registrar la posición y el ancho del libro
            usedBooks.Add((spawnPosition, bookWidth));
            newBook.name = $"Book_{number}";
        }
    }

    private bool IsPositionValid(Vector2 newPosition, List<(Vector2 position, float width)> usedBooks, float newBookWidth)
    {
        foreach (var usedBook in usedBooks)
        {
            float combinedSpacing = (usedBook.width + newBookWidth) / 2 + minSpacing; // Distancia mínima considerando tamaños
            if (Vector2.Distance(newPosition, usedBook.position) < combinedSpacing)
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
        Gizmos.DrawCube(center, new Vector3(spawnAreaSize.x, spawnAreaSize.y - 40f, 1));
    }
}
