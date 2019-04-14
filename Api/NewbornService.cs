using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.IO;
using System;
using Gene;
using UnityEngine;
using UnityEngine.Networking;
using graphQLClient;
using SimpleJSON;
using UnityEngine.UI;

namespace Gene
{
  public class NewbornService : MonoBehaviour
  {
    public List<float> response;
    public string responseUuid;
    public GameObject cell;
    public ApiConfig apiConfig;
    public delegate void QueryComplete();
    public static event QueryComplete onQueryComplete;

    public enum Status { Neutral, Loading, Complete, Error };

    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    private TrainingManager trainingManager;

    void Awake()
    {
      trainingManager = GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>();
    }

    public IEnumerator postNewborn(NewBornPostData newBornPostData, int agentId)
    {
      string jsonData;
      byte[] postData;
      Dictionary<string, string> postHeader;

      string newBornGraphQlMutation = apiConfig.newBornGraphQlMutation;
      NewbornService.variable["id"] = newBornPostData.id;
      NewbornService.variable["name"] = newBornPostData.name;
      newBornGraphQlMutation = QuerySorter(newBornGraphQlMutation);
      jsonData = NewbornServiceHelpers.ReturnJsonData(newBornGraphQlMutation);
      NewbornServiceHelpers.ConfigureForm(jsonData, out postData, out postHeader);
      WWW www = new WWW(apiConfig.url, postData, postHeader);
      yield return www;
      Debug.Log(newBornGraphQlMutation);
      Debug.Log(www.text);
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        transform.GetComponent<NewBornBuilder>().PostGeneration(createdNewBornId, 0, agentId); // will it always be first generation
      }
    }


    public IEnumerator postGeneration(GenerationPostData generationPostData, string modelId, int agentId)
    {
      string jsonData;
      byte[] postData;
      Dictionary<string, string> postHeader;
      string modelGraphQlMutation = apiConfig.modelGraphQlMutation;
      NewbornService.variable["id"] = generationPostData.id;
      NewbornService.variable["modelNewbornId"] = modelId;
      NewbornService.variable["cellPositions"] = JSON.Parse(JsonUtility.ToJson(generationPostData))["cellPositions"].ToString();
      NewbornService.variable["cellInfos"] = JSON.Parse(JsonUtility.ToJson(generationPostData))["cellInfos"].ToString();
      modelGraphQlMutation = QuerySorter(modelGraphQlMutation);
      jsonData = NewbornServiceHelpers.ReturnJsonData(modelGraphQlMutation);
      NewbornServiceHelpers.ConfigureForm(jsonData, out postData, out postHeader);

      WWW www = new WWW(apiConfig.url, postData, postHeader);
      Debug.Log(modelGraphQlMutation);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("New Generation successfully posted!");
        Transform[] childs = transform.Cast<Transform>().ToArray();
        DestroyAgent(childs);
        response = new List<float>();
        Debug.Log(JSON.Parse(www.text));
        string responseId = JSON.Parse(www.text)["data"]["createModel"]["id"];
        foreach (var cellInfo in JSON.Parse(www.text)["data"]["createModel"]["cellInfos"].AsArray)
        {
          response.Add(cellInfo.Value.AsFloat);
        }

        GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>().requestApiData = true;
        GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>().BuildNewBornFromFetch(true, responseId, agentId);
      }
    }

    public IEnumerator getNewborn(string id, int agentId, bool IsGetAfterPost)
    {
      string jsonData;
      byte[] postData;
      Dictionary<string, string> postHeader;

      string newBornGraphQlQuery = apiConfig.newBornGraphQlQuery;
      NewbornService.variable["id"] = id;
      newBornGraphQlQuery = QuerySorter(newBornGraphQlQuery);
      jsonData = NewbornServiceHelpers.ReturnJsonData(newBornGraphQlQuery);
      NewbornServiceHelpers.ConfigureForm(jsonData, out postData, out postHeader);

      WWW www = new WWW(apiConfig.url, postData, postHeader);
      Debug.Log(newBornGraphQlQuery);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("NewBorn successfully requested!");
        response = new List<float>();
        string responseId = JSON.Parse(www.text)["data"]["getNewborn"]["id"];
        foreach (var cellInfo in JSON.Parse(www.text)["data"]["getNewborn"]["models"]["items"][0]["cellInfos"].AsArray)
        {
          response.Add(cellInfo.Value.AsFloat);
        }

        GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>().requestApiData = true;
        GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>().BuildNewBornFromFetch(false, responseId, agentId);
      }
    }

    public static string QuerySorter(string query)
    {
      string finalString;
      string[] splitString;
      string[] separators = { "$", "^" };
      splitString = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
      finalString = splitString[0];
      for (int i = 1; i < splitString.Length; i++)
      {
        if (i % 2 == 0)
        {
          finalString += splitString[i];
        }
        else
        {
          if (!splitString[i].Contains("[]"))
          {
            finalString += variable[splitString[i]];
          }
          else
          {
            finalString += ArraySorter(splitString[i]);
          }
        }
      }
      return finalString;
    }

    public static string ArraySorter(string theArray)
    {
      string[] anArray;
      string solution;
      anArray = array[theArray];
      solution = "[";
      foreach (string a in anArray)
      {

      }
      for (int i = 0; i < anArray.Length; i++)
      {
        solution += anArray[i].Trim(new Char[] { '"' });
        if (i < anArray.Length - 1)
          solution += ",";
      }
      solution += "]";
      Debug.Log("This is solution " + solution);
      return solution;
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
