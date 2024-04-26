using UnityEngine;

public class SheetMovement : MonoBehaviour
{
    private Cloth cloth;
    public float amplitude = 0.5f;
    public float frequency = 1.0f;
    private float baselineY;

    void Start()
    {
        cloth = GetComponent<Cloth>();
        baselineY = transform.position.y;
    }

    void FixedUpdate()
    {
        float y = baselineY + amplitude * Mathf.Sin(Time.time * frequency);
        Vector3 position = new Vector3(transform.position.x, y, transform.position.z);
        Vector3 direction = (position - transform.position).normalized;
        
        cloth.externalAcceleration = direction * 10f;
    }
}