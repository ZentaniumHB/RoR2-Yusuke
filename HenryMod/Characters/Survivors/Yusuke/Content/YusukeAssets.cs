using RoR2;
using UnityEngine;
using YusukeMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using UnityEngine.AddressableAssets;
using YusukeMod.Characters.Survivors.Yusuke.Extra;
using On;

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
        public static GameObject spiritShotGunTracer;

        public static GameObject spiritWaveChargeEffect;
        public static GameObject spiritWaveChargePotentEffect;
        public static GameObject spiritWaveImpactEffect;
        public static GameObject spiritWaveProjectileEffect;

        public static GameObject demonGunChargeEffect;
        public static GameObject demonGunChargePotentEffect;
        public static GameObject demonGunMuzzleFlashEffect;
        public static GameObject mazokuElectricChargeEffect;
        public static GameObject demonShotgunTracerEffect;
        public static GameObject demonShotgunChargeEffect;
        public static GameObject demonShotgunHitEffect;

        public static GameObject spiritgunBeamEffect;

        // other effects
        public static GameObject dashStartSmallEffect;
        public static GameObject dashStartMaxEffect;
        public static GameObject dashGroundedEffect;
        public static GameObject dashAirEffect;
        public static GameObject dashBoomEffect;
        public static GameObject dashBoomContinuousEffect;
        public static GameObject chargeWindEffect;
        public static GameObject megaWindEffect;

        public static GameObject hitImpactEffect;
        public static GameObject punchBarrageSlowEffect;
        public static GameObject punchBarrageFastEffect;
        public static GameObject heavyHitRingEffect;
        public static GameObject heavyHitRingFollowingEffect;
        public static GameObject finalHitEffect;
        public static GameObject stompEffect;

        public static GameObject mazokuTransformationRaizenStartupEffect;
        public static GameObject maokuTansformationExplosionEffect;

        public static GameObject shadowDashSK1;
        public static GameObject shadowDashGrabSK1;
        public static GameObject gutPunchSlowEffect;
        public static GameObject gutPunchFastEffect;

        public static GameObject spiritCuffReleaseEffect;
        public static GameObject spiritCuffEffect;

        public static GameObject meleeSwingEffect1;
        public static GameObject meleeSwingEffect2;
        public static GameObject meleeSwingEffect3;
        public static GameObject meleeSwingEffect4;

        public static GameObject throwSwingSingleEffect;
        public static GameObject throwWindEffect;
        public static GameObject blackCastShadowEffect;
        public static GameObject blackCastShadowEffectAir;

        public static GameObject vanishLinesWhite;
        public static GameObject vanishLinesBlack;

        //explosion effects
        public static GameObject bombExplosionEffect;

        public static GameObject spiritGunExplosionEffect;
        public static GameObject demonGunExplosionEffect;

        public static GameObject spiritGunMegaExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject bombProjectilePrefab;
        public static GameObject basicSpiritGunPrefabPrimary;
        public static GameObject basicSpiritGunPrefab;
        public static GameObject spiritGunPiercePrefab;
        public static GameObject spiritGunMegaPrefab;


        public static GameObject demonGunProjectilePrefab;

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

            //spiritShotGunTracer = _assetBundle.LoadEffect("spiritShotgunBulletTrace");
            //spiritShotGunTracer.AddComponent<Tracer>();

            spiritShotGunTracerEffect = Asset.CloneTracer("TracerGoldGat", "spiritShotGunTracer");

            spiritShotGunTracerEffect.GetComponent<LineRenderer>().startColor = new Color32(152,255,255,255);   // light cyan
            spiritShotGunTracerEffect.GetComponent<LineRenderer>().endColor = new Color32(0, 128, 255, 255);    // blue
            spiritShotGunTracerEffect.GetComponent<LineRenderer>().widthMultiplier = 0.8f; // sizing


            demonShotgunTracerEffect = Asset.CloneTracer("TracerGoldGat", "demonShotGunTracer");

            demonShotgunTracerEffect.GetComponent<LineRenderer>().startColor = new Color32(255, 0, 0, 255);   // red
            demonShotgunTracerEffect.GetComponent<LineRenderer>().endColor = new Color32(255, 0, 0, 178);    // light red
            demonShotgunTracerEffect.GetComponent<LineRenderer>().widthMultiplier = 0.8f; // sizing

            /*Tracer spiritTracer = spiritShotGunTracer.AddComponent<Tracer>();
            spiritTracer.headTransform = spiritShotGunTracer.transform.GetChild(1);
            spiritTracer.headTransform = spiritShotGunTracer.transform.GetChild(2);
            spiritTracer.headTransform = spiritShotGunTracer.transform.GetChild(3);
            spiritTracer.speed = 250f;
            spiritTracer.length = 40f;

            BeamPointsFromTransforms beamPoint = spiritShotGunTracer.AddComponent<BeamPointsFromTransforms>();
            beamPoint.target = spiritShotGunTracer.GetComponent<LineRenderer>();
            Transform[] transforms = new Transform[2];
            transforms[0] = spiritShotGunTracer.transform.GetChild(1);
            transforms[1] = spiritShotGunTracer.transform.GetChild(2);
            beamPoint.pointTransforms = transforms;

            Modules.Content.CreateAndAddEffectDef(spiritShotGunTracer);*/

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");


            meleeSwingEffect1 = _assetBundle.LoadEffect("meleeSwing1Effect", true);
            meleeSwingEffect2 = _assetBundle.LoadEffect("meleeSwing2Effect", true);
            meleeSwingEffect3 = _assetBundle.LoadEffect("meleeSwing3Effect", true);
            meleeSwingEffect4 = _assetBundle.LoadEffect("meleeSwing4Effect", true);
        }

        private static void CreateHitAndOtherEffect()
        {
            Log.Info("Creating other effects. ");

            spiritShotGunHitEffect = _assetBundle.LoadEffect("spiritShotGunExplosion");
            demonShotgunHitEffect = _assetBundle.LoadEffect("demonShotGunExplosion");

            dashStartSmallEffect = _assetBundle.LoadEffect("dashStartSmall", "dashStartSmall", false, false);
            dashStartMaxEffect = _assetBundle.LoadEffect("dashStartMax", "dashStartMax", false, false);
            dashGroundedEffect = _assetBundle.LoadEffect("dashEffectGrounded", "dashEffectGrounded", true, false);
            dashAirEffect = _assetBundle.LoadEffect("dashEffectAir", "dashEffectAir", true, false);
            dashBoomEffect = _assetBundle.LoadEffect("dashBoom", "dashBoom", true, false);
            dashBoomContinuousEffect = _assetBundle.LoadEffect("dashBoomContinuous", "dashBoomContinuous", true, false);
            chargeWindEffect = _assetBundle.LoadEffect("chargeWind", "chargeWind", true, false);
            megaWindEffect = _assetBundle.LoadEffect("megaWindEffect", "megaWindEffect", false, false);

            hitImpactEffect = _assetBundle.LoadEffect("hitImpact", "hitImpact", true, false);
            punchBarrageFastEffect = _assetBundle.LoadEffect("punchBarrageFast", "punchBarrageFast", true, false);
            punchBarrageSlowEffect = _assetBundle.LoadEffect("punchBarrageSlow", "punchBarrageSlow", true, false);

            heavyHitRingEffect = _assetBundle.LoadEffect("heavyHitRing", "heavyHitRing", false, false);
            heavyHitRingFollowingEffect = _assetBundle.LoadEffect("heavyHItRingFollow", "heavyHItRingFollow", true, false);

            finalHitEffect = _assetBundle.LoadEffect("finalHit", "finalHit", false, false);
            stompEffect = _assetBundle.LoadEffect("stomp", "stomp", false, false);

            mazokuTransformationRaizenStartupEffect = _assetBundle.LoadEffect("mazokuTransformStartUp", "mazokuTransformStartUp", true, false);
            maokuTansformationExplosionEffect = _assetBundle.LoadEffect("mazokuTransoformExplosion", "mazokuTransoformExplosion", true, false);

            spiritCuffReleaseEffect = _assetBundle.LoadEffect("spiritCuffReleaseState", "spiritCuffReleaseState", true, false);
            spiritCuffEffect = _assetBundle.LoadEffect("spiritCuffReleaseParticles", "spiritCuffReleaseParticles", true, true);

            shadowDashSK1 = _assetBundle.LoadEffect("shadowDashSK1", "shadowDashSK1", false, false);
            shadowDashGrabSK1 = _assetBundle.LoadEffect("shadowDashGrabSK1", "shadowDashGrabSK1", false, false);

            gutPunchSlowEffect = _assetBundle.LoadEffect("gutPunchEffectSlow", "gutPunchEffectSlow", true, false);
            gutPunchFastEffect = _assetBundle.LoadEffect("gutPunchEffectFast", "gutPunchEffectFast", true, false);

            throwSwingSingleEffect = _assetBundle.LoadEffect("ThrowSingleWind", "ThrowSingleWind", false, false);
            throwWindEffect = _assetBundle.LoadEffect("ThrowWind", "ThrowWind", false, false);

            blackCastShadowEffect = _assetBundle.LoadEffect("blackCastShadowFaded", "blackCastShadowFaded", true, false);
            blackCastShadowEffectAir = _assetBundle.LoadEffect("blackCastShadowFadedAir", "blackCastShadowFadedAir", true, false);

            vanishLinesWhite = _assetBundle.LoadEffect("vanishLinesWhite", "vanishLinesWhite", true, false);
            vanishLinesBlack = _assetBundle.LoadEffect("vanishLinesBlack", "vanishLinesBlack", true, false);

        }

        private static void CreateSpiritEnergyEffects()
        {
            // last parameter determins if effectData should be skipped or not, some cases spawning effects are not using EffectManager, such as the charge effects. 
            spiritGunChargeEffect = _assetBundle.LoadEffect("spiritGunCharge", "spiritGunCharge", false, true);
            spiritGunChargePotentEffect = _assetBundle.LoadEffect("spiritGunChargePotent", "spiritGunChargePotent", false, true);
            spiritGunMegaChargeEffect = _assetBundle.LoadEffect("spiritMegaCharge", "spiritMegaCharge", true, true);
            spiritGunMegaChargePotentEffect = _assetBundle.LoadEffect("spiritMegaChargePotent", "spiritMegaChargePotent", true, true);

            spiritgunBeamEffect = _assetBundle.LoadEffect("spiritgunBeam", "spiritgunBeam", true, false);

            Log.Info("loading shotgun effect");
            spiritShotGunChargeEffect = _assetBundle.LoadEffect("spiritShotGunCharge", "spiritShotGunCharge", true, true);
            spiritShotGunChargePotentEffect = _assetBundle.LoadEffect("spiritShotGunChargePotent", "spiritShotGunChargePotent", true, true);

            Log.Info("loading wave effect");
            spiritWaveChargeEffect = _assetBundle.LoadEffect("spiritWaveCharge", "spiritWaveCharge", true, true);
            spiritWaveChargePotentEffect = _assetBundle.LoadEffect("spiritWavePotent", "spiritWavePotent", true, true);
            spiritWaveProjectileEffect = _assetBundle.LoadEffect("spiritWaveProjectile", "spiritWaveProjectile", true, true);

            spiritGunMuzzleFlashEffect = _assetBundle.LoadEffect("spiritGunMuzzleFlash", "spiritGunMuzzleFlash", true, false);
            spiritGunMegaMuzzleFlashEffect = _assetBundle.LoadEffect("spiritMegaMuzzle", "spiritMegaMuzzle", true, false);
            spiritWaveImpactEffect = _assetBundle.LoadEffect("spiritWaveImpact", "spiritWaveImpact", true, false);

            demonGunChargeEffect = _assetBundle.LoadEffect("demonGunCharge", "demonGunCharge", false, true);
            demonGunChargePotentEffect = _assetBundle.LoadEffect("demonGunChargePotent", "demonGunChargePotent", false, true);
            mazokuElectricChargeEffect = _assetBundle.LoadEffect("mazokuElectricChargeObj", "mazokuElectricChargeObj", false, true);
            demonGunMuzzleFlashEffect = _assetBundle.LoadEffect("demonGunMuzzleFlash", "demonGunMuzzleFlash", true, false);

            demonShotgunChargeEffect = _assetBundle.LoadEffect("spiritDemonShotGunEffect", "spiritDemonShotGunEffect", true, true);

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

            spiritGunExplosionEffect.AddComponent<DestroyOnTimer>().duration = 3f;


            spiritGunMegaExplosionEffect = _assetBundle.LoadEffect("spiritgunMegaExplosionBigger", "spiritgunMegaExplosionBigger");

            if (!spiritGunMegaExplosionEffect)
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

            spiritGunMegaExplosionEffect.AddComponent<DestroyOnTimer>().duration = 5f;

            demonGunExplosionEffect = _assetBundle.LoadEffect("demonGunExplosion", "demonGunExplosion");

            if (!demonGunExplosionEffect)
                return;

            ShakeEmitter demonShakeEmitter = demonGunExplosionEffect.AddComponent<ShakeEmitter>();
            demonShakeEmitter.amplitudeTimeDecay = true;
            demonShakeEmitter.duration = 0.5f;
            demonShakeEmitter.radius = 200f;
            demonShakeEmitter.scaleShakeRadiusWithLocalScale = false;

            demonShakeEmitter.wave = new Wave
            {
                amplitude = 0.5f,
                frequency = 20f,
                cycleOffset = 0f
            };

            demonGunExplosionEffect.AddComponent<DestroyOnTimer>().duration = 3f;

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
            spiritGunSpeed.lifetime = 15;

            // explosion impact 
            ProjectileImpactExplosion spiritGunImpact = basicSpiritGunPrefab.GetComponent<ProjectileImpactExplosion>();
            spiritGunImpact.blastRadius = 8f;
            spiritGunImpact.impactEffect = spiritGunExplosionEffect;


            // [DEMON GUN] --------------------------------------

            // cloning
            GameObject demonGun = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            demonGunProjectilePrefab = PrefabAPI.InstantiateClone(demonGun, "demonGunProjectile");

            // add screen shake?


            // settings for the appearance
            ProjectileController demonGunAesthetics = demonGunProjectilePrefab.GetComponent<ProjectileController>();

            // changing the prefab appearance for now
            if (_assetBundle.LoadAsset<GameObject>("demonGunProjectile") != null)
            {
                demonGunAesthetics.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("demonGunProjectile");
            }
            else
            {
                demonGunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");
            }


            // speed and duration
            ProjectileSimple demonGunSpeed = demonGunProjectilePrefab.GetComponent<ProjectileSimple>();
            demonGunSpeed.desiredForwardSpeed = 160;
            demonGunSpeed.lifetime = 5;

            // explosion impact 
            ProjectileImpactExplosion demonGunImpact = demonGunProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            demonGunImpact.blastRadius = 8f;
            demonGunImpact.impactEffect = demonGunExplosionEffect;



        }

        private static void CreateBasicSpiritGunPrimary()
        {
            // cloning
            GameObject baseSpiritGun = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            basicSpiritGunPrefabPrimary = PrefabAPI.InstantiateClone(baseSpiritGun, "basicSpiritGunProjectile");

            // add screen shake?

            // settings for the appearance
            ProjectileController spiritgunAesthetics = basicSpiritGunPrefabPrimary.GetComponent<ProjectileController>();

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
            ProjectileSimple spiritGunSpeed = basicSpiritGunPrefabPrimary.GetComponent<ProjectileSimple>();
            spiritGunSpeed.desiredForwardSpeed = 120;
            spiritGunSpeed.lifetime = 15;

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
            spiritGunSpeed.lifetime = 30;

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
