using System.Collections;
using System.Linq;
using MLAgents;
using UnityEngine;
using Service.Newborn;
using Service.Trainer;
using Components.Newborn;

namespace Components.Manager
{
  [ExecuteInEditMode]
  public class Manager : MonoBehaviour
  {
    public bool isTrainingMode;
    public string newbornId;
    public string TrainerName;
    public void DeleteEnvironment()
    {
      Transform[] childs = transform.Cast<Transform>().ToArray();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
      FindObjectOfType<Academy>().broadcastHub.broadcastingBrains.Clear();
    }

    public void BuildTrainerEnvironment()
    {
      StartCoroutine(FindObjectOfType<TrainerService>().DownloadTrainer(TrainerName));
    }

    public IEnumerator RequestNewbornAgentInfo()
    {
      Academy academy = FindObjectOfType<Academy>();
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

    public void ResetTrainerData()
    {
      StartCoroutine(TrainerService.UpdateTrainerData(TrainerName, string.Empty));
      DeleteEnvironment();
    }
  }
}