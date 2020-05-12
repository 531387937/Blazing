using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireBlade))]
    public class RayfireBladeEditor : Editor
    {
        // Inspector editing
        public override void OnInspectorGUI()
        {
            // Get target
            RayfireBlade blade = target as RayfireBlade;

            // Space
            GUILayout.Space (3);

            // Draw script UI
            DrawDefaultInspector();

            // Space
            GUILayout.Space (3);

            // Label
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);

            // Tag filter
            blade.tagFilter = EditorGUILayout.TagField ("Tag", blade.tagFilter);

            // Layer mask
            List<string> layerNames = new List<string>();
            for (int i = 0; i <= 31; i++)
                layerNames.Add (i + ". " + LayerMask.LayerToName (i));
            blade.mask = EditorGUILayout.MaskField ("Layer", blade.mask, layerNames.ToArray());
        }
    }
}