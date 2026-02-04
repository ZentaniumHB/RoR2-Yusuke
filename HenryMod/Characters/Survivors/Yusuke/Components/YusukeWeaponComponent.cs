using EntityStates.GolemMonster;
using RoR2BepInExPack.GameAssetPaths;
using UnityEngine;
using YusukeMod.Characters.Survivors.Yusuke.Components;

namespace YusukeMod.Survivors.Yusuke.Components
{
    internal class YusukeWeaponComponent : MonoBehaviour
    {
        // used to prevent clashing idle animations
        private bool isFollowUpAttackActive;

        //used for referencing the gameobject that is necessary
        private GameObject chargeObject;

        private GameObject electricSparksObject;

        private bool hasMazokuRevive = true;

        private bool hasSacredEneryRevive = true;

        private bool hasSacredEnergyReleased = false;

        private bool isOverdriveSkillSetEnabled;

        private byte reviveType;

        private bool hasBeenKnocked;

        private bool isInKnockedState;

        private bool isInOverdriveState;

        // near death enum for different conditions
        public enum NearDeathIndex
        {
            Mazoku,
            Sacred,
            Boton
        }

        private void Awake()
        {
            //any funny custom behavior you want here
            //for example, enforcer uses a component like this to change his guns depending on selected skill
        }

        public void SetFollowUpBoolean(bool isActive)
        {
            isFollowUpAttackActive = isActive;
        }

        public bool GetFollowUpBoolean()
        {
            return isFollowUpAttackActive;
        }

        public void SetReferenceChargeObject(GameObject obj)
        {
            chargeObject = obj;
        }

        public void SetElectricChargeObject(GameObject obj)
        {
            electricSparksObject = obj;
        }

        public void ShowChargeObject(bool isVisible)
        {
            if(chargeObject != null)
            {
                chargeObject.SetActive(isVisible);
            }

            if(electricSparksObject != null)
            {
                electricSparksObject.SetActive(isVisible);
            }
        }

        // get and set
        public bool GetMazokuRevive()
        {
            return hasMazokuRevive;
        }

        public bool GetSacredEnergyRevive()
        {
            return hasSacredEneryRevive;
        }

        public bool GetKnockedState()
        {
            return hasBeenKnocked;
        }

        public bool GetKnockedBoolean()
        {
            return isInKnockedState;
        }
        public bool GetSacredEnergyReleased()
        {
            return hasSacredEnergyReleased;
        }

        public bool GetOverdriveState()
        {
            return isInOverdriveState;
        }

        public bool GetOverDriveSkillsActivity()
        {
            return isOverdriveSkillSetEnabled;
        }

        public void UseMazokuRevive()
        {
            hasMazokuRevive = false;
        }

        public void UseSacredEnergyRevive()
        {
            hasSacredEneryRevive = false;
        }

        public void SetKnockedState(bool isKnocked)
        {
            hasBeenKnocked = isKnocked;
        }

        public void SetKnockedBoolean(bool isInStateMachine)
        {
            isInKnockedState = isInStateMachine;
        }

        public void SetSacredEnergyRelease(bool hasSacredEnergyRelease)
        {
            hasSacredEnergyReleased = hasSacredEnergyRelease;

            SacredComponent sacredComponent = gameObject.GetComponent<SacredComponent>();
            if(sacredComponent) sacredComponent.hasReleaseSacredEnergy = true;
        }

        public void SetOverdriveState(bool isOverdriveStateActive)
        {
            isInOverdriveState = isOverdriveStateActive;
        }

        public void SetOverDriveSkillsActivity(bool isEnabled)
        {
            isOverdriveSkillSetEnabled = isEnabled;
        }

    }
}