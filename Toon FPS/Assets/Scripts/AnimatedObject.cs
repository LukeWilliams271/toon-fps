using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedObject : MonoBehaviour
{
    public GameMaster master;

    //allows us to iterate through attack points
    int lastAttackPoint;

    //the number of bullets fired during this specific "trigger pull"
    int bulletsShot;

    public AttackPatterns attackPattern;

    private ObjectPooler objectPooler;

    private bool allowInvoke = true;

    private bool shot = false;

    [HideInInspector]
    public float randomZRotation = 360;
    private void Start()
    {
        master = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        objectPooler = master.GetComponent<ObjectPooler>();
    }

    private void Shoot()
    {
        if (objectPooler != null)
        {
            GetComponentInChildren<SpreadPatternSetup>().ChangeSpreadPattern(attackPattern.weaponProperties.spreadPattern);

            bulletsShot++;

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
            currentBullet.transform.Rotate(0, 0, Random.Range(-randomZRotation, randomZRotation));

            if (attackPattern.vFXAndSFX.impactTexture.Length > 0)
            {
                currentBullet.GetComponent<EnemyProjectile>().impactTexture = attackPattern.vFXAndSFX.impactTexture[Random.Range(0, attackPattern.vFXAndSFX.impactTexture.Length - 1)];
            }
            currentBullet.transform.Rotate(0, 0, Random.Range(-360, 360));

            shot = true;
            //Invoke resetShot function
            if (allowInvoke)
            {
                if (bulletsShot < attackPattern.weaponProperties.bulletsPerTap)  //This is for attacks with multiple bullets per attack
                {
                    Invoke("Shoot", attackPattern.weaponProperties.timeBetweenShots);
                }
            }
        }
    }
}
