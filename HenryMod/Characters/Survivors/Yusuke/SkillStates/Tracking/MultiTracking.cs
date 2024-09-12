using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UIElements.UIR;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking
{
    public class MultiTracking : BaseSkillState
    {
        public static Color Green = new Color(0.0f, 0.5f, 0f, 1f);

        public float maxTrackingDistance = 30f;
        public float maxTrackingAngle = 30f;

        private GameObject trackingPrefab;
        private Indicator indicator;
        
        private float trackerUpdateStopwatch;
        private float trackerUpdateFrequency = 20f;
        public int pelletCounter = 12;

        private bool hasMaxSoundPlayed;
        public static string hitSoundString = "HenryRoll";

        public List<HurtBox> targetsList;
        public List<HurtBox> previousHurtBoxes = new List<HurtBox>();
        private Dictionary<HurtBox, IndicatorInfo> targetIndicators;
        private BullseyeSearch search;


        private struct IndicatorInfo
        {
            //public int refCount;

            public ShotgunPelletIndicator indicator;
        }

        private class ShotgunPelletIndicator : Indicator
        {
            //public int pelletCount;


            public ShotgunPelletIndicator(GameObject owner, GameObject visualizerPrefab)
            : base(owner, visualizerPrefab)
            {
            }
        }

        


        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                targetsList = new List<HurtBox>();
                targetIndicators = new Dictionary<HurtBox, IndicatorInfo>();
                search = new BullseyeSearch();
                Log.Info("Enter Compete");
            }

        }

        public override void FixedUpdate()
        {

            base.FixedUpdate();

            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                //Log.Info("NEEDS TO BE CLEANED");
                trackerUpdateStopwatch = 0f;

                CleanTargetsList(previousHurtBoxes);
                SearchForTarget(out var currentTarget);

                if (currentTarget.Count > 0)
                {
                    AddTagToEnemy(currentTarget);
                    //previousHurtBoxes = currentTarget; // this is used for the clean target, checking if hurtbox is in range

                    
                    Log.Info("Length of previousHurtBoxes (AFTER POINTING): " + previousHurtBoxes.Count);
                    Log.Info("The currentTarget AFTER THE POINTER WAS DONE: " + currentTarget.Count);
                }
                

            }

            if (!IsKeyDown() && isAuthority)
            {
                outer.SetNextStateToMain();
                return;

            }


            /*if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                
                trackerUpdateStopwatch = 0f;
                targetResults.Clear();
                //Log.Info("searching...");
                SearchForMultipeTargets(inputBank.GetAimRay());
                //Log.Info("displaying results");
                foreach (HurtBox singleTarget in targetResults)
                {
                    
                    indicator.targetTransform = (singleTarget ? singleTarget.transform : null);
                    
                }


            }*/

        }

        protected virtual bool IsKeyDown()
        {
            return IsKeyDownAuthority();
        }

        private void CleanTargetsList(List<HurtBox> previousHurtBoxes)
        {
            Log.Info("Number of targets marked: " + targetsList.Count);
            Log.Info("Number of targets Indicators: " + targetIndicators.Count);

            if(previousHurtBoxes.Count == 0)
            {
                Log.Info("Length of previousHurtBoxes: 0");
            }
            else
            {
                Log.Info("Length of previousHurtBoxes: " + previousHurtBoxes.Count);
            }

            

            for (int i = targetsList.Count - 1; i >= 0; i--)
            {
                
                HurtBox hurtBox = targetsList[i];
                if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive)
                {
                    // Enemy is most likely dead, remove tag 
                    RemoveTagOnEnemy(i);
                    
                }

                if (previousHurtBoxes.Count > 0)
                {

                    Log.Info("previousHurtbox exists,checking range");
                    UpdateScan(out List<HurtBox> currentlist);
                    if (!currentlist.Contains(previousHurtBoxes[i]))
                    {
                       //previousHurtbox no longer in range, removing
                        RemoveTagOnEnemy(i);
                        
                    }
                    else
                    {
                        //previousHurtbox still in range
                    }


                }
                   
            }
        }

        private void RemoveTagOnEnemy(int i)
        {
            HurtBox key = targetsList[i];

            targetsList.RemoveAt(i);
            previousHurtBoxes.RemoveAt(i);
            if(targetIndicators.TryGetValue(key, out var value))
            {
                //removes the target on them
                value.indicator.active = false;
                targetIndicators.Remove(key);
                if(hasMaxSoundPlayed)
                    hasMaxSoundPlayed = false;

            }


            Log.Info("Number of targets marked (AFTER DELETION): " + targetsList.Count);
            Log.Info("Number of targets Indicators (AFTER DELETION): " + targetIndicators.Count);
            Log.Info("Length of previousHurtBoxes (AFTER DELETION): " + previousHurtBoxes.Count);

        }


        private void AddTagToEnemy(List<HurtBox> hurtBox)
        {

            Log.Info("Number of targets marked (BEFORE ADDING): " + targetsList.Count);
            Log.Info("Number of targets Indicators (BEFORE ADDING): " + targetIndicators.Count);
            Log.Info("Length of previousHurtBoxes (BEFORE ADDING): " + previousHurtBoxes.Count);

            foreach (HurtBox box in hurtBox)
            {
                if(targetsList.Count != pelletCounter)
                {
                    // if the target hitbox doesn't exist in the list already, add tag.
                    if (!targetIndicators.TryGetValue(box, out var value))
                    {
                        // Enemy does not exist - adding tag 
                        targetsList.Add(box);
                        IndicatorInfo indicatorInfo = default(IndicatorInfo);
                        indicatorInfo.indicator = new ShotgunPelletIndicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
                        value = indicatorInfo;  // the image that is shown on the enemy
                        value.indicator.targetTransform = box.transform;
                        value.indicator.active = true;
                        // add the hurtbox to the previousList, that will be used when updating the scan.
                        previousHurtBoxes.Add(box);
                    }
                    // still add them to the list 
                    targetIndicators[box] = value;
                }
                else
                {
                    if (!hasMaxSoundPlayed)
                    {
                        Util.PlaySound(hitSoundString, gameObject);
                        hasMaxSoundPlayed = true;
                    }
                    
                    Log.Info("Max pellets reached");
                }
                
                

            }

            Log.Info("Number of targets marked (AFTER ADDING): " + targetsList.Count);
            Log.Info("Number of targets Indicators (AFTER ADDING): " + targetIndicators.Count);
            Log.Info("Length of previousHurtBoxes (AFTER ADDING): " + previousHurtBoxes.Count);

        }




        public GameObject GetTargets()
        {
            if (GetTargets() != null)
            {
                return GetTargets().gameObject;
            }
            return null;
        }



        public void SearchForTarget(out List<HurtBox> currentHurtbox)
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
            // for everything that was found

            List<HurtBox> totalEnemies = new List<HurtBox>();
            foreach (HurtBox result in search.GetResults())
            {
                // if it has a healthbar and they are alive
                if ((bool)result.healthComponent && result.healthComponent.alive)
                {
                    totalEnemies.Add(result);
                    
                }
                    
            }
            
            currentHurtbox = totalEnemies;
            return;


        }

        public void UpdateScan(out List<HurtBox> currentList)
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
            
            currentList = search.GetResults().ToList<HurtBox>();
            return ;


        }

        public override void OnExit()
        {
            // clearing completely
            if (targetIndicators != null)
            {
                foreach (KeyValuePair<HurtBox, IndicatorInfo> targetIndicator in targetIndicators)
                {
                    targetIndicator.Value.indicator.active = false;
                }
            }
        }

    }
}
