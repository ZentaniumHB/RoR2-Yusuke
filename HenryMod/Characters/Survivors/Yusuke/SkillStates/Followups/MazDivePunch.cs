using EntityStates;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Survivors.Yusuke;
using EntityStates.Croco;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using System.Linq;
using UnityEngine.UIElements.UIR;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine.Networking;
using RoR2.Audio;
using Random = UnityEngine.Random;
using UnityEngine.Networking.NetworkSystem;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    public class MazDivePunch : BaseSkillState
    {

        private float duration = 2;
        private float fireTime;

        public float charge = 2f;

        private float speed = 10f;
        private bool beginDive;
        private bool SkipDive;
        private bool playAnim;
        private byte attackFinisherID;
        
        private bool resetY;
        private float velocityDivider = 0.1f;

        public int ID;
        public HurtBox target;
        public Collider enemyCollider;
        private DivePunchController divePunchController;
        private KnockbackController knockbackController;
        private PinnableList pinnableList;


        private OverlapAttack attack;
        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float pushForce = 350f;
        protected float radius = 6f;
        public GameObject hitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex impactSound = YusukeAssets.swordHitSoundEvent.index;
        protected string hitboxGroupName = "SwordGroup";


        private OverlapAttack stompAttack;
        protected DamageType stompDamageType = DamageType.Stun1s;
        protected float stompDamageCoefficient = 8f;
        protected float stompProcCoefficient = 1f;
        protected float stompPushForce = 350f;
        protected float stompRadius = 60f;
        public GameObject stompHitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex stompImpactSound = YusukeAssets.swordHitSoundEvent.index;
        protected string stompHitboxGroupName = "SwordGroup";


        private bool hasBarrageFinished;
        private float punchStopwatch;
        
        private float stompStopwatch;
        private float punchReset = 0.05f;
        private float stompReset = 0.5f;
        private float punchCount = 0;
        private float stompCount = 0;
        private int maxPunches = 40;
        private int maxStomps = 4;
        private float finalPunchStartup = 0.6f;   // for the final punch animation
        private float finalKickStartup = 0.8f;   // for the final punch animation


        // Animation flags

        private float groundedAnimationStartupDelayValue = 0.6f;
        private float groundedAnimationStopwatch;
        private bool hasStartUpPlayed;

        private bool isFinalPunchAnimationActive;
        private float finalPunchDelayStopwatch = 0f;
        private bool rapidPunchAnim;

        private bool isFinalKickAnimationActive;
        private float finalKickDelayStopwatch = 0f;

        private float animationEndLag;


        // Net
        private bool hasAppliedStun;
        private bool hasAppliedForce;


        //effects 
        public GameObject hitImpactEffectPrefab;
        public GameObject punchBarragePrefab;
        public GameObject heavyHitEffectPrefab;
        public GameObject heavyHitEffectFollowPrefab;
        public GameObject finalHitEffectPrefab;
        private GameObject stompEffectPrefab;

        private GameObject shadowDashEffectPrefab;
        private bool hasAppliedPunchEffect;

        private readonly string dashCenter = "dashCenter";
        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string divePunchCenter = "divePunchCenter";
        private readonly string mainPosition = "mainPosition";


        private YusukeWeaponComponent yusukeWeaponComponent;

        public override void OnEnter()
        {
            base.OnEnter();

            hitImpactEffectPrefab = YusukeAssets.hitImpactEffect;
            punchBarragePrefab = YusukeAssets.punchBarrageFastEffect;
            heavyHitEffectPrefab = YusukeAssets.heavyHitRingEffect;
            heavyHitEffectFollowPrefab = YusukeAssets.heavyHitRingFollowingEffect;
            finalHitEffectPrefab = YusukeAssets.finalHitEffect;
            stompEffectPrefab = YusukeAssets.stompEffect;


            shadowDashEffectPrefab = YusukeAssets.shadowDashSK1;


            characterMotor.enabled = true;
            characterDirection.enabled = true;
            //Log.Info("[DIVE PUNCH] - Creating controller");
            divePunchController = new DivePunchController();

            pinnableList = new PinnableList();
            pinnableList = gameObject.AddComponent<PinnableList>();

            divePunchController = target.healthComponent.body.gameObject.AddComponent<DivePunchController>();

            characterMotor.Motor.ForceUnground();

            knockbackController = new KnockbackController();
            attackFinisherID = (byte)Random.Range(1, 3); // pick between 1 or 2, it determines the type of animation/attack is done. 


            yusukeWeaponComponent = gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetFollowUpBoolean(true);

            EditAttackEffects();
            EffectManager.SimpleMuzzleFlash(shadowDashEffectPrefab, gameObject, dashCenter, true);

        }

        private void EditAttackEffects()
        {
            hitImpactEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1;

            float duration = punchReset * (maxPunches - 1);
            punchBarragePrefab.AddComponent<DestroyOnTimer>().duration = duration + 0.1f;

            heavyHitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;
            heavyHitEffectFollowPrefab.AddComponent<DestroyOnTimer>().duration = 2f;
            shadowDashEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;
            stompEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;

            EffectComponent component2 = heavyHitEffectPrefab.GetComponent<EffectComponent>();
            if (component2) component2.parentToReferencedTransform = false;

            finalHitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;
        }

        public override void OnExit()
        {
            base.OnExit();

            // this is needed since the animations here has no transition connection, without it there is a chance that an animation will loop forever, unless interrupted again
            PlayAnimation("FullBody, Override", "BufferEmpty", "ThrowBomb.playbackRate", 1f);   
            characterMotor.enabled = true;
            characterDirection.enabled = true;

            // so the character doesn't go flying like crazy due to the velocity, the current velocity will be divided by the given percentage decimal (velocityDivider). 
            Vector3 currentVelocity = characterMotor.velocity;
            Vector3 velocityPercentage = currentVelocity * velocityDivider;
            characterMotor.velocity = velocityPercentage;

            yusukeWeaponComponent = gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetFollowUpBoolean(false);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

             

            if (!beginDive && !SkipDive) DashTowardsEnemy();

            if (beginDive)
            {
                SlowDownAndDescend();

                if (!playAnim)
                {
                    if (attackFinisherID == 1) PlayAnimation("FullBody, Override", "DivePunch", "Roll.playbackRate", 0.5f);
                    if (attackFinisherID == 2) PlayAnimation("FullBody, Override", "StompDive", "Roll.playbackRate", 0.5f);

                    EffectManager.SimpleMuzzleFlash(hitImpactEffectPrefab, gameObject, divePunchCenter, false);
                    EffectManager.SimpleMuzzleFlash(heavyHitEffectFollowPrefab, gameObject, divePunchCenter, false);
                    EffectManager.SimpleMuzzleFlash(stompEffectPrefab, gameObject, divePunchCenter, false);

                    playAnim = true;
                }

                if (fixedAge >= fireTime)
                {
                    if (isGrounded && fixedAge > 0.2f)
                    {
                        divePunchController.hasLanded = true;
                        characterMotor.enabled = false;
                        characterDirection.enabled = false;

                        ApplyStun();

                        if (!hasBarrageFinished)
                        {
                            if (attackFinisherID == 1) MachineGunPunch();
                            if (attackFinisherID == 2) StompThemOut();

                        }
                    }
                }
            }

            if (SkipDive)
            {
                if (!hasBarrageFinished)
                {
                    //Log.Info("ATTAACKING");
                    characterMotor.enabled = false;
                    characterDirection.enabled = false;
                    MachineGunPunch();

                }
            }

            if (fixedAge >= duration && isAuthority && hasBarrageFinished)
            {
                Log.Info("Attack complete");
                if (target.healthComponent.alive) knockbackController = target.healthComponent.body.gameObject.GetComponent<KnockbackController>();
                if (knockbackController)
                {
                    Log.Info("Now deleting knockback controller");
                    knockbackController.ForceDestory();

                }

                inputBank.moveVector = Vector3.zero;

                // for both animations there are different cooldowns for the animations, these timers will delay the next state to be active (the main state)
                animationEndLag += GetDeltaTime();
                if (attackFinisherID == 1)
                {
                    if (animationEndLag > 0.8f) outer.SetNextStateToMain();
                }
                else
                {
                    if (animationEndLag > 1.8f) outer.SetNextStateToMain();
                }
                

            }

        }

        private void ApplyStun()
        {
            if (!hasAppliedStun)
            {
                hasAppliedStun = true;
                if (NetworkServer.active)
                {
                    /* the sum is based on the amount of time the enemy is pinned by Yusuke (once landed on the ground). Including all the startup and punch animation intervals 
                     *  In this case the stun will vary for each attack with a different ID.
                     */
                    float stunDuration = 0;
                    if(attackFinisherID == 1) stunDuration = (maxPunches * punchReset) + (punchReset + finalPunchStartup);
                    if(attackFinisherID == 2) stunDuration = (maxStomps * stompReset) + (stompStopwatch);

                    target.healthComponent.GetComponent<SetStateOnHurt>()?.SetStunInternal(stunDuration);

                    // for bosses, the state needs to be set manually as they lack the SetStateOnHurt component
                    if (target.healthComponent.body.isChampion || target.healthComponent.body.isChampion)
                    {
                        EntityStateMachine enemyMachine = target.healthComponent.body.GetComponent<EntityStateMachine>();
                        if (enemyMachine != null)
                        {
                            Log.Info("Setting the state for boss. ");
                            StunState stunState = new StunState();
                            stunState.duration = stunDuration;
                            enemyMachine.SetState(stunState);
                        }
                    }

                }
            }
        }

        private void DashTowardsEnemy()
        {
            //Log.Info("Capturing target");

            PlayAnimation("FullBody, Override", "Dash", "Roll.playbackRate", duration);

            if (!target.healthComponent.alive) outer.SetNextStateToMain();    // if the enemy is killed whilst the dash is happening, then simple exit the state.

            CharacterMotor enemyMotor = target.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
            Rigidbody enemyRigidBody = target.healthComponent.body.gameObject.GetComponent<Rigidbody>();


            if (enemyRigidBody)
            {   // I believe most if not all enemies have a rigid body so... if there is a motor, then teleport; otherwise dash instead.
                if (enemyMotor) 
                {
                    characterMotor.rootMotion += target.gameObject.transform.position - transform.position;
                }
                else
                {   // setting the attackFinisher to 1 will ensure it does finish the attack, since non-motor characters will always be punched
                    attackFinisherID = 1;
                    if (characterMotor && characterDirection)
                    {
                        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
                        // Calculate the velocity in the direction of the target
                        Vector3 forwardSpeed = directionToTarget * (speed * moveSpeedStat) * charge;
                        // Apply the velocity to the character's motor
                        characterMotor.velocity = forwardSpeed;

                    }
                }
            }

            
            Log.Info("Attack finisher: " + attackFinisherID);

            FindMatchingHurtbox();
        }

        private void FindMatchingHurtbox()
        {
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
                        CharacterMotor enemyMotor = target.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
                        Rigidbody enemyRigidBody = target.healthComponent.body.gameObject.GetComponent<Rigidbody>();

                        if (enemyRigidBody)
                        {
                            if (enemyMotor)
                            {
                                if (attackFinisherID == 1) divePunchController.pivotTransform = FindModelChild("HandR");
                                if (attackFinisherID == 2) divePunchController.pivotTransform = FindModelChild("FootR");
                                divePunchController.Pinnable = true;
                                divePunchController.yusukeBody = characterBody;
                                beginDive = true;
                                break;
                            }
                            else
                            {
                                divePunchController.pivotTransform = target.gameObject.transform;   // maybe make an empty object on the player and make it refernce it
                                divePunchController.hasLanded = true;
                                divePunchController.Pinnable = false;
                                SkipDive = true;
                                break;
                            }

                        }
                    }


                }
            }
        }

        private void SlowDownAndDescend()
        {
            Vector3 slowedVelocity = characterMotor.velocity;
            float slowValue = 2f;
            // Slow down the X and Z axes

            Vector3 direction = GetAimRay().direction;
            direction.y = Mathf.Max(direction.y, 1);

            slowedVelocity.x = Mathf.Lerp(slowedVelocity.x, 0f, slowValue * Time.deltaTime);
            slowedVelocity.z = Mathf.Lerp(slowedVelocity.z, 0f, slowValue * Time.deltaTime);
            Vector3 vector3 = new Vector3(direction.x, 0f, direction.z).normalized * 1.5f;

            // resetting the Y value so the velocity doesn't send them too high
            if (!resetY)
            {
                resetY = true;
                slowedVelocity.y = 0f;
            }

            // Set the updated velocity back
            characterMotor.velocity = slowedVelocity + vector3;
        }


        private void MachineGunPunch()
        {
            punchStopwatch += GetDeltaTime();
            groundedAnimationStopwatch += GetDeltaTime(); // stopwatch that is used for allowing the animation to play out before the next animation interrupts

            if (target.healthComponent.alive)
            {
                //If grounded, it will play the animation startup
                if (punchStopwatch > punchReset && !isFinalPunchAnimationActive) // reset the timer, and attack again
                {
                    if (!hasStartUpPlayed && !SkipDive)
                    {
                        hasStartUpPlayed = true;
                        PlayAnimation("FullBody, Override", "DiveMachinePunchStartup", "Roll.playbackRate", duration);
                    }

                    if (!SkipDive && groundedAnimationStopwatch > groundedAnimationStartupDelayValue)
                    {
                        if (NetworkServer.active)
                        {
                            // this boolean is to allow the animation to play only once, since it is not connected to the "bufferEmpty" state in the animation layer, it will constantly loop the first few frames
                            if (!rapidPunchAnim)
                            {
                                rapidPunchAnim = true;
                                PlayAnimation("FullBody, Override", "MazokuDiveMachinePunchGrounded", "MazokuMachinePunch.playbackRate", 0.3f);
                            }

                        }
                        ThrowPunch();
                    }

                    if (SkipDive)
                    {
                        PlayAnimation("FullBody, Override", "DiveMachinePunchAir", "Roll.playbackRate", duration);
                        ThrowPunch();
                        Log.Info("barraged finished: " + hasBarrageFinished);
                    }



                }


            }

            // when the target is not alive, then simply skip everything and exit the state.
            if (!target.healthComponent.alive) hasBarrageFinished = true;

            // if the max punches is reached, or the enemy is killed, make the boolean true. Once true, it will return
            if (attackFinisherID == 1 && punchCount == maxPunches - 1 && target.healthComponent.alive)
            {
                DeliverFinalPunch();
                if (hasBarrageFinished)
                {
                    Log.Info("TOTAL PUNCHES: " + punchCount);
                    RemoveControllerOnEnemy();
                }


            }

        }

        private void StompThemOut()
        {
            stompStopwatch += GetDeltaTime();
            groundedAnimationStopwatch += GetDeltaTime(); // stopwatch that is used for allowing the animation to play out before the next animation interrupts

            if (target.healthComponent.alive)
            {
                if (!hasStartUpPlayed && !SkipDive)
                {
                    hasStartUpPlayed = true;
                    PlayAnimation("FullBody, Override", "StompDiveStartUp", "Roll.playbackRate", duration);
                }

                if (stompStopwatch > stompReset && !isFinalKickAnimationActive)
                {
                    if (!SkipDive && groundedAnimationStopwatch > groundedAnimationStartupDelayValue)
                    {
                        if (NetworkServer.active)
                        {
                            // this boolean is to allow the animation to play only once, since it is not connected to the "bufferEmpty" state in the animation layer, it will constantly loop the first few frames
                            if (!rapidPunchAnim)
                            {
                                rapidPunchAnim = true;
                                PlayAnimation("FullBody, Override", "StompThemOut", "ThrowBomb.playbackRate", 1f);
                            }

                        }
                        StompAttack();
                    }

                    /*if ()
                    {
                        if (NetworkServer.active)
                        {
                            PlayAnimation("FullBody, Override", "StompThemOut", "ThrowBomb.playbackRate", 1f);
                        }
                        StompAttack();

                    }*/
                }
                
            }

            // when the target is not alive, then simply skip everything and exit the state.
            if (!target.healthComponent.alive) hasBarrageFinished = true;

            Log.Info("stompCount: " + stompCount);

            if (attackFinisherID == 2 && stompCount >= maxStomps - 1 && target.healthComponent.alive)
            {
                DeliverFinalKick();
                if (hasBarrageFinished)
                {
                    Log.Info("TOTAL Stomps: " + stompCount);
                    RemoveControllerOnEnemy();

                }
            }

        }

        private void RemoveControllerOnEnemy()
        {
            Log.Info("BARAGE HAS BEEN COMPLETE, REMOVING DIVE COTROLLER");
            if (beginDive) divePunchController.EnemyRotation(divePunchController.modelTransform, false);
            Log.Info("First deleting dive punch");
            if (target.healthComponent.alive) divePunchController.Remove();
        }

        private void ThrowPunch()
        {
            EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
            {
                origin = target.gameObject.transform.position,
                scale = 8f
            }, transmit: true);

            if (!hasAppliedPunchEffect)
            {
                hasAppliedPunchEffect = true;
                EffectManager.SimpleMuzzleFlash(punchBarragePrefab, gameObject, divePunchCenter, false);


            }

            attack = new OverlapAttack
            {
                damageType = damageType,
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = GetTeam(),
                damage = damageCoefficient * damageStat + (charge / 6),
                procCoefficient = procCoefficient,
                hitEffectPrefab = hitEffectPrefab,
                pushAwayForce = pushForce,
                hitBoxGroup = FindHitBoxGroup(hitboxGroupName),
                isCrit = RollCrit(),
                impactSound = impactSound
            };
            attack.Fire();
            punchCount++;
            punchStopwatch = 0;
        }

        private void StompAttack()
        {
            EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
            {
                origin = target.gameObject.transform.position,
                scale = 8f
            }, transmit: true);

            EffectManager.SimpleMuzzleFlash(stompEffectPrefab, gameObject, mainPosition, false);

            stompAttack = new OverlapAttack
            {
                damageType = stompDamageType,
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = GetTeam(),
                damage = stompDamageCoefficient * damageStat,
                procCoefficient = stompProcCoefficient,
                hitEffectPrefab = stompHitEffectPrefab,
                pushAwayForce = stompPushForce,
                hitBoxGroup = FindHitBoxGroup(stompHitboxGroupName),
                isCrit = RollCrit(),
                impactSound = stompImpactSound
            };
            stompAttack.Fire();
            stompCount++;
            stompStopwatch = 0f;
        }

        private void ApplyForce()
        {
            if (!hasAppliedForce)
            {
                hasAppliedForce = true;
                // grabbing the facing direction and applying a force to the enemy
                Vector3 forceVector = characterDirection.forward;    // for now the direction is based on the characters forward direction
                forceVector *= 20000f;
                if (target.healthComponent.body.isChampion || target.healthComponent.body.isBoss) forceVector = characterDirection.forward *= 35000f;
                knockbackController.ForceDestory(); // destroying the controller first, so it doesn't interrupt the force vector
                AttackForce(forceVector);
            }

        }

        // Applying force
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

        private void DeliverFinalPunch()
        {
            finalPunchDelayStopwatch += GetDeltaTime(); // starts counting the delay before the final punch connects
            
            // this boolean will prevent the if statement in the fixedUpate method running, which would cause conflict with the other animations
            if (!isFinalPunchAnimationActive)
            {
                isFinalPunchAnimationActive = true; 
                if (NetworkServer.active)
                {
                    if (!SkipDive)
                    {
                        PlayAnimation("FullBody, Override", "DiveMachinePunchGroundedFinsh", "Roll.playbackRate", 1);
                    }
                    else
                    {
                        PlayAnimation("FullBody, Override", "DiveMachinePunchAirFinish", "Roll.playbackRate", 1);
                    }

                }

            }
            // if and ONLY if the time passes (which should be enough time for the animation to play) will then the boolean will be true exiting the state in the fixedUpdate
            if (finalPunchDelayStopwatch > finalPunchStartup) 
            {
                ApplyForce();
                EffectManager.SimpleMuzzleFlash(finalHitEffectPrefab, gameObject, muzzleCenter, false);
                EffectManager.SimpleMuzzleFlash(heavyHitEffectPrefab, gameObject, muzzleCenter, false);
                hasBarrageFinished = true; 
            } 
            
        }

        private void DeliverFinalKick()
        {
            finalKickDelayStopwatch += GetDeltaTime(); // starts counting the delay before the final kick connects
            Log.Info("finalKickDelayStopwatch: "+ finalKickDelayStopwatch);

            // this boolean will prevent the if statement in the fixedUpate method running, which would cause conflict with the other animations
            if (!isFinalKickAnimationActive)
            {
                StompAttack();
                isFinalKickAnimationActive = true;
                if (NetworkServer.active)
                {
                    if (!SkipDive)
                    {
                        PlayAnimation("FullBody, Override", "StompFinish", "Roll.playbackRate", 1);
                    }
                    else
                    {
                        PlayAnimation("FullBody, Override", "DiveMachinePunchAirFinish", "Roll.playbackRate", 1);
                    }

                }

            }
            // if and ONLY if the time passes (which should be enough time for the animation to play) will then the boolean will be true exiting the state in the fixedUpdate
            if (finalKickDelayStopwatch > finalKickStartup)
            {
                Log.Info("Attack kick has ended, so make barrage true");
                ApplyForce();
                EffectManager.SimpleMuzzleFlash(finalHitEffectPrefab, gameObject, muzzleCenter, false);
                EffectManager.SimpleMuzzleFlash(heavyHitEffectPrefab, gameObject, muzzleCenter, false);
                hasBarrageFinished = true;
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
