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
  public class TrainingService : MonoBehaviour
  {
    public string responseUuid;

    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    private static String graphQlInput;

    public delegate void RebuildAgentCallback(Transform transform, WWW www, GameObject agent);


    public static IEnumerator TrainNewborn(string newbornId)
    {
      // TODO: call the newborn instance with the new newborn id
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
        Debug.Log(www.text);
      }
    }
  }
}