using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{       
    public float speed = 1f;

    private float timeCounter = 0;
    private float objectOriginX;
    private float objectOriginY;
    // Start is called before the first frame update
    void Start()
    {
        objectOriginX = transform.position.x;
        objectOriginY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter += Time.deltaTime * speed;

        float x = Mathf.Cos(timeCounter);
        float y = Mathf.Sin(timeCounter);
        float z = 0;

        transform.position = new Vector3(objectOriginX + x, objectOriginY + y, z);
    }
}
