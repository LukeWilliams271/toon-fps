using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInformation : MonoBehaviour
{
    public string levelName;
    public Transform playerBodyRef;
    public List<NewWeaponPickup> chestsInOrder;
    public List<EnemyScript> enemies;
    public List<CheckPoint> checkPointsInOrder;
    
}
