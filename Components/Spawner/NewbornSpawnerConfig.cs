using UnityEngine;


namespace Components.Spawner
{
  [CreateAssetMenu(fileName = "Newborn Spawner Config", menuName = "NewbornSpawnerConfig")]
  public class NewbornSpawnerConfig : ScriptableObject
  {
    public GameObject AgentPrefab;
    public int agentNumber;
    public bool isRandomSpawn;
    public float spawnRange;
    public bool instantiateSingleBrain;
    public bool control;
  }
};