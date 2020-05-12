using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Debris")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-debris-component/")]
    public class RayfireDebris : MonoBehaviour
    {
        public RFDebris debris = new RFDebris();
       
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Awake
        void Awake()
        {
            
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }
        

    }
}