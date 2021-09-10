using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    [Header("Weapon Meta Setup")]
    //the blank projectile prefab
    public GameObject projectile;

    //a bit of magic
    public bool allowInvoke = true;

    //what specific type of enemy this is
    public Enemy enemy;

    //the player
    [HideInInspector]
    public Transform player;

    //allows the pathfinding code to check that its not trying to move where no ground exists, or onto the player 
    public LayerMask whatIsGround, whatIsPlayer;

    //the enemy graphic, or artwork
    public GameObject graphic;

    //the sprite mask object
    public SpriteMask graphicSpriteMask;

    //the sprite mask animator
    public Animator spriteMaskAnimator;

    //the container for the graphic (also functions as hitbox)
    public Transform graphicHolder;

    [Header("Navigation AI Variables")]

    //stores the location our AI is activly trying to walk to
    private Vector3 walkPoint;

    //whether or not the above location exists
    bool walkPointSet;

    //A countdown value to make the character reroll the walkpoint if it doesnt reach it in enough time
    float walkPointCountdown = 5;

    //disallows us to attack while we are running attack code
    bool alreadyAttacked;

    //allows us to iterate through attack points
    int lastAttackPoint;

    //the number of bullets fired during this specific "trigger pull"
    int bulletsShot;

    //the attackpattern we are currently attacking with
    [HideInInspector]
    AttackPatterns attackPattern;

    //the attackpattern we run when we gib to splatter the blood and such
    public AttackPatterns gibPattern;

    //the attackpattern we run when we die to give the player their health bonus;
    public AttackPatterns healthBonusAttackPattern;

    //what we are shooting at
    Transform target;

    //weather or not we are agrod
    bool asleep;

    //counts down the delay between attacks
    float attackCountdown;

    //changes the enemy agro distance, meaning it will stay asleep until the player gets closer
    bool isAmbushEnemy = false;

    //stores what animation we should play this frame
    [HideInInspector]
    public AnimationSet toPlay;

    //whether or not we are dead
    [HideInInspector]
    public bool isDead;

    //whether or not we are playing our death animation
    [HideInInspector]
    public bool isDying;

    public float gravity;
    public GameMaster master;
    private ObjectPooler objectPooler;

    private bool smoothAnimationTrans = false;

    private bool attacking;

    //Empty object to hold the spread pattern and rotates to face player
    public Transform firePointHolder;

    public CharacterController cont;

    private Vector3 gravVel, addToMove, lastFrameMoveVector;

    float lerpTime = 300;
    float lerpValue;

    float flyingEnemyY;

    public void Start()
    {
        //hook up some of the important components
        master = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        player = master.playerBody;
        graphic.GetComponent<Animator>().runtimeAnimatorController = enemy.animCont;
        objectPooler = master.GetComponent<ObjectPooler>();
        GetComponent<HealthManagement>().maxHealth = enemy.maxHealth;
        graphicHolder.GetComponent<BoxCollider>().size = new Vector3(enemy.hitboxSize.x, enemy.hitboxSize.y, 1);
        graphicHolder.GetComponent<BoxCollider>().center = new Vector3(enemy.hitboxCenter.x, enemy.hitboxCenter.y, 0);
    }
    public void Update()
    {
        graphicSpriteMask.sprite = graphic.GetComponent<SpriteRenderer>().sprite;
        //graphicSpriteMask.transform.localScale = graphic.transform.localScale;
        //graphicSpriteMask.transform.localPosition = graphic.transform.localPosition;
        //graphicSpriteMask.transform.localRotation = new Quaternion(0, 0, graphic.transform.localRotation.z, 0);

        if (player == null && master.playerBody != null) player = master.playerBody;
      
        //if we aren't dead and we dont have a target
        if (target == null && !isDead && !isDying)
        {
            //check for target. If we find one, wake up
            if (player != null) CheckForPlayer();
            if (target == null) asleep = true;
            else asleep = false;

        }

        //if we are asleep, play some idle animation
        if (asleep && !isDead && !isDying)
        {
            toPlay = enemy.animations[0]; //Zero should be the idex of the 0 degree idle animation
        }

        //we are trying to attack, check if our countdown is done
        if (!asleep && !alreadyAttacked && !isDead && !isDying)
        {
            alreadyAttacked = CheckForAttack(1f);
        }

        //if we can, do some movement
        if (!asleep && !alreadyAttacked && !attacking && !isDead && !isDying)
        {
            if (Vector3.Distance(transform.position, target.position) <= enemy.chaseRange && Vector3.Distance(transform.position, target.position) > 0.5f) RunChase();
            else RunStrafe();
            
        }

        //ey, it's time to be dead
        if (isDead)
        {
        }

        //Displays the correct sprite 
        DisplaySprite(ManageSprites());
    }

    public void FixedUpdate()
    {
        if (walkPointSet && !isDead && !isDead)
        {
            Move(walkPoint);
        }

    }

    public void OnCollisionEnter(Collision collision)
    {
        Move(collision.contacts[0].normal * 0.1f);
        if (collision.collider.CompareTag("Surface")) SearchWalkPoint();
    }

    public void AddVelocity(Vector3 force)
    {
        addToMove += force;
    }
    public void CheckForPlayer()
    {
        //gets the LOS range 
        float range = enemy.LOSSightRange;
        if (isAmbushEnemy) range /= 4;

        //shoots a ray to check if the player is in LOS and in range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, player.position - transform.position, out hit, range))
        {
            //if its the player, agro and set the player as target
            if (hit.transform.gameObject.CompareTag("Player") && target == null)
            {
                asleep = false;
                target = hit.transform;            
            }
        }
    }

    public bool CheckForAttack(float countdownMult)
    {
        //check if our countdown since last attack is done
        if (Mathf.Round(attackCountdown * countdownMult) <= 0)
        {
            //runs through every attackpattern to see if the player is within that attack's range
            foreach (AttackPatterns a in enemy.attackPatterns)
            {
                //if it is, lets run that attacks
                if (Vector3.Distance(target.position, transform.position) <= a.weaponProperties.range)
                {
                    AttackPlayer(a);
                    attackCountdown = Random.Range(enemy.minAttackCountdown, enemy.maxAttackCountdown);
                    return true;
                }
            }
            //on the off chance no attacks are in range, keep counting down
            attackCountdown -= Time.deltaTime;
            return false;
        }
        else
        {
            //if we are checking and the countdown is not finished yet, keep counting down
            attackCountdown -= Time.deltaTime;
            return false;
        }
    }

    public void AttackPlayer(AttackPatterns attack)
    {
        attacking = true;

        smoothAnimationTrans = false;
        //Sets up the firepoints in accordance to the spreadPattern in attack
        GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(attack.weaponProperties.spreadPattern);

        //Stop the enemy movement
        transform.LookAt(target);
            //set destination
        toPlay = attack.vFXAndSFX.dirAnimations;

        //Setup and run the shoot function
        bulletsShot = 0;
        attackPattern = attack;
        Invoke("Shoot", attack.vFXAndSFX.startingLag);
        
    }

    public void RunStrafe()
    {
        //gets a walk point in the event we dont have one
        if (!walkPointSet || walkPointCountdown <= 0)
        {
            SearchWalkPoint();
            walkPointCountdown = Random.Range(0.5f, 5);
        }
        //sets the agent desitation if we do have one
        if (walkPointSet)
        {
            walkPointCountdown -= Time.deltaTime;
            if (Physics.Raycast(transform.position, walkPoint, 0.8f))
            {
                if (Random.Range(0, 10) == 0) SearchWalkPoint();
            }
        }
        //Get the distance to the walk point
        //Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reached
        //if (distanceToWalkPoint.magnitude < 1.7f) walkPointSet = false;
        smoothAnimationTrans = true;
        toPlay = enemy.animations[0];
    }
    public void RunChase()
    {
        smoothAnimationTrans = true;
        //if the target is close enough, set them as the walkpoint
        walkPoint = player.transform.position - transform.position;
        walkPointSet = true;
        
        
        toPlay = enemy.animations[0];
        CheckForAttack(0.5f);
    }
    public void ResetAttack()
    {
        //allows us to try attacking again
        alreadyAttacked = false;
        attacking = false;
    }

    private void SearchWalkPoint()
    {

        float randomZ = Random.Range(-enemy.walkPointRange, enemy.walkPointRange);
        float randomX = Random.Range(-enemy.walkPointRange, enemy.walkPointRange);

        if (enemy.flying)
        {
            RaycastHit heightCheck;
            Physics.Raycast(transform.position, Vector3.down, out heightCheck, whatIsGround);
            Debug.Log(Vector3.Distance(transform.position, heightCheck.transform.position));
            if (Vector3.Distance(transform.position, heightCheck.transform.position) < 5)
            {
                flyingEnemyY = Random.Range(0, enemy.walkPointRange / 2);
            }
            else if (Vector3.Distance(transform.position, heightCheck.transform.position) > 15)
            {
                flyingEnemyY = Random.Range(-enemy.walkPointRange / 2, 0);
            } else flyingEnemyY = Random.Range(-enemy.walkPointRange / 2, enemy.walkPointRange / 2);
        }
        else flyingEnemyY = 0;

        RaycastHit hit;
        Physics.Raycast(transform.position, new Vector3(randomX, 0, randomZ), out hit, 1f);
        RaycastHit hitTwo;
        Physics.Raycast(new Vector3(randomX, 0, randomZ).normalized, Vector3.down, out hitTwo, 1.2f);
        Debug.DrawLine(transform.position, transform.position + new Vector3(randomX, 0, randomZ), Color.red, 5f);
   
        walkPoint = new Vector3(randomX, 0, randomZ);
        walkPointSet = true;
    
       
    }
    public void Move(Vector3 moveAdd)
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f, whatIsGround);
        Vector3 moveVector = new Vector3(0, 0, 0);
        if (isGrounded || enemy.flying)
        {
            gravVel = new Vector3(0, 0, 0);
            if (isDead) moveVector = new Vector3(0, 0, 0);
            else moveVector += moveAdd.normalized * GetComponent<EnemyStatus>().speed;
        }
        else
        {
            gravVel += Vector3.down * gravity * Time.deltaTime;
        }
        if (addToMove != new Vector3(0, 0, 0))
        {
            addToMove = Vector3.Lerp(addToMove, new Vector3(0, 0, 0), 0.5f);
        }
        if (enemy.flying)
        {
            moveVector += new Vector3(0, flyingEnemyY, 0);
        } else
        {
            moveVector = new Vector3(moveVector.x, 0, moveVector.z);
        }
        if (moveVector != lastFrameMoveVector) lerpValue = 12;

        if (lerpValue - lerpTime * Time.deltaTime >= 0) lerpValue = lerpValue - lerpTime * Time.deltaTime;
        else lerpValue = 0;

        moveVector = Vector3.Lerp(moveVector, lastFrameMoveVector, lerpValue / 12);
        lastFrameMoveVector = moveVector;
        transform.LookAt(transform.position + new Vector3(moveVector.x, 0, moveVector.z).normalized);

        cont.Move(moveVector * Time.deltaTime);
        cont.Move(addToMove * 10 * Time.deltaTime);
        cont.Move(gravVel * Time.deltaTime);

        Debug.DrawRay(transform.position, moveVector, Color.blue, 0.1f);

    }
    public void Die()
    {
        smoothAnimationTrans = false;
        graphicHolder.GetComponent<BoxCollider>().enabled = false;
        //self explanitory
        isDying = true;

        AttackPatterns specialGib = Instantiate(gibPattern);

        for (int i = 0; i < specialGib.vFXAndSFX.dirAnimations.clips.Length; i++)
        {
            specialGib.vFXAndSFX.dirAnimations.clips[i] = enemy.deathAnimation; 
        }
        for (int i = 0; i < enemy.specialGipSprites.Length; i++)
        {
            specialGib.vFXAndSFX.bulletSprites[i] = enemy.specialGipSprites[i];
        }
        AttackPlayer(specialGib);
        master.playerBody.GetComponent<HealthManagement>().StartHealthLossDelay();
        Invoke("FinishDeath", enemy.deathAnimation.length);
    }
    private void FinishDeath()
    {
        AttackPlayer(healthBonusAttackPattern);

        isDying = false;
        isDead = true;
        master.activeChest.score++;
    }

    private void Shoot()
    {
        alreadyAttacked = true;

        bulletsShot++;

        firePointHolder.LookAt(target);

        //set firepoints if multiple
        Transform attackPoint = null;
        Transform[] spreadPoints = GetComponentInChildren<SpreadPatternSetup>().GetComponentsInChildren<Transform>(false);
        lastAttackPoint++;
        if (lastAttackPoint >= spreadPoints.Length) lastAttackPoint = 2;
        attackPoint = spreadPoints[lastAttackPoint];


        //create projectile
        GameObject currentBullet = objectPooler.SpawnFromPool("Enemy Projectile", attackPoint.position, Quaternion.identity);

        //calculate random spread
        float x = Random.Range(-attackPattern.weaponProperties.spread / 100, attackPattern.weaponProperties.spread / 100);
        float y = Random.Range(-attackPattern.weaponProperties.spread / 100, attackPattern.weaponProperties.spread / 100);


        //apply bullet roation 
        Vector3 directionWithSpread = (attackPoint.transform.forward + new Vector3(x, y, 0)).normalized;

        currentBullet.transform.forward = directionWithSpread;

        //apply any active properties
        currentBullet.GetComponent<EnemyProjectile>().attackPattern = attackPattern;
        currentBullet.GetComponent<EnemyProjectile>().master = master;

        //apply forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread * attackPattern.weaponProperties.shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(transform.up * attackPattern.weaponProperties.upwardForce, ForceMode.Impulse);
        if (attackPattern.vFXAndSFX.impactTexture.Length > 0)
        {
            currentBullet.GetComponent<EnemyProjectile>().impactTexture = attackPattern.vFXAndSFX.impactTexture[Random.Range(0, attackPattern.vFXAndSFX.impactTexture.Length - 1)];
        }
        currentBullet.transform.Rotate(0, 0, Random.Range(-360, 360));

        //Invoke resetShot function
        if (allowInvoke)
        {
            if (bulletsShot < attackPattern.weaponProperties.bulletsPerTap)  //This is for attacks with multiple bullets per attack
            {
                Invoke("Shoot", attackPattern.weaponProperties.timeBetweenShots);
            }
            else         //Finishes the attack
            {
                    //set destination
                //TO DO: trigger animation
                Invoke("ResetAttack", attackPattern.vFXAndSFX.endingLag);
            }
        }
    }

    private int ManageSprites()
    {
        //rotate the graphic holder so that it faces the player's camera
        graphicHolder.LookAt(GameObject.FindGameObjectWithTag("MainCamera").transform);

        //this is getting the angle in degrees that the enemy is facing in referance to the player, so we can display the sprite that approximates that angle
        float camAngle = (360 + Vector3.SignedAngle(graphicHolder.transform.forward, transform.forward, transform.up)) % 360;

        //figure out which directional animation approximates the camangle
        for (int i = 0; i < 8; i++)
        {  
            if (Mathf.Round(camAngle/45) == i)
            {
                //return the offset for our chosen directional animation
                return i;        
            }
        }
        return 0;
    }

    private void DisplaySprite(int dirOffset)
    {
        //set up the animator
        Animator animator = graphic.GetComponent<Animator>();

        //we are taking the list of animation and finding the one we want to play, before adding and offset to the index based on the direction ManageSprites gives us
        AnimationClip animToPlay = toPlay.clips[dirOffset];

        //figures out the exact time where we are switching animations, allowing for a smooth transition
        float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;

        if (animToPlay != null)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animToPlay.name))
            {
                //if (isDead) animToPlay = master.UniversalBlank;
                if (smoothAnimationTrans) animator.Play(animToPlay.name, 0, time);
                else animator.Play(animToPlay.name, 0, 0);
            }
        }

        //rotating to look at the camera
        Vector3 lookPos = new Vector3(GameObject.FindGameObjectWithTag("MainCamera").transform.position.x, GameObject.FindGameObjectWithTag("MainCamera").transform.position.y, GameObject.FindGameObjectWithTag("MainCamera").transform.position.z);
        if (enemy.spriteVerticalSquish) lookPos.y = lookPos.y - (4 * (lookPos.y - transform.position.y) / 5);   //This is the line that reduces the rotation upward and downward to make verticality work
        graphicHolder.LookAt(lookPos);
    }

}
