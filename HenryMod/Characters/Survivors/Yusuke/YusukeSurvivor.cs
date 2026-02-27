using BepInEx.Configuration;
using YusukeMod.Modules;
using YusukeMod.Modules.Characters;
using YusukeMod.Survivors.Yusuke.Components;
using YusukeMod.Survivors.Yusuke.SkillStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

using YusukeMod.Modules.BaseStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Tracking;
using YusukeMod.SkillStates;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Followups;
using YusukeMod.Characters.Survivors.Yusuke.Components;
using RoR2.UI;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.PowerUp;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.SpiritAttack;
using RoR2.Projectile;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using Rewired.Utils;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.Grabs;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates;
using System.Xml.Linq;
using YusukeMod.Characters.Survivors.Yusuke.SkillStates.OverdriveStates;

namespace YusukeMod.Survivors.Yusuke
{
    public class YusukeSurvivor : SurvivorBase<YusukeSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "yusukeassetbundle"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "YusukeBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "YusukeMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlYusukeUrameshi";
        public override string displayPrefabName => "YusukeUrameshiDisplay";

        public const string YUSUKE_PREFIX = YusukePlugin.DEVELOPER_PREFIX + "_YUSUKE_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => YUSUKE_PREFIX;


       


        // loadout skills
        internal static SkillDef primaryMelee;
        internal static SkillDef primarySpiritGun;

        internal static SkillDef secondarySpiritGun;
        internal static SkillDef secondarySpiritShotgun;

        internal static SkillDef utilityDash;
        internal static SkillDef utilityWave;

        internal static SkillDef specialSpiritGunMega;
        internal static SkillDef specialSpiritCuff;

        // Sprites

        internal static Sprite spiritGunIcon;
        internal static Sprite spiritBeamIcon;
        internal static Sprite spiritGunDoubleIcon;
        internal static Sprite spiritShotgunIcon;
        internal static Sprite spiritShotGunDoubleIcon;


        // follow-up skills
        internal static SkillDef meleeFollowUp;
        internal static SkillDef spiritGunFollowUp;
        internal static SkillDef spiritShotgunFollowUp;

        // mazoku moves
        internal static SkillDef mazokuMelee;
        internal static SkillDef mazokuDemonGun;
        internal static SkillDef mazokuDemonGunPrimary;
        internal static SkillDef backToBackStrikes;
        internal static SkillDef demonDash;
        internal static SkillDef swingCombo;
        internal static SkillDef demonGunMega;

        internal static SkillDef overdriveSpiritSnipe;
        internal static SkillDef overdriveSpiritShotgunAA12;
        internal static SkillDef overdriveSpiritWaveImpactFist;
        internal static SkillDef overdriveSpiritFlow;
        internal static SkillDef overdrive12Hook;

        //HUD
        internal static HUD hud = null;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = YUSUKE_PREFIX + "NAME",
            subtitleNameToken = YUSUKE_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = Color.white,
            sortPosition = 100,

            crosshair = Asset.LoadCrosshair("Standard"),
            podPrefab = null,   // set to null to prevent pod spawning animation when starting

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                /*new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = assetBundle.LoadMaterial("matHenry"),
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                },
                new CustomRendererInfo
                {
                    childName = "Model",

                },*/
                new CustomRendererInfo
                {
                    childName = "Model",
                    
                },
                new CustomRendererInfo
                {
                    childName = "YusukeHair1",
                    
                },
                new CustomRendererInfo
                {
                    childName = "YusukeHair2",
                    
                }
        };

        public override UnlockableDef characterUnlockableDef => YusukeUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new YusukeItemDisplays();

        public override Type characterDeathState => typeof(YusukeDeathState);

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }



        public override void Initialize()
        {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            YusukeUnlockables.Init();

            base.InitializeCharacter();

            YusukeConfig.Init();
            YusukeStates.Init();
            YusukeTokens.Init();

            YusukeAssets.Init(assetBundle);
            YusukeBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<YusukeWeaponComponent>();
            bodyPrefab.AddComponent<SingleTracking>().TurnOff();
            bodyPrefab.AddComponent<Hook12Tracking>().TurnOff();
            bodyPrefab.AddComponent<SpiritSnipeTracking>().TurnOff();
            bodyPrefab.AddComponent<YusukeHUD>();
            bodyPrefab.AddComponent<SpiritCuffComponent>();
            bodyPrefab.AddComponent<SacredComponent>();

            bodyPrefab.AddComponent<PivotRotation>();   // visual pivot rotation for the animations and vfx
            bodyPrefab.AddComponent<PitchYawControl>();
            LoadAdditionalSprites();
            //anything else here
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            //Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", "SwordHitbox");

            Prefabs.SetupHitBoxGroup(characterModelObject, "MeleeGroup", "meleeHitBox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "divePunchGroup", "divePunchHitBox");

            Prefabs.SetupHitBoxGroup(characterModelObject, "overdriveShotgunSingleGroup", "overdriveShotgunSingleHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "overdriveShotgunFinalGroup", "overdriveShotgunFInalHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "overdriveWaveGroup", "overdriveWaveHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "mazokuExplosionGroup", "mazokuExplosionHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "overdrive12HookUppercutGroup", "overdrive12HookUppercutHitBox");
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(YusukeMain), typeof(EntityStates.SpawnTeleporterState));   // setting the main state, the spawning state, and body
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
            Prefabs.AddEntityStateMachine(bodyPrefab, "MazokuWeapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Overdrive");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            //AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();

            // creating follow up skills 
            CreateFollowUpSkills();
            CreateMazokuSkills();
            CreateOverdriveSkills();

        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = YUSUKE_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "PASSIVE_DESCRIPTION",
                keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };

            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryPassive",
                skillNameToken = YUSUKE_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            SteppedSkillDef primarySkillDef1 = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "HenrySlash",
                    YUSUKE_PREFIX + "PRIMARY_SLASH_NAME",
            YUSUKE_PREFIX + "PRIMARY_SLASH_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.PunchCombo)),
                    "Weapon",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            primarySkillDef1.stepCount = 4;
            primarySkillDef1.stepGraceDuration = 0.5f;

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef1);
            YusukeSurvivor.primaryMelee = primarySkillDef1;



            SkillDef primarySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo
            { 
                skillName = "YusukeSpiritGunPrimary",
                skillNameToken = YUSUKE_PREFIX + "PRIMARY_GUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "PRIMARY_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpiritGunIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeSpiritGunPrimary)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 5f,
                baseMaxStock = 4,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef2);
            YusukeSurvivor.primarySpiritGun = primarySkillDef2;

        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            //here is a basic skill def with all fields accounted for
            SkillDef secondarySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeSpiritGun",
                skillNameToken = YUSUKE_PREFIX + "SECONDARY_GUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "SECONDARY_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpiritGunIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeSpiritGun)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 5f,
                baseMaxStock = 4,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef1);
            YusukeSurvivor.secondarySpiritGun = secondarySkillDef1;

            SkillDef secondarySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeShotgun",
                skillNameToken = YUSUKE_PREFIX + "SECONDARY_SHOTGUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "SECONDARY_SHOTGUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpiritShotgunIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeSpiritShotgun)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 5f,
                baseMaxStock = 2,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef2);
            YusukeSurvivor.secondarySpiritShotgun = secondarySkillDef2;

        }

        private void AddUtiitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            //here's a skilldef of a typical movement skill.
            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeSlideDash",
                skillNameToken = YUSUKE_PREFIX + "UTILITY_SLIDEDASH_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "UTILITY_SLIDEDASH_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Roll)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.Frozen,

                baseRechargeInterval = 5f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1);
            YusukeSurvivor.utilityDash = utilitySkillDef1;


            SkillDef utilitySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeSpiritWave",
                skillNameToken = YUSUKE_PREFIX + "UTILITY_WAVE_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "UTILITY_WAVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeSpiritWave)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 5f,
                baseMaxStock = 2,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef2);
            YusukeSurvivor.utilityWave = utilitySkillDef2;

        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            //a basic skill. some fields are omitted and will just have default values
            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeBomb",
                skillNameToken = YUSUKE_PREFIX + "SPECIAL_SPIRITMEGA_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "SPECIAL_SPIRITMEGA_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeSpiritGunMega)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 15f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false

            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1);
            YusukeSurvivor.specialSpiritGunMega = specialSkillDef1;


            SkillDef specialSkillDef2 = Skills.CreateCuffSkillDef(new SkillDefInfo
            {
                skillName = "YusukeSpiritCuff",
                skillNameToken = YUSUKE_PREFIX + "SPECIAL_SPIRITCUFF_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "SPECIAL_SPIRITCUFF_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ReleaseSpiritCuff)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 100,
                requiredStock = 0,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false

            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef2);
            YusukeSurvivor.specialSpiritCuff = specialSkillDef2;
        }

        // follow up skills being created
        private void CreateFollowUpSkills()
        {

            YusukeSurvivor.meleeFollowUp = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeFollowUpMelee",
                skillNameToken = YUSUKE_PREFIX + "FOLLOWUP_MELEE_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "FOLLOWUP_MELEEE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(DivePunch)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 30f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            YusukeSurvivor.spiritGunFollowUp = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeFollowUpSpiritGun",
                skillNameToken = YUSUKE_PREFIX + "FOLLOWUP_GUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "FOLLOWUP_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SpiritGunFollowUp)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Frozen,

                baseRechargeInterval = 20f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                
                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            YusukeSurvivor.spiritShotgunFollowUp = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeFollowUpShotgun",
                skillNameToken = YUSUKE_PREFIX + "FOLLOWUP_SHOTGUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "FOLLOWUP_SHOTGUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SpiritShotgunFollowUp)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Frozen,

                baseRechargeInterval = 10f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });


        }

        // creation of the mazoku skills
        private void CreateMazokuSkills()
        {

            SteppedSkillDef mazokuPrimnary = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "MazokuMelee",
                    YUSUKE_PREFIX + "PRIMARY_MAZOKUMELEE_NAME",
            YUSUKE_PREFIX + "PRIMARY_MAZOKUMELEE_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon0"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.SlashCombo)),
                    "Weapon2",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            mazokuPrimnary.stepCount = 2;
            mazokuPrimnary.stepGraceDuration = 0.5f;
            YusukeSurvivor.mazokuMelee = mazokuPrimnary;


            YusukeSurvivor.mazokuDemonGun = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeDemonGun",
                skillNameToken = YUSUKE_PREFIX + "SECONDARY_MAZOKUGUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "MAZOKU_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texDemonGunIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeDemonGun)),
                activationStateMachineName = "MazokuWeapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 15f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });


            YusukeSurvivor.mazokuDemonGunPrimary = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeDemonGunPrimary",
                skillNameToken = YUSUKE_PREFIX + "PRIMARY_MAZOKUGUN_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "MAZOKU_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texDemonGunIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeDemonGunPrimary)),
                activationStateMachineName = "MazokuWeapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 15f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });


            YusukeSurvivor.backToBackStrikes = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeBackToBack",
                skillNameToken = YUSUKE_PREFIX + "SECONDARY_MAZBACKTOBACK_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "MAZOKU_BACKTOBACK_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(MazBackToBackStrikes)),
                activationStateMachineName = "MazokuWeapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 10f,
                baseMaxStock = 2,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });


            YusukeSurvivor.demonDash = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeDemonDash",
                skillNameToken = YUSUKE_PREFIX + "UTILITY_MAZDEMONDASH_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "MAZOKU_DEMONDASH_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(DemonDash)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.Frozen,

                baseRechargeInterval = 5f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });



            YusukeSurvivor.swingCombo = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeSwingCombo",
                skillNameToken = YUSUKE_PREFIX + "UTILTY_MAZSWINGCOMBO_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "MAZOKU_SWINGCOMBO_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SwingCombo)),
                activationStateMachineName = "MazokuWeapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 12f,
                baseMaxStock = 2,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });


            YusukeSurvivor.demonGunMega = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeDemonGunMega",
                skillNameToken = YUSUKE_PREFIX + "SPECIAL_MAZ_MEGA_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "SPECIAL_MAZ_MEGA_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texScepterSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeDemonGunMega)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "MazokuWeapon",
                interruptPriority = EntityStates.InterruptPriority.Stun,

                baseRechargeInterval = 25f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false

            });

        }


        private void CreateOverdriveSkills()
        {
            YusukeSurvivor.overdriveSpiritSnipe = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeOverdriveSpiritSnipe",
                skillNameToken = YUSUKE_PREFIX + "OVERDRIVE_SPIRITSNIPE_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "OVERDRIVE_SPIRITSNIPE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(OverdriveSpiritSnipe)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 200f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            YusukeSurvivor.overdriveSpiritShotgunAA12 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeOverdriveSpiritShotgunAA12",
                skillNameToken = YUSUKE_PREFIX + "OVERDRIVE_SHOTGUNAA12_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "OVERDRIVE_SHOTGUNAA12_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(OverdriveSpiritShotgunAA12)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 200f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            YusukeSurvivor.overdriveSpiritWaveImpactFist = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeOverdriveSpiritWaveImpactFist",
                skillNameToken = YUSUKE_PREFIX + "OVERDRIVE_SPIRITWAVE_IMPACT_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "OVERDRIVE_SPIRITWAVE_IMPACT_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(OverdriveSpiritWaveImpactFist)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 200f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            YusukeSurvivor.overdriveSpiritFlow = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeOverdriveSpiritFlow",
                skillNameToken = YUSUKE_PREFIX + "OVERDRIVE_SPIRITFLOW_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "OVERDRIVE_SPIRITFLOW_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(OverdriveSpiritFlow)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 200f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            YusukeSurvivor.overdrive12Hook = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "YusukeOverdrive12Hooks",
                skillNameToken = YUSUKE_PREFIX + "OVERDRIVE_12HOOKS_NAME",
                skillDescriptionToken = YUSUKE_PREFIX + "OVERDRIVE_12HOOKS_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("0"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Overdrive12Hooks)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Death,

                baseRechargeInterval = 200f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = true,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

        }


        #endregion skills


        private void LoadAdditionalSprites()
        {   
            // the icons here are mainly used for visual representation of the moves that will come out during certain states, will switch between them
            spiritGunIcon = assetBundle.LoadAsset<Sprite>("texSpiritGunIcon");
            spiritBeamIcon = assetBundle.LoadAsset<Sprite>("texSpiritBeamIcon");
            spiritGunDoubleIcon = assetBundle.LoadAsset<Sprite>("texSpiritGunDoubleIcon");

            spiritShotgunIcon = assetBundle.LoadAsset<Sprite>("texSpiritShotgunIcon");
            spiritShotGunDoubleIcon = assetBundle.LoadAsset<Sprite>("texSpiritShotgunTimes2");

        }

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin

            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(YUSUKE_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);

            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            YusukeAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.GlobalEventManager.OnCharacterDeath += MazokuIncrease;
            On.RoR2.UI.HUD.Awake += GetHUD;
            On.RoR2.CharacterMaster.OnBodyStart += Run_onRunStartGlobal;
            On.RoR2.BulletAttack.ProcessHit += BulletProcessHit;
            On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += ProjectilePocessExplosion;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //On.RoR2.CharacterMaster.Respawn += CharacterMaster_Respawn;


        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            float percentageTrigger = 0.15f;
            /* take damage hook for now, until I find a better way to do this, the problem with this is that it doesn't account for any buffs or debuffs
             *  so there is a chance that it can return as non lethal but actually is with buffs.
             */
            

            orig(self, damageInfo);
        }

        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {

            orig(self, damageInfo);
            if (self)
            {
                if (self.body.name.Contains("YusukeBody"))
                {
                    YusukeWeaponComponent yusukeWeaponComponent = self.body.GetComponent<YusukeWeaponComponent>();
                    //Log.Info("HEALTH NOW: " + percentageTrigger * self.body.maxHealth);
                    if (yusukeWeaponComponent)
                    {
                        if (self.health - damageInfo.damage <= 0 && !yusukeWeaponComponent.GetKnockedBoolean())
                        {
                            if (yusukeWeaponComponent.GetMazokuRevive() || yusukeWeaponComponent.GetSacredEnergyRevive()) // or other revive (soon)
                            {
                                Log.Info("Critical low health, trigger knocked state!");
                                damageInfo.damage = 0;
                                yusukeWeaponComponent.SetKnockedState(true);

                            }

                        }
                    }

                }
            }

        }

        /* private CharacterBody CharacterMaster_Respawn(On.RoR2.CharacterMaster.orig_Respawn orig, CharacterMaster self, Vector3 footPosition, Quaternion rotation, bool wasRevivedMidStage)
         {
             // recreating the character, which applies to revives and spawning into the next stage
             CharacterBody body = orig(self, footPosition, rotation, wasRevivedMidStage);

             // if respawning from the next stage and not mid stage, then do what is necessary

             if (self.GetBodyObject().name.Contains("YusukeBody"))
             {
                 if (wasRevivedMidStage == false)
                 {
                     // checking the number of stages, greater than zero is necessary so it won't overlap the very start animation
                     if (Run.instance.stageClearCount > 0)
                     {
                         // play a spawn sound here
                         //Util.PlaySound("Play_VoiceLetsGo2", body.gameObject);
                         //body.GetComponent<EntityStateMachine>().SetNextState(new YusukeBeginSpawn());
                     }

                 }
             }

             return body;
         }*/


        private void ProjectilePocessExplosion(On.RoR2.Projectile.ProjectileImpactExplosion.orig_OnProjectileImpact orig, RoR2.Projectile.ProjectileImpactExplosion self, ProjectileImpactInfo impactInfo)
        {
            
            // grab the collider and hurtbox
            Collider collider = impactInfo.collider;
            HurtBox component = collider.GetComponent<HurtBox>();
            if ((bool)component)
            {
                // check the teamindex of the object and make sure it is a monster
                Log.Info("Object's team index: " +component.teamIndex);
                if (component.teamIndex == TeamIndex.Monster)
                {
                    // checking if the owner of the projectile has the spiritcuffcomponent, also checking if they have the tag
                    ProjectileController controller = self.GetComponent<ProjectileController>();
                    SkillTags tag = self.GetComponent<SkillTags>();

                    if ((bool)controller && tag != null)
                    {
                        if (controller.owner.GetComponent<SpiritCuffComponent>()) 
                        {
                            /* if so, check the values in the skilltags component and check if primary is true. This determins which spirit gun
                                is the primary and which is the secondary. 
                            */
                            SpiritCuffComponent cuffComponent = controller.owner.GetComponent<SpiritCuffComponent>();
                            if (cuffComponent && tag.isPrimary == true) 
                            {
                                Log.Info("Incerasing cuff from primary spirit gun. ");
                                cuffComponent.IncreaseCuff(2f);

                            } 
                        }
                    }
                }
            }
            orig(self, impactInfo);
        }

        private bool BulletProcessHit(On.RoR2.BulletAttack.orig_ProcessHit orig, BulletAttack self, ref BulletAttack.BulletHit hitInfo)
        {
            bool process = orig(self, ref hitInfo);

            if (self.owner)
            {
                // the bullet fired will check whether the gameobject has the skillTag component that is always added whenever the spirit beam bullet is created and fired
                if (self.owner.GetComponent<SpiritCuffComponent>() && self.owner.gameObject.GetComponent<SkillTags>())
                {
                    // if so, it will remove the tag and will increase the spirit cuff component accordingly.
                    SkillTags tag = self.owner.gameObject.GetComponent<SkillTags>();
                    tag.Remove();
                    SpiritCuffComponent cuffComponent = self.owner.GetComponent<SpiritCuffComponent>();
                    if (cuffComponent.hasReleased) cuffComponent.IncreaseCuff(2f);  // if in the released state, increase it by 2
                }
            }

            return process;

        }

        private void Run_onRunStartGlobal(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {

            // adding components to the correct characterMaster
            orig(self, body);
            // cehcks if the playerCharacterMaster controller exists
            if (self.playerCharacterMasterController != null)
            {
                //used to check the local user and checks if it exists
                LocalUser localuser = LocalUserManager.GetFirstLocalUser();
                if (localuser != null && localuser.currentNetworkUser != null)
                {
                    // checking if the networkUser that is storing the players info is the same as the local one. 
                    if (self.playerCharacterMasterController.networkUser == localuser.currentNetworkUser)
                    {

                       /* Log.Info("self master net ID: " + self.playerCharacterMasterController.netId);
                        Log.Info("Localuser net ID: " + Localuser.currentNetworkUser.netId);
                        if(body) Log.Info("netID from charactermaster on body: " + body.master.playerCharacterMasterController.netId);
                        Log.Info("This master belongs to you.");*/

                        if(!self.gameObject.GetComponent<MazokuComponent>())
                        {
                            self.gameObject.AddComponent<MazokuComponent>();
                            Log.Info("MazokuComponent added");
                            
                        }
                        else
                        {
                            Log.Info("MazokuComponent already exists");
                        }
                         

                    }
                }
            }

            
        }

        

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if(damageReport.attackerBody != null && damageReport.attacker != null && damageReport != null)
            {

                if (damageReport.victim)
                {
                    //Killed eneny was found check what attack killed them through damageType
                    if(damageReport.damageInfo.damageType == DamageType.SlowOnHit)
                    {
                        // Enemy was killed by shotgun as it's the only attack that has the SlowOnHit type, so find and replenish the appropriate skills

                        if (damageReport.attackerBody.skillLocator.utility.activationState.stateType == typeof(Roll))
                        {
                            
                            float value = damageReport.attackerBody.skillLocator.utility.rechargeStopwatch;
                            damageReport.attackerBody.skillLocator.utility.RunRecharge(value);

                        }


                    }

                    

                    
                }
            }
            //throw new NotImplementedException();
        }

        private void MazokuIncrease(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            // adding components to the correct characterMaster
            orig(self, damageReport);
            if (damageReport.attackerBody != null && damageReport.attacker != null && damageReport != null)
            {

                if (damageReport.victim)
                {
                    //used to check the local user and checks if it exists
                    LocalUser localuser = LocalUserManager.GetFirstLocalUser();
                    if (localuser != null && localuser.currentNetworkUser != null)
                    {
                        // iterating through all masters within the list to check if they contain a YusukeBody
                        for (int i = CharacterMaster.readOnlyInstancesList.Count - 1; i >= 0; i--)
                        {
                            CharacterMaster master = CharacterMaster.readOnlyInstancesList[i];
                            if (master.teamIndex == TeamIndex.Player && master.bodyPrefab == BodyCatalog.FindBodyPrefab("YusukeBody"))
                            {
                                // checking if the master belongs to the local player
                                if (master.playerCharacterMasterController.networkUser == localuser.currentNetworkUser)
                                {
                                    // increasing the mazoku value (using the increase value float instaed as it will cause a stack overflow when trying to trigger the method in here.
                                    MazokuComponent mazokuComponent = master.GetComponent<MazokuComponent>();
                                    
                                    if (damageReport.victimIsBoss)
                                    {
                                        mazokuComponent.increaseValue = 100f; //10f
                                    }
                                    else if (damageReport.victimIsElite)
                                    {
                                        mazokuComponent.increaseValue = 100f; //5f
                                    }
                                    else
                                    {
                                        mazokuComponent.increaseValue = 100f; //1f
                                    }

                                }
                            }

                        }
                    }

                }
            }
            
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            /*if (damageInfo.attacker != null && damageInfo != null)
            {
                if (damageInfo.attacker.name.Contains("YusukeBody"))
                {
                    SacredComponent sacredComponent = damageInfo.attacker.gameObject.GetComponent<SacredComponent>();
                    YusukeWeaponComponent YusukeWeaponComponent = damageInfo.attacker.gameObject.GetComponent<YusukeWeaponComponent>();

                    if (YusukeWeaponComponent) Log.Info("YusukeWeaponComp exists");
                    if (sacredComponent) Log.Info("sacredComponent exists");
                    if (YusukeWeaponComponent && sacredComponent && YusukeWeaponComponent.GetSacredEnergyReleased())
                    {
                        sacredComponent.IncreaseSacredGauge(1);
                    }
                }
            }*/
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {

            if (sender.HasBuff(YusukeBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }

            if (sender.HasBuff(YusukeBuffs.spiritMegaSlowDebuff))
            {
                args.moveSpeedReductionMultAdd += 0.8f; // 80 percent movement debuff
            }


            if (sender.HasBuff(YusukeBuffs.spiritMegaArmourBuff))
            {
                args.armorAdd += 500f; // 500 armor buff
            }

        }


        private void GetHUD(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self); // Don't forget to call this, or the vanilla / other mods' codes will not execute!
            hud = self;

            //hud.mainContainer.transform // This will return the main container. You should put your UI elements under it or its children!
            // Rest of the code is to go here
        }

        private void OnDestroy()
        {
            On.RoR2.UI.HUD.Awake -= GetHUD;
        }

    }
}