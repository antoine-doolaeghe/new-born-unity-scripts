using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class TrainingSpawner : MonoBehaviour {
    public int agentNumber;
    public GameObject agent;
    public GameObject camera;
    public Vector3 agentScale;
    public Vector3 groundScale;
    public Vector3 targetPosition;
    public float floorHeight;
    public Academy academy;

    public void Delete () {
        int childCount = transform.childCount;
        academy.broadcastHub.broadcastingBrains.Clear ();
        foreach (Transform child in transform) {
            DestroyImmediate (child.gameObject);
        }
    }

    public void Build () {
        int floor = 0;
        int squarePosition = 0;
        GameObject trainingFloor = new GameObject ();
        GameObject trainingCamera = Instantiate (camera, trainingFloor.transform);
        trainingFloor.name = "Floor" + floor;
        trainingFloor.transform.parent = transform;
        trainingCamera.name = "Camera" + floor;
        for (var i = 0; i < agentNumber; i++) {
            if (i != 0 && i % 4 == 0) {
                floor++;
                squarePosition = 0;
                trainingFloor = new GameObject ();
                trainingFloor.name = "Floor" + floor;
                trainingFloor.transform.parent = transform;
                trainingFloor.transform.localPosition = new Vector3 (0f, floorHeight * floor, 0f);
                GameObject c = Instantiate (camera, trainingFloor.transform);
                c.name = "Camera" + floor;
                c.transform.localPosition = new Vector3 (c.transform.localPosition.x, floorHeight - 30f, c.transform.localPosition.z);
            }

            GameObject a = Instantiate (agent, trainingFloor.transform);
            a.transform.localScale = groundScale;
            a.name = "trainer" + floor + "." + squarePosition;
            a.transform.Find ("Target").localPosition = targetPosition;
            Brain brain = Resources.Load<Brain> ("Brains/agentBrain" + i);
            brain.brainParameters.vectorObservationSize = 3;
            brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
            brain.brainParameters.vectorActionSize = new int[1] {3000};;
            academy.broadcastHub.broadcastingBrains.Add (brain);
            academy.broadcastHub.SetControlled (brain, true);
            a.transform.Find ("NewBorn").gameObject.GetComponent<AgentTrainBehaviour> ().brain = brain;

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