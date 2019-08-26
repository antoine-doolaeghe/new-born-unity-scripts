using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.IO.Abstractions.TestingHelpers;
using Components.Newborn.Gene;

namespace Newborn
{
  public class GeneHelperTest : GeneHelper
  {
    [Test]
    public void ReturnMixedForReproductionTest()
    {
      List<GeneInformation> femaleGeneInformation = new List<GeneInformation>() { new GeneInformation(new List<float>(), new List<string>()) };
      List<GeneInformation> maleGeneInformation = new List<GeneInformation>() { new GeneInformation(new List<float>(), new List<string>()) };
      GeneHelperTest.ReturnMixedForReproduction(femaleGeneInformation, maleGeneInformation);
    }
  }
}
