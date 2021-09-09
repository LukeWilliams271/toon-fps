using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPooledObject
{    // Script for the projectile fired from any weapon.

    public SpriteRenderer spriteRend;

    [Header("Assignables")]
    public GameObject explosionPrefab;
    public LayerMask whatIsViable;

    public AttackPatterns attackPattern;
    public PlayerCont player;

    [Header("Custom Properties")]
    [HideInInspector]
    public Texture impactTexture;

    private int collisions;
    private PhysicMaterial physicsMat;

    private float maxLifetime;

    private float gravMult;
    public GameMaster master;

    public float lockOnRampUp;

    int lastAttackPoint, bulletsShot;
    public GameObject simpleAnimation;

    // Start is called before the first frame update
    public void OnSpawn(float delay)
    {
        Invoke("SetUp", delay);
    }

    // Update is called once per frame
    void Update()
    {
        //When to explode
        //if (collisions > attackPattern.bulletPhysics.maxCollisions) Explode();
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Explode();
        GetComponent<Rigidbody>().AddForce(Vector3.down * gravMult, ForceMode.Impulse);
        if (attackPattern.bulletProperties.guided != 0) GuidedMovement();

      
        if (spriteRend.enabled == false && attackPattern.vFXAndSFX.bulletSprites.Length != 0)
            spriteRend.enabled = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        //Count up collisions
        if (!other.collider.CompareTag("Enemy") && !other.collider.GetComponent<EnemyProjectile>() && !other.collider.CompareTag("Player"))
        {
            collisions++;
            if ((collisions >= attackPattern.bulletPhysics.maxCollisions || attackPattern.vFXAndSFX.impactOnBounce) && other.collider.CompareTag("Surface")) BulletImpact(other);
            else if (collisions >= attackPattern.bulletPhysics.maxCollisions && attackPattern.explosionProperties.explodesOnCollision) Explode();
            else if (collisions >= attackPattern.bulletPhysics.maxCollisions) RemoveBullet();
        }

        //Explode if we hit an enemy and we are supposed to explode
        if (other.collider.CompareTag("Player") && attackPattern.explosionProperties.explosionAttackPattern != null && attackPattern.explosionProperties.explodeOnHit)
        {
            GetComponent<BoxCollider>().enabled = false;
            Explode();
        }
        else if (other.collider.CompareTag("Player"))
        {
            GetComponent<BoxCollider>().enabled = false;
            DirectDamage(other.gameObject.GetComponent<HealthManagement>());
        }

        if (other.collider.gameObject.layer == whatIsViable && attackPattern.explosionProperties.explodeOnHit) Explode();
        else if (other.collider.gameObject.layer == whatIsViable) DirectDamage(other.gameObject.GetComponent<HealthManagement>());
    }

    void SetUp()
    {
        //Create new physics material
        physicsMat = new PhysicMaterial();
        physicsMat.bounciness = attackPattern.bulletPhysics.bounciness;
        physicsMat.frictionCombine = PhysicMaterialCombine.Minimum;
        physicsMat.bounceCombine = PhysicMaterialCombine.Maximum;

        Physics.IgnoreLayerCollision(14, 8);
        Physics.IgnoreLayerCollision(14, 9);
        //Physics.IgnoreLayerCollision(14, 14);
        //Assign physics material to collider
        GetComponent<BoxCollider>().material = physicsMat;
        GetComponent<BoxCollider>().enabled = true;

        //Set gravity
        gravMult = attackPattern.bulletPhysics.gravityMultiplier;

        //Set sprite
        //spriteRend = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (attackPattern.vFXAndSFX.bulletSprites.Length > 0) 
            spriteRend.sprite = attackPattern.vFXAndSFX.bulletSprites[Random.Range(0, attackPattern.vFXAndSFX.bulletSprites.Length - 1)];
            
        else
        {
            spriteRend.GetComponent<SpriteRenderer>().enabled = false;
        }
        //Set some extras
        maxLifetime = attackPattern.bulletProperties.maxLifetime;
        transform.localScale *= attackPattern.bulletProperties.bulletSize;
        player = master.player;
        if (attackPattern.explosionProperties.explosionAttackPattern != null) GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.spreadPattern);
    }

    void Explode()
    {
        StopBullet();
        if (attackPattern.explosionProperties.explosionAttackPattern != null) GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.spreadPattern);

        transform.position = transform.position - transform.forward * 0.1f;
        lastAttackPoint = 3;
        if (attackPattern.explosionProperties.explosionAttackPattern != null) Shoot();

        Invoke("RemoveBullet", 0.01f);
    }
    void DirectDamage(HealthManagement target)
    {
        StopBullet();
        target.TakeDamage(attackPattern.bulletProperties.bulletDamage);
        RemoveBullet();
    }

    void BulletImpact(Collision other)
    {
        if (!attackPattern.vFXAndSFX.impactOnBounce) StopBullet();

        if (attackPattern.vFXAndSFX.impactTexture != null)
        {
            GameObject obj = master.GetComponent<ObjectPooler>().SpawnFromPool("Decal", other.contacts[0].point + new Vector3(0, 0, 0.2f), transform.rotation);
            obj.transform.localScale = attackPattern.vFXAndSFX.impactTextureSize;
            obj.transform.forward = other.contacts[0].normal * -1;

            MeshRenderer rend = obj.GetComponentInChildren<MeshRenderer>();
            Material m = rend.material;
            m.mainTexture = impactTexture;

            rend.transform.Rotate(0, 0, Random.Range(0, 359));
            rend.material = m;
        }
        if (attackPattern.explosionProperties.explodesOnCollision)
        {
            Animator o = Instantiate(simpleAnimation, transform.position, Quaternion.identity).GetComponentInChildren<Animator>();
            o.Play(attackPattern.explosionProperties.explosionAnimation.name);
            Destroy(o.transform.parent.gameObject, attackPattern.explosionProperties.explosionAnimation.length);
            Explode();
        }
        else if (!attackPattern.vFXAndSFX.impactOnBounce) Invoke("RemoveBullet", 0.01f);
        else if (collisions >= attackPattern.bulletPhysics.maxCollisions) Invoke("RemoveBullet", 0.01f);
    }
    void StopBullet()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
    }
    void GuidedMovement()
    {
        if (player != null)
        {
            Vector3 oldVelocity = GetComponent<Rigidbody>().velocity.normalized;
            StopBullet();
            
            transform.LookAt(master.playerBody.transform);
            Vector3 direction = new Vector3(oldVelocity.x, oldVelocity.y, oldVelocity.z).normalized + transform.forward.normalized * attackPattern.bulletProperties.guided / 25 * lockOnRampUp;
            GetComponent<Rigidbody>().velocity = direction * attackPattern.weaponProperties.shootForce;
            lockOnRampUp += attackPattern.bulletProperties.guidedRampUp;
            
        }
    }
    void RemoveBullet()
    {
        master.GetComponent<ObjectPooler>().RemoveFromWorld(gameObject);
    }

    void Shoot()
    {
        CancelInvoke();
        bulletsShot++;

        //set firepoints if multiple
        Transform attackPoint = null;
        Transform[] spreadPoints = GetComponentInChildren<SpreadPatternSetup>().GetComponentsInChildren<Transform>(false);
        lastAttackPoint++;
        if (lastAttackPoint >= spreadPoints.Length) lastAttackPoint = 2;
        attackPoint = spreadPoints[lastAttackPoint];


        //create projectile
        GameObject currentBullet = master.GetComponent<ObjectPooler>().SpawnFromPool("Enemy Projectile", attackPoint.position, Quaternion.identity);

       // Debug.Log(currentBullet.transform.position);
        //calculate random spread
        float x = Random.Range(-attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.spread / 100, attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.spread / 100);
        float y = Random.Range(-attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.spread / 100, attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.spread / 100);


        //apply bullet roation 
        Vector3 directionWithSpread = (attackPoint.transform.forward + new Vector3(x, y, 0)).normalized;

        currentBullet.transform.forward = directionWithSpread;

        //apply any active properties
        currentBullet.GetComponent<EnemyProjectile>().attackPattern = attackPattern.explosionProperties.explosionAttackPattern;
        currentBullet.GetComponent<EnemyProjectile>().master = master;

        //apply forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread * attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(transform.up * attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.upwardForce, ForceMode.Impulse);
        if (attackPattern.explosionProperties.explosionAttackPattern.vFXAndSFX.impactTexture.Length > 0)
        {
            currentBullet.GetComponent<EnemyProjectile>().impactTexture = attackPattern.explosionProperties.explosionAttackPattern.vFXAndSFX.impactTexture[Random.Range(0, attackPattern.explosionProperties.explosionAttackPattern.vFXAndSFX.impactTexture.Length - 1)];
        }
        currentBullet.transform.Rotate(0, 0, Random.Range(-360, 360));

        //Invoke resetShot function

        if (bulletsShot < attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.bulletsPerTap)  //This is for attacks with multiple bullets per attack
        {
            Invoke("Shoot", attackPattern.explosionProperties.explosionAttackPattern.weaponProperties.timeBetweenShots);
        }
    }
}
