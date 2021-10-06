using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NewWeaponPickup : MonoBehaviour
{
    [HideInInspector]
    public GameMaster master;
    public SpriteRenderer pickup;
    public int legendaryThreshold;
    public int epicThreshold;
    public int rareThreshold;
    public int orderInLevel;

    [Space]
    [Header("Anim Conts")]
    public RuntimeAnimatorController commonCont;
    public RuntimeAnimatorController rareCont;
    public RuntimeAnimatorController epicCont;
    public RuntimeAnimatorController legendaryCont;

    private WeaponScript w;
    private WeaponScript c;
    private bool opened;
    [HideInInspector]
    public bool weaponShown;
    [HideInInspector]
    public bool collectable = false;
    [HideInInspector]
    public bool openable = false;

    [HideInInspector]
    public int score;
    [HideInInspector]
    public float scoreAsFloat;

    public void Start()
    {
        master = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        weaponShown = false;
        opened = false;
       
    }
    public void Update()
    {
        scoreAsFloat =  (float)score / (float)legendaryThreshold;

        if (opened == false)
        {
            c = RandomizeWeapon(); //Literally just for changing the color of the gift box
        }
        else openable = false;
        if (weaponShown)
        {
            pickup.gameObject.SetActive(true);
        }
        else
        {
            pickup.gameObject.SetActive(false);
        }

        if (master.GetComponent<KeyBinds>().KeyAtBind("Interact", false) && collectable == true)
        {
            CollectWeapon();
            master.player.hudManager.CloseInteractPrompt();
        }

        if (master.GetComponent<KeyBinds>().KeyAtBind("Interact", false) && openable == true)
        {
            OpenChest();
            master.player.hudManager.CloseInteractPrompt();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && weaponShown == true)
        {

            collision.gameObject.GetComponent<HealthManagement>().playerCont.hudManager.PromptInteract("collect " + w.tabInfo.formalName);
            collectable = true;   
        }
        if (collision.gameObject.CompareTag("Player") && opened == false)
        {
            Debug.Log("Hello");

            collision.gameObject.GetComponent<HealthManagement>().playerCont.hudManager.PromptInteract("open gift");
            openable = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && weaponShown == true)
        {
            collision.gameObject.GetComponent<HealthManagement>().playerCont.hudManager.CloseInteractPrompt();
            collectable = false;
            openable = false;
        }
        if (collision.gameObject.CompareTag("Player") && opened == false)
        {
            collision.gameObject.GetComponent<HealthManagement>().playerCont.hudManager.CloseInteractPrompt();
            collectable = false;
            openable = false;
        }
    }

    public void OpenChest()
    {
        w = RandomizeWeapon();
        pickup.sprite = w.metaSetup.pickupSprite;
        GetComponent<Animator>().SetTrigger("OpenTrig");
        weaponShown = true;
        opened = true;
        master.activeChestIndex++;
    }

    public WeaponScript RandomizeWeapon()
    {
        List<WeaponScript> weaponList = new List<WeaponScript>();

        if (score >= legendaryThreshold)
        {
            foreach (WeaponScript w in master.allWeaponScripts) if (w.weaponProperties.rarity.ToString() == "Legendary") weaponList.Add(w);
            GetComponent<Animator>().runtimeAnimatorController = legendaryCont;

        } else if (score >= epicThreshold && score <= +legendaryThreshold)
        {
            foreach (WeaponScript w in master.allWeaponScripts) if (w.weaponProperties.rarity.ToString() == "Epic") weaponList.Add(w);
            GetComponent<Animator>().runtimeAnimatorController = epicCont;
        }
        else if (score >= rareThreshold && score <= epicThreshold)
        {
            foreach (WeaponScript w in master.allWeaponScripts) { if (w.weaponProperties.rarity.ToString() == "Rare") weaponList.Add(w); }
            GetComponent<Animator>().runtimeAnimatorController = rareCont;
        }
        else if (score <= rareThreshold)
        {
            foreach (WeaponScript w in master.allWeaponScripts) { if (w.weaponProperties.rarity.ToString() == "Common") weaponList.Add(w); }
            GetComponent<Animator>().runtimeAnimatorController = commonCont;
        }

        return weaponList[UnityEngine.Random.Range(0, weaponList.ToArray().Length)];
    }

    public void CollectWeapon()
    {
        WeaponManager m = master.GetComponent<WeaponManager>();
        weaponShown = false;

        if (master.GetComponent<WeaponManager>().weapons.Length <= 5)
        {
            List<WeaponScript> temp = new List<WeaponScript>(m.weapons);
            temp.Add(w);
            m.weapons = temp.ToArray();
            m.activeWeapon = temp.ToArray().Length - 1;

            List<float> tempAmmo = new List<float>(m.weaponAmmoCounts);
            tempAmmo.Add(1);
            m.weaponAmmoCounts = tempAmmo.ToArray();
            m.inventoryManager.AssignWeaponToButton(w, Array.IndexOf(m.weapons, w));

        }
    }
}
