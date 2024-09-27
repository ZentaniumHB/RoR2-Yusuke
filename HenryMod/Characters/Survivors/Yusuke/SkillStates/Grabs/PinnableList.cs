using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using UnityEngine;

using UnityEngine;

namespace YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs
{
    public class PinnableList : MonoBehaviour
    {

        public List<string> NotPinnable;

        private void Awake()
        {

            NotPinnable = new List<string>();
            string[] names = new string[] { 
                "SuperRoboBallBossBody(Clone)",
                "MinorConstructBody(Clone)",
                "MinorConstructOnKillBody(Clone)",
                "ArchWispBody(Clone)",
                "EngiTurretBody(Clone)",
                "GeepBody(Clone)",
                "GrandParentBody(Clone)",
                "GipBody(Clone)",
                "GreaterWispBody(Clone)",
                "Turret1Body(Clone)",
                "GupBody(Clone)",
                "MagmaWormBody(Clone)",
                "MajorConstructBody(Clone)",
                "UrchinTurretBody(Clone)",
                "ShopkeeperBody(Clone)",
                "ElectricWormBody(Clone)",
                "RoboBallBossBody(Clone)",
                "SquidTurretBody(Clone)",
                "VoidBarnacleNoCastBody(Clone)",
                "VoidBarnacleBody(Clone)",
                "VoidBarnacleNoCastBody(Clone)",
                "VagrantBody(Clone)",
                "MinorConstructAttachableBody(Clone)",
                "MiniVoidRaidCrabBodyBase(Clone)",
                "MiniVoidRaidCrabBodyPhase1(Clone)",
                "MiniVoidRaidCrabBodyPhase2(Clone)",
                "MiniVoidRaidCrabBodyPhase3(Clone)",
                "MiniVoidRaidCrabBodyPhase4(Clone)" 
            
            };

            NotPinnable.AddRange(names);

        }

        public bool CheckIfNotPinnable(string enemyName)
        {
            foreach(string name in NotPinnable)
            {
                if(enemyName ==  name) return true;
            }
            return false;
        }

        public void Remove()
        {
            Destroy(this);
        }

    }
}
