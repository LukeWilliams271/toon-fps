using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Attack Pattern", menuName = "Attack Pattern")]
public class AttackPatterns : ScriptableObject
{
    //Stores stats for a specific attack

    [System.Serializable]
    public struct VFXAndSFX
    {
        public string shootSFX;
        public float startingLag;
        public float endingLag;
        public Sprite[] bulletSprites;
        public Texture[] impactTexture;
        public Vector3 impactTextureSize;
        public bool impactOnBounce;
        public AnimationSet dirAnimations;
    }
    public VFXAndSFX vFXAndSFX;

    [System.Serializable]
    public struct WeaponProperties
    {
        public GameObject spreadPattern;

        public float range;
        //public bool shootWhileMoving = false;
        public GameObject strafePattern;

        public float shootForce;
        public float upwardForce;

        public float spread, timeBetweenShots;
        public int bulletsPerTap;
    }
    public WeaponProperties weaponProperties;

    [System.Serializable]
    public struct BulletProperties
    {
        public float bulletSize;
        public int bulletDamage;
        public float maxLifetime;
        [Range(0, 1)]
        public float guided;
        public float guidedRampUp;
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
        public bool explodesOnCollision;
        public bool explodeOnHit;
        public AnimationClip explosionAnimation;
        public AttackPatterns explosionAttackPattern;
    }
    public ExplosionProperties explosionProperties;
}
