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

    void Awake()
    {
      newBornBuilder = transform.GetComponent<NewBornBuilder>();
      newborn = transform.GetComponent<Newborn>();
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
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        agent.transform.GetComponent<Newborn>().GenerationIndex = JSON.Parse(www.text)["data"]["createNewborn"]["generation"]["index"];
        agent.transform.GetComponent<NewBornBuilder>().PostNewbornModel(createdNewBornId, 0, agent); // will it always be first generation
      }
    }

    public static IEnumerator PostReproducedNewborn(NewBornPostData newBornPostData, GameObject agent, GameObject agentPartner)
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
        // ALL THE DATA RECEIVED SHOULD BE passed to the newborn here
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        agent.transform.GetComponent<Newborn>().childs.Add(createdNewBornId);
        agentPartner.transform.GetComponent<Newborn>().childs.Add(createdNewBornId);
        agent.transform.GetComponent<NewBornBuilder>().PostNewbornModel(createdNewBornId, 0, agent); // will it always be first generation
      }
    }


    public static IEnumerator PostNewbornModel(Transform transform, GenerationPostData generationPostData, string modelId, GameObject agent)
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
    }

    public IEnumerator GetNewborn(string id, GameObject agent, bool IsGetAfterPost)
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