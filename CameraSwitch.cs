using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour {
    public List<GameObject> cameraList;
    private float Timer;
    private int cameraId;

    void Start () {
        Timer = 0f;
        cameraId = 0;
        cameraList[cameraId].SetActive (true);
    }

    // Update is called once per frame
    void Update () {
        Timer += Time.deltaTime;
        if (Timer > 5f) {
            Timer = 0f;
            cameraList[cameraId].SetActive (false);
            cameraList[cameraId++].SetActive (true);
            if(cameraId == cameraList.Count) {
                cameraId = 0;
            }
        }
    }
}