using UnityEditor;
using UnityEngine;

namespace MountainBackgroundsPixelArt
{
    [CustomEditor(typeof(MainCamera))]
    public class MainCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MainCamera mainCamera = (MainCamera)target;

            EditorGUILayout.HelpBox("In some scenes, if Vertical Axis and Camera Size are locked by default, it is not recommended to unlock them.", MessageType.Info);

            mainCamera.smoothCamera = EditorGUILayout.Toggle("Smooth Camera", mainCamera.smoothCamera);
            mainCamera.lockVerticalAxis = EditorGUILayout.Toggle("Lock Vertical Axis", mainCamera.lockVerticalAxis);
            mainCamera.lockCameraSize = EditorGUILayout.Toggle("Lock Camera Size", mainCamera.lockCameraSize);

            if (!mainCamera.lockCameraSize)
                mainCamera.cameraSize = EditorGUILayout.Slider("Camera Size", mainCamera.cameraSize, 5.0f, 9.0f);

            if (GUI.changed)
                EditorUtility.SetDirty(mainCamera);
        }
    }
}
