using System.Collections.Generic;
using UnityEngine;

public class BagSpawner : MonoBehaviour
{
    [SerializeField] private GetVariables getVariables;
    public GameObject bagPrefab; // Prefab de las bolsas
    public GameObject spawnAreaObject; // Objeto que define el área de spawn
    public GameObject sceneObject;
    public List<float> weights; // Lista de pesos únicos
    public int numberOfBags; // Número de bolsas a generar

    private Vector2 spawnAreaMin; // Esquina inferior izquierda del área
    private Vector2 spawnAreaMax; // Esquina superior derecha del área

    public void GenerateBags()
    {
        // Actualizar el área de spawn dinámicamente
        DefineSpawnArea();

        // Asegurarse de que la cantidad de bolsas no exceda la cantidad de pesos
        numberOfBags = weights.Count;

        // Crear una copia de la lista de pesos y barajarla
        List<float> shuffledWeights = new List<float>(weights);
        ShuffleList(shuffledWeights);

        // Generar bolsas
        for (int i = 0; i < numberOfBags; i++)
        {
            // Generar una posición aleatoria dentro del área
            float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector2 spawnPosition = new Vector2(randomX, randomY);

            // Instanciar el prefab en la posición generada y establecer su padre
            GameObject newBag = Instantiate(bagPrefab, spawnPosition, Quaternion.identity, sceneObject.transform);

            // Restablecer la escala local para evitar que herede la del padre
            newBag.transform.localScale = Vector3.one;

            // Asignar el peso único desde la lista barajada
            AssignWeight(newBag, shuffledWeights[i]);
        }
    }

    private void DefineSpawnArea()
    {
        if (spawnAreaObject != null)
        {
            // Calcular el área de spawn basada en la posición y escala actuales
            Vector3 position = spawnAreaObject.transform.position;
            Vector3 scale = spawnAreaObject.transform.localScale;

            spawnAreaMin = new Vector2(position.x - scale.x / 3, position.y - scale.y / 3);
            spawnAreaMax = new Vector2(position.x + scale.x / 3, position.y + scale.y / 3);
        }
        else
        {
            Debug.LogError("El objeto SpawnArea no está asignado.");
        }
    }

    private void AssignWeight(GameObject bag, float weight)
    {
        // Buscar el script DregDropWeight en el prefab
        DregDropWeight dropWeightScript = bag.GetComponent<DregDropWeight>();
        if (dropWeightScript != null)
        {
            dropWeightScript.dynamicMass = weight; // Asignar el peso a la variable dynamicMass
        }
        else
        {
            Debug.LogError("El objeto generado no tiene el script DregDropWeight.");
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        // Barajar la lista de forma aleatoria
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
