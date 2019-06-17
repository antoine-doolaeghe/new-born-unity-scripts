using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Newborn
{
  [ExecuteInEditMode]
  public class TrainingService : MonoBehaviour
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
      TrainingService.variable["id"] = "\"" + newbornId + "\"";

      WWW www;
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.trainingGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("Training Instance successfully launched");
        string instanceId = JSON.Parse(www.text)["data"]["start"];
        yield return NewbornService.UpdateInstanceId(newbornId, instanceId);
      }
    }

    public static IEnumerator UpdateTrainingStage(string newbornId, string stage)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      TrainingService.variable["id"] = "\"" + newbornId + "\"";
      TrainingService.variable["stage"] = "\"" + stage + "\"";

      WWW www;
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.updateTrainingStageQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log(JSON.Parse(www.text));
        Debug.Log("Training Stage successfully updated");
      }
    }

    public static IEnumerator StreamNewbornTrainingModel(string newbornId)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      TrainingService.variable["id"] = "\"" + newbornId + "\"";
      Debug.Log("Requesting the trained newborn model");
      WWW www;
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.trainingGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("Training Instance successfully launched");
        string instanceId = JSON.Parse(www.text)["data"]["start"];
        yield return NewbornService.UpdateInstanceId(newbornId, instanceId);
      }
    }
  }
}