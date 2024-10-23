using EntityStates;
using IL.RoR2.Achievements.Bandit2;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;


namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class MazBackToBackStrikes : BaseSkillState
    {
        public static float baseDuration = 1.25f;

        private float duration = 0.5f;
        private HurtBox enemyHurtBox = null;

        public SingleTracking tracking;
        private PinnableList pinnableList;
        private MazokuGrabController mazokuGrabController;
        private KnockbackController knockbackController;

        private bool hasTargetBeenFound;
        private bool isEnemyKilled;
        private bool hasSelectionMade;
        private float attackStopWatch;
        private float consecutiveDuration;
        private float attackInterval = 0.1f;
        private bool hasIncreaseAttackSpeed;


        private OverlapAttack attack;
        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float pushForce = 350f;
        protected float radius = 6f;
        public GameObject hitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex impactSound = YusukeAssets.swordHitSoundEvent.index;
        protected string hitboxGroupName = "SwordGroup";


        public override void OnEnter()
        {
            base.OnEnter();
            tracking = gameObject.GetComponent<SingleTracking>();
            mazokuGrabController = new MazokuGrabController();

            pinnableList = new PinnableList();
            pinnableList = gameObject.AddComponent<PinnableList>();

            knockbackController = new KnockbackController();

            isEnemyKilled = false;
            hasSelectionMade = false;
            characterMotor.Motor.ForceUnground();
        }

        private void TeleportToTarget()
        {
           
            characterMotor.rootMotion += enemyHurtBox.gameObject.transform.position - transform.position;
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(enemyHurtBox == null)
            {
                // get the enemies model that is marked and teleport to them.
                enemyHurtBox = tracking.GetTrackingTarget();
                if(!hasTargetBeenFound) TeleportToTarget();
            }


            if (!hasTargetBeenFound) 
            {
                // once teleported, find the matching hurtbox
                FindMatchingHurtbox();
            }
            else
            {
                // if the target is found, then attack the grabbed enemy if they are not killed or a followup selection is made.
                ConsecutiveAttack();

                if (inputBank.skill1.down)
                {
                    mazokuGrabController.EnemyRotation(mazokuGrabController.modelTransform, false);
                    if (mazokuGrabController.hasRevertedRotation)
                    {
                        mazokuGrabController.Remove();
                        hasSelectionMade = true;
                    }   
                }
            }



            if (isAuthority && fixedAge >= duration && hasSelectionMade || isEnemyKilled)
            {
                Log.Info("Stop attacking (mazoku)");
                outer.SetNextStateToMain();
            }
        }

        private void ConsecutiveAttack()
        {
            // if the character is in the list or not, do different things. 

            characterBody.SetAimTimer(0.5f); 
            attackStopWatch += GetDeltaTime();
            consecutiveDuration += GetDeltaTime();
            Log.Info("consecutive: " + consecutiveDuration);

            if (!hasIncreaseAttackSpeed && consecutiveDuration > 2)
            {
                hasIncreaseAttackSpeed = true;
                attackInterval /= 2;
            }

            if (enemyHurtBox.healthComponent.alive)
            {
                if (attackStopWatch > attackInterval)
                {

                    if (NetworkServer.active)
                    {
                        //Log.Info("Animaton" );
                        PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", 0.2f);


                    }
                    EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
                    {
                        origin = enemyHurtBox.gameObject.transform.position,
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
                    attackStopWatch = 0;

                }


            }

            if (!enemyHurtBox.healthComponent.alive)
            {
                if (enemyHurtBox.healthComponent.health < 1) //using alive doesn't work here
                {
                    mazokuGrabController.Remove();
                    isEnemyKilled = true;
                }
                    
            }
            else
            {
                if (hasSelectionMade)
                {

                    Log.Info("BARAGE HAS BEEN COMPLETE, REMOVING DIVE COTROLLER");
                    Log.Info("First deleting dive punch");
                    
                }
            }


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
                if (capturedHurtbox == enemyHurtBox)
                {
                    Log.Info("Enemy hurtbox exists. Now grabbing...");
                    GrabEnemy();
                    hasTargetBeenFound = true; break;

                }
            }
        }

        private void GrabEnemy()
        {
            mazokuGrabController = enemyHurtBox.healthComponent.body.gameObject.AddComponent<MazokuGrabController>();
            if (pinnableList)
            {
                if (!pinnableList.CheckIfNotPinnable(enemyHurtBox.healthComponent.gameObject.name))
                {
                    Log.Info("This character is not in the list, gut punch.");
                    mazokuGrabController.pivotTransform = FindModelChild("HandR");

                }
                else
                {
                    mazokuGrabController.pivotTransform = FindModelChild("HandR"); // make it pivot to a different bone or object (set it up in the 
                    Log.Info("This character is  in the list, headbutt");


                }

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            
        }

        

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
