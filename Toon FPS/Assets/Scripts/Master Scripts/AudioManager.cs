using System.Collections;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public List<AudioSource> cancelOnSwapList;

    // Start is called before the first frame update
    void Update()
    {
        SoundDoneCheck(); 
    }

    public void Play(Sound s, float pitchShiftMult)
    {
        AudioSource[] a = GetComponents<AudioSource>();
        int i = 0;
        List<AudioSource> soundClones = new List<AudioSource>();
        foreach (AudioSource p in a)
        {
            
            if (p == s.source)
            {
                soundClones.Add(p);
                if (i > 5 && soundClones != null) soundClones[i - 5].Stop();
                i++;
            }
        }
            s.source = gameObject.AddComponent<AudioSource>();
            if (s.cancelOnWeaponSwap) cancelOnSwapList.Add(s.source);
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.mixer;
            s.source.volume = s.volume;
            s.source.pitch = UnityEngine.Random.Range(s.pitch - s.pitchShiftAmount, s.pitch + s.pitchShiftAmount);
            s.source.loop = s.loops;
            s.source.bypassReverbZones = !s.reverb;

            s.source.PlayOneShot(s.source.clip);
  
    }
    public void WeaponSwapCancel()
    {
        foreach (AudioSource p in cancelOnSwapList)
        {
             p.Stop();
        }
        
    }
    
    public void SoundDoneCheck()
    {
        AudioSource[] a = GetComponents<AudioSource>();

        foreach (AudioSource p in a)
        {
            if (p != null && !p.isPlaying)
            {
                if (cancelOnSwapList.Contains(p)) cancelOnSwapList.Remove(p);
                Destroy(p);
            }
        }
    }
}
