using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
{
    // Script for the projectile fired from any weapon.

    public SpriteRenderer spriteRend;
    public GameObject simpleAnimation;

    [Header("Assignables")]
    [HideInInspector]
    public GameObject explosionPrefab;
    [HideInInspector]
    public WeaponScript weaponScript;
    [HideInInspector]
    public WeaponStatus weaponStatus;
    [HideInInspector]
    public int spriteOrderTracker;
    [HideInInspector]
    public Texture impactTexture;

    [Header("Custom Properties")]
    

    public int collisions;
    private int pierces = 0;
    private PhysicMaterial physicsMat;

    private float maxLifetime = 2;
    public PlayerCont playerCont;
    public GameMaster master;

    private float lockOnRampUp;

    private float gravMult;

    public Sprite sprite;

    //
    //This is all related to explosion work
    //
    int bulletsShot, lastAttackPoint;
    bool finishedSettingUp = false;
    //


    // Start is called before the first frame update
    public void OnSpawn(float delay)
    {
        Invoke("SetUp", delay);
    }

    // Update is called once per frame
    void Update()
    {
        //When to explode
        
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Invoke("RemoveBullet", 0.01f);
        if (weaponScript.bulletProperties.lockOn != 0) GuidedMovement();
        GetComponent<Rigidbody>().AddForce(Vector3.down * gravMult, ForceMode.Impulse);


        if (spriteRend.sprite != sprite)
            spriteRend.sprite = sprite;
        if (spriteRend.enabled == false && weaponScript.metaSetup.spritePattern.ToString() != "None")
            spriteRend.enabled = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (finishedSettingUp)
        {
            StopBullet();
            if (!other.collider.CompareTag("Player")) collisions++;
            if (other.collider.CompareTag("Enemy"))
            {
                DirectDamage(other.gameObject.GetComponentInParent<HealthManagement>());
            }
            else if (collisions >= weaponScript.bulletPhysics.maxCollisions) BulletImpact(other);
        }
        
    }

    void SetUp()
    {
        //Create new physics material
        physicsMat = new PhysicMaterial();
        physicsMat.bounciness = weaponScript.bulletPhysics.bounciness;
        physicsMat.frictionCombine = PhysicMaterialCombine.Minimum;
        physicsMat.bounceCombine = PhysicMaterialCombine.Maximum;
        //Assign physics material to collider
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.material = physicsMat;
        Physics.IgnoreLayerCollision(9, 12); //no collision (bullets and bulletsIgnore)
        Physics.IgnoreLayerCollision(9, 10); //no collision (bullets and player)
        Physics.IgnoreLayerCollision(9, 9);  //no collision (bullets and other bullets)

        //Set gravity
        gravMult = weaponScript.bulletPhysics.gravityMultiplier;

        //Set sprite
        if (spriteRend == null) spriteRend = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (weaponScript.metaSetup.bulletSprites.Length > 0 && weaponScript.metaSetup.spritePattern.ToString() == "Random")
        {
            sprite = weaponScript.metaSetup.bulletSprites[Random.Range(0, weaponScript.metaSetup.bulletSprites.Length - 1)];
        }
        else if (weaponScript.metaSetup.bulletSprites.Length > 0 && weaponScript.metaSetup.spritePattern.ToString() == "InputOrder")
        {
            sprite = weaponScript.metaSetup.bulletSprites[spriteOrderTracker];
        }
        else if (weaponScript.metaSetup.bulletSprites.Length <= 0)
        {
            spriteRend.GetComponent<SpriteRenderer>().enabled = false;
        }
        //Set some extras
        maxLifetime = weaponScript.bulletProperties.maxLifetime;
        transform.localScale = weaponScript.bulletProperties.bulletSize;
        if (weaponScript.bulletProperties.originatesFrom.ToString() == "Projectile") GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(weaponScript.weaponProperties.spreadPattern);
        else if (weaponScript.weaponProperties.altFireWeaponScript && weaponScript.weaponProperties.altFireWeaponScript.bulletProperties.originatesFrom.ToString() == "Projectile") GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(weaponScript.weaponProperties.altFireWeaponScript.weaponProperties.spreadPattern);
        if (weaponScript.explosionProperties.explosionWeaponScript != null) GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.spreadPattern);
        finishedSettingUp = true;

        AudioSource source = GetComponent<AudioSource>();

        if (weaponScript.metaSetup.bulletSFX.clip != null)
        {
            source.enabled = true;
            Sound s = weaponScript.metaSetup.bulletSFX;
            source.clip = s.clip;
            source.outputAudioMixerGroup = s.mixer;
            source.volume = s.volume;
            source.pitch = s.pitch * Random.Range(0.9f, 1.1f);
            source.loop = s.loops;
            source.bypassReverbZones = !s.reverb;

            source.PlayOneShot(source.clip);
        }
        else
        {
            source.enabled = false;
        }
    }

    void Explode()
    {
        StopBullet();
        Animator o = Instantiate(simpleAnimation, transform.position, Quaternion.identity).GetComponentInChildren<Animator>();
        o.Play(weaponScript.explosionProperties.explosionAnimation.name);
        if (weaponScript.explosionProperties.explosionSFX != null)
        {
            AudioSource source = o.GetComponent<AudioSource>();
            Sound s = weaponScript.explosionProperties.explosionSFX;
            source.clip = s.clip;
            source.outputAudioMixerGroup = s.mixer;
            source.volume = s.volume;
            source.pitch = s.pitch * Random.Range(0.9f, 1.1f);
            source.loop = s.loops;
            source.bypassReverbZones = !s.reverb;

            source.PlayOneShot(source.clip);
        }

        Destroy(o.transform.parent.gameObject, weaponScript.explosionProperties.explosionSFX.clip.length + Mathf.Abs(weaponScript.explosionProperties.explosionSFX.clip.length - weaponScript.explosionProperties.explosionAnimation.length));
        spriteRend.enabled = false;
        if (weaponScript.explosionProperties.explosionWeaponScript != null) GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.spreadPattern);

        lastAttackPoint = 3;
        if (weaponScript.explosionProperties.explosionWeaponScript != null) Shoot();

        //run the explosion code

        //pierces++;
        //if (pierces >= weaponScript.bulletProperties.pierce) Invoke("RemoveBullet", 0.02f);
    }
    void DirectDamage(HealthManagement target)
    {
        StopBullet();
        target.TakeDamage(weaponScript.bulletProperties.bulletDamage);
        if (weaponScript.weaponProperties.weaponStatus != null)
        {
            target.GetComponent<EnemyStatus>().ApplyNewStatus(weaponStatus, this);
        }
        if (weaponScript.explosionProperties.explodeOnHit)
        {
            Explode();
        }
        else
            Invoke("RemoveBullet", 0.01f);
    }

    void BulletImpact(Collision other)
    {
        StopBullet();
        transform.up = -other.contacts[0].normal;
        transform.position = transform.position - transform.up * 0.15f;
        if (other.collider.CompareTag("Surface") && weaponScript.metaSetup.impactSprite != null)
        {
        
            GameObject obj = master.GetComponent<ObjectPooler>().SpawnFromPool("Decal", other.contacts[0].point + (other.contacts[0].normal * 0.02f), transform.rotation);
            obj.transform.localScale = weaponScript.metaSetup.impactSpriteSize;
            obj.transform.forward = other.contacts[0].normal * -1;
       
            MeshRenderer rend = obj.GetComponentInChildren<MeshRenderer>();
            Material m = rend.material;
            m.mainTexture = impactTexture;
        
            rend.transform.Rotate(0, 0, Random.Range(0, 359));
            rend.material = m;
  
        }
        if (weaponScript.explosionProperties.explodesOnCollisionNum != 0)
        {
            
            Explode();
        }
        else
            Invoke("RemoveBullet", 0.01f);

    }

    void RemoveBullet()
    {
        StopBullet();
        maxLifetime = 3f;
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;
        master.GetComponent<ObjectPooler>().RemoveFromWorld(gameObject);
    }
    void GuidedMovement()
    {
        float bestDistance = 9999;
        GameObject bestCollider = null;

        Collider[] enemies = Physics.OverlapSphere(transform.position, 200, weaponScript.bulletProperties.whatIsEnemy);
        for (int i = 0; i < enemies.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, enemies[i].transform.position);

            if (distance < bestDistance)
            {
                bestCollider = enemies[i].gameObject;
                bestDistance = distance;
            }
        }
        if (bestCollider != null)
        {
            Vector3 oldVelocity = GetComponent<Rigidbody>().velocity.normalized;
            StopBullet();
            transform.LookAt(bestCollider.transform);
            Vector3 direction = new Vector3(oldVelocity.x, oldVelocity.y, oldVelocity.z).normalized + transform.forward.normalized * weaponScript.bulletProperties.lockOn /25 * lockOnRampUp;
            GetComponent<Rigidbody>().velocity = direction * weaponScript.weaponProperties.shootForce;
            lockOnRampUp += weaponScript.bulletProperties.lockOnRampUp;
        }  
    }
    void StopBullet()
    {
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
    }

    void Shoot()
    {
        bulletsShot++;

        //set firepoints if multiple
        Transform attackPoint = null;
        Transform[] spreadPoints = GetComponentInChildren<SpreadPatternSetup>().GetComponentsInChildren<Transform>(false);
        lastAttackPoint++;
        if (lastAttackPoint >= spreadPoints.Length) lastAttackPoint = 3;
        attackPoint = spreadPoints[lastAttackPoint];


        //create projectile
        GameObject currentBullet = playerCont.objPooler.SpawnFromPool("Player Projectile", attackPoint.position, Quaternion.identity);
        //Debug.Log(currentBullet.transform.position);


        //calculate random spread
        float x = Random.Range(-weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.spread / 100, weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.spread / 100);
        float y = Random.Range(-weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.spread / 100, weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.spread / 100);


        //apply bullet roation 
        Vector3 directionWithSpread = (attackPoint.transform.forward + new Vector3(x, y, 0)).normalized;

        currentBullet.transform.forward = directionWithSpread;

        //apply any active properties
        currentBullet.GetComponent<Projectile>().weaponScript = weaponScript.explosionProperties.explosionWeaponScript;
        currentBullet.GetComponent<Projectile>().master = master;

        //apply forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread * weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(transform.up * weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.upwardForce, ForceMode.Impulse);
        if (weaponScript.explosionProperties.explosionWeaponScript.metaSetup.impactSprite.Length > 0)
        {
            currentBullet.GetComponent<Projectile>().impactTexture = weaponScript.explosionProperties.explosionWeaponScript.metaSetup.impactSprite[Random.Range(0, weaponScript.explosionProperties.explosionWeaponScript.metaSetup.impactSprite.Length - 1)];
        }
        currentBullet.transform.Rotate(0, 0, Random.Range(-360, 360));
        //currentBullet.GetComponent<Projectile>().SetUp();

        //Invoke resetShot function
       
       if (bulletsShot < weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.bulletsPerTap)  //This is for attacks with multiple bullets per attack
       {
            Invoke("Shoot", weaponScript.explosionProperties.explosionWeaponScript.weaponProperties.timeBetweenShots);
       } else
       {
           Invoke("RemoveBullet", 0.01f);
       }
    }
}
