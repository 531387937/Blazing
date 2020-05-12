using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFDemolitionCluster
    {
        [Header ("  Properties")]
        [Space(1)]
        
        [Tooltip ("Set Runtime Demolition type for released fragments")]
        public bool meshDemolition;
        
        [Header ("  Connected Cluster")]
        [Space(1)]
        
        [Tooltip ("Defines Connectivity algorithm for clusters.")]
        public ConnectivityType connectivity;
        
        [Space(1)]
        [Tooltip ("Defines distance from contact point in percentage relative to object's size which will be detached at contact.")]
        [Range (1, 100)]
        public int contactRadius;

        // Hidden
        [HideInInspector] public RFCluster cluster;
        [HideInInspector] public float damageRadius;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFDemolitionCluster()
        {
            meshDemolition = false;
            connectivity   = ConnectivityType.ByBoundingBox;
            contactRadius  = 15;
            
            cluster        = null;
            
            Reset();
        }

        // Copy from
        public void CopyFrom (RFDemolitionCluster demolition)
        {
            meshDemolition = demolition.meshDemolition;
            connectivity   = demolition.connectivity;
            contactRadius  = demolition.contactRadius;

            Reset();
        }
        
        // Reset
        public void Reset()
        {
            damageRadius   = 0f;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Set fragments collider TODO check if already has colliders
        public static bool Clusterize (RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.NestedCluster)
                return ClusterizeNested (scr);
            if (scr.objectType == ObjectType.ConnectedCluster)
                return ClusterizeConnected (scr);
            return false;
        }
        
        // Create one cluster which includes all children meshes
        static bool ClusterizeNested (RayfireRigid scr)
        {
            // Get all nested children with meshes
            MeshFilter[] childMeshes = scr.gameObject.GetComponentsInChildren<MeshFilter>();

            // No meshes in children
            if (childMeshes.Length == 0)
                return false;

            // Create mesh colliders for every input mesh
            RFPhysic.SetClusterColliders (scr, childMeshes);

            return true;
        }
        
        // Create one cluster which includes only children meshes, not children of children meshes.
        static bool ClusterizeConnected (RayfireRigid scr)
        {
            // Setup cluster and shard if first time. Do not if copied from parent
            if (scr.clusterDemolition.cluster == null || scr.clusterDemolition.cluster.id == 0)
            {
                // Set cluster
                scr.clusterDemolition.cluster = RFCluster.SetCluster (scr.transForm, scr.clusterDemolition.connectivity);
                scr.clusterDemolition.cluster.id = 1;

                // Set shard neibs
                RFShard.SetShardNeibs (scr.clusterDemolition.cluster.shards, scr.clusterDemolition.connectivity);
            }
            
            // Get all children meshes
            List<MeshFilter> childMeshes = new List<MeshFilter>();
            for (int i = 0; i < scr.transForm.childCount; i++)
            {
                MeshFilter mf = scr.transForm.GetChild (i).GetComponent<MeshFilter>();
                if (mf != null)
                    childMeshes.Add (mf);
            }
            
            // No meshes in children
            if (childMeshes.Count == 0)
                return false;

            // float t1 = Time.realtimeSinceStartup;
            
            // Create mesh colliders for every input mesh and collect
            RFPhysic.SetClusterColliders (scr, childMeshes.ToArray());

            // TODO connectivity check to find solo shards and make sure they are not connected
            
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        // Demolish cluster to children nodes
        public static void DemolishCluster(RayfireRigid scr)
        {
            // Skip if not runtime
            if (scr.demolitionType != DemolitionType.Runtime)
                return;
            
            // TODO inherit original cluster velocity
            
            // Cluster demolition
            if (scr.objectType == ObjectType.NestedCluster)
                DemolishClusterNested (scr);
            else if (scr.objectType == ObjectType.ConnectedCluster)
                DemolishClusterConnected (scr);
        }

        // Demolish nested cluster
        static void DemolishClusterNested (RayfireRigid scr)
        {
            // Set demolished state. IMPORTANT should be here
            scr.limitations.demolished = true;
            scr.fragments = new List<RayfireRigid>();

             // Get list of all children to check
            List<Transform> childTms = new List<Transform>();
            for (int c = 0; c < scr.transform.childCount; c++)
                childTms.Add (scr.transform.GetChild (c));
            
            // Add rigid component to all children
            AddRigidComponent (scr, childTms);
                
            // Create parent for all fragments. TODO use existing cluster gameobject instead.
            GameObject root = new GameObject (scr.gameObject.name + "_root");
            root.transform.parent = RayfireMan.inst.transForm;
            scr.rootChild             = root.transform;
            scr.rootChild.position    = scr.transForm.position;
            scr.rootChild.rotation    = scr.transForm.rotation;

            // Turn off demolition for solo fragments
            if (scr.clusterDemolition.meshDemolition == false)
                foreach (var frag in scr.fragments)
                    if (frag.objectType == ObjectType.Mesh)
                        frag.demolitionType = DemolitionType.None;
                
            // Parent to root
            for (int i = 0; i < scr.fragments.Count; i++)
                scr.fragments[i].transform.parent = root.transform;
        }
        
        // Demolish connected cluster
        static void DemolishClusterConnected (RayfireRigid scr)
        {
            // TODO If demolition.chunkRadius == 100. Detach all.
            // TODO Infinite demolition to detached frags.

            // Get detach radius distance
            float detachRadius = scr.limitations.bboxSize / 100f * scr.clusterDemolition.contactRadius;

            // Damage radius. TODO reset it.
            if (scr.clusterDemolition.damageRadius > 0)
                detachRadius = scr.clusterDemolition.damageRadius;

            // Detached solo fragments
            List<RFShard> detachShards = new List<RFShard>();

            // Get all colliders TODO input mask by gameobject
            List<Collider> detachColliders = Physics.OverlapSphere (scr.limitations.contactPoint, detachRadius).ToList();

            // No colliders to detach
            if (detachColliders.Count == 0)
                return;

            // Detach contacted fragments to solo. IMPROVE force spread by shards connection info. Connectivity check
            for (int i = scr.physics.clusterColliders.Count - 1; i >= 0; i--)
            {
                if (detachColliders.Contains (scr.physics.clusterColliders[i]))
                {
                    // Detach shard ans im them solo
                    detachShards.Add (scr.clusterDemolition.cluster.shards[i]);
                    scr.clusterDemolition.cluster.shards.RemoveAt (i);

                    // Destroy colliders to keep original cluster in scene
                    scr.DestroyCollider (scr.physics.clusterColliders[i]);
                    scr.physics.clusterColliders.RemoveAt (i);
                }
            }

            // No detach shards
            if (detachShards.Count == 0)
                return;

            // Prepare
            scr.fragments = new List<RayfireRigid>();

            // Create parent for all fragments.
            GameObject root = new GameObject (scr.gameObject.name + "_root");
            root.transform.parent = RayfireMan.inst.transForm;
            scr.rootChild             = root.transform;
            scr.rootChild.position    = scr.transForm.position;
            scr.rootChild.rotation    = scr.transForm.rotation;

            // Get tm list for detached shards
            List<Transform> detachChildren = detachShards.Select (t => t.tm).ToList();

            // Add rigid component to detached children
            AddRigidComponent (scr, detachChildren);

            // Parent to main root
            for (int i = 0; i < detachChildren.Count; i++)
                detachChildren[i].parent = root.transform;

            // All shards were detached. Set demolished state
            if (detachChildren.Count == scr.clusterDemolition.cluster.shards.Count)
            {
                scr.limitations.demolished = true;
                return;
            }

            // Check cluster for connectivity and return list of not connected clusters
            scr.clusterDemolition.cluster.childClusters = RFCluster.ConnectivityCheck (scr.clusterDemolition.cluster.shards);

            // CLuster is demolished
            scr.limitations.demolished = true;
            
            // Main cluster connected. Create new cluster based on left shards. IMPORTANT. attempt to use old cluster with old RB cause sim instability
            if (scr.clusterDemolition.cluster.childClusters.Count == 0)
            {
                CreateClusterRuntime (scr, scr.clusterDemolition.cluster);
            }

            // Main cluster not connected. Create connected cluster for every cluster in list
            else
            {
                foreach (RFCluster cls in scr.clusterDemolition.cluster.childClusters)
                    CreateClusterRuntime (scr, cls);
            }

            // Turn off demolition for solo fragments
            if (scr.clusterDemolition.meshDemolition == false)
                foreach (var frag in scr.fragments)
                    if (frag.objectType == ObjectType.Mesh)
                        frag.demolitionType = DemolitionType.None;
        }
        
        // Create runtime clusters
        static void CreateClusterRuntime (RayfireRigid scr, RFCluster cls)
        {
            // Cluster with solo shard. Add rigid component, reparent
            if (cls.shards.Count == 1)
            {
                AddRigidComponent (scr, new List<Transform> (1) {cls.shards[0].tm});
                cls.shards[0].tm.parent = RayfireMan.inst.transForm;
                return;
            }

            // Create root for left children
            GameObject leftRoot = new GameObject();

            // Turn off
            leftRoot.SetActive (false);

            leftRoot.name               = scr.gameObject.name + "_cls";
            leftRoot.transform.position = scr.transForm.position;
            leftRoot.transform.rotation = scr.transForm.rotation;
            leftRoot.transform.parent   = RayfireMan.inst.transForm;

            // Parent to main root
            for (int s = 0; s < cls.shards.Count; s++)
                cls.shards[s].tm.parent = leftRoot.transform;

            // Add rigid to object
            RayfireRigid newScr = leftRoot.gameObject.AddComponent<RayfireRigid>();
            newScr.initialization = RayfireRigid.InitType.AtStart;
            
            // Collect fragment
            scr.fragments.Add (newScr);

            // Copy properties from parent to fragment node
            scr.CopyPropertiesTo (newScr);

            // Set to mesh 
            newScr.objectType = ObjectType.ConnectedCluster;
            newScr.physics.colliderType = RFColliderType.Mesh;

            // Set cluster
            newScr.clusterDemolition.cluster = cls;
            newScr.clusterDemolition.cluster.id = 2;

            // Turn on
            leftRoot.SetActive (true);
        }
        
        // Add rigid component to transform list
        static void AddRigidComponent (RayfireRigid scr, List<Transform> tmList)
        {
            for (int i = 0; i < tmList.Count; i++)
            {
                // Turn off
                tmList[i].gameObject.SetActive (false);

                // Check if object already has Rigid script
                RayfireRigid newScr = tmList[i].gameObject.AddComponent<RayfireRigid>();
                newScr.initialization = RayfireRigid.InitType.AtStart;
                
                // Skip excluded                                    ????????????
                if (newScr.physics.exclude == true)
                    continue;

                // Collect fragment
                scr.fragments.Add (newScr);

                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (newScr);

                // Set to mesh 
                newScr.objectType = ObjectType.Mesh;
                newScr.physics.colliderType = RFColliderType.Mesh;

                // Set as cluster if has children with meshes TODO check for mesh filter
                if (newScr.transform.childCount > 0)
                {
                    newScr.objectType = ObjectType.NestedCluster;
                }

                // Set to dynamic if solo TODO fix
                else
                {
                    newScr.simulationType = SimType.Dynamic;
                }

                // Update depth level and amount
                newScr.limitations.currentDepth = scr.limitations.currentDepth + 1;

                // Turn on
                tmList[i].gameObject.SetActive (true);

                // IMPORTANT. Set mesh collider convex for gun impact detection
                if (newScr.objectType == ObjectType.Mesh)
                    if (newScr.physics.meshCollider != null)
                        ((MeshCollider)newScr.physics.meshCollider).convex = true;
            }
        }
        
        // Create mesh colliders for every input mesh TODO input cluster to control all nest roots for correct colliders
        void SetMeshCollidersTest0 (RayfireRigid scr, MeshFilter[] childMeshes)
        {
            // Variables
            Mesh tempMesh;

            //float t1 = Time.realtimeSinceStartup;
            
            // Get existing colliders
            MeshCollider[] clsCols = scr.gameObject.GetComponents<MeshCollider>();
            if (clsCols.Length > 0)
            {
                foreach (var col in clsCols)
                    scr.physics.clusterColliders.Add (col);
                return;
            }

            // Check children for mesh or cluster root until all children will not be checked
            for (int i = 0; i < childMeshes.Length; i++)
            {
                // Skip
                if (childMeshes[i].sharedMesh == null)
                    continue;
                
                // Set up new collider based on child mesh TODO check if has
                MeshCollider childCol = childMeshes[i].gameObject.GetComponent<MeshCollider>();
                if (childCol == null)
                {
                    childCol = childMeshes[i].gameObject.AddComponent<MeshCollider>();
                    childCol.sharedMesh = childMeshes[i].sharedMesh;
                    childCol.convex = true;
                }
                
                // Disable
                childCol.enabled = false;
                
                // Offset mesh for collider
                List<Vector3> vertices = new List<Vector3>();
                childCol.sharedMesh.GetVertices (vertices);
                for (int v = 0; v < vertices.Count; v++)
                    vertices[v] = scr.transform.InverseTransformPoint (childMeshes[i].transform.TransformPoint (vertices[v]));
                
                // Set new mesh data
                tempMesh = new Mesh();
                tempMesh.SetVertices (vertices);
                tempMesh.triangles = childMeshes[i].sharedMesh.triangles;

                // Set up new collider based on child mesh
                MeshCollider meshCol = scr.gameObject.AddComponent<MeshCollider>();
                meshCol.convex     = true;
                meshCol.sharedMesh = tempMesh;
                
                // Collect colliders
                scr.physics.clusterColliders.Add (meshCol);
            }
            
            //Debug.Log (Time.realtimeSinceStartup - t1);
        }
        
        /*// Generate colliders
        public void GenerateColliders()
        {
            transForm = GetComponent<Transform>();
            physics.colliderType = RFColliderType.Mesh;
            RFDemolitionCluster.Clusterize (this);
            physics.colliderType = RFColliderType.None;
        }
        
        // Delete all colliders
        public void DeleteColliders()
        {
            Collider[] cols = gameObject.GetComponents<Collider>();
            for (int i = 0; i < cols.Length; i++)
                DestroyImmediate (cols[i]);
            physics.colliderType = RFColliderType.Mesh;
            clusterDemolition.cluster = null;
        }*/
    }
}