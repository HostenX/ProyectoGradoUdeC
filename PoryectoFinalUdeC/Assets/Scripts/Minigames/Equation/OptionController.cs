using TMPro;
using UnityEngine;

public class OptionController : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // Referencia al TextMeshPro para mostrar el n�mero
    public string Option; // N�mero asignado a este libro

    // Cambiar a string, ya que puede ser tanto un n�mero como un operador
    public void Initialize(string assignedOption)
    {
        Option = assignedOption;

        if (textMeshPro != null)
        {
            textMeshPro.text = Option; // Mostrar el texto (n�mero o operador)
        }
        else
        {
            Debug.LogWarning("El componente TextMeshPro no est� asignado.");
        }
    }
}
