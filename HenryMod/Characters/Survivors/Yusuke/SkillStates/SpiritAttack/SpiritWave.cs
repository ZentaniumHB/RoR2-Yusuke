using EntityStates;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using YusukeMod;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Modules.BaseStates;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.SkillStates
{
    public class SpiritWave : BaseSkillState
    {
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

        public float maxTrackingDistance = 10f;
        public float maxTrackingAngle = 60f;

        public float actionStopwatch = 0.0f;
        public float actionTimeDuration = 0.8f;

        // attack settings

        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float pushForce = 350f;
        protected Vector3 bonusForce = Vector3.zero;
        protected string hitboxGroupName = "SwordGroup";
        public GameObject hitEffectPrefab = YusukeAssets.swordHitImpactEffect;
        protected NetworkSoundEventIndex impactSound = YusukeAssets.swordHitSoundEvent.index;
        private bool hasPunched;

        // animation settings
        protected Animator animator;
        protected float hitStopDuration = 0.012f;


        private bool collision;
        private BullseyeSearch search = new BullseyeSearch();
        private SphereSearch bodySearch = new SphereSearch();
        private HurtBox target;
        public List<Collider> bodyList;
        private bool isBodyFound = false;
        private Indicator indicator;
        public GameObject targetIcon;
        private KnockbackController knockbackController;
        private Vector3 vector;
        private OverlapAttack attack;


        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;

        private Vector3 previousPosition;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            knockbackController = new KnockbackController();

            duration = Mathf.Lerp(minDuration, maxDuration, charge);
            Log.Info("Duration: " + duration);
            forwardDirection = GetAimRay().direction;

            chargedMaxSpeed = GetChargedMax(charge);
            chargedFinalSpeed = GetChargedFinal(charge);

            UpdateDashSpeed(chargedMaxSpeed, chargedFinalSpeed);

            if (characterMotor && characterDirection)
            {

                characterMotor.velocity = forwardDirection * dashSpeed;
                
            }


            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            


        }

        /*private Vector3 UpdateDashSpeed(Vector3 currentVelocity)
        {
            return currentVelocity + forwardDirection * Mathf.Lerp(minDashSpeed, maxDashSpeed, charge);
        }*/

        private void UpdateDashSpeed(float max, float final)
        {
            dashSpeed = (moveSpeedStat * 1.2f) * Mathf.Lerp(max, final, fixedAge / duration);
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
            //dashSpeed = UpdateDashSpeed(characterMotor.velocity);

            UpdateDashSpeed(chargedMaxSpeed, chargedFinalSpeed);
            //Log.Info("dashSpeed: " + dashSpeed);

/*            hitPauseTimer -= Time.deltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                RemoveHitstop();
            }*/


            if (target)
            {
                SearchForPhysicalBody();
                if (isBodyFound)
                {
                    ThrowPunch();
                    actionStopwatch += Time.fixedDeltaTime;
                    if ((bool)target.healthComponent && target.healthComponent.alive && !collision)
                    {

                        if (targetIcon == null)
                        {
                            targetIcon = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
                        }
                        indicator = new Indicator(gameObject, targetIcon);

                        // add controller to the enemy that is marked
                        knockbackController = target.healthComponent.body.gameObject.AddComponent<KnockbackController>();
                        knockbackController.moveID = 1;
                        knockbackController.knockbackDirection = characterMotor.velocity;

                        Vector3 fr = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
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
                characterBody.isSprinting = true;
                if (characterDirection) characterDirection.forward = forwardDirection;
                if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);

                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    vector = normalized * dashSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;


                    characterMotor.velocity = vector;

                }
                previousPosition = transform.position;
            }

            if (collision)
            {
                if (isAuthority)
                {
                    if (hasPunched)
                    {
                        //need to do attack Authority thing...
                        OnHitEnemyAuthority();

                    }
                    
                }

                if (inputBank.jump.justPressed)
                {
                    actionStopwatch = actionTimeDuration;
                }

            }
           


            if (isAuthority && fixedAge >= duration)
            {
                if (!collision)
                {
                    outer.SetNextStateToMain();
                    return;
                }

                if(collision && actionStopwatch >= actionTimeDuration) {
                    outer.SetNextStateToMain();
                    return;
                }



            }
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

                attack.Fire();
                hasPunched = true;
            }
            
        }

        public override void OnExit()
        {
            base.OnExit();
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            if(target && isBodyFound)
                indicator.active = false;

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }


        private void SearchForTarget()
        {

            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = false;
            search.searchOrigin = transform.position;
            search.searchDirection = forwardDirection;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);

            target = search.GetResults().FirstOrDefault();
        }


        private void SearchForPhysicalBody()
        {
            
            Collider[] capturedBody = Physics.OverlapSphere(transform.position, 2f, LayerIndex.entityPrecise.mask);
            List<Collider> capturedColliders = capturedBody.ToList();

            foreach (Collider result in capturedColliders)
            {
                HurtBox capturedHurtbox = result.GetComponent<HurtBox>();
                if (capturedHurtbox == target)
                {
                    isBodyFound = true;

                }
                else
                {
                    //Log.Info("not the same box");
                }

            }

        }

        
    }

}

