using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class InteractiveTextManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform textContainer;
    [SerializeField] public float verticalOffset;
    [SerializeField] private float fadeDuration = 1f; // Duración de la transición

    private CanvasGroup textCanvasGroup; // Para manejar la transparencia
    private List<GameObject> slots = new List<GameObject>();

    private void Awake()
    {
        // Asegurar que haya un CanvasGroup en el texto
        textCanvasGroup = textMeshPro.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
            textCanvasGroup = textMeshPro.gameObject.AddComponent<CanvasGroup>();
    }

    public void GenerateInteractiveText(string baseText)
    {
        // Limpiar texto y slots previos
        foreach (GameObject slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();
        textMeshPro.text = baseText; // Establecer el texto inicial

        // Forzar a TextMeshPro a actualizar su layout
        textMeshPro.ForceMeshUpdate();

        // Obtener la información del texto
        var textInfo = textMeshPro.textInfo;

        for (int i = 0; i < baseText.Length; i++)
        {
            if (baseText[i] == '_')
            {
                // Obtener la posición del carácter `_`
                int charIndex = i;
                var charInfo = textInfo.characterInfo[charIndex];

                // Calcular posición en pantalla del carácter
                Vector3 charPosition = (charInfo.bottomLeft + charInfo.topRight) / 2;

                // Convertir la posición local del texto a posición del mundo
                Vector3 worldPosition = textMeshPro.transform.TransformPoint(charPosition);
                worldPosition += Vector3.up * verticalOffset;

                // Crear un slot en la posición calculada
                GameObject newSlot = Instantiate(slotPrefab, textContainer);
                newSlot.transform.position = worldPosition;

                // Aplicar Fade-In
                StartCoroutine(FadeInSlot(newSlot));

                slots.Add(newSlot);
                newSlot.name = $"Slot {slots.Count}";
            }

        }
        StartCoroutine(FadeInText());
    }

    private IEnumerator FadeInSlot(GameObject slot)
    {
        CanvasGroup canvasGroup = slot.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = slot.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        float duration = fadeDuration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeInText()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            textCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCanvasGroup.alpha = 1;
    }
    public string CompileText()
    {
        StringBuilder finalText = new StringBuilder(textMeshPro.text); // Usamos StringBuilder para manejar el texto

        int underscoreIndex = 0; // Índice para buscar guiones bajos en el texto

        foreach (GameObject slot in slots)
        {
            var slotComponent = slot.GetComponent<Slot>();
            if (slotComponent != null)
            {
                string selectedOption = slotComponent.GetAssignedOptionText();
                if (!string.IsNullOrEmpty(selectedOption))
                {
                    // Buscar el siguiente guion bajo en el texto
                    underscoreIndex = finalText.ToString().IndexOf('_', underscoreIndex);

                    if (underscoreIndex != -1)
                    {
                        // Reemplazar el guion bajo con la opción seleccionada
                        finalText.Remove(underscoreIndex, 1); // Eliminar el guion bajo
                        finalText.Insert(underscoreIndex, selectedOption); // Insertar la opción
                    }
                }
            }
        }

        Debug.Log($"Texto completo: {finalText.ToString()}");
        return finalText.ToString();
    }

}
