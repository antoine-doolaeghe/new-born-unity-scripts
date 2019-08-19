using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Editor
{
    public class SceneHelper
    {
        #region Public Methods

        public static void Focus(Object target, DrawCameraMode mode = DrawCameraMode.Wireframe, bool autoSelect = true)
        {
            EditorWindow.GetWindow<SceneView>("", typeof(SceneView));

            if (autoSelect)
                Selection.activeObject = target;

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();

#if UNITY_2018_3

#elif UNITY_2018 || UNITY_2019

#else
                SceneView.lastActiveSceneView.Repaint();

#endif
            }
        }

        public static void UnFocus()
        {
            if (SceneView.lastActiveSceneView != null)
            {
#if UNITY_2018_3

#elif UNITY_2018 || UNITY_2019
                try
                {
                    SceneView.lastActiveSceneView.Repaint();
                }catch
                { }
#else
                SceneView.lastActiveSceneView.Repaint();
#endif
            }
        }

        #endregion Public Methods
    }
}