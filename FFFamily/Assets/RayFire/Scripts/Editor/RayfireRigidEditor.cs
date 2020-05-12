using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireRigid))]
    public class RayfireRigidEditor : Editor
    {
        // Target
        RayfireRigid rigid = null;

        public override void OnInspectorGUI()
        {
            // Get target
            rigid = target as RayfireRigid;
            if (rigid == null)
                return;
            
            // Space
            GUILayout.Space (8);
            
            // Initialize
            if (Application.isPlaying == true)
            {
                if (rigid.initialized == false)
                {
                    if (GUILayout.Button ("Initialize", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireRigid != null)
                                if ((targ as RayfireRigid).initialized == false)
                                    (targ as RayfireRigid).Initialize();
                }
                
                // Reuse
                else
                {
                    if (GUILayout.Button ("Reset Rigid", GUILayout.Height (25)))
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                    if ((targ as RayfireRigid).initialized == true)
                                        (targ as RayfireRigid).ResetRigid();
                }
            }

            GUILayout.Space (2); 
                
            // Begin
            GUILayout.BeginHorizontal();
            
            // Mesh object type
            if (rigid.objectType == ObjectType.Mesh)
            {
                if (Application.isPlaying == false)
                {
                    // Precache  buttons
                    if (rigid.demolitionType == DemolitionType.ManualPrecache)
                    {
                        // Precache
                        if (GUILayout.Button (" Manual Precache ", GUILayout.Height (25)))
                        {
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                {
                                    (targ as RayfireRigid).limitations.contactPoint = (targ as RayfireRigid).transform.TransformPoint (Vector3.zero);
                                    (targ as RayfireRigid).ManualPrecache();
                                }
                        }

                        if (GUILayout.Button ("    Clear    ", GUILayout.Height (25)))
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                    (targ as RayfireRigid).DeleteCache();
                    }

                    // Prefragment buttons
                    else if (rigid.demolitionType == DemolitionType.ManualPrefragment)
                    {
                        // Prefragment
                        if (GUILayout.Button ("Prefragment ", GUILayout.Height (25)))
                        {
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                {
                                    (targ as RayfireRigid).limitations.contactPoint = (targ as RayfireRigid).transform.TransformPoint (Vector3.zero);
                                    (targ as RayfireRigid).ManualPrefragment();
                                }
                        }

                        if (GUILayout.Button (" Delete", GUILayout.Height (25)))
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                {
                                    (targ as RayfireRigid).DeleteCache();
                                    (targ as RayfireRigid).DeleteFragments();
                                }
                    }

                    // Cache buttons
                    else if (rigid.demolitionType == DemolitionType.ManualPrefabPrecache)
                    {
                        // Precache
                        if (GUILayout.Button (" Prefab Precache", GUILayout.Height (25)))
                        {
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                {
                                    (targ as RayfireRigid).limitations.contactPoint = (targ as RayfireRigid).transform.TransformPoint (Vector3.zero);
                                    (targ as RayfireRigid).PrefabPrecache();
                                }
                        }

                        if (GUILayout.Button ("    Clear    ", GUILayout.Height (25)))
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                    (targ as RayfireRigid).DeleteCache();
                    }

                    // // Cluster colliders
                    // if (rigid.objectType == ObjectType.NestedCluster || rigid.objectType == ObjectType.ConnectedCluster)
                    // {
                    //     if (GUILayout.Button ("Create colliders", GUILayout.Height (25)))
                    //         rigid.GenerateColliders();
                    //
                    //     if (GUILayout.Button ("    Clear    ", GUILayout.Height (25)))
                    //         rigid.DeleteColliders();
                    // }
                }
            }

            // End
            EditorGUILayout.EndHorizontal();
            
            // Manual Prefab precache
            if (rigid.demolitionType == DemolitionType.ManualPrefabPrecache)
            {
                if (rigid.HasRfMeshes == false)
                    GUILayout.Label ("WARNING: No Rf Meshes Precached yet");
                if (rigid.HasFragments == true)
                    GUILayout.Label ("WARNING: Has fragments");
                
                // Compress mesh data
                rigid.meshDemolition.compressPrefab = GUILayout.Toggle (rigid.meshDemolition.compressPrefab, "Compress Mesh data");
            }

            // Manual Precache warning
            if (rigid.demolitionType == DemolitionType.ManualPrecache)
            {
                if (rigid.HasMeshes == false)
                    GUILayout.Label ("WARNING: No Meshes Precached yet");
                if (rigid.HasFragments == true)
                    GUILayout.Label ("WARNING: Has fragments");
            }

            // Manual Prefragment warning
            else if (rigid.demolitionType == DemolitionType.ManualPrefragment)
            {
                if (rigid.HasFragments == false)
                    GUILayout.Label ("WARNING: No Fragments yet");
            }

            // Demolition actions
            if (Application.isPlaying == true)
            {
                // Begin
                GUILayout.BeginHorizontal();
                
                // Demolish
                if (GUILayout.Button ("Demolish", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireRigid != null)
                            if ((targ as RayfireRigid).simulationType != SimType.Static)
                                (targ as RayfireRigid).Demolish();
                }

                // Activate
                if (GUILayout.Button ("Activate", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireRigid != null)
                            if ((targ as RayfireRigid).simulationType == SimType.Inactive || (targ as RayfireRigid).simulationType == SimType.Kinematic)
                                (targ as RayfireRigid).Activate();
                }
                
                // Fade
                if (GUILayout.Button ("Fade", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireRigid != null)
                            if ((targ as RayfireRigid).fading.fadeType != FadeType.None)
                                (targ as RayfireRigid).Fade();
                }

                // End
                EditorGUILayout.EndHorizontal();
            }
            
            // Cache info
            if (rigid.HasMeshes == true)
                GUILayout.Label ("Precached Unity Meshes: " + rigid.meshes.Length);
            if (rigid.HasFragments == true)
                GUILayout.Label ("Fragments: " + rigid.fragments.Count);
            if (rigid.HasRfMeshes == true)
                GUILayout.Label ("Precached Serialized Meshes: " + rigid.rfMeshes.Length);

            // Demolition info
            if (Application.isPlaying == true && rigid.enabled == true && rigid.initialized == true)
            {
                // Space
                GUILayout.Space (3);

                // Info
                GUILayout.Label ("Info", EditorStyles.boldLabel);

                // Excluded
                if (rigid.physics.exclude == true)
                    GUILayout.Label ("WARNING: Object excluded from simulation.");

                // Size
                GUILayout.Label ("    Size: " + rigid.limitations.bboxSize.ToString());

                // Demolition
                GUILayout.Label ("    Demolition depth: " + rigid.limitations.currentDepth.ToString() + "/" + rigid.limitations.depth.ToString());

                // Damage
                if (rigid.damage.enable == true)
                    GUILayout.Label ("    Damage applied: " + rigid.damage.currentDamage.ToString() + "/" + rigid.damage.maxDamage.ToString());

                // Restriction
                if (rigid.restriction.broke == true)
                    GUILayout.Label ("    Object broke restriction...");
                
                // Fading
                if (rigid.fading.state == 1)
                    GUILayout.Label ("    Object about to fade...");
                
                // Fading
                if (rigid.fading.state == 2)
                    GUILayout.Label ("    Fading in progress...");

                // Bad mesh
                if (rigid.meshDemolition.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                    GUILayout.Label ("    Object has bad mesh and will not be demolished anymore");
                
            }

            // Space
            GUILayout.Space (5);

            // Draw script UI
            DrawDefaultInspector();
        }
    }
}