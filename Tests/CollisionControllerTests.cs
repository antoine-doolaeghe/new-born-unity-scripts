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
      yield return SceneManager.LoadSceneAsync("MockAgentScene", LoadSceneMode.Single);
      SceneManager.SetActiveScene(SceneManager.GetSceneByName("MockAgentScene"));
    }

    [UnityTest]
    public IEnumerator TestActiveScene()
    {
      Assert.IsTrue(SceneManager.GetActiveScene().name == "MockAgentScene");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestCollisionControll()
    {
      GameObject spawner0 = GameObject.Find("Spawner0");
      GameObject spawner1 = GameObject.Find("Spawner1");
      
      CollisionController tcA = spawner0.transform.GetChild(0).transform.GetChild(0).transform.GetComponent<CollisionController>();
      CollisionController tcB = spawner1.transform.GetChild(0).transform.GetChild(0).transform.GetComponent<CollisionController>();

      yield return spawner0.transform.GetChild(0).transform.position = new Vector3(1f, 1f, 1f);
      yield return spawner1.transform.GetChild(0).transform.position = new Vector3(-1f, -1f, -1f);

      Assert.IsFalse(tcA.touchingNewborn);
      Assert.IsFalse(tcB.touchingNewborn);

      yield return spawner0.transform.GetChild(0).transform.position = new Vector3(0f, 0f, 0f);
      yield return spawner1.transform.GetChild(0).transform.position = new Vector3(0f, 0f, 0f);

      Assert.IsTrue(tcA.touchingNewborn);
      Assert.IsTrue(tcB.touchingNewborn);

      yield return spawner0.transform.GetChild(0).transform.position = new Vector3(1f, 1f, 1f);
      yield return spawner1.transform.GetChild(0).transform.position = new Vector3(-1f, -1f, -1f);

      Assert.IsFalse(tcA.touchingNewborn);
      Assert.IsFalse(tcB.touchingNewborn);
    }
  }
}
