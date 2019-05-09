using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sex config", menuName = "agentConfig")]
public class SexConfig : ScriptableObject
{
  public static List<string> sexes = new List<string>() { "male", "female" };
}