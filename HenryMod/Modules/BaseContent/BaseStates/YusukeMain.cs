using EntityStates;
using IL.RoR2.Achievements.FalseSon;
using Rewired.Demos;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.Modules.BaseStates
{
    public class YusukeMain : GenericCharacterMain
    {
        // animation layer that references the layer index for animations
        public enum AnimationLayerIndex
        {
            Body,
            Hurt,
            GunCharge,
            ShotgunCharge,
            WaveCharge,
            MegaCharge,
            Mazoku
        }

        private GameObject spiritCuffEffect = null;

        Vector3 currentPosition;
        Vector3 latestGroundPosition;

        private float timer;
        private bool hasIdleBegun;
        private bool hasIdleEnded;
        private int interval = 0;

        // follow-up intervals
        private float meleeRechargeInterval;
        private float spiritGunRechargeInterval;
        private float spiritShotGunRechargeInterval;

        private float meleeStartTimer = 30f;
        private float spiritGunStartTimer = 20f;
        private float spiritShotGunStartTimer = 10f;

        public bool isPrimaryReady;
        public bool isSecondaryReady;

        //private int previousSkillSlot;
        //private int previousID;



        private List<int> skillSlotList = new List<int>();
        private List<int> IDList = new List<int>();

        public List<int> targetsList;

        private int previousStock;
        private int previousSecondaryStock;

        // Y-axis ray (for spirit gun mega)
        private float maxDistance;
        private Ray yDistanceRay;
        private RaycastHit hit;

        // mazoku demon gun timer
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;
        private bool decrementPenaltyTimer;
        public float penaltyTimer;
        private bool hasRevertedDemonGunMega;

        //used for Animation layers
        private Animator animator = null;
        private bool isRestAnimationActive = false;
        private MazokuComponent mazokuComponent;

        private SpiritCuffComponent spiritCuffComponent;
        private GameObject spiritCuffObject;
        private GameObject spiritCuffEffectPrefab;

        private string playbackRateParam = "animInterrupt.playbackRate";

        public void Start()
        {
            
        }


        // Idle stuff for Yusuke
        public override void OnEnter()
        {
            base.OnEnter();
            // setting the y-distance ray for spirit gun mega

            // getting the animator component for the layer switching
            animator = GetModelAnimator();

            meleeRechargeInterval = 0f;
            spiritGunRechargeInterval = 0f;
            spiritShotGunRechargeInterval = 0f;

            isPrimaryReady = true;
            isSecondaryReady = true;
            mazokuComponent = characterBody.master.gameObject.GetComponent<MazokuComponent>();
            spiritCuffComponent = characterBody.gameObject.GetComponent<SpiritCuffComponent>();

            // used to control the spirit cuff effect
            spiritCuffEffectPrefab = YusukeAssets.spiritCuffEffect;
            if (!spiritCuffObject) spiritCuffObject = YusukePlugin.CreateEffectObject(spiritCuffEffectPrefab, FindModelChild("mainPosition"));
            spiritCuffObject.SetActive(false);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            currentPosition = transform.position;
            if (isGrounded) latestGroundPosition = transform.position;

            //checking for any movement, including skill activations 
            if (!CheckForMovement() && !CheckForInputs() && rigidbody.velocity == Vector3.zero) StartIdleTime();
            if (CheckForMovement() || CheckForInputs() || !rigidbody.velocity.Equals(Vector3.zero)) ResetIdleTime();


            if (hasIdleBegun)
            {
                timer += Time.deltaTime;
            }

            if (hasIdleEnded)
            {
                if(isRestAnimationActive) PlayAnimation("FullBody, Override", "BufferEmpty", playbackRateParam, 1f);
                isRestAnimationActive = false;
            }

            if (timer > 5)
            {
                // play the animation required. 
                if (!isRestAnimationActive)
                {
                    isRestAnimationActive = true;
                    PlayAnimation("FullBody, Override", "IdleToRest", playbackRateParam, 1f);
                }
            }


            CheckFollowUpIntervals();
            TransformProperties();
            CheckPenaltyTimer();

            Chat.AddMessage("melee timer: "+meleeRechargeInterval);
            Chat.AddMessage("primary move status: "+isPrimaryReady);
            Chat.AddMessage("shotgun timer: " + spiritShotGunRechargeInterval);
            Chat.AddMessage("secondary move status: " + isSecondaryReady);
            Chat.AddMessage("");
            Chat.AddMessage("");

        }

        private void CheckPenaltyTimer()
        {
            if (penaltyTimer > 0) {
                if (mazokuComponent != null)
                {
                    if (mazokuComponent.previousValue == mazokuComponent.maxMazokuValue)
                    {
                        //Log.Info("Penalty timer inside MAIN STATE: " + penaltyTimer);
                        decrementPenaltyTimer = true;
                    }
                }
            }
            else
            {
                decrementPenaltyTimer = false;
            }
               
            if(decrementPenaltyTimer) penaltyTimer -= Time.fixedDeltaTime;
        }

        // transformation state
        private void TransformProperties()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                
                if (mazokuComponent != null) 
                {
                    if (mazokuComponent.previousValue == mazokuComponent.maxMazokuValue)
                    {
                        if (isGrounded)
                        {
                            if(penaltyTimer <= 0)
                            {
                                outer.SetNextState(new BeginMazokuTransformation());
                                // switching to the mazoku AnimationLayer
                                SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, true);
                                hasRevertedDemonGunMega = false;
                            }
                            else
                            {
                                if (penaltyTimer != 0) Log.Info("Penalty Timer: " + Mathf.Round(penaltyTimer));
                            }
                            
                        }
                        else
                        {
                            Log.Info("User needs to be grounded. ");
                        }
                    }
                    else
                    {
                        Log.Info("Mazoku is not ready... sorry.");
                        
                    }
                }
            }


            if(mazokuComponent != null)
            {
                
                // checks if the reverse boolean is true, which indicates that the mazoku transformation duration has ended
                if (mazokuComponent.startReverse)
                {
                    mazokuComponent.startReverse = false;
                    Log.Info("Switching skills back from maz.");
                    // Changing back the mazoku animation set
                    SwitchMovementAnimations((int)AnimationLayerIndex.Mazoku, false);
                    outer.SetNextState(SwitchBackSkills(2));    // switching reverse so it reverts back to the skills it needs

                }

                if (!mazokuComponent.hasTransformed && skillLocator.special.skillNameToken == prefix + "SPECIAL_MAZ_MEGA_NAME" && !inputBank.skill4.down)
                {
                    if (!hasRevertedDemonGunMega)
                    {
                        hasRevertedDemonGunMega = true;
                        skillLocator.special.UnsetSkillOverride(gameObject, YusukeSurvivor.demonGunMega, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.special.SetSkillOverride(gameObject, YusukeSurvivor.specialSpiritGunMega, GenericSkill.SkillOverridePriority.Contextual);

                    }

                }

            }

            // cheap way of doing it, but I spent too long trying to figure out another way of doing it. If it works, it works.
            if(spiritCuffComponent != null)
            {
                if (spiritCuffComponent.hasReleased)
                {
                    spiritCuffObject.SetActive(true);
                }
                else
                {
                    spiritCuffObject.SetActive(false);
                }
            }
        }

        protected virtual EntityState SwitchBackSkills(int ID)
        {
            return new SwitchSkills
            {
                switchID = ID,
            };
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        // returns the animators status on movement. 
        private bool CheckForMovement()
        {
            return animator.GetBool("isMoving");
        }

        private bool CheckForInputs()
        {
            if (inputBank.skill1.down || inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down || inputBank.jump.down || inputBank.sprint.down)
                return true;
            else
                return false;
        }

        private void StartIdleTime()
        {
            if (!hasIdleBegun)
            {
                hasIdleEnded = false;
                hasIdleBegun = true;
                //Chat.AddMessage("Idle...");
            }
        }

        private void ResetIdleTime()
        {
            if (!hasIdleEnded)
            {
                hasIdleEnded = true;
                hasIdleBegun = false;
                //Chat.AddMessage("Movement...");
            }

            timer = 0f;
            interval = 0;
        }

        public bool CompareYAxis()
        {
            yDistanceRay = new Ray(transform.position, Vector3.down);
            maxDistance = 5000f;
            float value = 0;
            if (Physics.Raycast(yDistanceRay, out hit, maxDistance))
            {
                value = hit.distance;
            }

            //Math.Abs(value);
            //Chat.AddMessage("Y-Axis distance: " + value);

            // 0 in this case means there is no ground underneath Yusuke (skybox?)
            if (value >= 40.0 || value == 0)
            {
                return true;
            }
            else
            {
                return false;
            }



        }

        // used for the upgraded spirit wave move, whenever the skill is used, it will start the countdown
        private void CheckFollowUpIntervals()
        {
            if (meleeRechargeInterval > 0) meleeRechargeInterval -= Time.fixedDeltaTime;
            if (spiritGunRechargeInterval > 0) spiritGunRechargeInterval -= Time.fixedDeltaTime;
            if (spiritShotGunRechargeInterval > 0) spiritShotGunRechargeInterval -= Time.fixedDeltaTime;

            if (meleeRechargeInterval < 0) CheckPreviousAttackValues(1);
            if (spiritGunRechargeInterval < 0) CheckPreviousAttackValues(2);
            if (spiritShotGunRechargeInterval < 0) CheckPreviousAttackValues(3);
        }

        private void CheckPreviousAttackValues(int moveID)
        {

            

            for (int index = 0; index < skillSlotList.Count; index++) 
            {

                int prevSlot = skillSlotList[index];
                //int index = skillSlotList.IndexOf(slot);
                int prevID = IDList[index];

                // checking the availability of the moves if they are on cooldown or not
                if (prevSlot == 1 && prevID == 1)
                {
                    if (moveID == prevID)
                    {
                        Log.Info("PRIMARY HAS BEEN RESTORED");
                        isPrimaryReady = true;
                        skillSlotList.RemoveAt(index); 
                        IDList.RemoveAt(index);

                        Log.Info("Inside SkillSlotList AFTER removal: ");
                        foreach (int s in skillSlotList)
                        {
                            Log.Info(s);
                        }

                        Log.Info("Inside IDList AFTER removal: ");
                        foreach (int s in IDList)
                        {
                            Log.Info(s);
                        }
                    }





                }
                if (prevSlot == 1 && prevID == 2)
                {
                    if (moveID == prevID)
                    {
                        Log.Info("PRIMARY HAS BEEN RESTORED");
                        isPrimaryReady = true;
                        skillSlotList.RemoveAt(index);
                        IDList.RemoveAt(index);

                        Log.Info("Inside SkillSlotList AFTER removal: ");
                        foreach (int s in skillSlotList)
                        {
                            Log.Info(s);
                        }

                        Log.Info("Inside IDList AFTER removal: ");
                        foreach (int s in IDList)
                        {
                            Log.Info(s);
                        }
                    }




                }
                if (prevSlot == 2 && prevID == 2)
                {
                    if (moveID == prevID)
                    {
                        Log.Info("SECONDARY HAS BEEN RESTORED");
                        isSecondaryReady = true;
                        skillSlotList.RemoveAt(index);
                        IDList.RemoveAt(index);

                        Log.Info("Inside SkillSlotList AFTER removal: ");
                        foreach (int s in skillSlotList)
                        {
                            Log.Info(s);
                        }

                        Log.Info("Inside IDList AFTER removal: ");
                        foreach (int s in IDList)
                        {
                            Log.Info(s);
                        }
                    }




                }
                if (prevSlot == 2 && prevID == 3)
                {
                    if (moveID == prevID)
                    {
                        Log.Info("SECONDARY HAS BEEN RESTORED");
                        isSecondaryReady = true;
                        skillSlotList.RemoveAt(index);
                        IDList.RemoveAt(index);

                        Log.Info("Inside SkillSlotList AFTER removal: ");
                        foreach (int s in skillSlotList)
                        {
                            Log.Info(s);
                        }

                        Log.Info("Inside IDList AFTER removal: ");
                        foreach (int s in IDList)
                        {
                            Log.Info(s);
                        }
                    }




                }

            }



            
        }

        // used for the upgraded spirit wave move
        public void StartCoolDown(int skillSlot,int ID)
        {
            // once a move is used, it resets the cooldown interval
            Log.Info("Starting cooldown");
            if(ID == 1) meleeRechargeInterval = meleeStartTimer;
            if(ID == 2) spiritGunRechargeInterval = spiritGunStartTimer;
            if(ID == 3) spiritShotGunRechargeInterval = spiritShotGunStartTimer;

            if (skillSlot == 1) 
            {
                Log.Info("PRIMARY IS RECHARGING");
                isPrimaryReady = false;
            }
            if (skillSlot == 2) 
            {
                Log.Info("SECONDARY IS RECHARGING");
                isSecondaryReady = false;
            }
            if (skillSlot == 3)
            {
                Log.Info("SECONDARY SHOTGUN IS RECHARGING");
                isSecondaryReady = false;
            }


            // adds the skills to the list, this is mainly used so that more moves can be placed on cooldown instead of only one
            skillSlotList.Add(skillSlot);
            IDList.Add(ID);

            Log.Info("Inside SkillSlotList BEFORE removal: ");
            foreach (int s in skillSlotList)
            {
                Log.Info(s);
            }

            Log.Info("Inside IDList BEFORE removal: ");
            foreach (int s in IDList)
            {
                Log.Info(s);
            }


        }

        // return the difference between the countdown and the start time as RunRecharge is being used to recharge the skill
        public float GetInterval(int ID)
        {
            if (ID == 1) return meleeStartTimer - meleeRechargeInterval;
            if (ID == 2) return spiritGunStartTimer - spiritGunRechargeInterval;
            if (ID == 3) return spiritShotGunStartTimer - spiritShotGunRechargeInterval;
            return 0;
        }

        // gets the move status to check whether the move can be used based on the interval
        public bool GetMoveStatus(int skillSlot)
        {
            if(skillSlot == 1) return isPrimaryReady;
            if(skillSlot == 2) return isSecondaryReady;
            return false;
        }

        // sets the stock of the move for the primary or secondary move 
        public void SetStock(int stockValue, int skillSlot)
        {
            if(skillSlot == 1) previousStock = stockValue;
            if(skillSlot == 2) previousSecondaryStock = stockValue;
        }

        // retrives the stock, that way it doesn't reset the entire move stock, and doesn't max them out
        public int RetrieveStock(int skillSlot)
        {
            if(skillSlot == 1) return previousStock;
            if(skillSlot == 2) return previousSecondaryStock;
            return 0;
        }

        /* whenever there is a change in animations in the layer, this method will be called, this is used whenever there is an animation set that 
            needs to overwrite the current body layer animation set (walk, run, jump, etc)*/
        public void SwitchMovementAnimations(int animationLayerIndex, bool isSwitching)
        {
            

            if (animator && isSwitching)
            {
                animator.SetLayerWeight(animationLayerIndex, 1f);
                Log.Info("Layer " + animationLayerIndex + " has been switched on. ");

            }
            else
            {
                animator.SetLayerWeight(animationLayerIndex, 0f);
                Log.Info("Layer " + animationLayerIndex + " has been switched off. ");
            }
        }

    }
}
