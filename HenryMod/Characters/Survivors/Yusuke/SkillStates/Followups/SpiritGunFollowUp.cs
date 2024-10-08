using EntityStates;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    internal class SpiritGunFollowUp : BaseSkillState
    {

        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"
        public GameObject spiritImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion"); //YusukeAssets.spiritGunExplosionEffect;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public float charge;

        public int ID;
        public HurtBox target;

        public override void OnEnter()
        {
            base.OnEnter();

            Log.Info("ENTERED SPIRIT FOLLOW UP");
            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
            Log.Info("LEAVING SPIRIT FOLLOW UP");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (ID != 0)
            {
                if (fixedAge >= fireTime)
                {
                    Fire();
                }

                if (fixedAge >= duration && isAuthority)
                {

                    Log.Info("ID IN Spirit follow up: " + ID);
                    outer.SetNextState(new RevertSkills
                    {
                        moveID = ID

                    });

                }
            }
            else
            {
                outer.SetNextStateToMain();     // this is only because for some reason this state gets called more than once, dunno why yet.
            }



        }

        private void Fire()
        {
            if (!hasFired)
            {

                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                Ray aimRay = GetAimRay();


                EffectManager.SpawnEffect(spiritImpactEffect, new EffectData
                {
                    origin = target.gameObject.transform.position,
                    scale = 8f
                }, transmit: true);
                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = target.gameObject.transform.position - transform.position,
                    origin = aimRay.origin,
                    damage = (charge / 2) * damageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default,
                    procCoefficient = procCoefficient,
                    radius = 0.75f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,

                }.Fire();   // blah
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

    }
}
