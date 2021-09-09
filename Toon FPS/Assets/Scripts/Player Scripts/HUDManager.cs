using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class HUDManager : MonoBehaviour
{
    [HideInInspector]
    public GameMaster master;
    public PlayerCont player;
    public HealthManagement playerHealth;
    private bool jamIsOn;
    private float jamCountDown;

    [System.Serializable]
    public struct HUDElements
    {
        public TMP_Text healthText;
        public TMP_Text bonusHealthText;
        public Slider healthBar;
        public Animator healthBarAnimator;

        public TMP_Text enemiesLeft;
        public TMP_Text chestNextUpgrade;
        public Animator chestIcon;
        public TMP_Text ammoText;
        public TMP_Text weaponNameText;

        public TMP_Text interactPrompt;
        public Animator raspberryJam;
        public Image crossHair;
    }
    public HUDElements hudElements;


    // Start is called before the first frame update
    void Start()
    {
        master = player.master;
        hudElements.healthBar.maxValue = player.standardHealth;
        jamIsOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        hudElements.healthText.text = Convert.ToInt32(playerHealth.health).ToString();
        hudElements.bonusHealthText.text = "+" + playerHealth.overHeal.ToString();
        hudElements.healthBar.value = playerHealth.health;
        hudElements.weaponNameText.text = player.weaponScript.tabInfo.formalNameShort;

        if (playerHealth.health <= 50) hudElements.healthBarAnimator.speed = 2f;

        
        int bulletsPerAmmo;
        bulletsPerAmmo = player.weaponScript.weaponProperties.bulletsPerOneAmmo;
        if (bulletsPerAmmo < 1) bulletsPerAmmo = 1;
        hudElements.ammoText.text = (player.bulletsLeft / bulletsPerAmmo).ToString() + "/" + (player.weaponScript.weaponProperties.magSize / bulletsPerAmmo).ToString();
        if (master.activeChest != null) UpdateChestIcon();

        if (jamIsOn)
        {
            jamCountDown -= Time.deltaTime;
            if (jamCountDown <= 0)
            {
                FadeOutRaspberryJam();
            }
        }
        if (!jamIsOn)
        {
            hudElements.raspberryJam.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        }
    }

    public void PromptInteract(string action)
    {
        hudElements.interactPrompt.text = "Press " + master.GetComponent<KeyBinds>().GetText("Interact") + " to " + action;
        hudElements.interactPrompt.gameObject.SetActive(true);
    }
    public void CloseInteractPrompt()
    {
        hudElements.interactPrompt.gameObject.SetActive(false);
    }
    public void UpdateChestIcon()
    {
        hudElements.enemiesLeft.text = (master.activeChest.legendaryThreshold - master.activeChest.score).ToString();
        if (master.activeChest.score >= master.activeChest.legendaryThreshold)
        {
            if (!hudElements.chestIcon.GetCurrentAnimatorStateInfo(0).IsName("LegendaryAnim"))
            {
                hudElements.chestIcon.Play("LegendaryAnim");
            }
            hudElements.chestNextUpgrade.text = "0";
        }
        else if (master.activeChest.score >= master.activeChest.epicThreshold)
        {
            if (!hudElements.chestIcon.GetCurrentAnimatorStateInfo(0).IsName("EpicAnim"))
            {
                hudElements.chestIcon.Play("EpicAnim");
            }
            hudElements.chestNextUpgrade.text = (master.activeChest.legendaryThreshold - master.activeChest.score).ToString();
        }
        else if (master.activeChest.score >= master.activeChest.rareThreshold)
        {
            if (!hudElements.chestIcon.GetCurrentAnimatorStateInfo(0).IsName("RareAnim"))
            {
                hudElements.chestIcon.Play("RareAnim");
            }
            hudElements.chestNextUpgrade.text = (master.activeChest.epicThreshold - master.activeChest.score).ToString();
        }
        else
        {
            hudElements.chestIcon.Play("CommonAnim");
            hudElements.chestNextUpgrade.text = (master.activeChest.rareThreshold - master.activeChest.score).ToString();
        }
    }

    public void PlayRaspberryJam()
    {
        jamIsOn = true;
        jamCountDown = 1;
        hudElements.raspberryJam.SetTrigger("FadeInTrig");
    }

    public void FadeOutRaspberryJam()
    {
        hudElements.raspberryJam.SetTrigger("FadeOutTrig");
        jamIsOn = false;
    }

}
