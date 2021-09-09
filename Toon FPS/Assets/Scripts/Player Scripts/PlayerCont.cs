using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using UnityEngine.AI;

public class PlayerCont : MonoBehaviour
{
    [Header("Constants")]
    public GameMaster master; //the game master, obv.
    PlayerMovement pl;
    [HideInInspector]
    public KeyBinds binds;  //the game master's keybind script
    //public GameObject projectile; //This is the base prefab for a projectile
    public Animator universalAnim;     //the animator for the hands/weapon
    public Camera fpsCam;   //referace to Camera 
    public CharacterController playerCharacter;
    public HUDManager hudManager;
    public RuntimeAnimatorController miscAnimCont;
    public ObjectPooler objPooler;

    public bool inputLocked = false;

    //debug
    public bool allowInvoke;
    public bool allowRInvoke = false;
    [HideInInspector]
    public WeaponScript weaponScript;     //the weaponscript of the weapon we are currently using

    [Header("Player Stats")]
    public float maxHealth;       //max health of the player
    public float standardHealth;  //The health not considered "bonus health" that get healed by health packs

    //bullets left refers to the bullets remaining in the mag after a trigger pull
    //bullets shot keeps track of how many bullets we've shot in a single trigger pull (for burst weapons)
    [HideInInspector]
    public int bulletsLeft, bulletsShot, bulletSpriteTracker;

    //all of these are states in which the player can't shoot 
    [HideInInspector]
    public bool shooting, a_shooting, readyToShoot, reloading, a_readyToShoot, paused, switching;
    
    //rotates the projectile somewhere between - and + 360, for flavor
    [HideInInspector]
    public float randomZRotation = 360;

    //allows us to iterate through the attackpoints
    int lastAttackPoint;

    //allows us to relese a charged shot when nessicary (0 = not charging, 1 = charging, 2 = charged, -1 = uncharged)
    int chargeState = 0;

    private void Awake()
    {
        //set up master stuff
        master = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        binds = master.GetComponent<KeyBinds>();
        objPooler = master.GetComponent<ObjectPooler>();
    }
    void Start()
    { 
        //set everything back to normal upon startup
        bulletsLeft = weaponScript.weaponProperties.magSize;
        readyToShoot = true;
        a_readyToShoot = true;
        master.player = this;

        playerCharacter.GetComponent<HealthManagement>().maxHealth = maxHealth;

        //this helps setup the starting weapon
        SwitchWeapons();
    }

    void Update()
    {
        //runs the check for player actions
        if (!inputLocked) MyInput();

        if (chargeState == 2)
        {
            DoOriginCheckAndShoot(weaponScript);
            chargeState = 0;
        }
    }

    private void MyInput()
    {
        //check if the game is paused
        if (master.GetComponent<MenuManager>().gameIsPaused || master.GetComponent<MenuManager>().insideTabMenu)
        {
            paused = true;
        } else
        {
            paused = false;
        }

        //Check if allowed to hold trigger down
        if (weaponScript.weaponProperties.allowTiggerHold) shooting = binds.KeyAtBind("Fire", true);
        else shooting = binds.KeyAtBind("Fire", false);

        //Do the same for our alternate fire
        if (weaponScript.weaponProperties.altFireWeaponScript != null && weaponScript.weaponProperties.altFireWeaponScript.weaponProperties.allowTiggerHold) a_shooting = binds.KeyAtBind("AltFire", true);
        else if (weaponScript.weaponProperties.altFireWeaponScript != null) a_shooting = binds.KeyAtBind("AltFire", false);

        //check if we release the charge on a hold-to-charge weapon 
        if (weaponScript.weaponProperties.allowTiggerHold && !shooting && chargeState == 1)
        {
            ResetAllInvokes();
            Invoke("UnchargeShot", weaponScript.weaponProperties.unchargeTime);
        }
        if (weaponScript.weaponProperties.altFireWeaponScript != null && weaponScript.weaponProperties.altFireWeaponScript.weaponProperties.allowTiggerHold && !a_shooting && chargeState == 1)
        {
            ResetAllInvokes();
            Invoke("UnchargeShot", weaponScript.weaponProperties.altFireWeaponScript.weaponProperties.unchargeTime);
        }

        //Reload
        if (!reloading && (bulletsLeft < weaponScript.weaponProperties.magSize) && binds.KeyAtBind("Reload", false) && readyToShoot) Reload();
        if (!reloading && (bulletsLeft <= 0 && weaponScript.weaponProperties.magSize != 0) && (readyToShoot && a_readyToShoot)) Reload();

        //shooting
        if (((readyToShoot && shooting) || (a_readyToShoot && a_shooting)) && !reloading && (bulletsLeft > 0 || weaponScript.weaponProperties.magSize == 0) && !paused && !switching)
        {
            bulletsShot = 0;
            if (a_shooting) ChargeShot(weaponScript.weaponProperties.altFireWeaponScript, true);       
            else ChargeShot(weaponScript, false);
        }

        //Weapon swaping
        WeaponManager weaponManager = master.GetComponent<WeaponManager>();
        if (!shooting && binds.KeyAtBind("NextWeapon", true) && !paused)
        {
            weaponManager.activeWeapon--;
            if (weaponManager.activeWeapon < 0) weaponManager.activeWeapon = weaponManager.weapons.Length - 1;  
        } 
        if (!shooting && binds.KeyAtBind("LastWeapon", true) && !paused)
        {                                                                                   
            weaponManager.activeWeapon++;
            if (weaponManager.activeWeapon > weaponManager.weapons.Length - 1) weaponManager.activeWeapon = 0;
        }
        //Weapon Swaping but more
        if (!shooting && binds.KeyAtBind("Weapon1", true) && !paused && weaponManager.weapons.Length >= 1) weaponManager.activeWeapon = 0;
        if (!shooting && binds.KeyAtBind("Weapon2", true) && !paused && weaponManager.weapons.Length >= 2) weaponManager.activeWeapon = 1;
        if (!shooting && binds.KeyAtBind("Weapon3", true) && !paused && weaponManager.weapons.Length >= 3) weaponManager.activeWeapon = 2;
        if (!shooting && binds.KeyAtBind("Weapon4", true) && !paused && weaponManager.weapons.Length >= 4) weaponManager.activeWeapon = 3;
        if (!shooting && binds.KeyAtBind("Weapon5", true) && !paused && weaponManager.weapons.Length >= 5) weaponManager.activeWeapon = 4;
    }

    private void ChargeShot(WeaponScript weapon, bool isAlt)
    {
        //play any charging animations

        if (isAlt) a_readyToShoot = false;
        else readyToShoot = false;
        chargeState = 1;
        Invoke("FinishChargeShot", weapon.weaponProperties.chargeTime); 
    }

    private void FinishChargeShot()
    {
        chargeState += 1;
    }

    private void UnchargeShot()
    {  
        a_readyToShoot = true;
        readyToShoot = true;
        chargeState = 0;
    }
  
    public void DoOriginCheckAndShoot(WeaponScript w)
    {
        chargeState = 0;

        //Check where this shot should trigger from
        fpsCam.GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(w.weaponProperties.spreadPattern);
        lastAttackPoint = 2;

        if (w.bulletProperties.originatesFrom.ToString() == "Character" && !reloading) Shoot(w, fpsCam.transform);
        else if (w.bulletProperties.originatesFrom.ToString() == "Projectile")
        {
            GameObject[] g = GameObject.FindGameObjectsWithTag("Projectile");
            foreach (GameObject p in g)
            {
                if (p.GetComponent<Projectile>().weaponScript == weaponScript) Shoot(w, p.transform);
            }
        }
    }
    public void Shoot(WeaponScript w, Transform shotOrigin)
    {
        //Set the master ammo count for this weapon to the bullets left / the weapons mag size
        if (weaponScript.weaponProperties.magSize != 0) master.GetComponent<WeaponManager>().weaponAmmoCounts[master.GetComponent<WeaponManager>().activeWeapon] = (float)bulletsLeft / (float)weaponScript.weaponProperties.magSize;

        //clean up shooting
        if (w == weaponScript) readyToShoot = false;
        else a_readyToShoot = false;

        //cleanup for alt fire
        if ((w == weaponScript.weaponProperties.altFireWeaponScript && weaponScript.weaponProperties.altFireWeaponScript && weaponScript.weaponProperties.altFireWeaponScript.weaponProperties.consumeAmmo) || (w == weaponScript && weaponScript.weaponProperties.consumeAmmo)) bulletsLeft--;
        bulletsShot++;

        //set firepoints if multiple
        Transform attackPoint = null;
        Transform[] spreadPoints = shotOrigin.gameObject.GetComponentInChildren<SpreadPatternSetup>().GetComponentsInChildren<Transform>(false);
        if (lastAttackPoint >= spreadPoints.Length || lastAttackPoint < 2) lastAttackPoint = 2;
        attackPoint = spreadPoints[lastAttackPoint];
        lastAttackPoint++;

        //create projectile
        GameObject currentBullet = objPooler.SpawnFromPool("Player Projectile", attackPoint.position, attackPoint.rotation);

        //calculate random spread
        float x = Random.Range(-w.weaponProperties.spread / 100, w.weaponProperties.spread / 100);
        float y = Random.Range(-w.weaponProperties.spread / 100, w.weaponProperties.spread / 100);

        //apply bullet roation 
        Vector3 directionWithSpread = (attackPoint.transform.forward + new Vector3(x, y, 0)).normalized;
        currentBullet.transform.forward = directionWithSpread;

        //give the projectile our weapon information and some other information
        Projectile currentBulletProjectile = currentBullet.GetComponent<Projectile>();
        currentBulletProjectile.weaponScript = w;
        currentBulletProjectile.weaponStatus = w.weaponProperties.weaponStatus;
        currentBulletProjectile.master = master;
        currentBulletProjectile.playerCont = this;
        //currentBulletProjectile.OnSpawn();

        //This allows us to use the INPUT ORDER option for the bullets sprite by cycling the index of the sprites array
        if (bulletSpriteTracker >= weaponScript.metaSetup.bulletSprites.Length-1) bulletSpriteTracker = 0;
        else bulletSpriteTracker++;
        currentBulletProjectile.spriteOrderTracker = bulletSpriteTracker;

        if (weaponScript.metaSetup.impactSprite.Length > 0) 
        {
            currentBulletProjectile.impactTexture = weaponScript.metaSetup.impactSprite[Random.Range(0, weaponScript.metaSetup.impactSprite.Length - 1)];
        }

        //apply forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread * w.weaponProperties.shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * w.weaponProperties.upwardForce, ForceMode.Impulse);
        currentBullet.transform.Rotate(0, 0, Random.Range(-randomZRotation, randomZRotation));

        chargeState = 0;
        //Invoke resetShot function
        if (allowInvoke)
        {
            //check if this is a burst weapon and we need to shoot some more projectiles
            if (bulletsShot < w.weaponProperties.bulletsPerTap && bulletsLeft > 0)
            {
                Wait(w.weaponProperties.timeBetweenShots);
                Shoot(w, shotOrigin);
            } else                  //this is the end of this particular trigger pull
            {
                //play the shoot animation and sound effect
                universalAnim.SetTrigger("ShootTrig");
                //master.GetComponent<AudioManager>().Play(w.metaSetup.shootSFX);
                CameraShaker.Instance.ShakeOnce(w.metaSetup.camShakeMagnitude, w.metaSetup.camShakeRoughness, 
                    w.metaSetup.camShakeFadeInTime, w.metaSetup.camShakeFadeOutTime);

                //removes our ability to shoot until after the weapon's specific delay
                if (w == weaponScript) Invoke("ResetShot", w.weaponProperties.timeBetweenShooting);
                else Invoke("ResetAltShot", w.weaponProperties.timeBetweenShooting);
            }
        }
       
    }
    private void ResetShot()
    {
        //reseting so we can run the code again
        readyToShoot = true;
        allowInvoke = true;
    }
    private void ResetAltShot()
    {
        //reseting the alternate fire so we can run the code again
        a_readyToShoot = true;
        allowInvoke = true;
    }

    public void Reload()
    {
        // starts the reload animation and prevents shooting
        if (allowRInvoke == true)
        {
            reloading = true;
            
            universalAnim.SetTrigger("ReloadTrig");
            Invoke("ReloadFinished", weaponScript.weaponProperties.reloadTime);
            
            allowRInvoke = false;

        }
    }
    
    private void ReloadFinished()
    {
        bulletsLeft = weaponScript.weaponProperties.magSize;
       
        //allows shooting again and actually reloads the mag
        reloading = false;
        allowRInvoke = true;
    }
    private void SwitchFinished()
    {
        //allows us to shoot again
        switching = false;
        allowRInvoke = true;
    }

    public void SwitchWeapons()
    {
        //disables the shooting while we switch weapons
        switching = true;

        //set up the spread pattern for the new gun
        if (weaponScript.weaponProperties.spreadPattern != null)
        {
            fpsCam.GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(weaponScript.weaponProperties.spreadPattern);
            hudManager.hudElements.crossHair.sprite = weaponScript.metaSetup.crosshairSprite;
            hudManager.hudElements.crossHair.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1) * weaponScript.metaSetup.crosshairSizeScale;
        }
        lastAttackPoint = 2;

        //plays a blank sound effect to cancel any weapon sound effects (like reloading)
        master.GetComponent<AudioManager>().WeaponSwapCancel();

        universalAnim.runtimeAnimatorController = weaponScript.metaSetup.anim;
        //universalAnim.SetTrigger("SwapTrig");

        //delay for the switch time before allowing us to shoot again
        Invoke("SwitchFinished", weaponScript.weaponProperties.switchToTime);
        chargeState = 0;
    }

    //allows us to delay the shoot function without Invoke
    private IEnumerable Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
    }

    public void MantleAnim(PlayerMovement p)
    {
        pl = p;
        universalAnim.runtimeAnimatorController = miscAnimCont;
        ResetAllInvokes();
        Invoke("MantleReset", miscAnimCont.animationClips[0].length);        
    }
    public void MantleReset()
    {
        pl.isMantleing = false;
        SwitchWeapons();
    }

    public void ResetAllInvokes()
    {
        CancelInvoke();
        reloading = false;
        switching = false;
        readyToShoot = true;
        a_readyToShoot = true;
        if (pl != null) pl.isMantleing = false;
        chargeState = 0;
    }
}
