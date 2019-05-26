using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Gene;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gene
{
  [ExecuteInEditMode]
  public class NewbornService : MonoBehaviour
  {
    public string responseUuid;
    public static List<float> cellInfoResponse;
    public GameObject cell;
    public delegate void QueryComplete();
    public static event QueryComplete onQueryComplete;

    public enum Status { Neutral, Loading, Complete, Error };

    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    private NewBornBuilder newBornBuilder;
    private Newborn newborn;

    private static String graphQlInput;

    public delegate void RebuildAgentCallback(Transform transform, WWW www, GameObject agent);


    void Awake()
    {
      newBornBuilder = transform.GetComponent<NewBornBuilder>();
      newborn = transform.GetComponent<Newborn>();
    }

    public static void RebuildAgent(Transform transform, WWW www, GameObject agent)
    {
      Debug.Log("Newborn Model successfully posted!");
      DestroyAgent(transform);
      // HERE you need to make the adjustment for wether what need to be done. 
      cellInfoResponse = new List<float>();
      JSONNode responseData = JSON.Parse(www.text)["data"]["createModel"];
      string responseId = responseData["id"];
      foreach (var cellInfo in responseData["cellInfos"].AsArray)
      {
        cellInfoResponse.Add(cellInfo.Value.AsFloat);
      }
      TrainingManager trainingManager = GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>();
      trainingManager.requestApiData = true;
      trainingManager.BuildNewBornFromFetch(true, responseId, agent);
    }

    public static void SuccessfullReproductionCallback(Transform transform, WWW www, GameObject agent)
    {
      Debug.Log("Successfull Newborn reproduction");
      // TODO: Action after a successfull newborn reproduction;
    }

    public static IEnumerator PostNewborn(NewBornPostData newBornPostData, GameObject agent = null)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      NewbornService.variable["id"] = newBornPostData.id;
      NewbornService.variable["name"] = "\"newborn\"";
      NewbornService.variable["sex"] = "\"demale\"";
      NewbornService.variable["newbornGenerationId"] = newBornPostData.generationId;

      WWW www;
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newBornGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);
      Debug.Log(graphQlInput);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log(www.text);
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        agent.transform.GetComponent<Newborn>().GenerationIndex = JSON.Parse(www.text)["data"]["createNewborn"]["generation"]["index"];
        NewbornService.RebuildAgentCallback handler = NewbornService.RebuildAgent;
        yield return agent.transform.GetComponent<NewBornBuilder>().PostNewbornModel(createdNewBornId, 0, agent, handler); // will it always be first generation
      }
    }

    public static IEnumerator PostReproducedNewborn(NewBornPostData newBornPostData, GameObject agent, GameObject agentPartner)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      NewbornService.variable["id"] = newBornPostData.id;
      NewbornService.variable["name"] = "\"newborn\"";
      NewbornService.variable["sex"] = "\"female\"";
      NewbornService.variable["newbornGenerationId"] = newBornPostData.generationId;

      WWW www;
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newBornGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        agent.transform.GetComponent<Newborn>().childs.Add(createdNewBornId);
        agentPartner.transform.GetComponent<Newborn>().childs.Add(createdNewBornId);
      }
    }


    public static IEnumerator PostNewbornModel(Transform transform, GenerationPostData generationPostData, string modelId, GameObject agent, RebuildAgentCallback callback)
    {
      byte[] postData;

      Dictionary<string, string> postHeader;
      string cellPositionsString = "[";
      for (int i = 0; i < generationPostData.cellPositions.Count; i++)
      {
        cellPositionsString = cellPositionsString + JSON.Parse(JsonUtility.ToJson(generationPostData.cellPositions[i]))["position"].ToString() + ",";
      }
      cellPositionsString = cellPositionsString + "]";
      NewbornService.variable["id"] = generationPostData.id;
      NewbornService.variable["modelNewbornId"] = modelId;
      NewbornService.variable["cellPositions"] = cellPositionsString;
      NewbornService.variable["cellInfos"] = JSON.Parse(JsonUtility.ToJson(generationPostData))["cellInfos"].ToString();

      WWW www;
      ServiceHelpers.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.modelGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        callback(transform, www, agent);
      }
    }

    public static IEnumerator GetNewborn(string id, GameObject agent, bool IsGetAfterPost)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;

      NewbornService.variable["id"] = id;
      WWW www;
      ServiceHelpers.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newBornGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("NewBorn successfully requested!");
        cellInfoResponse = new List<float>();
        Debug.Log(www.text);
        JSONNode responseData = JSON.Parse(www.text)["data"]["getNewborn"];
        string responseId = responseData["id"];
        foreach (var cellInfo in responseData["models"]["items"][0]["cellInfos"].AsArray)
        {
          cellInfoResponse.Add(cellInfo.Value.AsFloat);
        }

        TrainingManager trainingManager = GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>();
        trainingManager.requestApiData = true;
        trainingManager.BuildNewBornFromFetch(false, responseId, agent);
      }
    }

    public static IEnumerator UpdateInstanceId(string id, string instanceId)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      NewbornService.variable["id"] = "\"" + id + "\"";
      NewbornService.variable["instanceId"] = "\"" + instanceId + "\"";
      WWW www;
      ServiceHelpers.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateNewbornInstanceId, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        Debug.Log(www.text);
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text);
        Debug.Log(responseData);
        if (responseData["data"])
        {
          Debug.Log("NewBorn instanceId successfully updated!");
        }
        else
        {
          throw new Exception("There was an error sending request: " + responseData);
        }
      }
    }

    public static void DestroyAgent(Transform transform)
    {
      Transform[] childs = transform.Cast<Transform>().ToArray();
      transform.gameObject.SetActive(true);
      transform.GetComponent<NewBornBuilder>().DeleteCells();
      transform.GetComponent<AgentTrainBehaviour>().DeleteBodyParts();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
    }
  }
}