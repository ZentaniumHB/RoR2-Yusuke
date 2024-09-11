using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking
{
    public class Tracking : HuntressTracker
    {
        public static Color Green = new Color(0.0f, 0.5f, 0f, 1f);


        private readonly BullseyeSearch multiSearch = new BullseyeSearch();

        public List<HurtBox> targetResults;

        private Tracking()
        {
            maxTrackingDistance = 24f;
            maxTrackingAngle = 24f;
        }


        private void Awake()
        {
            GameObject gameObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator"), "Tracking");
            if (gameObject != null)
            {
                SpriteRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].color = Green;
                }
                Reflection.SetFieldValue<Indicator>((object)this, "indicator", new Indicator(base.gameObject, gameObject));
            }
            this.enabled = false;

        }



        public void TurnOn()
        {
            this.enabled = true;
        }

        public void TurnOff()
        {
            this.enabled = false;
        }


        public GameObject GetTargets()
        {
            if (GetTargets() != null)
            {
                return GetTargets().gameObject;
            }
            return null;
        }



        public List<HurtBox> SearchMultipeTargets(Ray aimRay)
        {

            /*BullseyeSearch multiSearch = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.all,
                teamMaskFilter.RemoveTeam(teamComponent.teamIndex),
                filterByLoS = true,
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = maxTrackingDistance,
                maxAngleFilter = maxTrackingAngle,
                
            };*/

            targetResults = multiSearch.GetResults().ToList<HurtBox>();

            return targetResults;
        }

    }
}
