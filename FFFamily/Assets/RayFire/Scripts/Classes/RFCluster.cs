using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace RayFire
{
    // Cluster class
    [Serializable]
    public class RFCluster : IComparable<RFCluster>
    {
        public int id;
        public Transform tm;
        public Transform rootParent;
        public int depth;
        [NonSerialized] public Vector3 pos;
        [NonSerialized] public Bounds bound;
        [NonSerialized] public List<RFShard> shards = new List<RFShard>();
        [NonSerialized] public List<RFCluster> childClusters = new List<RFCluster>();
        [NonSerialized] public List<RFCluster> neibClusters = new List<RFCluster>();
        [NonSerialized] public List<float> neibArea = new List<float>();
        [NonSerialized] List<float> neibPerc = new List<float>();

        // Compare by size
        public int CompareTo(RFCluster otherCluster)
        {
            float thisSize = bound.size.magnitude;
            float otherSize = otherCluster.bound.size.magnitude;
            if (thisSize > otherSize)
                return -1;
            if (thisSize < otherSize)
                return 1;
            return 0;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Method
        /// /////////////////////////////////////////////////////////
        
        // Get all shards ain all child clusters
        List<RFShard> GetNestedShards(bool OwnShards = false)
        {
            List<RFShard> nestedShards = new List<RFShard>();
            List<RFCluster> nestedClusters = new List<RFCluster>();
            nestedClusters.AddRange(childClusters);

            // Collect own shards
            if (OwnShards == true)
                nestedShards.AddRange(shards);

            while (nestedClusters.Count > 0)
            {
                nestedShards.AddRange(nestedClusters[0].shards);
                nestedClusters.AddRange(nestedClusters[0].childClusters);
                nestedClusters.RemoveAt(0);
            }
            return nestedShards;
        }

        // Get all shards ain all child clusters
        public List<RFCluster> GetNestedClusters()
        {
            List<RFCluster> nestedClusters = new List<RFCluster>();
            nestedClusters.AddRange(childClusters);

            List<RFCluster> checkClusters = new List<RFCluster>();
            checkClusters.AddRange(childClusters);

            while (checkClusters.Count > 0)
            {
                nestedClusters.AddRange(checkClusters[0].childClusters);
                checkClusters.RemoveAt(0);
            }

            return nestedClusters;
        }

        // Check if other cluster has shared face
        bool TrisNeib(RFCluster otherCluster)
        {
            // Check if cluster shards has 1 neib in other cluster shards
            foreach (RFShard shard in shards)
                for (int i = 0; i < shard.neibShards.Count; i++)
                    if (otherCluster.shards.Contains(shard.neibShards[i]) == true)
                        return true;

            List<RFShard> nestedShards = GetNestedShards();
            List<RFShard> otherNestedShards = otherCluster.GetNestedShards();

            foreach (RFShard shard in nestedShards)
                for (int i = 0; i < shard.neibShards.Count; i++)
                    if (otherNestedShards.Contains(shard.neibShards[i]) == true)
                        return true;

            //// Check if other cluster among neib clusters
            //if (neibClusters.Contains(otherCluster) == true)
            //    return true;

            //// Check if other cluster children clusters has
            //foreach (RFCluster cluster in childClusters)
            //    for (int i = 0; i < cluster.neibClusters.Count; i++)
            //        if (otherCluster.neibClusters.Contains(cluster.neibClusters[i]) == true)
            //            return true;

            return false;
        }

        // Get shared area with another cluster
        float NeibArea(RFCluster otherCluster)
        {
            float area = 0f;
            foreach (RFShard shard in shards)
                for (int i = 0; i < shard.neibShards.Count; i++)
                    if (otherCluster.shards.Contains(shard.neibShards[i]) == true)
                        area += shard.neibArea[i];


            List<RFShard> nestedShards = GetNestedShards();
            List<RFShard> otherNestedShards = otherCluster.GetNestedShards();

            foreach (RFShard shard in nestedShards)
                for (int i = 0; i < shard.neibShards.Count; i++)
                    if (otherNestedShards.Contains(shard.neibShards[i]) == true)
                        area += shard.neibArea[i];


            //// Check if other cluster children clusters has
            //foreach (RFCluster cluster in childClusters)
            //    for (int i = 0; i < cluster.neibClusters.Count; i++)
            //        if (otherCluster.neibClusters.Contains(cluster.neibClusters[i]) == true)
            //            area += cluster.neibArea[i];

            return area;
        }

        // Get neib index with biggest shared area
        public int GetNeibIndArea(List<RFCluster> clusterList = null)
        {
            // Get neib index with biggest shared area
            float biggestArea = 0f;
            int neibInd = 0;
            for (int i = 0; i < neibClusters.Count; i++)
            {
                // Skip if check neib shard not in filter list
                if (clusterList != null)
                    if (clusterList.Contains(neibClusters[i]) == false)
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

        // Find neib clusters amount cluster list and set them with neib area
        public static void SetClusterNeib(List<RFCluster> clusters, bool connectivity)
        {
            // Set list
            foreach (RFCluster cluster in clusters)
            {
                cluster.neibClusters = new List<RFCluster>();
                cluster.neibArea = new List<float>();
                cluster.neibPerc = new List<float>();
            }

            // Set neib and area info
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int s = 0; s < clusters.Count; s++)
                {
                    if (s != i)
                    {
                        // Check if shard was not added as neib before
                        if (clusters[s].neibClusters.Contains(clusters[i]) == false)
                        {
                            // Bounding box intersection check
                            if (clusters[i].bound.Intersects(clusters[s].bound) == true)
                            {
                                // No need in face check connectivity
                                if (connectivity == false)
                                {
                                    float size = clusters[i].bound.size.magnitude;

                                    clusters[i].neibClusters.Add(clusters[s]);
                                    clusters[i].neibArea.Add(size);

                                    clusters[s].neibClusters.Add(clusters[i]);
                                    clusters[s].neibArea.Add(size);
                                }

                                // Face to face connectivity check
                                else
                                {
                                    // Check for shared faces and collect neibs and areas
                                    if (clusters[i].TrisNeib(clusters[s]) == true)
                                    {
                                        float area = clusters[i].NeibArea(clusters[s]);

                                        clusters[i].neibClusters.Add(clusters[s]);
                                        clusters[i].neibArea.Add(area);

                                        clusters[s].neibClusters.Add(clusters[i]);
                                        clusters[s].neibArea.Add(area);
                                    }
                                }
                            }
                        }
                    }
                }

                // Set area ratio
                float maxArea = Mathf.Max(clusters[i].neibArea.ToArray());
                foreach (float area in clusters[i].neibArea)
                {
                    if (maxArea > 0)
                        clusters[i].neibPerc.Add(area / maxArea);
                    else
                        clusters[i].neibPerc.Add(0f);
                }
            }
        }

        // Get neib cluster from shardList which is neib to one of the shards
        public static RFCluster GetNeibClusterArea(List<RFCluster> clusters, List<RFCluster> clusterList)
        {
            // No clusters to pick
            if (clusterList.Count == 0)
                return null;

            // Get all neibs for clusters, exclude neibs not from clusterList
            List<RFCluster> allNeibs = new List<RFCluster>();

            // Biggest area
            float biggestArea = 0f;
            RFCluster biggestCluster = null;

            // Check cluster
            foreach (RFCluster cluster in clusters)
            {
                // Check neibs
                for (int i = 0; i < cluster.neibClusters.Count; i++)
                {
                    // Shared are is too small relative other neibs
                    if (cluster.neibPerc[i] < 0.5f)
                        continue;

                    // Neib cluster has shared area lower than already founded 
                    if (biggestArea >= cluster.neibArea[i])
                        continue;

                    // Neib already in neib list
                    if (allNeibs.Contains(cluster.neibClusters[i]) == true)
                        continue;

                    // Neib not among allowed clusters
                    if (clusterList.Contains(cluster.neibClusters[i]) == false)
                        continue;

                    // Remember neib
                    allNeibs.Add(cluster.neibClusters[i]);
                    biggestArea = cluster.neibArea[i];
                    biggestCluster = cluster.neibClusters[i];
                }
            }

            // Pick shard with biggest area
            return biggestCluster;
        }

        /// /////////////////////////////////////////////////////////
        /// Connectivity
        /// /////////////////////////////////////////////////////////

        // Connectivity check
        public static List<RFCluster> ConnectivityCheck (List<RFShard> shards)
        {
//            // New list for solo shards
//            List<RFShard> soloShards = new List<RFShard>();
//
//            // Collect solo shards with no neibs
//            for (int i = shards.Count - 1; i >= 0; i--)
//                if (shards[i].neibShards.Count == 0)
//                    soloShards.Add(shards[i]);
            
            // Check all shards and collect new clusters
            int shardsAmount = shards.Count;
            List<RFCluster> newClusters = new List<RFCluster>();
            while (shards.Count > 0)
            {
                // Lists
                List<RFShard> checkShards = new List<RFShard>();
                List<RFShard> newClusterShards = new List<RFShard>();

                // Start from first shard
                checkShards.Add(shards[0]);
                newClusterShards.Add(shards[0]);

                // Collect by neibs
                while (checkShards.Count > 0)
                {
                    // Add neibs to check
                    foreach (RFShard neibShard in checkShards[0].neibShards)
                    {
                        // If neib among current cluster shards
                        if (shards.Contains(neibShard) == true)
                        {
                            // And not already collected
                            if (newClusterShards.Contains(neibShard) == false)
                            {
                                checkShards.Add(neibShard);
                                newClusterShards.Add(neibShard);
                            }
                        }
                    }

                    // Remove checked
                    checkShards.RemoveAt(0);
                }

                // Child cluster connected
                if (shardsAmount == newClusterShards.Count)
                    break;
                
                // Child cluster not connected. Create new cluster and add to parent
                RFCluster newCluster = new RFCluster();
                newCluster.shards = newClusterShards;
                newClusters.Add(newCluster);

                // Remove from all shards list
                for (int i = shards.Count - 1; i >= 0; i--)
                    if (newClusterShards.Contains(shards[i]) == true)
                        shards.RemoveAt(i);
            }

            return newClusters;
        }

        // Unyielding Connectivity check
        public static void ConnectivityCheckUny (RFCluster cluster)
        {
            // Reset child clusters to store not connected
            cluster.childClusters = new List<RFCluster>();

            // NO shards
            if (cluster.shards.Count == 0)
                return;

            // Clear all activated/demolished shards
            for (int i = cluster.shards.Count - 1; i >= 0; i--)
            {
                if (cluster.shards[i].rigid == null || 
                    cluster.shards[i].rigid.activation.connect == null ||
                    cluster.shards[i].rigid.limitations.demolished == true)
                    cluster.shards.RemoveAt (i);
            }
            
            // Get all uny/regular shards
            List<RFShard> regShards = new List<RFShard>();
            List<RFShard> checkShards = new List<RFShard>();
            foreach (var shard in cluster.shards)
            {
                if (shard.rigid.activation.unyielding == true)
                    checkShards.Add (shard);
                else
                    regShards.Add (shard);
            }

            // Nothing to activate. Everything activated.
            if (regShards.Count == 0)
            {
                cluster.shards.Clear();
                return;
            }

            // Remove all uny shards from regular shards and left not connected
            while (checkShards.Count > 0)
            {
                foreach (RFShard neibShard in checkShards[0].neibShards)
                {
                    if (regShards.Contains(neibShard) == true)
                    {
                        regShards.Remove (neibShard);
                        checkShards.Add (neibShard);
                    }
                }
                checkShards.RemoveAt(0);
            }

            // Update input cluster
            foreach (var shard in regShards)
                cluster.shards.Remove (shard);

            // Nothing to activate
            if (regShards.Count == 0)
                return;
            
            // Set not connected shards as child cluster shards to be activated
            RFCluster newCluster = new RFCluster();
            newCluster.shards = regShards;
            cluster.childClusters.Add (newCluster);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collider
        /// /////////////////////////////////////////////////////////
        
        // Create base cluster with children as shards
        public static RFCluster SetCluster(Transform transform, ConnectivityType connectivity)
        {
            // Create Base cluster
            RFCluster cluster = new RFCluster();

            cluster.tm = transform;
            cluster.rootParent = null;
            cluster.depth = 0;
            cluster.pos = transform.position;

            // Set cluster id
            cluster.id = 0;

            // Set shards for main cluster
            cluster.shards = RFShard.GetShards(cluster.tm, connectivity);

            return cluster;
        }

        // Get all children bounds
        public static Bounds GetChildrenBound(Transform tm)
        {
            // Collect all children transforms
            List<Renderer> renderers = tm.GetComponentsInChildren<Renderer>().ToList();

            // Get list of bounds
            List<Bounds> bounds = new List<Bounds>();
            foreach (Renderer renderer in renderers)
                bounds.Add(renderer.bounds);

            return GetBoundsBound(bounds);
        }

        // Get bound by list of bounds
        public static Bounds GetBoundsBound(List<Bounds> bounds)
        {
            // new bound
            Bounds bound = new Bounds();

            // No mesh renderers
            if (bounds.Count == 0)
            {
                Debug.Log("GetBoundsBound error");
                return bound;
            }

            // Basic bounds min and max values
            float minX = bounds[0].min.x;
            float minY = bounds[0].min.y;
            float minZ = bounds[0].min.z;
            float maxX = bounds[0].max.x;
            float maxY = bounds[0].max.y;
            float maxZ = bounds[0].max.z;

            // Compare with other bounds
            if (bounds.Count > 1)
            {
                for (int i = 1; i < bounds.Count; i++)
                {
                    if (bounds[i].min.x < minX)
                        minX = bounds[i].min.x;
                    if (bounds[i].min.y < minY)
                        minY = bounds[i].min.y;
                    if (bounds[i].min.z < minZ)
                        minZ = bounds[i].min.z;

                    if (bounds[i].max.x > maxX)
                        maxX = bounds[i].max.x;
                    if (bounds[i].max.y > maxY)
                        maxY = bounds[i].max.y;
                    if (bounds[i].max.z > maxZ)
                        maxZ = bounds[i].max.z;
                }
            }

            // Get center
            bound.center = new Vector3((maxX - minX) / 2f, (maxY - minY) / 2f, (maxZ - minZ) / 2f);

            // Get min and max vectors
            bound.min = new Vector3(minX, minY, minZ);
            bound.max = new Vector3(maxX, maxY, maxZ);

            return bound;
        }
    }

    // Triangle
    [Serializable]
    public class RFTriangle
    {
        public int id;
        public float area;
        public Vector3 normal;
        public Vector3 pos;
        public List<int> verts;
        public List<int> neibs;

        // Constructor
        public RFTriangle(int Id, float Area, Vector3 Normal, Vector3 Pos, List<int> Verts)
        {
            id = Id;
            area = Area;
            normal = Normal;
            pos = Pos;
            verts = Verts;
            neibs = new List<int>();
        }

        // Set mesh triangles
        public static List<RFTriangle> SetTriangles(Transform tm, MeshFilter mf)
        {
            // Get mesh
            Mesh mesh = mf.sharedMesh;

            // Get all triangles by mesh
            List<RFTriangle> triangles = GetTriangles(mesh, tm);

            // Get all face by triangles
            // List<RFFace> faces = GetFaces(triangles);

            return triangles;
        }

        // Get all triangles by mesh
        public static List<RFTriangle> GetTriangles(Mesh mesh, Transform tm)
        {
            // List of all triangles
            List<RFTriangle> localTriangles = new List<RFTriangle>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                // Vertex indexes
                int i1 = mesh.triangles[i];
                int i2 = mesh.triangles[i + 1];
                int i3 = mesh.triangles[i + 2];

                // Get vertices position and area
                Vector3 v1 = tm.TransformPoint(mesh.vertices[i1]);
                Vector3 v2 = tm.TransformPoint(mesh.vertices[i2]);
                Vector3 v3 = tm.TransformPoint(mesh.vertices[i3]);
                Vector3 cross = Vector3.Cross(v1 - v2, v1 - v3);
                float area = cross.magnitude * 0.5f;

                // Set position
                Vector3 pos = (v1 + v2 + v3) / 3f;

                // Create triangle and collect it
                RFTriangle triangle = new RFTriangle(i / 3, area, mesh.normals[i1], pos, new List<int> { i1, i2, i3 });
                localTriangles.Add(triangle);
            }

            //Set triangle neibs
            //for (int i = 0; i < localTriangles.Count; i++)
            //{
            //    RFTriangle tri1 = localTriangles[i];
            //    for (int t = 0; t < localTriangles.Count; t++)
            //    {
            //        if (i != t && tri1.neibs.Count < 3)
            //        {
            //            Check tri for neib state

            //           RFTriangle tri2 = localTriangles[t];

            //            Tri1 and tri2 is not neibs already
            //            if (tri2.neibs.Contains(tri1.id) == false)
            //            {
            //                Amount of neib verts
            //                int vertsMatch = 0;

            //                Check for same verts
            //                foreach (int vertInd in tri1.verts)
            //                        if (tri2.verts.Contains(vertInd) == true)
            //                            vertsMatch++;

            //                Tri is neib
            //                if (vertsMatch > 1)
            //                {
            //                    tri1.neibs.Add(tri2.id);
            //                    tri2.neibs.Add(tri1.id);
            //                }
            //            }
            //        }
            //    }
            //}

            return localTriangles;
        }
    }

    // Face 
    [System.Serializable]
    public class RFFace
    {
        public int id;
        public float area;
        public Vector3 pos;
        public Vector3 normal;
        public List<int> tris;

        // Constructor
        public RFFace(int Id, float Area, Vector3 Normal)
        {
            id = Id;
            area = Area;
            normal = Normal;
            tris = new List<int>();
        }

        // Get all face in mesh by triangles. IMPORTANT turn on triangle neib calculation in RFTriangle
        List<RFFace> GetFaces(List<RFTriangle> Triangles)
        {
            List<int> checkedTris = new List<int>();
            List<RFFace> localFaces = new List<RFFace>();

            // Check every triangle
            int faceId = 0;
            foreach (RFTriangle tri in Triangles)
            {
                // Skip triangle if it is already part of face
                if (checkedTris.Contains(tri.id) == false)
                {
                    // Mark tri as checked
                    checkedTris.Add(tri.id);

                    // Create face
                    RFFace face = new RFFace(faceId, tri.area, tri.normal);
                    face.pos = tri.pos;
                    faceId++;
                    face.tris.Add(tri.id);

                    // List of all triangles to check
                    List<RFTriangle> trisToCheck = new List<RFTriangle>();
                    trisToCheck.Add(tri);

                    // Check all neibs
                    while (trisToCheck.Count > 0)
                    {
                        // Check neib tris
                        foreach (int neibId in trisToCheck[0].neibs)
                        {
                            if (checkedTris.Contains(neibId) == false)
                            {
                                // Get neib tri
                                RFTriangle neibTri = Triangles[neibId];

                                // Compare normals
                                if (tri.normal == neibTri.normal)
                                {
                                    face.area += neibTri.area;
                                    face.pos += neibTri.pos;
                                    face.tris.Add(neibId);
                                    checkedTris.Add(neibId);
                                    trisToCheck.Add(neibTri);
                                }
                            }
                        }
                        trisToCheck.RemoveAt(0);
                    }

                    // Set pos
                    face.pos /= face.tris.Count;

                    // Collect face
                    localFaces.Add(face);
                }
            }

            return localFaces;
        }
    }
}

