using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [System.Serializable]
    public class RFReferenceDemolition
    {
        [Header ("  Source")]
        [Space (1)]
        
        public GameObject reference;
        public List<GameObject> randomList;
        
        [Header ("  Properties")]
        [Space (1)]
        
        //public AlignType type;
        
        [Tooltip ("Add RayFire Rigid component to reference with mesh")]
        public bool addRigid;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFReferenceDemolition()
        {
            reference = null;
            randomList = new List<GameObject>();
            addRigid = true;
        }

        // Copy from
        public void CopyFrom (RFReferenceDemolition referenceDemolitionDml)
        {
            reference = referenceDemolitionDml.reference;
            randomList = referenceDemolitionDml.randomList;
            addRigid = referenceDemolitionDml.addRigid;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////   
        
        // Get reference
        public GameObject GetReference()
        {
            // Return single ref
            if (reference != null && randomList.Count == 0)
                return reference;

            // Get random ref
            List<GameObject> refs = new List<GameObject>();
            if (randomList.Count > 0)
            {
                foreach (var r in randomList)
                    if (r != null)
                        refs.Add (r);
                if (refs.Count > 0)
                    return refs[Random.Range (0, refs.Count)];
            }

            return null;
        }
        
        // Demolish object to reference
        public static bool DemolishReference (RayfireRigid scr)
        {
            if (scr.demolitionType == DemolitionType.ReferenceDemolition)
            {
                // Get instance
                GameObject referenceGo = scr.referenceDemolition.GetReference();
                
                // Has reference
                if (referenceGo != null)
                {
                    // Instantiate turned off reference 
                    bool refState = referenceGo.activeSelf;
                    referenceGo.SetActive (false);
                    GameObject fragRoot = scr.InstantiateGo (referenceGo);
                    referenceGo.SetActive (refState);
                    fragRoot.name = referenceGo.name;

                    // Set tm
                    scr.rootChild                  = fragRoot.transform;
                    scr.rootChild.position         = scr.transForm.position;
                    scr.rootChild.rotation         = scr.transForm.rotation;
                    scr.rootChild.transform.parent = RayfireMan.inst.transForm;

                    // Clear list for fragments
                    scr.fragments = new List<RayfireRigid>();
                    
                    // Check root for rigid props
                    RayfireRigid rootScr = fragRoot.gameObject.GetComponent<RayfireRigid>();

                    // Reference Root has not rigid. Add to
                    if (rootScr == null && scr.referenceDemolition.addRigid == true)
                    {
                        // Add rigid and copy
                        rootScr = fragRoot.gameObject.AddComponent<RayfireRigid>();
                        rootScr.initialization = RayfireRigid.InitType.AtStart;
                        
                        scr.CopyPropertiesTo (rootScr);

                        // Single mesh TODO improve
                        if (fragRoot.transform.childCount == 0)
                        {
                            rootScr.objectType = ObjectType.Mesh;
                        }

                        // Multiple meshes
                        if (fragRoot.transform.childCount > 0)
                        {
                            rootScr.objectType = ObjectType.MeshRoot;
                        }
                    }

                    // Activate and init rigid
                    scr.rootChild.gameObject.SetActive (true);

                    // Reference has rigid
                    if (rootScr != null)
                    {
                        // Create rigid for root children
                        if (rootScr.objectType == ObjectType.MeshRoot)
                        {
                            foreach (var frag in rootScr.fragments)
                                frag.limitations.currentDepth++;
                            scr.fragments.AddRange (rootScr.fragments);
                            scr.DestroyRigid (rootScr);
                        }

                        // Get ref rigid
                        else if (rootScr.objectType == ObjectType.Mesh ||
                                 rootScr.objectType == ObjectType.SkinnedMesh)
                        {
                            rootScr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                            RFDemolitionMesh.DemolishMesh(rootScr);
                            
                            // TODO COPY MESH DATA FROM ROOTSCR TO THIS TO REUSE
                            
                            scr.fragments.AddRange (rootScr.fragments);
                            RayfireMan.DestroyFragment (rootScr, rootScr.rootParent, 1f);
                        }

                        // Get ref rigid
                        else if (rootScr.objectType == ObjectType.NestedCluster ||
                                 rootScr.objectType == ObjectType.ConnectedCluster)
                        {
                            rootScr.Default();
                            rootScr.limitations.contactPoint = scr.limitations.contactPoint;
                            RFDemolitionCluster.DemolishCluster (rootScr);
                            rootScr.physics.exclude = true;
                            scr.fragments.AddRange (rootScr.fragments);
                            RayfireMan.DestroyFragment (rootScr, rootScr.rootParent, 1f);
                        }

                        // Has rigid by has No fragments. Stop demolition
                        if (scr.HasFragments == false)
                        {
                            scr.demolitionType = DemolitionType.None;
                            return false;
                        }
                    }
                }

                // Has no rigid, has No fragments, but demolished
                scr.limitations.demolished = true;
            }

            return true;
        }
    }
}