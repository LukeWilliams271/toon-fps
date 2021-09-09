using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    public PlayerCont cont;
    public float pitchShiftMaxMult;
    public float pitchShiftMinMult;

    public void PlaySound(string soundEffect)
    {
        AudioManager am = cont.master.GetComponent<AudioManager>();
        if (soundEffect == "reload")
        {
            cont.master.GetComponent<AudioManager>().Play(cont.weaponScript.metaSetup.reloadSFX, Random.Range(pitchShiftMinMult, pitchShiftMaxMult));
        }
        if (soundEffect == "shoot") 
        {
            cont.master.GetComponent<AudioManager>().Play(cont.weaponScript.metaSetup.shootSFX, Random.Range(pitchShiftMinMult, pitchShiftMaxMult));
        }
       
    }
}
