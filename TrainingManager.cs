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
    [Header("Production Mode")]
    public bool isProductionMode;
    [Header("Environment parameters")]
    public int spawnerNumber;
    public int agentNumber;
    public GameObject Camera;
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

    public void BuildSpawners()
    {
      GameObject parent = transform.gameObject;
      int floor = 0;
      int squarePosition = 0;

      for (var i = 0; i < spawnerNumber; i++)
      {
        List<GameObject> newBornAgents = new List<GameObject>();
        GameObject spawner;
        Brain brain = Resources.Load<Brain>("Brains/agentBrain" + i);

        if (!isProductionMode && i % 4 == 0)
        {
          parent = new GameObject();
          NameFloor(parent, floor);
          floor++;
          squarePosition = 0;
          parent = CreateTrainingFloor(floor);
        }

        InstantiateSpawner(parent, floor, squarePosition, out spawner);

        if(isProductionMode) {
          // Randomly place the agents.
        } else {
          PositionTrainingSpawner(squarePosition, spawner);
        }
        
        SetBrainParams(brain, Regex.Replace(System.Guid.NewGuid().ToString(), @"[^0-9]", ""));

        if (!isTargetDynamic && !isProductionMode)
        {
          Instantiate(StaticTarget, spawner.transform);
        }

        for (int y = 0; y < agentNumber; y++)
        {
          AgentTrainBehaviour atBehaviour;
          NewBornBuilder newBornBuilder;
          GameObject newBornAgent;
          newBornAgents.Add(AddAgent(spawner, out newBornAgent, out atBehaviour, out newBornBuilder));
          AddBrainToAgentBehaviour(atBehaviour, brain);
          SetRequestApi(newBornBuilder, atBehaviour, requestApiData);
          AddMinCellNb(newBornBuilder, minCellNb);
        }

        AssignTarget(newBornAgents);

        squarePosition++;
      }
    }

    private void AssignTarget(List<GameObject> newBornAgents)
    {
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

    public void BuildRandomTrainingNewBorn(bool buildFromPost, int agentId = 0)
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

    public void BuildRandomProductionNewBorn(Transform agent)
    {
      // Handle starting/communication with api data
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour>();
      NewBornBuilder newBornBuilder = agent.GetComponent<NewBornBuilder>();
      NewbornService newbornService = agent.GetComponent<NewbornService>();

      newBornBuilder.requestApiData = false;
      newBornBuilder.initNewBorn(agentConfig.layerNumber, agentConfig.threshold);
      atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { newBornBuilder.cellNb * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = newBornBuilder.cellNb * 13 - 4;
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

    public void RequestTrainingAgentInfo()
    {
      Debug.Log("Requesting Agent info from server...");
      for (int a = 0; a < Agents.Count; a++)
      {
        NewbornService newbornService = Agents[a].transform.GetComponent<NewbornService>();
        StartCoroutine(newbornService.getNewborn(cellId, a, false));
      }
    }

    public void RequestProductionAgentInfo()
    {
      foreach (GameObject fooObj in GameObject.FindGameObjectsWithTag("agent"))
      {
        Debug.Log(fooObj.name);
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

    private static void PositionProductionSpawner()
    {
      Debug.Log("TO-DO: Position the production spawner");
    }

    private void InstantiateSpawner(GameObject parent, int floor, int squarePosition, out GameObject spawner)
    {
      spawner = Instantiate(TrainingPrefab, parent.transform);
      spawner.name = isProductionMode ? ("Spawner") : ("Trainer" + floor + "." + squarePosition);
      spawner.transform.localScale = groundScale;
    }

    private GameObject AddAgent(GameObject spawner, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder)
    {
      newBornAgent = Instantiate(AgentPrefab, spawner.transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newBornBuilder.CellPrefab = CellPrefab;
      Agents.Add(newBornAgent);
      newBornAgent.transform.localPosition = new Vector3(Random.Range(-randomPositionIndex, randomPositionIndex), 0f, Random.Range(-randomPositionIndex, randomPositionIndex));
      return newBornAgent;
    }

    private void NameFloor(GameObject trainingFloor, int floor)
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