using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    [SerializeField] private InteractiveTextManager textManager;
    [SerializeField] private MinigameController minigameController;
    public GameObject textContainer;

    public int LinkedNumber { get; private set; } = -1; // Opción asignada (-1 si no tiene)

    private void Start()
    {
        if (textManager == null)
        {
            textContainer = GameObject.Find("TextContainer"); // Buscar por nombre
            textManager = textContainer.GetComponent<InteractiveTextManager>();
        }
        else {
            Debug.Log("TextContainerNoEncontrado");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {

        if (eventData != null)
        {
            // Mover el objeto arrastrado al slot
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;

            // Obtener la opción arrastrada
            OptionController option = eventData.pointerDrag.GetComponent<OptionController>();

            if (option != null)
            {
                LinkedNumber = option.Option; // Asignar la opción al slot
                Debug.Log($"{gameObject.name} = {LinkedNumber}");
            }

            
            /*
            if (minigameController != null)
            {
                minigameController.OnCompleteButtonPressed();
            }
            else
            {
                Debug.LogError("MinigameController no está asignado.");
            }
            */
        }
    }

    public string GetAssignedOptionText()
    {
        // Devuelve el texto correspondiente a la opción asignada
        return LinkedNumber != -1 ? LinkedNumber.ToString() : "";
    }
}
