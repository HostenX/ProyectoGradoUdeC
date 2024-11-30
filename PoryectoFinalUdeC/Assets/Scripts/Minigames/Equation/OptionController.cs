using TMPro;
using UnityEngine;

public class OptionController : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // Referencia al TextMeshPro para mostrar el número
    public string Option; // Número asignado a este libro

    // Cambiar a string, ya que puede ser tanto un número como un operador
    public void Initialize(string assignedOption)
    {
        Option = assignedOption;

        if (textMeshPro != null)
        {
            textMeshPro.text = Option; // Mostrar el texto (número o operador)
        }
        else
        {
            Debug.LogWarning("El componente TextMeshPro no está asignado.");
        }
    }
}
