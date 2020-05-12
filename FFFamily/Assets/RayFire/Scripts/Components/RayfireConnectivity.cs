using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Fragments from demolition
// Scale support for bound + Unyielding component

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Connectivity")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-connectivity-component/")]
    public class RayfireConnectivity : MonoBehaviour
    {
        public enum ConnTargetType
        {
            Gizmo    = 0,
            Children = 1
        }
        
        [Space (2)]
        [Header ("Properties")]
        [Space (1)]
        public ConnTargetType source = ConnTargetType.Gizmo;
        [Space (1)]
        public ConnectivityType connectivityType = ConnectivityType.ByBoundingBox;

        // [Space (2)]
        // [Header ("Check")]
        // [HideInInspector] public bool onActivation = true;
        // [Space (1)]
        // [HideInInspector] public bool onDemolition = true;
                
        [Space (2)]
        [Header ("Connections")]
        [Space (1)]
        public bool showConnections = false;
        [Space(1)]
        [Range(0, 1f)] public float sphereSize = 1f;
        
        [Space (2)]
        [Header ("Gizmo")]
        [Space (1)]
        public bool showGizmo = true;
        public Vector3 size = new Vector3(1f,1f,1f);

        // Hidden
        [HideInInspector] public bool checkConnectivity;
        [HideInInspector] public bool checkNeed;
        
        // Private
        List<RayfireRigid> rigidList;
        [NonSerialized] 
        public RFCluster cluster;

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
                
        // Awake
        void Awake()
        {
            // Set by children.
            SetByChildren();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Set by gizmo. In start to detect kinematik non convex objects
            SetByGizmo();
            
            // Rigid check
            if (rigidList.Count == 0)
            {
                Debug.Log ("RayFire Connectivity: " + name + " has no objects to check for connectivity. Object Excluded from simulation.", gameObject);
                return;
            }
            
            // Connectivity check cor
            StartCoroutine(ConnectivityCor());
        }

        /// /////////////////////////////////////////////////////////
        /// Connectivity
        /// /////////////////////////////////////////////////////////

        // Gizmo tms
        void SetByGizmo()
        {
            if (source == ConnTargetType.Gizmo)
            {
                // Get box overlap fragments
                Collider[] colliders = Physics.OverlapBox (transform.position, size/2f, transform.rotation);
                
                // Get box cast transforms
                List<Transform> tmList = new List<Transform>();
                foreach (var col in colliders)
                    if (tmList.Contains (col.transform) == false)
                        tmList.Add (col.transform);
                
                SetConnectivity (tmList);
            }
        }

        // Children tms
        void SetByChildren()
        {
            if (source == ConnTargetType.Children)
            {
                List<Transform> tmList = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                    tmList.Add (transform.GetChild (i));

                SetConnectivity (tmList);
            }
        }
        
        // Set connectivity fragments and main node
        void SetConnectivity(List<Transform> tmList)
        {
            // No targets
            if (tmList.Count == 0)
                return;

            // Get rigid with byConnectivity
            rigidList = new List<RayfireRigid>();
            foreach (var tm in tmList)
            {
                RayfireRigid rigid = tm.GetComponent<RayfireRigid>();
                if (rigid != null)
                    if (rigid.simulationType == SimType.Inactive || rigid.simulationType == SimType.Kinematic)
                        if (rigid.activation.byConnectivity == true)
                            rigidList.Add (rigid);
            }
            
            // No targets
            if (rigidList.Count == 0)
                return;
            
            // Set this connectivity as main connectivity node
            foreach (var rigid in rigidList)
            {
                rigid.activation.connect = this;
            }

            // Create Base cluster
            SetCluster ();
        }
        
        // Set cluster
        void SetCluster ()
        {
            cluster = new RFCluster();

            // Set shards for main cluster
            cluster.shards = RFShard.GetShards(rigidList, connectivityType);
            
            // Set shard neibs
            RFShard.SetShardNeibs (cluster.shards, connectivityType);
        }

        // Get not connected groups
        void Check()
        {
            // Get not connected clusters
            RFCluster.ConnectivityCheckUny (cluster);
            
            // TODO turn of if no objs to activate
            
            //  clusters
        }

        // Connectivity check cor
        IEnumerator ConnectivityCor()
        {
            checkConnectivity = true;
            while (checkConnectivity == true)
            {
                if (checkNeed == true)
                {
                    // Reset
                    checkNeed = false;
                    
                    // Get not connected groups
                    CheckConnectivity();
                }
         
                yield return null;
            }
        }
        
        // Check for connectivity
        void CheckConnectivity()
        {
            // Get not connected groups
            Check();
            
            // Stop checking. Everything activated
            if (cluster.shards.Count == 0)
                checkConnectivity = false;

            // Activate not connected
            Activate();
        } 
        
        // Activate not connected
        void Activate()
        {
            // Nothing to activate
            if (cluster.childClusters.Count == 0)
                return;

            // Activate
            foreach (var shard in cluster.childClusters[0].shards)
                shard.rigid.Activate();
        }
        
    }
}