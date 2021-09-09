using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameMaster master;
    [Space(3)]
    public GameObject informationUI;
    public Image infoImage;
    public TMP_Text infoName;
    public TMP_Text infoDescription;
    public TMP_Text infoTutorial;

    [TextArea(10, 15)]
    public string tabMenuTutorialText;
    [Space(3)]
    public WeaponManager weaponManager;
    public InventorySlot[] slots;

    public bool isReordering;
    public int oldIndex;
    public InventorySlot selectedSlot;

    public void OnEnable()
    {
        infoImage.sprite = null;
        infoName.text = "";
        infoDescription.text = tabMenuTutorialText;
        infoTutorial.text = "";
    }
    public void AssignWeaponToButton(WeaponScript weapon, int index)
    {
        slots[index].weapon = weapon;
        slots[index].DisplaySprite();
    }

    public void ReorderWeapons(InventorySlot chosenSlot)
    {
        if (chosenSlot.weapon != null)
        {
            if (isReordering)
            {
                InventorySlot[] s = GetComponentsInChildren<InventorySlot>();
                s[oldIndex].transform.SetSiblingIndex(Array.IndexOf(s, chosenSlot));
                s = GetComponentsInChildren<InventorySlot>();

                foreach (InventorySlot i in s)
                {
                    if (i.weapon != null)
                    {
                        weaponManager.weapons[Array.IndexOf(s, i)] = i.weapon;
                        weaponManager.weaponAmmoCounts[Array.IndexOf(s, i)] = i.ammoCount;
                    }
                }
                weaponManager.lastWeapon = -1;
                isReordering = false;
                ClearSlotUI();
            }
            else if (!isReordering)
            {
                oldIndex = chosenSlot.transform.GetSiblingIndex();
                chosenSlot.slotArrow.enabled = true;
                chosenSlot.slotArrow.color = Color.grey;
                isReordering = true;
            }
        }
    }

    public void ExamineWeapon(InventorySlot chosenSlot)
    {
        if (chosenSlot.weapon != null)
        {
            selectedSlot = chosenSlot;

            infoImage.sprite = selectedSlot.weapon.tabInfo.tabMenuSprite;
            infoName.text = selectedSlot.weapon.tabInfo.formalName;
            infoDescription.text = selectedSlot.weapon.tabInfo.description;
            infoTutorial.text = selectedSlot.weapon.tabInfo.tutorial;
            informationUI.SetActive(true);
        }
    }
    public void DropWeapon()
    {
        Debug.Log("Ran Drop");
        WeaponManager w = master.GetComponent<WeaponManager>();
        if (selectedSlot != null)
        {
            
            selectedSlot.transform.SetSiblingIndex(w.weapons.Length - 1);
            List<WeaponScript> temp = new List<WeaponScript>(w.weapons);
            temp.Remove(selectedSlot.weapon);
            w.weapons = temp.ToArray();
           
            w.lastWeapon = -1;
            informationUI.SetActive(false);
            selectedSlot.weapon = null;
           
        }
    }

    public void ClearSlotUI()
    {
        foreach (InventorySlot s in slots)
        {
            s.slotArrow.enabled = false;
        }
    }
}

    
