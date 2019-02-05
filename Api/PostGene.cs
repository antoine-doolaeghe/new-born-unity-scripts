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

using UnityEngine.UI;

namespace Gene
{
  public class PostGene : MonoBehaviour
  {
    public List<List<Info>> response;
    public string responseUuid;
    public GameObject cell;
    public ApiConfig apiConfig;

    public IEnumerator postCell(List<CellInfo> cellInfos, List<Position> cellPositions, string cellName, int agentId = 0)
    {
      Debug.Log(cellPositions.Count);
      PostObject postObject = new PostObject(cellInfos, cellPositions, cellName);
      Debug.Log(postObject.cellPositions);

      string jsonString = JsonUtility.ToJson(postObject);
      string url = apiConfig.url;
      Debug.Log(jsonString);
      UnityWebRequest www = UnityWebRequest.Put(url, jsonString);
      yield return www.SendWebRequest();
      if (www.isDone)
      {
        Debug.Log("Agents successfully posted");
        Transform[] childs = transform.Cast<Transform>().ToArray();
        PostResponseObject responseObject = JsonUtility.FromJson<PostResponseObject>(www.downloadHandler.text);
        string uuid = responseObject.uuid;
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
            response = new List<List<Info>>();
            GetResponseObject getResponseObject = JsonUtility.FromJson<GetResponseObject>(www.downloadHandler.text);
            foreach (var cellInfo in getResponseObject.getCellInfo)
            {
              response.Add(cellInfo.info);
            }
            responseUuid = getResponseObject.cellName;
            // TO REFACTOR HERE
            transform.parent.transform.parent.transform.parent.transform.GetComponent<TrainingManager>().requestApiData = true;
            transform.parent.transform.parent.transform.parent.transform.GetComponent<TrainingManager>().BuildAgentCell(IsGetAfterPost, agentId);
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

  [Serializable]
  public struct PostObject
  {
    public List<CellInfo> cellInfos;
    public List<Position> cellPositions;
    public string cellName;

    public PostObject(List<CellInfo> cellInfos, List<Position> cellPositions, string cellName)
    {
      this.cellInfos = cellInfos;
      this.cellPositions = cellPositions;
      this.cellName = cellName;
    }
  }

  public struct PostResponseObject
  {
    public string uuid;

    public PostResponseObject(string uuid)
    {
      this.uuid = uuid;
    }
  }

  public struct GetResponseObject
  {
    public List<GetCellInfo> getCellInfo;
    public string cellName;

    public GetResponseObject(List<GetCellInfo> getCellInfo, string cellName)
    {
      this.getCellInfo = getCellInfo;
      this.cellName = cellName;
    }
  }

  [Serializable]
  public struct GetCellInfo
  {
    public List<Info> info;
    public GetCellInfo(List<Info> info)
    {
      this.info = info;
    }
  }


  [Serializable]
  public struct Position
  {
    public float x;
    public float y;
    public float z;
    public Position(Vector3 pos)
    {
      this.x = pos.x;
      this.y = pos.y;
      this.z = pos.z;
    }
  }
  [Serializable]
  public struct Info
  {
    public string val;
    public Info(string val)
    {
      this.val = val;
    }
  }
  

  [Serializable]
  public struct CellInfo
  {
    public List<Info> infos;
    public CellInfo(List<Info> infos)
    {
      this.infos = infos;
    }
  }
}
