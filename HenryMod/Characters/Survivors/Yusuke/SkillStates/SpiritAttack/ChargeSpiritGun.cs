using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using YusukeMod;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using YusukeMod.Modules.BaseStates;
using YusukeMod.SkillStates;
using static YusukeMod.Modules.BaseStates.YusukeMain;

namespace YusukeMod.Survivors.Yusuke.SkillStates
{

    public class ChargeSpiritGun : BaseChargeSpirit
    {

        protected float totalCharge { get; private set; }
        public bool isMaxCharge;

        public virtual int attackID { get; set; } = 2;

        private bool hasIconSwitch;

        Animator animator = null;
        private YusukeMain mainState;



        private GameObject spiritGunChargeEffectPrefab;
        private GameObject spiritGunChargeEffectObject;
        private bool hasRegularEffectSpawned;

        private GameObject spiritGunChargeEffectPotentPrefab;
        private GameObject spiritGunChargeEffectPotentObject;
        private bool hasMaxChargeEffectSpawned;

        private DestroyOnTimer destroyRegularChargeTimer;
        private readonly string fingerTipString = "fingerTipR";

        public override void OnEnter()
        {
            spiritGunChargeEffectPrefab = YusukeAssets.spiritGunChargeEffect;
            spiritGunChargeEffectPotentPrefab = YusukeAssets.spiritGunChargePotentEffect;

            SwitchAnimationLayer();

            Log.Info("attack ID: " + attackID);
            cuffComponent = characterBody.GetComponent<SpiritCuffComponent>();
            // starting value, max value and how fast to increment
            chargeValue = 0.0f;
            chargeLimit = 100.0f;

            if (cuffComponent)
            {
                if (cuffComponent.hasReleased)
                {
                    baseChargeDuration = 3.0f;
                }
                else
                {
                    baseChargeDuration = 5.0f;
                }
            }

            SpawnChargeEffect(false);
            chargeDuration = baseChargeDuration;

            base.OnEnter();

        }

        // this charge effect does not use the EffectManager, gives control on when to destroy the object this way (unless there IS a way when using EffectManager that I don't know)"
        private void SpawnChargeEffect(bool isMaxCharge)
        {
            Log.Info("Grabbing the component. ");
            if(!isMaxCharge)
            {
                hasRegularEffectSpawned = true;
                if (spiritGunChargeEffectPrefab != null) spiritGunChargeEffectObject = YusukePlugin.CreateEffectObject(spiritGunChargeEffectPrefab, FindModelChild("fingerTipR"));
            }
            else
            {
                hasMaxChargeEffectSpawned = true;
                if (spiritGunChargeEffectPotentPrefab != null) spiritGunChargeEffectPotentObject = YusukePlugin.CreateEffectObject(spiritGunChargeEffectPotentPrefab, FindModelChild("fingerTipR"));

            }
            
            
        }

        // switching the animation layer within unity. This will perform the spirit gun animations that is synced to the body animations instead. 
        private void SwitchAnimationLayer()
        {
            EntityStateMachine stateMachine = characterBody.GetComponent<EntityStateMachine>();
            if (stateMachine == null)
            {
                Log.Error("No State machine found");
            }
            else
            {
                Type currentStateType = stateMachine.state.GetType();
                if (currentStateType == typeof(YusukeMain))
                {
                    mainState = (YusukeMain)stateMachine.state;
                    // goes through the animation layers and switches them within the main state.
                    mainState.SwitchMovementAnimations((int)AnimationLayerIndex.GunCharge, true);

                }

            }

        }

        public override void OnExit()
        {
            base.OnExit();
            if(isMaxCharge || cuffComponent.hasReleased) RevertIconSwitch(2);
            // if any chargeEffect objects still exist, remove them.
            if (spiritGunChargeEffectObject) EntityState.Destroy(spiritGunChargeEffectObject);
            if (spiritGunChargeEffectPotentObject) EntityState.Destroy(spiritGunChargeEffectPotentObject);



        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            chargeIncrement = chargeLimit / baseChargeDuration * Time.fixedDeltaTime; // takes 'chargeDuration' seconds to get to chargeLimit
            if (!isMaxCharge) chargeValue += chargeIncrement;
            //totalCharge = Mathf.Lerp(0.0f, chargeLimit, fixedAge / baseChargeDuration);
            totalCharge = Mathf.Clamp(chargeValue, 0.0f, chargeLimit);
            //if (!isMaxCharge) Log.Info($"Spirit gun charge: " + totalCharge);


            if (fixedAge >= chargeDuration)
            {
                if (!isMaxCharge)
                {
                    Chat.AddMessage("Max charge.");
                    Log.Info($"Total charge (regular): " +totalCharge);
                    isMaxCharge = true;
                    
                }
            }

            if (isMaxCharge)
            {
                characterBody.isSprinting = false;
                base.characterBody.SetAimTimer(1f);

                DestroyCurrentEffect();
                if (!hasMaxChargeEffectSpawned) SpawnChargeEffect(true);

                if (!hasIconSwitch)
                {
                    hasIconSwitch = true;

                    /* checks whether the cuff state is released and changes the icon accordingly 
                     * This is mainly done for visual purposes. SO the player knows what type of spirit gun they are doing
                    */
                    if (cuffComponent)
                    {
                        if (cuffComponent.hasReleased)
                        {
                            IconSwitch(true, attackID);
                        }
                        else
                        {
                            IconSwitch(false, attackID);
                        }
                    }
                    
                }

                
            }

            if (!IsKeyDown() && isAuthority)
            {
                totalCharge = RoundTheFloat(totalCharge);

                if (isMaxCharge)
                {
                    if (cuffComponent)
                    {
                        // if spiritcuff is activated, do spirit beam
                        if (cuffComponent.hasReleased)
                        {

                            outer.SetNextState(BeamNextState());
                        }
                        else
                        {
                            // if not, do spirit gun double
                            outer.SetNextState(DoubleNextState());
                        }
                    }

                }
                else
                {
                    // if neither, just do regular
                    outer.SetNextState(SpiritNextState());
                }
                
                Log.Info($"Total charge (rounded (regular)): " + totalCharge);
            }


        }

        // deleting the first effect so the second effect can be created and shown
        private void DestroyCurrentEffect()
        {
            if (hasRegularEffectSpawned)
            {
                hasRegularEffectSpawned = false;
                Log.Info("Removing effect:");
                EntityState.Destroy(spiritGunChargeEffectObject);
            }
            
        }

        public override void Update()
        {
            // changing the crosshair for charge state.
            base.Update();
            characterBody.SetSpreadBloom(age / chargeDuration);

        }

        protected virtual bool IsKeyDown() {

            return IsKeyDownAuthority();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        protected virtual EntityState SpiritNextState()
        {
            return new Shoot
            {
                charge = totalCharge,
                
            };
        }

        protected virtual EntityState DoubleNextState()
        {
            return new SpiritGunDouble
            {
                charge = totalCharge,
                isMaxCharge = isMaxCharge
            };
        }

        protected virtual EntityState BeamNextState()
        {

            return new FireSpiritBeam
            {
                charge = totalCharge,
                
            };
        }



    }


}
