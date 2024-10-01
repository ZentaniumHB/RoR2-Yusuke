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


        private void Start()
        {

        }

        public bool IncreaseCuff(int type)
        {
            // create a switch case for each type depending on the move that was used.
 
            switch (type)
            {
                case 1:
                    return IncreaseCuff(1f);
                case 2:
                    return IncreaseCuff(3f);
                case 3:
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
            Log.Info("Previous value: "+previousValue);
            return true;
        }

    }
}
