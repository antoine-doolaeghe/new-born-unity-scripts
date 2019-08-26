using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Components.Newborn.Anatomy;

namespace Newborn
{
  [CustomEditor(typeof(AnatomyBuilder))]
  public class AnatomyBuilderEditor : Editor
  {
    public int agentNumber;
    public GameObject agent;
    public Vector3 agentScale;

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      AnatomyBuilder AnatomyBuilder = (AnatomyBuilder)target;

      // if (GUILayout.Button ("Post Single Gene")) {
      //     AnatomyBuilder.PostCell("");
      // }
    }
  }
}