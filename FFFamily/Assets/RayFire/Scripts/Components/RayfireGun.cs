using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    // Gun script
    [AddComponentMenu("RayFire/Rayfire Gun")]
    [HelpURL("http://rayfirestudios.com/unity-online-help/unity-gun-component/")]
    public class RayfireGun : MonoBehaviour
    {
        // Hidden
        [HideInInspector] public bool showRay = true;
        [HideInInspector] public bool showHit = true;
        [HideInInspector] public bool shooting = false;

        [Header("  Properties")]
        [Space (2)]
        
        public AxisType axis = AxisType.XRed;
        [Range(0f, 100f)] public float maxDistance = 50f;
        public Transform target;

        [Header("  Burst")]
        [Space (2)]
        
        [Range(2, 20)] public int rounds = 1;
        [Range(0.01f, 5f)] public float rate = 0.3f;

        [Header("  Impact")]
        [Space (2)]
        
        [Range(0f, 5)] public float strength = 1f;
        [Range(0f, 3)] public float radius = 1f;

        [Header("  Damage")]
        [Space (2)]
        
        [Range(0, 100)] public float damage = 1f;

        [Header("  Impact particles")]
        [Space (2)]
        
        public bool debris = true;
        public bool dust = true;
        //[HideInInspector] public bool sparks = false;

        // [Header("  Decals")]
        //[HideInInspector] public bool decals = false;
        //[HideInInspector] public List<Material> decalsMaterial;

        [Header("  Impact flash")]
        [Space (2)]
        
        public bool enableImpactFlash = true;
        [Range(0.1f, 5f)] public float flashStrength = 0.6f;
        [Range(0.01f, 10f)] public float flashRange = 6f;
        [Range(0.01f, 2f)] public float flashDistance = 0.4f;
        public Color flashColor = new Color(1f, 1f, 0.8f);

        //[Header("Projectile")]
        //[HideInInspector] public bool projectile = false;
        
        [HideInInspector] public int mask = -1;
        [HideInInspector] public string tagFilter = "Untagged";

        // Event
        public RFShotEvent shotEvent = new RFShotEvent();


        // Impact Sparks
        //[Header("Shotgun")]
        //public int pellets = 1;
        //public int spread = 2;
        //public float recoilStr = 1f;
        //public float recoilFade = 1f;
        // Projectile: laser, bullet, pellets
        // Muzzle flash: position, color, str
        // Shell drop: position, direction, prefab, str, rotation
        // Impact decals
        // Impact blood
        // Ricochet

        //// Start is called before the first frame update
        //void Start()
        //{

        //    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        //    Debug.Log(mesh.vertices.Length);
        //    Debug.Log(mesh.triangles.Length);
        //    List<Vector3> vertChecked = new List<Vector3>();
        //    Vector3 norm = new Vector3(0f, 0f, -1f);

        //    for (int i = 0; i < mesh.vertices.Length; i++)
        //    {

        //        if (mesh.normals[i] == norm)
        //        {
        //            Debug.Log(mesh.triangles[i]);
        //            Debug.Log(mesh.vertices[i]);
        //        }                
        //    }
        //}


        /// /////////////////////////////////////////////////////////
        /// Single Shot
        /// /////////////////////////////////////////////////////////

        // Start shooting
        public void StartShooting()
        {
            if (shooting == false)
            {
                StartCoroutine(StartShootCor());
            }
        }

        // Start shooting
        IEnumerator StartShootCor()
        {
            // Vars
            int shootId = 0;
            shooting = true;

            while (shooting == true)
            {
                // Single shot
                Shoot(shootId);
                shootId++;

                yield return new WaitForSeconds(rate);
            }
        }

        // Stop shooting
        public void StopShooting()
        {
            shooting = false;
        }

        // Shoot over axis
        public void Shoot(int shootId = 1)
        {
            // Set vector
            Vector3 shootVector = ShootVector;

            // Consider burst recoil // TODO
            if (shootId > 1)
                shootVector = ShootVector;

            // Set position
            Vector3 shootPosition = transform.position;

            // Shoot
            Shoot(shootPosition, shootVector);
        }

        // Shoot over axis
        public void Shoot(Vector3 shootPosition, Vector3 shootVector)
        {
            // Set trigger state =
            QueryTriggerInteraction trigger = QueryTriggerInteraction.Ignore;

            // Get intersection collider
            RaycastHit hit;
            bool hitState = Physics.Raycast(shootPosition, shootVector, out hit, maxDistance, mask, trigger);

            // Pos and normal info
            Vector3 impactPoint = hit.point;
            Vector3 impactNormal = hit.normal;

            // No hits
            if (hitState == false)
                return;

            // Check for tag
            if (tagFilter != "Untagged" && tag != hit.transform.tag)
                return;

            // If mesh collider
            // int triId = hit.triangleIndex;
            // Vector3 bar = hit.barycentricCoordinate;

            // Create impact flash
            if (enableImpactFlash == true)
                ImpactFlash(hit.point, hit.normal);

            // Check for Rigid script
            RayfireRigid scrRigid = hit.transform.GetComponent<RayfireRigid>();

            // NO Rigid script. TODO optional add rigid script
            if (scrRigid == null)
                return;

            // Apply damage if enabled
            if (scrRigid.damage.enable == true)
            {
                // Check for demolition
                bool damageDemolition = scrRigid.ApplyDamage(damage, impactPoint, radius);

                // Target was demolished
                if (damageDemolition == true && scrRigid.HasFragments == true)
                {
                    // Get new fragment target
                    bool dmlHitState = Physics.Raycast(shootPosition, shootVector, out hit, maxDistance, mask, trigger);
                    
                    // Stop. No new target TODO proceed with debris, dust, event
                    if (dmlHitState == false)
                        return;
                }
            }

            // Hit data
            impactPoint = hit.point;
            impactNormal = hit.normal;
            // Rigidbody rb = hit.rigidbody;
            // Collider col = hit.collider;
            scrRigid = hit.transform.GetComponent<RayfireRigid>();

            // NO Rigid script.
            if (scrRigid == null)
                return;
            
            // Activation of kinematik/inactive
            List<RayfireRigid> rigidList = ActivationCheck(scrRigid, impactPoint, radius);

            // Impact hit
            ImpactHit(rigidList, impactPoint, shootVector, hit.normal);

            // Impact Debris
            ImpactDebris(scrRigid, impactPoint, impactNormal);

            // Impact Dust
            ImpactDust(scrRigid, impactPoint, impactNormal);
            
            // Event
            shotEvent.InvokeLocalEvent(this);
            RFShotEvent.InvokeGlobalEvent(this);
        }

        /// /////////////////////////////////////////////////////////
        /// Burst
        /// /////////////////////////////////////////////////////////

        // Shoot over axis
        public void Burst()
        {
            if (shooting == false)
                StartCoroutine(BurstCor());
        }

        // Burst shooting coroutine
        IEnumerator BurstCor()
        {
            shooting = true;
            for (int i = 0; i < rounds; i++)
            {
                // Stop shooting
                if (shooting == false)
                    break;

                // Single shot
                Shoot(i);

                yield return new WaitForSeconds(rate);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Vfx
        /// /////////////////////////////////////////////////////////

        // Create impact flash
        void ImpactFlash(Vector3 position, Vector3 normal)
        {
            // Get light position
            Vector3 lightPos = normal * flashDistance + position;

            // Create light object
            GameObject impactFlashGo = new GameObject("impactFlash");
            impactFlashGo.transform.position = lightPos;

            // Create light
            Light lightScr = impactFlashGo.AddComponent<Light>();
            lightScr.color = flashColor;
            lightScr.intensity = flashStrength;
            lightScr.range = flashRange;

            lightScr.shadows = LightShadows.Hard;

            // Destroy with delay
            Destroy(impactFlashGo, 0.2f);
        }

        // Impact Debris
        void ImpactDebris(RayfireRigid scrRigid, Vector3 impactPos, Vector3 impactNormal)
        {
            if (debris == true && scrRigid.scrDebris != null && scrRigid.scrDebris.debris.onImpact == true)
                RFDebris.CreateDebrisImpact(scrRigid, impactPos, impactNormal, (short)20);
        }

        // Impact Dust
        void ImpactDust(RayfireRigid scrRigid, Vector3 impactPos, Vector3 impactNormal)
        {
            if (dust == true && scrRigid.scrDust != null && scrRigid.scrDust.dust.onImpact == true)
                RFDust.CreateDustImpact(scrRigid, impactPos, impactNormal, (short)20);
        }

        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////

        // Activate all rigid scripts in radius range
        List<RayfireRigid> ActivationCheck(RayfireRigid scrTarget, Vector3 position, float radius)
        {
            // Get rigid list with target object
            List<RayfireRigid> rigidList = new List<RayfireRigid>();
            if (scrTarget != null)
                rigidList.Add (scrTarget);

            // Check fo radius activation
            if (radius > 0)
            {
                // Get all colliders
                Collider[] colliders = Physics.OverlapSphere(position, radius, mask);

                // Collect all rigid bodies in range
                foreach (Collider col in colliders)
                {
                    // Tag filter
                    if (tagFilter != "Untagged" && col.tag != tagFilter)
                        continue;

                    // Get attached rigid body
                    RayfireRigid scrRigid = col.gameObject.GetComponent<RayfireRigid>();

                    // TODO check for connected cluster

                    // Collect new Rigid bodies and rigid scripts
                    if (scrRigid != null && rigidList.Contains(scrRigid) == false)
                        rigidList.Add(scrRigid);
                }
            }

            // Activate Rigid
            foreach (RayfireRigid scrRigid in rigidList)
                ActivationCheck(scrRigid);

            return rigidList;
        }

        // Activation of kinematik/inactive
        void ActivationCheck(RayfireRigid scrRigid)
        {
            if (scrRigid.simulationType == SimType.Inactive || scrRigid.simulationType == SimType.Kinematic)
                if (scrRigid.activation.byImpact == true)
                    scrRigid.Activate();
        }

        /// /////////////////////////////////////////////////////////
        /// Impact
        /// /////////////////////////////////////////////////////////
        
        // Hit object at impact position
        void ImpactHit(List<RayfireRigid> rigidList, Vector3 impactPos, Vector3 shootVector, Vector3 impactNormal)
        {
            if (strength > 0)
            {
                foreach (RayfireRigid scr in rigidList)
                {
                    if (scr.physics.rigidBody != null && scr.physics.rigidBody.isKinematic == false)
                    {
                        // Get impact direction TODO average with normal
                        Vector3 impactDirection = shootVector;

                        // Get impact force
                        Vector3 impactForce = impactDirection * strength;

                        // Add force
                        //scrRigid.rigidBody.AddForceAtPosition(impactForce, impactPos, ForceMode.Impulse);
                        scr.physics.rigidBody.AddForceAtPosition(impactForce, impactPos, ForceMode.VelocityChange);
                    }
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////

        // Get shooting ray
        public Vector3 ShootVector
        {
            get
            {
                // Vector to target if defined
                if (target != null)
                {
                    Vector3 targetRay = target.position - transform.position;
                    return targetRay.normalized;
                }

                // Vectors by axis
                if (axis == AxisType.XRed)
                    return transform.right;
                if (axis == AxisType.YGreen)
                    return transform.up;
                if (axis == AxisType.ZBlue)
                    return transform.forward;
                return transform.up;
            }
        }

    }
}
