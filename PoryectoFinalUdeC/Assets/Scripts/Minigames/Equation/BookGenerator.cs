using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BookGenerator : MonoBehaviour
{
    [SerializeField] private GameObject bookPrefab; // Prefab del objeto "Book"
    [SerializeField] private Transform spawnContainer; // Contenedor para los libros
    [SerializeField] private RectTransform generationArea; // Área de generación en la UI
    public List<string> numbers; // Lista de números para los libros
    public float minSpacing; // Distancia mínima entre libros
    public Color gizmoColor = new Color(0, 1, 0, 0.25f); // Color del gizmo

    public void GenerateBooks()
    {
        if (numbers == null || numbers.Count == 0)
        {
            Debug.LogError("La lista de números está vacía o no está asignada.");
            return;
        }

        if (bookPrefab == null)
        {
            Debug.LogError("El prefab de Book no está asignado en el Inspector.");
            return;
        }

        if (spawnContainer == null || generationArea == null)
        {
            Debug.LogError("El contenedor o el área de generación no están asignados en el Inspector.");
            return;
        }

        // Obtener los límites del área de generación
        Vector2 areaSize = generationArea.rect.size;
        Vector2 areaPosition = generationArea.anchoredPosition;

        // Limpiar objetos previos
        foreach (Transform child in spawnContainer)
        {
            Destroy(child.gameObject);
        }

        List<(Vector2 position, float width)> usedBooks = new List<(Vector2, float)>();

        foreach (string number in numbers)
        {
            Vector2 spawnPosition;
            float bookWidth = 0f;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
                float randomY = Random.Range(-areaSize.y / 2, areaSize.y / 2);
                spawnPosition = new Vector2(randomX, randomY) + areaPosition;
                attempts++;
            }
            while (!IsPositionValid(spawnPosition, usedBooks, bookWidth) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("No se pudo encontrar una posición válida para un libro después de varios intentos.");
                continue;
            }

            GameObject newBook = Instantiate(bookPrefab, spawnContainer);
            if (newBook == null)
            {
                Debug.LogError("No se pudo instanciar el libro.");
                continue;
            }

            CanvasGroup canvasGroup = newBook.GetComponent<CanvasGroup>() ?? newBook.AddComponent<CanvasGroup>();
            OptionController optionController = newBook.GetComponent<OptionController>();
            optionController?.Initialize(number);

            RectTransform bookRectTransform = newBook.GetComponent<RectTransform>();
            if (bookRectTransform == null) continue;

            int charCount = number.Length;
            float baseWidth = 128f;
            float extraWidthPerChar = 8f;
            float maxWidth = 300f;

            bookWidth = Mathf.Clamp(baseWidth + (charCount - 4) * extraWidthPerChar, baseWidth, maxWidth);
            bookRectTransform.sizeDelta = new Vector2(bookWidth, bookRectTransform.sizeDelta.y);

            bookRectTransform.anchoredPosition = spawnPosition;
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeInBook(canvasGroup, 0.9f));

            usedBooks.Add((spawnPosition, bookWidth));
            newBook.name = $"Book_{number}";
        }
    }

    private IEnumerator FadeInBook(CanvasGroup canvasGroup, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private bool IsPositionValid(Vector2 newPosition, List<(Vector2 position, float width)> usedBooks, float newBookWidth)
    {
        foreach (var usedBook in usedBooks)
        {
            float combinedSpacing = (usedBook.width + newBookWidth) / 2 + minSpacing;
            if (Vector2.Distance(newPosition, usedBook.position) < combinedSpacing)
            {
                return false;
            }
        }
        return true;
    }
}
