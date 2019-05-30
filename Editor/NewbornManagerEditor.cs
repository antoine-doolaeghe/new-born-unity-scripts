using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gene
{
  [CustomEditor(typeof(NewbornManager))]
  public class NewbornManagerBuildEditor : Editor
  {

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      NewbornManager manager = (NewbornManager)target;

      EditorGUILayout.LabelField("Environmnet Parameters");

      if (GUILayout.Button("Build environment"))
      {
        manager.BuildSpawners();
      }

      if (GUILayout.Button("Delete environment"))
      {
        manager.DeleteSpawner();
      }

      EditorGUILayout.LabelField("Random NewBorn Builds");

      if (GUILayout.Button("Build NewBorn Production Cells"))
      {
        foreach (GameObject spawner in manager.Spawners)
        {
          spawner.GetComponent<NewbornSpawner>().BuildAllAgentsRandomNewBorn();
        }
      }

      if (GUILayout.Button("Delete NewBorn Cells"))
      {
        foreach (GameObject spawner in manager.Spawners)
        {
          spawner.GetComponent<NewbornSpawner>().DeleteCell();
        }
      }

      if (GUILayout.Button("Add Agent Generation"))
      {
        manager.ClearBroadCastingBrains();
        foreach (GameObject spawner in manager.Spawners)
        {
          spawner.GetComponent<NewbornSpawner>().BuildAllAgentsRandomGeneration();
        }
      }

      EditorGUILayout.LabelField("Serviced Newborn Builds");

      if (manager.isTrainingMode)
      {
        if (GUILayout.Button("Request Training NewBorn"))
        {
          manager.RequestNewborn();
        }
      }

      if (!manager.isTrainingMode)
      {
        if (GUILayout.Button("Request Production NewBorn"))
        {
          manager.RequestProductionAgentInfo();
        }
      }

      if (manager.isTrainingMode)
      {
        if (GUILayout.Button("Request Training NewBorn (target generation)"))
        {
          manager.RequestNewborn();
        }
      }

      EditorGUILayout.LabelField("API Post request");

      if (GUILayout.Button("Post Training NewBorn"))
      {
        manager.PostTrainingNewborns();
      }
    }
  }
}