using System.Collections.Generic;
using UnityEngine;

// Rayfire classes
namespace RayFire
{
    // Debris class
    [System.Serializable]
    public class RFDust : RFParticles
    {
        [Header("Dust materials")]
        [Range(0.01f, 1f)] public float opacity = 1f;
        public List<Material> dustMaterials = new List<Material>();

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDust()
        {
            particleType = ParticleType.Dust;
            particleSystem = null;
            tmDemolition = null;
            onDemolition = false;
            onActivation = false;
            onImpact = false;
            burstType = BurstType.TotalAmount;
            burstAmount = 30;
            distanceRate = 3f;
            sizeMin = 2f;
            sizeMax = 7f;
            lifeMin = 3f;
            lifeMax = 7f;
            rotationSpeed = 0f;
            collisionRadiusScale = 1f;
            percentage = 50;
            sizeThreshold = 0.3f;
            castShadows = true;
            receiveShadows = true;
            opacity = 0.25f;
            dustMaterials = new List<Material>();
        }

        // Copy from
        public void CopyFrom(RFDust dust)
        {
            particleType = ParticleType.Dust;
            onDemolition = dust.onDemolition;
            onActivation = dust.onActivation;
            onImpact = dust.onImpact;
            burstType = dust.burstType;
            burstAmount = dust.burstAmount;
            distanceRate = dust.distanceRate;
            sizeMin = dust.sizeMin;
            sizeMax = dust.sizeMax;
            lifeMin = dust.lifeMin;
            lifeMax = dust.lifeMax;
            rotationSpeed = dust.rotationSpeed;
            collisionRadiusScale = dust.collisionRadiusScale;
            sizeThreshold = dust.sizeThreshold;
            castShadows = dust.castShadows;
            receiveShadows = dust.receiveShadows;

            opacity = dust.opacity;
            dustMaterials = dust.dustMaterials;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 

        // Create dust particle system
        public static void CreateDustRigidList(RayfireRigid scr, List<RayfireRigid> hosts)
        {
            // Check for positive amount
            if (AmountCheck(scr, ParticleType.Dust) == false)
                return;

            // Dust hosts
            List<RayfireRigid> particlesHosts = GetParticlesHosts(hosts, scr.scrDust.dust.sizeThreshold, scr.scrDust.dust.percentage);

            // Get amount list
            List<int> burstAmountList = GetBurstAmountList(particlesHosts, scr.scrDust.dust.burstType, scr.scrDust.dust.burstAmount);

            // Decrease burst amount by depth fade
            SetBurstAmount(scr, hosts, ParticleType.Dust);
            
            // Create particle systems
            int seed = 0;
            for (int i = 0; i < particlesHosts.Count; i++)
            {
                // Set random seed
                Random.InitState(seed++);

                // Create single dust particle system
                CreateDustRigid(scr, particlesHosts[i], burstAmountList[i]);
            }
        }

        // Create single dust particle system
        public static void CreateDustRigid(RayfireRigid scr, RayfireRigid host, int amount)
        {
            float speedMin = 0f;
            float speedMax = 0.5f;
            float gravity = Physics.gravity.magnitude / 9.81f;
            float gravityMax = gravity * 0.7f;
            float gravityMin = gravity * -0.08f;
            
            // Particle system
            ParticleSystem particleSystem = CreateParticleSystem(host, scr.scrDust.dust.lifeMax, ParticleType.Dust);

            // Set main module TODO SET SPEED
            SetMain(particleSystem.main, scr.scrDust.dust.lifeMin, scr.scrDust.dust.lifeMax, scr.scrDust.dust.sizeMin, scr.scrDust.dust.sizeMax, gravityMin, gravityMax, speedMin, speedMax);

            // Emission over distance
            SetEmission(particleSystem.emission, scr.scrDust.dust.distanceRate, (short)amount);

            // Emission from mesh
            SetShapeMesh(particleSystem.shape, scr, host.meshFilter.sharedMesh);

            // Collision
            SetCollision(particleSystem.collision, scr.scrDust.dust.collisionRadiusScale, ParticleType.Dust);

            // Color over life time
            SetColorOverLife(particleSystem.colorOverLifetime, scr.scrDust.dust.opacity);

            // Noise
            SetNoise(particleSystem.noise);

            // Renderer
            SetParticleRendererDust(particleSystem.GetComponent<ParticleSystemRenderer>(), scr, scr.scrDust.dust.dustMaterials, scr.scrDust.dust.castShadows, scr.scrDust.dust.receiveShadows);

            // Start playing
            particleSystem.Play();
        }

        // Create single dust particle system
        public static void CreateDustImpact(RayfireRigid scr, Vector3 impactPos, Vector3 impactNormal, short amount)
        {
            float speedMin = 0f;
            float speedMax = 0.5f;
            float gravity = Physics.gravity.magnitude / 9.81f;
            float gravityMax = gravity * 0.7f;
            float gravityMin = gravity * -0.08f;

            // Create host TODO MOVE IN METHOD TO PARTICLES CREATE PARTICLES
            GameObject host = new GameObject();
            host.name = "ImpactDust";
            host.transform.position = impactPos;
            host.transform.LookAt(impactPos + impactNormal);
            host.transform.parent = scr.transForm;
            host.transform.localScale = Vector3.one;
            GameObject.Destroy(host, 10f); // TODO FIX LIFE TIME

            // Create particle system
            ParticleSystem particleSystem = host.AddComponent<ParticleSystem>();
            particleSystem.Stop();

            // Set dust host vats
            scr.scrDust.dust.particleSystem = particleSystem;
            scr.scrDust.dust.tmDemolition = host.transform;

            // Set main module TODO SET SPEED
            SetMain(particleSystem.main, scr.scrDust.dust.lifeMin, scr.scrDust.dust.lifeMax, scr.scrDust.dust.sizeMin, scr.scrDust.dust.sizeMax, gravityMin, gravityMax, speedMin, speedMax);

            // Emission over distance
            SetEmission(particleSystem.emission, scr.scrDust.dust.distanceRate, amount);

            // Emission from mesh
            SetShapeObject(particleSystem.shape);

            // Collision
            SetCollision(particleSystem.collision, scr.scrDust.dust.collisionRadiusScale, ParticleType.Dust);

            // Color over life time
            SetColorOverLife(particleSystem.colorOverLifetime, scr.scrDust.dust.opacity);

            // Noise
            SetNoise(particleSystem.noise);

            // Renderer
            SetParticleRendererDust(particleSystem.GetComponent<ParticleSystemRenderer>(), scr, scr.scrDust.dust.dustMaterials, scr.scrDust.dust.castShadows, scr.scrDust.dust.receiveShadows);

            // Start playing
            particleSystem.Play();
        }
    }

    // Debris class
    [System.Serializable]
    public class RFDebris : RFParticles
    {
        [Header("  Debris references")]
        
        
        public Material material;
        public List<Mesh> debrisList = new List<Mesh>();

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDebris()
        {
            particleType = ParticleType.Debris;
            particleSystem = null;
            tmDemolition = null;
            onDemolition = false;
            onActivation = false;
            onImpact = false;
            burstType = BurstType.PerOneUnitSize;
            burstAmount = 20;
            distanceRate = 1f;
            sizeMin = 0.5f;
            sizeMax = 1.5f;
            lifeMin = 2f;
            lifeMax = 13f;
            rotationSpeed = 4f;
            collisionRadiusScale = 0.1f;
            percentage = 50;
            sizeThreshold = 0.5f;
            castShadows = true;
            receiveShadows = true;
            material = null;
            debrisList = new List<Mesh>();
        }

        // Copy from
        public void CopyFrom(RFDebris debris)
        {
            particleType = ParticleType.Debris;
            onDemolition = debris.onDemolition;
            onActivation = debris.onActivation;
            onImpact = debris.onImpact;

            burstType = debris.burstType;
            burstAmount = debris.burstAmount;
            distanceRate = debris.distanceRate;
            sizeMin = debris.sizeMin;
            sizeMax = debris.sizeMax;
            lifeMin = debris.lifeMin;
            lifeMax = debris.lifeMax;
            rotationSpeed = debris.rotationSpeed;
            collisionRadiusScale = debris.collisionRadiusScale;
            sizeThreshold = debris.sizeThreshold;
            castShadows = debris.castShadows;
            receiveShadows = debris.receiveShadows;

            material = debris.material;
            debrisList = debris.debrisList;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 

        // Create debris particle system
        public static void CreateDebrisRigidList(RayfireRigid source, List<RayfireRigid> scrList)
        {
            // Check for positive amount
            if (AmountCheck(source, ParticleType.Debris) == false)
                return;

            // Debris mesh list
            List<Mesh> debrisMeshList = GetDebrisMeshList(source.scrDebris.debris.debrisList);

            // Debris hosts
            List<RayfireRigid> particlesHosts = GetParticlesHosts(scrList, source.scrDebris.debris.sizeThreshold, source.scrDebris.debris.percentage);

            // Get amount list
            List<int> burstAmountList = GetBurstAmountList(particlesHosts, source.scrDebris.debris.burstType, source.scrDebris.debris.burstAmount);

            // Decrease burst amount by depth fade
            SetBurstAmount(source, scrList, ParticleType.Debris);

            // Variables
            int seed = 0;

            // Create particle systems
            for (int i = 0; i < particlesHosts.Count; i++)
            {
                // Set random seed
                Random.InitState(seed++);

                // Create single debris particle system
                CreateDebrisRigid(source, particlesHosts[i], debrisMeshList, burstAmountList[i]);
            }
        }

        // Create single debris particle system
        public static void CreateDebrisRigid(RayfireRigid source, RayfireRigid host, List<Mesh> debrisMeshList, int amount)
        {
            float speedMin = 0f; // TODO relative to size
            float speedMax = 2f;
            float gravity = Physics.gravity.magnitude / 9.81f;
            float gravityMax = gravity * 1.1f;
            float gravityMin = gravity * 0.9f;
            ParticleSystem.MinMaxCurve curveSizeOverLifeTime = GetCurveSizeOverLifeTime();
            ParticleSystem.MinMaxCurve curveRotationBySpeed = GetCurveRotationBySpeed(source.scrDebris.debris.rotationSpeed);

            // Particle system
            ParticleSystem particleSystem = CreateParticleSystem(host, source.scrDebris.debris.lifeMax, ParticleType.Debris);

            // Set main module
            SetMain(particleSystem.main, source.scrDebris.debris.lifeMin, source.scrDebris.debris.lifeMax, source.scrDebris.debris.sizeMin, source.scrDebris.debris.sizeMax, gravityMin, gravityMax, speedMin, speedMax);

            // Emission over distance
            SetEmission(particleSystem.emission, source.scrDebris.debris.distanceRate, (short)amount);

            // Emission from mesh
            SetShapeMesh(particleSystem.shape, source, host.meshFilter.sharedMesh);

            // Inherit velocity
            SetVelocity(particleSystem.inheritVelocity);

            // Size over lifetime
            SetSizeOverLifeTime(particleSystem.sizeOverLifetime, curveSizeOverLifeTime);

            // Rotation by speed
            SetRotationBySpeed(particleSystem.rotationBySpeed, curveRotationBySpeed);

            // Collision
            SetCollision(particleSystem.collision, source.scrDebris.debris.collisionRadiusScale, ParticleType.Debris);

            // Renderer
            SetParticleRendererDebris(particleSystem.GetComponent<ParticleSystemRenderer>(), source, debrisMeshList, source.scrDebris.debris.castShadows, source.scrDebris.debris.receiveShadows);

            // Start playing
            particleSystem.Play();
        }

        // Create single debris particle system
        public static void CreateDebrisImpact(RayfireRigid source, Vector3 impactPos, Vector3 impactNormal, short amount)
        {
            float speedMin = 3f; // TODO relative to size
            float speedMax = 8f;
            float gravity = Physics.gravity.magnitude / 9.81f;
            float gravityMax = gravity * 1.1f;
            float gravityMin = gravity * 0.9f;
            ParticleSystem.MinMaxCurve curveSizeOverLifeTime = GetCurveSizeOverLifeTime();
            ParticleSystem.MinMaxCurve curveRotationBySpeed = GetCurveRotationBySpeed(source.scrDebris.debris.rotationSpeed);

            // Create host
            GameObject host = new GameObject();
            host.name = "ImpactDebris";
            host.transform.position = impactPos;
            host.transform.LookAt(impactPos + impactNormal);
            host.transform.parent = source.transForm;
            host.transform.localScale = Vector3.one;
            GameObject.Destroy(host, 5f); // TODO FIX LIFE TIME

            // Create particle system
            ParticleSystem particleSystem = host.AddComponent<ParticleSystem>();
            particleSystem.Stop();

            // Set debris host vats
            source.scrDebris.debris.particleSystem = particleSystem;
            source.scrDebris.debris.tmDemolition = host.transform;
           
            // Set main module
            SetMain(particleSystem.main, source.scrDebris.debris.lifeMin, source.scrDebris.debris.lifeMax, source.scrDebris.debris.sizeMin, source.scrDebris.debris.sizeMax, gravityMin, gravityMax, speedMin, speedMax);

            // Emission over distance
            SetEmission(particleSystem.emission, source.scrDebris.debris.distanceRate, amount);

            // Emission from mesh
            SetShapeObject(particleSystem.shape);

            // Inherit velocity
            SetVelocity(particleSystem.inheritVelocity);

            // Size over lifetime
            SetSizeOverLifeTime(particleSystem.sizeOverLifetime, curveSizeOverLifeTime);

            // Rotation by speed
            SetRotationBySpeed(particleSystem.rotationBySpeed, curveRotationBySpeed);

            // Collision
            SetCollision(particleSystem.collision, source.scrDebris.debris.collisionRadiusScale, ParticleType.Debris);

            // Renderer TODO CHECK MESH LIST
            SetParticleRendererDebris(particleSystem.GetComponent<ParticleSystemRenderer>(), source, source.scrDebris.debris.debrisList, source.scrDebris.debris.castShadows, source.scrDebris.debris.receiveShadows);

            // Start playing
            particleSystem.Play();
        }
    }

    // Debris class
    [System.Serializable]
    public class RFParticles
    {
        // Particles Type
        public enum ParticleType
        {
            Debris  = 0,
            Dust    = 1
        }

        // Burst Type
        public enum BurstType
        {
            None            = 0,
            TotalAmount     = 1,
            PerOneUnitSize  = 2,
            FragmentAmount  = 3
        }

        // Hidden
        [HideInInspector] public ParticleType particleType;
        [HideInInspector] public ParticleSystem particleSystem = null;
        [HideInInspector] public Transform tmDemolition = null;
        [HideInInspector] public Transform tmActivation = null;

        // Particle Debris
        public bool onDemolition = false;
        public bool onActivation = false;
        public bool onImpact = false;

        [Header("  Emitting")]
        [Space (2)]
        
        public BurstType burstType = BurstType.PerOneUnitSize;
        [Range(0, 500)] public int burstAmount = 20;
        [Range(0f, 5f)] public float distanceRate = 1f;

        [Header("  Lifetime")]
        [Space (2)]
        
        [Range(1f, 60f)] public float lifeMin = 10f;
        [Range(1f, 60f)] public float lifeMax = 15f;

        [Header("  Size")] // TODO optional in percents relative to host size
        [Space (2)]
        
        [Range(0.1f, 10f)] public float sizeMin = 0.5f;
        [Range(0.1f, 10f)] public float sizeMax = 1.5f;

        [Header("  Other")]
        [Space (2)]
        
        [Range(0f, 10f)] public float rotationSpeed = 4f;
        [Range(0.1f, 2f)] public float collisionRadiusScale = 1f;

        [Header("  Filters")]
        [Space (2)]
        
        [Range(10, 100)] public int percentage = 100;
        [Range(0.05f, 5)] public float sizeThreshold = 0.5f;

        [Header("  Shadows")]
        [Space (2)]
        
        public bool castShadows = true;
        public bool receiveShadows = true;
        
        // Init particles on demolition
        public static void InitDemolitionParticles(RayfireRigid scr)
        {
            // No frags
            if (scr.HasFragments == false)
                return;
            
            // Create debris particles
            if (scr.scrDebris != null)
            {
                if (scr.scrDebris.debris.onDemolition == true)
                {
                    RFDebris.CreateDebrisRigidList (scr, scr.fragments);
                }
            }

            // Create dust particles
            if (scr.scrDust != null)
            {
                if (scr.scrDust.dust.onDemolition == true)
                {
                    RFDust.CreateDustRigidList (scr, scr.fragments);
                }
            }

            // Detach child particles in case object has child particles and about to be deleted
            DetachParticles(scr);
        }

        // Init particles on activation
        public static void InitActivationParticles(RayfireRigid scr)
        {
            // No frags
            if (scr.HasFragments == false)
                return;
            
            // Create debris particles
            if (scr.scrDebris != null)
                if (scr.scrDebris.debris.onActivation == true)
                    RFDebris.CreateDebrisRigid (scr, scr, scr.scrDebris.debris.debrisList, scr.scrDebris.debris.burstAmount);

            // Create dust particles
            if (scr.scrDust != null)
                if (scr.scrDust.dust.onActivation == true)
                    RFDust.CreateDustRigid (scr, scr, scr.scrDust.dust.burstAmount);
        }

        // Detach child particles in case object has child particles and about to be deleted
        static void DetachParticles(RayfireRigid scr)
        {
            // Detach debris particle system if fragment was already demolished/activated before
            if (scr.scrDebris != null)
            {
                if (scr.scrDebris.debris.tmDemolition != null)
                {
                    scr.scrDebris.debris.tmDemolition.parent     = null;
                    scr.scrDebris.debris.tmDemolition.localScale = Vector3.one;
                }
                if (scr.scrDebris.debris.tmActivation != null)
                {
                    scr.scrDebris.debris.tmActivation.parent     = null;
                    scr.scrDebris.debris.tmActivation.localScale = Vector3.one;
                }
            }

            // Detach dust particle system if fragment was already demolished/activated before
            if (scr.scrDust != null)
            {
                if (scr.scrDust.dust.tmDemolition != null)
                {
                    scr.scrDust.dust.tmDemolition.parent     = null;
                    scr.scrDust.dust.tmDemolition.localScale = Vector3.one;
                }
                if (scr.scrDust.dust.tmActivation != null)
                {
                    scr.scrDust.dust.tmActivation.parent     = null;
                    scr.scrDust.dust.tmActivation.localScale = Vector3.one;
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Main Module
        /// /////////////////////////////////////////////////////////

        // Set main module
        public static void SetMain (ParticleSystem.MainModule main, float lifeMin, float lifeMax, float sizeMin, float sizeMax, float gravityMin, float gravityMax, float speedMin, float speedMax)
        {
            main.duration = 3f;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 500;
            main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;

            // Curve variation
            main.startDelay = new ParticleSystem.MinMaxCurve(0.02f, 0.1f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.25f);
            main.gravityModifier = new ParticleSystem.MinMaxCurve(gravityMin, gravityMax);
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////

        // Set emission
        public static void SetEmission(ParticleSystem.EmissionModule emissionModule, float distanceRate, short burstAmount)
        {
            emissionModule.enabled = true;
            emissionModule.rateOverTimeMultiplier = 0f;
            emissionModule.rateOverDistanceMultiplier = distanceRate;

            // Set burst
            if (burstAmount > 0)
            {
                ParticleSystem.Burst burst = new ParticleSystem.Burst(0.0f, burstAmount, burstAmount, 1, 999f);
                ParticleSystem.Burst[] bursts = new [] { burst };
                emissionModule.SetBursts(bursts);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////

        // Set emitter mesh shape
        public static void SetShapeMesh (ParticleSystem.ShapeModule shapeModule, RayfireRigid scrSource, Mesh mesh)
        {
            shapeModule.normalOffset = 0f;
            shapeModule.shapeType = ParticleSystemShapeType.Mesh;
            shapeModule.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shapeModule.mesh = mesh;

            // Emit from inner surface
            if (scrSource.materials.innerMaterial != null)
            {
                shapeModule.useMeshMaterialIndex = true;
                shapeModule.meshMaterialIndex = 1;
            }
        }

        // Set emitter mesh shape
        public static void SetShapeObject(ParticleSystem.ShapeModule shapeModule)
        {
            shapeModule.shapeType = ParticleSystemShapeType.Hemisphere;
            shapeModule.radius = 0.2f;
            shapeModule.radiusThickness = 0f;
        }

        /// /////////////////////////////////////////////////////////
        /// Velocity
        /// /////////////////////////////////////////////////////////

        // Set velocity
        public static void SetVelocity(ParticleSystem.InheritVelocityModule velocity)
        {
            velocity.enabled = true;
            velocity.mode = ParticleSystemInheritVelocityMode.Initial;
            velocity.curve = new ParticleSystem.MinMaxCurve(0.75f, 1f);
        }

        /// /////////////////////////////////////////////////////////
        /// Size Over Life Time
        /// /////////////////////////////////////////////////////////

        // Set size over life time. Increase almost instantly particles after birth
        public static void SetSizeOverLifeTime(ParticleSystem.SizeOverLifetimeModule sizeOverLifeTime, ParticleSystem.MinMaxCurve curveSizeTime)
        {
            sizeOverLifeTime.enabled = true;
            sizeOverLifeTime.size = curveSizeTime;
        }

        // Get Curve for Size Over Life Time
        public static ParticleSystem.MinMaxCurve GetCurveSizeOverLifeTime()
        {
            float sizeTimeStart = 0.01f;
            Keyframe[] keysSize = new Keyframe[4];
            keysSize[0] = new Keyframe(0f, 0f);
            keysSize[1] = new Keyframe(sizeTimeStart, 1f);
            keysSize[2] = new Keyframe(0.95f, 1f);
            keysSize[3] = new Keyframe(1f, 0f);

            AnimationCurve curveSize = new AnimationCurve(keysSize);
            ParticleSystem.MinMaxCurve curveSizeTime = new ParticleSystem.MinMaxCurve(1f, curveSize);
            return curveSizeTime;
        }

        /// /////////////////////////////////////////////////////////
        /// Rotation by Speed
        /// /////////////////////////////////////////////////////////

        // Set Rotation by Speed
        public static void SetRotationBySpeed(ParticleSystem.RotationBySpeedModule rotationBySpeed, ParticleSystem.MinMaxCurve curveRotationBySpeed)
        {
            rotationBySpeed.enabled = true;
            rotationBySpeed.range = new Vector2(1f, 0f);
            rotationBySpeed.z = curveRotationBySpeed;
        }

        // Get Curve for Rotation by Speed
        public static ParticleSystem.MinMaxCurve GetCurveRotationBySpeed(float rotationSpeed)
        {
            // Value 1f = 57 degrees
            Keyframe key1 = new Keyframe(0f, rotationSpeed);
            Keyframe key2 = new Keyframe(0.5f, 0f);
            Keyframe[] keys = new Keyframe[2];
            keys[0] = key1;
            keys[1] = key2;
            AnimationCurve curve = new AnimationCurve(keys);
            ParticleSystem.MinMaxCurve curveRotationBySpeed = new ParticleSystem.MinMaxCurve(1f, curve);
            return curveRotationBySpeed;
        }

        /// /////////////////////////////////////////////////////////
        /// Color Over Lifetime
        /// /////////////////////////////////////////////////////////

        // Set color over life time
        public static void SetColorOverLife(ParticleSystem.ColorOverLifetimeModule colorLife, float opacity)
        {
           // ParticleSystem.ColorOverLifetimeModule colorLife = particleSystem.colorOverLifetime;
            colorLife.enabled = true;
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
            alphaKeys[0] = new GradientAlphaKey(0f, 0f);
            alphaKeys[1] = new GradientAlphaKey(opacity, 0.1f);
            alphaKeys[2] = new GradientAlphaKey(opacity, 0.2f);
            alphaKeys[3] = new GradientAlphaKey(0f, 1f);
            Gradient gradient = new Gradient();
            gradient.alphaKeys = alphaKeys;
            colorLife.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////

        public static void SetNoise (ParticleSystem.NoiseModule noise)
        {
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            noise.frequency = 0.3f;
            noise.scrollSpeed = 0.7f;
            noise.quality = ParticleSystemNoiseQuality.Medium;
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Set collision TODO set collision vars
        public static void SetCollision(ParticleSystem.CollisionModule collision, float radius, ParticleType particleType) {
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.radiusScale = radius;
            if (particleType == ParticleType.Debris)
            {
                collision.dampen = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
                collision.bounce = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            }
            else if (particleType == ParticleType.Dust)
            {
                collision.dampenMultiplier = 0f;
                collision.bounceMultiplier = 0f;
                collision.enableDynamicColliders = false;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Renderer
        /// /////////////////////////////////////////////////////////

        // Set renderer
        public static void SetParticleRendererDebris (ParticleSystemRenderer renderer, RayfireRigid scrSource, List<Mesh> meshList, bool cast, bool receive)
        {
            // Common vars
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.alignment = ParticleSystemRenderSpace.World;

            // Set predefined meshes
            if (meshList.Count > 0)
            {
                if (meshList.Count <= 4)
                {
                    renderer.SetMeshes (meshList.ToArray());
                    renderer.mesh = meshList[0];
                }
                else
                {
                    List<Mesh> newList = new List<Mesh>();
                    for (int i = 0; i < 4; i++)
                        newList.Add (meshList[Random.Range (0, meshList.Count)]);
                    renderer.SetMeshes (newList.ToArray());
                    renderer.mesh = newList[0];
                }
            }

            // Set material. Original or inner
            SetMaterialDebris(scrSource, renderer);

            // Shadow casting
            if (cast == true)
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Shadow receiving
            renderer.receiveShadows = receive;
        }

        // Set renderer
        public static void SetParticleRendererDust(ParticleSystemRenderer renderer, RayfireRigid scrSource, List<Material> dustMaterials, bool cast, bool receive)
        {
            // Common vars
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.World;
            renderer.normalDirection = 1f;

            // Set material. Original or inner
            SetMaterialDust(renderer, dustMaterials);

            // Shadow casting
            if (cast == true)
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Shadow receiving
            renderer.receiveShadows = receive;

            // Dust vars
            renderer.sortMode = ParticleSystemSortMode.OldestInFront;
            renderer.minParticleSize = 0.0001f;
            renderer.maxParticleSize = 999999f;
            renderer.alignment = ParticleSystemRenderSpace.Facing;



            // Set Roll in 2018.3 and older builds TODO
            //           if (Application.unityVersion == "2018.3.0f2")
            //               renderer.shadowBias = 0.55f;
            //               renderer.allowRoll = false;
        }

        // Set material
        static void SetMaterialDebris(RayfireRigid scrSource, ParticleSystemRenderer renderer)
        {
            // Set debris material if defined
            if (scrSource.scrDebris.debris.material != null)
                renderer.sharedMaterial = scrSource.scrDebris.debris.material;

            // Set material. Original or inner
            else
            {
                if (scrSource.materials.innerMaterial == null)
                    renderer.sharedMaterial = scrSource.meshRenderer.sharedMaterial;
                else
                    renderer.sharedMaterial = scrSource.materials.innerMaterial;
            }
        }

        // Set material
        static void SetMaterialDust(ParticleSystemRenderer renderer, List<Material> dustMaterials)
        {
            // No material
            if (dustMaterials.Count == 0)
            {
                Debug.Log("Define dust material");
                return;
            }

            // Set material
            if (dustMaterials.Count == 1)
                renderer.sharedMaterial = dustMaterials[0];
            else
                renderer.sharedMaterial = dustMaterials[Random.Range(0, dustMaterials.Count - 1)];
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Create particle root
        public static ParticleSystem CreateParticleSystem(RayfireRigid scr, float lifeMax, ParticleType particleType)
        {
            // Get root suffix
            string suffix = "_debris";
            if (particleType == ParticleType.Dust)
                suffix = "_dust";

            // Create root
            GameObject host = new GameObject(scr.name + suffix);
            host.transform.position = scr.transform.position;
            host.transform.rotation = scr.transform.rotation;
            host.transform.parent = scr.transForm;
            host.transform.localScale = Vector3.one;

            // Particle system
            ParticleSystem particleSystem = host.AddComponent<ParticleSystem>();
            if (particleType == ParticleType.Debris)
            {
                scr.scrDebris.debris.particleSystem = particleSystem;
                scr.scrDebris.debris.tmDemolition = host.transform;
            }
            else if (particleType == ParticleType.Dust)
            {
                scr.scrDust.dust.particleSystem = particleSystem;
                scr.scrDust.dust.tmDemolition = host.transform;
            }
            
            // Destroy after all particles death
            GameObject.Destroy(host, lifeMax + particleSystem.main.duration);

            // Stop for further properties set
            particleSystem.Stop();
            
            return particleSystem;
        }

        // Decrease burst amount by depth fade
        public static void SetBurstAmount(RayfireRigid scrSource, List<RayfireRigid> scrList, ParticleType particleType)
        {
            // Set debris burst amount
            if (particleType == ParticleType.Debris)
            {
                if (scrSource.scrDebris.debris.burstType == BurstType.TotalAmount || scrSource.scrDebris.debris.burstType == BurstType.FragmentAmount)
                {
                    float size = scrSource.limitations.bound.size.magnitude;
                    foreach (RayfireRigid scr in scrList)
                        scr.scrDebris.debris.burstAmount = (int)(scr.limitations.bound.size.magnitude / size * scrSource.scrDebris.debris.burstAmount);
                }
            }

            // Set dust burst amount
            if (particleType == ParticleType.Dust)
            {
                if (scrSource.scrDust.dust.burstType == BurstType.TotalAmount || scrSource.scrDust.dust.burstType == BurstType.FragmentAmount)
                {
                    float size = scrSource.limitations.bound.size.magnitude;
                    foreach (RayfireRigid scr in scrList)
                        scr.scrDust.dust.burstAmount = (int)(scr.limitations.bound.size.magnitude / size * scrSource.scrDust.dust.burstAmount + 5);
                }
            }
        }

        // Check for positive amount
        public static bool AmountCheck(RayfireRigid scrSource, ParticleType particleType)
        {
            // Check debris burst amount
            if (particleType == ParticleType.Debris)
            {
                if (scrSource.scrDebris.debris.burstType == BurstType.None && scrSource.scrDebris.debris.distanceRate == 0)
                {
                    Debug.Log(scrSource.name + " has debris enabled but has no amount");
                    return false;
                }
            }

            // Check dust burst amount
            if (particleType == ParticleType.Dust)
            {
                if (scrSource.scrDust.dust.burstType == BurstType.None && scrSource.scrDust.dust.distanceRate == 0)
                {
                    Debug.Log(scrSource.name + " has dust enabled but has no amount");
                    return false;
                }
            }

            return true;
        }

        // Get debris meshes
        public static List<Mesh> GetDebrisMeshList(List<Mesh> debrisList)
        {
            // Debris mesh list
            List<Mesh> meshList = new List<Mesh>();
            foreach (Mesh mesh in debrisList)
                if (mesh != null && mesh.vertexCount > 3)
                    meshList.Add(mesh);

            // Check for debris meshes
            if (meshList.Count == 0)
            {
                Debug.Log("Define debris mesh");
                // TODO create default debris mesh
            }

            return meshList;
        }

        // Get debris hosts
        public static List<RayfireRigid> GetParticlesHosts (List<RayfireRigid> scrList, float sizeThreshold, int percentage)
        {
            // Filter particle sources
            List<RayfireRigid> particleHosts = new List<RayfireRigid>();

            // Set max amount
            int maxAmount = scrList.Count;
            if (percentage < 100)
                maxAmount = scrList.Count * percentage / 100;

            // Collect hosts list
            foreach (RayfireRigid scr in scrList)
            {
                // Max amount reached
                if (particleHosts.Count >= maxAmount)
                    break;

                // Filter by size threshold
                if (scr.limitations.bound.size.magnitude < sizeThreshold)
                    continue;

                // Filter by percentage
                if (Random.Range(0, 100) > percentage)
                    continue;

                // Collect particle hosts
                particleHosts.Add(scr);
            }
            return particleHosts;
        }

        // Get amount list
        public static List<int> GetBurstAmountList(List<RayfireRigid> particleHosts, BurstType burstType, int burstAmount)
        {
            List<int> amountList = new List<int>();

            // No burst
            if (burstType == BurstType.None)
                foreach (RayfireRigid scr in particleHosts)
                    amountList.Add(0);

            // Same burst amount for every fragment
            if (burstType == BurstType.FragmentAmount)
                foreach (RayfireRigid scr in particleHosts)
                    amountList.Add(burstAmount);

            // Burst amount per particles per fragment size
            else if (burstType == BurstType.PerOneUnitSize)
                foreach (RayfireRigid scr in particleHosts)
                    amountList.Add((int)(burstAmount * scr.limitations.bound.size.magnitude));

            // Burst amount by total amount divided among hosts by their amount and size
            else if (burstType == BurstType.TotalAmount)
            {
                // Get sum of all sizes
                float totalSize = 0f;
                foreach (RayfireRigid scr in particleHosts)
                    totalSize += scr.limitations.bound.size.magnitude;

                // Get size per particle
                float sizePerParticle = totalSize / burstAmount;
               
                // Get size for every host by it's size
                foreach (RayfireRigid scr in particleHosts)
                    amountList.Add((int)(scr.limitations.bound.size.magnitude / sizePerParticle));
            }

            return amountList;
        }
    }
}


