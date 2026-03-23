using UnityEditor;
using UnityEngine;

namespace MountainBackgroundsPixelArt
{
    [CustomEditor(typeof(ParallaxEffect))]
    public class ParallaxEffectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ParallaxEffect parallaxEffect = (ParallaxEffect)target;

            EditorGUILayout.HelpBox("To ensure proper positioning of the layers, it is not recommanded to change the default values of this script.", MessageType.Info);

            parallaxEffect.parallaxIntensityX =
                EditorGUILayout.Slider(new GUIContent("Parallax Intensity X", "The higher the value, the greater the parallax effect on the horizontal axis."), parallaxEffect.parallaxIntensityX, 0.0f, 1.0f);
            parallaxEffect.parallaxIntensityY =
                EditorGUILayout.Slider(new GUIContent("Parallax Intensity Y", "The higher the value, the greater the parallax effect on the vertical axis."), parallaxEffect.parallaxIntensityY, 0.0f, 1.0f);
            parallaxEffect.independantSpeed =
                EditorGUILayout.Slider(new GUIContent("Independant Speed", "The layer will move independently to the left if the value is less than 0, and to the right if the value is greater than 0."), parallaxEffect.independantSpeed, -1.0f, 1.0f);

            if (GUI.changed)
                EditorUtility.SetDirty(parallaxEffect);
        }
    }
}