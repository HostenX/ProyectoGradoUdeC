using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class GetVariables : MonoBehaviour
{
    [SerializeField] private InteractiveTextManager interactiveTextManager;
    [SerializeField] private BookGenerator bookGenerator;
    [SerializeField] private BagSpawner bagSpawner;
    [SerializeField] private WireGenerator wireGenerator;

    public int minijuegoId;
    public List<string> opciones;
    public string pregunta;
    public List<string> listaPreguntas;
    public List<string> listaRespuestas;
    public List<float> pesos;
    public string respuestaCorrecta;

    public string Curso;
    public int UsuarioCreador;
    public string tipoMinijuego;

    public int puntosBase;
    public int penalidadPuntos;
    public int puntajeActual;

    private string apiUrl = "https://localhost:7193/api/Minijuego/GetMinijuegosFiltrados";

    void Start()
    {
        // Buscar PlayerData en la escena
        PlayerData playerData = FindObjectOfType<PlayerData>();

        if (playerData != null)
        {
            Curso = playerData.Curso;
            UsuarioCreador = playerData.TeacherId;

        }
        else
        {
            Debug.LogError("No se encontró PlayerData en la escena.");
            return;
        }

        StartCoroutine(GetMinijuegoData(Curso, UsuarioCreador, 1, tipoMinijuego));
    }
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
            var minijuego = minijuegos[0];
            minijuegoId = minijuego.minijuegoId;
            puntosBase = int.Parse(minijuego.puntosBase);
            penalidadPuntos = minijuego.penalidadPuntos;
            puntajeActual = puntosBase;
            Debug.Log($"Minijuego ID: {minijuegoId}, Puntos base: {puntosBase}, Penalidad: {penalidadPuntos}");

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

    void ConfigurarEcuacion(Minijuego minijuego)
    {
        pregunta = minijuego.valoresPregunta;
        opciones = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();
        respuestaCorrecta = minijuego.respuestaCorrecta;
        interactiveTextManager.GenerateInteractiveText(pregunta);
        bookGenerator.numbers = new List<string>(opciones);
        bookGenerator.GenerateBooks();
    }

    void ConfigurarBalanza(Minijuego minijuego)
    {
        opciones = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();
        bagSpawner.weights = opciones.Select(s => float.Parse(s)).ToList();
        bagSpawner.GenerateBags();
    }

    void ConfigurarFrase(Minijuego minijuego)
    {
        pregunta = minijuego.valoresPregunta;
        opciones = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();
        respuestaCorrecta = minijuego.respuestaCorrecta;
        interactiveTextManager.GenerateInteractiveText(pregunta);
        bookGenerator.numbers = new List<string>(opciones);
        bookGenerator.GenerateBooks();
    }

    void ConfigurarConectarValores(Minijuego minijuego)
    {
        listaPreguntas = minijuego.valoresPregunta.Split(',').Select(s => s.Trim()).ToList();
        listaRespuestas = minijuego.valoresRespuesta.Split(',').Select(s => s.Trim()).ToList();
        if (listaPreguntas.Count != listaRespuestas.Count)
        {
            Debug.LogWarning("Las listas de preguntas y respuestas no coinciden en tamaño.");
            return;
        }
        wireGenerator.GenerateWires(listaPreguntas, listaRespuestas);
    }

    public void RegistrarResultado(bool esCorrecto)
    {
        if (esCorrecto)
        {
            Debug.Log("¡Respuesta correcta! Se mantienen los puntos: " + puntajeActual);
        }
        else
        {
            puntajeActual -= penalidadPuntos;
            if (puntajeActual < 0)
                puntajeActual = 0;
            Debug.Log("Respuesta incorrecta. Puntos restantes: " + puntajeActual);
        }
    }
}

[JsonObject]
public class Minijuego
{
    [JsonProperty("minijuegoId")] public int minijuegoId;
    [JsonProperty("titulo")] public string titulo;
    [JsonProperty("descripcion")] public string descripcion;
    [JsonProperty("tematicoId")] public int tematicoId;
    [JsonProperty("usuarioCreadorId")] public int usuarioCreadorId;
    [JsonProperty("estadoId")] public int estadoId;
    [JsonProperty("penalidadPuntos")] public int penalidadPuntos;
    [JsonProperty("intentosPermitidos")] public int? intentosPermitidos;
    [JsonProperty("tiempoMinimo")] public int? tiempoMinimo;
    [JsonProperty("puntosBase")] public string puntosBase;
    [JsonProperty("valoresPregunta")] public string valoresPregunta;
    [JsonProperty("valoresRespuesta")] public string valoresRespuesta;
    [JsonProperty("respuestaCorrecta")] public string respuestaCorrecta;
    [JsonProperty("cursoMinijuego")] public string cursoMinijuego;
    [JsonProperty("tipoMinijuego")] public string tipoMinijuego;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            JArray valuesArray = jsonObject["$values"] as JArray;
            return valuesArray?.ToObject<T[]>();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deserializando JSON: " + e.Message);
            return null;
        }
    }
}
