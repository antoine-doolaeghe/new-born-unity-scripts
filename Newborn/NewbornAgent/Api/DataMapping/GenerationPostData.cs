using System;
using System.Collections.Generic;

[Serializable]
public struct GenerationPostData
{
  public List<float> cellInfos;
  public List<PositionPostData> cellPositions;
  public string id;
  public GenerationPostData(string id, List<PositionPostData> cellPositions, List<float> cellInfos)
  {
    this.id = id;
    this.cellInfos = cellInfos;
    this.cellPositions = cellPositions;
  }
}

public struct PositionPostData
{
  public List<float> position;
  public PositionPostData(List<float> position)
  {
    this.position = position;
  }
}