using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Tests
{
  public class CollisionControllerTests
  {
    [UnityTest]
    public IEnumerator TestActiveScene()
    {
      Assert.IsTrue(SceneManager.GetActiveScene().name == "MockAgentScene");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestCollisionControll()
    {
      GameObject agentSphereA = GameObject.Find("SphereA");
      GameObject agentSphereB = GameObject.Find("SphereB");
      CollisionController tcA = agentSphereA.GetComponent<CollisionController>();
      CollisionController tcB = agentSphereB.GetComponent<CollisionController>();

      yield return agentSphereA.transform.position = new Vector3(1f, 1f, 1f);
      yield return agentSphereB.transform.position = new Vector3(-1f, -1f, -1f);

      Assert.IsFalse(tcA.touchingNewborn);
      Assert.IsFalse(tcB.touchingNewborn);

      yield return agentSphereA.transform.position = new Vector3(0f, 0f, 0f);
      yield return agentSphereB.transform.position = new Vector3(0f, 0f, 0f);

      Assert.IsTrue(tcA.touchingNewborn);
      Assert.IsTrue(tcB.touchingNewborn);

      yield return agentSphereB.transform.position = new Vector3(10f, 10f, 10f);

      Assert.IsFalse(tcA.touchingNewborn);
      Assert.IsFalse(tcB.touchingNewborn);
    }
  }
}
