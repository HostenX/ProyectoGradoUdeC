using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceSide : MonoBehaviour
{
    private float totalWeight = 0f; // Peso acumulado en este lado

    public float TotalWeight => totalWeight; // Propiedad para acceder al peso acumulado

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Intenta obtener el script DregDropWeight en el objeto que entra
        DregDropWeight weightScript = collision.GetComponent<DregDropWeight>();
        if (weightScript != null)
        {
            // Sumar el peso del objeto al total
            totalWeight += weightScript.dynamicMass;
            Debug.Log($"Bag entró al lado. Peso agregado: {weightScript.dynamicMass}. Total: {totalWeight}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Intenta obtener el script DregDropWeight en el objeto que sale
        DregDropWeight weightScript = collision.GetComponent<DregDropWeight>();
        if (weightScript != null)
        {
            // Restar el peso del objeto al total
            totalWeight -= weightScript.dynamicMass;
            Debug.Log($"Bag salió del lado. Peso restado: {weightScript.dynamicMass}. Total: {totalWeight}");
        }
    }
}
