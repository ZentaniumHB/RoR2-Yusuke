using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    internal class PitchYawControl : MonoBehaviour
    {
        private float originalPitchRangeMax;
        private float originalPitchRangeMin;
        private float originalYawRangeMin;
        private float originalYawRangeMax;
        private float originalGiveUpDuration;

        private readonly float largeRangeValue = 9999f;

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

                }
                else
                {
                    aimAnim.pitchRangeMax = originalPitchRangeMax;
                    aimAnim.pitchRangeMin = originalPitchRangeMin;
                    aimAnim.yawRangeMin = originalYawRangeMin;
                    aimAnim.yawRangeMax = originalYawRangeMax;

                    aimAnim.giveupDuration = originalGiveUpDuration;

                }
                aimAnim.AimImmediate();
            }

        }
    }
}
