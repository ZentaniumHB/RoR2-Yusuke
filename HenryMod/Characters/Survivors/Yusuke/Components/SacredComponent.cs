using RoR2BepInExPack.GameAssetPaths;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Survivors.Yusuke.Components;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    internal class SacredComponent : MonoBehaviour
    {
        // enum based on whether the sacred meter decreases over time or empties completely
        public enum OverdriveType
        {
            STANDARD,
            TIMED
        }

        public float chargeIncrement;
        public float baseChargeDuration = 3.0f;    // time it take for the component to charge in seconds

        public float maxSacredValue = 100f;
        public float chargeValue;
        public float currentSacredValue;
        public float previousValue;

        public bool hasReleaseSacredEnergy = false;
        public bool isSacredInUse = false;
        public bool hasSacredEnergy;
        private float decreaseValue = 10f;
        public bool startReverse;

        private bool hasDisplayedMaxIcon;
        private bool hasDisplayedMaxBlueIcon;
        private bool hasHiddenMaxIcon;
        private bool hasHiddenMaxBlueIcon;

        YusukeHUD yusukeHud;
        YusukeWeaponComponent yusukeWeaponComponent;

        public void Start()
        {
            yusukeHud = gameObject.GetComponent<YusukeHUD>();
            yusukeWeaponComponent = gameObject.GetComponent<YusukeWeaponComponent>();

            if(yusukeHud)
            {
                Log.Info("[SACRED ENERGY COMPONENT] - Yusuke HUD has been found. ");
            }
            else
            {
                Log.Info("Yusuke HUD not found. ");
            }
        }

        private void Update()
        {
            if(hasReleaseSacredEnergy && !isSacredInUse) IncreaseSacredGauge(1);
            if (hasSacredEnergy)
            {
                SwitchToMaxIcon();
                if (startReverse)
                {
                    //Log.Info("currentSpiritValue " + currentSpiritValue);
                    HideMaxIcon();
                    currentSacredValue = Mathf.Clamp(currentSacredValue - (decreaseValue * Time.deltaTime), 0f, maxSacredValue);
                    previousValue = currentSacredValue;
                    
                }

            }
            if (previousValue <= 0)
            {
                ResetValues();
                HideMaxBlueIcon();

            }
        }

        private void SwitchToMaxIcon()
        {
            if (!hasDisplayedMaxIcon)
            {
                hasDisplayedMaxIcon = true;
                if (yusukeHud)
                {
                    yusukeHud.DisplaySacredMaxGuageImage(true);
                }
 
            }
        }

        private void SwitchToMaxBlueIcon()
        {
            if (!hasDisplayedMaxBlueIcon)
            {
                hasDisplayedMaxBlueIcon = true;
                if (yusukeHud)
                {
                    yusukeWeaponComponent.SetFlowState(true);
                    yusukeHud.DisplaySacredMaxGuageBlueFlowImage(true);
                }

            }
        }

        

        private void HideMaxIcon()
        {
            if (!hasHiddenMaxIcon)
            {
                hasHiddenMaxIcon = true;
                if (yusukeHud)
                {
                    yusukeHud.DisplaySacredMaxGuageImage(false);
                }

            }
        }

        private void HideMaxBlueIcon()
        {
            if (!hasHiddenMaxBlueIcon)
            {
                hasHiddenMaxBlueIcon = true;
                if (yusukeHud)
                {
                    yusukeHud.DisplaySacredMaxGuageBlueFlowImage(false);
                    yusukeWeaponComponent.SetFlowState(false);
                }

            }
        }

        public bool IncreaseSacredGauge(int type)
        {
            // create a switch case for each type depending on the move that was used.

            switch (type)
            {
                case 1:
                    return IncreaseSacredGauge();
                case 2:

                case 3:

                default: return false;
            }
        }

        public bool IncreaseSacredGauge()
        {
            if (currentSacredValue >= maxSacredValue) 
            {
                hasSacredEnergy = true;
                return false;
            } 

            if (currentSacredValue >= maxSacredValue && previousValue < maxSacredValue)
            {
                //play sound or change a colour, something to indicate that it is filled.
                
            }

            if (startReverse == false)
            {
                chargeIncrement = maxSacredValue / baseChargeDuration * Time.deltaTime;
                chargeValue += chargeIncrement;
                currentSacredValue = Mathf.Clamp(chargeValue, 0.0f, maxSacredValue);

                previousValue = currentSacredValue;

            }

            return true;
        }

        public void UseOverdriveAbility(byte type)
        {
            if(type == (byte)OverdriveType.STANDARD)
            {
                HideMaxIcon();
                ResetValues();
            }
            else
            {
                SwitchToMaxBlueIcon();
                startReverse = true;
            }
        }

        private void ResetValues()
        {
            currentSacredValue = 0;
            chargeValue = 0;
            previousValue = currentSacredValue;
            hasSacredEnergy = false;
            startReverse = false;
            hasDisplayedMaxIcon = false;
            hasDisplayedMaxBlueIcon = false;
            hasHiddenMaxIcon = false;
            hasDepletedSacredMeter = false;
        }
    }
}
