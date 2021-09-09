using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectScript : MonoBehaviour
{
    public Animator animator;
    public AnimationClip toPlay;

    void Update()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            if (clipInfo[0].clip.name != toPlay.name)
            {
                animator.Play(toPlay.name);
            }
        }
    }
}
