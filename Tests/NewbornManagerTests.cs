// using System.Collections;
// using System.Collections.Generic;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using UnityEngine.SceneManagement;

// namespace Tests
// {
//   public class ManagerTests
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
//       Assert.IsTrue(GameObject.Find("Manager").transform.childCount == 2);
//       Newborn.Manager tm = GameObject.Find("Manager").GetComponent<Newborn.Manager>();
//       tm.DeleteEnvironment();
//       Assert.IsTrue(GameObject.Find("Manager").transform.childCount == 0);
//       yield return null;
//     }

//     [UnityTest]
//     public IEnumerator TestBuildAgentScene()
//     {
//       Newborn.Manager tm = GameObject.Find("Manager").GetComponent<Newborn.Manager>();
//       tm.DeleteEnvironment();
//       Assert.IsTrue(GameObject.Find("Manager").transform.childCount == 0);
//       tm.spawnerNumber = 2;
//       tm.BuildSpawners();
//       Assert.IsTrue(GameObject.Find("Manager").transform.childCount == 2);
//       yield return null;
//     }
//   }
// }
