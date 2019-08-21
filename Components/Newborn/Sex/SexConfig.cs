using System.Collections.Generic;
using UnityEngine;

namespace Components.Newborn.Sex
{
  [CreateAssetMenu(fileName = "Sex config", menuName = "sexConfig")]
  public class SexConfig : ScriptableObject
  {
    public static List<string> sexes = new List<string>() { "male", "female" };
  }
}