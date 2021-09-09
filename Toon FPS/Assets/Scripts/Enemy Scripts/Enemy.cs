using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class AnimationSet
{
    [Header("Order 0d-360d")]
    public AnimationClip[] clips = new AnimationClip[8];
}

[System.Serializable]
[CreateAssetMenu(fileName = "New Enemy Type", menuName = "Enemy Type")]
public class Enemy : ScriptableObject
{
    //This object stores the universal information for a specific enemy type
    //For stats related to specific attacks, see AttackPatterns

    public RuntimeAnimatorController animCont;
    public AnimationClip deathAnimation;

    [Header("Order Walk, Die")]
    public AnimationSet[] animations = new AnimationSet[2];

    [Space]
    [Header("Attack Patterns, order to be checked for")]
    public AttackPatterns[] attackPatterns;
    [Space]
    public Sprite[] specialGipSprites;

    [Space]
    [Header("Enemy Stats")]
    public float maxHealth = 50;
    public float moveSpeed = 10f;
    public bool flying = false;
    public Vector2 hitboxSize;
    public Vector2 hitboxCenter;
    [Space]
    [Header("Enemy AI")]
    public int agentType;
    public float chaseRange;
    public float LOSSightRange;
    public float minSightRange;
    public int minAttackCountdown;
    public int maxAttackCountdown;
    public float walkPointRange;
    public bool spriteVerticalSquish = true;

}
