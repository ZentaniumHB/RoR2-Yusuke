using EntityStates;
using RoR2.Audio;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Survivors.Yusuke;
using Random = UnityEngine.Random;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups
{
    internal class MazMeleeFollowUp : BaseSkillState
    {

        private float duration = 2;
        private float fireTime;

        public float charge;

        private float speed = 10f;
        private bool beginDive;
        private bool SkipDive;
        private bool playAnim;
        private int attackFinisherID;

        private bool resetY;


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
        private float punchCount = 0;
        private float stompCount = 0;
        private int maxPunches = 40;
        private int maxStomps = 4;


        // Animation flags
        private bool hasStartUpPlayed;
        private bool isFinalPunchAnimationActive;
        private float finalPunchDelayStopwatch = 0f;
        private float groundedAnimationStartupDelayValue = 0.6f;
        private float groundedAnimationStopwatch;
        private bool rapidPunchAnim;

        public override void OnEnter()
        {
            base.OnEnter();

            characterMotor.enabled = true;
            characterDirection.enabled = true;
            //Log.Info("[DIVE PUNCH] - Creating controller");
            divePunchController = new DivePunchController();

            pinnableList = new PinnableList();
            pinnableList = gameObject.AddComponent<PinnableList>();

            divePunchController = target.healthComponent.body.gameObject.AddComponent<DivePunchController>();

            characterMotor.Motor.ForceUnground();

            knockbackController = new KnockbackController();
            attackFinisherID = Random.Range(1, 3); // pick between 1 or 2, it determines the type of animation/attack is done. 



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

            if (!beginDive && !SkipDive) TeleportToEnemy();

            if (beginDive)
            {
                SlowDownAndDescend();

                if (!playAnim)
                {
                    if(attackFinisherID == 1) PlayAnimation("FullBody, Override", "DivePunch", "Roll.playbackRate", 0.5f);
                    if(attackFinisherID == 2) PlayAnimation("FullBody, Override", "StompDive", "Roll.playbackRate", 0.5f);
                    playAnim = true;
                }


                if (fixedAge >= fireTime)
                {


                    if (isGrounded && fixedAge > 0.2f)
                    {
                        divePunchController.hasLanded = true;
                        characterMotor.enabled = false;
                        characterDirection.enabled = false;


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

                Log.Info("Attack compolete");
                if (target.healthComponent.alive) knockbackController = target.healthComponent.body.gameObject.GetComponent<KnockbackController>();
                if (knockbackController)
                {
                    Log.Info("Now deleting knockback controller");
                    knockbackController.ForceDestory();

                }
                outer.SetNextStateToMain();

            }



        }

        

        private void TeleportToEnemy()
        {
            //Log.Info("Capturing target");

            characterMotor.rootMotion += target.gameObject.transform.position - transform.position;
            FindMatchingHurtbox();
        }

        private void FindMatchingHurtbox()
        {
            // grab the enemy collders that are nearby
            Collider[] collisions;
            collisions = Physics.OverlapSphere(transform.position, 50, LayerIndex.entityPrecise.mask);
            List<Collider> colliders = collisions.ToList();

            foreach (Collider collider in colliders)
            {
                // get the colliders hurtbox and compare whether they equal to the marked enemies hurtbox
                HurtBox capturedHurtbox = collider.GetComponent<HurtBox>();

                // check each hurtbox and catpure the hurtbox they have, then compare the two for a match.
                if (capturedHurtbox == target)
                {
                    Log.Info("Enemy hurtbox exists. Now grabbing...");

                    CharacterMotor enemyMotor = target.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
                    Rigidbody enemyRigidBody = target.healthComponent.body.gameObject.GetComponent<Rigidbody>();

                    if (enemyRigidBody)
                    {
                        if (enemyMotor)
                        {
                            if(attackFinisherID == 1) divePunchController.pivotTransform = FindModelChild("HandR");
                            if(attackFinisherID == 2) divePunchController.pivotTransform = FindModelChild("FootR");
                            divePunchController.Pinnable = true;
                            beginDive = true;
                            break;
                        }
                        else
                        {
                            Log.Info("This character is  in the list, punch instead");
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


        private void SlowDownAndDescend()
        {
            Vector3 slowedVelocity = characterMotor.velocity;
            float slowValue = 1f;
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
                if (!hasStartUpPlayed && !SkipDive && attackFinisherID == 1)
                {
                    hasStartUpPlayed = true;
                    PlayAnimation("FullBody, Override", "DiveMachinePunchStartup", "Roll.playbackRate", duration);
                }


                if (punchStopwatch > 0.05f && !isFinalPunchAnimationActive) // reset the timer, and attack again
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
            if (attackFinisherID == 1 && punchCount == maxPunches-1 && target.healthComponent.alive)
            {
                DeliverFinalPunch();
                if (hasBarrageFinished)
                {
                    Log.Info("TOTAL PUNCHES: " + punchCount);
                    Log.Info("BARAGE HAS BEEN COMPLETE, REMOVING DIVE COTROLLER");
                    if (beginDive) divePunchController.EnemyRotation(divePunchController.modelTransform, false);
                    Log.Info("First deleting dive punch");
                    if (target.healthComponent.alive) divePunchController.Remove();
                }
                
                
            }

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

        // placing the attack in a different method since the stomp and the punches will do differemt amounts of damage
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
                damage = damageCoefficient * damageStat,
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

        // stomps are different in terms of animation resets
        private void StompThemOut()
        {
            stompStopwatch += GetDeltaTime();
            if (target.healthComponent.alive)
            {
                if (stompStopwatch > 0.5f)
                {

                    if (NetworkServer.active)
                    {
                        PlayAnimation("FullBody, Override", "StompThemOut", "ThrowBomb.playbackRate", 1f);


                    }
                    StompAttack();

                }


            }

            if (attackFinisherID == 2 && stompCount == maxStomps || !target.healthComponent.alive)
            {
                hasBarrageFinished = true;
                PlayAnimation("FullBody, Override", "BufferEmpty", "ThrowBomb.playbackRate", 1f);   // this is needed since the stomp has no transition connection, without it will loop forever
                Log.Info("TOTAL STOMPS: " + stompCount);
                Log.Info("BARAGE (STOMP) HAS BEEN COMPLETE, REMOVING DIVE COTROLLER");
                if (beginDive) divePunchController.EnemyRotation(divePunchController.modelTransform, false);
                Log.Info("First deleting dive punch");
                if (target.healthComponent.alive) divePunchController.Remove();
            }
        }

        

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
