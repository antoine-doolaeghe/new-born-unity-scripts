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
    [UnitySetUp]
    public IEnumerator TestSceneSetup()
    {
      Scene testScene = SceneManager.GetActiveScene();
      yield return SceneManager.LoadSceneAsync("MockTestScene", LoadSceneMode.Single);
      SceneManager.SetActiveScene(SceneManager.GetSceneByName("MockTestScene"));
    }

    [UnityTest]
    public IEnumerator TestActiveScene()
    {
      Assert.IsTrue(SceneManager.GetActiveScene().name == "MockTestScene");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestCollisionController()
    {
      GameObject spawner0 = GameObject.Find("Spawner0");
      GameObject spawner1 = GameObject.Find("Spawner1");

      CollisionController collisionControllerA = spawner0.transform.GetChild(0).transform.GetChild(0).transform.GetComponent<CollisionController>();
      CollisionController collisionControllerB = spawner1.transform.GetChild(0).transform.GetChild(0).transform.GetComponent<CollisionController>();

      yield return spawner0.transform.GetChild(0).transform.position = new Vector3(1f, 1f, 1f);
      yield return spawner1.transform.GetChild(0).transform.position = new Vector3(-1f, -1f, -1f);

      Assert.IsFalse(collisionControllerA.touchingNewborn);
      Assert.IsFalse(collisionControllerB.touchingNewborn);

      yield return spawner0.transform.GetChild(0).transform.position = new Vector3(0f, 0f, 0f);
      yield return spawner1.transform.GetChild(0).transform.position = new Vector3(0f, 0f, 0f);

      Assert.IsTrue(collisionControllerA.touchingNewborn);
      Assert.IsTrue(collisionControllerB.touchingNewborn);

      yield return spawner0.transform.GetChild(0).transform.position = new Vector3(1f, 1f, 1f);
      yield return spawner1.transform.GetChild(0).transform.position = new Vector3(-1f, -1f, -1f);

      Assert.IsFalse(collisionControllerA.touchingNewborn);
      Assert.IsFalse(collisionControllerB.touchingNewborn);
    }
  }
}
