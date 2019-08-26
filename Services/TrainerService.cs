using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using MLAgents;
using Service.Newborn;

namespace Service.Trainer
{
  [ExecuteInEditMode]
  public class TrainerService : MonoBehaviour
  {
    public string responseUuid;

    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    private static String graphQlInput;

    public delegate void PostModelCallback(Transform transform, WWW www, GameObject agent);


    public static IEnumerator TrainNewborn(string newbornId)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      TrainerService.variable["id"] = "\"" + newbornId + "\"";

      WWW www;
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.trainingGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["start"];
        if (responseData == null)
        {
          throw new Exception("There was an error sending request: " + www.text);
        }
        Debug.Log("Training Instance successfully launched");
        string instanceId = responseData;
        yield return NewbornService.UpdateInstanceId(newbornId, instanceId);
      }
    }

    public IEnumerator DownloadTrainer(string TrainerName)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      TrainerService.variable["key"] = "\"" + TrainerName + "\"";
      WWW www;
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.downloadTrainerData, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error + www.text);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["downloadTrainer"];
        if (responseData == null)
        {
          throw new Exception("There was an error sending request: " + www.text);
        }
        Debug.Log(responseData);


        Debug.Log("<b><color=green>[DownloadTrainer Success]</color></b>");
        yield return FindObjectOfType<BuildStorage>().LoadDataFile(responseData);
        // TODO should you always get the command like args
        FindObjectOfType<Academy>().GetCommandLineArgs();
      }
    }

    public static IEnumerator UpdateTrainerData(string TrainerName, string TrainerData)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      TrainerData = TrainerData.Replace("\"", "'");
      TrainerService.variable["key"] = "\"" + TrainerName + "\"";
      TrainerService.variable["data"] = "\"" + TrainerData + "\"";

      WWW www;
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateTrainerData, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error + www.text);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["updateTrainer"];
        if (responseData == null)
        {
          throw new Exception("There was an error sending request: " + www.text);
        }
        Debug.Log("<b><color=green>[UpdateTrainer Success]</color></b>");
      }
    }
  }
}