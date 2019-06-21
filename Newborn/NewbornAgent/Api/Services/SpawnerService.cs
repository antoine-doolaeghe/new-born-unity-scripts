using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

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
        Debug.Log(JSON.Parse(www.text));
        foreach (System.Collections.Generic.KeyValuePair<string, SimpleJSON.JSONNode> newbornId in JSON.Parse(www.text)["data"]["listNewborns"]["items"])
        {
          AgentTrainBehaviour atBehaviour;
          NewBornBuilder newBornBuilder;
          NewbornAgent newborn;
          GameObject newBornAgent;

          // unset gestation for the female partner
          for (int i = 0; i < newbornId.Value["parents"].Count; i++)
          {
            if (GameObject.Find(newbornId.Value["parents"][i]) != null && GameObject.Find(newbornId.Value["parents"][i]).GetComponent<NewbornAgent>().isGestating)
            {
              Debug.Log("Ending newborn gestation");
              GameObject.Find(newbornId.Value["parents"][i]).GetComponent<NewbornAgent>().UnsetNewbornInGestation();
              StartCoroutine(NewbornService.UpdateLivingStatus(newbornId.Value["parents"][i], "true"));
            }
          }
          Vector3 agentPosition = spawner.GetComponent<NewbornSpawner>().ReturnAgentPosition(0);
          GameObject agent = spawner.GetComponent<NewbornSpawner>().BuildAgent(true, agentPosition, out newBornAgent, out atBehaviour, out newBornBuilder, out newborn);
          if (transform.Find("Ground") != null)
          {
            spawner.GetComponent<NewbornSpawner>().AssignGround(transform.Find("Ground").transform);
          }
          agent.GetComponent<TargetController>().target = agent.transform;
          yield return StartCoroutine(NewbornService.GetNewborn(newbornId.Value["id"], agent, false));
          Debug.Log(newbornId.Value["id"]);
          GameObject.Find("S3Service").GetComponent<S3Service>().GetObject(newbornId.Value["id"], agent);
          StartCoroutine(NewbornService.UpdateLivingStatus(newbornId.Value["id"], "true"));
        };
      }
    }
  }
}