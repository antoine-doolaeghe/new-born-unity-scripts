using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Newborn : MonoBehaviour
{
  public string Sex;
  public GameObject CellPrefab;
  public List<List<GameObject>> NewBornGenerations;
  public List<GameObject> Cells;
  public List<Vector3> CellPositions;
  public List<Vector3> CelllocalPositions;
  public List<GeneInformation> GeneInformations;
}
