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
      NewbornManager spawner = (NewbornManager)target;

      EditorGUILayout.LabelField("Environmnet Parameters");

      if (GUILayout.Button("Build environment"))
      {
        spawner.BuildSpawners();
      }

      if (GUILayout.Button("Delete environment"))
      {
        spawner.DeleteSpawner();
      }

      EditorGUILayout.LabelField("Random NewBorn Builds");

      if (spawner.isTrainingMode)
      {
        if (GUILayout.Button("Build NewBorn Training Cells"))
        {
          for (int i = 0; i < spawner.Agents.Count; i++)
          {
            spawner.BuildRandomTrainingNewBornCoroutine(false, i);
          }
        }
      }

      if (!spawner.isTrainingMode)
      {
        if (GUILayout.Button("Build NewBorn Production Cells"))
        {
          foreach (GameObject agent in GameObject.FindGameObjectsWithTag("agent"))
          {
            spawner.BuildRandomProductionNewBornCoroutine(agent.transform);
          }
        }
      }

      if (GUILayout.Button("Delete NewBorn Cells"))
      {
        spawner.DeleteCell();
      }

      if (GUILayout.Button("Add Agent Generation"))
      {
        spawner.BuildRandomGeneration();
      }

      EditorGUILayout.LabelField("Serviced Newborn Builds");

      if (spawner.isTrainingMode)
      {
        if (GUILayout.Button("Request Training NewBorn"))
        {
          spawner.RequestNewborn();
        }
      }

      if (!spawner.isTrainingMode)
      {
        if (GUILayout.Button("Request Production NewBorn"))
        {
          spawner.RequestProductionAgentInfo();
        }
      }

      if (spawner.isTrainingMode)
      {
        if (GUILayout.Button("Request Training NewBorn (target generation)"))
        {
          spawner.RequestNewborn();
        }
      }

      // TO-DO 
      // if (!spawner.isTrainingMode)
      // {
      //   if (GUILayout.Button("Request Production NewBorn (target generation)"))
      //   {
      //     spawner.RequestProductionAgentInfo();
      //   }
      // }

      EditorGUILayout.LabelField("API Post request");

      if (GUILayout.Button("Post Training NewBorn"))
      {
        spawner.PostTrainingNewborns();
      }


      // TO-DO
      // if (GUILayout.Button("Update Training NewBorn"))
      // {
      //   // this will need to be the possibility to load a specific model number
      // }
    }
  }
}