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

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    public class DivePunch : BaseSkillState
    {


        private float duration = 2;
        private float fireTime;
        
        public float charge;

        private float speed = 10f;
        private bool beginDive;
        private bool SkipDive;
        private bool playAnim;

        private bool resetY;


        public int ID;
        public HurtBox target;
        public Collider enemyCollider;
        private DivePunchController DivePunchController;
        private KnockbackController knockbackController;
        private PinnableList pinnableList;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private Vector3 vector;



        private OverlapAttack attack;
        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float pushForce = 350f;
        protected float radius = 6f;
        public GameObject hitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex impactSound = YusukeAssets.swordHitSoundEvent.index;
        protected string hitboxGroupName = "SwordGroup";


        private bool hasBarrageFinished;
        private float punchStopwatch;
        private float punchCount = 0;
        private int maxPunches = 6;


        // Animation flags used to play animations when needed. 
        private bool hasStartUpPlayed;
        private bool isFinalPunchAnimationActive;
        private float finalPunchDelayStopwatch = 0f;
        private float groundedAnimationStartupDelayValue = 0.6f;
        private float groundedAnimationStopwatch;

        public override void OnEnter()
        {
            base.OnEnter();     

            if (ID != 0)
            {
                characterMotor.enabled = true;
                characterDirection.enabled = true;
                //Log.Info("[DIVE PUNCH] - Creating controller");
                DivePunchController = new DivePunchController();

                pinnableList = new PinnableList();
                pinnableList = gameObject.AddComponent<PinnableList>();

                //Log.Info("[DIVE PUNCH] - Adding to enemy.");
                DivePunchController = target.healthComponent.body.gameObject.AddComponent<DivePunchController>();

                characterMotor.Motor.ForceUnground();

                knockbackController = new KnockbackController();

            }
            

            
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.enabled = true;
            characterDirection.enabled = true;


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (ID != 0)
            {

                if(!beginDive && !SkipDive) DashTowardsEnemy();

                if (beginDive)
                {
                    SlowDownAndDescend();

                    if(!playAnim)
                    {
                        PlayAnimation("FullBody, Override", "DivePunch", "Roll.playbackRate", 0.5f);
                        playAnim = true;
                    }


                    if (fixedAge >= fireTime)
                    {


                        if (isGrounded && fixedAge > 0.2f)
                        {
                            DivePunchController.hasLanded = true;
                            characterMotor.enabled = false;
                            characterDirection.enabled = false;


                            if (!hasBarrageFinished)
                            {
                                //Log.Info("ATTAACKING");
                                MachineGunPunch(charge);

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
                        MachineGunPunch(charge);

                    }
                }

                if (fixedAge >= duration && isAuthority && hasBarrageFinished)
                {

                    Log.Info("Attack compolete");
                    if(target.healthComponent.alive) knockbackController = target.healthComponent.body.gameObject.GetComponent<KnockbackController>();
                    if (knockbackController)
                    {
                        Log.Info("Now deleting knockback controller");
                        knockbackController.ForceDestory();

                    }
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
        private void DashTowardsEnemy()
        {
            //Log.Info("Capturing target");

            if(ID != 0)
            {
                PlayAnimation("FullBody, Override", "Dash", "Roll.playbackRate", duration);
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
                                if (pinnableList)
                                {
                                    if (!pinnableList.CheckIfNotPinnable(target.healthComponent.gameObject.name)){
                                        Log.Info("This character is not in the list, dive");
                                        DivePunchController.pivotTransform = FindModelChild("HandR");
                                        DivePunchController.centerOfCollider = result.bounds.center;
                                        DivePunchController.Pinnable = true;
                                        beginDive = true;
                                        break;
                                    }
                                    else
                                    {
                                        Log.Info("This character is  in the list, punch instead");
                                        DivePunchController.pivotTransform = target.gameObject.transform;
                                        DivePunchController.hasLanded = true;
                                        DivePunchController.Pinnable = false;
                                        DivePunchController.centerOfCollider = result.bounds.center;
                                        SkipDive = true;
                                    }
                                    
                                }
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


        private void MachineGunPunch(float charge)
        {
            punchStopwatch += GetDeltaTime();
            groundedAnimationStopwatch += GetDeltaTime();   // stopwatch that is used for allowing the animation to play out before the next animation interrupts 

            if (target.healthComponent.alive)
            {
                //If grounded, it will play the animation startup
                if (!hasStartUpPlayed && !SkipDive)
                {
                    hasStartUpPlayed = true;
                    PlayAnimation("FullBody, Override", "DiveMachinePunchStartup", "Roll.playbackRate", duration);
                }

                if (punchStopwatch > 0.2f && !isFinalPunchAnimationActive) // the boolean checks if the final animation is being played, that way it won't be interropted by the animations below. 
                {
                    if(!SkipDive && groundedAnimationStopwatch > groundedAnimationStartupDelayValue)    // once the animation reaches that value, it will then play the next animations
                    {
                        if (NetworkServer.active)
                        {
                            PlayAnimation("FullBody, Override", "DiveMachinePunchGrounded", "Roll.playbackRate", duration);

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
            
            if(!target.healthComponent.alive) hasBarrageFinished = true;

            if(punchCount == maxPunches-1 && target.healthComponent.alive)
            {
                groundedAnimationStopwatch = 0f; // stopping the punch stopwatch, not really needed but whatever
                DeliverFinalPunch();    // final punch

                if (ID != 0 && hasBarrageFinished)
                {
                    Log.Info("TOTAL PUNCHES: "+punchCount);
                    Log.Info("BARAGE HAS BEEN COMPLETE, REMOVING DIVE COTROLLER");
                    if(beginDive) DivePunchController.EnemyRotation(DivePunchController.modelTransform, false);
                    Log.Info("First deleting dive punch");
                    if(target.healthComponent.alive) DivePunchController.Remove();
                }
            }

        }


        private void ThrowPunch()
        {
            EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
            {
                origin = target.gameObject.transform.position,
                scale = 8f
            }, transmit: true);

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
            if (finalPunchDelayStopwatch > 0.8f) hasBarrageFinished = true; 
            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
