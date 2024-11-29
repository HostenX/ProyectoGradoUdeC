using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class InteractiveTextManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro; // Referencia al componente de texto
    [SerializeField] private GameObject slotPrefab; // Prefab del objeto Slot
    [SerializeField] private Transform textContainer; // Contenedor para los slots y texto dinámico
    [SerializeField] public float verticalOffset;

    private List<GameObject> slots = new List<GameObject>(); // Lista de slots generados

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
                slots.Add(newSlot);

                newSlot.name = $"Slot {slots.Count}";
            }
        }
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
