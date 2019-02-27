using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct GenerationPostData
{
  public List<float> cellInfos;
  public List<List<float>> cellPositions;
  public System.Guid id;
  public GenerationPostData(System.Guid id, List<List<float>> cellPositions, List<float> cellInfos)
  {
    this.id = id;
    this.cellInfos = cellInfos;
    this.cellPositions = cellPositions;
  }
}