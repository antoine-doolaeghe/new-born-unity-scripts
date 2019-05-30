using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Newborn
{
  [CustomEditor(typeof(NewBornBuilder))]
  public class NewBornBuilderEditor : Editor
  {
    public int agentNumber;
    public GameObject agent;
    public Vector3 agentScale;

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      NewBornBuilder newBornBuilder = (NewBornBuilder)target;

      // if (GUILayout.Button ("Post Single Gene")) {
      //     newBornBuilder.PostCell("");
      // }
    }
  }
}