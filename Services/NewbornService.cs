﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using Service.Generation;
using Service.Trainer;
using Components.Newborn;
using Components.Generation;
using Components.Newborn.Anatomy;

namespace Service.Newborn
{
  [ExecuteInEditMode]
  public class NewbornService : MonoBehaviour
  {
    public string responseUuid;
    public GameObject cell;
    public delegate void QueryComplete();
    public static event QueryComplete onQueryComplete;
    public enum Status { Neutral, Loading, Complete, Error };
    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();
    private NewbornAgent newborn;
    private static String graphQlInput;
    static byte[] postData;
    static Dictionary<string, string> postHeader;
    public delegate IEnumerator PostModelCallback(Transform transform, WWW www, GameObject agent, string newbornId);

    public delegate IEnumerator PostNewbornFromReproductionCallback(GameObject agent, GameObject agentPartner, string newbornId);

    public static IEnumerator GetNewborn(string id, GameObject agent, bool IsPlayMode)
    {
      NewbornService.variable["id"] = id;
      WWW www;
      ServiceUtils.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newBornGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("❌There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["getNewborn"];
        if (responseData == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        Debug.Log("NewBorn successfully requested!");
        agent.GetComponent<NewbornAgent>().AssignNewbornInfoFromResponse(responseData);
        agent.GetComponent<NewbornAgent>().AssignNewbornModelInfoFromResponse(responseData["models"]["items"][0]);
        agent.GetComponent<AnatomyBuilder>().BuildNewbornFromResponse(agent, responseData["id"]);
      }
    }

    #region Update Methods
    public static IEnumerator UpdateInstanceId(string id, string instanceId)
    {
      NewbornService.variable["id"] = "\"" + id + "\"";
      NewbornService.variable["instanceId"] = "\"" + instanceId + "\"";
      WWW www;
      ServiceUtils.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateNewbornInstanceId, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("❌There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["updateNewborn"];
        if (responseData == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        Debug.Log("NewBorn instanceId successfully updated!");
      }
    }

    public static IEnumerator UpdateTrainedStatus(string id, string status)
    {
      NewbornService.variable["id"] = "\"" + id + "\"";
      NewbornService.variable["status"] = status;
      WWW www;
      ServiceUtils.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateTrainedStatus, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        Debug.Log(www.text);
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["updateNewborn"];
        if (responseData == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        Debug.Log("NewBorn instanceId successfully updated!");
      }
    }

    public static IEnumerator UpdateLivingStatus(string id, string status)
    {
      NewbornService.variable["id"] = "\"" + id + "\"";
      NewbornService.variable["status"] = status;
      WWW www;
      ServiceUtils.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateLivingStatusQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        Debug.Log(www.text);
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["updateNewborn"];
        if (responseData["data"]["updateNewborn"] == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        Debug.Log("NewBorn Living status successfully updated!");
      }
    }

    public static IEnumerator UpdateTrainingStage(string newbornId, string stage)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      NewbornService.variable["id"] = "\"" + newbornId + "\"";
      NewbornService.variable["stage"] = "\"" + stage + "\"";

      WWW www;
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateTrainingStageQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["updateNewborn"];
        if (responseData == null)
        {
          throw new Exception("❌There was an error sending request:" + www.text);
        }
        Debug.Log("Training Stage successfully updated: " + stage);
      }
    }

    public static IEnumerator UpdateNewbornChildsAndPartners(string newbornId, List<string> childs, List<string> partners)
    {
      NewbornService.variable["id"] = "\"" + newbornId + "\"";
      NewbornService.variable["childs"] = ServiceUtils.ReturnNewbornChilds(childs);
      NewbornService.variable["partners"] = ServiceUtils.ReturnNewbornPartners(partners);
      WWW www;
      ServiceUtils.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateNewbornChildsAndPartners, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("❌There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text);
        if (responseData["data"]["updateNewborn"] == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        Debug.Log("NewBorn childs successfully updated!");
      }
    }
    #endregion

    #region Post Methods
    public static IEnumerator PostNewbornModel(Transform transform, GenerationPostData generationPostData, string modelId, GameObject agent, PostModelCallback callback)
    {
      string cellPositionsString = BuildArmPositionString(generationPostData);
      NewbornService.variable["id"] = generationPostData.id;
      NewbornService.variable["modelNewbornId"] = modelId;
      NewbornService.variable["cellPositions"] = cellPositionsString;
      NewbornService.variable["cellPaths"] = JSON.Parse(JsonUtility.ToJson(generationPostData))["cellPaths"].ToString();
      NewbornService.variable["cellInfos"] = JSON.Parse(JsonUtility.ToJson(generationPostData))["cellInfos"].ToString();
      WWW www;
      ServiceUtils.graphQlApiRequest(NewbornService.variable, NewbornService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.modelGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("❌There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["createModel"];
        if (responseData == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        Debug.Log(www);
        yield return callback(transform, www, agent, modelId);
      }
    }


    public static IEnumerator PostNewborn(NewBornPostData newBornPostData, GameObject agent = null)
    {
      NewbornService.variable["id"] = "\"" + newBornPostData.id + "\"";
      NewbornService.variable["name"] = "\"newborn\"";
      NewbornService.variable["sex"] = "\"female\"";
      NewbornService.variable["newbornGenerationId"] = "\"" + newBornPostData.generationId + "\"";

      WWW www;
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newbornGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        Debug.Log(graphQlInput);
        Debug.Log(www.text);
        throw new Exception("❌There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["createNewborn"];
        if (responseData == null)
        {
          throw new Exception("There was an error sending request: " + www.text);
        }
        agent.transform.GetComponent<NewbornAgent>().AssignNewbornInfoFromResponse(responseData);
        NewbornService.PostModelCallback callback = NewbornService.RebuildAgent;
        yield return agent.transform.GetComponent<AnatomyBuilder>().PostNewbornModel(responseData["id"], 0, agent, callback); // will it always be first generation
      }
    }

    public static IEnumerator PostNewbornFromReproduction(NewBornPostData newBornPostData, GameObject agent, GameObject agentPartner, PostNewbornFromReproductionCallback callback)
    {
      NewbornService.variable["id"] = "\"" + newBornPostData.id + "\"";
      NewbornService.variable["name"] = "\"newborn\"";
      NewbornService.variable["sex"] = "\"female\"";
      NewbornService.variable["newbornGenerationId"] = "\"" + GenerationService.generations[GenerationService.generations.Count - 1] + "\"";  // Get the latest generation;;
      NewbornService.variable["parentA"] = "\"" + agent.GetComponent<AgentTrainBehaviour>().brain.name + "\"";
      NewbornService.variable["parentB"] = "\"" + agentPartner.GetComponent<AgentTrainBehaviour>().brain.name + "\"";
      WWW www;
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.postNewbornFromReproductionGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);
      Debug.Log(graphQlInput);
      yield return www;
      if (www.error != null)
      {
        Debug.Log(JSON.Parse(www.text));
        throw new Exception("❌There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["createNewborn"];
        if (responseData == null)
        {
          throw new Exception("There was an error sending request: " + www.text);
        }
        string createdNewBornId = JSON.Parse(www.text)["data"]["createNewborn"]["id"];
        yield return callback(agent, agentPartner, createdNewBornId);
      }
    }

    #endregion

    #region callback methods
    public static IEnumerator RebuildAgent(Transform transform, WWW www, GameObject agent, string newbornId)
    {
      Debug.Log("Newborn Model successfully posted!");
      DestroyAgent(transform);
      JSONNode responseData = JSON.Parse(www.text)["data"]["createModel"];
      agent.GetComponent<NewbornAgent>().AssignNewbornModelInfoFromResponse(responseData);
      agent.GetComponent<AnatomyBuilder>().BuildNewbornFromResponse(agent, responseData["id"]);
      yield return "";
    }

    public static void DestroyAgent(Transform transform)
    {
      Transform[] childs = transform.Cast<Transform>().ToArray();
      transform.gameObject.SetActive(true);
      transform.GetComponent<AnatomyBuilder>().ResetBuilder();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
    }

    private static string BuildArmPositionString(GenerationPostData generationPostData)
    {
      string cellPositionsString = "[";
      for (int i = 0; i < generationPostData.cellPositions.Count; i++)
      {
        cellPositionsString = cellPositionsString + JSON.Parse(JsonUtility.ToJson(generationPostData.cellPositions[i]))["position"].ToString() + ",";
      }
      cellPositionsString = cellPositionsString + "]";
      return cellPositionsString;
    }

    public static IEnumerator SuccessfullModelCallback(Transform transform, WWW www, GameObject agent, string newbornId = null)
    {
      Debug.Log("Successfull Newborn reproduction 🏩💦🍆");
      yield return TrainerService.TrainNewborn(newbornId);
    }

    public static IEnumerator SuccessfullPostNewbornFromReproductionCallback(GameObject agent, GameObject agentPartner, string newbornId)
    {
      agent.transform.GetComponent<NewbornAgent>().childs.Add(newbornId);
      agentPartner.transform.GetComponent<NewbornAgent>().childs.Add(newbornId);
      agent.transform.GetComponent<NewbornAgent>().partners.Add(agentPartner.name);
      agentPartner.transform.GetComponent<NewbornAgent>().partners.Add(agent.name);
      yield return NewbornService.UpdateNewbornChildsAndPartners(agent.name, agent.transform.GetComponent<NewbornAgent>().childs, agent.transform.GetComponent<NewbornAgent>().partners);
      yield return NewbornService.UpdateNewbornChildsAndPartners(agentPartner.name, agentPartner.transform.GetComponent<NewbornAgent>().childs, agentPartner.transform.GetComponent<NewbornAgent>().partners);
      NewbornService.PostModelCallback PostModelCallback = NewbornService.SuccessfullModelCallback;
      yield return agent.GetComponent<AnatomyBuilder>().PostNewbornModel(newbornId, 0, agent, PostModelCallback); // will it always be first generation
    }
    #endregion
  }
}