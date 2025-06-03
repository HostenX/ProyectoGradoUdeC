using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;

public class TeacherListManager : MonoBehaviour
{
    public Transform contentPanel; // Panel donde se instanciarán los botones
    public GameObject teacherButtonPrefab; // Prefab del botón
    private string apiUrl = "https://gamificationudecapi.azurewebsites.net/api/Usuario/teachers-names";

    void Start()
    {
        StartCoroutine(GetTeachers());
    }

    IEnumerator GetTeachers()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("token"));
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<TeacherResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    foreach (var teacher in response.teachers)
                    {
                        GameObject buttonObj = Instantiate(teacherButtonPrefab, contentPanel);
                        buttonObj.GetComponentInChildren<TMP_Text>().text = teacher.NombreCompleto;
                        buttonObj.GetComponent<Button>().onClick.AddListener(() => OnTeacherSelected(teacher.UsuarioId));
                    }
                }
            }
            else
            {
                Debug.LogError("Error al obtener docentes: " + request.error);
            }
        }
    }

    void OnTeacherSelected(int teacherId)
    {
        PlayerPrefs.SetInt("selectedTeacherId", teacherId);
        SceneManager.LoadScene("BasementMain");
    }

    [System.Serializable]
    private class TeacherResponse
    {
        public bool success;
        public List<TeacherData> teachers;
    }

    [System.Serializable]
    private class TeacherData
    {
        public int UsuarioId;
        public string NombreCompleto;
    }
}
