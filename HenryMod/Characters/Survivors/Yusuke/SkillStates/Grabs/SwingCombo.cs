using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.KnockbackStates;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class SwingCombo : BaseSkillState
    {
        public static float baseDuration = 1.25f;
        private float dashSpeed;

        private float maxInitialSpeed = 6f;
        private float finalSpeed = 1.5f;
        private Vector3 vector;
        private float exponent;

        private Vector3 originalCharacterForward;
        private Vector3 finalRotation;

        private float spinMinSpeed = 0.1f;
        private float spinMaxSpeed = 60f;

        private float dashTime = 0f; 
        private float duration = 1f;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private float dashFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        // target (enemy) variables
        private bool targetFound;
        private bool isBodyFound;
        private HurtBox target;
        private Transform targetModelTransform;

        //Net
        private bool hasAppliedStun;

        //animation variables
        private bool startSwingAnimation;
        private float animationTimer;
        private float spinDuration = 2f;
        private bool hasThrownEnemy;
        private float actionStopwatch = 0;
        private bool skipSwing;

        // attack vearibales
        protected float damageCoefficient = 30f;
        protected float procCoefficient = 1f;

        private KnockbackController knockbackController;
        private SwingController swingController;

        //indicator
        private Indicator indicator;
        public GameObject targetIcon;
        private bool hasSelectionBeenMade;
        public const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        public override void OnEnter()
        {
            base.OnEnter();

            UpdateDashSpeed(maxInitialSpeed, finalSpeed);
            forwardDirection = GetAimRay().direction;
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * dashSpeed;
            }
            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            PlayAnimation("FullBody, Override", "MazokuSwingDashGrab", "Slide.playbackRate", duration);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            dashTime += GetDeltaTime();
            if (!targetFound)
            {
                Dash();
                SearchForBody();
            }

            if (targetFound) SwingThrow();

            if (dashTime > duration && !targetFound)
            {
                // revert the icons. 
                outer.SetNextStateToMain();
            }


        }

        private void SwingThrow()
        {
            animationTimer += Time.fixedDeltaTime;
            if (!startSwingAnimation)
            {
                startSwingAnimation = true;
                if (target)
                {
                    if (GrabEnemy())
                    {
                        skipSwing = false;
                        Log.Info("Setting maz controller. ");
                        swingController = target.healthComponent.body.gameObject.AddComponent<SwingController>();
                        swingController.pivotTransform = FindModelChild("HandR");  // make it pivot to a different bone or empty object(set it up in the editor)
                        swingController.yusukeBody = characterBody; // giving the controller the characterbody so it knows the current forward vector for the rotation.
                        PlayAnimation("FullBody, Override", "MazokuSwing", "ShootGun.playbackRate", duration);

                        Log.Info("target model transform... ");
                        targetModelTransform = target.healthComponent.modelLocator.transform;
                        originalCharacterForward = characterDirection.forward;

                        
                    }
                    else
                    {
                        skipSwing = true;
                        
                        ReleaseSwing();
                    }
                    
                }

            }

             
            if (!skipSwing)
            {
                if (animationTimer < spinDuration) SpinYusuke();
            }

            if (animationTimer > spinDuration && !skipSwing)
            {
                ReleaseSwing();
            }

            if (hasThrownEnemy)
            {
                actionStopwatch += Time.fixedDeltaTime;

                if (inputBank.skill1.down)
                {
                    PlayAnimation("FullBody, Override", "Dash", "Roll.playbackRate", 2f);
                    Log.Info("Clicked");
                    // send to next state.
                    characterMotor.enabled = false;
                    characterDirection.enabled = false;
                    knockbackController.isFollowUpActive = true;
                    outer.SetNextState(MazMeleeFollowUp());
                }

                if (inputBank.skill2.down && isAuthority)
                {
                    Log.Info("Clicked skill 2");
                    outer.SetNextState(DemonGunFollowUp());


                }

                if (inputBank.skill4.down)
                {


                }

                //Log.Info("action timer: " + actionStopwatch);
                if (actionStopwatch > knockbackController.knockbackDuration || hasSelectionBeenMade)
                {
                    // revert the icons. 
                    outer.SetNextStateToMain();
                }
            }
            
        }

        private void ApplyStun()
        {
            // places a stun on the enemy for the attacks option timer.
            if (!hasAppliedStun)
            {
                hasAppliedStun = true;
                if (NetworkServer.active)
                {
                    target.healthComponent.GetComponent<SetStateOnHurt>()?.SetStun(spinDuration + actionStopwatch);
                }
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

        private void ReleaseSwing()
        {
            if (!hasThrownEnemy)
            {
                Vector3 forwardDir = GetAimRay().direction;
                characterDirection.forward = forwardDir;
                characterDirection.moveVector = forwardDir;
                characterMotor.moveDirection = forwardDir;
                inputBank.moveVector = Vector3.zero;

                //characterDirection.forward = originalCharacterForward;
                PlayAnimation("FullBody, Override", "SwingRelease", "Roll.playbackRate", 2f);
                if (!skipSwing) 
                { 
                    swingController.Remove();
                }
                else
                {
                    // Create another animation for this, more like a swing throw
                    PlayAnimation("FullBody, Override", "SwingRelease", "Roll.playbackRate", 2f);
                    Vector3 forceVector = GetAimRay().direction;    // for now the Aim Ray is based on the characters facing direction
                    forceVector *= 20000f;
                    AttackForce(forceVector);

                }

                // create the indicator on the body to show which enemy will receive the follow up
                if (targetIcon == null)
                {
                    targetIcon = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
                }
                indicator = new Indicator(gameObject, targetIcon);


                knockbackController = target.healthComponent.body.gameObject.AddComponent<KnockbackController>();
                knockbackController.knockbackDirection = GetAimRay().direction;
                knockbackController.knockbackSpeed = moveSpeedStat + spinMaxSpeed;
                knockbackController.pivotTransform = characterBody.transform;

                // switch the icon to show the differnet outcomes, we don't need to use new entity states as we are not storing any stock count.
                SwapIcon();

                indicator.targetTransform = target.transform;
                indicator.active = true;
                characterMotor.enabled = false;
                characterDirection.enabled = false;

                hasThrownEnemy = true;
                ApplyStun();


            }
        }

        private bool GrabEnemy()
        {

            CharacterMotor enemyMotor = target.healthComponent.body.gameObject.GetComponent<CharacterMotor>();
            Rigidbody enemyRigidBody = target.healthComponent.body.gameObject.GetComponent<Rigidbody>();

            if (enemyRigidBody)
            {
                if (enemyMotor)
                {
                    return true;
                }
                
            }
            characterBody.SetAimTimer(0.1f);
            return false;
            
        }

        private void SpinYusuke()
        {

            if (characterMotor)
            {
                characterDirection.moveVector = Vector3.zero;
                characterMotor.moveDirection = Vector3.zero;
            }

            float transitionDuration = spinDuration;
            float stopWatch = animationTimer;   // increased by time.fixedDeltaTime
            float power = 2f;

            /*
             Creating the division between the stopWatch and transitionDuration
             Exponent will gradually increase as time goes on due to the animationTimer, once reaching 1 it will stop increasing

             */
            float increase = Mathf.Clamp01(stopWatch / transitionDuration);
            exponent = Mathf.Pow(increase, power);

            // lerp will slowly increase to the max value in exponent time
            float currentValue = Mathf.Lerp(spinMinSpeed, spinMaxSpeed, exponent);

            Quaternion finalRotation = Quaternion.AngleAxis(currentValue, Vector3.up);
            characterDirection.forward = finalRotation * characterDirection.forward;

            // decelerate movement speed when spinning
            float decelerateValue = 0.95f; 
            characterMotor.velocity *= decelerateValue;


        }

        protected virtual EntityState MazMeleeFollowUp()
        {
            return new MazDivePunch
            {
                target = target
            };
        }

        protected virtual EntityState DemonGunFollowUp()
        {
            return new DemonGunFollowUp
            {
                target = target
            };
        }


        private void SwapIcon()
        {
            switch (skillLocator.primary.skillName)
            {
                case prefix + "PRIMARY_MAZOKUMELEE_NAME":
                    //skillLocator.primary.skillDef.icon = YusukeSurvivor.mazMeleeFollowUpIcon;
                    break;
                case prefix + "PRIMARY_MAZOKUGUN_NAME":
                    //skillLocator.primary.skillDef.icon = YusukeSurvivor.mazSpiritGunFollowUpIcon;
                    break;
            }

            switch (skillLocator.secondary.skillName) {

                case prefix + "SECONDARY_MAZOKUGUN_NAME":
                    //skillLocator.secondary.skillDef.icon = YusukeSurvivor.mazSpiritGunFollowUpIcon;
                    break;
                case prefix + "SECONDARY_MAZBACKTOBACK_NAME":
                    //skillLocator.secondary.skillDef.icon = YusukeSurvivor.mazSpiritShotgunFollowUpIcon;
                    break;
            }

            switch (skillLocator.special.skillName) {
                case prefix + "UTILITY_MAZ_BLINK_DASH_NAME":
                    //skillLocator.special.skillDef.icon = YusukeSurvivor.mazSpiritMegaFollowUpIcon;
                    break;

            }
        }

        private void RevertIcons()
        {
            switch (skillLocator.primary.skillName)
            {
                case prefix + "PRIMARY_MAZOKUMELEE_NAME":
                    //skillLocator.primary.skillDef.icon = YusukeSurvivor.mazMeleeFollowUpIcon;
                    break;
                case prefix + "PRIMARY_MAZOKUGUN_NAME":
                    //skillLocator.primary.skillDef.icon = YusukeSurvivor.mazSpiritGunFollowUpIcon;
                    break;
            }

            switch (skillLocator.secondary.skillName)
            {

                case prefix + "SECONDARY_MAZOKUGUN_NAME":
                    //skillLocator.secondary.skillDef.icon = YusukeSurvivor.mazSpiritGunFollowUpIcon;
                    break;
                case prefix + "SECONDARY_MAZBACKTOBACK_NAME":
                    //skillLocator.secondary.skillDef.icon = YusukeSurvivor.mazSpiritShotgunFollowUpIcon;
                    break;
            }

            switch (skillLocator.special.skillName)
            {
                case prefix + "UTILITY_MAZ_BLINK_DASH_NAME":
                    //skillLocator.special.skillDef.icon = YusukeSurvivor.mazSpiritMegaFollowUpIcon;
                    break;

            }
        }

        private void SearchForBody()
        {
            if (!isBodyFound)
            {
                Vector3 sphereCenter = transform.position + transform.forward;
                Collider[] capturedBody;

                capturedBody = Physics.OverlapSphere(sphereCenter, 2f, LayerIndex.entityPrecise.mask);

                List<Collider> capturedColliders = capturedBody.ToList();

                
                foreach (Collider result in capturedColliders)
                {
                    
                    HurtBox capturedHurtbox = result.GetComponent<HurtBox>();

                    if (capturedHurtbox.healthComponent && capturedHurtbox.healthComponent.alive)
                    {
                        if(capturedHurtbox.healthComponent.gameObject != gameObject)
                        {
                            target = capturedHurtbox;
                            isBodyFound = true;
                            targetFound = true;
                            break;
                        }
                        
                    }
                    
                }

            }

        }

        private void Dash()
        {
            // still need to figure out how to create a better dash instead of relying on the template
            UpdateDashSpeed(maxInitialSpeed, finalSpeed);
            forwardDirection = GetAimRay().direction;
            characterDirection.moveVector = forwardDirection;

            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dashFOV, 60f, fixedAge / duration);

            dashTime += Time.fixedDeltaTime;
            Vector3 normalized = (transform.position - previousPosition).normalized;
            if (characterMotor && characterDirection && normalized != Vector3.zero)
            {
                vector = normalized * dashSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;

            }

            characterMotor.velocity = vector;
            previousPosition = transform.position;


        }

        private void UpdateDashSpeed(float max, float final)
        {
            dashSpeed = (moveSpeedStat) * Mathf.Lerp(max, final, fixedAge / duration);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();
            if (target)  // removes indicators if present
            {
                if (indicator != null) indicator.active = false;
            }
            characterMotor.velocity.y = 0f;
            characterMotor.enabled = true;
            characterDirection.enabled = true;
            RevertIcons();

            Log.Info("Exiting the Swing Combo state. ");

        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
