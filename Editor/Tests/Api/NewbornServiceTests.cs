using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.IO.Abstractions.TestingHelpers;

namespace Gene
{
  public class NewbornServiceTest : NewbornService
  {
    [Test]
    public void TestGenerationPostData()
    {
      string modelId = "TestId";
      int agentId = 12345;
      GenerationPostData generationPostData = new GenerationPostData("newbornId", new List<List<float>>(), new List<float>());
      NewbornService newbornService = new NewbornService();
      newbornService.PostNewbornModel(generationPostData, modelId, agentId);
    }
  }
}
