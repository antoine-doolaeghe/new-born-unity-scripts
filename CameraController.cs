using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, 1f * Time.deltaTime, Space.Self);
    }
}
