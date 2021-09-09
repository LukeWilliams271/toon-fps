using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class SettingsManger : MonoBehaviour
{
    public GameObject[] tabs;

    [Space(5)]
    public AudioMixer audioMixer;

    [HideInInspector]
    public GameObject activeTab;

    [HideInInspector]
    public float mouseScrollSensitivity = 0.5f;

    public void ChangeTab(GameObject ui)
    {
        ClearTabs();
        ui.SetActive(true);
        activeTab = ui;

    }
    public void ClearTabs()
    {
        foreach (GameObject g in tabs)
        {
            g.SetActive(false);
        }
    }
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
    }
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", volume);
    }
    public void SetEffectsVolume(float volume)
    {
        audioMixer.SetFloat("effectsVolume", volume);
    }
}
