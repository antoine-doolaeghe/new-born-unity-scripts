using UnityEngine;


namespace Components.Newborn
{
  [CreateAssetMenu(fileName = "Joint config", menuName = "jointConfig")]
  public class JointConfig : ScriptableObject
  {
    public float bounciness = 10f;
    public float yLimit = 22.5f;
    public float highLimit = 135f;
    public float lowLimit = 22.5f;
    public float zLimit = 70f;
    public float threshold;
  }
};