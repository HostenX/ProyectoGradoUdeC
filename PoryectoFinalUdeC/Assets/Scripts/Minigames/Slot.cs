using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    [SerializeField] private MinigameController minigameController;
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Ondrop");

        if (eventData != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;

            OptionController option = eventData.pointerDrag.GetComponent<OptionController>();

            if (option != null) {
                int LinkedNumber = option.Option;

                Debug.Log($"{gameObject.name} = {LinkedNumber}");
            }

            /*if (minigameController != null)
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
}
