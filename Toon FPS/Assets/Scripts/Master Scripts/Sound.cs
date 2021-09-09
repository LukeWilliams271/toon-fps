using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound 
{
    public string name;

    public AudioClip clip;

    public AudioMixerGroup mixer;

    [Range(0f, 1f)]
    public float volume;

    [Range(0.1f, 3f)]
    public float pitch;

    public float pitchShiftAmount;

    public bool loops;

    public bool reverb;

    public bool cancelOnWeaponSwap;

    [HideInInspector]
    public AudioSource source;
}
