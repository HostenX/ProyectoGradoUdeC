using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquationGenerator : MonoBehaviour
{
    public string equationTemplate;
    public int correctAnswer;

    public void GenerateEquation()
    {
        // Generar operandos y operador aleatorios
        int operand1 = Random.Range(1, 10);
        int operand2 = Random.Range(1, 10);
        string operatorSymbol = "+";

        // Calcular el resultado según el operador
        correctAnswer = operand1 + operand2;

        // Crear la ecuación con un espacio vacío para completar
        equationTemplate = $"{operand1} {operatorSymbol} ? = {correctAnswer}";
        Debug.Log($"Nueva ecuación: {equationTemplate}");
    }
}
