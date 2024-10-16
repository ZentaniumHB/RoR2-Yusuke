using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.Extra
{
    public class SkillTags : MonoBehaviour
    {

        public bool isPrimary; // used for checking if the spiritGun 
        public bool isSecondary;

        internal void Remove()
        {
            Destroy(this);
        }
    }
}
