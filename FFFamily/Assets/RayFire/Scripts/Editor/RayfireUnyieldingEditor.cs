using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireUnyielding))]
    public class RayfireUnyieldingEditor : Editor
    {
        static Color wireColor = new Color (0.58f, 0.77f, 1f);

        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireUnyielding targ, GizmoType gizmoType)
        {
            // Gizmo preview
            if (targ.showGizmo == true)
            {
                // Gizmo properties
                Gizmos.color  = wireColor;
                Gizmos.matrix = targ.transform.localToWorldMatrix;

                // Cube
                Gizmos.DrawWireCube (Vector3.zero, targ.size);
            }
        }

        // Inspector
        public override void OnInspectorGUI()
        {
            // Get target
            // RayfireUnyielding targ = target as RayfireUnyielding;

            // Space
            GUILayout.Space (8);

            // Begin
            GUILayout.BeginHorizontal();



            // End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Draw script UI
            DrawDefaultInspector();
        }
    }
}