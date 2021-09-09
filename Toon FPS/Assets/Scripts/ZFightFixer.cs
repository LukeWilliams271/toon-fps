using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZFightFixer : MonoBehaviour
{
    public MeshRenderer rend;

    public void Awake()
    {
        //rend.sortingOrder = 2;  
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ZFightFixer>())
        {
            rend.sortingOrder = collision.gameObject.GetComponent<ZFightFixer>().rend.sortingOrder + 1;
        }
       
    }
}
