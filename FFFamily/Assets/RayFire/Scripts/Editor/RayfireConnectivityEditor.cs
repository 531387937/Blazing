using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireConnectivity))]
    public class RayfireConnectivityEditor : Editor
    {
        static Color wireColor = new Color (0.58f, 0.77f, 1f);

        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireConnectivity targ, GizmoType gizmoType)
        {
            // Connections
            if (targ.showConnections == true)
            {
                if (Application.isPlaying == true)
                {
                    Gizmos.color = Color.green;
                    if (targ.cluster != null && targ.cluster.shards.Count > 0)
                    {
                        foreach (var shard in targ.cluster.shards)
                        {
                            // Set color
                            Gizmos.color = shard.rigid.activation.unyielding == true
                                ? Color.red
                                : Color.green;

                            // draw sphere
                            if (targ.sphereSize > 0)
                                Gizmos.DrawWireSphere (shard.tm.position, shard.bound.size.magnitude / 13f * targ.sphereSize);

                            // Draw connection
                            foreach (var neibShard in shard.neibShards)
                                if (neibShard.rigid.activation.connect != null)
                                    Gizmos.DrawLine (shard.tm.position, neibShard.tm.position);
                        }
                    }
                }
            }

            // Gizmo preview
            if (targ.showGizmo == true)
            {
                // Gizmo properties
                Gizmos.color = wireColor;

                // Gizmo
                if (targ.source == RayfireConnectivity.ConnTargetType.Gizmo)
                {
                    Gizmos.matrix = targ.transform.localToWorldMatrix;
                    Gizmos.DrawWireCube (Vector3.zero, targ.size);
                }

                // Children
                if (targ.source == RayfireConnectivity.ConnTargetType.Children)
                {
                    if (targ.transform.childCount > 0)
                    {
                        Bounds bound = RFCluster.GetChildrenBound (targ.transform);
                        Gizmos.DrawWireCube (bound.center, bound.size);
                    }
                }
            }
        }

        // Inspector
        public override void OnInspectorGUI()
        {
            // Get target
            // RayfireConnectivity targ = target as RayfireConnectivity;

            // Space
            GUILayout.Space (8);

            // Begin
            GUILayout.BeginHorizontal();

            // if (GUILayout.Button ("Set Connectivity", GUILayout.Height (25)))
            //     if (Application.isPlaying == true)
            //         targ.checkNeed = true;

            // End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Draw script UI
            DrawDefaultInspector();
        }
    }
}