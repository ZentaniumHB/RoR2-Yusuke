using EntityStates;
using EntityStates.Jellyfish;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
        private float finalSpeed = 1f;

        private float dashTime; 
        private float duration = 1f;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private float dashFOV = EntityStates.Commando.DodgeState.dodgeFOV;


        private bool targetFound;
        private bool isBodyFound;
        private HurtBox target;

        //animation variables
        private bool startSwingAnimation;
        private float animationTimer;
        private bool canThrowEnemy;
        private float actionStopwatch = 0;

        private DivePunchController DivePunchController;
        private KnockbackController knockbackController;
        private MazokuGrabController mazokuGrabController;

        //indicator
        private Indicator indicator;
        public GameObject targetIcon;
        private bool hasSelectionBeenMade;
        public const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        public override void OnEnter()
        {
            base.OnEnter();

            forwardDirection = GetAimRay().direction;
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * dashSpeed;
            }
            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (!targetFound)
            {
                Dash();
                SearchForBody();
            }

            if (targetFound) SwingThrow();


        }

        private void SwingThrow()
        {
            animationTimer += Time.fixedDeltaTime;
            if (!startSwingAnimation)
            {
                startSwingAnimation = true;
                if (target)
                {
                    
                    Log.Info("Setting maz controller. ");
                    mazokuGrabController = target.healthComponent.body.gameObject.AddComponent<MazokuGrabController>();
                    mazokuGrabController.pivotTransform = FindModelChild("HandR");  // make it pivot to a different bone or empty object(set it up in the editor)
                    PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", 2f);
                    Log.Info("maz settings done. ");
                    
                }
                
                
                // maybe addtimedbuff
            }

            //Log.Info("Animation Timer:");
            if (animationTimer > 2f)
            {
                actionStopwatch += Time.fixedDeltaTime;
                if (!canThrowEnemy)
                {
                    
                    mazokuGrabController.Remove();

                    // create the indicator on the body to show which enemy will receive the follow up
                    if (targetIcon == null)
                    {
                        targetIcon = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
                    }
                    indicator = new Indicator(gameObject, targetIcon);
                    indicator.targetTransform = target.transform;


                    knockbackController = target.healthComponent.body.gameObject.AddComponent<KnockbackController>();
                    knockbackController.knockbackDirection = GetAimRay().direction;
                    knockbackController.knockbackSpeed = moveSpeedStat * 1.8f;
                    knockbackController.pivotTransform = characterBody.transform;

                    // switch the icon to show the differnet outcomes, we don't need to use new entity states as we are not storing any stock count.
                    SwapIcon();
                    indicator.active = true;
                    characterMotor.enabled = false;
                    characterDirection.enabled = false;

                    canThrowEnemy = true;


                }

                if (inputBank.skill1.down)
                {
                    PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", 2f);
                    Log.Info("Clicked");
                    // send to next state.
                    characterMotor.enabled = false;
                    characterDirection.enabled = false;
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

        protected virtual EntityState MazMeleeFollowUp()
        {
            return new MazMeleeFollowUp
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
                Vector3 vector = normalized * dashSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;
                characterMotor.velocity = vector;
            }

            
            previousPosition = transform.position;


        }

        private void UpdateDashSpeed(float max, float final)
        {
            dashSpeed = (moveSpeedStat * 1.2f) * Mathf.Lerp(max, final, fixedAge / duration);
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

        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
