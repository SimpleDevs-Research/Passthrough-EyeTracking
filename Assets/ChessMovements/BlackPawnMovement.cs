using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackPawnMovement : MonoBehaviour
{
    private Vector3 relativeOffset = new Vector3(0.123f, 0, 0);
    public float speed = 1.0f;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        startPoint = transform.localPosition;
        endPoint = startPoint + relativeOffset;
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime * speed;
        transform.localPosition = Vector3.Lerp(startPoint, endPoint, Mathf.PingPong(time, 1));
    }
}
