using EntityStates;
using EntityStates.NewtMonster;
using IL.RoR2.Achievements.Bandit2;
using Newtonsoft.Json.Bson;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.Components;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using static YusukeMod.Modules.BaseStates.YusukeMain;


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
        private Vector3 storedDirection; 

        private bool hasTargetBeenFound;
        private bool isEnemyKilled;
        private bool hasSelectionMade;
        private float attackStopWatch;
        private float consecutiveDuration;
        private float totalConsecutiveTime = 8f;
        private float attackInterval = 0.1f;
        private bool hasIncreaseAttackSpeed;

        private float launchAnimationSpeed = 0.8f;
        private float launchAnimationDuration;
        private bool hasLaunchAnaimationEnded;
        private float shotgunFireStopwatch;
        private int numberOfShots;
        private float dashSpeed = 20f;
        private float velocityDivider = 0.01f; 

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
        protected string hitboxGroupName = "MeleeGroup";
        private bool hasAttackEnded;
        private bool hasLaunchedEnemy;
        private YusukeMain mainState;

        // boolean flags for kick 
        private bool hasKickedEnemy;
        private bool hasDashed;
        private bool hasTeleported;

        // boolean and floats for shotgun for ungrabbable enemies
        private bool hasFiredShotgun;
        private float shotGunEndLag;
        private float shotGunEndLagDuration = 0.5f;
        // flag to tell if the enemy can be grabbed or not
        private bool skipGrab;
        private float dashTimer;
        private float dashMaxTimer = 1f;

        // effects
        private GameObject shadowDashEffectPrefab;
        private GameObject shadowDashGrabEffectPrefab;

        private GameObject demonShotgunEffect;
        private GameObject demonShotgunObject;

        private GameObject demonShotgunExplosion;
        private GameObject demonGunTracerEffect;

        private GameObject gutPunchSlowPrefab;
        private GameObject gutPunchFastPrefab;
        private GameObject gutPunchSlowObject;
        private GameObject gutPunchFastObject;

        private GameObject hitImpactEffectPrefab;
        private GameObject finalHitEffectPrefab;
        private GameObject heavyHitEffectPrefab;
        private GameObject dashStartMaxEffectPrefab;
        private GameObject dashBoomPrefab;

        private bool hasPlayedShadowDash;
        private bool hasPlayedGutPunch;
        private bool hasSpawnedFinalHitEffect;
        
        private readonly string dashCenter = "dashCenter";
        private readonly string muzzleCenter = "muzzleCenter";
        private readonly string gutPunchCenter = "gutPunchCenter";
        private readonly string mainPosition = "mainPosition";

        private YusukeWeaponComponent yusukeWeaponComponent;

        public override void OnEnter()
        {
            base.OnEnter();

            SwitchAnimationLayer();

            tracking = gameObject.GetComponent<SingleTracking>();
            mazokuGrabController = new MazokuGrabController();
            knockbackController = new KnockbackController();


            hitImpactEffectPrefab = YusukeAssets.hitImpactEffect;
            heavyHitEffectPrefab = YusukeAssets.heavyHitRingEffect;
            finalHitEffectPrefab = YusukeAssets.finalHitEffect;
            dashStartMaxEffectPrefab = YusukeAssets.dashStartMaxEffect;
            dashBoomPrefab = YusukeAssets.dashBoomEffect;

            shadowDashEffectPrefab = YusukeAssets.shadowDashSK1;
            shadowDashGrabEffectPrefab = YusukeAssets.shadowDashGrabSK1;

            gutPunchSlowPrefab = YusukeAssets.gutPunchSlowEffect;
            gutPunchFastPrefab = YusukeAssets.gutPunchFastEffect;

            demonShotgunEffect = YusukeAssets.demonShotgunChargeEffect;
            demonShotgunExplosion = YusukeAssets.demonShotgunHitEffect;
            demonGunTracerEffect = YusukeAssets.demonShotgunTracerEffect;

            isEnemyKilled = false;
            hasSelectionMade = false;
            characterMotor.Motor.ForceUnground();


            yusukeWeaponComponent = gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetFollowUpBoolean(true);

            EditAttackEffects();

        }


        private void EditAttackEffects()
        {
            hitImpactEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1;
            heavyHitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;
            dashStartMaxEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;
            finalHitEffectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;

            shadowDashEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;
            shadowDashGrabEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;

            demonShotgunExplosion.AddComponent<DestroyOnTimer>().duration = 1;

            if (demonShotgunEffect != null) demonShotgunObject = YusukePlugin.CreateEffectObject(demonShotgunEffect, FindModelChild("HandR"));
            demonShotgunObject.SetActive(false);
            CreateGutPunchEffects();
            

        }

        private void CreateGutPunchEffects()
        {
            if (!gutPunchSlowObject) gutPunchSlowObject = YusukePlugin.CreateEffectObject(gutPunchSlowPrefab, FindModelChild(gutPunchCenter));
            if (!gutPunchFastObject) gutPunchFastObject = YusukePlugin.CreateEffectObject(gutPunchFastPrefab, FindModelChild(gutPunchCenter));
            gutPunchFastObject.SetActive(false);
            gutPunchSlowObject.SetActive(false);

        }

        private void SwitchAnimationLayer()
        {
            EntityStateMachine stateMachine = characterBody.GetComponent<EntityStateMachine>();


            // make an switch case for the effect that should be used
            

            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    mainState = (YusukeMain)stateMachine.state;

                    // need to re-enable the mazoku layer since the transformation is still active
                    MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
                    if (maz.hasTransformed)
                    {
                        mainState.SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, false);
                        mainState.SwitchMovementAnimations((int)AnimationLayerIndex.MegaCharge, true);
                    }


                }

            }

            

        }

        private void TeleportToTarget()
        {
            characterMotor.rootMotion += enemyHurtBox.gameObject.transform.position - transform.position;
            if (!hasPlayedShadowDash)
            {
                hasPlayedShadowDash = true;
                EffectManager.SimpleMuzzleFlash(shadowDashGrabEffectPrefab, gameObject, dashCenter, true);
                gutPunchSlowObject.SetActive(true);
            }


        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (enemyHurtBox == null)
            {
                // get the enemies model that is marked and teleport to them.
                enemyHurtBox = tracking.GetTrackingTarget();
                if (enemyHurtBox)
                {
                    if (!hasTargetBeenFound) TeleportToTarget();
                }
                else
                {
                    Log.Error("No enemy found. ");
                    // re-adding the stock since it was used.
                    skillLocator.secondary.AddOneStock();
                    outer.SetNextStateToMain();
                }
                
            }


            if (!hasTargetBeenFound)
            {
                // once teleported, find the matching hurtbox
                FindMatchingHurtbox();
            }
            else
            {

                // if the enemy has a regular motor and can be grabbed
                if (!skipGrab)
                {
                    SlowVelocity();
                    // if the target is found, then attack the grabbed enemy if they are not killed or a followup selection is made.
                    if (!hasSelectionMade) 
                    {
                        ConsecutiveAttack();
                        
                        if (consecutiveDuration > totalConsecutiveTime)
                        {
                            if (demonShotgunObject) EntityState.Destroy(demonShotgunObject);
                            LaunchEnemy();
                            if (launchAnimationDuration > launchAnimationSpeed)
                            {
                                EndAnimationLoops();
                                outer.SetNextStateToMain();
                            
                            }
                                
                        }

                    }
                        
                    if (inputBank.skill1.down) 
                    {
                        hasSelectionMade = true;
                        mazokuGrabController.Remove();
                        
                    }

                    if (hasSelectionMade)   // once a selection is made (skill 1 selected), then it will continue
                    {
                        if (demonShotgunObject) EntityState.Destroy(demonShotgunObject);
                        LaunchEnemy();
                        if (launchAnimationDuration > launchAnimationSpeed) DashAndKick();
                        
                    }
                }
                else
                {
                    // Count the shotgun suration for the shotgun as the emeny cannot be grabbed
                    shotGunEndLag += Time.fixedDeltaTime;
                    characterBody.SetAimTimer(0.1f);
                    // after the shotgun duration (so the animation can be seen and not interruped), kick em
                    if (shotGunEndLag > shotGunEndLagDuration)
                    {
                        hasPlayedShadowDash = false;
                        TeleportToTarget();
                        KickEnemy();
                    }
                }
                
            }

            

            if (isAuthority && fixedAge >= duration && hasAttackEnded || isEnemyKilled)
            {
                Log.Info("Stop attacking (mazoku)");
                outer.SetNextStateToMain();
            }
        }

        private void SwitchGutPunchEffect(bool isSpedUp)
        {
            if (!isSpedUp)
            {
                gutPunchSlowObject.SetActive(true);
                gutPunchFastObject.SetActive(false);
            }
            else
            {
                gutPunchSlowObject.SetActive(false);
                gutPunchFastObject.SetActive(true);
            }
        }

        // This method prevents any animaions looping as most of the looped ones are not linked to bufferEmpty. 
        private void EndAnimationLoops()
        {
            Log.Info("Removing the animation loop;");
            PlayAnimation("FullBody, Override", "BufferEmpty", "ShootGun.playbackRate", 1f);
            PlayAnimation("BothHands, Override", "BufferEmpty", "ThrowBomb.playbackRate", 1f); 
        }

        private void SlowVelocity()
        {

            float decelerateValue = 0.2f; // 50f  // 150

            characterMotor.velocity *= decelerateValue;
            float x = characterMotor.velocity.x;
            float y = characterMotor.velocity.y;
            float z = characterMotor.velocity.z;

            base.characterMotor.velocity = new Vector3(x, y, z);
        }

        // removes the mazokucontroller and adds the knockback controller
        private void LaunchEnemy()
        {
            if (!hasLaunchedEnemy) 
            { 
                hasLaunchedEnemy = true;

                RemovePunchEffects();
                

                mazokuGrabController.Remove();
                PauseVelocity();
                AddKnockbackController();
                
                PlayAnimation("BothHands, Override", "BufferEmpty", "ThrowBomb.playbackRate", 1f); //clearing the infinite looped gutpunch animation
                if (isGrounded)
                {
                    PlayAnimation("FullBody, Override", "MazokuToss", "Roll.playbackRate", launchAnimationSpeed);
                }
                else
                {
                    PlayAnimation("FullBody, Override", "MazokuTossAir", "Roll.playbackRate", launchAnimationSpeed);
                }
                

            }

            launchAnimationDuration += Time.fixedDeltaTime;
            Log.Info(launchAnimationDuration);
            
        }

        private void RemovePunchEffects()
        {
            if(gutPunchFastObject) EntityState.Destroy(gutPunchFastObject); 
            if(gutPunchSlowObject) EntityState.Destroy(gutPunchSlowObject);
            if(demonShotgunObject) EntityState.Destroy(demonShotgunObject);

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
            characterMotor.velocity = Vector3.zero;
            characterMotor.enabled = false;
            characterDirection.enabled = false;

        }

        private void ShotgunAA12()
        {

            shotgunFireStopwatch += Time.fixedDeltaTime;

            //fires shots every x amount of seconds defined here.
            if (shotgunFireStopwatch > 0.25f)
            {
                shotgunFireStopwatch = 0;
                Fire();

            }

        }

        private void ShotgunPunch()
        {
            if (!hasFiredShotgun)
            {
                hasFiredShotgun = true;
                characterBody.SetAimTimer(0.1f);
                characterMotor.enabled = false;
                PlayAnimation("BothHands, Override", "ShootSpiritShotgun", "ShootGun.playbackRate", 1f);
                Fire();
            }
            
        }

        private void DashAndKick()
        {
            dashTimer += GetDeltaTime();

            characterMotor.enabled = true;
            characterDirection.enabled = true;
            Log.Info("Now dashing. ");
            if(!hasDashed)
            {
                hasDashed = true;
                PlayAnimation("FullBody, Override", "Dash", "ShootGun.playbackRate", 1f);
                EffectManager.SimpleMuzzleFlash(dashStartMaxEffectPrefab, gameObject, mainPosition, false);
                EffectManager.SimpleMuzzleFlash(dashBoomPrefab, gameObject, dashCenter, false);

            }

            if(dashTimer > dashMaxTimer)
            {
                EndAnimationLoops();
                outer.SetNextStateToMain();
            }

            Vector3 enemyPosition = enemyHurtBox.gameObject.transform.position;

            Vector3 directionToTarget = (enemyPosition - transform.position).normalized;

            // Calculate the velocity in the direction of the target
            Vector3 forwardSpeed = directionToTarget * (dashSpeed * moveSpeedStat);

            // Apply the velocity to the character's motor
            characterMotor.velocity = forwardSpeed;

            Collider[] colliders;
            colliders = Physics.OverlapSphere(transform.position, 2, LayerIndex.entityPrecise.mask);
            List<Collider> capturedColliders = colliders.ToList();

            // check each hurtbox and catpure the hurtbox they have, then compare the two for a match.
            foreach (Collider result in capturedColliders)
            {
                HurtBox capturedHurtbox = result.GetComponent<HurtBox>();

                if (capturedHurtbox)
                {
                    if (capturedHurtbox == enemyHurtBox)
                    {

                        if (!hasKickedEnemy) 
                        { 
                            KickEnemy();
                            
                            
                        }
                            
                        
                    }


                }
            }
        }

        private void KickEnemy()
        {

            hasKickedEnemy = true;
            PlayAnimation("FullBody, Override", "BackToBackMeleeFinish", "ShootGun.playbackRate", 1f);

            // when doing the kick, grabbing the knockbackcontroller direction and applying a force vector which will be used for special knockback for the enemies
            Vector3 forceVector = GetAimRay().direction;    // for now the Aim Ray is based on the characters facing direction
            forceVector *= 20000f;

            EffectManager.SimpleMuzzleFlash(finalHitEffectPrefab, gameObject, muzzleCenter, false);
            EffectManager.SimpleMuzzleFlash(heavyHitEffectPrefab, gameObject, muzzleCenter, false);

            knockbackController.ForceDestory(); // destroying the controller first, so it doesn't interrupt the force vector
            AttackForce(forceVector);

            hasAttackEnded = true;

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
            enemyHurtBox.healthComponent.TakeDamage(damageInfo);
        }

        private void Fire()
        {
            SearchForEnemies(); 
            float damageDivision = enemyTargets.Count(); // division based on enemy scan

            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            if(enemyTargets.Count != 0) Util.PlaySound("HenryShootPistol", gameObject);

            Ray aimRay = GetAimRay();

            foreach (HurtBox enemy in enemyTargets)
            {
                //do a check if its a max charge, it SlowOnHit. If not, then regular.
                EffectManager.SpawnEffect(demonShotgunExplosion, new EffectData
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
                    radius = 4f,
                    sniper = false,
                    stopperMask = LayerIndex.world.mask,
                    weapon = null,
                    tracerEffectPrefab = demonGunTracerEffect,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = demonShotgunExplosion,

                }.Fire();
            }
        }

        private void ConsecutiveAttack()
        {
            // if the character is in the list or not, do different (animation). 

            if (!hasPlayedGutPunch)
            {
                hasPlayedGutPunch = true;
                if (NetworkServer.active && !hasSelectionMade)
                {
                    if(!hasIncreaseAttackSpeed) PlayAnimation("BothHands, Override", "GutPunch", "ThrowBomb.playbackRate", 0.4f);
                    if(hasIncreaseAttackSpeed) PlayAnimation("BothHands, Override", "GutPunchFast", "ThrowBomb.playbackRate", 0.4f);
                }
            }

            characterBody.SetAimTimer(0.5f); 
            attackStopWatch += GetDeltaTime();
            consecutiveDuration += GetDeltaTime();
            Log.Info("consecutive: " + consecutiveDuration);

            //SwitchGutPunchEffect(false);
            // the punches will increase its speed if 2 seconds passes. 
            if (!hasIncreaseAttackSpeed && consecutiveDuration > 2)
            {
                hasIncreaseAttackSpeed = true;
                hasPlayedGutPunch = false;
                SwitchGutPunchEffect(true);
                demonShotgunObject.SetActive(true);
                attackInterval /= 2;
                
            }

            // if the gut punch duration is still active, then fire the shotgun attack 
            if(hasIncreaseAttackSpeed && consecutiveDuration < totalConsecutiveTime) ShotgunAA12();

            if (enemyHurtBox.healthComponent.alive)
            {
                if (attackStopWatch > attackInterval)
                {

                    
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
                if (enemyHurtBox.healthComponent.health < 0.1) //using alive doesn't work here
                {
                    EndAnimationLoops(); // force end prevents the 
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
            // try to get the enemies motor and rigid body
            CharacterMotor enemyMotor = enemyHurtBox.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
            Rigidbody enemyRigidBody = enemyHurtBox.healthComponent.body.gameObject.GetComponent<Rigidbody>();

            if (enemyRigidBody)
            {
                if (enemyMotor)
                {

                    if (enemyHurtBox.healthComponent.body.isChampion || enemyHurtBox.healthComponent.body.isChampion)
                    {
                        // skip the grab stuff and just attack, as grabbing will cause issues. 
                        skipGrab = true;
                        characterBody.SetAimTimer(0.1f);
                        ShotgunPunch();
                    }
                    else
                    {
                        // add the grab component and locate the pivot 
                        mazokuGrabController = enemyHurtBox.healthComponent.body.gameObject.AddComponent<MazokuGrabController>();
                        mazokuGrabController.yusukeBody = characterBody;
                        mazokuGrabController.pivotTransform = FindModelChild("HandL"); // make it pivot to a different bone or empty object (set it up in the editor)
                    }

                    
                }
                else
                {
                    // skip the grab stuff and just attack, as grabbing will cause issues. 
                    skipGrab = true;
                    characterBody.SetAimTimer(0.1f);
                    ShotgunPunch();
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

            if (!hasKickedEnemy && hasAttackEnded) {
                PlayAnimation("FullBody, Override", "BufferEmpty", "Roll.playbackRate", launchAnimationSpeed);
            }
            
            characterMotor.enabled = true;
            characterDirection.enabled = true;

            // so the character doesn't go flying like crazy due to the velocity, the current velocity will be divided by the given percentage decimal (velocityDivider). 
            Vector3 currentVelocity = characterMotor.velocity;
            Vector3 velocityPercentage = currentVelocity * velocityDivider;
            characterMotor.velocity = velocityPercentage;

            mainState.SwitchMovementAnimations((int)AnimationLayerIndex.MegaCharge, false);

            // need to re-enable the mazoku layer since the transformation it's still active
            MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
            if (maz.hasTransformed)
            {
                mainState.SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, true);
            }

            RemovePunchEffects();

            yusukeWeaponComponent = gameObject.GetComponent<YusukeWeaponComponent>();
            yusukeWeaponComponent.SetFollowUpBoolean(false);

        }

        

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
