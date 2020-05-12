using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu("RayFire/Rayfire Shatter")]
    [HelpURL("http://rayfirestudios.com/unity-online-help/unity-shatter-component/")]
    public class RayfireShatter : MonoBehaviour
    {
        [Header ("  Fragments")]
        [Space (2)]
        
        public FragType    type = FragType.Voronoi;
        [Space (2)]
        
        public RFVoronoi   voronoi   = new RFVoronoi();
        [Space (2)]
        public RFSplinters splinters = new RFSplinters();
        [Space (2)]
        public RFSplinters slabs     = new RFSplinters();
        [Space (2)]
        public RFRadial    radial    = new RFRadial();
        [Space (2)]
        public RFCustom    custom = new RFCustom();
        [Space (2)]
        public RFSlice     slice = new RFSlice();
        [Space (2)]
        public RFTets      tets  = new RFTets();

        [Header ("  Properties")]
        [Space (2)]
        
		[Tooltip ("Editor: Allows to fragment complex multi element hi poly meshes with topology issues like open edges and unwelded vertices.")]
		public FragmentMode mode = FragmentMode.Editor;
		[Space (2)]
        
        public RFSurface material = new RFSurface();
        public RFShatterCluster clusters = new RFShatterCluster();
        public RFShatterAdvanced advanced = new RFShatterAdvanced();
        
        [Header("Center")]
        [HideInInspector] public bool showCenter;
        [HideInInspector] public Vector3    centerPosition;
        [HideInInspector] public Quaternion centerDirection;

        [Header("Components")]
        [HideInInspector] public Transform           transForm;
        [HideInInspector] public MeshFilter          meshFilter;
        [HideInInspector] public MeshRenderer        meshRenderer;
        [HideInInspector] public SkinnedMeshRenderer skinnedMeshRend;

        [Header("Variables")]
        [HideInInspector] public Mesh[]             meshes           = null;
        [HideInInspector] public Vector3[]          pivots           = null;
        [HideInInspector] public List<Transform>    rootChildList    = new List<Transform>();
        [HideInInspector] public List<GameObject>   fragmentsAll     = new List<GameObject>();
        [HideInInspector] public List<GameObject>   fragmentsLast    = new List<GameObject>();
        [HideInInspector] public List<RFDictionary> origSubMeshIdsRF = new List<RFDictionary>();

        // Hidden
        [HideInInspector] public int   shatterMode  = 1;
        [HideInInspector] public bool  colorPreview = false;
        [HideInInspector] public bool  scalePreview = true;
        [HideInInspector] public float previewScale = 0f;
        [HideInInspector] public float size = 0f;
        [HideInInspector] public float rescaleFix = 1f;
        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public Bounds bound;
        
        static float minSize = 0.01f;
        
        // Preview variables
        [HideInInspector] public bool resetState = false;
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Reset
        private void Reset()
        {
            ResetCenter();
        }

        // Set default vars before fragment
        void SetVariables()
        {
            size          = 0f;
            rescaleFix    = 1f;
            originalScale = transForm.localScale;
        }
        
        // Cache variables
        bool DefineComponents()
        {
            // Check if prefab
            if (gameObject.scene.rootCount == 0)
            {
                Debug.Log ("Shatter component unable to fragment prefab because prefab unable to store Unity mesh. Fragment prefab in scene.");
                return false;
            }
            
            // Mesh storage 
            meshFilter = GetComponent<MeshFilter>();
            skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();

            // 
            if (meshFilter == null && skinnedMeshRend == null)
            {
              Debug.Log ("No mesh"); 
              return false;
            }
            
            if (meshFilter != null && meshFilter.sharedMesh == null)
            {
              Debug.Log ("No mesh");  
              return false;
            }
              
            if (skinnedMeshRend != null && skinnedMeshRend.sharedMesh == null)
            {
              Debug.Log ("No mesh"); 
              return false;
            }

            // Not readable mesh TODO add for Rigid
            if (meshFilter != null && meshFilter.sharedMesh.isReadable == false)
            {
                Debug.Log ("Mesh not readable. Turn on"); 
                return false;
            }
            
            // Get components
            transForm        = GetComponent<Transform>();
            origSubMeshIdsRF = new List<RFDictionary>();
            
            // Mesh renderer
            if (skinnedMeshRend == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                bound = meshRenderer.bounds;
            }
            
            // Skinned mesh
            if (skinnedMeshRend != null)
                bound = skinnedMeshRend.bounds;
            
            return true;
        }

        // Get bounds
        public Bounds GetBound()
        {
            // Mesh renderer
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    return meshRenderer.bounds;
            }
            else
                return meshRenderer.bounds;
            
            // Skinned mesh
            if (skinnedMeshRend == null)
            {
                skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRend != null)
                    return skinnedMeshRend.bounds;
            }

            return new Bounds();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Fragment this object by shatter properties
        public void Fragment()
        {
            // Cache variables
            if (DefineComponents() == false)
                return;
            
            // Cache default vars
            SetVariables();
            
            // Check if object is too small
            ScaleCheck();
            
            // Cache
            RFFragment.CacheMeshes(ref meshes, ref pivots, ref origSubMeshIdsRF, this);

            // Stop
            if (meshes == null)
                return;
            
            // Create fragments
            fragmentsLast = CreateFragments();

            // Collect to all fragments
            fragmentsAll.AddRange(fragmentsLast);
            
            // Reset original object back if it was scaled
            transForm.localScale = originalScale;
        }
        
        // Create fragments by mesh and pivots array
        private List<GameObject> CreateFragments()
        {
            // No mesh were cached
            if (meshes == null)
                return null;

            // Clear array for new fragments
            GameObject[] fragArray = new GameObject[meshes.Length];

            // Vars 
            string goName = gameObject.name;
            string baseName = goName + "_sh_";
            
            // Create root object
            GameObject root = new GameObject(goName + "_root");
            root.transform.position = transForm.position;
            root.transform.rotation = transForm.rotation;
            root.transform.parent = transForm.parent;
            rootChildList.Add(root.transform);

            // Create instance for fragments
            GameObject fragInstance;
            if (advanced.copyComponents == true)
            {
                fragInstance = Instantiate(gameObject);
                fragInstance.transform.rotation = Quaternion.identity;
                fragInstance.transform.localScale = Vector3.one;

                // Destroy shatter
                DestroyImmediate(fragInstance.GetComponent<RayfireShatter>());
            }
            else
            {
                fragInstance = new GameObject();
                fragInstance.AddComponent<MeshFilter>();
                fragInstance.AddComponent<MeshRenderer>();
            }
            
            // Get original mats
            Material[] mats = skinnedMeshRend != null 
                ? skinnedMeshRend.sharedMaterials 
                : meshRenderer.sharedMaterials;
            
            // Create fragment objects
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Rescale mesh
                if (rescaleFix != 1f)
                    RFFragment.RescaleMesh (meshes[i], rescaleFix);

                // Instantiate. IMPORTANT do not parent when Instantiate
                GameObject fragGo = Instantiate(fragInstance);
                fragGo.transform.localScale = Vector3.one;
                
                // Set multymaterial
                MeshRenderer targetRend = fragGo.GetComponent<MeshRenderer>();
                RFSurface.SetMaterial(origSubMeshIdsRF, mats, material, targetRend, i, meshes.Length);
                
                // Set fragment object name and tm
                fragGo.name = baseName + (i + 1);
                fragGo.transform.position = transForm.position + (pivots[i] / rescaleFix);
                fragGo.transform.parent = root.transform;
                
                // Set fragment mesh
                MeshFilter mf = fragGo.GetComponent<MeshFilter>();
                mf.sharedMesh = meshes[i];
                mf.sharedMesh.name = fragGo.name;

                // Set mesh collider
                MeshCollider mc = fragGo.GetComponent<MeshCollider>();
                if (mc != null)
                    mc.sharedMesh = meshes[i];

                // Add in array
                fragArray[i] = fragGo;
            }

            // Destroy instance
            DestroyImmediate(fragInstance);

            // Empty lists
            meshes = null;
            pivots = null;
            origSubMeshIdsRF = new List<RFDictionary>();

            return fragArray.ToList();
        }

        /// /////////////////////////////////////////////////////////
        /// Deleting
        /// /////////////////////////////////////////////////////////

        // Delete fragments from last Fragment method
        public void DeleteFragmentsLast()
        {
            // Clean fragments list pre
            fragmentsLast.Clear();
            for (int i = fragmentsAll.Count - 1; i >= 0; i--)
                if (fragmentsAll[i] == null)
                    fragmentsAll.RemoveAt (i);
            
            // Check for all roots
            for (int i = rootChildList.Count - 1; i >= 0; i--)
                if (rootChildList[i] == null)
                    rootChildList.RemoveAt (i);
            
            // No roots
            if (rootChildList.Count == 0)
                return;  
            
            // Destroy root with fragments
            DestroyImmediate(rootChildList[rootChildList.Count - 1].gameObject);

            // Remove from list
            rootChildList.RemoveAt(rootChildList.Count - 1);
            
            // Clean all fragments list post
            for (int i = fragmentsAll.Count - 1; i >= 0; i--)
                if (fragmentsAll[i] == null)
                    fragmentsAll.RemoveAt (i);
        }

        // Delete all fragments and roots
        public void DeleteFragmentsAll()
        {
            // Clear lists
            fragmentsLast.Clear();
            fragmentsAll.Clear();
            
            // Check for all roots
            for (int i = rootChildList.Count - 1; i >= 0; i--)
                if (rootChildList[i] != null)
                    DestroyImmediate(rootChildList[i].gameObject);
            rootChildList.Clear();
        }

        // Reset center helper
        public void ResetCenter()
        {
            centerPosition = Vector3.zero;
            centerDirection = Quaternion.identity;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Scale
        /// /////////////////////////////////////////////////////////
        
        // Check if object is too small
        void ScaleCheck()
        {
            // Ge size from renderers
            if (meshRenderer != null)
                size = meshRenderer.bounds.size.magnitude;
            if (skinnedMeshRend != null)
                size = skinnedMeshRend.bounds.size.magnitude;
            
            // Get rescaleFix if too small
            if (size != 0f && size < minSize)
            {
                // Get rescaleFix factor
                rescaleFix = 1f / size;
                
                // Scale small object up to shatter
                Vector3 newScale = transForm.localScale * rescaleFix;
                transForm.localScale = newScale;
                
                // Warning
                Debug.Log ("Warning. Object " + name + " is too small.");
            }
        }
        
        // Reset original object and fragments scale
        public void ResetScale (float scaleValue)
        {
            // Reset scale
            if (resetState == true && scaleValue == 0f)
            {
                if (skinnedMeshRend != null)
                    skinnedMeshRend.enabled = true;

                if (meshRenderer != null)
                    meshRenderer.enabled = true;

                if (fragmentsLast.Count > 0)
                    foreach (GameObject fragment in fragmentsLast)
                        if (fragment != null)
                            fragment.transform.localScale = Vector3.one;

                resetState = false;
            }
        }
    }
}