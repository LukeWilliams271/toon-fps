using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManagement : MonoBehaviour
{
    public bool isPlayer;
    public PlayerCont playerCont;
    public float health;
    public float maxHealth;
    [HideInInspector]
    public float overHeal;
    [HideInInspector]
    public GameMaster master;
    public float playerBonusLossRate;

    private float time;

    public float onKillDelayTime;
    [HideInInspector]
    public float playerHealthLossDelay;
    public void TakeDamage(float value)
    {
        if (!isPlayer)
        {
            health -= value;
            EnemyTakeDamage();
        }
        else if (isPlayer)
        {
            if (overHeal <= 0)
            {
                health -= value;
                overHeal = 0;
            }
            else
            {
                overHeal -= value;
            }
            if (value < 0) overHeal -= value;
            if (value > 0) PlayerTakeDamage();
        }
        if (health <= 0)
        {
            if (!isPlayer)
            {
                EnemyDeath();
            } else
            {
                PlayerDeath();
            }
             
        }

    }
    public void Start()
    {
        if (!isPlayer) health = GetComponent<EnemyScript>().enemy.maxHealth;
        else health = playerCont.standardHealth;
    }
    public void Awake()
    {
        master = GameObject.Find("GameMaster").GetComponent<GameMaster>();
    }

    public void Update()
    {
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        if (isPlayer)
        {
            if (playerHealthLossDelay >= 0) playerHealthLossDelay -= Time.deltaTime;

            if (health > playerCont.standardHealth)
            {
                overHeal += health - playerCont.standardHealth;
                health = playerCont.standardHealth;
            }

            if (overHeal < 0) overHeal = 0;
            if (overHeal > playerCont.maxHealth) overHeal = maxHealth;

            if (playerHealthLossDelay <= 0 && overHeal > 0)
            {
                time += Time.deltaTime * playerBonusLossRate;
                if (time >= 1)
                {
                    overHeal--;
                    time = 0;
                }
            }
        }
    }
    private void SetEnemyAnimation(int zeroDIndex)
    {
        if (!isPlayer)
        {
            EnemyScript e = GetComponent<EnemyScript>();
            e.toPlay = e.enemy.animations[zeroDIndex];
        }
    }

    private void PlayerDeath()
    {
        master.GetComponent<MenuManager>().LoadLevel(master.GetComponent<MenuManager>().currentLevel);
    }   
    private void EnemyDeath()
    {
        if (GetComponent<EnemyScript>().isDying == false) GetComponent<EnemyScript>().Die();
    }

    private void PlayerTakeDamage()
    {
        playerCont.hudManager.PlayRaspberryJam();
    }
    private void EnemyTakeDamage()
    {
        GetComponent<EnemyScript>().spriteMaskAnimator.SetTrigger("DmgTrig");
    }

    public void StartHealthLossDelay()
    {
        playerHealthLossDelay = onKillDelayTime;
    }
}
