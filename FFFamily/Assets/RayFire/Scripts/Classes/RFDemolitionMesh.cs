using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if (UNITY_EDITOR || UNITY_STANDALONE)
using RayFire.DotNet;
#endif

namespace RayFire
{
    // [Serializable]
    // public class RFMeshInput
    // {
    //     public bool init;
    //     public bool pre;
    //     public bool post;
    // }
    
    [Serializable]
    public class RFDemolitionMesh
    {
        [Header ("  Fragments")]
        [Space (2)]
        
        [Tooltip ("Defines amount of new fragments after demolition.")]
        [Range (3, 300)]
        public int amount;

        [Tooltip ("Defines additional amount variation for object in percents.")]
        [Range (0, 100)]
        public int variation;

        [Tooltip ("Amount multiplier for next Depth level. Allows to decrease fragments amount of every next demolition level.")]
        [Range (0.01f, 1f)]
        public float depthFade;

        [Space (3)]
        [Tooltip ("Higher value allows to create more tiny fragments closer to collision contact point and bigger fragments far from it.")]
        [Range (0f, 1f)]
        public float contactBias;

        [Tooltip ("Defines Seed for fragmentation algorithm. Same Seed will produce same fragments for same object every time.")]
        [Range (1, 50)]
        public int seed;
        
        [Tooltip ("Allows to use RayFire Shatter properties for fragmentation. Works only if object has RayFire Shatter component.")]
        public bool useShatter;
        
        [Header ("  Advanced")]
        [Space (2)]
        
        public RFFragmentProperties properties;
        // public bool preInput;
        // public RFMeshInput meshInput;
        public RFRuntimeCaching runtimeCaching;
        
        // Hidden
        [HideInInspector] public int badMesh;
        [HideInInspector] public int shatterMode;
        [HideInInspector] public int totalAmount;
        [HideInInspector] public int innerSubId;
        [HideInInspector] public bool compressPrefab;
        [HideInInspector] public Quaternion cacheRotationStart; 
        [HideInInspector] public Mesh mesh;
        [HideInInspector] public RFShatter rfShatter;
        [HideInInspector] public RayfireShatter scrShatter;
        
       
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFDemolitionMesh()
        {
            amount             = 15;
            variation          = 0;
            depthFade          = 0.5f;
            contactBias        = 0f;
            seed               = 1;
            useShatter         = false;
            
            properties         = new RFFragmentProperties();
            runtimeCaching     = new RFRuntimeCaching();
            
            Reset();
            
            shatterMode        = 1;
            innerSubId         = 0;
            compressPrefab     = true;
            cacheRotationStart = Quaternion.identity;
            
            mesh               = null;
            rfShatter          = null;
        }

        // Copy from
        public void CopyFrom (RFDemolitionMesh demolition)
        {
            amount         = demolition.amount;
            variation      = demolition.variation;
            depthFade      = demolition.depthFade;
            seed           = demolition.seed;
            contactBias    = demolition.contactBias;
            useShatter     = false;
            
            properties.CopyFrom (demolition.properties);
            runtimeCaching = new RFRuntimeCaching();
            
            Reset();
            
            shatterMode    = 1;
            innerSubId     = 0;
            compressPrefab     = true;
            cacheRotationStart = Quaternion.identity;
                        
            mesh        = null;
            rfShatter   = null;
        }
        
        // Reset
        public void Reset()
        {
            badMesh        = 0;
            totalAmount    = 0;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        // Demolish single mesh to fragments
        public static bool DemolishMesh(RayfireRigid scr)
        {
            // Object demolition
            if (scr.objectType != ObjectType.Mesh && scr.objectType != ObjectType.SkinnedMesh)
                return true;
            
            // Skip if reference
            if (scr.demolitionType == DemolitionType.ReferenceDemolition)
                return true;
            
            // Already has fragments
            if (scr.HasFragments == true)
            {
                // Set tm 
                scr.rootChild.position         = scr.transForm.position;
                scr.rootChild.rotation         = scr.transForm.rotation;
                scr.rootChild.transform.parent = RayfireMan.inst.transForm;
                
                // Activate root and fragments
                scr.rootChild.gameObject.SetActive (true);
                
                // Start all coroutines
                for (int i = 0; i < scr.fragments.Count; i++)
                {
                    scr.fragments[i].StartAllCoroutines();
                }

                scr.limitations.demolished = true;
                return true;
            }
            
            // Has serialized meshes but has no Unity meshes - convert to unity meshes
            if (scr.HasRfMeshes == true && scr.HasMeshes == false)
                RFMesh.ConvertRfMeshes (scr);
            
            // Has unity meshes - create fragments
            if (scr.HasMeshes == true)
            {
                scr.fragments = CreateFragments(scr);
                scr.limitations.demolished = true;
                return true;
            }

            // Still has no Unity meshes - cache Unity meshes
            if (scr.HasMeshes == false)
            {
                // Cache unity meshes
                CacheRuntime(scr);

                // Caching in progress. Stop demolition
                if (scr.meshDemolition.runtimeCaching.inProgress == true)
                    return false;

                // Has unity meshes - create fragments
                if (scr.HasMeshes == true)
                {
                    scr.fragments = CreateFragments(scr);
                    scr.limitations.demolished = true;
                    return true;
                }
            }
            
            return false;
        }
        
        // Create fragments by mesh and pivots array
        public static List<RayfireRigid> CreateFragments (RayfireRigid scr)
        {
            // Fragments list
            List<RayfireRigid> scrArray = new List<RayfireRigid>();

            // Stop if has no any meshes
            if (scr.meshes == null)
                return scrArray;
            
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Create root object and parent
            RFLimitations.CreateRoot (scr);
            
            // Vars 
            int    baseLayer = scr.meshDemolition.GetLayer(scr);
            string baseTag   = scr.gameObject.tag;
            string baseName  = scr.gameObject.name + "_fr_";

            // Save original rotation
            // Quaternion originalRotation = rootChild.transform.rotation;
            
            // Set rotation to precache rotation
            if (scr.demolitionType == DemolitionType.AwakePrecache ||
                scr.demolitionType == DemolitionType.ManualPrecache ||
                scr.demolitionType == DemolitionType.ManualPrefabPrecache)
                scr.rootChild.transform.rotation = scr.cacheRotation;

            // Get original mats
            Material[] mats = scr.skinnedMeshRend != null
                ? scr.skinnedMeshRend.sharedMaterials
                : scr.meshRenderer.sharedMaterials;

            // Create fragment objects
            for (int i = 0; i < scr.meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst == null
                    ? RayfireMan.CreateRigidInstance()
                    : RayfireMan.inst.GetPoolObject();

                // Setup
                rfScr.transform.position    = scr.transForm.position + scr.pivots[i];
                rfScr.transform.parent      = scr.rootChild;
                rfScr.name                  = baseName + i;
                rfScr.gameObject.tag        = baseTag;
                rfScr.gameObject.layer      = baseLayer;
                rfScr.meshFilter.sharedMesh = scr.meshes[i];
                rfScr.rootParent            = scr.rootChild;

                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (rfScr);

                // Set collider
                RFPhysic.SetFragmentMeshCollider (rfScr, scr.meshes[i]);
                
                // Shadow casting
                if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                    RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > scr.meshes[i].bounds.size.magnitude)
                    rfScr.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                
                // Turn on
                rfScr.gameObject.SetActive (true);

                // Set multymaterial
                RFSurface.SetMaterial (scr.subIds, mats, scr.materials, rfScr.meshRenderer, i, scr.meshes.Length);

                // Update depth level and amount
                rfScr.limitations.currentDepth = scr.limitations.currentDepth + 1;
                rfScr.meshDemolition.amount = (int)(rfScr.meshDemolition.amount * rfScr.meshDemolition.depthFade);
                if (rfScr.meshDemolition.amount < 2)
                    rfScr.meshDemolition.amount = 2;

                // Add in array
                scrArray.Add (rfScr);
            }

            // Fix transform for precached fragments
            if (scr.demolitionType == DemolitionType.AwakePrecache ||
                scr.demolitionType == DemolitionType.ManualPrecache ||
                scr.demolitionType == DemolitionType.ManualPrefabPrecache)
                scr.rootChild.rotation = scr.transForm.rotation;

            // Fix runtime caching rotation difference. Get rotation difference and add to root
            if (scr.demolitionType == DemolitionType.Runtime && scr.meshDemolition.runtimeCaching.type != CachingType.Disable)
            {
                Quaternion cacheRotationDif = scr.transForm.rotation * Quaternion.Inverse (scr.meshDemolition.cacheRotationStart);
                scr.rootChild.rotation = cacheRotationDif * scr.rootChild.rotation;
            }

            return scrArray;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Start cache fragment meshes. Instant or runtime
        static void CacheRuntime (RayfireRigid scr)
        {
            // Reuse existing cache
            if (scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset && scr.reset.mesh == RFReset.MeshResetType.ReuseFragmentMeshes)
                if (scr.HasMeshes == true)
                    return;

            // Clear all mesh data
            scr.DeleteCache();

            // Cache meshes
            if (scr.meshDemolition.runtimeCaching.type == CachingType.Disable)
                CacheInstant(scr);
            else
                scr.CacheFrames();
        }
        
        // Instant caching into meshes
        public static void CacheInstant (RayfireRigid scr)
        {
            //Debug.Log ("CacheInstant");

            // Timestamp
            //float t1 = Time.realtimeSinceStartup;

            // Input mesh, setup
            if (RFFragment.PrepareCacheMeshes (scr) == false)
                return;

            // Timestamp
            //float t2 = Time.realtimeSinceStartup;

            RFFragment.CacheMeshesInst (ref scr.meshes, ref scr.pivots, ref scr.subIds, scr);

            // Timestamp
            //float t3 = Time.realtimeSinceStartup;

            //Debug.Log (t2 - t1);
            //Debug.Log (t3 - t2);
        }       
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Get layer for fragments
        public int GetLayer (RayfireRigid scr)
        {
            // Inherit layer
            if (properties.layer.Length == 0)
                return scr.gameObject.layer;
            
            // No custom layer
            if (RayfireMan.inst.layers.Contains (properties.layer) == false)
                return 0;

            // Get custom layer
            return LayerMask.NameToLayer (properties.layer);
        }
        
        // Cor to fragment mesh over several frames
        public IEnumerator RuntimeCachingCor (RayfireRigid scr)
        {
            // Object should be demolished when cached all meshes but not during caching
            bool demolitionShouldLocal = scr.limitations.demolitionShould == true;
            scr.limitations.demolitionShould = false;
            
            // Input mesh, setup, record time
            float t1 = Time.realtimeSinceStartup;
            if (RFFragment.PrepareCacheMeshes (scr) == false)
                yield break;
                        
            // Set list with amount of mesh for every frame
            List<int> batchAmount = runtimeCaching.type == CachingType.ByFrames
                ? RFRuntimeCaching.GetBatchByFrames(runtimeCaching.frames, totalAmount)
                : RFRuntimeCaching.GetBatchByFragments(runtimeCaching.fragments, totalAmount);
            
            // Caching in progress
            runtimeCaching.inProgress = true;

            // Wait next frame if input took too much time or long batch
            float t2 = Time.realtimeSinceStartup - t1;
            if (t2 > 0.025f || batchAmount.Count > 5)
                yield return null;

            // Save tm for multi frame caching
            GameObject tmRefGo = RFRuntimeCaching.CreateTmRef (scr);

            // Start rotation
            cacheRotationStart = scr.transForm.rotation;
            
            // Iterate every frame. Calc local frame meshes
            List<Mesh>         meshesList = new List<Mesh>();
            List<Vector3>      pivotsList = new List<Vector3>();
            List<RFDictionary> subList    = new List<RFDictionary>();
            for (int i = 0; i < batchAmount.Count; i++)
            {
                // Check for stop
                if (runtimeCaching.stop == true)
                {
                    ResetRuntimeCaching(scr, tmRefGo);
                    yield break;
                }
                
                // Cache defined points
                RFFragment.CacheMeshesMult (tmRefGo.transform, ref meshesList, ref pivotsList, ref subList, scr, batchAmount, i);
                // TODO create fragments for current batch
                // TODO record time and decrease batches amount if less 30 fps
                yield return null;
            }
            
            // Set to main data vars
            scr.meshes = meshesList.ToArray();
            scr.pivots = pivotsList.ToArray();
            scr.subIds = subList;

            // Clear
            scr.DestroyObject (tmRefGo);
            scr.meshDemolition.scrShatter = null;
            
            // Set demolition ready state
            if (runtimeCaching.skipFirstDemolition == false && demolitionShouldLocal == true)
                scr.limitations.demolitionShould = true;
            
            // Reset damage
            if (runtimeCaching.skipFirstDemolition == true && demolitionShouldLocal == true)
                scr.damage.Reset();
            
            // Caching finished
            runtimeCaching.inProgress = false;
            runtimeCaching.wasUsed = true;
        }

        // Stop runtime caching and reset it
        public void StopRuntimeCaching()
        {
            if (runtimeCaching.inProgress == true)
                runtimeCaching.stop = true;
        }
        
        void ResetRuntimeCaching (RayfireRigid scr, GameObject tmRefGo)
        {
            scr.DestroyObject (tmRefGo);
            runtimeCaching.stop = false;
            runtimeCaching.inProgress = false;
            scr.meshDemolition.rfShatter = null;
            scr.DeleteCache();
        }
    }
}