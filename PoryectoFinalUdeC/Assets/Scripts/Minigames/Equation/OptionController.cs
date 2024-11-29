using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptionController : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // Referencia al TextMeshPro para mostrar el número
    public int Option; // Número asignado a este libro

    public void Initialize(int assignedOption)
    {
        Option = assignedOption;

        if (textMeshPro != null)
        {
            textMeshPro.text = Option.ToString(); // Mostrar el número en el texto
        }
        else
        {
            Debug.LogWarning("El componente TextMeshPro no está asignado.");
        }
    }
}
