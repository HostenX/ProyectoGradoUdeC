using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    private Queue<string> sentences;

    void Start()
    {
        sentences = new Queue<string>();
        dialogPanel.SetActive(false);
    }

    public void StartDialog(string[] dialogLines)
    {
        dialogPanel.SetActive(true);
        sentences.Clear();

        foreach (string sentence in dialogLines)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialog();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogText.text = sentence;
    }

    public void EndDialog()
    {
        dialogPanel.SetActive(false);
    }
}
