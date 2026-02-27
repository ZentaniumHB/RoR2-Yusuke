using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using static RoR2.AimAnimator;
using EntityStates.VoidSurvivor.Vent;
using System.Runtime.InteropServices;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class PitchYawControl : MonoBehaviour
    {
        private float originalPitchRangeMax;
        private float originalPitchRangeMin;
        private float originalYawRangeMin;
        private float originalYawRangeMax;
        private float originalGiveUpDuration;
        //
        private readonly float largeRangeValue = 9999f;
        //
        public CharacterDirection characterDirection;
        public CharacterBody characterBody;
        public InputBankTest inputBank;
        private ModelLocator modelLocator;
        public Animator animator;
        private AimAnimator aimAnimator;

        private float snipePitchMax = 40f;
        private float lerpPitch;
        private float lerpPitchVelocity;

        private float elapsedTime = 0;


        public void Awake() 
        {

            characterBody = gameObject.GetComponent<CharacterBody>();
            characterDirection = gameObject.GetComponent<CharacterDirection>();
            inputBank = gameObject.GetComponent<InputBankTest>();
            modelLocator = gameObject.GetComponent<ModelLocator>();
            animator = modelLocator.modelTransform.gameObject.GetComponent<Animator>();
            aimAnimator = modelLocator.modelTransform.gameObject.GetComponent<AimAnimator>();

        }

        public void ChangePitchAndYawRange(bool isInCutscene, Transform modelTransform, AimAnimator aimAnim)
        {

            if (modelTransform)
            {
                aimAnim = modelTransform.GetComponent<AimAnimator>();
                if (isInCutscene)
                {

                    originalPitchRangeMax = aimAnim.pitchRangeMax;
                    originalPitchRangeMin = aimAnim.pitchRangeMin;
                    originalYawRangeMax = aimAnim.yawRangeMax;
                    originalYawRangeMin = aimAnim.yawRangeMin;
                    originalGiveUpDuration = aimAnim.giveupDuration;

                    aimAnim.pitchRangeMax = largeRangeValue;
                    aimAnim.pitchRangeMin = -largeRangeValue;
                    aimAnim.yawRangeMin = -largeRangeValue;
                    aimAnim.yawRangeMax = largeRangeValue;

                    aimAnim.giveupDuration = 0f;

                    //aimAnim.enableAimWeight = true;
                    Log.Info("Elapsed time on start: "+elapsedTime);

                }
                else
                {

                    aimAnim.pitchRangeMax = originalPitchRangeMax;
                    aimAnim.pitchRangeMin = originalPitchRangeMin;
                    aimAnim.yawRangeMin = originalYawRangeMin;
                    aimAnim.yawRangeMax = originalYawRangeMax;

                    aimAnim.giveupDuration = originalGiveUpDuration;

                    //aimAnim.enableAimWeight = false;
                    Log.Info("Elapsed time on exit: " + elapsedTime);
                }
                aimAnim.AimImmediate();
            }

        }

        // attempts to slowly decrease the pitch/yaw value to its original
        public void RestorePitch(Transform modelTransform, AimAnimator aimAnim, float transitionDuration)
        {
            aimAnim = modelTransform.GetComponent<AimAnimator>();
            if (modelTransform)
            {

                elapsedTime += Time.deltaTime;

                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                aimAnim.pitchRangeMax = Mathf.Lerp(largeRangeValue, originalPitchRangeMax, t);
                aimAnim.pitchRangeMin = Mathf.Lerp(-largeRangeValue, originalPitchRangeMin, t);

                float num = Mathf.Lerp(largeRangeValue, originalPitchRangeMax, t);

                /*aimAnim.pitchRangeMax = originalPitchRangeMax;
                aimAnim.pitchRangeMin = originalPitchRangeMin;*/

                aimAnim.giveupDuration = originalGiveUpDuration;



            }

        }

        public void ResetElapsedTime()
        {
            elapsedTime = 0f;
        }



        /*public void ExtendPitch(Transform modelTransform, AimAnimator aimAnim)
        {

            aimAnim = modelTransform.GetComponent<AimAnimator>();
          
            originalPitchRangeMax = aimAnim.pitchRangeMax;
            originalPitchRangeMin = aimAnim.pitchRangeMin;

            aimAnim.giveupDuration = 0f;

            aimAnim.AimImmediate();

        }*/







    }
}
