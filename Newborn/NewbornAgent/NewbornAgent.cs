using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewbornAgent : MonoBehaviour
{
  private string bio;
  private string bornPlace;
  private string hexColor;
  private string id;
  public List<string> childs = new List<string>();
  public string title;
  public string Sex;
  public bool isGestating;
  public int GenerationIndex;
  public string GenerationId;
  public GameObject CellPrefab;
  public MLAgents.InferenceBrain.NNModel model;
  public List<List<GameObject>> NewBornGenerations;
  public List<GameObject> Cells;
  public List<Vector3> CellPositions;
  public List<Vector3> CelllocalPositions;
  public List<GeneInformation> GeneInformations;
}
