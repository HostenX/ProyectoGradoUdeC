using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class InteractableObject : MonoBehaviour
{
    public string colorJsonPath = "Assets/Scripts/JSON/colorData.json";
    public string logJsonPath = "Assets/Scripts/JSON/interactionLog.json";

    public void Interact()
    {
        ChangeColorFromJson();
        LogInteractionToJson();
    }

    void ChangeColorFromJson()
    {
        if (File.Exists(colorJsonPath))
        {
            // Leer el archivo JSON
            string jsonData = File.ReadAllText(colorJsonPath);
            ColorData colorData = JsonConvert.DeserializeObject<ColorData>(jsonData);

            // Convertir el color hexadecimal a Color de Unity
            if (ColorUtility.TryParseHtmlString(colorData.color, out Color newColor))
            {
                GetComponent<SpriteRenderer>().color = newColor;
                Debug.Log("Color cambiado a: " + newColor);
            }
        }
        else
        {
            Debug.LogError("Archivo JSON con color no encontrado en " + colorJsonPath);
        }
    }

    void LogInteractionToJson()
    {
        InteractionLog interactionLog;

        if (File.Exists(logJsonPath))
        {
            // Leer el archivo JSON existente
            string logData = File.ReadAllText(logJsonPath);
            interactionLog = JsonConvert.DeserializeObject<InteractionLog>(logData);
        }
        else
        {
            // Si no existe, crear uno nuevo
            interactionLog = new InteractionLog();
        }

        // Añadir una nueva entrada con la fecha y hora actuales
        interactionLog.interactionLogs.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        // Guardar los cambios en el archivo JSON
        string newLogData = JsonConvert.SerializeObject(interactionLog, Formatting.Indented);
        File.WriteAllText(logJsonPath, newLogData);

        Debug.Log("Interacción registrada en JSON en " + logJsonPath);
    }
}

// Definición de la clase ColorData
[Serializable]
public class ColorData
{
    public string color;
}

// Definición de la clase InteractionLog
[Serializable]
public class InteractionLog
{
    public List<string> interactionLogs = new List<string>();
}
