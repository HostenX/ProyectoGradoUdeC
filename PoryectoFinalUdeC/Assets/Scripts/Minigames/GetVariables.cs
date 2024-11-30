using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class GetVariables : MonoBehaviour
{
    [SerializeField] private InteractiveTextManager interactiveTextManager;
    [SerializeField] private BookGenerator bookGenerator;

    public List<string> numeros; // Lista de strings
    public string texto; // Texto de la pregunta

    private string apiUrl = "https://localhost:7193/api/Minijuego/GetMinijuegosFiltrados"; // Cambia esto por tu URL real de la API

    void Start()
    {
        // Llamar al método para obtener los datos de la API
        StartCoroutine(GetMinijuegoData("3A", 1, 1, "Ecuacion"));
    }

    // Método para hacer la llamada a la API y obtener el minijuego
    IEnumerator GetMinijuegoData(string curso, int usuarioCreadorId, int estadoId, string tipoMinijuego)
    {
        // Construir la URL con los parámetros de consulta
        string url = $"{apiUrl}?curso={curso}&usuarioCreadorId={usuarioCreadorId}&estadoId={estadoId}&tipoMinijuego={tipoMinijuego}";

        // Crear la solicitud
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Esperar la respuesta de la API
        yield return request.SendWebRequest();

        // Verificar si hubo un error en la solicitud
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener datos de la API: " + request.error);
            yield break;
        }

        // Procesar la respuesta JSON
        string jsonResponse = request.downloadHandler.text;
        var minijuegos = JsonHelper.FromJson<Minijuego>(jsonResponse);

        if (minijuegos != null && minijuegos.Length > 0)
        {
            // Extraer los valores de la respuesta y pregunta
            string valoresRespuesta = minijuegos[0].valoresRespuesta;
            string valoresPregunta = minijuegos[0].valoresPregunta;

            // Asignar los valores a las variables de Unity
            texto = valoresPregunta;

            // Convertir los valores de respuesta en una lista de strings (sin necesidad de conversión a enteros)
            numeros = valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();  // Lista de strings

            // Asignar la lista de números al generador de libros
            bookGenerator.numbers = new List<string>(numeros);

            // Llamar a los métodos de gestión de texto y generación de libros
            interactiveTextManager.GenerateInteractiveText(texto);
            bookGenerator.GenerateBooks();
        }
        else
        {
            Debug.LogWarning("No se encontró ningún minijuego con los filtros especificados.");
        }
    }
}

// Clases para deserializar el JSON
[System.Serializable]
public class Minijuego
{
    public int minijuegoId;
    public string titulo;
    public string descripcion;
    public int tematicoId;
    public int usuarioCreadorId;
    public int estadoId;
    public int penalidadPuntos;
    public int? intentosPermitidos;
    public int? tiempoMinimo;
    public string puntosBase;
    public string valoresPregunta;
    public string valoresRespuesta;
    public string respuestaCorrecta;
    public string cursoMinijuego;
    public string tipoMinijuego;
    public object estado;
    public object tematico;
    public object usuarioCreador;
}

// Helper para deserializar un arreglo de JSON
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        json = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
