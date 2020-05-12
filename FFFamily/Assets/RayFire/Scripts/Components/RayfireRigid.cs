using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-rigid-component/")]
    public class RayfireRigid : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }
        
        [Space (2)]
        public InitType initialization = InitType.ByMethod;
        [HideInInspector] public bool initialized;
        
        [Header ("  Main")]
        [Space (3)]
        
        [Tooltip ("Defines behaviour of object during simulation.")]
        public SimType simulationType = SimType.Dynamic;
        [Space (2)]
        public ObjectType objectType = ObjectType.Mesh;
        [Space (2)]
        public DemolitionType demolitionType = DemolitionType.None;
        
        [Header ("  Simulation")]
        [Space (3)]
        
        public RFPhysic     physics    = new RFPhysic();
        [Space (2)]
        public RFActivation activation = new RFActivation();
        [Space (2)]
        public RFRestriction restriction = new RFRestriction();
        
        [Header ("  Demolition")]
        [Space (3)]
        
        public RFLimitations         limitations         = new RFLimitations();
        [Space (2)]
        public RFDemolitionMesh      meshDemolition      = new RFDemolitionMesh();
        [Space (2)]
        public RFDemolitionCluster   clusterDemolition   = new RFDemolitionCluster();
        [Space (2)]
        public RFReferenceDemolition referenceDemolition = new RFReferenceDemolition();
        [Space (2)]
        public RFSurface             materials           = new RFSurface();
        [Space (2)]
        public RFDamage              damage              = new RFDamage();
        
        [Header ("  Common")]
        [Space (3)]
        
        public RFFade                fading              = new RFFade();
        [Space (2)]
        public RFReset               reset               = new RFReset();
        //public RFSound               sound = new RFSound();
        
        [Header ("  Info")]

        // Hidden
        [HideInInspector] public Mesh[] meshes;
        [HideInInspector] public Vector3[] pivots;
        [HideInInspector] public RFMesh[] rfMeshes;
        [HideInInspector] public List<RFDictionary> subIds;
        [HideInInspector] public List<RayfireRigid> fragments;
        [HideInInspector] public Quaternion cacheRotation; // NOTE. Should be public, otherwise rotation error on demolition.

        [HideInInspector] public Transform transForm;
        [HideInInspector] public Transform rootChild;
        [HideInInspector] public Transform rootParent;
        
        [HideInInspector] public MeshFilter meshFilter;
        [HideInInspector] public MeshRenderer meshRenderer;
        [HideInInspector] public SkinnedMeshRenderer skinnedMeshRend;
        [HideInInspector] public RayfireDebris scrDebris;
        [HideInInspector] public RayfireDust scrDust;
        
        // Events
        public RFDemolitionEvent demolitionEvent = new RFDemolitionEvent();
        public RFActivationEvent activationEvent = new RFActivationEvent();
        public RFRestrictionEvent restrictionEvent = new RFRestrictionEvent();

        // Awake
        void Awake()
        {
            if (initialization == InitType.AtStart)
            {
                AwakeMethods();
                StartMethods();
            }
        }

        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();

            // Init mesh root.
            if (SetRootMesh() == true)
                return;
            
            // Set components for mesh / skinned mesh / clusters
            SetComponentsBasic();

            // Check for user mistakes
            RFLimitations.Checks(this);

            // Set components for mesh / skinned mesh / clusters
            SetComponentsPhysics();
            
            // Input mesh for runtime mesh demolition
            // PreInput();
            
            // Precache meshes at awake
            AwakePrecache(); 
            
            // Prefragment object at awake
            AwakePrefragment();
        }
        
        // Start ops
        void StartMethods()
        {
            // Excluded from simulation
            if (physics.exclude == true)
                return;

            // Set Start variables
            SetObjectType();

            // Start all coroutines
            StartAllCoroutines();

            // Object initialized
            initialized = true;
        }

        // Initialize 
        public void Initialize()
        {
            if (initialized == false)
            {
                AwakeMethods();
                StartMethods();
            }

            // TODO add reinit for already initialized objects in case of property change
            else
            {
                
            }
        }
        
        // Reset rigid data
        public void Default()
        {
            // Reset
            limitations.Reset();
            meshDemolition.Reset();
            clusterDemolition.Reset();
            
            limitations.birthTime = Time.time + Random.Range (0f, 0.3f);

            // Birth position for activation check
            physics.initScale = transForm.localScale;
            physics.initPosition = transForm.position;
            physics.initRotation = transForm.rotation;
            
            // Set bound and size
            RFLimitations.SetBound(this);
        }
        
        /*// Input mesh for runtime mesh demolition
        void PreInput()
        {
            if (meshDemolition.preInput == false)
                return;
            
            if (objectType != ObjectType.Mesh)
                return;
            
            if (demolitionType != DemolitionType.Runtime)
                return;

            if (RFFragment.SetRigidShatter (this) == false)
            {
               Debug.Log ("no input");
            }
            else
            {
                Debug.Log (gameObject.name);
            }
        }*/
        
        /// /////////////////////////////////////////////////////////
        /// Awake ops
        /// /////////////////////////////////////////////////////////

        // Init mesh root. Copy Rigid component for children with mesh
        bool SetRootMesh()
        {
            if (objectType == ObjectType.MeshRoot)
            {
                // Stop if already initiated
                if (limitations.demolished == true || physics.exclude == true)
                    return true;
                
                // Get children
                List<Transform> children = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                    children.Add (transform.GetChild (i));
                
                // Add Rigid to child with mesh
                fragments = new List<RayfireRigid>();
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].GetComponent<MeshFilter>() != null)
                    {
                        // Get rigid  // TODO check if fragment already has Rigid, Reinit in this case.
                        RayfireRigid childRigid = children[i].gameObject.GetComponent<RayfireRigid>();
                        if (childRigid == null)
                            childRigid = children[i].gameObject.AddComponent<RayfireRigid>();
                        fragments.Add (childRigid);
                        
                        // Copy parent properties
                        CopyPropertiesTo (childRigid);
                        
                        // Init
                        childRigid.Initialize();
                    }
                }

                // TODO Setup as clusters root children with transform only

                // Turn off demolition and physics
                demolitionType  = DemolitionType.None;
                physics.exclude = true;
                return true;
            }

            return false;
        }
        
        // Define basic components
        void SetComponentsBasic()
        {
            // Set shatter component
            meshDemolition.scrShatter = meshDemolition.useShatter == true 
                ? GetComponent<RayfireShatter>() 
                : null;
            
            // Other
            transForm       = GetComponent<Transform>();
            meshFilter      = GetComponent<MeshFilter>();
            meshRenderer    = GetComponent<MeshRenderer>();
            skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();
            scrDebris       = GetComponent<RayfireDebris>();
            scrDust         = GetComponent<RayfireDust>();
            
            // Add missing mesh renderer
            if (meshFilter != null && meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        // Define components
        void SetComponentsPhysics()
        {
            // Excluded from simulation
            if (physics.exclude == true)
                return;
            
            // Physics components
            physics.rigidBody = GetComponent<Rigidbody>();
            physics.meshCollider = GetComponent<Collider>();

            // Mesh Set collider
            if (objectType == ObjectType.Mesh)
                RFPhysic.SetMeshCollider (this);

            // Cluster check TODO EXPOSE IN UI !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (objectType == ObjectType.NestedCluster || objectType == ObjectType.ConnectedCluster)
            {
                // No children mesh for clustering
                bool clusteringState = RFDemolitionCluster.Clusterize (this);
                if (clusteringState == false)
                {
                    physics.exclude = true;
                    Debug.Log ("RayFire Rigid: " + name + " has no children with mesh. Object Excluded from simulation.", gameObject);
                    return;
                }
            }
            
            // Rigid body
            if (simulationType != SimType.Static && physics.rigidBody == null)
            {
                physics.rigidBody = gameObject.AddComponent<Rigidbody>();
                physics.rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Start ops
        /// /////////////////////////////////////////////////////////
        
        // Set Start variables
        void SetObjectType ()
        {
            if (objectType == ObjectType.Mesh ||
                objectType == ObjectType.NestedCluster ||
                objectType == ObjectType.ConnectedCluster)
            {
                // Reset rigid data
                Default();

                // Set physics properties
                SetPhysics();
            }
        }

        // Set physics properties
        void SetPhysics()
        {
            // Excluded from sim
            if (physics.exclude == true)
                return;
            
            // MeshCollider physic material preset. Set new or take from parent 
            RFPhysic.SetColliderMaterial (this);

            // Set physical simulation type. Important. Should after collider material define
            RFPhysic.SetSimulationType (this);

            // Do not set convex, mass, drag for static
            if (simulationType == SimType.Static)
                return;
            
            // Convex collider meshCollider. After SetSimulation Type to turn off convex for kinematic
            RFPhysic.SetColliderConvex (this);
            
            // Set density. After collider defined
            RFPhysic.SetDensity (this);

            // Set drag properties
            RFPhysic.SetDrag (this);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Start all coroutines
        public void StartAllCoroutines()
        {
            // Stop if static
            if (simulationType == SimType.Static)
                return;
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Inactive
            if (gameObject.activeSelf == false)
                return;
            
            // Check for demolition state every frame
            if (demolitionType != DemolitionType.None)
                StartCoroutine (limitations.DemolishableCor(this));
            
            // Cache physics data for fragments 
            StartCoroutine (physics.PhysicsDataCor(this));
            
            // Activation by velocity\offset coroutines
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
            {
                if (activation.byVelocity > 0)
                    StartCoroutine (activation.ActivationVelocityCor(this));
                if (activation.byOffset > 0)
                    StartCoroutine (activation.ActivationOffsetCor(this));
            }
            
            // Init inactive every frame update coroutine
            if (simulationType == SimType.Inactive)
                StartCoroutine (activation.InactiveCor(this));
            
            // Init restriction check
            RFRestriction.InitRestriction (this);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition types
        /// /////////////////////////////////////////////////////////
        
        // Precache meshes at awake
        void AwakePrecache()
        {
            if (demolitionType == DemolitionType.AwakePrecache && objectType == ObjectType.Mesh)
                RFDemolitionMesh.CacheInstant(this);
        }
        
        // Predefine fragments
        void AwakePrefragment()
        {
            if (demolitionType == DemolitionType.AwakePrefragment && objectType == ObjectType.Mesh)
            {
                // Cache meshes
                RFDemolitionMesh.CacheInstant(this);

                // Predefine fragments
                Prefragment();
            }
        }
        
        // Precache meshes for prefab in editor
        public void PrefabPrecache()
        {
            if (demolitionType == DemolitionType.ManualPrefabPrecache && objectType == ObjectType.Mesh)
            {
                // Set components for mesh / skinned mesh / clusters
                SetComponentsBasic();
                
                // Cache meshes
                RFDemolitionMesh.CacheInstant(this);

                // Convert meshes to RFmeshes
                if (HasMeshes == true)
                {
                    rfMeshes = new RFMesh[meshes.Length];
                    for (int i = 0; i < meshes.Length; i++)
                        rfMeshes[i] = new RFMesh (meshes[i], meshDemolition.compressPrefab);
                }
                meshes = null;
            }
        }

        // Precache meshes in editor
        public void ManualPrecache()
        {
            if (demolitionType == DemolitionType.ManualPrecache && objectType == ObjectType.Mesh)
            {
                // Set components
                SetComponentsBasic();
                
                // Set components
                SetComponentsPhysics();
                
                // Cache meshes
                RFDemolitionMesh.CacheInstant(this);
            }
            else if (demolitionType == DemolitionType.ManualPrecache && objectType != ObjectType.Mesh)
                Debug.Log ("RayFire Rigid: " + name + " Object Type is not Mesh. Set to Mesh type to Precache.", gameObject);
        }
        
        // Precache meshes in editor
        public void ManualPrefragment()
        {
            if (demolitionType == DemolitionType.ManualPrefragment && objectType == ObjectType.Mesh)
            {
                // Set components
                SetComponentsBasic();
                
                // Set components
                SetComponentsPhysics();
               
                // Clear all mesh data
                DeleteCache();
                
                // Cache meshes
                RFDemolitionMesh.CacheInstant(this);

                // Predefine fragments
                Prefragment();
            }
            else if (demolitionType == DemolitionType.ManualPrefragment && objectType != ObjectType.Mesh)
                Debug.Log ("RayFire Rigid: " + name + " Object Type is not Mesh. Set to Mesh type to Prefragment.", gameObject);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Collision check
        void OnCollisionEnter (Collision collision)
        {
            // TODO check if it is better to check state or collisions str
            
            // Demolish object check
            if (DemolitionState() == false)
                return;
            
            // Check if collision demolition passed
            if (CollisionDemolition (collision) == true)
                limitations.demolitionShould = true;
        }
        
        // Check if collision demolition passed
        bool CollisionDemolition (Collision collision)
        {
            // Collision with kinematic object
            if (collision.rigidbody != null && collision.rigidbody.isKinematic == true)
            {
                if (collision.impulse.magnitude > physics.Solidity * limitations.solidity * RayfireMan.inst.globalSolidity * 7f)
                {
                    limitations.contactPoint = collision.contacts[0].point;
                    return true;
                }
            }

            // Collision force checks
            for (int i = 0; i < collision.contacts.Length; i++)
            {
                // Set contact point
                limitations.contactPoint = collision.contacts[i].point;
                
                // Demolish if collision high enough
                if (collision.relativeVelocity.magnitude > physics.Solidity * limitations.solidity * RayfireMan.inst.globalSolidity)
                    return true;
                
                // Collect damage by collision
                if (damage.enable == true && damage.collect == true)
                    if (ApplyDamage (collision.relativeVelocity.magnitude * damage.multiplier, limitations.contactPoint) == true)
                        return true;
            }

            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

         // Demolition available state
        public bool State ()
        {
            // Object already demolished
            if (limitations.demolished == true)
                return false;

            // Object already passed demolition state and demolishing is in progress
            if (meshDemolition.runtimeCaching.inProgress == true)
                return false;
            
            // Bad mesh check
            if (meshDemolition.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                return false;

            // Max amount check
            if (RayfireMan.MaxAmountCheck == false)
                return false;

            // Depth level check
            if (limitations.depth > 0 && limitations.currentDepth >= limitations.depth)
                return false;

            // Min Size check. Min Size should be considered and size is less than
            if (limitations.bboxSize < limitations.size)
                return false;

            // Safe frame
            if (Time.time - limitations.birthTime < limitations.time)
                return false;
            
            // Fading
            if (fading.state == 2)
                return false;

            return true;
        }
        
        // Check if object should be demolished
        public bool DemolitionState ()
        {
            // No demolition allowed
            if (demolitionType == DemolitionType.None)
                return false;
            
            // Non destructible material
            if (physics.Destructible == false)
                return false;

            // Demolition available check
            if (State() == false)
                return false;

            // Per frame time check
            if (RayfireMan.inst.timeQuota > 0 && RayfireMan.inst.maxTimeThisFrame > RayfireMan.inst.timeQuota)
                return false;

            return true;
        }
        
        // Demolish object
        public void Demolish()
        {
            // Initialize if not
            if (initialized == false)
                Initialize();

            // Static objects can not be demolished
            if (simulationType == SimType.Static)
                return;

            // Timestamp
            float t1 = Time.realtimeSinceStartup;
            
            // Restore position and rotation to prevent high collision offset
            transForm.position = physics.position;
            transForm.rotation = physics.rotation;

            // Demolish mesh or cluster to reference
            if (RFReferenceDemolition.DemolishReference(this) == false)
                return;

            // Demolish mesh and create fragments. Stop if runtime caching or no meshes/fragments were created
            if (RFDemolitionMesh.DemolishMesh(this) == false)
                return;
            
            // Demolish cluster to children nodes 
            RFDemolitionCluster.DemolishCluster(this);

            // Check fragments and proceed
            if (limitations.demolished == false)
            {
                demolitionType = DemolitionType.None;
                Debug.Log ("Demolish Object error: " + gameObject.name, gameObject);
                return;
            }

            // Connectivity check
            activation.CheckConnectivity();
            
            // Fragments initialisation
            InitFragments();
            
            // Sum total demolition time
            RayfireMan.inst.maxTimeThisFrame += Time.realtimeSinceStartup - t1;
            
            // Init particles
            RFParticles.InitDemolitionParticles(this);

            // Event
            demolitionEvent.InvokeLocalEvent (this);
            RFDemolitionEvent.InvokeGlobalEvent (this);
            
            // Destroy demolished object
            RayfireMan.DestroyFragment (this, rootParent);
            
            // Timestamp
            //float t2 = Time.realtimeSinceStartup;
            //Debug.Log (t2 - t1);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fragments
        /// /////////////////////////////////////////////////////////
        
        // Copy rigid properties from parent to fragments
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Object type
            toScr.objectType = objectType;

            // Set mesh type if copied from mesh root
            if (objectType == ObjectType.MeshRoot || objectType == ObjectType.SkinnedMesh)
                toScr.objectType = ObjectType.Mesh;

            // Sim type
            toScr.simulationType = simulationType;
            
            // Demolition type
            toScr.demolitionType = demolitionType;
            if (objectType != ObjectType.MeshRoot)
                if (demolitionType != DemolitionType.None)
                    toScr.demolitionType = DemolitionType.Runtime;
            
            // Copy physics
            toScr.physics.CopyFrom (physics);
            if (objectType != ObjectType.MeshRoot)
                if (simulationType == SimType.Sleeping)
                    toScr.simulationType = SimType.Dynamic;
                
            toScr.activation.CopyFrom (activation);
            toScr.restriction.CopyFrom (restriction);
            toScr.limitations.CopyFrom (limitations);
            toScr.meshDemolition.CopyFrom (meshDemolition);
            toScr.clusterDemolition.CopyFrom (clusterDemolition);
            
            // Copy reference demolition props
            if (objectType == ObjectType.MeshRoot)
                toScr.referenceDemolition.CopyFrom (referenceDemolition);
            
            toScr.materials.CopyFrom (materials);
            toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            toScr.reset.CopyFrom (this);
            
            // Copy debris
            if (scrDebris != null)
            {
                if (toScr.scrDebris == null)
                    toScr.scrDebris = toScr.gameObject.AddComponent<RayfireDebris>();
                toScr.scrDebris.debris.CopyFrom (scrDebris.debris);
            }
            
            // Copy dust
            if (scrDust != null)
            {
                if (toScr.scrDust == null)
                    toScr.scrDust = toScr.gameObject.AddComponent<RayfireDust>();
                toScr.scrDust.dust.CopyFrom (scrDust.dust);
            }
        }
        
        // Fragments initialisation
        void InitFragments()
        {
            // No fragments
            if (HasFragments == false)
                return;
            
            // Set velocity
            RFPhysic.SetFragmentsVelocity (this);
           
            // TODO set current frame for cluster demol types
            
            // Sum total new fragments amount
            RayfireMan.inst.advancedDemolitionProperties.currentAmount += fragments.Count;
            
            // Set ancestor
            RFLimitations.SetAncestor (this);
            RFLimitations.SetDescendants (this);
            
            // Fading. move to fragment
            if (fading.onDemolition == true)
                fading.DemolitionFade (fragments);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Manual methods
        /// /////////////////////////////////////////////////////////
        
        // Predefine fragments
        void Prefragment()
        {
            // Delete existing
            DeleteFragments();

            // Create fragments from cache
            fragments = RFDemolitionMesh.CreateFragments(this);
                
            // Stop
            if (HasFragments == false)
            {
                demolitionType = DemolitionType.None;
                return;
            }
            
            // Set physics properties
            foreach (var scr in fragments)
            {
                scr.SetComponentsBasic();
                scr.SetComponentsPhysics();
                scr.SetObjectType();
            }
            
            // Deactivate fragments root
            if (rootChild != null)
                rootChild.gameObject.SetActive (false);
        }

        // Clear cache info TODO do not erase pivots, keep for fragments reuse
        public void DeleteCache()
        {
            meshes   = null;
            pivots   = null;
            rfMeshes = null;
            subIds   = new List<RFDictionary>();
        }
        
        // Delete fragments
        public void DeleteFragments()
        {
            // Destroy root
            if (rootChild != null)
            {
                if (Application.isPlaying == true)
                    Destroy (rootChild.gameObject);
                else
                    DestroyImmediate (rootChild.gameObject);

                // Clear ref
                rootChild = null;
            }

            // Clear array
            fragments = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Blade
        /// /////////////////////////////////////////////////////////

        // Add new slice plane
        public void AddSlicePlane (Vector3[] slicePlane)
        {
            // Not even amount of slice data
            if (slicePlane.Length % 2 == 1)
                return;

            // Add slice plane data
            limitations.slicePlanes.AddRange (slicePlane);
        }
        
        // Slice object
        public void SliceObjectByPlanes()
        {
            // Empty lists
            DeleteCache();
            DeleteFragments();
    
            // SLice
            RFFragment.SliceMeshes (ref meshes, ref pivots, ref subIds, this, limitations.slicePlanes);

            // Remove plane info 
            limitations.slicePlanes.Clear();

            // Stop
            if (HasMeshes == false)
                return;

            // Get fragments
            fragments = CreateSlices();

            // TODO check for fragments
            
            // Set demolition 
            limitations.demolished = true;
            
            // Fragments initialisation
            InitFragments();

            // Event
            demolitionEvent.InvokeLocalEvent (this);
            RFDemolitionEvent.InvokeGlobalEvent (this);

            // Destroy original
            RayfireMan.DestroyFragment (this, rootParent);
        }

        // Create slices by mesh and pivots array
        List<RayfireRigid> CreateSlices()
        {
            // Create root object
            RFLimitations.CreateRoot (this);

            // Clear array for new fragments
            List<RayfireRigid> scrArray = new List<RayfireRigid>();

            // Vars 
            int    baseLayer = meshDemolition.GetLayer(this);
            string baseTag   = gameObject.tag;
            string baseName  = gameObject.name + "_sl_";

            // Create fragment objects
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst.GetPoolObject();

                // Setup
                rfScr.transform.position         = transForm.position + pivots[i];
                rfScr.transform.parent           = rootChild;
                rfScr.name                       = baseName + i;
                rfScr.gameObject.tag             = baseTag;
                rfScr.gameObject.layer           = baseLayer;
                rfScr.meshFilter.sharedMesh      = meshes[i];
                rfScr.meshFilter.sharedMesh.name = baseName + i;
                rfScr.rootParent                 = rootChild;

                // Copy properties from parent to fragment node
                CopyPropertiesTo (rfScr);

                // Shadow casting
                if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                    RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > meshes[i].bounds.size.magnitude)
                    rfScr.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

                // Turn on
                rfScr.gameObject.SetActive (true);

                // Set multymaterial
                RFSurface.SetMaterial (subIds, meshRenderer.sharedMaterials, materials, rfScr.meshRenderer, i, meshes.Length);

                // Inherit same current depth level
                rfScr.limitations.currentDepth = limitations.currentDepth + 1;

                // Set collider mesh
                MeshCollider mc = rfScr.physics.meshCollider as MeshCollider;
                if (mc != null)
                {
                    mc.sharedMesh = meshes[i];
                    mc.name       = meshes[i].name;
                }

                // Add in array
                scrArray.Add (rfScr);
            }

            // Empty lists
            DeleteCache();

            return scrArray;
        }

        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Caching into meshes over several frames
        public void CacheFrames()
        {
            StartCoroutine (meshDemolition.RuntimeCachingCor(this));
        }

        /// /////////////////////////////////////////////////////////
        /// Public methods
        /// /////////////////////////////////////////////////////////

        // Apply damage
        public bool ApplyDamage (float damageValue, Vector3 damagePoint, float damageRadius = 0f)
        {
            return RFDamage.ApplyDamage (this, damageValue, damagePoint, damageRadius);
        }
        
        // Activate inactive object
        public void Activate()
        {
            RFActivation.Activate (this);
        }
        
        // Fade this object
        public void Fade()
        {
            RFFade.Fade (this);
        }
        
        // Reset object
        public void ResetRigid()
        {
            RFReset.ResetRigid (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Destroy
        public void DestroyCollider(Collider col) { Destroy (col); }
        public void DestroyObject(GameObject go) { Destroy (go); }
        public void DestroyRigid(RayfireRigid rigid) { Destroy (rigid); }
        public void DestroyRb(Rigidbody rb) { Destroy (rb); }
        
        // Instantiate
        public GameObject InstantiateGo(GameObject  go) { return Instantiate (go); }
        public Mesh InstantiateMesh(Mesh mesh) { return Instantiate (mesh); }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Fragments/Meshes/RFMeshes check
        public bool HasFragments { get { return fragments != null && fragments.Count > 0; } }
        public bool HasMeshes { get { return meshes != null && meshes.Length > 0; } }
        public bool HasRfMeshes { get { return rfMeshes != null && rfMeshes.Length > 0; } }
    }
}

// Activation by continuity by weight
// Unyielding range
// man/awake amount diff because contact bias
// Precache on prefab loose cache
// mesh input in awake, 
// separate slice half, input for frag next frame
// awake cache slower at first demolition, faster if reused. check diff between precache and first demolition, move in awake expensive ops
// Peeling or surface fragmentation (custom point cloud), + not activated
// Replace Uhyielding component with Physic component to change any property