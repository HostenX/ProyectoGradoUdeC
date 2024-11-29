using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVariables : MonoBehaviour
{
    [SerializeField] private InteractiveTextManager interactiveTextManager;
    [SerializeField] private BookGenerator bookGenerator;
    public List<int> numeros; // Lista de enteros
    public string texto;

    void Awake()
    {
        // Inicializar la lista de n�meros
        numeros = new List<int> { 0, 1, 2, 3, 4 }; // Puedes a�adir m�s n�meros
    }

    void Start()
    {
        // Asignar la lista de n�meros al generador de libros
        bookGenerator.numbers = new List<int>(numeros); // Pasar una copia de la lista
        interactiveTextManager.GenerateInteractiveText(texto);
        bookGenerator.GenerateBooks();
    }
}
