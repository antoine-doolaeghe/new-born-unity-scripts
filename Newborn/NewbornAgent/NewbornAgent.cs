using System.Collections.Generic;
using UnityEngine;
using MLAgents;

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
  public List<List<GameObject>> NewBornGenerations;
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
}
