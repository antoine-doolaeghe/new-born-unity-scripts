using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEngine;

namespace Gene
{
  public class TrainingSpawner : MonoBehaviour
  {
    public int agentTrainerNumber;
    public int agentNumber;
    public GameObject agent;
    public GameObject camera;
    [Header("Environment parameters")]
    public Vector3 agentScale;
    public Vector3 groundScale;
    public Vector3 targetPosition;
    public float floorHeight;
    [Header("Agent parameters")]
    public int minCellNb;
    public bool requestApiData;
    public string cellId;
    public AgentConfig agentConfig;
    [Header("Academy parameters")]
    public Academy academy;
    public bool control;
    [Header("Brain parameters")]
    public int vectorObservationSize;
    public int vectorActionSize;
    public TextAsset brainModel;
    [Header("Camera parameters")]
    public float fieldOfView;

    private List<GameObject> Agents = new List<GameObject>();

    public void Delete()
    {
      Transform[] childs = transform.Cast<Transform>().ToArray();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
      Agents.Clear();
      academy.broadcastHub.broadcastingBrains.Clear();
      transform.GetComponent<CameraSwitch>().cameraList.Clear();
    }

    public void BuildAgents()
    {
      GameObject trainingFloor = new GameObject();
      GameObject trainingCamera = Instantiate(camera, trainingFloor.transform);
      int floor = 0;
      int squarePosition = 0;

      NameTrainingEnv(trainingFloor, trainingCamera, floor);

      for (var i = 0; i < agentTrainerNumber; i++)
      {
        GameObject a, newBorn;
        AgentTrainBehaviour atBehaviour;
        Cell cell;
        Brain brain = Resources.Load<Brain>("Brains/agentBrain" + i);

        if (i != 0 && i % 4 == 0)
        {
          floor++;
          squarePosition = 0;
          trainingFloor = CreateTrainingFloor(floor);
          CreateFloorCamera(trainingFloor, floor);
        }

        for (int y = 0; y < agentNumber; y++)
        {
          AddAgent(trainingFloor, floor, squarePosition, out a, out newBorn, out atBehaviour, out cell);
          AddAgentCamera(a, newBorn);
          SetBrainParams(brain);
          AddBrainToAgentBehaviour(atBehaviour, brain);
          SetRequestApi(cell, atBehaviour, requestApiData);
          AddMinCellNb(cell, minCellNb);
          SetSquarePosition(squarePosition, a);
        }

        squarePosition++;
      }
    }

    private void SetBrainParams(Brain brain)
    {
      brain.name = "NewBorn" + System.Guid.NewGuid();
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

    private void AddAgent(GameObject trainingFloor, int floor, int squarePosition, out GameObject a, out GameObject newBorn, out AgentTrainBehaviour atBehaviour, out Cell cell)
    {
      a = Instantiate(agent, trainingFloor.transform);
      newBorn = a.transform.Find("NewBorn").gameObject;
      atBehaviour = newBorn.transform.GetComponent<AgentTrainBehaviour>();
      cell = newBorn.transform.GetComponent<Cell>();
      Agents.Add(newBorn);
      a.transform.localScale = groundScale;
      newBorn.transform.localPosition = new Vector3(Random.Range(-150f, 150f), 0f, Random.Range(-150f, 150f));
      a.name = "trainer" + floor + "." + squarePosition;
      a.transform.Find("Target").localPosition = targetPosition;
    }

    private void AddAgentCamera(GameObject a, GameObject newBorn)
    {
      GameObject c = Instantiate(camera, a.transform);
      transform.GetComponent<CameraSwitch>().cameraList.Add(c);
      c.name = "Camera" + a.name;
      c.SetActive(false);
      c.transform.localPosition = new Vector3(newBorn.transform.localPosition.x, floorHeight - 30f, newBorn.transform.localPosition.z);
      c.GetComponent<Camera>().orthographic = false;
      c.GetComponent<Camera>().fieldOfView = fieldOfView;
    }

    private void NameTrainingEnv(GameObject trainingFloor, GameObject trainingCamera, int floor)
    {
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
      trainingCamera.name = "Camera" + floor;
    }

    public void BuildAgentCell(bool buildFromPost, int agentId = 0)
    {
      // Handle starting/communication with api data
      Transform agent = Agents[agentId].transform;
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour>();
      Cell cell = agent.GetComponent<Cell>();
      PostGene postGene = agent.GetComponent<PostGene>();

      if (requestApiData) // NON RANDOM BUILD
      {
        cell.partNb = agentConfig.layerNumber;
        cell.threshold = agentConfig.threshold;
        cell.requestApiData = true;
        cell.parseRequestData();
        if (buildFromPost)
        {
          atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
          atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
          atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { cell.cellNb * 3 };
          atBehaviour.brain.brainParameters.vectorObservationSize = cell.cellNb * 13 - 4;
          atBehaviour.brain.name = "NewBorn" + postGene.responseUuid;
        }
        else if (agentId == 0) // INIT FIRST BRAIN
        {
          setBrainParameters(academy, atBehaviour, cell, postGene);
          academy.broadcastHub.broadcastingBrains.Add(atBehaviour.brain);
        }
        else // ASSIGN ALL TO THE SAME BRAIN
        {
          atBehaviour.brain = Agents[0].transform.GetComponent<AgentTrainBehaviour>().brain;
        }
      }
      else // RANDOMLY BUILT
      {
        for (int k = 0; k < Agents.Count; k++)
        {
          cell.requestApiData = false;
          cell.initGerms(agentConfig.layerNumber, agentConfig.threshold);
          atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
          atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
          atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { Agents[agentId].transform.GetComponent<Cell>().cellNb * 3 };
          atBehaviour.brain.brainParameters.vectorObservationSize = Agents[agentId].transform.GetComponent<Cell>().cellNb * 13 - 4;
        }
      }
    }

    public void PostAgents()
    {
      Debug.Log("Posting Agent to server...");
      for (int a = 0; a < Agents.Count; a++)
      {
        Cell cell = Agents[a].transform.GetComponent<Cell>();
        PostGene postGene = Agents[a].transform.GetComponent<PostGene>();
        string postData = cell.HandlePostData();
        StartCoroutine(postGene.postCell(postData, agent.name, a));
      }
    }

    public void RequestAgentInfo()
    {
      Debug.Log("Requesting Agent info from server...");
      for (int a = 0; a < Agents.Count; a++)
      {
        PostGene postGene = Agents[a].transform.GetComponent<PostGene>();
        StartCoroutine(postGene.getCell(cellId, a, false));
      }
    }

    public void DeleteCell()
    {
      foreach (GameObject agent in Agents)
      {
        agent.SetActive(true);
        agent.GetComponent<Cell>().DeleteCells();
        agent.GetComponent<AgentTrainBehaviour>().DeleteBodyParts();
        Transform[] childs = agent.transform.Cast<Transform>().ToArray();
        foreach (Transform child in childs)
        {
          DestroyImmediate(child.gameObject);
        }
      }
    }

    private void AddMinCellNb(Cell cell, int minCellNb)
    {
      cell.minCellNb = minCellNb;
    }

    private void SetRequestApi(Cell cell, AgentTrainBehaviour atBehaviour, bool requestApiData)
    {
      atBehaviour.requestApiData = requestApiData;
      cell.requestApiData = requestApiData;
    }

    private void AddBrainToAgentBehaviour(AgentTrainBehaviour atBehaviour, Brain brain)
    {
      atBehaviour.brain = brain;
    }

    private void setBrainParameters(Academy academy, AgentTrainBehaviour atBehaviour, Cell cell, PostGene postGene)
    {
      academy.broadcastHub.broadcastingBrains.Clear();
      atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { cell.cellNb * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = cell.cellNb * 13 - 4;
      atBehaviour.brain.name = "NewBorn" + postGene.responseUuid;
    }
  }
}