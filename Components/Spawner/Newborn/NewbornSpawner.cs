using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;
using Service.Spawner;
using Components.Newborn.Sex;
using Components.Newborn.Anatomy;
using Components.Newborn;
using Components.Target;
using Service.Generation;

namespace Components.Spawner.Newborn
{
  public class NewbornSpawner : MonoBehaviour
  {
    [HideInInspector] public List<GameObject> Agents = new List<GameObject>();
    public NewbornSpawnerConfig NewbornSpawnerConfig;
    public bool isListeningToTrainedBorn;
    public bool hasLearningBrain;
    private float timer = 0.0f;
    public int vectorActionSize;
    private List<LearningBrain> learningBrains;
    private List<PlayerBrain> playerBrains;

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

    public void BuildAgents(bool HasApiConnection)
    {
      for (int y = 0; y < NewbornSpawnerConfig.agentNumber; y++)
      {
        AgentTrainBehaviour atBehaviour;
        AnatomyBuilder AnatomyBuilder;
        NewbornAgent newborn;
        GameObject newBornAgent;
        Vector3 agentPosition = ReturnAgentPosition(y);
        Agents.Add(BuildAgent(HasApiConnection, agentPosition, out newBornAgent, out atBehaviour, out AnatomyBuilder, out newborn));
        InstantiateTrainingBrain(newBornAgent, atBehaviour, AnatomyBuilder, y);
        if (transform.Find("Ground") != null)
        {
          AssignGround(transform.Find("Ground").transform);
        }
      }
    }

    public GameObject BuildAgent(bool HasApiConnection, Vector3 position, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out AnatomyBuilder AnatomyBuilder, out NewbornAgent newborn)
    {
      newBornAgent = Instantiate(NewbornSpawnerConfig.AgentPrefab);
      newBornAgent.transform.parent = transform;
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      AnatomyBuilder = newBornAgent.transform.GetComponent<AnatomyBuilder>();
      newborn = newBornAgent.transform.GetComponent<NewbornAgent>();
      newborn.Sex = SexConfig.sexes[UnityEngine.Random.Range(0, 2)];             // Randomly select male or female
      # region to refactor
      TargetController targetController = newBornAgent.transform.GetComponent<TargetController>();
      newBornAgent.transform.localPosition = position;
      targetController.spawner = this;
      atBehaviour.spawner = this;
      atBehaviour.targetController = targetController;
      # endregion
      SetApiRequestParameter(AnatomyBuilder, atBehaviour, HasApiConnection);
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
        AnatomyBuilder AnatomyBuilder = agent.transform.GetComponent<AnatomyBuilder>();
        AgentTrainBehaviour agentTrainBehaviour = agent.transform.GetComponent<AgentTrainBehaviour>();
        string newbornId = agentTrainBehaviour.brain.name;
        string newbornName = newborn.title;
        string newbornSex = newborn.Sex;
        string newbornHex = "mock hex";
        NewBornPostData newBornPostData = new NewBornPostData(newbornName, newbornId, generationId, newbornSex, newbornHex);
        StartCoroutine(AnatomyBuilder.PostNewborn(newBornPostData, agent));
      }
    }


    public Vector3 ReturnAgentPosition(int y)
    {
      Vector3 agentPosition;
      if (NewbornSpawnerConfig.isRandomSpawn)
      {
        float spawnRange = NewbornSpawnerConfig.spawnRange;
        agentPosition = new Vector3(Random.Range(0f, spawnRange), 5f, Random.Range(0f, spawnRange));
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
        agent.transform.GetComponent<AnatomyBuilder>().BuildAgentRandomGeneration(agent.transform);
      }
    }

    public void BuildAllAgentsRandomNewBorn()
    {
      foreach (GameObject agent in Agents)
      {
        StartCoroutine(agent.transform.GetComponent<AnatomyBuilder>().BuildAgentRandomNewBorn());
      }
    }

    public void ClearAgents()
    {
      foreach (GameObject agent in Agents)
      {
        agent.SetActive(true);
        agent.GetComponent<AnatomyBuilder>().ResetBuilder();
        Transform[] childs = agent.transform.Cast<Transform>().ToArray();
        foreach (Transform child in childs)
        {
          DestroyImmediate(child.gameObject);
        }
      }
    }
    #endregion

    #region helper methods
    private void InstantiateTrainingBrain(GameObject newBornAgent, AgentTrainBehaviour atBehaviour, AnatomyBuilder AnatomyBuilder, int y)
    {
      if (hasLearningBrain)
      {
        if (y == 0)
        {
          learningBrains = new List<LearningBrain>();
          LearningBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().learningBrain);
          AddBrainToAcademy(AnatomyBuilder, brain);
          UpdateTrainingParamFromLearningBrain(atBehaviour, AnatomyBuilder, brain);
          learningBrains.Add(brain);
        }
        else
        {
          if (NewbornSpawnerConfig.instantiateSingleBrain)
          {
            UpdateTrainingParamFromLearningBrain(atBehaviour, AnatomyBuilder, learningBrains[0]);
          }
          else
          {
            LearningBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().learningBrain);
            AddBrainToAcademy(AnatomyBuilder, brain);
            UpdateTrainingParamFromLearningBrain(atBehaviour, AnatomyBuilder, brain);
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
          UpdateTrainingParamFromPlayerBrain(atBehaviour, AnatomyBuilder, brain);
          AddBrainToAcademy(AnatomyBuilder, brain);
          playerBrains.Add(brain);
        }
        else
        {
          if (NewbornSpawnerConfig.instantiateSingleBrain)
          {
            UpdateTrainingParamFromPlayerBrain(atBehaviour, AnatomyBuilder, playerBrains[0]);
          }
          else
          {
            PlayerBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().playerBrain);
            UpdateTrainingParamFromPlayerBrain(atBehaviour, AnatomyBuilder, brain);
            AddBrainToAcademy(AnatomyBuilder, brain);
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

    private void UpdateTrainingParamFromLearningBrain(AgentTrainBehaviour atBehaviour, AnatomyBuilder AnatomyBuilder, LearningBrain brain)
    {
      SetBrainParams(AnatomyBuilder, brain, "temporaryName");
      AddBrainToAgentBehaviour(atBehaviour, brain);
    }
    private void UpdateTrainingParamFromPlayerBrain(AgentTrainBehaviour atBehaviour, AnatomyBuilder AnatomyBuilder, PlayerBrain brain)
    {
      SetBrainParams(AnatomyBuilder, brain, "temporaryName");
      AddBrainToAgentBehaviour(atBehaviour, brain);
    }
    private void SetBrainParams(AnatomyBuilder AnatomyBuilder, Brain brain, string brainName)
    {
      brain.name = brainName;
      brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
      AnatomyBuilder.academy.broadcastHub.SetControlled(brain, NewbornSpawnerConfig.control);
    }

    public void AddBrainToAcademy(AnatomyBuilder AnatomyBuilder, Brain brain)
    {
      AnatomyBuilder.academy.broadcastHub.broadcastingBrains.Add(brain);
    }


    public void AddBrainToAgentBehaviour(AgentTrainBehaviour atBehaviour, Brain brain)
    {
      atBehaviour.brain = brain;
    }

    public void SetApiRequestParameter(AnatomyBuilder AnatomyBuilder, AgentTrainBehaviour atBehaviour, bool HasApiConnection)
    {
      atBehaviour.HasApiConnection = HasApiConnection;
      AnatomyBuilder.HasApiConnection = HasApiConnection;
    }

    #endregion
  }
}
