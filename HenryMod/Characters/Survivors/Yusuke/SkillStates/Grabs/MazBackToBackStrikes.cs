using EntityStates;
using EntityStates.NewtMonster;
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
using static UnityEngine.ParticleSystem.PlaybackState;


namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class MazBackToBackStrikes : BaseSkillState
    {
        public static float baseDuration = 1.25f;

        private float duration = 0.5f;
        private HurtBox enemyHurtBox = null;
        private List<HurtBox> enemyTargets = new List<HurtBox>();

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

        private float launchAnimationSpeed = 0.8f;
        private float launchAnimationDuration;
        private bool hasLaunchAnaimationEnded;
        private float shotgunFireStopwatch;
        private int numberOfShots;

        public float maxTrackingDistance = 60f;
        public float maxTrackingAngle = 60f;
        public BullseyeSearch search = new BullseyeSearch();


        public static float shotDamageCoefficient = YusukeStaticValues.gunDamageCoefficient;
        public GameObject spiritImpactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion"); //YusukeAssets.spiritGunExplosionEffect;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); //"Prefabs/Effects/Tracers/TracerGoldGat"
        private string muzzleString;

        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 100f;
        public static float recoil = 3f;
        public static float range = 256f;


        private OverlapAttack attack;
        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float pushForce = 350f;
        protected float radius = 6f;
        public GameObject hitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex impactSound = YusukeAssets.swordHitSoundEvent.index;
        protected string hitboxGroupName = "SwordGroup";
        private bool hasAttackEnded;
        private bool hasLaunchedEnemy;

        public override void OnEnter()
        {
            base.OnEnter();
            tracking = gameObject.GetComponent<SingleTracking>();
            mazokuGrabController = new MazokuGrabController();
            knockbackController = new KnockbackController();

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

            if (enemyHurtBox == null)
            {
                // get the enemies model that is marked and teleport to them.
                enemyHurtBox = tracking.GetTrackingTarget();
                if (!hasTargetBeenFound) TeleportToTarget();
            }


            if (!hasTargetBeenFound)
            {
                // once teleported, find the matching hurtbox
                FindMatchingHurtbox();
            }
            else
            {
                
                // if the target is found, then attack the grabbed enemy if they are not killed or a followup selection is made.
                if(!hasSelectionMade) ConsecutiveAttack();

                if (inputBank.skill1.down)
                {
                    hasSelectionMade = true;
                    if (!mazokuGrabController.hasRevertedRotation) mazokuGrabController.EnemyRotation(mazokuGrabController.modelTransform, false);  //revert rotation so the enemey is back to normal
                }

                if (hasSelectionMade)   // once a selection is made (skill 1 selected), then it will continue
                {
                    
                    if (mazokuGrabController.hasRevertedRotation)  
                    {
                        if (numberOfShots != 6) // delay the shotgun barrage so the animation can play out, then start firing
                        {
                            LaunchEnemy();  
                            if(launchAnimationDuration > launchAnimationSpeed) ShotgunAA12();

                        }
                        else
                        {
                            // once six shots are fired, the attack has ended and will return
                            hasAttackEnded = true;
                            

                        }



                    }
                }
                
            }

            if (isAuthority && fixedAge >= duration && hasAttackEnded || isEnemyKilled)
            {
                Log.Info("Stop attacking (mazoku)");
                outer.SetNextStateToMain();
            }
        }

        // removes the mazokucontroller and adds the knockback controller
        private void LaunchEnemy()
        {
            if (!hasLaunchedEnemy) 
            { 
                hasLaunchedEnemy = true;
                mazokuGrabController.Remove();
                PauseVelocity();
                AddKnockbackController();
                PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", launchAnimationSpeed);

            }

            launchAnimationDuration += Time.fixedDeltaTime;
            Log.Info(launchAnimationDuration);
            
        }

        // knockback properties
        private void AddKnockbackController()
        {

            // add controller to the enemy 
            knockbackController = enemyHurtBox.healthComponent.body.gameObject.AddComponent<KnockbackController>();
            knockbackController.moveID = 1;

            knockbackController.knockbackDirection = GetAimRay().direction;
            knockbackController.knockbackSpeed = 40f;
            knockbackController.pivotTransform = characterBody.transform;
        }

        // pausing the velocity, so the character doesn't move (prevents any odd movements)
        private void PauseVelocity()
        {
            characterBody.SetAimTimer(0.1f);
            characterMotor.velocity = Vector3.zero;
            characterMotor.enabled = false;
            characterDirection.enabled = false;

        }

        private void ShotgunAA12()
        {
            PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", 1f);
            shotgunFireStopwatch += Time.fixedDeltaTime;

            //fires six shots every x amount of seconds defined here.
            if (shotgunFireStopwatch > 0.15)
            {
                if (numberOfShots != 6) Fire();
                shotgunFireStopwatch = 0;
                numberOfShots++;

            }


            

        }

        private void Fire()
        {
            SearchForEnemies(); =
            float damageDivision = enemyTargets.Count(); // division based on enemy scan

            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            Util.PlaySound("HenryShootPistol", gameObject);

            Ray aimRay = GetAimRay();

            foreach (HurtBox enemy in enemyTargets)
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
                    radius = 2f,
                    sniper = false,
                    stopperMask = LayerIndex.world.mask,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,

                }.Fire();
            }
        }

        private void ConsecutiveAttack()
        {
            // if the character is in the list or not, do different (animation). 

            characterBody.SetAimTimer(0.5f); 
            attackStopWatch += GetDeltaTime();
            consecutiveDuration += GetDeltaTime();
            Log.Info("consecutive: " + consecutiveDuration);

            // the punches will increase its speed if 2 seconds passes. 
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
                    mazokuGrabController.pivotTransform = FindModelChild("HandR");  // make it pivot to a different bone or empty object(set it up in the editor)

                }
                else
                {
                    mazokuGrabController.pivotTransform = FindModelChild("HandR"); // make it pivot to a different bone or empty object (set it up in the editor)
                    Log.Info("This character is  in the list, headbutt");


                }

            }
        }

        private void SearchForEnemies()
        {
            Ray aimRay = GetAimRay();
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            enemyTargets = search.GetResults().ToList();
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.enabled = true;
            characterDirection.enabled = true;
        }

        

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
