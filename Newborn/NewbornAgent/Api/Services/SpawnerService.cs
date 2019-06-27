using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Newborn
{
  [ExecuteInEditMode]
  public class SpawnerService : MonoBehaviour
  {
    public delegate IEnumerator SetParentGestationCallback(KeyValuePair<string, JSONNode> newbornId);
    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();
    static byte[] postData;
    static Dictionary<string, string> postHeader;
    private static String graphQlInput;
    public IEnumerator ListTrainedNewborn(NewbornSpawner spawner, SetParentGestationCallback SetParentGestationCallback)
    {
      WWW www;
      ServiceHelpers.graphQlApiRequest(SpawnerService.variable, SpawnerService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newbornsGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("NewBorn List successfully requested!");
        JSONNode responseData = JSON.Parse(www.text)["data"]["listNewborns"]["items"];
        if (responseData == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        foreach (System.Collections.Generic.KeyValuePair<string, SimpleJSON.JSONNode> newbornId in responseData)
        {
          AgentTrainBehaviour atBehaviour;
          NewBornBuilder newBornBuilder;
          NewbornAgent newborn;
          GameObject newBornAgent;

          SetParentGestationCallback(newbornId);
          GameObject agent;
          BuildAgentAtRuntime(spawner, out atBehaviour, out newBornBuilder, out newborn, out newBornAgent, out agent);
          yield return StartCoroutine(NewbornService.GetNewborn(newbornId.Value["id"], agent, false));
          Debug.Log(newbornId.Value["id"]);
          GameObject.Find("S3Service").GetComponent<S3Service>().GetObject(newbornId.Value["id"], agent);
          StartCoroutine(NewbornService.UpdateLivingStatus(newbornId.Value["id"], "true"));
        };
      }
    }

    public void BuildAgentAtRuntime(NewbornSpawner spawner, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder, out NewbornAgent newborn, out GameObject newBornAgent, out GameObject agent)
    {
      Vector3 agentPosition = spawner.ReturnAgentPosition(0);
      agent = spawner.BuildAgent(true, agentPosition, out newBornAgent, out atBehaviour, out newBornBuilder, out newborn);
      if (transform.Find("Ground") != null)
      {
        spawner.AssignGround(transform.Find("Ground").transform);
      }
      agent.GetComponent<TargetController>().target = agent.transform;
    }

    public static IEnumerator SetParentGestationToFalse(KeyValuePair<string, JSONNode> newbornId)
    {
      foreach (System.Collections.Generic.KeyValuePair<string, SimpleJSON.JSONNode> parentsId in newbornId.Value["parents"])
      {
        GameObject parent = GameObject.Find(parentsId.Value);
        if (parent != null && parent.GetComponent<NewbornAgent>().isGestating)
        {
          Debug.Log("Setting Gestation to false for" + parentsId.Value);
          parent.GetComponent<NewbornAgent>().UnsetNewbornInGestation();
          yield return NewbornService.UpdateLivingStatus(parentsId.Value, "true"); // WHY do you need to update the living status here ? 
        }
      }
    }
  }
}