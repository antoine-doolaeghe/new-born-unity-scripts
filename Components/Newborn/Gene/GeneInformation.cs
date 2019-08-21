using System.Collections.Generic;

namespace Components.Newborn.Gene
{
  [System.Serializable]
  public class GeneInformation
  {
    public List<float> info;
    public GeneInformation(List<float> info)
    {
      this.info = info;
    }
  }
}