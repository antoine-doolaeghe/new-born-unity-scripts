using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEngine;

namespace Gene
{
  public class TrainingManager : MonoBehaviour
  {
    [Header("Environment parameters")]
    public int agentTrainerNumber;
    public int agentNumber;
    public GameObject camera;
    public GameObject TrainingPrefab;
    public Vector3 agentScale;
    public Vector3 groundScale;
    public float floorHeight;
    [Header("Agent parameters")]
    public GameObject AgentPrefab;
    public GameObject CellPrefab;
    public int minCellNb;
    public bool requestApiData;
    public string cellId;
    public AgentConfig agentConfig;
    public float randomPositionIndex;
    [Header("Target parameters")]
    public GameObject StaticTarget;
    public bool isTargetDynamic;
    public Vector3 targetPosition;
    [Header("Academy parameters")]
    public Academy academy;
    public bool control;
    [Header("Brain parameters")]
    public int vectorObservationSize;
    public int vectorActionSize;
    public TextAsset brainModel;
    [Header("Camera parameters")]
    public float fieldOfView;

    [HideInInspector] public List<GameObject> Agents = new List<GameObject>();

    public void Delete()
    {
      Transform[] childs = transform.Cast<Transform>().ToArray();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
      Agents.Clear();
      academy.broadcastHub.broadcastingBrains.Clear();
    }

    public void BuildAgents()
    {
      GameObject trainingFloor = new GameObject();
      int floor = 0;
      int squarePosition = 0;

      NameTrainingEnv(trainingFloor, floor);

      for (var i = 0; i < agentTrainerNumber; i++)
      {
        List<GameObject> newBornAgents = new List<GameObject>();
        GameObject newBornTrainer;
        Brain brain = Resources.Load<Brain>("Brains/agentBrain" + i);

        if (i != 0 && i % 4 == 0)
        {
          floor++;
          squarePosition = 0;
          trainingFloor = CreateTrainingFloor(floor);
          // CreateFloorCamera(trainingFloor, floor);
        }

        AddTrainer(trainingFloor, floor, squarePosition, out newBornTrainer);
        SetSquarePosition(squarePosition, newBornTrainer);
        // AddAgentCamera(newBornTrainer);
        SetBrainParams(brain, Regex.Replace(System.Guid.NewGuid().ToString(), @"[^0-9]", ""));

        if (!isTargetDynamic)
        {
          Instantiate(StaticTarget, newBornTrainer.transform);
        }

        for (int y = 0; y < agentNumber; y++)
        {
          AgentTrainBehaviour atBehaviour;
          NewBornBuilder newBornBuilder;
          GameObject newBornAgent;
          newBornAgents.Add(AddAgent(newBornTrainer, out newBornAgent, out atBehaviour, out newBornBuilder));
          AddBrainToAgentBehaviour(atBehaviour, brain);
          SetRequestApi(newBornBuilder, atBehaviour, requestApiData);
          AddMinCellNb(newBornBuilder, minCellNb);
        }


        for (int y = 0; y < newBornAgents.Count; y++)
        {
          if (isTargetDynamic)
          {
            if (y != newBornAgents.Count - 1)
            {
              newBornAgents[y].GetComponent<AgentTrainBehaviour>().target = newBornAgents[y + 1].transform;
            }
            else
            {
              newBornAgents[y].GetComponent<AgentTrainBehaviour>().target = newBornAgents[0].transform;
            }
          }
          else
          {
            newBornAgents[y].GetComponent<AgentTrainBehaviour>().target = StaticTarget.transform;
          }
        }

        squarePosition++;
      }
    }


    public void BuildNewBornFromFetch(bool buildFromPost, string responseId, int agentId = 0)
    {
      Transform agent = Agents[agentId].transform;
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour>();
      NewBornBuilder newBornBuilder = agent.GetComponent<NewBornBuilder>();
      NewbornService newbornService = agent.GetComponent<NewbornService>();

      if (newBornBuilder.partNb == 0 && newBornBuilder.threshold == 0f)
      {
        newBornBuilder.partNb = agentConfig.layerNumber;
        newBornBuilder.threshold = agentConfig.threshold;
      }

      newBornBuilder.requestApiData = true;
      newBornBuilder.handleResponseData();
      if (buildFromPost)
      {
        atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
        atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
        atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { newBornBuilder.cellNb * 3 };
        atBehaviour.brain.brainParameters.vectorObservationSize = newBornBuilder.cellNb * 13 - 4;
        atBehaviour.brain.name = responseId;
      }
      else if (agentId == 0) // INIT FIRST BRAIN
      {
        setBrainParameters(academy, atBehaviour, newBornBuilder, newbornService);
        academy.broadcastHub.broadcastingBrains.Add(atBehaviour.brain);
      }
      else // ASSIGN ALL TO THE SAME BRAIN
      {
        atBehaviour.brain = Agents[0].transform.GetComponent<AgentTrainBehaviour>().brain;
      }
    }

    public void BuildRandomNewBorn(bool buildFromPost, int agentId = 0)
    {
      // Handle starting/communication with api data
      Transform agent = Agents[agentId].transform;
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour>();
      NewBornBuilder newBornBuilder = agent.GetComponent<NewBornBuilder>();
      NewbornService newbornService = agent.GetComponent<NewbornService>();

      newBornBuilder.requestApiData = false;
      newBornBuilder.initNewBorn(agentConfig.layerNumber, agentConfig.threshold);
      atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { Agents[agentId].transform.GetComponent<NewBornBuilder>().cellNb * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = Agents[agentId].transform.GetComponent<NewBornBuilder>().cellNb * 13 - 4;
    }

    public void BuildRandomGeneration()
    {
      academy.broadcastHub.broadcastingBrains.Clear();
      for (int a = 0; a < Agents.Count; a++)
      {
        NewBornBuilder newBornBuilder = Agents[a].transform.GetComponent<NewBornBuilder>();
        newBornBuilder.threshold = agentConfig.threshold;
        AgentTrainBehaviour atBehaviour = Agents[a].transform.GetComponent<AgentTrainBehaviour>();
        SetRequestApi(newBornBuilder, atBehaviour, false);
        newBornBuilder.BuildGeneration(newBornBuilder.GenerationInfos.Count, false);
        Brain brain = Resources.Load<Brain>("Brains/agentBrain" + a);
        SetBrainParams(brain, brain.name);
        Agents[a].gameObject.name = brain + "";
        brain.brainParameters.vectorObservationSize = vectorObservationSize;
        brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
        brain.brainParameters.vectorActionSize = new int[1] { Agents[a].transform.GetComponent<NewBornBuilder>().cellNb * 3 };
        brain.brainParameters.vectorObservationSize = Agents[a].transform.GetComponent<NewBornBuilder>().cellNb * 13 - 4;
        atBehaviour.brain = brain;
      }
    }

    public void PostAgents()
    {
      Debug.Log("Posting NewBorn to server...");
      for (int a = 0; a < Agents.Count; a++)
      {
        NewBornBuilder newBornBuilder = Agents[a].transform.GetComponent<NewBornBuilder>();
        AgentTrainBehaviour agentTrainBehaviour = Agents[a].transform.GetComponent<AgentTrainBehaviour>();
        string brainName = agentTrainBehaviour.brain.name;
        newBornBuilder.PostCell(brainName, a);
      }
    }

    public void RequestAgentInfo()
    {
      Debug.Log("Requesting Agent info from server...");
      for (int a = 0; a < Agents.Count; a++)
      {
        NewbornService newbornService = Agents[a].transform.GetComponent<NewbornService>();
        StartCoroutine(newbornService.getNewborn(cellId, a, false));
      }
    }

    public void DeleteCell()
    {
      foreach (GameObject agent in Agents)
      {
        agent.SetActive(true);
        agent.GetComponent<NewBornBuilder>().DeleteCells();
        agent.GetComponent<AgentTrainBehaviour>().DeleteBodyParts();
        Transform[] childs = agent.transform.Cast<Transform>().ToArray();
        foreach (Transform child in childs)
        {
          DestroyImmediate(child.gameObject);
        }
      }
    }

    private void SetBrainParams(Brain brain, string brainName)
    {
      brain.name = brainName;
      brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
      academy.broadcastHub.broadcastingBrains.Add(brain);
      academy.broadcastHub.SetControlled(brain, control);
    }

    private void CreateFloorCamera(GameObject trainingFloor, int floor)
    {
      GameObject cam = Instantiate(camera, trainingFloor.transform);
      transform.GetComponent<CameraSwitch>().cameraList.Add(cam);
      cam.name = "Camera" + floor;
      cam.SetActive(false);
      cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, floorHeight - 30f, cam.transform.localPosition.z);
    }

    private GameObject CreateTrainingFloor(int floor)
    {
      GameObject trainingFloor = new GameObject();
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
      trainingFloor.transform.localPosition = new Vector3(0f, floorHeight * floor, 0f);
      return trainingFloor;
    }

    private static void SetSquarePosition(int squarePosition, GameObject a)
    {
      switch (squarePosition)
      {
        case 0:
          a.transform.localPosition = new Vector3(0f, 0f, 0f);
          break;
        case 1:
          a.transform.localPosition = new Vector3(a.transform.Find("Ground").transform.localScale.x, 0f, 0f);
          break;
        case 2:
          a.transform.localPosition = new Vector3(0f, 0f, a.transform.Find("Ground").transform.localScale.z);
          break;
        case 3:
          a.transform.localPosition = new Vector3(a.transform.Find("Ground").transform.localScale.x, 0f, a.transform.Find("Ground").transform.localScale.z);
          break;
      }
    }

    private void AddTrainer(GameObject trainingFloor, int floor, int squarePosition, out GameObject newBornTrainer)
    {
      newBornTrainer = Instantiate(TrainingPrefab, trainingFloor.transform);
      newBornTrainer.name = "trainer" + floor + "." + squarePosition;
      newBornTrainer.transform.localScale = groundScale;
    }

    private GameObject AddAgent(GameObject newBornTrainer, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder)
    {
      newBornAgent = Instantiate(AgentPrefab, newBornTrainer.transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newBornBuilder.CellPrefab = CellPrefab;
      Agents.Add(newBornAgent);
      newBornAgent.transform.localPosition = new Vector3(Random.Range(-randomPositionIndex, randomPositionIndex), 0f, Random.Range(-randomPositionIndex, randomPositionIndex));
      return newBornAgent;
    }

    private void AddAgentCamera(GameObject a)
    {
      GameObject c = Instantiate(camera, a.transform);
      transform.GetComponent<CameraSwitch>().cameraList.Add(c);
      c.name = "Camera" + a.name;
      c.SetActive(false);
      c.transform.localPosition = new Vector3(0f, floorHeight - 30f, 0f);
      c.GetComponent<Camera>().orthographic = false;
      c.GetComponent<Camera>().fieldOfView = fieldOfView;
    }

    private void NameTrainingEnv(GameObject trainingFloor, int floor)
    {
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
    }


    private void AddMinCellNb(NewBornBuilder newBornBuilder, int minCellNb)
    {
      newBornBuilder.minCellNb = minCellNb;
    }

    private void SetRequestApi(NewBornBuilder newBornBuilder, AgentTrainBehaviour atBehaviour, bool requestApiData)
    {
      atBehaviour.requestApiData = requestApiData;
      newBornBuilder.requestApiData = requestApiData;
    }

    private void AddBrainToAgentBehaviour(AgentTrainBehaviour atBehaviour, Brain brain)
    {
      atBehaviour.brain = brain;
    }

    private void setBrainParameters(Academy academy, AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder, NewbornService newbornService)
    {
      academy.broadcastHub.broadcastingBrains.Clear();
      atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { newBornBuilder.cellNb * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = newBornBuilder.cellNb * 13 - 4;
      atBehaviour.brain.name = "NewBorn" + newbornService.responseUuid;
    }
  }
}