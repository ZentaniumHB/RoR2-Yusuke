using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking
{
    public class SpiritSnipeTracking : HuntressTracker
    {
        public static Color White = new Color(1f, 1f, 1f, 1f);


        private readonly BullseyeSearch multiSearch = new BullseyeSearch();

        public List<HurtBox> targetResults;

        private SpiritSnipeTracking()
        {
            maxTrackingDistance = 240f;
            maxTrackingAngle = 12f;
        }


        private void Awake()
        {
            GameObject gameObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator"), "Tracking");
            if (gameObject != null)
            {
                SpriteRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].color = White;
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

            targetResults = multiSearch.GetResults().ToList<HurtBox>();

            return targetResults;
        }

    }
}
