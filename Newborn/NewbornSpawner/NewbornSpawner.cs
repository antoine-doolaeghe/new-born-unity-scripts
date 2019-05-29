using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Gene
{
  public class NewbornSpawner : MonoBehaviour
  {
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

    public GameObject BuildAgent(GameObject spawner, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder, out Newborn newborn)
    {
      newBornAgent = Instantiate(AgentPrefab, spawner.transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour>();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder>();
      newborn = newBornAgent.transform.GetComponent<Newborn>();
      newborn.Sex = SexConfig.sexes[UnityEngine.Random.Range(0, 2)]; // Randomly select male or female
      // Agents.Add(newBornAgent);
      newBornAgent.transform.localPosition = new Vector3(UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex), 0f, UnityEngine.Random.Range(-randomPositionIndex, randomPositionIndex));
      return newBornAgent;
    }
  }
}