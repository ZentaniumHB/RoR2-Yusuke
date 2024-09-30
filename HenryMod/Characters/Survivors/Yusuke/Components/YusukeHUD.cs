using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.UI;
using TMPro;
using YusukeMod.Survivors.Yusuke;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    internal class YusukeHUD : MonoBehaviour
    {

        public CharacterBody characterBody;
        public HUD hud = null;
        public GameObject SpiritCuffGauge;
        public GameObject MazokuGauge;
        private GameObject bottomLeftCluster;
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        private bool hasCheckedUtility;
        private bool hasWaveUtility;

        public bool activateUI;


        public void Start()
        {

            //grabs the hud that is located and hooked from the Survivor class
            hud = YusukeSurvivor.hud;
            characterBody = GetComponent<CharacterBody>();
            hasCheckedUtility = false;

            //UI objects that are being created by loading them from the assets
            SpiritCuffGauge = UnityEngine.Object.Instantiate(YusukeAssets.SpiritCuffGauge);
            MazokuGauge = UnityEngine.Object.Instantiate(YusukeAssets.MazokuGauge);

            // finding the bottomleftCluster (section that holds the health, level and buff/debuff infomation
            Transform bottomLeftCluster = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas")
                 .Find("BottomLeftCluster");

            if (bottomLeftCluster != null)
            {
                /* setting the custom guages from the asset folder parent. This will allow the bars to share any HUD changes that occur when
                 *  the resolution changes
                 */
                Log.Info("bottomLeftCluster EXISTS!");
                SpiritCuffGauge.transform.SetParent(bottomLeftCluster.transform, false);
                MazokuGauge.transform.SetParent(bottomLeftCluster.transform, false);

                // manually changing the positions of the bars, so they fit under the healthbar.
                float localX = SpiritCuffGauge.transform.localPosition.x - 141;
                float localY = SpiritCuffGauge.transform.localPosition.y - 64;
                float localZ = SpiritCuffGauge.transform.localPosition.z;
                Vector3 spiritCuffFinalPosition = new Vector3(localX, localY, localZ);
                SpiritCuffGauge.transform.localPosition = spiritCuffFinalPosition;

                float MazLocalY = MazokuGauge.transform.localPosition.y - 64;
                Vector3 mazokuFinalPosition = new Vector3(localX, MazLocalY, localZ);
                MazokuGauge.transform.localPosition = mazokuFinalPosition;
            }
            else
            {
                Log.Info("bottomLeftCluster does not exits.");
            }

            MazokuGauge.SetActive(true);
            


        }


        public void FixedUpdate()
        {

            Log.Info(characterBody.skillLocator.utility.skillNameToken);



            if (characterBody)
            {
                if (!hasCheckedUtility)
                {
                    hasCheckedUtility = true;
                    Log.Info("Charabody exists");
                    if (characterBody.skillLocator.utility.skillNameToken != prefix + "UTILITY_WAVE_NAME")
                    {
                        hasWaveUtility = false;
                        SpiritCuffGauge.SetActive(false);

                    }
                    else
                    {
                        hasWaveUtility = true;
                    }
                }
                

            }


        }


    }
}
