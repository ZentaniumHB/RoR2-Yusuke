using System;
using System.Collections.Generic;
using System.Text;
using YusukeMod.Modules;
using RoR2;
using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    public class SpiritCuffComponent : MonoBehaviour
    {

        public float maxSpiritCuffValue = 100f;
        public float currentSpiritValue;
        public float previousValue;

        public bool hasReleased;
        private float decreaseValue = 3f;

        private bool hasDisplayedEffect;
        public GameObject spiritCuffObj;

        private void Start()
        {

        }

        private void FixedUpdate()
        {
            if (hasReleased)
            {
                //Log.Info("currentSpiritValue " + currentSpiritValue);
                currentSpiritValue = Mathf.Clamp(currentSpiritValue - (decreaseValue * Time.deltaTime), 0f, maxSpiritCuffValue);
                previousValue = currentSpiritValue;
            }
            if(previousValue <= 0)
            {
                currentSpiritValue = 0;
                previousValue = currentSpiritValue;
                hasReleased = false;
                hasDisplayedEffect = false;
            }
        }

        public bool IncreaseCuff(int type)
        {
            // create a switch case for each type depending on the move that was used.
 
            switch (type)
            {
                case 1:
                    if (hasReleased) return IncreaseCuff(0.5f);
                    return IncreaseCuff(1f);
                case 2:
                    if (hasReleased) return IncreaseCuff(1.5f);
                    return IncreaseCuff(3f);
                case 3:
                    if (hasReleased) return IncreaseCuff(0.5f);
                    return IncreaseCuff(1f);
                default: return false;
            }
        }

        public bool IncreaseCuff(float value) 
        { 
            if(currentSpiritValue >= maxSpiritCuffValue)
            {
                Log.Info("Reached max value. No need to increase.");
                return false;
            }
            if (currentSpiritValue >= maxSpiritCuffValue && previousValue < maxSpiritCuffValue)
            {
                //play sound or change a colour, something to indicate that it is filled.

            }
            currentSpiritValue = Mathf.Clamp(currentSpiritValue + value, 0f, maxSpiritCuffValue);
            previousValue = currentSpiritValue;
            Log.Info("current sacred value: "+previousValue);
            return true;
        }

        public void ChangeEffectState(bool isActive)
        {
            if (isActive)
            {
                hasDisplayedEffect = true;
            }
            else
            {
                hasDisplayedEffect = false;
            }
        }

        public bool GetCurrentEffectState()
        {
            return hasDisplayedEffect;
        }

    }
}
