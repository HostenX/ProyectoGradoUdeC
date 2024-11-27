using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptionController : MonoBehaviour
{
    public GetVariables getVariables;
    public TextMeshProUGUI textMeshPro;
    public int Option;

    private void Start()
    {
        if (getVariables != null && textMeshPro != null && getVariables.numeros.Count > 0)
        {
            int randomIndex = Random.Range(0, getVariables.numeros.Count);
            Option = getVariables.numeros[randomIndex];

            textMeshPro.text = Option.ToString();
            
        }
        else
        {
            Debug.LogWarning("Por favor, asegúrate de asignar todas las referencias y que la lista no esté vacía.");
        }
    }
}
