using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[System.Serializable]
public class GameMaster : MonoBehaviour
{
    public WeaponScript[] allWeaponScripts;

    [HideInInspector]
    public NewWeaponPickup activeChest;
    public int activeChestIndex;

    public LevelInformation levelInfo;

    public float gravity;

    public AnimationClip UniversalBlank;

    [HideInInspector]
    public PlayerCont player;
    [HideInInspector]
    public Transform playerBody;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        
    }
    
    private void Update()
    {
        if (GetComponent<MenuManager>().currentLevel != 0)
        {  
            if (levelInfo == null) levelInfo = GameObject.FindGameObjectWithTag("Level").GetComponent<LevelInformation>();
            activeChest = levelInfo.chestsInOrder[activeChestIndex];
            if (activeChest != levelInfo.chestsInOrder[activeChestIndex] || activeChest == null) activeChest = levelInfo.chestsInOrder[activeChestIndex];
            //Get the playerBody
            CharacterController p = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
            if (levelInfo != null) playerBody = levelInfo.playerBodyRef;
        }
        

        if (GetComponent<MenuManager>().currentLevel == 0) GetComponent<WeaponManager>().enabled = false;
        else
        {
            GetComponent<WeaponManager>().enabled = true;
        }

        if (GameObject.Find("GameMaster") != this.gameObject)
        {
            Destroy(this.gameObject);
        }

        Physics.gravity = new Vector3(0, -gravity, 0);

        if (GetComponent<MenuManager>().currentLevel == 0)
        {
           GetComponent<WeaponManager>().enabled = false;
        }
        else
        {
            GetComponent<WeaponManager>().enabled = true;
        }    
    }

}
