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
        spawner.BuildSpawners();
      }

      if (GUILayout.Button("Delete training environment"))
      {
        spawner.Delete();
      }

      EditorGUILayout.LabelField("Random NewBorn Builds");

      if (GUILayout.Button("Build NewBorn Training Cells"))
      {
        for (int i = 0; i < spawner.Agents.Count; i++)
        {
          spawner.BuildRandomTrainingNewBorn(false, i);
        }
      }

      if (GUILayout.Button("Build NewBorn Production Cells"))
      {
        foreach (GameObject agent in GameObject.FindGameObjectsWithTag("agent"))
        {
          spawner.BuildRandomProductionNewBorn(agent.transform);
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

      if (GUILayout.Button("Request Training NewBorn"))
      {
        spawner.RequestTrainingAgentInfo();
      }

      if (GUILayout.Button("Request Production NewBorn"))
      {
        spawner.RequestProductionAgentInfo();
      }

      if (GUILayout.Button("Request Training NewBorn (target generation)"))
      {
        spawner.RequestTrainingAgentInfo();
      }

      if (GUILayout.Button("Request Production NewBorn (target generation)"))
      {
        spawner.RequestProductionAgentInfo();
      }

      EditorGUILayout.LabelField("API Post request");

      if (GUILayout.Button("Post Training NewBorn"))
      {
        spawner.PostAgents();
      }

      if (GUILayout.Button("Update Training NewBorn"))
      {
        spawner.RequestTrainingAgentInfo();
      }
    }
  }
}