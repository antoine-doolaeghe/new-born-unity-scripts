using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CollisionController : MonoBehaviour
{
  public Transform target;
  [Header("Detect Targets")]
  public bool touchingNewborn;
  public bool touchingFood;
  private const string NewbornTag = "newborn";

  private const string FoodTag = "food";

  void OnCollisionEnter(Collision col)
  {
    if (col.transform.CompareTag(NewbornTag))
    {
      touchingNewborn = true;
    }
    if (col.transform.CompareTag(FoodTag))
    {
      touchingFood = true;
    }
  }

  void OnCollisionExit(Collision other)
  {
    if (other.transform.CompareTag(NewbornTag))
    {
      touchingNewborn = false;
    }
    if (other.transform.CompareTag(FoodTag))
    {
      touchingFood = false;
    }
  }
}
