using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    internal class SacredComponent : MonoBehaviour
    {

        public float maxSacredValue = 100f;
        public float currentSacredValue;
        public float previousValue;

        public bool hasReleaseSacredEnergy = false;
        public bool hasSacredEnergy;
        private float decreaseValue = 2f;
        public bool startReverse;


        public void Start()
        {

        }

        private void FixedUpdate()
        {
            if(hasReleaseSacredEnergy && IncreaseSacredGauge(1)) IncreaseSacredGauge(1);
            /*if (hasSacredEnergy)
            {
                //Log.Info("currentSpiritValue " + currentSpiritValue);
                currentSacredValue = Mathf.Clamp(currentSacredValue - (decreaseValue * Time.deltaTime), 0f, maxSacredValue);
                previousValue = currentSacredValue;
            }*/
            if (previousValue <= 0)
            {
                currentSacredValue = 0;
                previousValue = currentSacredValue;
                hasSacredEnergy = false;
            }
        }

        public bool IncreaseSacredGauge(int type)
        {
            // create a switch case for each type depending on the move that was used.

            switch (type)
            {
                case 1:
                    return IncreaseSacredGauge(0.005f);
                case 2:
                    return IncreaseSacredGauge(3f);
                case 3:
                    return IncreaseSacredGauge(1f);
                default: return false;
            }
        }

        public bool IncreaseSacredGauge(float value)
        {
            if (currentSacredValue >= maxSacredValue) return false;

            if (currentSacredValue >= maxSacredValue && previousValue < maxSacredValue)
            {
                //play sound or change a colour, something to indicate that it is filled.

            }
            currentSacredValue = Mathf.Clamp(currentSacredValue + value, 0f, maxSacredValue);
            previousValue = currentSacredValue;
            Log.Info("Previous value: " + currentSacredValue);
            return true;
        }


    }
}
