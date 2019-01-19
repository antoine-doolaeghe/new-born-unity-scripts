using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gene;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gene
{
  public class PostGene : MonoBehaviour
  {
    public string[] response;
    public string responseUuid;
    public GameObject cell;
    public ApiConfig apiConfig;

    public IEnumerator postCell(string cellInfos, string cellName, int agentId = 0)
    {
      PostObject postObject = new PostObject(cellInfos, cellName);
      string jsonString = JsonUtility.ToJson(postObject);
      string url = apiConfig.url;
      UnityWebRequest www = UnityWebRequest.Put(url, jsonString);
      yield return www.SendWebRequest();
      if (www.isDone)
      {
        Debug.Log("Agents successfully posted");
        Transform[] childs = transform.Cast<Transform>().ToArray();
        string uuid = www.downloadHandler.text.Split('"')[5]; // TO-DO REFACTOR THIS
        DestroyAgent(childs);
        // THEN REQUEST NEW AGENT INFO WITH RECEIVED UUID
        StartCoroutine(getCell(uuid, agentId, true));
      }
      else if (www.isNetworkError || www.isHttpError)
      {
        Debug.Log("There has been en error while posting the agent");
      }
    }

    public IEnumerator getCell(string id, int agentId, bool IsGetAfterPost)
    {
      using (UnityWebRequest www = UnityWebRequest.Get(apiConfig.url + id))
      {
        yield return www.Send();

        if (www.isNetworkError || www.isHttpError)
        {
          Debug.Log(www.error);
        }
        else
        {
          if (www.isDone)
          {
            Debug.Log("Agent Cell Successfully Requested");
            response = www.downloadHandler.text.Split(',')[0].Split('A');
            responseUuid = www.downloadHandler.text.Split(',')[1].Split('n')[1].Split(' ')[0];
            // TO REFACTOR HERE
            transform.parent.transform.parent.transform.parent.transform.GetComponent<TrainingSpawner>().requestApiData = true;
            transform.parent.transform.parent.transform.parent.transform.GetComponent<TrainingSpawner>().BuildAgentCell(IsGetAfterPost, agentId);
          }
        }
      }
    }

    private void DestroyAgent(Transform[] childs)
    {
      transform.gameObject.SetActive(true);
      transform.GetComponent<Cell>().DeleteCells();
      transform.GetComponent<AgentTrainBehaviour>().DeleteBodyParts();
      foreach (Transform child in childs)
      {
        DestroyImmediate(child.gameObject);
      }
    }

  }

  public class PostObject
  {
    public string cellInfos;
    public string cellName;

    public PostObject(string cellInfos, string cellName)
    {
      this.cellInfos = cellInfos;
      this.cellName = cellName;
    }
  }
}