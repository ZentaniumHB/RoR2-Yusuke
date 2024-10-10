using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp;

namespace YusukeMod.Modules.BaseStates
{
    public class YusukeMain : GenericCharacterMain
    {
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


        public void Start()
        {
            
        }


        // Idle stuff for Yusuke
        public override void OnEnter()
        {
            base.OnEnter();
            // setting the y-distance ray for spirit gun mega

            meleeRechargeInterval = 0f;
            spiritGunRechargeInterval = 0f;
            spiritShotGunRechargeInterval = 0f;

            isPrimaryReady = true;
            isSecondaryReady = true;

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            currentPosition = transform.position;
            if (isGrounded) latestGroundPosition = transform.position;

            //checking for any movement, including skill activations 
            if (CheckIdle() && rigidbody.velocity == Vector3.zero) StartIdleTime();
            if (!CheckIdle() || !rigidbody.velocity.Equals(Vector3.zero)) ResetIdleTime();

            if (hasIdleBegun)
            {
                timer += Time.deltaTime;
                //Chat.AddMessage("Time standing still: " +timer);
            }

            if (timer > 5)
            {
                // play animation when...
                //Chat.AddMessage("*whistling*");

            }


            CheckFollowUpIntervals();
            TransformInput();

            Chat.AddMessage("melee timer: "+meleeRechargeInterval);
            Chat.AddMessage("primary move status: "+isPrimaryReady);
            Chat.AddMessage("shotgun timer: " + spiritShotGunRechargeInterval);
            Chat.AddMessage("secondary move status: " + isSecondaryReady);
            Chat.AddMessage("");
            Chat.AddMessage("");

        }

        // transformation state
        private void TransformInput()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                MazokuComponent maz = characterBody.master.gameObject.GetComponent<MazokuComponent>();
                if (maz != null) 
                {
                    if (maz.previousValue == maz.maxMazokuValue)
                    {
                        if (isGrounded)
                        {
                            outer.SetNextState(new BeginMazokuTransformation());
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
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        private bool CheckIdle()
        {
            if (inputBank.skill1.down || inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down || inputBank.interact.down || inputBank.jump.down || inputBank.sprint.down || inputBank.activateEquipment.down)
                return false;
            else
            {
                return true;
            }

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


    }
}
