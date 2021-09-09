using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Weapon Status", menuName = "Weapon Status")]
public class WeaponStatus : ScriptableObject
{
    public string effectID;
    public bool selfStacking;
    public float duration;

    public float speedMultiplier;
    public Vector3 knockback;
    public int DOTDamagePerSecond;

    public RuntimeAnimatorController enemyStatusAnimation;
}
