using RoR2;
using UnityEngine;
using YusukeMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using UnityEngine.AddressableAssets;

namespace YusukeMod.Survivors.Yusuke
{
    public static class YusukeAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        //explosion effects
        public static GameObject bombExplosionEffect;

        public static GameObject spiritGunExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject bombProjectilePrefab;
        public static GameObject basicSpiritGunPrefab;
        public static GameObject spiritGunPiercePrefab;
        public static GameObject spiritGunMegaPrefab;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("HenrySwordHit");

            CreateEffects();

            CreateProjectiles();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();
            CreateSpiritExplosionEffects();

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
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
            Content.AddProjectilePrefab(bombProjectilePrefab);

            CreateSpiritGunPierce();
            Content.AddProjectilePrefab(spiritGunPiercePrefab);

            CreateSpiritGunMega();
            Content.AddProjectilePrefab(spiritGunMegaPrefab);
        }

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
            spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");

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

        private static void CreateSpiritGunPierce()
        {
            // cloning
            GameObject baseSpiritGunPierce = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion();
            spiritGunPiercePrefab = PrefabAPI.InstantiateClone(baseSpiritGunPierce, "basicSpiritGunProjectile");


            // add screen shake?

            // settings for the appearance
            ProjectileController spiritgunAesthetics = spiritGunPiercePrefab.GetComponent<ProjectileController>();
            //spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/CaptainTazerGhost");

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
            spiritgunAesthetics.ghostPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/MageLightningBombGhost");

            // speed and duration
            ProjectileSimple spiritGunSpeed = spiritGunMegaPrefab.GetComponent<ProjectileSimple>();
            spiritGunSpeed.desiredForwardSpeed = 80;
            spiritGunSpeed.lifetime = 10;

            // explosion impact 
            ProjectileImpactExplosion spiritGunImpact = spiritGunMegaPrefab.GetComponent<ProjectileImpactExplosion>();
            spiritGunImpact.blastRadius = 40f;
            GameObject explosion = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/VagrantCannonExplosion");
            spiritGunImpact.impactEffect = explosion;



        }

        #endregion projectiles
    }
}
