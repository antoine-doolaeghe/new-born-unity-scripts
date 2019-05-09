using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Tests
{
  public class AgentTrainBehaviourTests
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
      
      AgentTrainBehaviour at = spawner0.transform.GetChild(0).transform.GetComponent<AgentTrainBehaviour>();
      yield return null;
      // at.TouchedNewborn();
    }
  }
}
