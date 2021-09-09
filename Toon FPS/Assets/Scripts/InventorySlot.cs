using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public WeaponManager master;
    public InventoryManager inventoryManager;
    public WeaponScript weapon;
    public Image spriteRenderer;
    public TMP_Text slotLabel;
    [Space(3)]
    public Sprite emptySlotSprite;
    public Image slotArrow;
    [HideInInspector]
    public float ammoCount;
    public void DisplaySprite()
    {
        if (transform.GetSiblingIndex() <= master.weapons.Length - 1) weapon = master.weapons[transform.GetSiblingIndex()];
        else weapon = null;

        if (weapon != null)
        {
            spriteRenderer.sprite = weapon.tabInfo.tabMenuSprite;
            slotLabel.text = weapon.tabInfo.formalName;
        } else
        {
            spriteRenderer.sprite = emptySlotSprite;
            slotLabel.text = "";
        }
        
    }
    public void GrabAmmoCount()
    {
        if (weapon != null) ammoCount = master.weaponAmmoCounts[Array.IndexOf(master.weapons, weapon)];
    }
    public void Update()
    {
        GrabAmmoCount();
        DisplaySprite();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && weapon != null)
        {
            inventoryManager.ReorderWeapons(this);
        }
    }

}
