using BepInEx;
using YusukeMod.Survivors.Yusuke;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace YusukeMod
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class YusukePlugin : BaseUnityPlugin
    {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.zamble.YusukeMod";
        public const string MODNAME = "yusuke";
        public const string MODVERSION = "1.0.0";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "ZAM";

        public static YusukePlugin instance;

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new YusukeSurvivor().Initialize();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }

        // the creation of charge effect objects are done here.
        public static GameObject CreateEffectObject(GameObject objectPrefab, Transform locPositon)
        {
            GameObject effectObject = null;
            if(objectPrefab != null)
            {
                effectObject = Object.Instantiate(objectPrefab, locPositon);
            }
            return effectObject;
        }
    }
}
