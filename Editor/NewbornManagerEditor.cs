
using UnityEditor;
using UnityEngine;
using Components.Manager;
using Components.Newborn.Anatomy;
using Components.Spawner.Newborn;
namespace Newborn
{
  [CustomEditor(typeof(Manager))]
  public class ManagerBuildEditor : Editor
  {

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      Manager manager = (Manager)target;

      EditorGUILayout.LabelField("Environmnet Parameters");

      if (GUILayout.Button("Build environment"))
      {
        manager.BuildTrainerEnvironment();
      }
      GUI.backgroundColor = Color.red;
      if (GUILayout.Button("Delete environment"))
      {
        manager.DeleteEnvironment();
      }
      GUI.backgroundColor = Color.white;

      EditorGUILayout.LabelField("Random NewBorn Builds");

      if (GUILayout.Button("Build NewBorn Production Cells"))
      {
        foreach (AnatomyBuilder spawner in FindObjectsOfType<AnatomyBuilder>())
        {
          spawner.BuildAgentRandomNewBornCoroutine();
        }
      }
      GUI.backgroundColor = Color.red;
      if (GUILayout.Button("Delete NewBorn Cells"))
      {
        foreach (NewbornSpawner spawner in FindObjectsOfType<NewbornSpawner>())
        {
          spawner.ClearAgents();
        }
      }
      GUI.backgroundColor = Color.white;

      if (GUILayout.Button("Add Agent Generation"))
      {
        // TODO
        // manager.ClearBroadCastingBrains();
        // foreach (GameObject spawner in manager.Spawners)
        // {
        //   spawner.GetComponent<NewbornSpawner>().BuildAllAgentsRandomGeneration();
        // }
      }

      EditorGUILayout.LabelField("Serviced Newborn Builds");

      if (manager.isTrainingMode)
      {
        if (GUILayout.Button("Request Training NewBorn"))
        {
          manager.RequestNewborn();
        }
      }

      if (GUILayout.Button("Request Training NewBorn (target generation)"))
      {
        manager.RequestNewborn();
      }


      EditorGUILayout.LabelField("API Post request");
      GUI.backgroundColor = Color.green;
      if (GUILayout.Button("Post Training NewBorn"))
      {
        // manager.ClearBroadCastingBrains();
        foreach (NewbornSpawner spawner in FindObjectsOfType<NewbornSpawner>())
        {
          spawner.GetComponent<NewbornSpawner>().PostTrainingNewborns();
        }
      }
      EditorGUILayout.LabelField("Reset");
      GUI.backgroundColor = Color.yellow;
      if (GUILayout.Button("Reset trainer data"))
      {
        manager.ResetTrainerData();
      }
    }
  }
}