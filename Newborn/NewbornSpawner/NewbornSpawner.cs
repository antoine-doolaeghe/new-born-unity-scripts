
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
    public GameObject AgentPrefab;
    public float randomPositionIndex;
    public int minCellNb;
    public Brain brainObject;
    private float timer = 0.0f;
    public NewbornService newbornService;
    public int vectorActionSize;
    public bool control;
    public void Update()
    {
      timer += Time.deltaTime;
      if (timer > 10f)
      {
        StartCoroutine(transform.GetComponent<SpawnerService>().ListTrainedNewborn(transform.gameObject));
        timer = 0.0f;
      }
    }
    public GameObject BuildAgent(GameObject spawner, bool requestApiData, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder, out NewbornAgent newborn)
    {
      Brain brain = Instantiate(brainObject);
      SetBrainParams(brain, NewbornBrain.GenerateRandomBrainName());
      newBornAgent = Instantiate(AgentPrefab, spawner.transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newborn = newBornAgent.transform.GetComponent<NewbornAgent>();
      newborn.Sex = SexConfig.sexes[UnityEngine.Random.Range(0, 2)]; // Randomly select male or female
      newBornAgent.transform.localPosition = new Vector3(UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex), 0f, UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex));
      spawner.GetComponent<NewbornSpawner>().SetApiRequestParameter(newBornBuilder, atBehaviour, requestApiData);
      spawner.GetComponent<NewbornSpawner>().AddMinCellNb(newBornBuilder, minCellNb);
      spawner.GetComponent<NewbornSpawner>().AddBrainToAgentBehaviour(atBehaviour, brain);
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
        // DO a generation check ? 
        NewBornPostData newBornPostData = new NewBornPostData(newbornName, newbornId, generationId, newbornSex, newbornHex);
        newBornBuilder.PostNewborn(newBornPostData, agent);
      }
    }

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
      CrawlerAcademy academy = GameObject.Find("Academy").GetComponent<CrawlerAcademy>();
      brain.name = brainName;
      brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
      academy.broadcastHub.SetControlled(brain, control);
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
  }
}