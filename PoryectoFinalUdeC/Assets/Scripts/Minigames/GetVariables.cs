using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVariables : MonoBehaviour
{
    [SerializeField] private InteractiveTextManager interactiveTextManager;
    public List<int> numeros; // Lista de enteros
    public List<GameObject> objetos; // Lista de GameObjects
    public string texto;

    void Awake()
    {
        numeros = new List<int>();
        objetos = new List<GameObject>();

        numeros.Add(10);
        numeros.Add(20);

    }

    void Start()
    {
        
        interactiveTextManager.GenerateInteractiveText(texto);
    }

}

