using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickupScript : MonoBehaviour
{
    [System.Serializable]
    public struct PickupEffect
    {
        public int healing;
    }
    public PickupEffect pickupEffect;

    private int amountHeal;
    public void Start()
    {
        gameObject.SetActive(true);
        amountHeal = pickupEffect.healing;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            HealthManagement hpMan = other.gameObject.GetComponent<HealthManagement>();
            if (hpMan.health + pickupEffect.healing > hpMan.playerCont.standardHealth)
            {
                amountHeal = Convert.ToInt32(hpMan.playerCont.standardHealth - hpMan.health);   
            }
            hpMan.health += amountHeal;

            gameObject.SetActive(false);
        }
    }
}
