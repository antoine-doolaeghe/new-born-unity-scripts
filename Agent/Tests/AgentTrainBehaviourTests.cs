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
    public IEnumerator TestNewbornIsReproducingWhenTouchingAnotherNewborn()
    {
      GameObject agentSphereA = GameObject.Find("SphereA");
      GameObject agentSphereB = GameObject.Find("SphereB");
      CollisionController tcA = agentSphereA.GetComponent<CollisionController>();
      CollisionController tcB = agentSphereB.GetComponent<CollisionController>();

      yield return agentSphereA.transform.position = new Vector3(0f, 0f, 0f);
      yield return agentSphereB.transform.position = new Vector3(0f, 0f, 0f);

      // Assert here that the newborn has made a api call

      yield return agentSphereA.transform.position = new Vector3(1f, 1f, 1f);
      yield return agentSphereB.transform.position = new Vector3(-1f, -1f, -1f);
    }
  }
}
