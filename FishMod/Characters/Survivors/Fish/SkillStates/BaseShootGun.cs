using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.Fish
{
    public class BaseShootGun : BaseFishState
    {
        protected virtual GameObject MuzzleEffectPrefab => Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab;
        protected virtual GameObject HitEffectPrefab => Commando.CommandoWeapon.FirePistol2.hitEffectPrefab;
        protected virtual GameObject TracerEffectPrefab => Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab;

        protected virtual float DamageCoefficient => 1f;
        protected virtual float ProcCoefficient => 1f;
        protected virtual float Force => 100f;
        protected virtual float Radius => 0.5f;
        protected virtual float Range => 100f;
        protected virtual float BaseDuration => 1f;
        protected virtual float FirePercentTime => 0.0f;
        protected virtual string FireSoundString => "HenryShootPistol";
        protected virtual float RecoilAmplitude => 1f;
        protected virtual float MaxSpread => 1f;
        protected virtual float SpreadBloomValue => 0.3f;
        protected virtual float TrajectoryAimAssistMultiplier => 1f;
        protected virtual BulletAttack.FalloffModel FalloffModel => BulletAttack.FalloffModel.DefaultBullet;
        protected virtual string MuzzleName => "Muzzle";

        private float fireTime;
        private float stateDuration;
        private float reloadDuration;
        private Ray aimRay;
        private bool isCrit;

        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            stateDuration = BaseDuration / attackSpeedStat;
            reloadDuration = BaseDuration / attackSpeedStat;
            fireTime = FirePercentTime * stateDuration;

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
            Util.PlayAttackSpeedSound(FireSoundString, gameObject, attackSpeedStat);

            if (MuzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(MuzzleEffectPrefab, gameObject, MuzzleName, false);
            }

            AddRecoil(-0.4f * RecoilAmplitude, -0.8f * RecoilAmplitude, -0.3f * RecoilAmplitude, 0.3f * RecoilAmplitude);

            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = MaxSpread;
                bulletAttack.maxDistance = Range;
                bulletAttack.damage = DamageCoefficient * damageStat;
                bulletAttack.procCoefficient = ProcCoefficient;
                bulletAttack.force = Force;
                bulletAttack.tracerEffectPrefab = TracerEffectPrefab;
                bulletAttack.muzzleName = MuzzleName;
                bulletAttack.hitEffectPrefab = HitEffectPrefab;
                bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
                bulletAttack.isCrit = isCrit;
                bulletAttack.radius = Radius;
                bulletAttack.smartCollision = true;
                bulletAttack.trajectoryAimAssistMultiplier = TrajectoryAimAssistMultiplier;
                bulletAttack.damageType = DamageTypeCombo.GenericPrimary;
                bulletAttack.Fire();
            }
            characterBody.AddSpreadBloom(SpreadBloomValue);

            if (weaponController != null) weaponController.ConsumeAmmo();

            skillLocator.primary.cooldownOverride = reloadDuration;

            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            base.OnExit();

            skillLocator.primary.cooldownOverride = stateDuration;
        }
    }
}
