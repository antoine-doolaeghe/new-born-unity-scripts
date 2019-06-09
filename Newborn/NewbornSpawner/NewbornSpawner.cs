
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MLAgents;
using UnityEditor;
using UnityEngine;

namespace Newborn
{
  public class NewbornSpawner : MonoBehaviour
  {
    [HideInInspector] public List<GameObject> Agents = new List<GameObject>();
    public int agentNumber;
    public GameObject AgentPrefab;
    public float randomPositionIndex;
    public int minCellNb;
    public bool isLearningBrain;
    private float timer = 0.0f;
    public int vectorActionSize;
    public bool control;
    public GameObject StaticTarget;
    public bool isTargetDynamic;
    public bool isListeningToTrainedBorn;
    public void Update()
    {
      if (isListeningToTrainedBorn)
      {
        timer += Time.deltaTime;
        if (timer > 10f)
        {
          StartCoroutine(transform.GetComponent<SpawnerService>().ListTrainedNewborn(transform.gameObject));
          timer = 0.0f;
        }
      }
    }

    public void handleTarget()
    {
      /// ALL OF THIS LOGIC SHOULD BE HANDLED BY THE SPAWNER NOT THE MANAGER
      if (!isTargetDynamic)
      {
        Instantiate(StaticTarget, transform);
      }
      AssignTarget(Agents);
    }

    public void BuildAgents(bool requestApiData)
    {
      for (int y = 0; y < agentNumber; y++)
      {
        AgentTrainBehaviour atBehaviour;
        NewBornBuilder newBornBuilder;
        NewbornAgent newborn;
        GameObject newBornAgent;
        Agents.Add(BuildAgent(requestApiData, out newBornAgent, out atBehaviour, out newBornBuilder, out newborn));
      }
    }
    public GameObject BuildAgent(bool requestApiData, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder, out NewbornAgent newborn)
    {
      newBornAgent = Instantiate(AgentPrefab, transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newborn = newBornAgent.transform.GetComponent<NewbornAgent>();
      newborn.Sex = SexConfig.sexes[UnityEngine.Random.Range(0, 2)]; // Randomly select male or female
      newBornAgent.transform.localPosition = new Vector3(UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex), 0f, UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex));
      InstantiateTrainingBrain(newBornAgent, atBehaviour, newBornBuilder);
      SetApiRequestParameter(newBornBuilder, atBehaviour, requestApiData);
      AddMinCellNb(newBornBuilder, minCellNb);
      return newBornAgent;
    }

    public void PostTrainingNewborns()
    {
      Debug.Log("Posting training NewBorns to the server...");
      string generationId = GenerationService.generations[GenerationService.generations.Count - 1]; // Get the latest generation;
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
    private void InstantiateTrainingBrain(GameObject newBornAgent, AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder)
    {
      if (isLearningBrain)
      {
        LearningBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().learningBrain);
        UpdateTrainingParamFromLearningBrain(atBehaviour, newBornBuilder, brain);
      }
      else
      {
        PlayerBrain brain = Instantiate(newBornAgent.GetComponent<NewbornAgent>().playerBrain);
        UpdateTrainingParamFromPlayerBrain(atBehaviour, newBornBuilder, brain);
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
    private void UpdateTrainingParamFromLearningBrain(AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder, LearningBrain brain)
    {
      SetBrainParams(newBornBuilder, brain, NewbornBrain.GenerateRandomBrainName());
      AddBrainToAgentBehaviour(atBehaviour, brain);
    }
    private void UpdateTrainingParamFromPlayerBrain(AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder, PlayerBrain brain)
    {
      SetBrainParams(newBornBuilder, brain, NewbornBrain.GenerateRandomBrainName());
      AddBrainToAgentBehaviour(atBehaviour, brain);
    }
    private void SetBrainParams(NewBornBuilder newbornBuilder, Brain brain, string brainName)
    {
      brain.name = brainName;
      brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
      newbornBuilder.academy.broadcastHub.SetControlled(brain, control);
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