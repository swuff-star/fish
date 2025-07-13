using FishMod.Modules.Weapons;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.Fish.Guns
{
    public class FireMachinegun : BaseFishState
    {
        public static GameObject muzzleEffectPrefab = Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab;
        public static GameObject hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab;
        public static GameObject tracerEffectPrefab = Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab;

        public static float damageCoefficient = 2f;
        public static float procCoefficient = 1f;
        public static float force = 50f;
        public static float radius = 0.5f;
        public static float range = 96f;
        public static float baseDuration = 0.17f;
        public static float firePercentTime = 0.0f;
        public static string fireSoundString = "HenryShootPistol";
        public static float recoilAmplitude = 1f;
        public static float maxSpread = 1.5f;
        public static float spreadBloomValue = 0.3f;
        public static float trajectoryAimAssistMultiplier = 1f;
        public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.DefaultBullet;
        public static string muzzleName = "Muzzle";

        private float fireTime;
        private float stateDuration;
        private float reloadDuration;
        private Ray aimRay;
        private bool isCrit;

        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            stateDuration = baseDuration / attackSpeedStat;
            reloadDuration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * stateDuration;

            aimRay = GetAimRay();
            isCrit = RollCrit();

            StartAimMode(aimRay, stateDuration * 1.5f);

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (hasFired == false && fixedAge >= fireTime)
            {
                hasFired = true;
                Fire();
            }

            if (fixedAge >= stateDuration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            Util.PlayAttackSpeedSound(fireSoundString, gameObject, attackSpeedStat);

            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);
            }

            AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);

            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = maxSpread;
                bulletAttack.maxDistance = range;
                bulletAttack.damage = damageCoefficient * damageStat;
                bulletAttack.procCoefficient = procCoefficient;
                bulletAttack.force = force;
                bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                bulletAttack.muzzleName = muzzleName;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
                bulletAttack.isCrit = isCrit;
                bulletAttack.radius = radius;
                bulletAttack.smartCollision = true;
                bulletAttack.trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier;
                bulletAttack.damageType = DamageTypeCombo.GenericPrimary;
                bulletAttack.Fire();
            }
            characterBody.AddSpreadBloom(spreadBloomValue);

            if (weaponController != null) weaponController.ConsumeAmmo();

            //skillLocator.primary.cooldownOverride = reloadDuration;
            if (skillLocator.primary.skillDef is FishWeaponSkillDef fishWeaponSkillDef)
            {
                fishWeaponSkillDef.pseudoCooldownRemaining = reloadDuration;
            }

            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
