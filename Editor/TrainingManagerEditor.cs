using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gene
{
  [CustomEditor(typeof(TrainingManager))]
  public class TrainingManagerBuildEditor : Editor
  {
    public int agentNumber;
    public GameObject agent;
    public Vector3 agentScale;

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      TrainingManager spawner = (TrainingManager)target;

      EditorGUILayout.LabelField("Environmnet Parameters");

      if (GUILayout.Button("Build training environment"))
      {
        spawner.BuildAgents();
      }

      if (GUILayout.Button("Delete training environment"))
      {
        spawner.Delete();
      }

      EditorGUILayout.LabelField("Random NewBorn Builds");

      if (GUILayout.Button("Build NewBorn Cells"))
      {
        for (int i = 0; i < spawner.Agents.Count; i++)
        {
          spawner.BuildRandomNewBorn(false, i);
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

      if (GUILayout.Button("Request NewBorn (all generation)"))
      {
        spawner.RequestAgentInfo();
      }

      if (GUILayout.Button("Request NewBorn (target generation)"))
      {
        spawner.RequestAgentInfo();
      }

      EditorGUILayout.LabelField("API Post request");

      if (GUILayout.Button("Post NewBorn"))
      {
        spawner.PostAgents();
      }

      if (GUILayout.Button("Update NewBorn"))
      {
        spawner.RequestAgentInfo();
      }
    }
  }
}