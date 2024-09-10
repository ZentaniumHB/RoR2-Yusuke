using EntityStates;
using YusukeMod.Survivors.Yusuke;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq.JsonPath;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{
    public class FireShotgun : BaseSkillState
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

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public float charge;


        private float pelletRadius = 8f;
        private bool hasScanned;
        private List<HurtBox> targets;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime)
            {
                Scan();
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Scan()
        {
            if (!hasScanned)
            {
                hasScanned = true;
                Ray aimRay = GetAimRay();
                BullseyeSearch search = new BullseyeSearch
                {
                    teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                    filterByLoS = false,
                    searchOrigin = transform.position,
                    searchDirection = aimRay.direction,
                    sortMode = BullseyeSearch.SortMode.Distance,
                    maxDistanceFilter = pelletRadius,
                    maxAngleFilter = 20f
                };

                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);
                targets = search.GetResults().ToList<HurtBox>();

            }

            int enemyCount = 0;
            foreach (HurtBox singularTarget in targets)
            {
                if (singularTarget)
                {

                    
                }

            }

            Log.Info("Enemies found: " + enemyCount);





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

                /*BullseyeSearch search = new BullseyeSearch
                {
                    teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                    filterByLoS = false,
                    searchOrigin = transform.position,
                    searchDirection = aimRay.direction,
                    sortMode = BullseyeSearch.SortMode.Distance,
                    maxDistanceFilter = pelletRadius,
                    maxAngleFilter = 90f
                };

                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);

                List<HurtBox> target = search.GetResults().ToList<HurtBox>();
                int enemyCount = 0;
                foreach (HurtBox singularTarget in target)
                {
                    if (singularTarget)
                    {
                        enemyCount++;
                    }

                }

                Log.Info("Enemies found: " + enemyCount);*/

                if (isAuthority)
                {
                    
                    //AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                    

                    /*new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
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
                    }.Fire();*/
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}