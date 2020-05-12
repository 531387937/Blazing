using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reinit at script change, creates second Manager on Manual prefragment

namespace RayFire
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RayFire/Rayfire Man")]
    [HelpURL("http://rayfirestudios.com/unity-online-help/unity-man-component/")]
    public class RayfireMan : MonoBehaviour
    {
        // Static instance
        public static RayfireMan inst;
  
        [Space(2)]
        
        [Header("  Gravity")]
        [Space(2)]
        
        public bool setGravity = false;
        [Range(0f, 1f)] public float multiplier = 1f;
        
        [Header("  Materials")]
        [Space(2)]
        
        [Range(0f, 1f)] public float minimumMass = 0.1f;
        [Range(0f, 400f)] public float maximumMass = 400f;
        public RFMaterialPresets materialPresets = new RFMaterialPresets();
        
        [Header("  Demolition")]
        [Space(2)]
        
        [Range(0f, 5f)] public float globalSolidity = 1f;
        [Tooltip("Maximum time in milliseconds per frame allowed to be used for demolitions. Off if 0.")]
        [Range(0f, 0.1f)] public float timeQuota = 0.033f;
        [HideInInspector] public float maxTimeThisFrame = 0f;
        [Space(1)]
        public RFManDemolition advancedDemolitionProperties = new RFManDemolition();
        
        [Header("  Fragment pooling")]
        [Space(2)]
        
        public bool enablePooling = true;
        [Range(1, 500)] public int poolLimit = 60;
        [HideInInspector] public int poolRate = 2;
        [HideInInspector] public List<RayfireRigid> poolList = new List<RayfireRigid>();
        [HideInInspector] public Transform poolRoot;
        [HideInInspector] public RayfireRigid poolInstance;
        
        [Header("  About")]
        [HideInInspector] public Transform transForm;
        public static int buildMajor = 1;
        public static int buildMinor = 18;

        // TODO will be used not in runtime. Check and warn
        // private bool nonRuntime = false;
        
        // List of layers
        [HideInInspector] public List<string> layers;
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Set static instance
            SetInstance();
        }

        // Update
        void LateUpdate()
        {
            maxTimeThisFrame = 0f;
        }

        // Set instance
        void SetInstance()
        {
            // Set new static instance
            if (inst == null)
            {
                inst = this;
            }

            // Static instance not defined
            if (inst != null)
            {
                // Instance is this mono
                if (inst == this)
                {
                    // Set vars
                    SetVariables();
                    
                    // Start pooling objects for fragments
                    StartPooling();
                }

                // Instance is not this mono
                if (inst != this)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Set vars
        void SetVariables()
        {
            // Get components
            transForm = GetComponent<Transform>();

            // Reset amount
            advancedDemolitionProperties.currentAmount = 0;

            // Set gravity
            SetGravity();
            
            // Set Physic Materials if needed
            materialPresets.SetMaterials();

            // Set all layers and tags
            SetLayers();
        }

        // Set gravity
        void SetGravity()
        {
            if (setGravity == true)
            {
                Physics.gravity = -9.81f * multiplier * Vector3.up;
            }
        }
    
        // Create RayFire manager if not created
        public static void RayFireManInit()
        {
            if (inst == null)
            {
                GameObject rfMan = new GameObject("RayFireMan");
                inst = rfMan.AddComponent<RayfireMan>();
            }

            EditorCreate();
        }

        // Set instance ops for editor creation
        static void EditorCreate()
        {
            if (Application.isPlaying == false)
            {
                inst.SetInstance();
            }
        }
        
        // Set list of layers and tags
        void SetLayers()
        {
            // Set layers list
            layers = new List<string>();
            for(int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName (i);
                if (layerName.Length > 0)
                    layers.Add (layerName);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Pooling
        /// /////////////////////////////////////////////////////////

        // Enable objects pooling for fragments                
        void StartPooling()
        {
            // Create pool root
            CreatePoolRoot();

            // Create pool instance
            CreateInstance();

            // Pooling. Mot in editor
            if (Application.isPlaying == true && enablePooling == true)
                StartCoroutine(StartPoolingCor());
        }

        // Create pool root
        void CreatePoolRoot()
        {
            if (poolRoot == null)
            {
                GameObject poolGo = new GameObject("Pool");
                poolRoot = poolGo.transform;
                poolRoot.position = transform.position;
                poolRoot.parent = transform;
            }
        }

        // Keep full pool 
        IEnumerator StartPoolingCor()
        {
            // Clear list
            poolList.Clear();

            // Pooling loop
            while (enablePooling == true)
            {
                // Create if not enough
                if (poolList.Count < poolLimit)
                    for (int i = 0; i < poolRate; i++)
                        poolList.Add(CreatePoolObject());

                // Wait next frame
                yield return null;
            }
        }

        // Create pool object
        private RayfireRigid CreatePoolObject()
        {
            // Create instance if null
            if (poolInstance == null)
                CreateInstance();
            
            // Create
            RayfireRigid rfScr = Instantiate(poolInstance);

            // Set parent
            rfScr.transform.parent = poolRoot;
           
            return rfScr;
        }

        // Create pool object
        private void CreateInstance()
        {
            // Return if not null
            if (poolInstance != null) 
                return;
            
            // Create pool instance
            poolInstance = CreateRigidInstance();

            // Set tm
            poolInstance.transForm.position = transform.position;
            poolInstance.transForm.rotation = transform.rotation;

            // Set parent
            poolInstance.transForm.parent = poolRoot;
        }

        // Create pool object
        public static RayfireRigid CreateRigidInstance()
        {
            // Create
            GameObject instance = new GameObject("Instance");

            // Turn off
            instance.SetActive(false);

            // Setup
            MeshFilter mf = instance.AddComponent<MeshFilter>();
            MeshRenderer mr = instance.AddComponent<MeshRenderer>();
            RayfireRigid rigidInstance = instance.AddComponent<RayfireRigid>();
            rigidInstance.initialization = RayfireRigid.InitType.AtStart;
            Rigidbody rb = instance.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                
            // Define components
            rigidInstance.transForm = instance.transform;
            rigidInstance.meshFilter = mf;
            rigidInstance.meshRenderer = mr;
            rigidInstance.physics.rigidBody = rb;

            return rigidInstance;
        }

        // Get pool object
        public RayfireRigid GetPoolObject()
        {
            RayfireRigid scr;
            if (poolList.Count > 0)
            {
                scr = poolList[poolList.Count - 1];
                poolList.RemoveAt(poolList.Count - 1);
            }
            else 
                scr = CreatePoolObject();
            return scr;
        }
        
        // Check if fragment is the last child in root and delete root as well
        public static void DestroyFragment(RayfireRigid scr, Transform tm, float time = 0f)
        {
            // Decrement total amount.
            if (Application.isPlaying == true)
                inst.advancedDemolitionProperties.currentAmount--;

            // Deactivate
            scr.gameObject.SetActive(false);
            
            // Destroy
            if (scr.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
                DestroyOp(scr, tm, time);
        }
        
        // Check if fragment is the last child in root and delete root as well
        public static void DestroyGo(GameObject go)
        {
            Destroy(go);
        }
        
        // Check if fragment is the last child in root and delete root as well
        public static void DestroyOp(RayfireRigid scr, Transform tm, float time = 0f)
        {
            // Set delay
            if (time == 0)
                time = scr.reset.destroyDelay;

            // Object is going to be destroyed. Timer is on
            scr.reset.toBeDestroyed = true;
            
            // Destroy object
            if (time <= 0f)
                Destroy(scr.gameObject);
            else
                Destroy(scr.gameObject, time);
            
            // Destroy root TODO destroy after delay time if no children
            if (tm != null && tm.childCount == 0)
                Destroy(tm.gameObject);
        }
        
        // Max fragments amount check
        public static bool MaxAmountCheck
        {
            get { return inst.advancedDemolitionProperties.currentAmount < inst.advancedDemolitionProperties.maximumAmount; }
        }
    }
}

