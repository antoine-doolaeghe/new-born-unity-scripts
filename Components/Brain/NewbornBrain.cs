using System.Text.RegularExpressions;
using MLAgents;
using Components.Newborn;

namespace Components.Newborn.Util
{
  public class NewbornBrain
  {
    public static void SetBrainParameters(AgentTrainBehaviour atBehaviour, int CellCount)
    {
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { CellCount * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = CellCount * 13 - 4;
    }

    public static void SetBrainName(AgentTrainBehaviour atBehaviour, string responseId)
    {
      atBehaviour.brain.name = responseId;
    }

    public static string GenerateRandomName()
    {
      return Regex.Replace(System.Guid.NewGuid().ToString(), @"[^0-9]", "");
    }
  }
}