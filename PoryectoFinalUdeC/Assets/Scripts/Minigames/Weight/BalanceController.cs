using UnityEngine;

public class BalanceController : MonoBehaviour
{
    private Rigidbody2D rb;
    private HingeJoint2D hingeJoint;
    public float restoringTorque = 10f; // Fuerza para que la balanza vuelva al equilibrio
    public float maxAngle = 30f; // Ángulo máximo permitido en grados (opcional)
    public Transform anchorPoint; // Punto de anclaje dinámico (puede ser vacío o estático)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hingeJoint = GetComponent<HingeJoint2D>();

        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on this object.");
        }

        if (hingeJoint == null)
        {
            Debug.LogError("No HingeJoint2D found on this object.");
        }

        if (anchorPoint != null)
        {
            // Establecer el punto de anclaje inicial
            hingeJoint.connectedAnchor = anchorPoint.position;
        }
        else
        {
            // Si no hay punto de anclaje externo, usar el origen local del objeto
            hingeJoint.connectedAnchor = Vector2.zero;
        }
    }

    private void Update()
    {
        // Verifica si la balanza se ha movido y ajusta el punto de anclaje
        if (anchorPoint != null)
        {
            hingeJoint.connectedAnchor = anchorPoint.position;
        }
    }

    private void FixedUpdate()
    {
        // Calcula el ángulo actual de la balanza
        float angle = transform.rotation.eulerAngles.z;
        if (angle > 180) angle -= 360; // Convertir a rango [-180, 180]

        // Aplica un torque restaurador proporcional al ángulo
        rb.AddTorque(-angle * restoringTorque);

        // Opcional: Limitar el ángulo máximo
        if (Mathf.Abs(angle) > maxAngle)
        {
            float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            transform.rotation = Quaternion.Euler(0, 0, clampedAngle);
        }
    }
}
