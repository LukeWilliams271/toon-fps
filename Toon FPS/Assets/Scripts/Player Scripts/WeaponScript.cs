using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class WeaponScript : ScriptableObject
{

    [HideInInspector]
    public PlayerCont player;

    [System.Serializable]
    public struct TabInfo
    {
        public Sprite tabMenuSprite;
        public string formalName;
        public string formalNameShort;
        [TextArea(10, 15)]
        public string description;
        [TextArea(10, 15)]
        public string tutorial;
    }
    public TabInfo tabInfo;

    [System.Serializable]
    public struct MetaSetup
    {
        public RuntimeAnimatorController anim;

        public Sprite pickupSprite;
        public Sprite[] bulletSprites;
        public enum sPattern { Random, InputOrder, None }
        public sPattern spritePattern;

        public Texture[] impactSprite;
        public Vector3 impactSpriteSize;
        public Sprite crosshairSprite;
        public float crosshairSizeScale;

        [Space(1)]
        [Header("SoundFX")]
        public Sound shootSFX;
        public Sound altShootSFX;
        public Sound reloadSFX;
        public Sound bulletSFX;
        [Space(1)]
        public float camShakeMagnitude;
        public float camShakeRoughness;
        public float camShakeFadeInTime;
        public float camShakeFadeOutTime;
    }
    [Header("Basic Settings")]
    public MetaSetup metaSetup;
    
    [System.Serializable]
    public struct WeaponProperties
    {
        public WeaponScript altFireWeaponScript;
        [Space]
        public WeaponStatus weaponStatus;
        [Space]
        public GameObject spreadPattern;
        [Space]
        public float shootForce;
        public float upwardForce;

        public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
        public int magSize, bulletsPerTap, bulletsPerOneAmmo;
        public bool allowTiggerHold, consumeAmmo;
        public float chargeTime, unchargeTime, switchToTime;

        public enum m_rarity { Common, Rare, Epic, Legendary }
        public m_rarity rarity;
    }
    public WeaponProperties weaponProperties;

    [System.Serializable]
    public struct BulletProperties
    {
        public Vector3 bulletSize;
        public int bulletDamage;
        public float maxLifetime;
        public int pierce;
        public enum origin { Character, Projectile }
        public origin originatesFrom;
        [Range(0, 1)]
        public float lockOn;
        public float lockOnRampUp;
        public LayerMask whatIsEnemy;
        public LayerMask whatIsIgnore;
    }
    public BulletProperties bulletProperties;

    [System.Serializable]
    public struct BulletPhysics
    {
        [Range(0, 1)]
        public float bounciness;
        public float gravityMultiplier;
        public int maxCollisions;
    }
    public BulletPhysics bulletPhysics; 

    [System.Serializable]
    public struct ExplosionProperties
    {
        public bool explodesOnLifetime;
        public int explodesOnCollisionNum;
        public bool explodeOnHit;
        public AnimationClip explosionAnimation;
        public Sound explosionSFX;
        public WeaponScript explosionWeaponScript;
    }
    public ExplosionProperties explosionProperties;

    
}
