using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotator : MonoBehaviour
{
    public Transform spriteRend;
    public bool dontReduceVerticalRotation;
    Transform cam;

    private void Awake()
    {
        if (spriteRend == null) 
            spriteRend = GetComponentInChildren<SpriteRenderer>().transform;
        if (cam == null)
           cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }
    // Update is called once per frame
    void Update()
    {
        // Rotates the sprite so that it always faces the camera
        if (cam == null)
            cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        Vector3 lookPos = cam.position;
        if (!dontReduceVerticalRotation) lookPos.y = lookPos.y - (4 * (lookPos.y - transform.position.y) / 5);   //This is the line that reduces the rotation upward and downward to make verticality work
        spriteRend.LookAt(lookPos);
        //spriteRend.Rotate(0, 180, 0);
    }
}
