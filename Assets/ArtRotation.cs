using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtRotation : MonoBehaviour
{
    private float x;
    private float rotationSpeed;
    private float y;
    private float z;
    // Start is called before the first frame update
    void Start()
    {
        x = transform.localEulerAngles.x;
        rotationSpeed = 45.0f;   
        y = transform.localEulerAngles.y;
        z = transform.localEulerAngles.z;
        
    }

    void FixedUpdate()
    {
        x += Time.deltaTime * rotationSpeed;

        transform.localRotation = Quaternion.Euler(x, y, z);
    }
}
