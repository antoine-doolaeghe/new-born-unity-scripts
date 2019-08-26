using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Service.Newborn;
using System.IO.Abstractions.TestingHelpers;
using Components.Generation;
namespace Newborn
{
  public class NewbornServiceTest : NewbornService
  {
    [Test]
    public void TestGenerationPostData()
    {
      string modelId = "TestId";
      int agentId = 12345;
      GenerationPostData generationPostData = new GenerationPostData("newbornId", new List<PositionPostData>(), new List<float>(), new List<string>());
      NewbornService newbornService = new NewbornService();
      // newbornService.PostNewbornModel(generationPostData, modelId, agentId);
    }
  }
}
