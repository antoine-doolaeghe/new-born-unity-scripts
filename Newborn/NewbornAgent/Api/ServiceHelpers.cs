using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public class ServiceHelpers
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

  public static void ConfigureForm(string jsonData, string apiKey, out byte[] postData, out Dictionary<string, string> postHeader)
  {
    WWWForm form = new WWWForm();
    postData = Encoding.ASCII.GetBytes(jsonData);
    postHeader = form.headers;
    postHeader.Add("X-Api-Key", apiKey);
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

  public static void graphQlApiRequest(Dictionary<string, string> variable, Dictionary<string, string[]> array, out byte[] postData, out Dictionary<string, string> postHeader, out WWW www, out string graphQlInput, string input, string apiKey, string url)
  {
    string jsonData;
    graphQlInput = QuerySorter(input, variable, array);
    jsonData = ServiceHelpers.ReturnJsonData(graphQlInput);
    ConfigureForm(jsonData, apiKey, out postData, out postHeader);
    www = new WWW(url, postData, postHeader);
  }

  public static string QuerySorter(string query, Dictionary<string, string> variable, Dictionary<string, string[]> array)
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
          finalString += ArraySorter(splitString[i], array);
        }
      }
    }
    return finalString;
  }

  public static string ArraySorter(string theArray, Dictionary<string, string[]> array)
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

  public static string ReturnNewbornChilds(List<string> childs)
  {
    string childString = "";
    foreach (var child in childs)
    {
      childString = childString + "\"" + child + "\"" + ",";
    }

    return childString;
  }

  public static string ReturnNewbornPartners(List<string> partners)
  {
    string partnerString = "";
    foreach (var partner in partners)
    {
      partnerString = partnerString + "\"" + partner + "\"" + ",";
    }

    return partnerString;
  }
}