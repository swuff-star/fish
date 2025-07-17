using RoR2;
using UnityEngine;
using FishMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using UnityEngine.AddressableAssets;
using FishMod.Characters.Survivors.Fish.Components;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using HarmonyLib;

namespace FishMod.Survivors.Fish
{
    public static class FishAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        // projectiles
        public static GameObject bombProjectilePrefab;

        // bullet-related
        public static GameObject bulletPrefab;
        public static GameObject bulletGhostPrefab;
        public static GameObject heavyBulletPrefab;

        // laser-related
        public static GameObject laserTracerPrefab;

        // pickups
        public static GameObject ammoPickupPrefab;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("HenrySwordHit");

            CreateEffects();

            CreateProjectiles();

            CreateBullets();
            CreateLasers();

            CreatePickups();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
        }

        // to-do: some of this stuff definitely will need to be generic methods that defines all the different variants
        // like yeah bro just go and manually redefine every laser for laser brain
        // wtf
        private static void CreateBullets()
        {
            // regular bullet 
            #region bullet
            bulletGhostPrefab = _assetBundle.LoadAsset<GameObject>("BulletGhost");
            if (bulletGhostPrefab == null)
            {
                Log.Error("FishAssets.CreateBulletEffects : Failed to initialize bullet ghost.");
            }

            ParticleSystem bulletParticle = bulletGhostPrefab.transform.Find("Glow")?.GetComponent<ParticleSystem>();
            if (bulletParticle != null && bulletParticle.TryGetComponent(out ParticleSystemRenderer psr))
            {
                psr.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matGenericFlash.mat").WaitForCompletion();
            }

            Light light = bulletGhostPrefab.transform.Find("Light")?.GetComponent<Light>();

            bulletGhostPrefab.AddComponent<NetworkIdentity>();
            bulletGhostPrefab.AddComponent<ProjectileGhostController>();

            VFXAttributes vfx = bulletGhostPrefab.AddComponent<VFXAttributes>();
            vfx.vfxPriority = VFXAttributes.VFXPriority.Always;
            vfx.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfx.DoNotPool = false;
            vfx.DoNotCullPool = false;

            if (light != null)
            {
                vfx.optionalLights.AddItem(light);

                FlickerLight flicker = bulletGhostPrefab.AddComponent<FlickerLight>();

                flicker.sinWaves.AddItem(new Wave() { amplitude = 0.2f, frequency = 12f, cycleOffset = 1.2f });
                flicker.sinWaves.AddItem(new Wave() { amplitude = 0.2f, frequency = 10f, cycleOffset = 2f });
                flicker.sinWaves.AddItem(new Wave() { amplitude = 0.1f, frequency = 60f, cycleOffset = 0f });
            }

            bulletPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/Fireball.prefab").WaitForCompletion(), "FishBulletProjectile");
            if (bulletPrefab.TryGetComponent(out ProjectileController pc))
            {
                pc.ghostPrefab = bulletGhostPrefab;
            }

            if (bulletPrefab.TryGetComponent(out ProjectileSimple ps))
            {
                ps.desiredForwardSpeed = FishStaticValues.bulletSpeed;
            }

            if (bulletPrefab.TryGetComponent(out ProjectileSingleTargetImpact psti))
            {
                // psti.impactEffect = ;  later
            }

            if (bulletPrefab.TryGetComponent(out ProjectileDamage pd))
            {
                pd.damageType.damageType = DamageType.Generic;
                pd.damageType.damageTypeExtended = DamageTypeExtended.Generic;
                pd.damageType.damageSource = DamageSource.Primary;
            }

            // maybe could have sounds for bullets when they're flying past?
            // little whooshing sound
            foreach (AkEvent ak in bulletPrefab.GetComponents<AkEvent>())
            {
                UnityEngine.Object.Destroy(ak);
            }

            if (bulletPrefab.TryGetComponent(out AkGameObj akgo))
            {
                UnityEngine.Object.Destroy(akgo);
            }

            #endregion bullet
        }

        private static void CreateLasers()
        {
            laserTracerPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CaptainDefenseMatrix/TracerCaptainDefenseMatrix.prefab").WaitForCompletion(), "FishLaserTracerPrefab");

            laserTracerPrefab.AddComponent<NetworkIdentity>();

            if (laserTracerPrefab == null)
            {
                Log.Error("FishAssets.CreateLasers : Failed to instantiate laser tracer!");
            }

            if (laserTracerPrefab.TryGetComponent(out EffectComponent ec))
            {
                // ec.soundName =   laser sound string here
            }

            if (laserTracerPrefab.TryGetComponent(out Tracer tracer))
            {
                // tracer.beamDensity =   ??;
                // tracer.speed =   ??;
                // tracer.length =   ??;
            }

            if (laserTracerPrefab.TryGetComponent(out LineRenderer renderer))
            {
                Material mat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Captain/matCaptainDefenseMatrixLaser.mat").WaitForCompletion();
                Material tracerMat = new Material(mat);

                tracerMat.SetTexture("_RemapTex", _assetBundle.LoadAsset<Texture2D>("texRampLaser"));
                renderer.material = tracerMat;
            }

            Material ringMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/GreaterWisp/matOmniRing1GreaterWisp.mat").WaitForCompletion();

            Transform startTransform = laserTracerPrefab.transform.Find("StartTransform");
            if (startTransform != null)
            {
                Transform ring = startTransform.Find("Ring");
                if (ring != null && ring.TryGetComponent(out ParticleSystem rps) && rps.TryGetComponent(out ParticleSystemRenderer rpsr)) 
                {
                    rps.startSize *= 0.3f;
                    rpsr.material = ringMat;
                }
                Transform flash = startTransform.Find("Flash");
                if (flash != null && flash.TryGetComponent(out ParticleSystem fps))
                {
                    fps.startColor = Color.green;
                }
            }

            Transform tracerHead = laserTracerPrefab.transform.Find("TracerHead");
            if (tracerHead != null)
            {
                Transform ring = tracerHead.Find("Ring");
                if (ring != null && ring.TryGetComponent(out ParticleSystem rps) && rps.TryGetComponent(out ParticleSystemRenderer rpsr))
                {
                    rps.startSize *= 0.3f;
                    rpsr.material = ringMat;
                }
                Transform flash = tracerHead.Find("Flash");
                if (flash != null && flash.TryGetComponent(out ParticleSystem fps))
                {
                    fps.startColor = Color.green;
                }
                Transform light = tracerHead.Find("Point Light");
                if (light != null && light.TryGetComponent(out Light l))
                {
                    l.color = Color.green;
                }
            }

            Content.CreateAndAddEffectDef(laserTracerPrefab);

            Log.Info("FishAssets.CreateLasers : Finished setting up lasers");
        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateBombProjectile();
            Content.AddProjectilePrefab(bombProjectilePrefab);
        }

        private static void CreatePickups()
        {
            CreateAmmoPickup();
            Content.AddNetworkedObjectPrefab(ammoPickupPrefab);
        }

        private static void CreateAmmoPickup()
        {
            ammoPickupPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandolier/AmmoPack.prefab").WaitForCompletion(), "FishAmmoPickup");

            GameObject pickupTrigger = ammoPickupPrefab.transform.Find("PickupTrigger").gameObject;
            if (pickupTrigger != null)
            {
                AmmoPickup ammoPickup = pickupTrigger.GetComponent<AmmoPickup>();

                GameObject baseObject = ammoPickup.baseObject;
                TeamFilter teamFilter = ammoPickup.teamFilter;
                GameObject pickupEffect = ammoPickup.pickupEffect;

                UnityEngine.Object.Destroy(pickupTrigger.GetComponent<AmmoPickup>());

                FishAmmoPickup fishAmmoPickup = pickupTrigger.AddComponent<FishAmmoPickup>();
                fishAmmoPickup.baseObject = baseObject;
                fishAmmoPickup.teamFilter = teamFilter;
                fishAmmoPickup.pickupEffect = pickupEffect;
            }

            GameObject gravitationController = ammoPickupPrefab.transform.Find("GravitationController").gameObject;
            if (gravitationController != null)
            {
                GravitatePickup gravitatePickup = gravitationController.GetComponent<GravitatePickup>();

                Rigidbody rb = gravitatePickup.rigidbody;
                TeamFilter teamFilter = gravitatePickup.teamFilter;
                float acceleration = gravitatePickup.acceleration;
                float maxSpeed = gravitatePickup.maxSpeed;

                UnityEngine.Object.Destroy(gravitationController.GetComponent<GravitatePickup>());

                FishGravitatePickup fishGravitatePickup = gravitationController.AddComponent<FishGravitatePickup>();
                fishGravitatePickup.rigidbody = rb;
                fishGravitatePickup.teamFilter = teamFilter;
                fishGravitatePickup.acceleration = acceleration;
                fishGravitatePickup.maxSpeed = maxSpeed;
            }
        }

        private static void CreateBombProjectile()
        {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            bombProjectilePrefab = Asset.CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(bombProjectilePrefab.GetComponent<ProjectileImpactExplosion>());
            ProjectileImpactExplosion bombImpactExplosion = bombProjectilePrefab.AddComponent<ProjectileImpactExplosion>();
            
            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.blastDamageCoefficient = 1f;
            bombImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = bombExplosionEffect;
            bombImpactExplosion.lifetimeExpiredSound = Content.CreateAndAddNetworkSoundEventDef("HenryBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombProjectilePrefab.GetComponent<ProjectileController>();

            if (_assetBundle.LoadAsset<GameObject>("HenryBombGhost") != null)
                bombController.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("HenryBombGhost");
            
            bombController.startSound = "";
        }
        #endregion projectiles
    }
}
