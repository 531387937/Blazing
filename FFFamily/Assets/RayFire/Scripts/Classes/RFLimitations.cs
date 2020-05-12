using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFLimitations
    {
        [Tooltip ("Local Object solidity multiplier for object. Low Solidity makes object more fragile.")]
        [Range (0.0f, 10f)]
        public float solidity;

        [Tooltip ("Defines how deep object can be demolished. Depth is limitless if set to 0.")]
        [Range (0, 7)]
        public int depth;

        [Tooltip ("Safe time. Measures in seconds and allows to prevent fragments from being demolished right after they were just created.")]
        [Range (0.05f, 10f)]
        public float time;

        [Tooltip ("Prevent objects with bounding box size less than defined value to be demolished.")]
        [Range (0.01f, 5f)]
        public float size;
        
        //[Tooltip ("")]
        //[Range (1, 100)]
        // TODO public int probability;
        
        [Tooltip ("Allows object to be sliced by object with RayFire Blade component.")]
        public bool sliceByBlade;

        [Header ("Hidden")]
        [HideInInspector] public List<Vector3> slicePlanes;
        [HideInInspector] public Vector3 contactPoint;
        [HideInInspector] public bool demolitionShould;
        [HideInInspector] public bool demolished;
        [HideInInspector] public float birthTime;
        [HideInInspector] public float bboxSize;
        [HideInInspector] public int currentDepth;
        [HideInInspector] public Bounds bound;
        [HideInInspector] public RayfireRigid ancestor;
        [HideInInspector] public List<RayfireRigid> descendants;
        
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFLimitations()
        {
            solidity         = 0.1f;
            depth            = 1;
            time             = 0.2f;
            size             = 0.1f;
            sliceByBlade     = false;
            
            currentDepth     = 0;
            birthTime        = 0f;
            bboxSize         = 0f;

            ancestor = null;
            descendants = new List<RayfireRigid>();
            
            Reset();
        }

        // Copy from
        public void CopyFrom (RFLimitations limitations)
        {
            solidity         = limitations.solidity;
            depth            = limitations.depth;
            time             = limitations.time;
            size             = limitations.size;
            sliceByBlade     = limitations.sliceByBlade;
            
            // Do not copy currentDepth. Set in other place
            
            Reset();
        }
        
        // Reset
        public void Reset()
        {
            slicePlanes      = new List<Vector3>();
            contactPoint     = Vector3.zero;
            demolitionShould = false;
            demolished       = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
         
        // Cache velocity for fragments 
        public IEnumerator DemolishableCor(RayfireRigid scr)
        {
            while (scr.demolitionType != DemolitionType.None)
            {
                // Max depth reached
                if (scr.limitations.depth > 0 && scr.limitations.currentDepth >= scr.limitations.depth)
                    scr.demolitionType = DemolitionType.None;

                // Init demolition
                if (scr.limitations.demolitionShould == true)
                    scr.Demolish();
                
                // Check for slicing planes and init slicing
                else if (scr.limitations.sliceByBlade == true && scr.limitations.slicePlanes.Count > 1)
                    scr.SliceObjectByPlanes();
                
                yield return null;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check for user mistakes
        public static void Checks (RayfireRigid scr)
        {
   
            // TODO static and cluster
            
            // ////////////////
            // Sim Type
            // ////////////////
            
            // Static and demolishable
            if (scr.simulationType == SimType.Static)
            {
                if (scr.demolitionType != DemolitionType.None)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is not None. Static object can not be demolished. Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }
            }
            
            // ////////////////
            // Object Type
            // ////////////////
            
            // Object can not be simulated as mesh
            if (scr.objectType == ObjectType.Mesh)
            {
                if (scr.meshFilter == null || scr.meshFilter.sharedMesh == null)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no mesh. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as cluster
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
            {
                if (scr.transForm.childCount == 0)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no children. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as mesh
            else if (scr.objectType == ObjectType.SkinnedMesh)
            {
                if (scr.skinnedMeshRend == null)
                    Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no SkinnedMeshRenderer. Object Excluded from simulation.", scr.gameObject);
                
                // Excluded from sim by default
                scr.physics.exclude = true;
            }
            
            // ////////////////
            // Demolition Type
            // ////////////////
            
            // Demolition checks
            if (scr.demolitionType != DemolitionType.None)
            {
                // // Static
                // if (scr.simulationType == SimType.Static)
                // {
                //     Debug.Log ("RayFire Rigid: " + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to None.", scr.gameObject);
                //     scr.demolitionType = DemolitionType.None;
                // }
                
                // Set runtime demolition for clusters and skinned mesh
                if (scr.objectType == ObjectType.SkinnedMesh ||
                    scr.objectType == ObjectType.NestedCluster ||
                    scr.objectType == ObjectType.ConnectedCluster)
                {
                    if (scr.demolitionType != DemolitionType.Runtime && scr.demolitionType != DemolitionType.ReferenceDemolition)
                    {
                        Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to Runtime.", scr.gameObject);
                        scr.demolitionType = DemolitionType.Runtime;
                    }
                }
                
                // No Shatter component for runtime demolition with Use Shatter on
                if (scr.meshDemolition.scrShatter == null && scr.meshDemolition.useShatter == true)
                {
                    if (scr.demolitionType == DemolitionType.Runtime ||
                        scr.demolitionType == DemolitionType.AwakePrecache ||
                        scr.demolitionType == DemolitionType.AwakePrefragment)
                    {
                        
                        Debug.Log ("RayFire Rigid: " + scr.name + "Demolition Type is " + scr.demolitionType.ToString() + ". Has no Shatter component, but Use Shatter property is On. Use Shatter property was turned Off.", scr.gameObject);
                        scr.meshDemolition.useShatter = false;
                    }
                }
            }
            
            // None check
            if (scr.demolitionType == DemolitionType.None)
            {
                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to None. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to None. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to None. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
            }

            // Runtime check
            else if (scr.demolitionType == DemolitionType.Runtime)
            {
                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Runtime. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Runtime. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Runtime. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
                
                // No runtime caching for rigid with shatter with tets/slices/glue
                if (scr.meshDemolition.useShatter == true && scr.meshDemolition.runtimeCaching.type != CachingType.Disable)
                {
                    if (scr.meshDemolition.scrShatter.type == FragType.Decompose ||
                        scr.meshDemolition.scrShatter.type == FragType.Tets ||
                        scr.meshDemolition.scrShatter.type == FragType.Slices || 
                        scr.meshDemolition.scrShatter.clusters.enable == true)
                    {
                        Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type is Runtime, Use Shatter is On. Unsupported fragments type. Runtime Caching supports only Voronoi, Splinters, Slabs and Radial fragmentation types. Runtime Caching was Disabled.", scr.gameObject);
                        scr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                    }
                }
            }

            // Awake precache check
            else if (scr.demolitionType == DemolitionType.AwakePrecache)
            {
                if (scr.HasMeshes == true)
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Precache. Had manually precached Unity meshes which were overwritten.", scr.gameObject);
                
                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Awake Precache. Has manually precached serialized meshes.", scr.gameObject);
                }
            }

            // Awake prefragmented check
            else if (scr.demolitionType == DemolitionType.AwakePrefragment)
            {
                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Prefragment. Has manually prefragmented objects", scr.gameObject);
                }

                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached Unity meshes.", scr.gameObject);
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached serialized meshes.", scr.gameObject);
                }
            }

            // Prefab precache check
            else if (scr.demolitionType == DemolitionType.ManualPrefabPrecache)
            {
                if (scr.HasRfMeshes == false)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Prefab Precache. Has no precached serialized meshes, Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Prefab Precache. Has manually precached meshes.", scr.gameObject);
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Prefab Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }
            }

            // Manual precache check
            else if (scr.demolitionType == DemolitionType.ManualPrecache)
            {
                if (scr.HasMeshes == false)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Precache. Has no manually precached Unity meshes, Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Manual Precache. Has manually precached serialized meshes.", scr.gameObject);
                }
            }

            // Manual prefragmented check
            else if (scr.demolitionType == DemolitionType.ManualPrefragment)
            {
                if (scr.HasFragments == false)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Prefragment. Has no manually prefragmented objects, Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Prefragment. Has manually precached Unity meshes.", scr.gameObject);
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Manual Prefragment. Has manually precached serialized meshes.", scr.gameObject);
                }
            }
            
            // TODO Tag and Layer check
        }
        
        // Set bound and size
        public static void SetBound (RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.Mesh)
                scr.limitations.bound = scr.meshRenderer.bounds;
            else if (scr.objectType == ObjectType.Mesh)
                scr.limitations.bound = scr.skinnedMeshRend.bounds;
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
                scr.limitations.bound = RFCluster.GetChildrenBound (scr.transForm);
            scr.limitations.bboxSize = scr.limitations.bound.size.magnitude;
        }
        
        // Set ancestor
        public static void SetAncestor (RayfireRigid scr)
        {
            // Set ancestor to this if it is ancestor
            if (scr.limitations.ancestor == null)
                for (int i = 0; i < scr.fragments.Count; i++)
                    scr.fragments[i].limitations.ancestor = scr;
            else
                for (int i = 0; i < scr.fragments.Count; i++) 
                    scr.fragments[i].limitations.ancestor = scr.limitations.ancestor;
        }
        
        // Set descendants 
        public static void SetDescendants (RayfireRigid scr)
        {
            if (scr.limitations.ancestor == null)
                scr.limitations.descendants.AddRange (scr.fragments);
            else
                scr.limitations.ancestor.limitations.descendants.AddRange (scr.fragments);
            
        }
        
        // Create root
        public static void CreateRoot (RayfireRigid rfScr)
        {
           GameObject root = new GameObject();
           root.transform.parent = RayfireMan.inst.transForm;
           root.name             = rfScr.gameObject.name + "_root";
           rfScr.rootChild             = root.transform;
           rfScr.rootChild.position    = rfScr.transForm.position;
           rfScr.rootChild.rotation    = rfScr.transForm.rotation;
        }
    }
}