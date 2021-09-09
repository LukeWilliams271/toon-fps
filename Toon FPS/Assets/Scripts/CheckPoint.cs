using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Playables;

public class CheckPoint : MonoBehaviour
{
    public MenuManager master;
    public PlayableDirector director;

    public void Awake()
    {
        master = GameObject.Find("GameMaster").GetComponent<MenuManager>();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && master.GetComponent<GameMaster>().levelInfo != null)
        {
            master.checkPointIndex = master.GetComponent<GameMaster>().levelInfo.checkPointsInOrder.IndexOf(this);

            if (director != null)
            {
                director.Play();
            }
        }
    }
}
