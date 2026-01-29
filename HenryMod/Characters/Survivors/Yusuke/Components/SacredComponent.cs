using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    internal class SacredComponent : MonoBehaviour
    {
        public float chargeIncrement;
        public float baseChargeDuration = 3.0f;    // time it take for the component to charge in seconds

        public float maxSacredValue = 100f;
        public float chargeValue;
        public float currentSacredValue;
        public float previousValue;

        public bool hasReleaseSacredEnergy = false;
        public bool isSacredInUse = false;
        public bool hasSacredEnergy;
        private float decreaseValue = 2f;
        public bool startReverse;

        private bool hasDisplayedMaxIcon;
        YusukeHUD yusukeHud;

        public void Start()
        {
            yusukeHud = gameObject.GetComponent<YusukeHUD>();
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
                SwitchIcon();
                //Log.Info("currentSpiritValue " + currentSpiritValue);
                /*currentSacredValue = Mathf.Clamp(currentSacredValue - (decreaseValue * Time.deltaTime), 0f, maxSacredValue);
                previousValue = currentSacredValue;*/
            }
            if (previousValue <= 0)
            {
                currentSacredValue = 0;
                previousValue = currentSacredValue;
                hasSacredEnergy = false;
            }
        }

        private void SwitchIcon()
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

            chargeIncrement = maxSacredValue / baseChargeDuration * Time.deltaTime; 
            chargeValue += chargeIncrement;
            currentSacredValue = Mathf.Clamp(chargeValue, 0.0f, maxSacredValue);

            previousValue = currentSacredValue;
            //Log.Info("Previous value: " + currentSacredValue);
            return true;
        }

        public void UseOverdriveAbility()
        {

        }


    }
}
