using EntityStates;
using EntityStates.Bison;
using EntityStates.Commando.CommandoWeapon;
using Rewired.Demos;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Survivors.Yusuke;


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
        private bool hasAddedController;
        private bool isMostLikelyFlyingEnemy;

        //slow down variables
        private float slowDownStopWatch;
        private float slowDownDuration = 1f;

        private ShotgunController controller;

        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"
        public GameObject spiritImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion"); //YusukeAssets.spiritGunExplosionEffect;
        private string muzzleString;
        public static float procCoefficient = 1f;
        public static float damageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        private bool hasAppliedForce;

        private GameObject spiritShotGunChargeEffectPotentPrefab;
        private GameObject spiritShotGunChargeEffectPotentObject;

        private Transform modelTransform;
        private AimAnimator aimAnim;

        public static GameObject spiritTracerEffect = YusukeAssets.spiritShotGunTracerEffect;
        private GameObject spiritShotGunExplosionHitEffect = YusukeAssets.spiritShotGunHitEffect;
        private GameObject spiritGunMuzzleFlashPrefab;

        private bool hasPlayedStartUpAnimation;
        private bool hasPlayedFinishAnimation;

        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string divePunchCenter = "divePunchCenter";

        public override void OnEnter()
        {
            base.OnEnter();

            if (ID != 0)
            {
                spiritShotGunChargeEffectPotentPrefab = YusukeAssets.spiritShotGunChargePotentEffect;
                spiritGunMuzzleFlashPrefab = YusukeAssets.spiritGunMuzzleFlashEffect;

                modelTransform = GetModelTransform();
                aimAnim = modelTransform.GetComponent<AimAnimator>();

                controller = new ShotgunController();
                characterMotor.enabled = true;
                characterDirection.enabled = true;
                characterMotor.Motor.ForceUnground();
                muzzleString = "Muzzle";


                aimAnim.enabled = false;
                SpawnAndEditEffect();
            }
            

        }

        private void SpawnAndEditEffect()
        {
            if (spiritShotGunChargeEffectPotentPrefab != null) spiritShotGunChargeEffectPotentObject = YusukePlugin.CreateEffectObject(spiritShotGunChargeEffectPotentPrefab, FindModelChild("HandR"));
            
        }


        public override void OnExit()
        {
            base.OnExit();
            if (ID != 0)
            {
                aimAnim.enabled = true;
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
                    AddController();

                    if (!hasPlayedStartUpAnimation)
                    {
                        hasPlayedStartUpAnimation = true;
                        PlayAnimation("FullBody, Override", "ShotgunFollowUpStartUp", "ShootGun.playbackRate", 1f);
                        spiritShotGunChargeEffectPotentObject.SetActive(true);

                    }

                    if(slowDownStopWatch > slowDownDuration)
                    {
                        SearchForTargets(out var targets);
                        ShootShotGun(targets);
                        controller.Remove();
                        ApplyForce();
                        outer.SetNextState(new RevertSkills
                        {
                            moveID = ID

                        });
                    }
                    else
                    {
                        SlowDown();
                    }
                    

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

        private void ApplyForce()
        {
            if (!hasAppliedForce)
            {
                hasAppliedForce = true;
                Vector3 forceVector = (characterDirection.forward + -transform.up);    
                forceVector *= 20000f;
                if (target.healthComponent.body.isChampion || target.healthComponent.body.isBoss) forceVector = characterDirection.forward *= 35000f;

                AttackForce(forceVector);
            }
        }

        private void AttackForce(Vector3 forceVector)
        {
            DamageInfo damageInfo = new DamageInfo
            {
                attacker = gameObject,
                damage = damageCoefficient * damageStat,
                crit = RollCrit(),
                procCoefficient = procCoefficient,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.SlowOnHit,
                position = characterBody.corePosition,
                force = forceVector,
                canRejectForce = false
            };
            target.healthComponent.TakeDamage(damageInfo);
        }

        private void AddController()
        {
            if (!hasAddedController)
            {
                hasAddedController = true;

                if (target)
                {
                    CharacterMotor enemyMotor = target.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
                    Rigidbody enemyRigidBody = target.healthComponent.body.gameObject.GetComponent<Rigidbody>();

                    if (enemyRigidBody)
                    {
                        if (enemyMotor)
                        {
                            Log.Info("Adding shotgun controller");
                            controller = target.healthComponent.body.gameObject.AddComponent<ShotgunController>();
                            controller.pivotTransform = FindModelChild("FootL");
                            controller.Pinnable = true;
                            controller.yusukeBody = characterBody;
                        }
                        else
                        {
                            isMostLikelyFlyingEnemy = true;
                            slowDownStopWatch = slowDownDuration + 1;
                        }

                    }

                    
                }
                // the knockback controller that is on the body will have an effect on the characters rotation and enabling. So remove it after adding the Shotgun controller
                KnockbackController knockBack = target.healthComponent.body.gameObject.GetComponent<KnockbackController>();
                if (knockBack) knockBack.ForceDestory();

            }
        }

        private void SlowDown()
        {

            // slows down the velocity Yusuke is traveling
            slowDownStopWatch += GetDeltaTime();
            float decelerateValue = 0.95f;
            float finalVal = 0.75f;

            float time = slowDownStopWatch / slowDownDuration;
            float val = Mathf.Lerp(decelerateValue, finalVal, time);
            Log.Info("Value: " + val);
            characterMotor.velocity *= val;
            
        }

        private void ShootShotGun(List<HurtBox> targets)
        {
            if (!hasFiredShotgun)
            {
                hasFiredShotgun = true;

                if (spiritShotGunChargeEffectPotentObject) EntityState.Destroy(spiritShotGunChargeEffectPotentObject);

                if (!hasPlayedFinishAnimation)
                {
                    hasPlayedFinishAnimation = true;
                    PlayAnimation("FullBody, Override", "ShotgunFollowUpFinish", "ShootGun.playbackRate", 1f);
                }

                SendDiagonally();

                Log.Info("Firing shotgun");
                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(spiritGunMuzzleFlashPrefab, gameObject, divePunchCenter, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                Ray aimRay = GetAimRay();

                int damageDivision = targets.Count;
                

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
                        tracerEffectPrefab = spiritTracerEffect,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = spiritShotGunExplosionHitEffect,

                    }.Fire();
                }
            }
        }

        // send Yusuke diagonally backwards based on the character direciton
        private void SendDiagonally()
        {
            
            if (isMostLikelyFlyingEnemy) 
            { 
                characterMotor.velocity = (transform.up) * 40f;
            }
            else
            {
                characterMotor.velocity = (-characterDirection.forward + transform.up) * 20f;
            }
                
        }

        public void SearchForTargets(out List<HurtBox> currentHurtbox)
        {

            Log.Info("Searching for targets");
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
            
            spiritShotGunChargeEffectPotentObject.SetActive(false);
            if (!target.healthComponent.alive) 
            {   // if the enemy is killed whilst the dash is happening, then simple exit the state.
                outer.SetNextState(new RevertSkills
                {
                    moveID = ID

                });    
            }

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
                    colliders = Physics.OverlapSphere(transform.position, 4, LayerIndex.entityPrecise.mask);
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
            return InterruptPriority.Death;
        }


    }
}
