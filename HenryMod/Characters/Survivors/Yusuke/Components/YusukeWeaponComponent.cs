using UnityEngine;

namespace YusukeMod.Survivors.Yusuke.Components
{
    internal class YusukeWeaponComponent : MonoBehaviour
    {
        // used to prevent clashing idle animations
        private bool isFollowUpAttackActive;

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
    }
}