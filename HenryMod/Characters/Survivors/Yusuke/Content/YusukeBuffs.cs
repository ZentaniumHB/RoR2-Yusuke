using RoR2;
using UnityEngine;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeBuffs
    {

        public static readonly Color yusukeBuffColour = new Color(0.0f, 0.4f, 0.2f);

        public static readonly Color overdriveSlowColour = new Color(0.0f, 1.0f, 1.0f);


        // armor buff gained during roll
        public static BuffDef armorBuff;

        // slow 
        public static BuffDef spiritMegaSlowDebuff;

        // Armour 
        public static BuffDef spiritMegaArmourBuff;

        // Armour 
        public static BuffDef overdriveSlowBuff;



        public static void Init(AssetBundle assetBundle)
        {
            armorBuff = Modules.Content.CreateAndAddBuff("HenryArmorBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.white,
                false,
                false);

            spiritMegaSlowDebuff = Modules.Content.CreateAndAddBuff("SpiritMegaSlowDebuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/Slow80").iconSprite,
                yusukeBuffColour,
                false,
                true);

            spiritMegaArmourBuff = Modules.Content.CreateAndAddBuff("SpiritMegaAmourBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/ArmorBoost").iconSprite,
                yusukeBuffColour,
                false,
                false);

            overdriveSlowBuff = Modules.Content.CreateAndAddBuff("SpiritMegaSlowDebuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/Slow50").iconSprite,
                overdriveSlowColour,
                false,
                true);
        }
    }
}
