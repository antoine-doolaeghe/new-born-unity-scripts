using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MyBox;

namespace Gene
{
  [CustomEditor(typeof(TrainingManager))]
  public class TrainingManagerBuildEditor : Editor
  {

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      TrainingManager spawner = (TrainingManager)target;

      EditorGUILayout.LabelField("Environmnet Parameters");

      if (GUILayout.Button("Build environment"))
      {
        spawner.BuildSpawners();
      }

      if (GUILayout.Button("Delete environment"))
      {
        spawner.Delete();
      }

      EditorGUILayout.LabelField("Random NewBorn Builds");

      if (spawner.isTrainingMode)
      {
        if (GUILayout.Button("Build NewBorn Training Cells"))
        {
          for (int i = 0; i < spawner.Agents.Count; i++)
          {
            spawner.BuildRandomTrainingNewBorn(false, i);
          }
        }
      }

      if (!spawner.isTrainingMode)
      {
        if (GUILayout.Button("Build NewBorn Production Cells"))
        {
          foreach (GameObject agent in GameObject.FindGameObjectsWithTag("agent"))
          {
            spawner.BuildRandomProductionNewBorn(agent.transform);
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
          spawner.RequestTrainingAgentInfo();
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
          spawner.RequestTrainingAgentInfo();
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
        spawner.PostAgents();
      }


      // TO-DO
      // if (GUILayout.Button("Update Training NewBorn"))
      // {
      //   // this will need to be the possibility to load a specific model number
      // }
    }
  }
}