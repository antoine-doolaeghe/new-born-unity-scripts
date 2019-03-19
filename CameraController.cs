using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0,6f*Time.deltaTime,0);
    }
}
