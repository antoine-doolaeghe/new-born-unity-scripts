using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;

namespace Newborn
{
  public class NewbornSpawner : MonoBehaviour
  {
    [HideInInspector] public List<GameObject> Agents = new List<GameObject>();
    public int agentNumber;
    public bool isListeningToTrainedBorn;
    public GameObject AgentPrefab;
    public int minCellNb;
    public bool hasLearningBrain;
    private float timer = 0.0f;
    public int vectorActionSize;
    public bool control;
    private List<LearningBrain> learningBrains;
    private List<PlayerBrain> playerBrains;
    public bool isRandomSpawn;
    public float spawnRange;

    public bool instantiateSingleBrain;

    public void Update()
    {
      if (isListeningToTrainedBorn)
      {
        timer += Time.deltaTime;
        if (timer > 10f)
        {
          SpawnerService.SetParentGestationCallback callback = SpawnerService.SetParentGestationToFalse;
          StartCoroutine(transform.GetComponent<SpawnerService>().ListTrainedNewborn(this, callback));
          timer = 0.0f;
        }
      }
    }

    public void BuildAgents(bool requestApiData)
    {
      for (int y = 0; y < agentNumber; y++)
      {
        AgentTrainBehaviour atBehaviour;
        NewBornBuilder newBornBuilder;
        NewbornAgent newborn;
        GameObject newBornAgent;
        // let make the region a bit more dynamic here
        Vector3 agentPosition = ReturnAgentPosition(y);
        Agents.Add(BuildAgent(requestApiData, agentPosition, out newBornAgent, out atBehaviour, out newBornBuilder, out newborn));
        InstantiateTrainingBrain(newBornAgent, atBehaviour, newBornBuilder, y);
        if (transform.Find("Ground") != null)
        {
          AssignGround(transform.Find("Ground").transform);
        }
      }
    }

    public GameObject BuildAgent(bool requestApiData, Vector3 position, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder, out NewbornAgent newborn)
    {
      newBornAgent = Instantiate(AgentPrefab, transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newborn = newBornAgent.transform.GetComponent<NewbornAgent>();
      newborn.Sex = SexConfig.sexes[UnityEngine.Random.Range(0, 2)];             // Randomly select male or female
      # region to refactor
      TargetController targetController = newBornAgent.transform.GetComponent<TargetController>();
      newBornAgent.transform.localPosition = position;
      targetController.spawner = this;
      atBehaviour.spawner = this;
      atBehaviour.targetController = targetController;
      # endregion
      SetApiRequestParameter(newBornBuilder, atBehaviour, requestApiData);
      AddMinCellNb(newBornBuilder, minCellNb);
      return newBornAgent;
    }

    public void PostTrainingNewborns()
    {
      Debug.Log("Posting training NewBorns to the server...");
      string generationId = GenerationService.generations[GenerationService.generations.Count - 1];  // Get the latest generation;
      GameObject[] agentList = GameObject.FindGameObjectsWithTag("agent");
      foreach (GameObject agent in Agents)
      {
        NewbornAgent newborn = agent.transform.GetComponent<NewbornAgent>();
        NewBornBuilder newBornBuilder = agent.transform.GetComponent<NewBornBuilder>();
        AgentTrainBehaviour agentTrainBehaviour = agent.transform.GetComponent<AgentTrainBehaviour>();
        string newbornId = agentTrainBehaviour.brain.name;
        string newbornName = newborn.title;
        string newbornSex = newborn.Sex;
        string newbornHex = "mock hex";
        NewBornPostData newBornPostData = new NewBornPostData(newbornName, newbornId, generationId, newbornSex, newbornHex);
        StartCoroutine(newBornBuilder.PostNewborn(newBornPostData, agent));
      }
    }


    public Vector3 ReturnAgentPosition(int y)
    {
      Vector3 agentPosition;
      if (isRandomSpawn)
      {
        agentPosition = new Vector3(Random.Range(0f, spawnRange), 10f, Random.Range(0f, spawnRange));
      }
      else
      {

        agentPosition = PositionGridAgent(y);
      }

      return agentPosition;
    }

    private Vector3 PositionGridAgent(int y)
    {
      y += 1;
      Vector3 position = new Vector3(0f, 0f, 0f);
      switch (y % 4)
      {
        case 0:
          position = new Vector3(20f * y, 0f, 20f * y);
          break;
        case 1:
          position = new Vector3(20f * y, 0f, -20f * y);
          break;
        case 2:
          position = new Vector3(-20f * y, 0f, 20f * y);
          break;
        case 3:
          position = new Vector3(-20f * y, 0f, -20f * y);
          break;
      }
      return position;
    }

    public void resetMinimumTargetDistance()
    {
      foreach (GameObject agent in Agents)
      {
        TargetController targetController = agent.transform.GetComponent<TargetController>();
        targetController.resetMinimumTargetDistance();
      }
    }

    public void resetTrainingAgents()
    {
      foreach (GameObject agent in Agents)
      {
        AgentTrainBehaviour agentTrainBehaviour = agent.transform.GetComponent<AgentTrainBehaviour>();
        agentTrainBehaviour.AgentReset();
      }
    }

    #region editor methods
    public void BuildAllAgentsRandomGeneration()
    {
      foreach (GameObject agent in Agents)
      {
        agent.transform.GetComponent<NewBornBuilder>().BuildAgentRandomGeneration(agent.transform);
      }
    }

    public void BuildAllAgentsRandomNewBorn()
    {
      foreach (GameObject agent in Agents)
      {
        StartCoroutine(agent.transform.GetComponent<NewBornBuilder>().BuildAgentRandomNewBorn());
      }
    }

    public void ClearAgents()
    {
      foreach (GameObject agent in Agents)
      {
        agent.SetActive(true);
        agent.GetComponent<NewBornBuilder>().ClearNewborns();
        Transform[] childs = agent.transform.Cast<Transform>().ToArray();
        foreach (Transform child in childs)
        {
          DestroyImmediate(child.gameObject);
        }
      }
    }
    #endregion

    #region helper methods
    private void InstantiateTrainingBrain(GameObject newBornAgent, AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder, int y)
    {
      if (hasLearningBrain)
      {
        if (y == 0)
        {
          learningBrains = new List<LearningBrain>();
          LearningBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().learningBrain);
          AddBrainToAcademy(newBornBuilder, brain);
          UpdateTrainingParamFromLearningBrain(atBehaviour, newBornBuilder, brain);
          learningBrains.Add(brain);
        }
        else
        {
          if (instantiateSingleBrain)
          {
            UpdateTrainingParamFromLearningBrain(atBehaviour, newBornBuilder, learningBrains[0]);
          }
          else
          {
            LearningBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().learningBrain);
            AddBrainToAcademy(newBornBuilder, brain);
            UpdateTrainingParamFromLearningBrain(atBehaviour, newBornBuilder, brain);
            learningBrains.Add(brain);
          }
        }
      }
      else
      {
        if (y == 0)
        {
          playerBrains = new List<PlayerBrain>();
          PlayerBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().playerBrain);
          UpdateTrainingParamFromPlayerBrain(atBehaviour, newBornBuilder, brain);
          AddBrainToAcademy(newBornBuilder, brain);
          playerBrains.Add(brain);
        }
        else
        {
          if (instantiateSingleBrain)
          {
            UpdateTrainingParamFromPlayerBrain(atBehaviour, newBornBuilder, playerBrains[0]);
          }
          else
          {
            PlayerBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().playerBrain);
            UpdateTrainingParamFromPlayerBrain(atBehaviour, newBornBuilder, brain);
            AddBrainToAcademy(newBornBuilder, brain);
            playerBrains.Add(brain);
          }
        }
      }
    }

    public void AssignGround(Transform ground)
    {
      foreach (GameObject agent in Agents)
      {
        agent.GetComponent<TargetController>().ground = ground;
      }
    }

    private void UpdateTrainingParamFromLearningBrain(AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder, LearningBrain brain)
    {
      SetBrainParams(newBornBuilder, brain, "temporaryName");
      AddBrainToAgentBehaviour(atBehaviour, brain);
    }
    private void UpdateTrainingParamFromPlayerBrain(AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder, PlayerBrain brain)
    {
      SetBrainParams(newBornBuilder, brain, "temporaryName");
      AddBrainToAgentBehaviour(atBehaviour, brain);
    }
    private void SetBrainParams(NewBornBuilder newbornBuilder, Brain brain, string brainName)
    {
      brain.name = brainName;
      brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
      newbornBuilder.academy.broadcastHub.SetControlled(brain, control);
    }

    public void AddBrainToAcademy(NewBornBuilder newbornBuilder, Brain brain)
    {
      newbornBuilder.academy.broadcastHub.broadcastingBrains.Add(brain);
    }


    public void AddBrainToAgentBehaviour(AgentTrainBehaviour atBehaviour, Brain brain)
    {
      atBehaviour.brain = brain;
    }

    public void SetApiRequestParameter(NewBornBuilder newBornBuilder, AgentTrainBehaviour atBehaviour, bool requestApiData)
    {
      atBehaviour.requestApiData = requestApiData;
      newBornBuilder.requestApiData = requestApiData;
    }

    public void AddMinCellNb(NewBornBuilder newBornBuilder, int minCellNb)
    {
      newBornBuilder.minCellNb = minCellNb;
    }
    #endregion
  }
}
