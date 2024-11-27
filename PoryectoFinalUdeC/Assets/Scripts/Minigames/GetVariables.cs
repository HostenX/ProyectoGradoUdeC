using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVariables : MonoBehaviour
{
    public List<int> numeros; // Lista de enteros
    public List<GameObject> objetos; // Lista de GameObjects

    void Awake()
    {
        numeros = new List<int>();
        objetos = new List<GameObject>();

        numeros.Add(10);
        numeros.Add(20);

    }

}

