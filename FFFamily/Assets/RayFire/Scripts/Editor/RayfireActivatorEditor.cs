using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireActivator))]
    public class RayfireActivatorEditor : Editor
    {
        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireActivator activator, GizmoType gizmoType)
        {
            // Gizmo preview
            if (activator.showGizmo == true)
            {

                Color wireColor = new Color (0.58f, 0.77f, 1f);

                // Gizmo properties
                Gizmos.color  = wireColor;
                Gizmos.matrix = activator.transform.localToWorldMatrix;

                // Box gizmo
                if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                {
                    // Offsets
                    float x = activator.boxSize.x / 2f;
                    float y = activator.boxSize.y / 2f;
                    float z = activator.boxSize.z / 2f;

                    // Get points
                    Vector3 p1 = new Vector3 (-x, -y, -z);
                    Vector3 p2 = new Vector3 (-x, -y, +z);
                    Vector3 p3 = new Vector3 (+x, -y, -z);
                    Vector3 p4 = new Vector3 (+x, -y, +z);
                    Vector3 p5 = new Vector3 (-x, y,  -z);
                    Vector3 p6 = new Vector3 (-x, y,  +z);
                    Vector3 p7 = new Vector3 (+x, y,  -z);
                    Vector3 p8 = new Vector3 (+x, y,  +z);

                    // Gizmo Lines
                    Gizmos.DrawLine (p1, p2);
                    Gizmos.DrawLine (p3, p4);
                    Gizmos.DrawLine (p5, p6);
                    Gizmos.DrawLine (p7, p8);
                    Gizmos.DrawLine (p1, p5);
                    Gizmos.DrawLine (p2, p6);
                    Gizmos.DrawLine (p3, p7);
                    Gizmos.DrawLine (p4, p8);
                    Gizmos.DrawLine (p1, p3);
                    Gizmos.DrawLine (p2, p4);
                    Gizmos.DrawLine (p5, p7);
                    Gizmos.DrawLine (p6, p8);

                    // Selectable sphere
                    float sphereSize = (x + y + z) * 0.03f;
                    if (sphereSize < 0.1f)
                        sphereSize = 0.1f;
                    Gizmos.color = new Color (1.0f, 0.60f, 0f);
                    Gizmos.DrawSphere (new Vector3 (x,  0, 0f), sphereSize);
                    Gizmos.DrawSphere (new Vector3 (-x, 0, 0f), sphereSize);
                    Gizmos.DrawSphere (new Vector3 (0f, 0, z),  sphereSize);
                    Gizmos.DrawSphere (new Vector3 (0f, 0, -z), sphereSize);
                }

                // Sphere gizmo
                if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                {
                    // Vars
                    int   size   = 45;
                    float rate   = 0f;
                    float scale  = 1f / size;
                    float radius = activator.sphereRadius;

                    Vector3 previousPoint = Vector3.zero;
                    Vector3 nextPoint     = Vector3.zero;

                    // Draw top eye
                    rate            = 0f;
                    nextPoint.y     = 0f;
                    previousPoint.y = 0f;
                    previousPoint.x = radius * Mathf.Cos (rate);
                    previousPoint.z = radius * Mathf.Sin (rate);
                    for (int i = 0; i < size; i++)
                    {
                        rate        += 2.0f * Mathf.PI * scale;
                        nextPoint.x =  radius * Mathf.Cos (rate);
                        nextPoint.z =  radius * Mathf.Sin (rate);
                        Gizmos.DrawLine (previousPoint, nextPoint);
                        previousPoint = nextPoint;
                    }

                    // Draw top eye
                    rate            = 0f;
                    nextPoint.x     = 0f;
                    previousPoint.x = 0f;
                    previousPoint.y = radius * Mathf.Cos (rate);
                    previousPoint.z = radius * Mathf.Sin (rate);
                    for (int i = 0; i < size; i++)
                    {
                        rate        += 2.0f * Mathf.PI * scale;
                        nextPoint.y =  radius * Mathf.Cos (rate);
                        nextPoint.z =  radius * Mathf.Sin (rate);
                        Gizmos.DrawLine (previousPoint, nextPoint);
                        previousPoint = nextPoint;
                    }

                    // Draw top eye
                    rate            = 0f;
                    nextPoint.z     = 0f;
                    previousPoint.z = 0f;
                    previousPoint.y = radius * Mathf.Cos (rate);
                    previousPoint.x = radius * Mathf.Sin (rate);
                    for (int i = 0; i < size; i++)
                    {
                        rate        += 2.0f * Mathf.PI * scale;
                        nextPoint.y =  radius * Mathf.Cos (rate);
                        nextPoint.x =  radius * Mathf.Sin (rate);
                        Gizmos.DrawLine (previousPoint, nextPoint);
                        previousPoint = nextPoint;
                    }

                    // Selectable sphere
                    float sphereSize = radius * 0.07f;
                    if (sphereSize < 0.1f)
                        sphereSize = 0.1f;
                    Gizmos.color = new Color (1.0f, 0.60f, 0f);
                    Gizmos.DrawSphere (new Vector3 (0f,      radius,  0f),      sphereSize);
                    Gizmos.DrawSphere (new Vector3 (0f,      -radius, 0f),      sphereSize);
                    Gizmos.DrawSphere (new Vector3 (radius,  0f,      0f),      sphereSize);
                    Gizmos.DrawSphere (new Vector3 (-radius, 0f,      0f),      sphereSize);
                    Gizmos.DrawSphere (new Vector3 (0f,      0f,      radius),  sphereSize);
                    Gizmos.DrawSphere (new Vector3 (0f,      0f,      -radius), sphereSize);
                }
            }
        }

        // Sphere gizmo radius
        private void OnSceneGUI()
        {
            RayfireActivator activator = target as RayfireActivator;
            if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
            {
                var transform = activator.transform;

                // Draw handles
                EditorGUI.BeginChangeCheck();
                activator.sphereRadius = Handles.RadiusHandle (transform.rotation, transform.position, activator.sphereRadius, true);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (activator, "Change Radius");
                }
            }
        }

        // Inspector
        public override void OnInspectorGUI()
        {
            // Get target
            RayfireActivator activator = target as RayfireActivator;

            // Space
            GUILayout.Space (8);

            // Begin
            GUILayout.BeginHorizontal();

            // Cache buttons
            if (GUILayout.Button ("   Start   ", GUILayout.Height (25)))
                if (Application.isPlaying == true)
                    activator.TriggerAnimation();
            if (GUILayout.Button ("    Stop    ", GUILayout.Height (25)))
                if (Application.isPlaying == true)
                    activator.StopAnimation();
            if (GUILayout.Button ("Reset", GUILayout.Height (25)))
                if (Application.isPlaying == true)
                    activator.ResetAnimation();

            // End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (1);

            // Begin
            GUILayout.BeginHorizontal();

            // Cache buttons
            if (GUILayout.Button ("Add Position"))
                activator.AddPosition (activator.transform.position);
            if (GUILayout.Button ("Remove Last"))
                if (activator.positionList.Count > 0)
                    activator.positionList.RemoveAt (activator.positionList.Count - 1);
            if (GUILayout.Button ("Clear All"))
                activator.positionList.Clear();

            // End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Positions info
            if (activator.positionList != null && activator.positionList.Count > 0)
            {
                GUILayout.Label ("Positions : " + activator.positionList.Count);

                // Space
                GUILayout.Space (2);
            }

            // Draw script UI
            DrawDefaultInspector();
        }
    }
}