using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Newborn
{
  public class GeneHelper
  {
    public static List<GeneInformation> ReturnMixedForReproduction(List<GeneInformation> femaleGene, List<GeneInformation> maleGene)
    {
      List<GeneInformation> newGeneInformation = new List<GeneInformation>();
      newGeneInformation.Add(new GeneInformation(new List<float>()));
      int longerGeneLength = maleGene[0].info.Count > femaleGene[0].info.Count ? maleGene[0].info.Count : femaleGene[0].info.Count;
      for (int i = 0; i < longerGeneLength; i++)
      {
        if (maleGene[0].info.Count > i && femaleGene[0].info.Count > i)
        {
          float info = (maleGene[0].info[i] + femaleGene[0].info[i]) / 2;
          newGeneInformation[0].info.Add(info);
        }
        else if (maleGene[0].info.Count > i && femaleGene[0].info.Count < i)
        {
          float info = maleGene[0].info[i] - 0.3f;
          newGeneInformation[0].info.Add(info);
        }
        else if (maleGene[0].info.Count < i && femaleGene[0].info.Count > i)
        {
          float info = femaleGene[0].info[i] - 0.3f;
          newGeneInformation[0].info.Add(info);
        }
      }
      return newGeneInformation;
    }
  }
}