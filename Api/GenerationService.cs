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
  public class GenerationService : MonoBehaviour
  {
    public ApiConfig apiConfig;
    private String graphQlInput;
    public List<string> generations;
    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

    public IEnumerator GetGenerations()
    {
      byte[] postData;
      Dictionary<string, string> postHeader;
      WWW www;
      generations = new List<string>();
      ServiceHelpers.graphQlApiRequest(variable, array, out postData, out postHeader, out www, out graphQlInput, apiConfig.generationsGraphQlQuery, apiConfig.apiKey, apiConfig.url);
      Debug.Log(apiConfig.apiKey);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        generations.Clear();
        foreach (var generation in JSON.Parse(www.text)["data"]["listGenerations"]["items"].AsArray)
        {
          generations.Add(generation.Value["id"]);
        }
      }
    }

    public IEnumerator PostGeneration(string generationId, int generationIndex)
    {
      byte[] postData;
      Dictionary<string, string> postHeader;

      GenerationService.variable["id"] = generationId;
      GenerationService.variable["index"] = generationIndex.ToString();

      WWW www;
      ServiceHelpers.graphQlApiRequest(GenerationService.variable, GenerationService.array, out postData, out postHeader, out www, out graphQlInput, apiConfig.generationsGraphQlMutation, apiConfig.apiKey, apiConfig.url);
      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log(JSON.Parse(www.text));
        generations.Add(JSON.Parse(www.text)["data"]["createGeneration"]["id"]);
      }
    }
  }
}