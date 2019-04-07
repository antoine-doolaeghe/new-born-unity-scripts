using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
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
public class NewbornServiceHelpers : MonoBehaviour
{

  public class Query
  {
    public string query;
  }
  public static string ReturnJsonData(string mutationString)
  {
    Query query = new Query();
    string jsonData = "";
    query = new Query { query = mutationString };
    jsonData = JsonUtility.ToJson(query);
    return jsonData;
  }

  public static void ConfigureForm(string jsonData, out byte[] postData, out Dictionary<string, string> postHeader)
  {
    WWWForm form = new WWWForm();
    postData = Encoding.ASCII.GetBytes(jsonData);
    postHeader = form.headers;
    postHeader.Add("X-Api-Key", "da2-rbxq3r664bapfeghuz2znft5r4");
    if (postHeader.ContainsKey("Content-Type"))
      postHeader["Content-Type"] = "application/json";
    else
      postHeader.Add("Content-Type", "application/json");
  }

  public static IEnumerator WaitForRequest(WWW data)
  {
    yield return data; // Wait until the download is done
    if (data.error != null)
    {
      Debug.Log("There was an error sending request: " + data.error);
    }
    else
    {
      Debug.Log(data.text);
    }
  }
}
