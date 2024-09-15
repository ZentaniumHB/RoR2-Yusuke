using EntityStates;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using YusukeMod;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.Survivors.Yusuke;
using YusukeMod.Survivors.Yusuke.SkillStates;

namespace YusukeMod.SkillStates
{
    public class SpiritWave : BaseSkillState
    {
        public float charge;
        public bool isMaxCharge;
        private Vector3 forwardDirection;

        private float chargedFinalSpeed;
        private float chargedMaxSpeed;

        private float minimunDashSpeed = 3f;
        private float minFinalDashSpeed = 1.25f;

        private float MaximumDashSpeed = 6f;
        private float MaxFinalDashSpeed = 2.5f;

        private float dashSpeed;

        private float duration;
        private float minDuration = 0.3f;
        private float maxDuration = 0.8f;

        public float maxTrackingDistance = 4f;
        public float maxTrackingAngle = 80f;

        public float actionStopwatch = 0.0f;
        public float actionTimeDuration = 0.8f;


        private bool collision;
        private BullseyeSearch search = new BullseyeSearch();
        private HurtBox target;
        private Indicator indicator;
        public GameObject targetIcon;



        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;

        private Vector3 previousPosition;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = Mathf.Lerp(minDuration, maxDuration, charge);
            forwardDirection = GetAimRay().direction;

            chargedMaxSpeed = GetChargedMax(charge);
            chargedFinalSpeed = GetChargedFinal(charge);

            UpdateDashSpeed(chargedMaxSpeed, chargedFinalSpeed);

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * dashSpeed;
                
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            


        }

        /*private Vector3 UpdateDashSpeed(Vector3 currentVelocity)
        {
            return currentVelocity + forwardDirection * Mathf.Lerp(minDashSpeed, maxDashSpeed, charge);
        }*/

        private void UpdateDashSpeed(float max, float final)
        {
            dashSpeed = moveSpeedStat * Mathf.Lerp(max, final, fixedAge / duration);
        }

        private float GetChargedMax(float charge)
        {
            //return (charge / 100.0f) * MaximumDashSpeed;

            return Mathf.Lerp(MaxFinalDashSpeed, MaximumDashSpeed, charge / 100.0f);

        }

        private float GetChargedFinal(float charge)
        {
            //return (charge / 100.0f) * MaxFinalDashSpeed; //chargedFinalSpeed
            return Mathf.Lerp(minFinalDashSpeed, minimunDashSpeed, charge / 100.0f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //dashSpeed = UpdateDashSpeed(characterMotor.velocity);

            UpdateDashSpeed(chargedMaxSpeed, chargedFinalSpeed);
            //Log.Info("dashSpeed: " + dashSpeed);

            if (target)
            {
                actionStopwatch += Time.fixedDeltaTime;
                Log.Info("Action timer: "+ actionStopwatch);
                //Log.Info("Target found.");
                if ((bool)target.healthComponent && target.healthComponent.alive && !collision)
                {
                    //Log.Info("creating indicator.");

                    if (targetIcon == null)
                    {
                        targetIcon = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
                    }
                    indicator = new Indicator(gameObject, targetIcon);

                    //Log.Info("locating indicator.");
                    indicator.targetTransform = target.transform;
                    indicator.active = true;
                    collision = true;

                }   
            }

            if (!collision)
            {
                SearchForTarget();
                characterBody.isSprinting = true;
                if (characterDirection) characterDirection.forward = forwardDirection;
                if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);

                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * dashSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;

                    characterMotor.velocity = vector;
                }
                previousPosition = transform.position;
            }

            if (collision)
            {
                
                float decelerateValue = 0.2f; // 50f  // 
                characterMotor.velocity = new Vector3(decelerateValue, decelerateValue, decelerateValue);
            }
           


            if (isAuthority && fixedAge >= duration)
            {
                if (!collision)
                {
                    outer.SetNextStateToMain();
                    return;
                }

                if(collision && actionStopwatch >= actionTimeDuration) {
                    outer.SetNextStateToMain();
                    return;
                }



            }
        }


        public override void OnExit()
        {
            base.OnExit();
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            indicator.active = false;

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }


        private void SearchForTarget()
        {
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = transform.position;
            search.searchDirection = forwardDirection;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);

            target = search.GetResults().FirstOrDefault();
        }
    }

}

