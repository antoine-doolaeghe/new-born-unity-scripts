using System.Collections.Generic;

namespace Components.Newborn.Gene
{
  public class GeneHelper
  {
    public static List<GeneInformation> ReturnMixedForReproduction(List<GeneInformation> femaleGene, List<GeneInformation> maleGene)
    {
      List<GeneInformation> newGeneInformation = new List<GeneInformation>();
      newGeneInformation.Add(new GeneInformation(new List<float>(), new List<string>()));
      int longerGeneLength = maleGene[0].info.Count > femaleGene[0].info.Count ? maleGene[0].info.Count : femaleGene[0].info.Count;
      for (int i = 0; i < longerGeneLength; i++)
      {
        if (maleGene[0].info.Count > i && femaleGene[0].info.Count > i)
        {
          float info = (maleGene[0].info[i] + femaleGene[0].info[i]) / 2;
          newGeneInformation[0].path.Add(maleGene[0].path[i]); // TODO temporary solution (need to find a logic for path distribution on reprodutcion)
          newGeneInformation[0].info.Add(info);
        }
        else if (maleGene[0].info.Count > i && femaleGene[0].info.Count < i)
        {
          float info = maleGene[0].info[i] - 0.3f;
          newGeneInformation[0].path.Add(maleGene[0].path[i]);// TODO temporary solution (need to find a logic for path distribution on reprodutcion)
          newGeneInformation[0].info.Add(info);
        }
        else if (maleGene[0].info.Count < i && femaleGene[0].info.Count > i)
        {
          float info = femaleGene[0].info[i] - 0.3f;
          newGeneInformation[0].path.Add(maleGene[0].path[i]);// TODO temporary solution (need to find a logic for path distribution on reprodutcion)
          newGeneInformation[0].info.Add(info);
        }
      }
      return newGeneInformation;
    }
  }
}