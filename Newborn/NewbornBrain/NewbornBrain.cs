using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLAgents;
using UnityEngine;

namespace Gene
{
  public class NewbornBrain
  {
    public static void SetBrainParameters(AgentTrainBehaviour atBehaviour, int cellNb)
    {
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { cellNb * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = (cellNb) * 13 - 4;
    }

    public static void SetBrainName(AgentTrainBehaviour atBehaviour, string responseId)
    {
      atBehaviour.brain.name = responseId;
    }
  }
}