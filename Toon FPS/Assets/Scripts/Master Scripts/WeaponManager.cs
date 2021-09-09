using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponManager : MonoBehaviour
{
    public WeaponScript noWeaponWeaponScript;
    public WeaponScript[] weapons;
    public float[] weaponAmmoCounts;
    private float[] weaponAmmoCountdown = new float[5];

    public int activeWeapon;
    [HideInInspector]
    public int lastWeapon;

    private int backgroundWeaponIndex;

    [HideInInspector]
    public PlayerCont p;
    public InventoryManager inventoryManager;

    // Start is called before the first frame update
    private void Start()
    {
        lastWeapon = -1;
    }
    void Update()
    {
        EditWeaponAmmoCounts();
        for (int i = 0; i < weapons.Length - 1; i++)
        {
            inventoryManager.AssignWeaponToButton(weapons[i], i);
        }
        if (p == null) p = GetComponent<GameMaster>().player;

        if (activeWeapon > weapons.Length - 1) activeWeapon = weapons.Length - 1;
        if (activeWeapon < 0) activeWeapon = 0;

        if (activeWeapon != lastWeapon && p != null)
        {
            WeaponScript w;

            if (weapons.Length == 0)
            {
                w = noWeaponWeaponScript;
            } else
            {
                w = weapons[activeWeapon];
                float f = weaponAmmoCounts[activeWeapon] * w.weaponProperties.magSize;
                p.bulletsLeft = (int)f;
            }

            p.weaponScript = w;

            p.SwitchWeapons();

        }
        
        lastWeapon = activeWeapon;
        ReloadOldWeapon(); 
    }

    public void ReloadOldWeapon()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (i != activeWeapon && weapons[i] != null && 0 <= weaponAmmoCounts[i] && weaponAmmoCounts[i] <= 1)
            {
                weaponAmmoCountdown[i] = weaponAmmoCountdown[i] + 1 / weapons[i].weaponProperties.reloadTime * Time.deltaTime;
                if (weaponAmmoCountdown[i] >= 1)
                {
                    weaponAmmoCounts[i] = 1;
                    weaponAmmoCountdown[i] = 1;
                }
            }
        }
    }
    public void EditWeaponAmmoCounts()
    {
        if (weaponAmmoCounts.Length > weapons.Length)
        {
            List<float> temp = new List<float>(weaponAmmoCounts);
            for (int i = weaponAmmoCounts.Length - 1; i > weapons.Length - 1; i--)
            {
                temp.RemoveAt(i);
            }
            weaponAmmoCounts = temp.ToArray();
        }
        if (weaponAmmoCounts.Length < weapons.Length)
        {
            List<float> temp = new List<float>(weaponAmmoCounts);
            for (int i = weaponAmmoCounts.Length - 1; i < weapons.Length - 1; i++)
            {
                temp.Add(1);
            }
            weaponAmmoCounts = temp.ToArray();
        }
    }
}
