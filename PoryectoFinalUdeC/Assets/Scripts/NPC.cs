using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NPC : MonoBehaviour
{
    public Sprite[] npcSprites;
    private SpriteRenderer spriteRenderer;
    private DialogManager dialogManager;
    private string apiUrl = "https://localhost:7193/api/Apoyo/random";
    private string[] dialogLines;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (npcSprites.Length > 0)
        {
            spriteRenderer.sprite = npcSprites[Random.Range(0, npcSprites.Length)];
        }
        else
        {
            Debug.LogWarning("No hay sprites asignados al NPC.");
        }

        FlipSpriteRandomly();
        dialogManager = FindObjectOfType<DialogManager>();
    }

    void FlipSpriteRandomly()
    {
        if (Random.value > 0.5f)
        {
            spriteRenderer.flipX = true;
        }
    }

    public void StartDialog()
    {
        StartCoroutine(GetDialogFromAPI());
    }

    IEnumerator GetDialogFromAPI()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string dialogText = request.downloadHandler.text;

            // Separar el texto por comas y limpiar espacios
            dialogLines = dialogText.Split(',');
            for (int i = 0; i < dialogLines.Length; i++)
            {
                dialogLines[i] = dialogLines[i].Trim();
            }

            // Esperar a que la API termine antes de iniciar el diálogo
            if (dialogManager != null && dialogLines.Length > 0)
            {
                dialogManager.StartDialog(dialogLines);
            }
            else
            {
                Debug.LogWarning("No hay diálogo disponible para este NPC.");
            }
        }
        else
        {
            Debug.LogError("Error al obtener el diálogo: " + request.error);
        }
    }
}
