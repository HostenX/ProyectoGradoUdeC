using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class GetVariables : MonoBehaviour
{
    [SerializeField] private InteractiveTextManager interactiveTextManager;
    [SerializeField] private BookGenerator bookGenerator;
    [SerializeField] private BagSpawner bagSwner;
    [SerializeField] private WireGenerator wireGenerator;

    public List<string> opciones; // Opciones de respuesta
    public string pregunta; // Texto de la pregunta principal
    public List<string> listaPreguntas; // Lista de preguntas (para conectar valores)
    public List<string> listaRespuestas; // Lista de respuestas (para conectar valores)
    public List<float> pesos;
    public string respuestaCorrecta; // Respuesta correcta en caso de minijuegos directos

    public string Curso;
    public int UsuarioCreador;
    public string tipoMinijuego;

    private string apiUrl = "https://localhost:7193/api/Minijuego/GetMinijuegosFiltrados"; // Cambia esto por tu URL real de la API

    void Start()
    {
        // Llamar al método para obtener los datos de la API
        StartCoroutine(GetMinijuegoData(Curso, UsuarioCreador, 1, tipoMinijuego));
    }

    // Método para hacer la llamada a la API y obtener el minijuego
    IEnumerator GetMinijuegoData(string curso, int usuarioCreadorId, int estadoId, string tipoMinijuego)
    {
        string url = $"{apiUrl}?curso={curso}&usuarioCreadorId={usuarioCreadorId}&estadoId={estadoId}&tipoMinijuego={tipoMinijuego}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener datos de la API: " + request.error);
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        var minijuegos = JsonHelper.FromJson<Minijuego>(jsonResponse);

        if (minijuegos != null && minijuegos.Length > 0)
        {
            // Obtener el primer minijuego
            var minijuego = minijuegos[0];

            // Usar un switch para interpretar los datos según el tipo de minijuego
            switch (tipoMinijuego.ToLower())
            {
                case "ecuacion":
                    ConfigurarEcuacion(minijuego);
                    break;
                case "frase":
                    ConfigurarFrase(minijuego);
                    break;

                case "conectar":
                    ConfigurarConectarValores(minijuego);
                    break;
                case "balanza":
                    ConfigurarBalanza(minijuego);
                    break;

                default:
                    Debug.LogWarning("Tipo de minijuego no reconocido: " + tipoMinijuego);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("No se encontró ningún minijuego con los filtros especificados.");
        }
    }

    // Configuración específica para el minijuego tipo "Ecuación"
    void ConfigurarEcuacion(Minijuego minijuego)
    {
        pregunta = minijuego.valoresPregunta;
        opciones = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();
        respuestaCorrecta = minijuego.respuestaCorrecta;

        // Actualizar el UI y generar los libros
        interactiveTextManager.GenerateInteractiveEquation(pregunta);
        bookGenerator.numbers = new List<string>(opciones);
        bookGenerator.GenerateBooks();
    }
    void ConfigurarBalanza(Minijuego minijuego)
    {
        // Dividir y limpiar los valores de respuesta
        opciones = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();

        // Convertir las opciones de string a float y asignarlas a la lista de weights
        bagSwner.weights = opciones.Select(s => float.Parse(s)).ToList();

        // Generar las bolsas o elementos en la balanza
        bagSwner.GenerateBags();
    }

    void ConfigurarFrase (Minijuego minijuego)
    {
        pregunta = minijuego.valoresPregunta;
        opciones = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();
        respuestaCorrecta = minijuego.respuestaCorrecta;

        // Actualizar el UI y generar los libros
        interactiveTextManager.GenerateInteractiveText(pregunta);
        bookGenerator.numbers = new List<string>(opciones);
        bookGenerator.GenerateBooks();
    }

    // Configuración específica para el minijuego tipo "Conectar"
    void ConfigurarConectarValores(Minijuego minijuego)
    {
        listaPreguntas = minijuego.valoresPregunta.Split(',').Select(s => s.Trim()).ToList();
        listaRespuestas = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();

        // Validar si ambas listas tienen el mismo tamaño
        if (listaPreguntas.Count != listaRespuestas.Count)
        {
            Debug.LogWarning("Las listas de preguntas y respuestas no coinciden en tamaño.");
            return;

            
        }

        wireGenerator.GenerateWires(listaPreguntas, listaRespuestas);

        // Aquí puedes implementar lógica para UI específica, como generar elementos para conectar
        // Por ejemplo:
        foreach (var pregunta in listaPreguntas)
        {
            Debug.Log("Pregunta: " + pregunta);
        }
        foreach (var respuesta in listaRespuestas)
        {
            Debug.Log("Respuesta: " + respuesta);
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
