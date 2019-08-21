using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Service.Generation
{
  [ExecuteInEditMode]
  public class GenerationService : MonoBehaviour
  {
    private static String graphQlInput;
    public static List<string> generations;
    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    public static IEnumerator GetGenerations()
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      WWW www;
      generations = new List<string>();
      ServiceUtils.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.generationsGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode generationsResponse = JSON.Parse(www.text)["data"]["listGenerations"]["items"];
        if (generationsResponse == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        generations.Clear();
        foreach (var generation in generationsResponse.AsArray)
        {
          generations.Add(generation.Value["id"]);
        }
      }
    }

    public static IEnumerator PostGeneration(string generationId, int generationIndex)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;

      GenerationService.variable["id"] = generationId;
      GenerationService.variable["index"] = generationIndex.ToString();

      WWW www;
      ServiceUtils.graphQlApiRequest(GenerationService.variable, GenerationService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.generationsGraphQlMutation, ApiConfig.apiKey, ApiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        JSONNode responseData = JSON.Parse(www.text)["data"]["createGeneration"];
        if (JSON.Parse(www.text)["data"]["createGeneration"] == null)
        {
          throw new Exception("❌There was an error sending request: " + www.text);
        }
        generations.Add(responseData["id"]);
      }
    }
  }
}