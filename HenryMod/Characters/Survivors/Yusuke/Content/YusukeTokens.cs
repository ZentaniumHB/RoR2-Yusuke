﻿using System;
using YusukeMod.Modules;
using YusukeMod.Survivors.Yusuke.Achievements;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeTokens
    {
        public static void Init()
        {
            AddYusukeTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Henry.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddYusukeTokens()
        {
            string prefix = YusukeSurvivor.YUSUKE_PREFIX;

            string desc = "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so he left, searching for a new identity.";
            string outroFailure = "..and so he vanished, forever a blank slate.";

            Language.Add(prefix + "NAME", "Henry");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Chosen One");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Henry passive");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Sample text.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SLASH_NAME", "Sword");
            Language.Add(prefix + "PRIMARY_SLASH_DESCRIPTION", Tokens.agilePrefix + $"Swing forward for <style=cIsDamage>{100f * YusukeStaticValues.swordDamageCoefficient}% damage</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_GUN_NAME", "Handgun");
            Language.Add(prefix + "SECONDARY_GUN_DESCRIPTION", Tokens.agilePrefix + $"Fire a handgun for <style=cIsDamage>{100f * YusukeStaticValues.gunDamageCoefficient}% damage</style>.");

            Language.Add(prefix + "SECONDARY_SHOTGUN_NAME", "Spirit Shotgun");
            Language.Add(prefix + "SECONDARY_SHOTGUN_DESCRIPTION", Tokens.agilePrefix + $"Shotgun attack <style=cIsDamage>{100f * YusukeStaticValues.gunDamageCoefficient}% damage</style>.");

            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_ROLL_NAME", "Roll");
            Language.Add(prefix + "UTILITY_ROLL_DESCRIPTION", "Roll a short distance, gaining <style=cIsUtility>300 armor</style>. <style=cIsUtility>You cannot be hit during the roll.</style>");


            Language.Add(prefix + "UTILITY_WAVE_NAME", "Spirit Wave");
            Language.Add(prefix + "UTILITY_WAVE_DESCRIPTION", "Big punch dealing <style=cIsUtility>x ammount</style>. <style=cIsUtility> Slow movement. </style>");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_BOMB_NAME", "Bomb");
            Language.Add(prefix + "SPECIAL_BOMB_DESCRIPTION", $"Throw a bomb for <style=cIsDamage>{100f * YusukeStaticValues.bombDamageCoefficient}% damage</style>.");

            // spirit cuff release
            Language.Add(prefix + "SPECIAL_SPIRITCUFF_NAME", "Spirit Cuff Release");
            Language.Add(prefix + "SPECIAL_SPIRITCUFF_DESCRIPTION", $"Release a ton of spirit energy that's been stored within; increasing attack power and speed.");

            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(YusukeMasteryAchievement.identifier), "Henry: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(YusukeMasteryAchievement.identifier), "As Henry, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
