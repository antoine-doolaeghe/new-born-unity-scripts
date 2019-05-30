
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Newborn
{
  public class NewbornSpawner : MonoBehaviour
  {
    [HideInInspector] public List<GameObject> Agents = new List<GameObject>();
    public GameObject AgentPrefab;
    public float randomPositionIndex;
    private float timer = 0.0f;
    public void Update()
    {
      timer += Time.deltaTime;
      if (timer > 10f)
      {
        StartCoroutine(NewbornService.ListTrainedNewborn());
        timer = 0.0f;
      }
    }

    public GameObject BuildAgent(GameObject spawner, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder, out NewbornAgent newborn)
    {
      newBornAgent = Instantiate(AgentPrefab, spawner.transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newborn = newBornAgent.transform.GetComponent<NewbornAgent>();
      newborn.Sex = SexConfig.sexes[UnityEngine.Random.Range(0, 2)]; // Randomly select male or female
      // Agents.Add(newBornAgent);
      newBornAgent.transform.localPosition = new Vector3(UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex), 0f, UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex));
      return newBornAgent;
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
  }
}