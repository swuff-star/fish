using RoR2;
using UnityEngine;
using FishMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using UnityEngine.AddressableAssets;
using FishMod.Characters.Survivors.Fish.Components;

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

        // pickups
        public static GameObject ammoPickupPrefab;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("HenrySwordHit");

            CreateEffects();

            CreateProjectiles();

            CreatePickups();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
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
