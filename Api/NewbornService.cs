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
    public List<float> cellInfoResponse;
    public GameObject cell;
    public ApiConfig apiConfig;
    public delegate void QueryComplete();
    public static event QueryComplete onQueryComplete;

    public enum Status { Neutral, Loading, Complete, Error };

    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    private TrainingManager trainingManager;
    private NewBornBuilder newBornBuilder;
    private Newborn newborn;

    private String graphQlInput;

    void Awake()
    {
      trainingManager = GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>();
      newBornBuilder = transform.GetComponent<NewBornBuilder>();
      newborn = transform.GetComponent<Newborn>();
    }

    public IEnumerator PostNewborn(NewBornPostData newBornPostData, int agentId)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      Debug.Log("HERE2");
      NewbornService.variable["id"] = newBornPostData.id;
      NewbornService.variable["name"] = newBornPostData.name;
      NewbornService.variable["sex"] = newBornPostData.sex;
      NewbornService.variable["newbornGenerationId"] = newBornPostData.generationId;

      WWW www;
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, apiConfig.newBornGraphQlMutation, apiConfig.apiKey, apiConfig.url);
      Debug.Log(graphQlInput);
      yield return www;
      if (www.error != null)
      {
        Debug.Log(www.text);
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log(JSON.Parse(www.text));
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        newborn.GenerationIndex = JSON.Parse(www.text)["data"]["createNewborn"]["generation"]["index"];
        // Bring the newborn information to the generation object
        newBornBuilder.PostNewbornModel(createdNewBornId, 0, agentId); // will it always be first generation
      }
    }


    public IEnumerator PostNewbornModel(GenerationPostData generationPostData, string modelId, int agentId)
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
      ServiceHelpers.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, apiConfig.modelGraphQlMutation, apiConfig.apiKey, apiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("New Model successfully posted!");
        Transform[] childs = transform.Cast<Transform>().ToArray();
        DestroyAgent(childs);
        // HERE you need to make the adjustment for wether what need to be done. 
        cellInfoResponse = new List<float>();
        JSONNode responseData = JSON.Parse(www.text)["data"]["createModel"];
        string responseId = responseData["id"];
        foreach (var cellInfo in responseData["cellInfos"].AsArray)
        {
          cellInfoResponse.Add(cellInfo.Value.AsFloat);
        }

        trainingManager.requestApiData = true;
        trainingManager.BuildNewBornFromFetch(true, responseId, agentId);
      }
    }

    public IEnumerator GetNewborn(string id, int agentId, bool IsGetAfterPost)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;

      NewbornService.variable["id"] = id;

      WWW www;
      ServiceHelpers.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, apiConfig.newBornGraphQlQuery, apiConfig.apiKey, apiConfig.url);

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

        trainingManager.requestApiData = true;
        trainingManager.BuildNewBornFromFetch(false, responseId, agentId);
      }
    }

    public void DestroyAgent(Transform[] childs)
    {
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