using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.UI;
using TMPro;
using YusukeMod.Survivors.Yusuke;
using UnityEngine.UI;
using static AkMIDIEvent;
using UnityEngine.Networking.Types;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    internal class YusukeHUD : MonoBehaviour
    {

        public CharacterBody characterBody;
        public HUD hud = null;
        public GameObject SpiritCuffGauge;
        public GameObject MazokuGauge;
        const string prefix = YusukeSurvivor.YUSUKE_PREFIX;

        private bool hasCheckedUtility;
        public bool hasWaveUtility;
        public Image spiritCuffFill;
        public Image mazokuFill;

        private float currentMazokuAmmount;
        private float currentAmount;

        public bool activateUI;

        private Color cuffGuageColour;
        private Color cuffBorderColour;

        private Color mazokuGuageColour;
        private Color mazokuBorderColour;

        private bool hasMazokuSetup;

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

            //Log.Info(characterBody.skillLocator.utility.skillNameToken);
            
            if (characterBody)
            {
                if (!hasMazokuSetup) SetUpMazoku();
                if (!hasCheckedUtility)
                {
                    hasCheckedUtility = true;
                    // checks if the spiritcuff skill is equiped, if not, it doesn't display the gauge
                    if (characterBody.skillLocator.special.skillNameToken != prefix + "SPECIAL_SPIRITCUFF_NAME")
                    {
                        hasWaveUtility = false;
                        SpiritCuffGauge.SetActive(false);

                    }
                    else
                    {
                        //if it is, then the gauge is visible and the calculations for the spiritcuff gauge are applied.
                        hasWaveUtility = true;
                        Transform childTransform = SpiritCuffGauge.transform.Find("SpiritCuffCharge");
                        if(childTransform != null)
                        {
                            
                            spiritCuffFill = childTransform.GetComponent<Image>();
                            if (spiritCuffFill)
                            {
                                cuffGuageColour = spiritCuffFill.color;
                            }
                            else
                            {
                                //Log.Error("SpiritCuffFill does not exists");
                            }
                        }
                        else
                        {
                            //Log.Error("SpiritCuffCharge was not found");
                        }
                        
                    }
                }
                

            }
            if (hasWaveUtility)
            {
                UpdateCuffGauge();
            }
            UpdateMazokuGuage();


        }

        private void SetUpMazoku()
        {
            hasMazokuSetup = true;
            Transform childTransform = MazokuGauge.transform.Find("MazokuCharge");
            if (childTransform != null)
            {

                mazokuFill = childTransform.GetComponent<Image>();
                if (mazokuFill)
                {
                    mazokuGuageColour = mazokuFill.color;
                }
                else
                {
                    //Log.Error("SpiritCuffFill does not exists");
                }
            }
            else
            {
                //Log.Error("SpiritCuffCharge was not found");
            }

        }

        private void UpdateCuffGauge()
        {
            //Log.Info("SpiritCuff equiped, getting cuffComponent");
            SpiritCuffComponent cuffComponent = hud.targetBodyObject.GetComponent<SpiritCuffComponent>();
            if ((bool)cuffComponent)
            {
                // retrieves the value that needs to be added to the fill
                float finalFill = cuffComponent.currentSpiritValue / cuffComponent.maxSpiritCuffValue;
                UpdateFill(finalFill, 2);
                spiritCuffFill.fillAmount = currentAmount;
                if (currentAmount >= 1f)
                {
                    spiritCuffFill.color = Color.yellow;
                }
                else
                {
                    if (currentAmount <= 1f && !cuffComponent.hasReleased) spiritCuffFill.color = cuffGuageColour;
                }
            }
            else
            {
                //Log.Info("cuffComponent does not exist");
            }
        }

        private void UpdateMazokuGuage()
        {
            //Log.Info("SpiritCuff equiped, getting cuffComponent");
            MazokuComponent mazComponent = characterBody.master.GetComponent<MazokuComponent>();
            if ((bool)mazComponent)
            {
                // retrieves the value that needs to be added to the fill
                float finalMazFill = mazComponent.currentMazokuValue / mazComponent.maxMazokuValue;
                UpdateFill(finalMazFill, 1);
                mazokuFill.fillAmount = currentMazokuAmmount;
                if (currentMazokuAmmount >= 1f)
                {
                    mazokuFill.color = Color.magenta;
                }
                else
                {
                    if (currentMazokuAmmount <= 1f && !mazComponent.hasTransformed) mazokuFill.color = mazokuGuageColour;
                }
            }
            else
            {
                Log.Info("mazoku component does not exist");
            }
        }

        private void UpdateFill(float finalFill, int bar)
        {
            
            if(bar == 1) currentMazokuAmmount = finalFill;
            if(bar == 2) currentAmount = finalFill;



        }


    }
}
