using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Components.Camera
{
    public class CameraSwitch : MonoBehaviour {
    public TextMeshProUGUI textMeshProUGUI;
    public List<GameObject> cameraList;
    private float Timer;
    private int cameraId;

    void Start () {
        Timer = 0f;
        cameraId = 0;
        cameraList[cameraId].SetActive (true);
        textMeshProUGUI.text = cameraList[cameraId].transform.parent.name;
    }

    void Update () {
        Timer += Time.deltaTime;
        if (Timer > 5f) {
            Timer = 0f;
            cameraList[cameraId].SetActive (false);
            textMeshProUGUI.text = cameraList[cameraId].transform.parent.name;
            cameraList[cameraId++].SetActive (true);
            if (cameraId == cameraList.Count) {
                cameraId = 0;
            }
        }
    }
}   
}