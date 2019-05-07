using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Gene
{
  public class GeneHelper
  {
    public static void returnMixedForReproduction(List<GeneInformation> femaleGene, List<GeneInformation> maleGene)
    {
      Debug.Log(maleGene[0].info.Count);
      Debug.Log(femaleGene[0].info.Count);
    }
  }
}