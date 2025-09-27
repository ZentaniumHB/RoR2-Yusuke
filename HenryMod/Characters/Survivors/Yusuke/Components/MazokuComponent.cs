using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;

namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    internal class MazokuComponent : MonoBehaviour
    {

        public float maxMazokuValue = 100f;
        public float currentMazokuValue;
        public float previousValue;

        public bool hasTransformed;
        private float decreaseValue = 2f;
        public float increaseValue = 0; // used for the hook reasoning
        public bool startReverse;

        private bool isMazokuBarPaused;

        public void Start()
        {

        }

        private void FixedUpdate()
        {
            if (hasTransformed)
            {
                //Log.Info("currentMazokuValue " + currentMazokuValue);
                if(!isMazokuBarPaused) currentMazokuValue = Mathf.Clamp(currentMazokuValue - (decreaseValue * Time.deltaTime), 0f, maxMazokuValue);
                previousValue = currentMazokuValue;
            }
            if (previousValue <= 0)
            {
                if (hasTransformed) startReverse = true; //used for the mazoku revert
                currentMazokuValue = 0;
                previousValue = currentMazokuValue;
                hasTransformed = false;
                
            }

            if (increaseValue != 0) IncreaseMazokuGuage(increaseValue);
        }

        public bool IncreaseMazokuGuage(int type)
        {
            // create a switch case for each type depending on the move that was used.

            /*switch (type)
            {
                case 1:
                    if (hasTransformed) return IncreaseMazokuGuage(0.5f);
                    return IncreaseMazokuGuage(1f);
                case 2:
                    if (hasTransformed) return IncreaseMazokuGuage(1.5f);
                    return IncreaseMazokuGuage(3f);
                case 3:
                    if (hasTransformed) return IncreaseMazokuGuage(0.5f);
                    return IncreaseMazokuGuage(1f);
                default: return false;
            }*/

            if (hasTransformed) return IncreaseMazokuGuage(0f);
            return IncreaseMazokuGuage(type);
        }

        public bool IncreaseMazokuGuage(float value)
        {
            if (currentMazokuValue >= maxMazokuValue)
            {
                // Reached max mazoku value. No need to increase
                return false;
            }
            if (currentMazokuValue >= maxMazokuValue && previousValue < maxMazokuValue)
            {
                //play sound or change a colour, something to indicate that it is filled.

            }
            currentMazokuValue = Mathf.Clamp(currentMazokuValue + value, 0f, maxMazokuValue);
            previousValue = currentMazokuValue;
            Log.Info("Previous mazoku value: " + previousValue);
            increaseValue = 0f;
            return true;
        }


        public void HaltMazokuBar(bool shouldHalt)
        {
            isMazokuBarPaused = shouldHalt;
        }

        public void MaxReplenishMazokuBar()
        {
            increaseValue = 100;
        }

    }

}
