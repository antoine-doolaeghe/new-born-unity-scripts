using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using MyBox;
using UnityEngine;
using Service.Newborn;
using Components.Newborn;
using Components.Spawner.Newborn;

namespace Components.Manager
{
  [ExecuteInEditMode]
  public class NewbornManager : MonoBehaviour
  {
    public bool isTrainingMode;
    public int spawnerNumber;
    public bool requestApiData;
    public string newbornId;
    public Academy academy;
    public Brain brainObject;
    public int vectorObservationSize;
    public int vectorActionSize;
    public TextAsset brainModel;
    public List<GameObject> Spawners = new List<GameObject>();

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
        Brain brain = Instantiate(brainObject);
        NewbornSpawner[] spawners = Object.FindObjectsOfType<NewbornSpawner>();
        foreach (var spawner in spawners)
        {
          Spawners.Add(spawner.transform.gameObject);
          spawner.BuildAgents(requestApiData);
        }
      }
    }

    public IEnumerator RequestNewbornAgentInfo()
    {
      Debug.Log("Request Agent info from server...📡");
      GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
      Debug.Log("number of agents found");
      for (int a = 0; a < agents.Length; a++)
      {
        // TODO: would be better to build after fetch
        yield return StartCoroutine(NewbornService.GetNewborn(newbornId, agents[a], false));
        agents[a].GetComponent<AgentTrainBehaviour>().enabled = true;
      }
      Debug.Log("Finished to build Agents");
      academy.InitializeEnvironment();
      academy.initialized = true;
    }

    public void RequestNewborn()
    {
      StartCoroutine(RequestNewbornAgentInfo());
    }

    public void ClearBroadCastingBrains()
    {
      academy.broadcastHub.broadcastingBrains.Clear();
    }
  }
}