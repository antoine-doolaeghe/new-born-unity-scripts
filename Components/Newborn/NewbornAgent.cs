using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using SimpleJSON;
using Components.Newborn.Gene;
using Components.Newborn.Anatomy;

namespace Components.Newborn
{
  public class NewbornAgent : MonoBehaviour
  {
    private string bio;
    private string bornPlace;
    private string hexColor;
    private string id;
    public List<string> childs = new List<string>();
    public List<string> partners = new List<string>();
    public List<string> parents = new List<string>();
    public string title;
    public string Sex;
    public bool isGestating;
    public bool isReproducing;
    public int GenerationIndex;
    public string GenerationId;
    public GameObject CellPrefab;
    public LearningBrain learningBrain;
    public PlayerBrain playerBrain;
    public List<List<AnatomyCell>> NewBornGenerations;
    public List<GameObject> Cells;
    public List<Vector3> CellPositions;
    public List<Vector3> CelllocalPositions;
    public List<GeneInformation> GeneInformations;

    public void SetNewbornInGestation()
    {
      isGestating = true;
    }

    public void UnsetNewbornInGestation()
    {
      isGestating = true;
    }

    public void AssignNewbornInfoFromResponse(SimpleJSON.JSONNode newbornResponseData)
    {
      Sex = newbornResponseData["sex"];
      title = newbornResponseData["name"];
      GenerationId = newbornResponseData["generation"]["id"];
      GenerationIndex = newbornResponseData["generation"]["index"];
      childs = ReturnNewbornChildsFromResponse(newbornResponseData);
      parents = ReturnNewbornParentsFromResponse(newbornResponseData);
    }

    public void AssignNewbornModelInfoFromResponse(SimpleJSON.JSONNode modelResponseData)
    {
      List<float> newbornModelInfo = new List<float>();
      foreach (var cellInfo in modelResponseData["cellInfos"].AsArray)
      {
        newbornModelInfo.Add(cellInfo.Value.AsFloat);
      }

      GeneInformations.Add(new GeneInformation(new List<float>()));
      GeneInformations[0].info = newbornModelInfo;
    }

    public static List<string> ReturnNewbornChildsFromResponse(SimpleJSON.JSONNode newbornResponseData)
    {
      List<string> newbornChilds = new List<string>();
      if (newbornResponseData["childs"] != null)
      {
        foreach (var child in newbornResponseData["childs"].AsArray)
        {
          newbornChilds.Add(child.Value);
        }

        return newbornChilds;
      }
      return null;
    }

    private static List<string> ReturnNewbornParentsFromResponse(SimpleJSON.JSONNode newbornResponseData)
    {
      List<string> newbornParents = new List<string>();
      if (newbornResponseData["parents"] != null)
      {
        foreach (var parent in newbornResponseData["parents"].AsArray)
        {
          newbornParents.Add(parent.Value);
        }
        return newbornParents;
      }
      return null;
    }
  }

}