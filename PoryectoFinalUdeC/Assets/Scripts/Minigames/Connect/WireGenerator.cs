using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireGenerator : MonoBehaviour
{
    [SerializeField] private GameObject questionWirePrefab; // Prefab para cables de preguntas
    [SerializeField] private GameObject answerWirePrefab;   // Prefab para cables de respuestas
    [SerializeField] private Transform questionsContainer;  // Contenedor de preguntas
    [SerializeField] private Transform answersContainer;    // Contenedor de respuestas
    [SerializeField] private Color questionWireColor;       // Color para cables de preguntas
    [SerializeField] private Color answerWireColor;         // Color para cables de respuestas
    [SerializeField] private float generationDelay = 0.5f;  // Tiempo entre la generación de cada cable
    [SerializeField] private float fadeDuration = 0.5f;     // Duración del efecto de fade-in

    public void GenerateWires(List<string> questions, List<string> answers)
    {
        if (questions.Count != answers.Count)
        {
            Debug.LogError("Las listas de preguntas y respuestas deben tener el mismo tamaño.");
            return;
        }

        // Desordenar preguntas y respuestas en paralelo
        Shuffle(questions, answers);

        // Crear una lista de índices para las respuestas
        List<int> answerIndices = new List<int>();
        for (int i = 0; i < answers.Count; i++)
        {
            answerIndices.Add(i);
        }

        // Desordenar los índices de las respuestas
        Shuffle(answerIndices);

        // Iniciar la generación con efecto fade-in
        StartCoroutine(GenerateWiresWithTransition(questions, answers, answerIndices));
    }

    private IEnumerator GenerateWiresWithTransition(List<string> questions, List<string> answers, List<int> answerIndices)
    {
        for (int i = 0; i < questions.Count; i++)
        {
            // Crear el cable para la pregunta
            GameObject questionWire = Instantiate(questionWirePrefab, questionsContainer);
            var questionComponent = questionWire.GetComponent<Wire>();
            questionComponent.Initialize(questions[i], answers[i], questionWireColor);
            StartCoroutine(FadeIn(questionWire));

            // Crear el cable para la respuesta usando el índice desordenado
            int randomizedIndex = answerIndices[i];
            GameObject answerWire = Instantiate(answerWirePrefab, answersContainer);
            var answerComponent = answerWire.GetComponent<Wire>();
            answerComponent.Initialize(questions[i], answers[randomizedIndex], answerWireColor);
            StartCoroutine(FadeIn(answerWire));

            // Esperar un tiempo antes de generar el siguiente cable
            yield return new WaitForSeconds(generationDelay);
        }
    }

    private IEnumerator FadeIn(GameObject wire)
    {
        // Agregar un CanvasGroup si no lo tiene
        CanvasGroup canvasGroup = wire.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = wire.AddComponent<CanvasGroup>();

        // Comenzar en opacidad 0
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Asegurar que la opacidad final sea 1
        canvasGroup.alpha = 1f;
    }

    // Método para desordenar una lista de strings en paralelo
    private void Shuffle(List<string> list1, List<string> list2)
    {
        for (int i = list1.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            // Intercambiar en la primera lista
            string temp1 = list1[i];
            list1[i] = list1[randomIndex];
            list1[randomIndex] = temp1;

            // Intercambiar en la segunda lista
            string temp2 = list2[i];
            list2[i] = list2[randomIndex];
            list2[randomIndex] = temp2;
        }
    }

    // Sobrecarga del método Shuffle para listas de enteros
    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            // Intercambiar en la lista
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
