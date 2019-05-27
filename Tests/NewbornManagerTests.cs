using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Tests
{
  public class NewbornManagerTests
  {
    [UnitySetUp]
    public IEnumerator TestSceneSetup()
    {
      Scene testScene = SceneManager.GetActiveScene();
      yield return SceneManager.LoadSceneAsync("MockEmptyTestScene", LoadSceneMode.Single);
      SceneManager.SetActiveScene(SceneManager.GetSceneByName("MockEmptyTestScene"));
    }

    [UnityTest]
    public IEnumerator TestDeleteScene()
    {
      Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 2);
      Gene.NewbornManager tm = GameObject.Find("NewbornManager").GetComponent<Gene.NewbornManager>();
      tm.DeleteSpawner();
      Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 0);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestBuildAgentScene()
    {
      Gene.NewbornManager tm = GameObject.Find("NewbornManager").GetComponent<Gene.NewbornManager>();
      tm.DeleteSpawner();
      Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 0);
      tm.spawnerNumber = 2;
      tm.BuildSpawners();
      Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 2);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestBuildAgentCellScene()
    {
      Gene.NewbornManager tm = GameObject.Find("NewbornManager").GetComponent<Gene.NewbornManager>();
      tm.DeleteSpawner();
      tm.spawnerNumber = 1;
      tm.agentNumber = 1;
      tm.BuildSpawners();
      yield return tm.BuildRandomProductionNewBorn(GameObject.Find("Spawner0").transform.GetChild(0).transform);
      Assert.IsTrue(GameObject.Find("NewbornManager").transform.GetChild(0).transform.GetChild(0).transform.childCount > 0);
      yield return null;
    }

    // [UnityTest]
    // public IEnumerator TestDeleteAgentCellScene()
    // {
    //   Gene.NewbornManager tm = GameObject.Find("NewbornManager").GetComponent<Gene.NewbornManager>();
    //   tm.Delete();
    //   tm.spawnerNumber = 1;
    //   tm.agentNumber = 1;
    //   tm.BuildSpawners();
    //   yield return tm.BuildRandomProductionNewBorn(GameObject.Find("Spawner0").transform.GetChild(0).transform);
    //   Assert.IsTrue(GameObject.Find("NewbornManager").transform.GetChild(0).transform.GetChild(0).transform.childCount == 0);
    //   yield return null;
    // }
  }
}
