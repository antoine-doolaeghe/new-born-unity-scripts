using System;
using System.Collections.Generic;

namespace Components.Generation
{
  [Serializable]
  public struct GenerationPostData
  {
    public List<float> cellInfos;

    public List<string> cellPaths;
    public List<PositionPostData> cellPositions;
    public string id;
    public GenerationPostData(string id, List<PositionPostData> cellPositions, List<float> cellInfos, List<string> cellPaths)
    {
      this.id = id;
      this.cellInfos = cellInfos;
      this.cellPaths = cellPaths;
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
}