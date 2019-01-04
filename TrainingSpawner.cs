using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class TrainingSpawner : MonoBehaviour {
    public int agentNumber;
    public GameObject agent;
    public GameObject camera;
    [Header ("Environment parameters")]
    public Vector3 agentScale;
    public Vector3 groundScale;
    public Vector3 targetPosition;
    public float floorHeight;
    [Header ("Academy parameters")]
    public Academy academy;
    public bool control;
    [Header ("Brain parameters")]
    public int vectorObservationSize;
    public int vectorActionSize;

    public void Delete () {
        int childCount = transform.childCount;
        academy.broadcastHub.broadcastingBrains.Clear ();
        foreach (Transform child in transform) {
            DestroyImmediate (child.gameObject);
        }
        transform.GetComponent<CameraSwitch>().cameraList.Clear();

    }

    public void Build () {
        int floor = 0;
        int squarePosition = 0;
        GameObject trainingFloor = new GameObject ();
        GameObject trainingCamera = Instantiate (camera, trainingFloor.transform);

        // ENVIRONMENT PARAMETERS
        trainingFloor.name = "Floor" + floor;
        trainingFloor.transform.parent = transform;
        trainingCamera.name = "Camera" + floor;

        for (var i = 0; i < agentNumber; i++) {
            if (i != 0 && i % 4 == 0) {
                floor++;
                squarePosition = 0;
                // FLOOR PARAMETERS
                trainingFloor = new GameObject ();
                trainingFloor.name = "Floor" + floor;
                trainingFloor.transform.parent = transform;
                trainingFloor.transform.localPosition = new Vector3 (0f, floorHeight * floor, 0f);
                // INSTANTIATE FLOOR CAMERA
                GameObject cam = Instantiate (camera, trainingFloor.transform);
                transform.GetComponent<CameraSwitch> ().cameraList.Add (cam);
                cam.name = "Camera" + floor;
                cam.SetActive(false);
                cam.transform.localPosition = new Vector3 (cam.transform.localPosition.x, floorHeight - 30f, cam.transform.localPosition.z);
            }

            GameObject a = Instantiate (agent, trainingFloor.transform);
            GameObject newBorn = a.transform.Find ("NewBorn").gameObject;
            a.transform.localScale = groundScale;
            a.name = "trainer" + floor + "." + squarePosition;
            a.transform.Find ("Target").localPosition = targetPosition;
            // ADD BRAIN PARAMETERS
            Brain brain = Resources.Load<Brain> ("Brains/agentBrain" + i);
            brain.name = "NewBorn" + System.Guid.NewGuid ();
            brain.brainParameters.vectorObservationSize = vectorObservationSize;
            brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
            brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
            academy.broadcastHub.broadcastingBrains.Add (brain);
            academy.broadcastHub.SetControlled (brain, control);
            newBorn.transform.GetComponent<AgentTrainBehaviour> ().brain = brain;

            switch (squarePosition) {
                case 0:
                    a.transform.localPosition = new Vector3 (0f, 0f, 0f);
                    break;
                case 1:
                    a.transform.localPosition = new Vector3 (a.transform.Find ("Ground").transform.localScale.x, 0f, 0f);
                    break;
                case 2:
                    a.transform.localPosition = new Vector3 (0f, 0f, a.transform.Find ("Ground").transform.localScale.z);
                    break;
                case 3:
                    a.transform.localPosition = new Vector3 (a.transform.Find ("Ground").transform.localScale.x, 0f, a.transform.Find ("Ground").transform.localScale.z);
                    break;
            }

            squarePosition++;
        }
    }
}