using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.IO.Abstractions.TestingHelpers;

namespace Gene
{
  public class GeneHelperTest : GeneHelper
  {
    [Test]
    public void ReturnMixedForReproductionTest()
    {
      List<GeneInformation> femaleGeneInformation = new List<GeneInformation>(){new GeneInformation(new List<float>())};
      List<GeneInformation> maleGeneInformation = new List<GeneInformation>(){new GeneInformation(new List<float>())};
      GeneHelperTest.ReturnMixedForReproduction(femaleGeneInformation, maleGeneInformation);
    }
  }
}
