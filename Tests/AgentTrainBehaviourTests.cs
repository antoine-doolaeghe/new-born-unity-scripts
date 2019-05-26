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
    public IEnumerator TestCompatibleNewbornReproduction()
    {
      Assert.IsTrue(GameObject.Find("TrainingManager").transform.childCount == 2);
      GameObject spawner0 = GameObject.Find("Spawner0");
      GameObject spawner1 = GameObject.Find("Spawner1");
      GameObject agent0 = spawner0.transform.GetChild(0).gameObject;
      GameObject agent1 = spawner1.transform.GetChild(0).gameObject;
      agent0.transform.GetComponent<Newborn>().Sex = "female";
      agent1.transform.GetComponent<Newborn>().Sex = "male";
      AgentTrainBehaviour at = agent0.transform.GetComponent<AgentTrainBehaviour>();
      at.TouchedNewborn(agent1);
      Assert.IsTrue(spawner0.transform.GetChild(0).transform.GetComponent<Newborn>().isGestating);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestInCompatibleNewbornReproduction()
    {
      Assert.IsTrue(GameObject.Find("TrainingManager").transform.childCount == 2);
      GameObject spawner0 = GameObject.Find("Spawner0");
      GameObject spawner1 = GameObject.Find("Spawner1");
      GameObject agent0 = spawner0.transform.GetChild(0).gameObject;
      GameObject agent1 = spawner1.transform.GetChild(0).gameObject;
      agent0.transform.GetComponent<Newborn>().Sex = "female";
      agent1.transform.GetComponent<Newborn>().Sex = "female";
      AgentTrainBehaviour at = agent0.transform.GetComponent<AgentTrainBehaviour>();
      at.TouchedNewborn(agent1);
      Assert.IsFalse(spawner0.transform.GetChild(0).transform.GetComponent<Newborn>().isGestating);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestMaleReproduction()
    {
      Assert.IsTrue(GameObject.Find("TrainingManager").transform.childCount == 2);
      GameObject spawner0 = GameObject.Find("Spawner0");
      GameObject spawner1 = GameObject.Find("Spawner1");
      GameObject agent0 = spawner0.transform.GetChild(0).gameObject;
      GameObject agent1 = spawner1.transform.GetChild(0).gameObject;
      agent0.transform.GetComponent<Newborn>().Sex = "male";
      agent1.transform.GetComponent<Newborn>().Sex = "female";
      AgentTrainBehaviour at = agent0.transform.GetComponent<AgentTrainBehaviour>();
      at.TouchedNewborn(agent1);
      Assert.IsFalse(spawner0.transform.GetChild(0).transform.GetComponent<Newborn>().isGestating);
      yield return null;
    }
  }
}
