using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System;

public class Wire : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _image;
    private LineRenderer _lineRenderer;
    private Canvas _canvas;
    private Camera _gameCamera; // Variable para la c�mara personalizada
    private bool _isDragging = false;

    public bool IsLeft;
    public bool IsQuestion; // Indica si es un cable de pregunta o respuesta

    public string QuestionText { get; private set; }
    public string AnswerText { get; private set; }

    

    public void Initialize(string question, string answer, Color wireColor)
    {
        QuestionText = question;
        AnswerText = answer;
        SetColor(wireColor);
        UpdateText();
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _lineRenderer = GetComponent<LineRenderer>();
        _canvas = GetComponentInParent<Canvas>();
        _gameCamera = GameObject.FindWithTag("MinigameCamera").GetComponent<Camera>(); // Asignar la c�mara

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;
        }
    }

    public void SetColor(Color color)
    {
        if (_image != null)
        {
            _image.color = color;
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }

    private void UpdateText()
    {
        // Busca un componente hijo con TextMeshPro
        TextMeshPro textComponent = GetComponentInChildren<TextMeshPro>();

        if (textComponent != null)
        {
            // Cambia el texto seg�n el valor de IsQuestion
            if (IsQuestion)
            {
                textComponent.text = QuestionText;
            }
            else
            {
                textComponent.text = AnswerText;
            }
        }
        else
        {
            Debug.LogWarning("No se encontr� un componente TextMeshProUGUI en los hijos del Wire.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_lineRenderer != null && _canvas != null)
        {
            if (!IsLeft) // Verificar si la l�nea est� a la izquierda
            {
                Debug.Log("No se puede arrastrar esta l�nea.");
                return; // Si la l�nea no est� a la izquierda, no permitir el arrastre
            }
            _isDragging = true;
            _lineRenderer.enabled = true;

            Vector3 startPosition = _gameCamera.ScreenToWorldPoint(eventData.position);
            startPosition.z = 90f; // Ajustar la posici�n Z

            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 localPosition = rectTransform.position;
            startPosition = localPosition;

            _lineRenderer.SetPosition(0, startPosition); // Punto inicial
            _lineRenderer.SetPosition(1, startPosition); // Punto final (inicializado igual al inicial)
            Debug.Log($"Inicio de l�nea en: {startPosition}");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging && _lineRenderer != null)
        {
            Vector3 currentPosition = _gameCamera.ScreenToWorldPoint(eventData.position);
            currentPosition.z = 90f;

            _lineRenderer.SetPosition(1, currentPosition);
            Debug.Log($"Actualizando l�nea: {currentPosition}");
        }
    }

    public delegate void WireConnectedHandler(Wire wire, GameObject connectedObject);

    // En el m�todo OnEndDrag
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDragging && _lineRenderer != null)
        {
            _isDragging = false;

            Vector3 endPosition = _gameCamera.ScreenToWorldPoint(eventData.position);
            endPosition.z = 90f;

            _lineRenderer.SetPosition(1, endPosition);

            // Detectar conexi�n con otro objeto
            GameObject connectedObject = DetectConnection(endPosition);

            // Obtener referencia al MinigameController
            MinigameController minigameController = FindObjectOfType<MinigameController>();

            if (connectedObject != null)
            {
                // Verificar si la conexi�n es correcta
                Wire connectedWire = connectedObject.GetComponent<Wire>();
                if (connectedWire != null)
                {
                    if (IsConnectionValid(connectedWire))
                    {
                        Debug.Log("Conexi�n correcta.");

                        // Notificar al controlador
                        if (minigameController != null)
                        {
                            minigameController.wireConnections[this] = connectedObject; // Agregar al diccionario
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Conexi�n incorrecta. Int�ntalo de nuevo.");
                        // Deshabilitar la l�nea
                        _lineRenderer.enabled = false;

                        // Llamar al proceso de respuesta incorrecta en el MinigameController
                        if (minigameController != null)
                        {
                            minigameController.HandleIncorrectConnection();
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No se detect� conexi�n.");
                _lineRenderer.enabled = false; // Deshabilitar la l�nea

                // Tambi�n considerar esto como un intento fallido
                if (minigameController != null)
                {
                    minigameController.HandleIncorrectConnection();
                }
            }
        }
    }
    private bool IsConnectionValid(Wire connectedWire)
    {
        
        
        if (IsQuestion && !connectedWire.IsQuestion)
        {
            Debug.Log("Conectado�: " + QuestionText + "=" + connectedWire.AnswerText);
            return string.Equals(AnswerText, connectedWire.AnswerText, StringComparison.OrdinalIgnoreCase);
        }

        return false; // Si ambos son preguntas o respuestas, no es v�lido
    }



    private GameObject DetectConnection(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        return null;
    }


}
