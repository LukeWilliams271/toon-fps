using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyStatus : MonoBehaviour
{
    public GameObject statusAnimationObject;
    public List<WeaponStatus> activeStatusEffects;
    public List<float> effectDurations;
    public List<GameObject> animationObjects;
    [HideInInspector] EnemyScript enemyScript;

    public float baseSpeed;
    public float baseSize;
    public float baseDamageOverTime;

    [HideInInspector] public float speed;
    // Start is called before the first frame update
    void Start()
    {
        enemyScript = GetComponent<EnemyScript>();
        baseSpeed = enemyScript.enemy.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < activeStatusEffects.Count; i++)
        {
            effectDurations[i] -= Time.deltaTime;
            if (effectDurations[i] <= 0 || GetComponent<EnemyScript>().isDead == true)
            {
                effectDurations.RemoveAt(i);
                activeStatusEffects.RemoveAt(i);
                Destroy(animationObjects[i]);
                animationObjects.RemoveAt(i);
            }
        }
        ApplyStatusEffects();
    }

    public void ApplyNewStatus(WeaponStatus s, Projectile proj)
    {
        bool apply = true;
        foreach (WeaponStatus w in activeStatusEffects)
        {
            //Checking to see if the status has already been applied, and if it stacks with itself
            if (w.effectID != s.effectID || w.selfStacking == true) apply = true;
            else
            {
                apply = false;
                break;
            }
        }
        if (apply == true)
        {
            //applies the effect to the enemy
            GameObject i = Instantiate(statusAnimationObject, this.GetComponent<EnemyScript>().graphicHolder);
            i.GetComponent<Animator>().runtimeAnimatorController = s.enemyStatusAnimation;
            WeaponStatus weaponStatus = new WeaponStatus();
            weaponStatus = s;
            activeStatusEffects.Add(weaponStatus);
            effectDurations.Add(weaponStatus.duration);
            animationObjects.Add(i);

            //Some of the one time, on-apply effects
            if (s.knockback != new Vector3(0, 0, 0))
            {
                enemyScript.AddVelocity(weaponStatus.knockback);
            }
        }
    }

    public void ApplyStatusEffects()
    {
        speed = baseSpeed;

        for (int i = 0; i < activeStatusEffects.Count; i++)
        {
            //mod the moveSpeed
            speed *= activeStatusEffects[i].speedMultiplier;

            //Deal DoT damage for this tick
            if (activeStatusEffects[i].DOTDamagePerSecond != 0) GetComponent<HealthManagement>().TakeDamage(activeStatusEffects[i].DOTDamagePerSecond * Time.deltaTime);
        }    
    }
}
