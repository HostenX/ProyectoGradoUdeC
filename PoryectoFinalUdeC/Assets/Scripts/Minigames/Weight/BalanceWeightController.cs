using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BalanceWeightController : MonoBehaviour
{
    public BalanceSide leftSide; // Referencia al lado izquierdo
    public BalanceSide rightSide; // Referencia al lado derecho
    public TextMeshPro textDisplay; // Referencia al TextMeshPro en el objeto hijo

    // Lista de valores de desigualdad
    public List<string> inequalities = new List<string> { "<", ">", "=" };

    public string currentInequality; // Desigualdad seleccionada

    private void Start()
    {
        // Seleccionar una desigualdad aleatoriamente al inicio
        UpdateRandomInequality();
    }

    private void Update()
    {
        float leftWeight = leftSide.TotalWeight;
        float rightWeight = rightSide.TotalWeight;
    }

    // Método para seleccionar y actualizar el texto con una desigualdad aleatoria
    public void UpdateRandomInequality()
    {
        if (inequalities.Count > 0)
        {
            // Seleccionar aleatoriamente de la lista
            currentInequality = inequalities[Random.Range(0, inequalities.Count)];

            // Actualizar el TextMeshPro si está configurado
            if (textDisplay != null)
            {
                textDisplay.text = currentInequality;
                Debug.Log($"Nueva desigualdad seleccionada: {currentInequality}");
            }
            else
            {
                Debug.LogWarning("No se asignó el TextMeshPro para mostrar la desigualdad.");
            }
        }
        else
        {
            Debug.LogWarning("La lista de desigualdades está vacía.");
        }
    }
}
