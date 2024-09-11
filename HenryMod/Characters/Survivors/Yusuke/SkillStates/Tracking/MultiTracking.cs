using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking
{
    public class MultiTracking : BaseSkillState
    {
        public static Color Green = new Color(0.0f, 0.5f, 0f, 1f);

        public float maxTrackingDistance = 24;
        public float maxTrackingAngle = 24f;

        private GameObject trackingPrefab;
        private Indicator indicator;
        

        private float trackerUpdateStopwatch;
        private float trackerUpdateFrequency = 10f;

        public List<HurtBox> targetsList;
        private Dictionary<HurtBox, IndicatorInfo> targetIndicators;
        private BullseyeSearch search;


        private struct IndicatorInfo
        {
            public int refCount;

            public ShotgunPelletIndicator indicator;
        }

        private class ShotgunPelletIndicator : Indicator
        {
            public int pelletCount;


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
                Log.Info("NEEDS TO BE CLEANED");
                trackerUpdateStopwatch = 0f;
                CleanTargetsList();
                SearchForTarget(out var currentTarget);

                if ((bool)currentTarget)
                {
                    AddTagToEnemy(currentTarget);
                    //CheckIfVisible(currentTarget);
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

        private void CleanTargetsList()
        {
            Log.Info("Number of targets marked: " + targetsList.Count);
            Log.Info("Number of targets Indicators: " + targetIndicators.Count);
            for (int i = targetsList.Count - 1; i >= 0; i--)
            {
                
                HurtBox hurtBox = targetsList[i];
                if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive)
                {
                    // Enemy is most likely dead, remove tag 
                    RemoveTagOnEnemy(i);
                }
                   
            }
        }

        private void RemoveTagOnEnemy(int i)
        {
            HurtBox key = targetsList[i];
            targetsList.RemoveAt(i);
            if(targetIndicators.TryGetValue(key, out var value))
            {
                value.indicator.active = false;
                targetIndicators.Remove(key);

            }
            
        }

        private void CheckIfVisible(HurtBox hurtBox) 
        {
            int num = 0;

            foreach (HurtBox result in search.GetResults())
            {
                if (result != hurtBox)
                {
                    RemoveTagOnEnemy(num);
                }
                num++;
            }

        }

        private void AddTagToEnemy(HurtBox hurtBox)
        {
            
            // if the target hitbox doesn't exist in the list already, add tag.
            if (!targetIndicators.TryGetValue(hurtBox, out var value))
            {
                targetsList.Add(hurtBox);
                // Enemy does not exist - adding tag 
                IndicatorInfo indicatorInfo = default(IndicatorInfo);
                indicatorInfo.indicator = new ShotgunPelletIndicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
                value = indicatorInfo;  // the image that is shown on the enemy
                value.indicator.targetTransform = hurtBox.transform;
                value.indicator.active = true;
            }
            targetIndicators[hurtBox] = value;
        }




        public GameObject GetTargets()
        {
            if (GetTargets() != null)
            {
                return GetTargets().gameObject;
            }
            return null;
        }



        public void SearchForTarget(out HurtBox currentHurtbox)
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
            foreach (HurtBox result in search.GetResults())
            {
                // if it has a healthbar and they are alive
                if ((bool)result.healthComponent && result.healthComponent.alive)
                {
                    currentHurtbox = result;
                    return;
                }
                    
            }
            currentHurtbox = null;


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
