using EntityStates;
using EntityStates.Bison;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Survivors.Yusuke;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    public class SpiritShotgunFollowUp : BaseSkillState
    {

        public int ID;
        private BullseyeSearch search = new BullseyeSearch();
        public HurtBox target;

        public float maxTrackingDistance = 60f;
        public float maxTrackingAngle = 60f;

        private float speed = 10f;
        private float duration = 0.2f;
        public float charge;

        private bool hasFiredShotgun;
        private bool foundTarget;

        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"
        public GameObject spiritImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion"); //YusukeAssets.spiritGunExplosionEffect;
        private string muzzleString;
        public static float procCoefficient = 1f;
        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;

        public override void OnEnter()
        {
            base.OnEnter();

            if (ID != 0)
            {
                characterMotor.enabled = true;
                characterDirection.enabled = true;
                characterMotor.Motor.ForceUnground();
                muzzleString = "Muzzle";

            }


        }



        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (ID != 0)
            {

                if (!foundTarget) DashTowardsEnemy();

                if (foundTarget)
                {
                    Log.Info("target found searching for targets");
                    characterMotor.velocity = Vector3.zero;

                    SearchForTargets(out var targets);
                    ShootShotGun(targets);

                }

                if (fixedAge >= duration && isAuthority && hasFiredShotgun)
                {
                    Log.Info("shotgun complete");
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


        private void ShootShotGun(List<HurtBox> targets)
        {
            if (!hasFiredShotgun)
            {
                hasFiredShotgun = true;
                Log.Info("Firing shotgun");
                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                Ray aimRay = GetAimRay();

                int damageDivision = targets.Count;

                characterMotor.velocity = -characterDirection.moveVector * 18f;
                foreach (HurtBox enemy in targets)
                {
                    //do a check if its a max charge, it SlowOnHit. If not, then regular.
                    EffectManager.SpawnEffect(spiritImpactEffect, new EffectData
                    {
                        origin = enemy.gameObject.transform.position,
                        scale = 8f
                    }, transmit: true);
                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = enemy.gameObject.transform.position - transform.position,
                        origin = aimRay.origin,
                        damage = (damageCoefficient / damageDivision) * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.SlowOnHit,
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

                    }.Fire();
                }
            }
        }


        public void SearchForTargets(out List<HurtBox> currentHurtbox)
        {

            Log.Info("Searching for targets");
            Log.Info("ID:" + ID);
            Ray aimRay = GetAimRay();
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = transform.position;
            search.searchDirection = characterDirection.forward;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;

            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            // for everything that was found

            Log.Info("Getting results");
            List<HurtBox> totalEnemies = new List<HurtBox>();
            foreach (HurtBox result in search.GetResults())
            {
                // if it has a healthbar and they are alive
                if ((bool)result.healthComponent && result.healthComponent.alive)
                {
                    totalEnemies.Add(result);

                }

            }

            currentHurtbox = totalEnemies;
            return;


        }


        private void DashTowardsEnemy()
        {
            //Log.Info("Capturing target");

            if (ID != 0)
            {

                if (characterMotor && characterDirection)
                {


                    Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

                    // Calculate the velocity in the direction of the target
                    Vector3 forwardSpeed = directionToTarget * (speed * moveSpeedStat);

                    // Apply the velocity to the character's motor
                    characterMotor.velocity = forwardSpeed;

                    //Log.Info("Checking colliders");

                    //Log.Info("creating collider");
                    Collider[] colliders;
                    //Log.Info("physics");
                    colliders = Physics.OverlapSphere(transform.position, 2, LayerIndex.entityPrecise.mask);
                    //Log.Info("converting to list");
                    List<Collider> capturedColliders = colliders.ToList();

                    // check each hurtbox and catpure the hurtbox they have, then compare the two for a match.
                    foreach (Collider result in capturedColliders)
                    {
                        HurtBox capturedHurtbox = result.GetComponent<HurtBox>();

                        if (capturedHurtbox)
                        {
                            if (capturedHurtbox == target)
                            {
                                Log.Info("Enemy found");
                                foundTarget = true;
                                break;
                            }


                        }
                    }
                }
                else
                {
                    //Log.Info("No character motor or direction");
                }




            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }


    }
}
