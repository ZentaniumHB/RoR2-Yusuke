using EntityStates;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using YusukeMod;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace YusukeMod.SkillStates
{
    public class SpiritWave : BaseSkillState
    {

        //dash calculations and stopwatches
        public float charge;
        public bool isMaxCharge;
        private Vector3 forwardDirection;

        private float chargedFinalSpeed;
        private float chargedMaxSpeed;

        private float minimunDashSpeed = 3f;
        private float minFinalDashSpeed = 1.25f;

        private float MaximumDashSpeed = 6f;
        private float MaxFinalDashSpeed = 2.5f;

        private float dashSpeed;

        private float duration;
        private float minDuration = 0.3f;
        private float maxDuration = 0.8f;

        public float maxTrackingDistance = 12f;
        public float maxTrackingAngle = 60f;

        public float actionStopwatch = 0.0f;
        public float actionTimeDuration = 0.8f;


        private Vector3 prevMovementVector;

        // attack settings

        private OverlapAttack attack;
        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float pushForce = 350f;
        protected Vector3 bonusForce = Vector3.zero;
        protected string hitboxGroupName = "SwordGroup";
        public GameObject hitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex impactSound = YusukeAssets.swordHitSoundEvent.index;
        private bool hasPunched;
        private GameObject _;
        private bool hasFlyingBulletFired;
        private bool groundedVersion;


        // animation settings
        protected Animator animator;
        protected float hitStopDuration = 0.012f;

        // targeting and knockback
        private bool collision;
        private BullseyeSearch search = new BullseyeSearch();
        private Collider enemyCollider;
        private HurtBox target;
        public List<Collider> bodyList;
        private bool isBodyFound = false;
        private Indicator indicator;
        public GameObject targetIcon;
        private KnockbackController knockbackController;
        private Vector3 vector;


        //physic sphere 
        private float sphereRadius = 3f;
        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;
        private Vector3 previousPosition;
        private ModelLocator modelLocator;

        // skill prefix stuff and switching
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;
        private bool hasSwappedSkills;
        private EntityStateMachine stateMachine;
        private bool followUpActivated;

        private int chosenSkill;

        private bool hasAddedIDs;
        private bool nextState;

        private Type currentStateType;
        private int equipedPrimarySlot;
        private int equipedSecondarySlot;
        private int attackType;


        private GenericSkill previousSecondarySkill;



        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            knockbackController = new KnockbackController();
            stateMachine = characterBody.GetComponent<EntityStateMachine>();

            duration = Mathf.Lerp(minDuration, maxDuration, charge);
            Log.Info("Duration: " + duration);
            forwardDirection = GetAimRay().direction;

            chargedMaxSpeed = GetChargedMax(charge);
            chargedFinalSpeed = GetChargedFinal(charge);

            UpdateDashSpeed(chargedMaxSpeed, chargedFinalSpeed);

            if (!isGrounded)
            {
                characterBody.isSprinting = true;
                if (characterMotor && characterDirection)
                {

                    characterMotor.velocity = forwardDirection * dashSpeed;

                }
                Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
                previousPosition = transform.position - b;
            }

            // prevent any movement if spirit wave is grouned
            if (isGrounded)
            {
                prevMovementVector = characterMotor.velocity;
                vector = forwardDirection * dashSpeed;
                characterMotor.enabled = false;
                characterDirection.enabled = false;
                groundedVersion = true;
            }

        }

        private void UpdateDashSpeed(float max, float final)
        {
            dashSpeed = (moveSpeedStat * 1.2f) * Mathf.Lerp(max, final, fixedAge / duration);
            //Log.Info("Dashspeed: " + dashSpeed);
        }

        private float GetChargedMax(float charge)
        {
            //return (charge / 100.0f) * MaximumDashSpeed;

            return Mathf.Lerp(MaxFinalDashSpeed, MaximumDashSpeed, charge / 100.0f);

        }

        private float GetChargedFinal(float charge)
        {
            //return (charge / 100.0f) * MaxFinalDashSpeed; //chargedFinalSpeed
            return Mathf.Lerp(minFinalDashSpeed, minimunDashSpeed, charge / 100.0f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.disableAirControlUntilCollision = true;
            UpdateDashSpeed(chargedMaxSpeed, chargedFinalSpeed);

            if (target)
            {
                // an enemy has been scanned, now find the collider they have.
                if(!isBodyFound) Debug.Log("Enemy scanned, finding body");
                SearchForPhysicalBody();
                if (isBodyFound)
                {
                    if (!collision) Log.Info("Proceeding...punch!");
                    // Found a body, throw out punch
                    ThrowPunch();
                    actionStopwatch += Time.fixedDeltaTime;

                    // Check if the body has a health component and is currently alive
                    if ((bool)target.healthComponent && target.healthComponent.alive && !collision)
                    {
                        CharacterBody body = target.healthComponent.body;
                        if (body)
                        {
                            if (body.isFlying)  // flying targets get pushed with a 0 damage bullet, since I don't know how to control flying types with no body. 
                            {
                                if (!hasFlyingBulletFired)
                                {
                                    hasFlyingBulletFired = true;
                                    FireBulletForFlying();

                                }
                            }
                        }

                        // create the indicator on the body to show which enemy will receive the follow up
                        if (targetIcon == null)
                        {
                            targetIcon = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
                        }
                        indicator = new Indicator(gameObject, targetIcon);

                        // add controller to the enemy that is marked
                        knockbackController = target.healthComponent.body.gameObject.AddComponent<KnockbackController>();
                        knockbackController.moveID = 1;

                        // boolean is used for the knockback controller as the velocity will be different for grounded punch
                        if (groundedVersion)
                        {
                            if (!collision) Log.Info("Ground version!");
                            knockbackController.wasAttackGrounded = true;
                        }

                        knockbackController.knockbackDirection = forwardDirection;

                        knockbackController.knockbackSpeed = dashSpeed;
                        knockbackController.pivotTransform = characterBody.transform;
                        indicator.targetTransform = target.transform;
                        indicator.active = true;

                        collision = true;

                    }
                    

                }
                   
            }

            if (!collision)
            {
                SearchForTarget();

                if (characterDirection)
                {
                    characterDirection.forward = forwardDirection;
                    characterDirection.moveVector = forwardDirection;
                    // rotate the characters model so it looks better visually
                    modelLocator = characterBody.GetComponent<ModelLocator>();
                    Transform modelTransform = modelLocator.transform;

                    if (modelTransform)
                    {

                        
                    }
                    

                }


                if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);

                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    vector = normalized * dashSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;


                }

                // if not the grounded version, move the character forward, if grounded prevent movement
                if (!groundedVersion)
                {
                    characterMotor.velocity = vector;
                    previousPosition = transform.position;

                }
                else
                {
                    characterMotor.velocity = Vector3.zero;
                }

            }

            if (collision)
            {

                if (isAuthority)
                {
                    // check if they are in the correct state to do a follow up
                    if (!followUpActivated) SwitchSkills();
                    if (hasPunched)
                    {
                        OnHitEnemyAuthority();
                        
                    }

                    if (collision && actionStopwatch >= actionTimeDuration)
                    {
                        outer.SetNextState(new RevertSkills
                        {
                            moveID = 4,
                        });
                        return;
                    }

                    // if spacebar is pressed, it will cancel the duration time and will return to the main state.
                    if (inputBank.jump.down)
                    {
                        
                        actionStopwatch = actionTimeDuration + 1;
                    }
                    
                    // Once the move are changed, if either one of them are pressed, then it will move to the next state
                    if (!followUpActivated && inputBank.skill1.down && isAuthority)
                    {
                        if (CheckMoveAvailability(equipedPrimarySlot))
                        {
                            chosenSkill = 1;
                            Log.Info("chosen move: " + chosenSkill);
                            followUpActivated = true;
                            //SwitchSkillsBack(1);
                            knockbackController.isFollowUpActive = true;
                            if (!nextState)
                            {
                                nextState = true;
                                outer.SetNextState(MeleeFollowUp());
                            }
                            return;
                        }
                                              

                    }

                    Log.Info("MODEL NAME: " + target.healthComponent.gameObject.name);


                    if (!followUpActivated && inputBank.skill2.down && isAuthority)
                    {
                        if (CheckMoveAvailability(equipedSecondarySlot))
                        {
                            if(attackType == 2)
                            {
                                chosenSkill = 2;
                                Log.Info("chosen move: " + chosenSkill);
                                followUpActivated = true;
                                //SwitchSkillsBack(2);
                                if (!nextState)
                                {
                                    nextState = true;
                                    outer.SetNextState(GunFollowUp());
                                   
                                }

                            }
                            
                            if(attackType == 3) 
                            {
                                chosenSkill = 3;
                                Log.Info("chosen move: " + chosenSkill);
                                followUpActivated = true;
                                //SwitchSkillsBack(2);
                                if (!nextState)
                                {
                                    nextState = true;
                                    outer.SetNextState(ShotGunFollowUp());
                                    
                                }
                            }

                            return;

                            
                        }
                        

                    }

                }

            }

            if (isAuthority && fixedAge >= duration)
            {
                if (!collision)
                {
                    outer.SetNextStateToMain();
                    return;
                }


            }
        }

        // next followup entity state

        protected virtual EntityState MeleeFollowUp()
        {
            return new DivePunch
            {
                charge = charge,
                ID = chosenSkill,
                target = target,
                enemyCollider = enemyCollider,
            };
        }

        protected virtual EntityState GunFollowUp()
        {
            return new SpiritGunFollowUp
            {
                charge = charge,
                ID = chosenSkill,
                target = target,

            };
        }

        protected virtual EntityState ShotGunFollowUp()
        {
            return new SpiritShotgunFollowUp
            {
                charge = charge,
                ID = chosenSkill,
                target = target,

            };
        }

        


        private void FireBulletForFlying()
        {

            new BulletAttack
            {
                bulletCount = 1,
                aimVector = target.gameObject.transform.position - transform.position,
                origin = transform.position,
                damage = 0,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.None,
                maxDistance = 256f,
                force = 1600f,                                       // this field is literally the only reason why this bullet is being used.
                hitMask = LayerIndex.CommonMasks.bullet,
                minSpread = 0f,
                maxSpread = 0f,
                isCrit = RollCrit(),
                owner = gameObject,
                muzzleName = "",
                smartCollision = true,
                procChainMask = default,
                procCoefficient = procCoefficient,
                radius = 0.75f,
                sniper = false,
                stopperMask = LayerIndex.CommonMasks.bullet,
                weapon = null,
                tracerEffectPrefab = _,
                spreadPitchScale = 1f,
                spreadYawScale = 1f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                hitEffectPrefab = _,

            }.Fire();
        }

        private void OnHitEnemyAuthority()
        {

            float decelerationValue = 0.2f;
            characterMotor.velocity = new Vector3(decelerationValue, decelerationValue, decelerationValue);

        }

        public void ThrowPunch()
        {
            if (!hasPunched)
            {

                attack = new OverlapAttack
                {
                    damageType = damageType,
                    attacker = gameObject,
                    inflictor = gameObject,
                    teamIndex = GetTeam(),
                    damage = damageCoefficient * damageStat,
                    procCoefficient = procCoefficient,
                    hitEffectPrefab = hitEffectPrefab,
                    forceVector = bonusForce,
                    pushAwayForce = pushForce,
                    hitBoxGroup = FindHitBoxGroup(hitboxGroupName),
                    isCrit = RollCrit(),
                    impactSound = impactSound
                };

                // maybe add an bullet attack that can apply force to flying enemy but no damage and no effect?

                attack.Fire();
                hasPunched = true;
            }
            
        }

        public override void OnExit()
        {
            base.OnExit();
            Log.Info("LEAVING SPIRIT WAVE");

            // removes the fov change and allows aircontrol 
            characterMotor.disableAirControlUntilCollision = false;
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            if (target && isBodyFound)  // removes indicators if present
            {
                if(indicator != null) indicator.active = false;
            }

            if (groundedVersion)
            {
                characterMotor.enabled = true;
                characterDirection.enabled = true;
                characterMotor.velocity = prevMovementVector;
            }

            // if the player made no decision then pass this value
            if (!followUpActivated)
            {
                outer.SetNextState(new RevertSkills
                {
                    moveID = 4
                });
            }

            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        // searching for initial target
        private void SearchForTarget()
        {
            if (!hasPunched)
            {
                search.teamMaskFilter = TeamMask.all;
                search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                search.filterByLoS = true;
                search.searchOrigin = transform.position;
                search.searchDirection = forwardDirection;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.maxDistanceFilter = maxTrackingDistance;
                search.maxAngleFilter = maxTrackingAngle;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);

                target = search.GetResults().FirstOrDefault();
            }

        }

        private bool AlternativeSearch(HurtBox closestHurtbox)
        {
            // used to check if there is another enemy that might be closer than the enemy that wsa previously scanned
            if(!hasPunched)
            {
                search.teamMaskFilter = TeamMask.all;
                search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                search.filterByLoS = true;
                search.searchOrigin = transform.position;
                search.searchDirection = forwardDirection;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.maxDistanceFilter = maxTrackingDistance;
                search.maxAngleFilter = maxTrackingAngle;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);

                foreach (HurtBox enemy in search.GetResults())
                {
                    if (enemy == closestHurtbox)
                    {
                        Log.Info("New enemy found, new target!");
                        target = enemy;
                        return true;
                    }

                }

                return false;
            }
            else
            {
                return false;
            }


        }


        private void SearchForPhysicalBody()
        {

            if (!isBodyFound)
            {
                Vector3 sphereCenter = transform.position + transform.forward;
                Collider[] capturedBody;

                // increae the radius of the sphere and push it slightly forward to prevent hitting enemies from behind if standing
                if (groundedVersion) 
                {
                    sphereRadius = 8f;
                    Vector3 sphereLoc = new Vector3(sphereCenter.x, sphereCenter.y + 5f, sphereCenter.z);
                    capturedBody = Physics.OverlapSphere(sphereLoc, sphereRadius, LayerIndex.entityPrecise.mask);
                }
                else
                {   // if not, keep it regular
                    capturedBody = Physics.OverlapSphere(sphereCenter, sphereRadius, LayerIndex.entityPrecise.mask);
                }

                List<Collider> capturedColliders = capturedBody.ToList();

                // check each hurtbox and catpure the hurtbox they have, then compare the two for a match.
                foreach (Collider result in capturedColliders)
                {
                    enemyCollider = result;
                    HurtBox capturedHurtbox = result.GetComponent<HurtBox>();

                    if (capturedHurtbox)
                    {
                        if (capturedHurtbox == target)
                        {
                            isBodyFound = true;

                        }
                        else
                        {
                            // if they don't match, then do an alternative scan
                            if (AlternativeSearch(capturedHurtbox)) isBodyFound = true;
                        }

                    }

                }

            }

        }


        private void SwitchSkills()
        {
            // swap the skills, if cuffs are released

            /* ID 1 == Melee
               ID 2 == gun
               ID 3 == ShotGun

            */
            

            if (!hasSwappedSkills)
            {
                hasSwappedSkills = true;
                Log.Info(skillLocator.secondary.skillNameToken);
                Log.Info("Swapping skills");
                // going throught the skills and changing them
                switch (skillLocator.primary.skillNameToken)
                {
                    case prefix + "PRIMARY_SLASH_NAME":
                        StoreStockCount(1);
                        skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.primaryMelee, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.meleeFollowUp, GenericSkill.SkillOverridePriority.Contextual);
                        equipedPrimarySlot = 1;
                        attackType = 1;
                        FollowUpSettings(followUpActivated, 1, 1); // uses three parameters to determine what actions are needed for the move
                        break;
                    case prefix + "PRIMARY_GUN_NAME":
                        StoreStockCount(1);
                        skillLocator.primary.UnsetSkillOverride(gameObject, YusukeSurvivor.primarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.primary.SetSkillOverride(gameObject, YusukeSurvivor.spiritGunFollowUp, GenericSkill.SkillOverridePriority.Contextual);
                        equipedPrimarySlot = 1;
                        attackType = 1;
                        FollowUpSettings(followUpActivated, 1, 1); // uses three parameters to determine what actions are needed for the move
                        break;
                }
                switch (skillLocator.secondary.skillNameToken)
                {
                    case prefix + "SECONDARY_GUN_NAME":
                        StoreStockCount(2);
                        skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritGun, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.spiritGunFollowUp, GenericSkill.SkillOverridePriority.Contextual);
                        equipedSecondarySlot = 2;
                        attackType = 2;
                        FollowUpSettings(followUpActivated,2,2); // uses three parameters to determine what actions are needed for the move
                        Log.Info("Move has been changed");
                        break;
                    case prefix + "SECONDARY_SHOTGUN_NAME":
                        StoreStockCount(2);
                        base.skillLocator.secondary.UnsetSkillOverride(gameObject, YusukeSurvivor.secondarySpiritShotgun, GenericSkill.SkillOverridePriority.Contextual);
                        base.skillLocator.secondary.SetSkillOverride(gameObject, YusukeSurvivor.spiritShotgunFollowUp, GenericSkill.SkillOverridePriority.Contextual);
                        equipedSecondarySlot = 2;
                        attackType = 3;
                        FollowUpSettings(followUpActivated, 2,3);
                        break;


                }
            }
        }   


        // stores the current stock count for the move that is going to switch, that way it won't simply reset to zero everytime there is a switch
        private void StoreStockCount(int skillSlot)
        {
            YusukeMain mainState = (YusukeMain)stateMachine.state;
            if (skillSlot == 1) mainState.SetStock(skillLocator.primary.stock, 1);
            if (skillSlot == 2) mainState.SetStock(skillLocator.secondary.stock, 2);
        }

        // used to check if the move can be done, whether that be the primary or secondary slot 
        public bool CheckMoveAvailability(int equipedSlot)
        {
            YusukeMain targetState = (YusukeMain)stateMachine.state;
            if (targetState.GetMoveStatus(equipedSlot)) 
            {
                // move is ready to be used.
                return true;
            }
            if (!targetState.GetMoveStatus(equipedSlot)) 
            {
                // move is not ready to be used. 
                return false;
            } 
            return false;
        }

        public void FollowUpSettings(bool isFollowUpActive, int skillSlot, int ID)
        {
            Log.Info("FollowUpActivation: " + isFollowUpActive);
            /* clear the stocks on the follow up (follow up attacks will always have one stock as they rely on the original moves stock
             *  in adddition, the appropriate time 
             */
            if (skillSlot == 1) skillLocator.primary.DeductStock(1);
            if (skillSlot == 2) skillLocator.secondary.DeductStock(1);

            // grabbing state machine
            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    YusukeMain targetState = (YusukeMain)stateMachine.state;

                    Log.Info("Receiving interval");
                    /* if the user just released the punch, then it will retrieve the current conditions for each move, e.g. their cooldown timer 
                        which is stored on the YusukeMain state machine.
                     */
                    bool canUse = targetState.GetMoveStatus(skillSlot);

                    if (skillSlot == 1)
                    {
                        Log.Info("ID Before retrieval: " + ID);
                        // if the interval is smaller than 0 (meaning the skill is no longer on cooldown), re-add the stock.
                        if (targetState.GetInterval(ID) <= 0 || canUse)
                        {
                            Log.Info("yes....");
                            skillLocator.primary.AddOneStock();
                        }
                        else
                        {   // if not, then grab the difference between the inverval and the countdown start number and add it to the recharge stopwatch.
                            Log.Info("NOT SMALLER THAN 0: " + targetState.GetInterval(ID));
                            skillLocator.primary.RunRecharge(targetState.GetInterval(ID));

                        }
                        Log.Info("Recharged interval: " + targetState.GetInterval(ID));

                    }

                    if (skillSlot == 2)
                    {

                        Log.Info("ID Before retrieval: " + targetState.GetInterval(ID));
                        if (targetState.GetInterval(ID) <= 0 || canUse)
                        {
                            Log.Info("yes....");
                            skillLocator.secondary.AddOneStock();
                        }
                        else
                        {
                            Log.Info("NOT SMALLER THAN 0: " + ID);
                            skillLocator.secondary.RunRecharge(targetState.GetInterval(ID));

                        }
                        Log.Info("Recharged interval: " + targetState.GetInterval(ID));
                    }

                }
                else
                {
                    Log.Error("This is not the YusukeMain state.");

                }


            }
            
        }




    }

}

