using RoR2;
using UnityEngine;
using YusukeMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using UnityEngine.AddressableAssets;
using YusukeMod.Characters.Survivors.Yusuke.Extra;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        //spirit effects
        public static GameObject spiritGunChargeEffect;
        public static GameObject spiritGunChargePotentEffect;
        public static GameObject spiritGunMuzzleFlashEffect;

        public static GameObject spiritGunMegaChargeEffect;
        public static GameObject spiritGunMegaChargePotentEffect;
        public static GameObject spiritGunMegaMuzzleFlashEffect;

        public static GameObject spiritShotGunChargeEffect;
        public static GameObject spiritShotGunChargePotentEffect;
        public static GameObject spiritShotGunHitEffect;
        public static GameObject spiritShotGunTracerEffect;

        public static GameObject spiritWaveChargeEffect;
        public static GameObject spiritWaveChargePotentEffect;
        public static GameObject spiritWaveImpactEffect;

        // other effects
        public static GameObject dashStartSmallEffect;
        public static GameObject dashStartMaxEffect;
        public static GameObject dashGroundedEffect;
        public static GameObject dashAirEffect;
        public static GameObject dashBoomEffect;
        public static GameObject dashBoomContinuousEffect;
        public static GameObject chargeWindEffect;
        public static GameObject megaWindEffect;


        //explosion effects
        public static GameObject bombExplosionEffect;

        public static GameObject spiritGunExplosionEffect;
        public static GameObject spiritGunMegaExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject bombProjectilePrefab;
        public static GameObject basicSpiritGunPrefabPrimary;
        public static GameObject basicSpiritGunPrefab;
        public static GameObject spiritGunPiercePrefab;
        public static GameObject spiritGunMegaPrefab;

        //HUD
        public static GameObject SpiritCuffGauge;
        public static GameObject MazokuGauge;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("HenrySwordHit");

            CreateEffects();

            CreateProjectiles();

            CreateHUD();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();
            CreateSpiritExplosionEffects();
            CreateSpiritEnergyEffects();
            CreateHitAndOtherEffect();


            spiritShotGunTracerEffect = _assetBundle.LoadEffect("spiritShotgunBulletTrace");
            /*Tracer spiritTracer = spiritShotGunTracerEffect.AddComponent<Tracer>();
            spiritTracer.headTransform = spiritShotGunTracerEffect.transform.GetChild(1);
            spiritTracer.headTransform = spiritShotGunTracerEffect.transform.GetChild(2);
            spiritTracer.headTransform = spiritShotGunTracerEffect.transform.GetChild(3);
            spiritTracer.speed = 2f;
            spiritTracer.length = 1f;*/

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
        }

        private static void CreateHitAndOtherEffect()
        {
            Log.Info("Creating other effects. ");
            spiritShotGunHitEffect = _assetBundle.LoadEffect("spiritShotGunExplosion");
            dashStartSmallEffect = _assetBundle.LoadEffect("dashStartSmall", "dashStartSmall", false, false);
            dashStartMaxEffect = _assetBundle.LoadEffect("dashStartMax", "dashStartMax", false, false);
            dashGroundedEffect = _assetBundle.LoadEffect("dashEffectGrounded", "dashEffectGrounded", true, false);
            dashAirEffect = _assetBundle.LoadEffect("dashEffectAir", "dashEffectAir", true, false);
            dashBoomEffect = _assetBundle.LoadEffect("dashBoom", "dashBoom", true, false);
            dashBoomContinuousEffect = _assetBundle.LoadEffect("dashBoomContinuous", "dashBoomContinuous", true, false);
            chargeWindEffect = _assetBundle.LoadEffect("chargeWind", "chargeWind", true, false);
            megaWindEffect = _assetBundle.LoadEffect("megaWindEffect", "megaWindEffect", false, false);

        }

        private static void CreateSpiritEnergyEffects()
        {
            // last parameter determins if effectData should be skipped or not, some cases spawning effects are not using EffectManager, such as the charge effects. 
            spiritGunChargeEffect = _assetBundle.LoadEffect("spiritGunCharge", "spiritGunCharge", false, true);
            spiritGunChargePotentEffect = _assetBundle.LoadEffect("spiritGunChargePotent", "spiritGunChargePotent", false, true);
            spiritGunMegaChargeEffect = _assetBundle.LoadEffect("spiritMegaCharge", "spiritMegaCharge", true, true);
            spiritGunMegaChargePotentEffect = _assetBundle.LoadEffect("spiritMegaChargePotent", "spiritMegaChargePotent", true, true);

            Log.Info("loading shotgun effect");
            spiritShotGunChargeEffect = _assetBundle.LoadEffect("spiritShotGunCharge", "spiritShotGunCharge", true, true);
            spiritShotGunChargePotentEffect = _assetBundle.LoadEffect("spiritShotGunChargePotent", "spiritShotGunChargePotent", true, true);

            Log.Info("loading wave effect");
            spiritWaveChargeEffect = _assetBundle.LoadEffect("spiritWaveCharge", "spiritWaveCharge", true, true);
            spiritWaveChargePotentEffect = _assetBundle.LoadEffect("spiritWavePotent", "spiritWavePotent", true, true);

            spiritGunMuzzleFlashEffect = _assetBundle.LoadEffect("spiritGunMuzzleFlash", "spiritGunMuzzleFlash", true, false);
            spiritGunMegaMuzzleFlashEffect = _assetBundle.LoadEffect("spiritMegaMuzzle", "spiritMegaMuzzle", true, false);
            spiritWaveImpactEffect = _assetBundle.LoadEffect("spiritWaveImpact", "spiritWaveImpact", true, false);

        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }



        private static void CreateSpiritExplosionEffects()
        {
            spiritGunExplosionEffect = _assetBundle.LoadEffect("spiritGunExplosion", "spiritGunExplosion");

            if (!spiritGunExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = spiritGunExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 0.5f,
                frequency = 20f,
                cycleOffset = 0f
            };

            spiritGunMegaExplosionEffect = _assetBundle.LoadEffect("spiritgunMegaExplosionBigger", "spiritgunMegaExplosionBigger");

            if (!spiritGunExplosionEffect)
                return;

            ShakeEmitter megaShakeEmitter = spiritGunMegaExplosionEffect.AddComponent<ShakeEmitter>();
            megaShakeEmitter.amplitudeTimeDecay = true;
            megaShakeEmitter.duration = 1.5f;
            megaShakeEmitter.radius = 600f;
            megaShakeEmitter.scaleShakeRadiusWithLocalScale = false;

            megaShakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };
        }



        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateBombProjectile();
            Content.AddProjectilePrefab(bombProjectilePrefab);

            CreateBasicSpiritGun();
            Content.AddProjectilePrefab(basicSpiritGunPrefab);

            CreateBasicSpiritGunPrimary();
            Content.AddProjectilePrefab(basicSpiritGunPrefabPrimary);

            CreateSpiritGunPierce();
            Content.AddProjectilePrefab(spiritGunPiercePrefab);

            CreateSpiritGunMega();
            Content.AddProjectilePrefab(spiritGunMegaPrefab);
        }//

        private static void CreateBombProjectile()
        {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            bombProjectilePrefab = Asset.CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(bombProjectilePrefab.GetComponent<ProjectileImpactExplosion>());
            ProjectileImpactExplosion bombImpactExplosion = bombProjectilePrefab.AddComponent<ProjectileImpactExplosion>();
            
            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.blastDamageCoefficient = 1f;
            bombImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = bombExplosionEffect;
            bombImpactExplosion.lifetimeExpiredSound = Content.CreateAndAddNetworkSoundEventDef("HenryBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombProjectilePrefab.GetComponent<ProjectileController>();

            if (_assetBundle.LoadAsset<GameObject>("HenryBombGhost") != null)
                bombController.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("HenryBombGhost");
            
            bombController.startSound = "";
        }

        private static void CreateBasicSpiritGun()
        {
            // cloning
            GameObject baseSpiritGun = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            basicSpiritGunPrefab = PrefabAPI.InstantiateClone(baseSpiritGun, "basicSpiritGunProjectile");

            // add screen shake?


            // settings for the appearance
            ProjectileController spiritgunAesthetics = basicSpiritGunPrefab.GetComponent<ProjectileController>();

            // changing the prefab appearance for now
            if (_assetBundle.LoadAsset<GameObject>("spiritGunProjectile") != null)
            {
                spiritgunAesthetics.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("spiritGunProjectile");
            }
            else
            {
                spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");
            }


            // speed and duration
            ProjectileSimple spiritGunSpeed = basicSpiritGunPrefab.GetComponent<ProjectileSimple>();
            spiritGunSpeed.desiredForwardSpeed = 120;
            spiritGunSpeed.lifetime = 5;

            // explosion impact 
            ProjectileImpactExplosion spiritGunImpact = basicSpiritGunPrefab.GetComponent<ProjectileImpactExplosion>();
            spiritGunImpact.blastRadius = 8f;
            GameObject explosion = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion");
            spiritGunImpact.impactEffect = spiritGunExplosionEffect;



        }

        private static void CreateBasicSpiritGunPrimary()
        {
            // cloning
            GameObject baseSpiritGun = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            basicSpiritGunPrefabPrimary = PrefabAPI.InstantiateClone(baseSpiritGun, "basicSpiritGunProjectilePrimary");

            // add screen shake?

            // settings for the appearance
            ProjectileController spiritgunAesthetics = basicSpiritGunPrefabPrimary.GetComponent<ProjectileController>();
            spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");

            // speed and duration
            ProjectileSimple spiritGunSpeed = basicSpiritGunPrefabPrimary.GetComponent<ProjectileSimple>();
            spiritGunSpeed.desiredForwardSpeed = 120;
            spiritGunSpeed.lifetime = 5;

            // explosion impact 
            ProjectileImpactExplosion spiritGunImpact = basicSpiritGunPrefabPrimary.GetComponent<ProjectileImpactExplosion>();
            spiritGunImpact.blastRadius = 8f;
            GameObject explosion = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion");
            spiritGunImpact.impactEffect = spiritGunExplosionEffect;

            SkillTags tag = basicSpiritGunPrefabPrimary.AddComponent<SkillTags>();
            tag.isPrimary = true;

        }

        private static void CreateSpiritGunPierce()
        {
            // cloning
            GameObject baseSpiritGunPierce = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            spiritGunPiercePrefab = PrefabAPI.InstantiateClone(baseSpiritGunPierce, "basicSpiritGunProjectile");


            // add screen shake?

            // settings for the appearance
            ProjectileController spiritgunAesthetics = spiritGunPiercePrefab.GetComponent<ProjectileController>();
            //spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");      // Prefabs/ProjectileGhosts/FMJGhost

            // speed and duration
            ProjectileSimple spiritGunSpeed = spiritGunPiercePrefab.GetComponent<ProjectileSimple>();
            spiritGunSpeed.desiredForwardSpeed = 120;
            spiritGunSpeed.lifetime = 5;

            // explosion impact 
            ProjectileImpactExplosion spiritGunImpact = spiritGunPiercePrefab.GetComponent<ProjectileImpactExplosion>();
            spiritGunImpact.blastRadius = 8f;
            GameObject explosion = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FireworkExplosion");
            spiritGunImpact.impactEffect = explosion;


        }

        private static void CreateSpiritGunMega()
        {
            // cloning
            GameObject spiritGunMegaProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            spiritGunMegaPrefab = PrefabAPI.InstantiateClone(spiritGunMegaProjectile, "spiritGunMegaProjectile");

            // add screen shake?

            // settings for the appearance
            ProjectileController spiritgunAesthetics = spiritGunMegaPrefab.GetComponent<ProjectileController>();

            // changing the prefab appearance for now
            if (_assetBundle.LoadAsset<GameObject>("spiritGunProjectile") != null)
            {
                spiritgunAesthetics.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("spiritMegaProjectile");
            }
            else
            {
                spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");
            }


            // speed and duration
            ProjectileSimple spiritGunSpeed = spiritGunMegaPrefab.GetComponent<ProjectileSimple>();
            spiritGunSpeed.desiredForwardSpeed = 80;
            spiritGunSpeed.lifetime = 10;

            // explosion impact 
            ProjectileImpactExplosion spiritGunImpact = spiritGunMegaPrefab.GetComponent<ProjectileImpactExplosion>();
            spiritGunImpact.blastRadius = 60f;
            GameObject explosion = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/VagrantCannonExplosion");
            spiritGunImpact.impactEffect = spiritGunMegaExplosionEffect;



        }

        #endregion projectiles


        private static void CreateHUD()
        {
            Log.Info("Setting the UI OBJECT in Assets");
            SpiritCuffGauge = _assetBundle.LoadAsset<GameObject>("SpiritCuffGauge");
            MazokuGauge = _assetBundle.LoadAsset<GameObject>("MazokuGauge");

        }

    }
}
