using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Newborn;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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