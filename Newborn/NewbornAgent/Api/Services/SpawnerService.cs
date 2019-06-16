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
  public class SpawnerService : MonoBehaviour
  {
    private static String graphQlInput;
    public static Dictionary<string, string> variable = new Dictionary<string, string>();
    public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();
    static byte[] postData;
    static Dictionary<string, string> postHeader;
    public IEnumerator ListTrainedNewborn(GameObject spawner)
    {
      WWW www;
      ServiceHelpers.graphQlApiRequest(SpawnerService.variable, SpawnerService.array, out postData, out postHeader, out www, out graphQlInput, ApiConfig.newbornsGraphQlQuery, ApiConfig.apiKey, ApiConfig.url);

      yield return www;
      if (www.error != null)
      {
        throw new Exception("There was an error sending request: " + www.error);
      }
      else
      {
        Debug.Log("NewBorn List successfully requested!");
        foreach (System.Collections.Generic.KeyValuePair<string, SimpleJSON.JSONNode> newbornId in JSON.Parse(www.text)["data"]["listNewborns"]["items"])
        {
          AgentTrainBehaviour atBehaviour;
          NewBornBuilder newBornBuilder;
          NewbornAgent newborn;
          GameObject newBornAgent;

          // unset gestation for the female partner
          for (int i = 0; i < newbornId.Value["parents"].Count; i++)
          {
            if (GameObject.Find(newbornId.Value["parents"][i]).GetComponent<NewbornAgent>().isGestating)
            {
              Debug.Log("Ending newborn gestation");
              GameObject.Find(newbornId.Value["parents"][i]).GetComponent<NewbornAgent>().UnsetNewbornInGestation();
              StartCoroutine(NewbornService.UpdateDevelopmentStage(newbornId.Value["parents"][i], "living"));
            }
          }

          GameObject agent = spawner.GetComponent<NewbornSpawner>().BuildAgent(true, TrainingAgentConfig.positions[0], out newBornAgent, out atBehaviour, out newBornBuilder, out newborn);
          yield return StartCoroutine(NewbornService.GetNewborn(newbornId.Value["id"], agent, false));
          GameObject.Find("S3Service").GetComponent<S3Service>().GetObject(newbornId.Value["id"], agent);
          StartCoroutine(NewbornService.UpdateDevelopmentStage(newbornId.Value["id"], "living"));
        };
      }
    }
  }
}