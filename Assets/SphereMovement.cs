using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMovement : MonoBehaviour
{
    private Vector3 pointA;
    private Vector3 pointB = new Vector3(-2204f, 202f, -2960f);
    public float speed;
    private Rigidbody rb;
    private Vector3 currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentTarget = pointB;
        pointA = transform.localPosition;
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.localPosition, currentTarget) <= 200.0f)
        {
            if (currentTarget == pointB) {
                currentTarget = pointA;
            }
            else {
                currentTarget = pointB;
            }
        }
        Vector3 direction = (currentTarget - transform.localPosition).normalized;
        rb.AddForce(direction * speed);
    }
}
