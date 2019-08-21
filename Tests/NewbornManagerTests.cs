// using System.Collections;
// using System.Collections.Generic;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using UnityEngine.SceneManagement;

// namespace Tests
// {
//   public class NewbornManagerTests
//   {
//     [UnitySetUp]
//     public IEnumerator TestSceneSetup()
//     {
//       Scene testScene = SceneManager.GetActiveScene();
//       yield return SceneManager.LoadSceneAsync("MockEmptyTestScene", LoadSceneMode.Single);
//       SceneManager.SetActiveScene(SceneManager.GetSceneByName("MockEmptyTestScene"));
//     }

//     [UnityTest]
//     public IEnumerator TestDeleteScene()
//     {
//       Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 2);
//       Newborn.NewbornManager tm = GameObject.Find("NewbornManager").GetComponent<Newborn.NewbornManager>();
//       tm.DeleteSpawner();
//       Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 0);
//       yield return null;
//     }

//     [UnityTest]
//     public IEnumerator TestBuildAgentScene()
//     {
//       Newborn.NewbornManager tm = GameObject.Find("NewbornManager").GetComponent<Newborn.NewbornManager>();
//       tm.DeleteSpawner();
//       Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 0);
//       tm.spawnerNumber = 2;
//       tm.BuildSpawners();
//       Assert.IsTrue(GameObject.Find("NewbornManager").transform.childCount == 2);
//       yield return null;
//     }
//   }
// }
