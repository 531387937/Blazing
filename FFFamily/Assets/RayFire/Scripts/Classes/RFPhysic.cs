using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFImpulse
    {
        public bool previewLinear;
        public bool enableLinear;
        public Vector3 linearVelocity;
        public float linearVelocityVariation;
        public int angularVelocityDivergence;
        
        public bool enableAngular;
        public Vector3 angularVelocity;
        public int angularVelocityVariation;
    }
    
    [Serializable]
    public class RFPhysic
    {
        [Header("  Physic Material")]
        [Space(1)]
        
        [Tooltip("Material preset with predefined density, friction, elasticity and solidity. Can be edited in Rayfire Man component.")]
        public MaterialType materialType;
        
        [Space(1)]
        [Tooltip("Allows to define own Physic Material.")]
        public PhysicMaterial material;
        
        [Header("  Mass")]
        [Space(1)]
        
        public MassType massBy;
        [Space(1)]
        [Range(0.1f, 100f)] public float mass;
        
        [Header ("  Other")]
        [Space(1)]
        
        public RFColliderType colliderType;
        public bool useGravity;
        
        [Header ("  Fragments")]
        [Space(1)]

        [Tooltip("Multiplier for demolished fragments velocity.")]
        [Range(0, 5f)] public float dampening;
        
        //public CollisionDetectionMode collisionDetection;
        
        // Hidden
        [HideInInspector] public bool recorder;
        [HideInInspector] public bool exclude;
        
        
        [HideInInspector] public Quaternion rotation;
        [HideInInspector] public Vector3 position;
        [HideInInspector] public Vector3 velocity;
        
        [HideInInspector] public Vector3 initScale;
        [HideInInspector] public Vector3 initPosition;
        [HideInInspector] public Quaternion initRotation;
        
        [HideInInspector] public Rigidbody rigidBody;
        [HideInInspector] public Collider meshCollider;
        [HideInInspector] public List<Collider> clusterColliders;
            
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFPhysic()
        {
            materialType = MaterialType.Concrete;
            material     = null;
            massBy       = MassType.MaterialDensity;
            mass         = 1f;
            colliderType = RFColliderType.Mesh;
            useGravity   = true;
            dampening    = 0.8f;
            
            Reset();

            rotation     = Quaternion.identity;
            position     = Vector3.zero;
            velocity     = Vector3.zero;
            
            initScale    = Vector3.one;
            initPosition = Vector3.zero;
            initRotation = Quaternion.identity;
        }

        // Copy from
        public void CopyFrom(RFPhysic physics)
        {
            materialType = physics.materialType;
            material     = physics.material;
            massBy       = physics.massBy;
            mass         = physics.mass;
            colliderType = physics.colliderType;
            useGravity   = physics.useGravity;
            dampening    = physics.dampening;

            Reset();
        }
        
        // Reset
        public void Reset()
        {
            recorder     = false;
            exclude      = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation Type
        /// /////////////////////////////////////////////////////////
        
        // Set simulation type properties
        public static void SetSimulationType(RayfireRigid scr)
        {
            // Dynamic
            if (scr.simulationType == SimType.Dynamic)
                SetDynamic(scr);

            // Sleeping 
            else if (scr.simulationType == SimType.Sleeping)
                SetSleeping(scr);

            // Inactive
            else if (scr.simulationType == SimType.Inactive)
                SetInactive(scr);

            // Kinematic
            else if (scr.simulationType == SimType.Kinematic)
                SetKinematic(scr);

            // Static
            else if (scr.simulationType == SimType.Static)
                SetStatic(scr);
        }

        // Set as dynamic
        static void SetDynamic(RayfireRigid scr)
        {
            // Set dynamic rigid body properties
            scr.physics.rigidBody.isKinematic = false;
            scr.physics.rigidBody.useGravity = scr.physics.useGravity;
        }

        // Set as sleeping
        static void SetSleeping(RayfireRigid scr)
        {
            // Set dynamic rigid body properties
            scr.physics.rigidBody.isKinematic = false;
            scr.physics.rigidBody.useGravity = scr.physics.useGravity;

            // Set sleep
            scr.physics.rigidBody.Sleep();
        }

        // Set as inactive
        static void SetInactive(RayfireRigid scr)
        {
            scr.physics.rigidBody.isKinematic = false;
            scr.physics.rigidBody.useGravity = false;

            // Set sleep
            scr.physics.rigidBody.Sleep();
        }

        // Set as Kinematic
        static void SetKinematic(RayfireRigid scr)
        {
            scr.physics.rigidBody.isKinematic = true;
            scr.physics.rigidBody.useGravity = scr.physics.useGravity;
        }

        // Set as Static
        static void SetStatic(RayfireRigid scr)
        {
           
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid body
        /// /////////////////////////////////////////////////////////
        
        // Set density. After collider defined.
        public static void SetDensity(RayfireRigid scr)
        {
            // Do not set mass for kinematik
            if (scr.physics.rigidBody.isKinematic == true)
                return;
                
            // Default mass from inspector
            float m = scr.physics.mass;

            // TODO  fragments inherit density, distribute mass from mesh root to all fragments
            
            // Get mass by density
            if (scr.physics.massBy == MassType.MaterialDensity)
            {
                scr.physics.rigidBody.SetDensity(RayfireMan.inst.materialPresets.Density(scr.physics.materialType));
                m = scr.physics.rigidBody.mass;
            }
            
            // Check for minimum mass
            if (RayfireMan.inst.minimumMass > 0)
                if (m < RayfireMan.inst.minimumMass)
                    m = RayfireMan.inst.minimumMass;
            
            // Check for maximum mass
            if (RayfireMan.inst.maximumMass > 0)
                if (m > RayfireMan.inst.maximumMass)
                    m = RayfireMan.inst.maximumMass;
            
            // Update mass in inspector
            scr.physics.rigidBody.mass = m;
        }
        
        // Set drag properties
        public static void SetDrag(RayfireRigid scr)
        {
            scr.physics.rigidBody.drag        = (RayfireMan.inst.materialPresets.Drag(scr.physics.materialType));
            scr.physics.rigidBody.angularDrag = (RayfireMan.inst.materialPresets.AngularDrag(scr.physics.materialType));
        }

        // Set velocity
        public static void SetFragmentsVelocity (RayfireRigid scr)
        {
            // Current velocity
            if (scr.meshDemolition.runtimeCaching.wasUsed == true && scr.meshDemolition.runtimeCaching.skipFirstDemolition == false)
            {
                for (int i = 0; i < scr.fragments.Count; i++)
                    if (scr.fragments[i] != null)
                        scr.fragments[i].physics.rigidBody.velocity = scr.physics.rigidBody.GetPointVelocity (scr.fragments[i].transForm.position) * scr.physics.dampening;
            }

            // Previous frame velocity
            else
            {
                Vector3 baseVelocity = scr.physics.velocity * scr.physics.dampening;
                for (int i = 0; i < scr.fragments.Count; i++)
                    if (scr.fragments[i].physics.rigidBody != null)
                        scr.fragments[i].physics.rigidBody.velocity = baseVelocity;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collider
        /// /////////////////////////////////////////////////////////
        
        // Set fragments collider
        public static void SetFragmentMeshCollider(RayfireRigid scr, Mesh mesh)
        {
            // Custom collider
            scr.physics.colliderType = scr.meshDemolition.properties.colliderType;
            if (scr.meshDemolition.properties.sizeFilter > 0)
                if (mesh.bounds.size.magnitude < scr.meshDemolition.properties.sizeFilter)
                    scr.physics.colliderType = RFColliderType.None;
            
            // Skip collider
            SetMeshCollider (scr, mesh);
        }
        
        // Set fragments collider
        public static void SetMeshCollider (RayfireRigid scr, Mesh mesh = null)
        {
            // Skip collider
            if (scr.physics.colliderType == RFColliderType.None)
                return;
            
            // Discard collider if just trigger
            if (scr.physics.meshCollider != null && scr.physics.meshCollider.isTrigger == true)
                scr.physics.meshCollider = null;

            // No collider. Add own // TODO set non convex shape for collider
            if (scr.physics.meshCollider == null)
            {
                // Mesh collider
                if (scr.physics.colliderType == RFColliderType.Mesh)
                {
                    // Add Mesh collider
                    MeshCollider mCol = scr.gameObject.AddComponent<MeshCollider>();
                    
                    // Set mesh
                    if (mesh != null)
                        mCol.sharedMesh = mesh;

                    // Set convex for dynamic types // TODO convex for kinematik
                    if (scr.simulationType == SimType.Dynamic ||
                        scr.simulationType == SimType.Inactive ||
                        scr.simulationType == SimType.Sleeping)
                        mCol.convex = true;
                    scr.physics.meshCollider = mCol;
                }
                    
                // Box collider
                else if (scr.physics.colliderType == RFColliderType.Box)
                {
                    BoxCollider mCol = scr.gameObject.AddComponent<BoxCollider>();
                    scr.physics.meshCollider = mCol;
                }
                        
                // Sphere collider
                else if (scr.physics.colliderType == RFColliderType.Sphere)
                {
                    SphereCollider mCol = scr.gameObject.AddComponent<SphereCollider>();
                    scr.physics.meshCollider = mCol;
                }
            }
        }
        
        // Create mesh colliders for every input mesh TODO input cluster to control all nest roots for correct colliders
        public static void SetClusterColliders (RayfireRigid scr, MeshFilter[] childMeshes)
        {
            //float t1 = Time.realtimeSinceStartup;
            
            // Colliders list
            scr.physics.clusterColliders = new List<Collider>();
            
            // Skip collider
            if (scr.physics.colliderType == RFColliderType.None)
            {
                scr.physics.clusterColliders = scr.gameObject.GetComponents<Collider>().ToList();
                return;
            }

            // Check children for mesh or cluster root until all children will not be checked
            Mesh tempMesh;
            for (int i = 0; i < childMeshes.Length; i++)
            {
                // Skip
                if (childMeshes[i].sharedMesh == null)
                    continue;

                // Offset mesh for collider
                List<Vector3> vertices = new List<Vector3>();
                childMeshes[i].sharedMesh.GetVertices (vertices);
                for (int v = 0; v < vertices.Count; v++)
                    vertices[v] = scr.transform.InverseTransformPoint (childMeshes[i].transform.TransformPoint (vertices[v]));
                
                // Set new mesh data
                tempMesh = new Mesh();
                tempMesh.name = childMeshes[i].sharedMesh.name;
                tempMesh.SetVertices (vertices);
                tempMesh.triangles = childMeshes[i].sharedMesh.triangles;

                // Set up new collider based on child mesh
                MeshCollider meshCol = scr.gameObject.AddComponent<MeshCollider>();
                meshCol.convex = true;
                meshCol.sharedMesh = tempMesh;

                // Collect colliders
                scr.physics.clusterColliders.Add (meshCol);
            }

            //Debug.Log (Time.realtimeSinceStartup - t1);
        }
        
        // Set collider material
        public static void SetColliderMaterial(RayfireRigid scr)
        {
            // Set physics material if not defined by user
            if (scr.physics.material == null)
                scr.physics.material = scr.physics.PhysMaterial;
            
            // Set mesh collider material
            if (scr.physics.meshCollider != null)
            {
                scr.physics.meshCollider.sharedMaterial = scr.physics.material;
                return;
            }
            
            // Set cluster colliders material
            if (scr.physics.clusterColliders != null)
                if (scr.physics.clusterColliders.Count > 0)
                    foreach (var col in scr.physics.clusterColliders)
                        col.sharedMaterial = scr.physics.material;
        }
        
        // Set collider convex state
        public static void SetColliderConvex(RayfireRigid scr)
        {
            if (scr.physics.meshCollider != null)
            {
                // Not Mesh collider
                if (scr.physics.meshCollider is MeshCollider == false)
                    return;
                
                // Turn on convex for non kinematik
                MeshCollider mCol = (MeshCollider)scr.physics.meshCollider;
                if (scr.physics.rigidBody.isKinematic == false)
                    mCol.convex = true;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Cache physics data for fragments 
        public IEnumerator PhysicsDataCor (RayfireRigid scr)
        {
            while (exclude == false)
            {
                velocity = scr.physics.rigidBody.velocity;
                // TODO angularVelocity = rigidBody.angularVelocity; rigidBody.GetPointVelocity () set rotation to fragments
                position = scr.transForm.position;
                rotation = scr.transForm.rotation;
                yield return null;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Get Destructible state
        public bool Destructible
        {
            get { return RayfireMan.inst.materialPresets.Destructible(materialType); }
        }

        // Get physic material
        public int Solidity
        {
            get { return RayfireMan.inst.materialPresets.Solidity(materialType); }
        }

        // Get physic material
        PhysicMaterial PhysMaterial
        {
            get
            {
                // Return predefine material
                if (material != null)
                    return material;

                // Crete new material
                return RFMaterialPresets.Material(materialType);
            }
        }
    }
}