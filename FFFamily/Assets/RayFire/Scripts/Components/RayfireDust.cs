using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Dust")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-dust-component/")]
    public class RayfireDust : MonoBehaviour
    {
        public RFDust dust = new RFDust();
       
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