using System.Collections.Generic;

namespace Components.Newborn.Gene
{
  [System.Serializable]
  public class GeneInformation
  {
    public List<float> info;
    public List<string> path;
    public GeneInformation(List<float> info, List<string> path)
    {
      this.info = info;
      this.path = path;
    }
  }
}