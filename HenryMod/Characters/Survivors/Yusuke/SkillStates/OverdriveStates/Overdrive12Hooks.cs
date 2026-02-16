using EntityStates;
using Newtonsoft.Json.Bson;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;
using static RoR2.SolusWing.SolusWingPodAI.Simulation.SimulationState;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates
{
    internal class Overdrive12Hooks : BaseSkillState
    {

        private float duration = 1f;
        private float overdriveTimeDuration;
        private float overdriveFullDuration = 9.5f;
        private float initialSwing = 0.7f;
        private float initialFinalSwing = 6f;
        private float swingTimer = 0f;
        private float swingInterval = 0.38f;
        private byte swingCount;
        private byte maxSwingCount = 11;

        private HurtBox enemyHurtBox;
        public CharacterBody enemyBody;
        private Hook12Tracking hookTracking;
        private bool hasTargetBeenFound;
        private GameObject vanishLinesPrefab;
        private bool hasThrownUppercut;

        private bool isMotorEnemy;
        private bool shouldReturn;

        private CharacterMotor enemyMotor;
        private Rigidbody enemyRigidBody;

        private Vector3 forwardDirection;
        private Ray yDistanceRay;
        private RaycastHit hit;
        private float maxDistance;
        private bool hasSnappedToGround;
        private readonly string vanishLineLocation = "Chest";
        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string dashCenter = "dashCenter";
        private readonly string hitboxGroupName = "MeleeGroup";
        private readonly string finalHookLocation = "overdrive12HookFinalHit";


        private OverlapAttack attack;
        private DamageType damageType = DamageType.Generic;
        private float damageCoefficient = 3.5f;
        private float procCoefficient = 1f;
        private float pushForce = 300f;
        private Vector3 bonusForce = Vector3.zero;
        private float baseDuration = 1f;
        private GameObject swingEffectPrefab;
        private GameObject hitEffectPrefab;
        private GameObject finalHitEffectPrefab;
        private GameObject heavyHitEffectPrefab;

        private YusukeWeaponComponent yusukeWeaponComponent;
        

        public override void OnEnter()
        {
            base.OnEnter();
            Log.Info("Inside on enter OVERDRIVE 12 HOOK");

            if (!isGrounded)
            {
                Chat.AddMessage("Need to be grounded for the OVERDRIVE: 12 HOOK COMBO");
                Log.Warning("Need to be grounded for the OVERDRIVE: 12 HOOK COMBO");
                shouldReturn = true;

            }
            else
            {
                vanishLinesPrefab = YusukeAssets.vanishLinesWhite;
                hitEffectPrefab = YusukeAssets.hitImpactEffect;
                finalHitEffectPrefab = YusukeAssets.finalHitEffect;
                heavyHitEffectPrefab = YusukeAssets.heavyHitRingEffect;

                SetUpEffects();
                forwardDirection = GetAimRay().direction;

                hookTracking = gameObject.GetComponent<Hook12Tracking>();
                enemyHurtBox = hookTracking.GetTrackingTarget();
                if (enemyHurtBox)
                {
                    enemyBody = enemyHurtBox.healthComponent.body;

                    if (enemyBody)
                    {
                        if (CheckIfEnemyisAlive() && isGrounded)
                        {
                            if (!CheckForFlyingType())
                            {

                                EffectManager.SpawnEffect(vanishLinesPrefab, new EffectData
                                {
                                    origin = transform.position,
                                    scale = 1f
                                }, transmit: true);

                                if (CheckForGround())
                                {
                                    TeleportToTarget();
                                    hasTargetBeenFound = true;
                                    StunEnemy();
                                }
                                else
                                {
                                    if (enemyBody.isFlying)
                                    {
                                        MoveTargetToPlayer(true);
                                        hasTargetBeenFound = true;
                                        StunEnemy();
                                    }
                                    else
                                    {
                                        Log.Info("No ground undereath enemy");
                                        shouldReturn = true;
                                    }

                                }

                                // use a ray after to set the character to the ground
                            }
                            else
                            {
                                EffectManager.SpawnEffect(vanishLinesPrefab, new EffectData
                                {
                                    origin = enemyBody.healthComponent.body.gameObject.transform.position,
                                    scale = 1f
                                }, transmit: true);


                                MoveTargetToPlayer(false);
                                hasTargetBeenFound = true;
                                StunEnemy();

                            }
                        }
                        else
                        {
                            shouldReturn = true;
                        }

                    }

                }
                else
                {
                    Log.Error("No enemy found. ");
                    shouldReturn = true;
                }

                if (hasTargetBeenFound) 
                {
                    PlayAnimation("FullBody, Override", "Overdrive12Hooks", "Slide.playbackRate", duration);
                    //Util.PlaySound("Play_SoundOverdrive12Hooks", gameObject);

                    yusukeWeaponComponent = characterBody.gameObject.GetComponent<YusukeWeaponComponent>();
                    yusukeWeaponComponent.SetOverdriveState(true);
                }

                

            }

        }

        private void StunEnemy()
        {
            enemyBody.healthComponent.GetComponent<SetStateOnHurt>()?.SetStunInternal(14);
        }

        /*private void SnapToGround()
        {
            if (!hasSnappedToGround)
            {
                Log.Info("Snapping");
                yDistanceRay = new Ray(transform.position, Vector3.down);
                maxDistance = 50f;
                if (Physics.Raycast(yDistanceRay, out hit, maxDistance))
                {
                    TeleportHelper.TeleportBody(characterBody, hit.point, forceOutOfVehicle: true);
                }
                hasTargetBeenFound = true;
            }
            


        }*/

        private bool CheckForGround()
        {
            if (enemyMotor.isGrounded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetUpEffects()
        {
            vanishLinesPrefab.AddComponent<DestroyOnTimer>().duration = 1;
            hitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1;
            finalHitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;

            EffectComponent component = vanishLinesPrefab.GetComponent<EffectComponent>();
            if (component)
            {
                component.parentToReferencedTransform = false;

            }
        }

        private void ReturnToMain()
        {
            skillLocator.primary.AddOneStock();
            outer.SetNextStateToMain();
            return;
        }

        private bool CheckIfEnemyisAlive()
        {
            if (enemyBody && enemyBody.healthComponent && enemyBody.healthComponent.alive)
            {
                if (!enemyBody.healthComponent.body.isChampion || !enemyBody.healthComponent.body.isBoss)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        private bool CheckForFlyingType()
        {
            enemyMotor = enemyBody.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
            enemyRigidBody = enemyBody.healthComponent.body.gameObject.GetComponent<Rigidbody>();

            if (enemyRigidBody)
            {
                if (enemyMotor)
                {
                    enemyMotor.disableAirControlUntilCollision = true;
                    enemyMotor.velocity = Vector3.zero;
                    enemyMotor.rootMotion = Vector3.zero;
                    isMotorEnemy = true;
                    return false;

                }
                else
                {
                    enemyRigidBody.velocity = Vector3.zero;
                    isMotorEnemy = false;
                    return true;
                }

            }
            return true;
        }

        private void TeleportToTarget()
        {
            characterMotor.rootMotion += enemyBody.gameObject.transform.position - transform.position;

        }

        private void MoveTargetToPlayer(bool hasException)
        {
            if (hasException)
            {
                enemyBody.gameObject.transform.position = FindModelChild(muzzleCenter).transform.position;
            }
            else
            {
                Rigidbody enemyRigid = enemyBody.rigidbody;
                if (rigidbody)
                {
                    enemyRigid.position = FindModelChild(muzzleCenter).transform.position;
                }
            }
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!enemyBody || shouldReturn)
            {
                ReturnToMain();
            }
            else
            {
                //SnapToGround();

                characterMotor.velocity.y = -400;

                overdriveTimeDuration += GetDeltaTime();
                swingTimer += GetDeltaTime();
                ThrowPunchAtEnemy();

                Log.Info("overdriveTime: " + overdriveTimeDuration);

                if (isAuthority && characterMotor && hasTargetBeenFound)
                {
                    if (overdriveTimeDuration > 0.2)
                    {
                        characterMotor.velocity = Vector3.zero;
                    }
                    else
                    {
                        characterMotor.velocity.x = 0;
                        characterMotor.velocity.z = 0;
                    }
                    

                    inputBank.moveVector = Vector3.zero;
                    characterMotor.moveDirection = Vector3.zero;
                    /*characterMotor.velocity = Vector3.zero;*/

                    characterDirection.forward = forwardDirection;
                    characterDirection.moveVector = forwardDirection;

                    characterMotor.moveDirection = inputBank.moveVector;
                    characterDirection.moveVector = characterMotor.moveDirection;


                }
                FreezeEnemy();

                if(overdriveTimeDuration > initialFinalSwing)
                {
                    ThrowUpperCut();
                }


                if (isAuthority && fixedAge >= duration)
                {
                    if (overdriveTimeDuration > overdriveFullDuration)
                    {
                        outer.SetNextStateToMain();
                    }
                }

            }

        }

        private void ThrowUpperCut()
        {
            if (!hasThrownUppercut)
            {
                hasThrownUppercut = true;

                EffectManager.SimpleMuzzleFlash(finalHitEffectPrefab, gameObject, finalHookLocation, false);
                EffectManager.SimpleMuzzleFlash(heavyHitEffectPrefab, gameObject, finalHookLocation, false);

                // sends the enemy upwards
                Vector3 forceVector = (characterDirection.forward + transform.up);
                
                if (enemyBody.isFlying)
                {
                    forceVector *= 8000f;
                }
                else
                {
                    if(enemyMotor.mass > 200)
                    {
                        forceVector *= 20000f;
                    }
                    else
                    {
                        forceVector *= 10000f;
                    }
                    
                }
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
            enemyBody.healthComponent.TakeDamage(damageInfo);
            
        }

        private void ThrowPunchAtEnemy()
        {
            if(overdriveTimeDuration > initialSwing)
            {
                if(swingTimer > swingInterval && swingCount < maxSwingCount)
                {
                    swingTimer = 0;
                    swingCount++;

                    attack = new OverlapAttack();
                    attack.damageType = damageType;
                    attack.attacker = gameObject;
                    attack.inflictor = gameObject;
                    attack.teamIndex = GetTeam();
                    attack.damage = damageCoefficient * damageStat;
                    attack.procCoefficient = procCoefficient;
                    attack.hitEffectPrefab = hitEffectPrefab;
                    attack.forceVector = bonusForce;
                    attack.pushAwayForce = pushForce;
                    attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
                    attack.isCrit = RollCrit();

                    EffectManager.SimpleMuzzleFlash(hitEffectPrefab, gameObject, muzzleCenter, false);
                }
            }
        }

        private void FreezeEnemy()
        {
            if(overdriveTimeDuration < initialFinalSwing)
            {
                if (isMotorEnemy)
                {
                    enemyMotor.disableAirControlUntilCollision = true;
                    enemyMotor.velocity = Vector3.zero;
                    enemyMotor.rootMotion = Vector3.zero;
                    enemyBody.inputBank.enabled = false;

                    enemyMotor.enabled = false;
                    enemyBody.characterMotor.enabled = false;
                    enemyBody.characterDirection.forward = -forwardDirection;
                }
                else
                {
                    enemyRigidBody.velocity = Vector3.zero;
                    enemyBody.inputBank.enabled = false;
                    rigidbody.position = FindModelChild(muzzleCenter).transform.position;
                }
            }
            else
            {
                if (enemyMotor)
                {
                    enemyMotor.enabled = true;
                    enemyBody.characterMotor.enabled = true;
                }
            }
            
        }

        public override void OnExit()
        {
            base.OnExit();

            PlayAnimation("FullBody, Override", "BufferEmpty", "Slide.playbackRate", duration);

            Log.Info("Exiting 12 hook");
            if (characterMotor && characterDirection)
            {
                characterMotor.enabled = true;
                characterDirection.enabled = true;
            }

            yusukeWeaponComponent = gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetOverdriveState(false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
