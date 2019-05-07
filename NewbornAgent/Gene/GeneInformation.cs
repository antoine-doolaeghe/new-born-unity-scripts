using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLAgents;
using UnityEngine;

[System.Serializable]
public class GeneInformation
{
  public List<float> info;
  public GeneInformation(List<float> info)
  {
    this.info = info;
  }
}