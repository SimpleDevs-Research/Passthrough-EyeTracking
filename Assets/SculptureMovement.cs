using UnityEngine;

public class SculptureMovement : MonoBehaviour
{
    private Vector3 pointB;
    private Vector3 pointC;
    private Quaternion rotationB;
    private Quaternion rotationC;
    public float speed = 1.0f;

    private Vector3 targetPoint;
    private Quaternion targetRotation;
    private Vector3 initialPos;
    private Quaternion initialRot;
    private float time = 0.0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPos = transform.localPosition;
        initialRot = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);

        pointB = initialPos - new Vector3(0, 0, 400f);
        pointC = initialPos + new Vector3(0, 0, 500f);

        rotationB = Quaternion.Euler(transform.localEulerAngles.x - 35.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        rotationC = Quaternion.Euler(transform.localEulerAngles.x + 35.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);

        targetPoint = pointB;
        targetRotation = rotationB;
    }

    void Update()
    {
        if (Vector3.Distance(rb.position, targetPoint) <= 0.001f)
        {
            if (targetPoint == pointB)
            {
                targetPoint = pointC;
                targetRotation = rotationC;
            }
            else
            {
                targetPoint = pointB;
                targetRotation = rotationB;
            }
            time = 0.0f;
        }

        time += Time.deltaTime * speed;
        Vector3 newPosition = Vector3.Lerp(rb.position, targetPoint, Mathf.PingPong(time, 1));
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, Mathf.PingPong(time, 1));

        rb.MovePosition(newPosition);
        rb.MoveRotation(newRotation);
    }
}