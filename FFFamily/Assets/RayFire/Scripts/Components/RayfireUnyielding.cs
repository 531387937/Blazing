using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Unyielding")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-unyielding-component/")]
    public class RayfireUnyielding : MonoBehaviour
    {

        [Header ("Gizmo")]
        public bool showGizmo = true;
        public Vector3 size = new Vector3(1f,1f,1f);
        List<RayfireRigid> rigidList;

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Start is called before the first frame update
        void Start()
        {
            // Set uny state
            SetUnyState();
        }

        /// /////////////////////////////////////////////////////////
        /// Collider
        /// /////////////////////////////////////////////////////////

        // Set uny state
        void SetUnyState()
        {
            // Get box overlap fragments
            Collider[] colliders = Physics.OverlapBox (transform.position, size/2f, transform.rotation);
            
            // Get box cast transforms
            List<Transform> tmList = new List<Transform>();
            foreach (var col in colliders)
                if (tmList.Contains (col.transform) == false)
                    tmList.Add (col.transform);

            // Get rigids
            rigidList = new List<RayfireRigid>();
            foreach (var tm in tmList)
            {
                RayfireRigid rigid = tm.GetComponent<RayfireRigid>();
                if (rigid != null)
                    rigidList.Add (rigid);
            }

            // No rigids
            if (rigidList.Count == 0)
            {
                Debug.Log ("RayFire Unyielding: " + name + " was not used", gameObject);
                return;
            }

            // Set this uny state
            foreach (var rigid in rigidList)
                rigid.activation.unyielding = true;
        }
    }
}