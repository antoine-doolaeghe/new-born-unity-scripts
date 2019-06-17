using System.Collections.Generic;

[System.Serializable]
public class GeneInformation
{
  public List<float> info;
  public GeneInformation(List<float> info)
  {
    this.info = info;
  }
}