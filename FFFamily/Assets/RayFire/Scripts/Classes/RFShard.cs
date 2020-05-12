using System.Collections.Generic;
using UnityEngine;
using System;

namespace RayFire
{
    // Cluster child class
    [Serializable]
    public class RFShard : IComparable<RFShard>
    {
        public int id;
        public Transform tm = null;
        
        [HideInInspector] public Bounds bound;
        [HideInInspector] public float dist = -1f;
        [HideInInspector] public List<float> neibArea;
        [HideInInspector] public List<float> neibPerc;
        [NonSerialized] public RFCluster cluster;
        [NonSerialized] public List<RFShard> neibShards;
        [NonSerialized] List<RFTriangle> tris;
        [NonSerialized] public RayfireRigid rigid;

        // Constructor
        public RFShard(Transform Tm, int Id)
        {
            tm = Tm;
            id = Id;

            // Set bounds
            Renderer mr = Tm.GetComponent<Renderer>();
            if (mr != null)
                bound = mr.bounds;

            // TODO get bounds in other way


            // TODO add property to expand bounds
            // bound.Expand(1f);
        }

        // Compare by size
        public int CompareTo(RFShard otherShard)
        {
            float thisSize = bound.size.magnitude;
            float otherSize = otherShard.bound.size.magnitude;
            if (thisSize > otherSize)
                return -1;
            if (thisSize < otherSize)
                return 1;
            return 0;
        }

        /// /////////////////////////////////////////////////////////
        /// Shards
        /// /////////////////////////////////////////////////////////
                
        // Prepare shards. Set bounds, set neibs
        public static List<RFShard> GetShards(Transform tm, ConnectivityType connectivity)
        {
            // Get all children tms
            List<Transform> tmList = new List<Transform>();
            for (int i = 0; i < tm.childCount; i++)
                tmList.Add (tm.GetChild(i));
            
            return GetShards (tmList, connectivity);
        }
        
        // Prepare shards. Set bounds, set neibs
        public static List<RFShard> GetShards(List<RayfireRigid> rigidList, ConnectivityType connectivity)
        {
            List<RFShard> shardList = new List<RFShard>();
            for (int i = 0; i < rigidList.Count; i++)
            {
                // Get mesh filter
                MeshFilter mf = rigidList[i].GetComponent<MeshFilter>();

                // Child has no mesh
                if (mf == null)
                    continue;

                // Create new shard
                RFShard shard = new RFShard(rigidList[i].transform, i);
                shard.rigid = rigidList[i];
                
                // Set faces data for connectivity
                if (connectivity == ConnectivityType.ByMesh)
                    shard.tris = RFTriangle.SetTriangles(shard.tm, mf);

                // Collect shard
                shardList.Add(shard);
            }
            return shardList;
        }
        
        // Prepare shards. Set bounds, set neibs
        static List<RFShard> GetShards(List<Transform> tmList, ConnectivityType connectivity)
        {
            List<RFShard> shardList = new List<RFShard>();
            for (int i = 0; i < tmList.Count; i++)
            {
                // Get mesh filter
                MeshFilter mf = tmList[i].GetComponent<MeshFilter>();

                // Child has no mesh
                if (mf == null)
                    continue;

                // Create new shard
                RFShard shard = new RFShard(tmList[i], i);

                // Set faces data for connectivity
                if (connectivity == ConnectivityType.ByMesh)
                    shard.tris = RFTriangle.SetTriangles(shard.tm, mf);

                // Collect shard
                shardList.Add(shard);
            }
            return shardList;
        }

        /// /////////////////////////////////////////////////////////
        /// Neibs
        /// /////////////////////////////////////////////////////////
        
        // Check if other shard has shared face
        bool TrisNeib(RFShard otherShard)
        {
            foreach (RFTriangle tri in tris)
                foreach (RFTriangle otherTri in otherShard.tris)
                {
                    float areaDif = Mathf.Abs(tri.area - otherTri.area);
                    if (areaDif < 0.001f)
                    {
                        float posDif = Vector3.Distance(tri.pos, otherTri.pos);
                        if (posDif < 0.02f)
                            return true;
                    }
                }
            return false;
        }

        // Get shared area with another shard
        float NeibArea(RFShard otherShard)
        {
            float area = 0f;
            foreach (RFTriangle tri in tris)
                foreach (RFTriangle otherTri in otherShard.tris)
                {
                    float areaDif = Mathf.Abs(tri.area - otherTri.area);
                    if (areaDif < 0.001f)
                    {
                        float posDif = Vector3.Distance(tri.pos, otherTri.pos);
                        if (posDif < 0.02f)
                        {
                            area += tri.area;
                            continue;
                        }
                    }
                }
            return area;
        }

        // Get neib index with biggest shared area
        public int GetNeibIndArea(List<RFShard> shardList = null)
        {
            // Get neib index with biggest shared area
            float biggestArea = 0f;
            int neibInd = 0;
            for (int i = 0; i < neibShards.Count; i++)
            {
                // Skip if check neib shard not in filter list
                if (shardList != null)
                    if (shardList.Contains(neibShards[i]) == false)
                        continue;

                // Remember if bigger
                if (neibArea[i] > biggestArea)
                {
                    biggestArea = neibArea[i];
                    neibInd = i;
                }
            }

            // Return index of neib with biggest shared area
            if (biggestArea > 0)
                return neibInd;

            // No neib
            return -1;
        }
        
        // Set shard neibs
        public static void SetShardNeibs(List<RFShard> shards, ConnectivityType connectivity)
        {
            // Set list
            foreach (RFShard shard in shards)
            {
                shard.neibShards = new List<RFShard>();
                shard.neibArea = new List<float>();
                shard.neibPerc = new List<float>();
            }

            // Set neib and area info
            for (int i = 0; i < shards.Count; i++)
            {
                for (int s = 0; s < shards.Count; s++)
                {
                    if (s != i)
                    {
                        // Check if shard was not added as neib before
                        if (shards[s].neibShards.Contains(shards[i]) == false)
                        {
                            // Bounding box intersection check
                            if (shards[i].bound.Intersects(shards[s].bound) == true)
                            {
                                // No need in face check connectivity
                                if (connectivity == ConnectivityType.ByBoundingBox)
                                {
                                    float size = shards[i].bound.size.magnitude;

                                    shards[i].neibShards.Add(shards[s]);
                                    shards[i].neibArea.Add(size);


                                    shards[s].neibShards.Add(shards[i]);
                                    shards[s].neibArea.Add(size);
                                }

                                // Face to face connectivity check
                                else
                                {
                                    // Check for shared faces and collect neibs and areas
                                    if (shards[i].TrisNeib(shards[s]) == true)
                                    {
                                        float area = shards[i].NeibArea(shards[s]);

                                        shards[i].neibShards.Add(shards[s]);
                                        shards[i].neibArea.Add(area);

                                        shards[s].neibShards.Add(shards[i]);
                                        shards[s].neibArea.Add(area);
                                    }
                                }
                            }
                        }
                    }
                }


                float maxArea = Mathf.Max(shards[i].neibArea.ToArray());
                foreach (float area in shards[i].neibArea)
                {
                    if (maxArea > 0)
                        shards[i].neibPerc.Add(area / maxArea);
                    else
                        shards[i].neibPerc.Add(0f);
                }

            }
        }

        // Get neib shard from shardList which is neib to one of the shards
        public static RFShard GetNeibShardArea(List<RFShard> shardGroup, List<RFShard> shardList)
        {
            // No shards to pick
            if (shardList.Count == 0)
                return null;

            // Get all neibs for shards, exclude neibs not from shardList
            List<RFShard> allNeibs = new List<RFShard>();

            // Biggest area
            float biggestArea = 0f;
            RFShard biggestShard = null;

            // Check shard
            foreach (RFShard shard in shardGroup)
            {
                // Check neibs
                for (int i = 0; i < shard.neibShards.Count; i++)
                {
                    // Shared are is too small relative other neibs
                    if (shard.neibPerc[i] < 0.6f)
                        continue;

                    // Neib shard has shared area lower than already founded 
                    if (biggestArea >= shard.neibArea[i])
                        continue;

                    // Neib already in neib list
                    if (allNeibs.Contains(shard.neibShards[i]) == true)
                        continue;

                    // Neib not among allowed shards
                    if (shardList.Contains(shard.neibShards[i]) == false)
                        continue;

                    // Remember neib
                    allNeibs.Add(shard.neibShards[i]);
                    biggestArea = shard.neibArea[i];
                    biggestShard = shard.neibShards[i];
                }
            }
            allNeibs = null;

            // Pick shard with biggest area
            return biggestShard;
        }
    }
}

