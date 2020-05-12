using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireCombine))]
    public class RayfireCombineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Get shatter
            RayfireCombine combine = target as RayfireCombine;

            // Draw script UI
            DrawDefaultInspector();

            // Space
            GUILayout.Space (5);

            // Combine 
            if (GUILayout.Button ("Combine", GUILayout.Height (25)))
                combine.Combine();

            // Space
            GUILayout.Space (5);
        }
    }
}