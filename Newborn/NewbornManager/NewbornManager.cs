using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MLAgents;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Newborn
{
  [ExecuteInEditMode]
  public class NewbornManager : MonoBehaviour
  {
    [Header("Environment Mode")]
    public bool isTrainingMode;
    [Header("Environment parameters")]
    public int spawnerNumber;
    public GameObject TrainingPrefab;
    [ConditionalField("isTrainingMode")] public Vector3 agentScale;
    [ConditionalField("isTrainingMode")] public Vector3 groundScale;
    [ConditionalField("isTrainingMode")] public float floorHeight;
    [Header("Agent parameters")]
    public bool requestApiData;
    public string newbornId;

    [Header("Academy parameters")]
    public Academy academy;
    [Header("Brain parameters")]
    public Brain brainObject;
    public int vectorObservationSize;
    public int vectorActionSize;
    public TextAsset brainModel;
    [Header("Camera parameters")]

    [HideInInspector] public List<GameObject> Spawners = new List<GameObject>();
    public void DeleteSpawner()
    {
      Transform[] childs = transform.Cast<Transform>().ToArray();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
      Spawners.Clear();
      academy.broadcastHub.broadcastingBrains.Clear();
    }

    public void BuildSpawners()
    {
      GameObject parent = transform.gameObject;
      int floor = 0;
      int squarePosition = 0;

      for (var i = 0; i < spawnerNumber; i++)
      {
        GameObject spawner;
        Brain brain = Instantiate(brainObject);

        if (isTrainingMode && i % 4 == 0)
        {
          floor++;
          squarePosition = 0;
          parent = CreateTrainingFloor(floor);
          NameFloor(parent, floor);
        }

        Spawners.Add(InstantiateSpawner(parent, floor, squarePosition, out spawner));

        if (isTrainingMode)
        {
          PositionTrainingSpawner(squarePosition, spawner);
        }

        spawner.GetComponent<NewbornSpawner>().BuildAgents(requestApiData);
        spawner.GetComponent<NewbornSpawner>().BuildTarget();

        squarePosition++;
      }
    }

    public IEnumerator RequestNewbornAgentInfo()
    {
      Debug.Log("Request Agent info from server...");
      GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
      for (int a = 0; a < agents.Length; a++)
      {
        yield return StartCoroutine(NewbornService.GetNewborn(newbornId, agents[a], false));
      }
      Debug.Log("Finished to build Agents");
      academy.InitializeEnvironment();
      academy.initialized = true;
    }

    public void RequestNewborn()
    {
      StartCoroutine(RequestNewbornAgentInfo());
    }

    public void RequestProductionAgentInfo()
    {
      foreach (GameObject agent in GameObject.FindGameObjectsWithTag("agent"))
      {
        Debug.Log(agent.name);
      }
    }

    private GameObject CreateTrainingFloor(int floor)
    {
      GameObject trainingFloor = new GameObject();
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
      trainingFloor.transform.localPosition = new Vector3(0f, floorHeight * floor, 0f);
      return trainingFloor;
    }

    private static void PositionTrainingSpawner(int squarePosition, GameObject spawner)
    {
      Transform spawnerTransform = spawner.transform;
      Vector3 spawnerTransformGroundScale = spawnerTransform.Find("Ground").transform.localScale;
      switch (squarePosition)
      {
        case 0:
          spawnerTransform.localPosition = new Vector3(0f, 0f, 0f);
          break;
        case 1:
          spawnerTransform.localPosition = new Vector3(spawnerTransformGroundScale.x, 0f, 0f);
          break;
        case 2:
          spawnerTransform.localPosition = new Vector3(0f, 0f, spawnerTransformGroundScale.z);
          break;
        case 3:
          spawnerTransform.localPosition = new Vector3(spawnerTransformGroundScale.x, 0f, spawnerTransformGroundScale.z);
          break;
      }
    }

    private GameObject InstantiateSpawner(GameObject parent, int floor, int squarePosition, out GameObject spawner)
    {
      spawner = Instantiate(TrainingPrefab, parent.transform);
      spawner.name = ("Spawner" + squarePosition);
      spawner.transform.localScale = groundScale;
      return spawner;
    }

    private void NameFloor(GameObject trainingFloor, int floor)
    {
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
    }

    public void ClearBroadCastingBrains()
    {
      academy.broadcastHub.broadcastingBrains.Clear();
    }
  }
}