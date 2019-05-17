using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Agent config", menuName = "agentConfig")]
public class AgentConfig : ScriptableObject
{
  public static float bounciness = 10f;
  public static float yLimit = 22.5f;
  public static float highLimit = 135f;
  public static float lowLimit = 22.5f;
  public static float zLimit = 0f;
  public static float threshold = 0.75f;
  public static int layerNumber = 11;
}