using System.Collections;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public int UsuarioId;
    public string Curso;
    public int RolId;
    public string NombreUsuario;
    public int TeacherId;
    public int PuntajeActual;

    public void AgregarPuntaje(int puntajeObtenido)
    {
        PuntajeActual += puntajeObtenido;
    }
}
