using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;

namespace Newborn
{
  public class FoodSpawner : MonoBehaviour
  {
    public int foodNumber;
    public float spawnRange;
    public GameObject StaticTarget;
    public void BuildTarget()
    {
      if (foodNumber == 1)
      {
        GameObject target = Instantiate(StaticTarget, transform);
        target.transform.localPosition = new Vector3(0f, 0f, 0f);
        AssignTarget(transform.GetComponent<NewbornSpawner>().Agents);
      }
      else
      {
        for (int key = 0; key < foodNumber; key++)
        {
          GameObject target = Instantiate(StaticTarget, transform);
          target.transform.localPosition = new Vector3(Random.Range(0f, spawnRange), 0f, Random.Range(0f, spawnRange));
        }
      }
    }

    public void AssignTarget(List<GameObject> newBornAgents)
    {
      for (int y = 0; y < newBornAgents.Count; y++)
      {
        newBornAgents[y].GetComponent<TargetController>().target = transform;
      }
    }
  }
}
