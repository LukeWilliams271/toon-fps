using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public PlayerCont player;
    public Transform playerHands;

    public float moveSpeed;
    public float jumpForce;
    public float airMoveReduction;

    public float gravity = 9.8f;

    private Rigidbody rigidBody;
    [HideInInspector]
    public bool isGrounded;
    public bool isBonkHead;
    public bool noBonk;
    private bool wasGrounded;
    private bool coyoteTime;
    public LayerMask whatIsGround;
    [HideInInspector]
    public bool isMantleing;

    [HideInInspector]
    public MouseLook mouseLook;
    public Transform mantleCheck;

    private int horizontal;
    private int vertical;
    private bool jumping;
    private bool jump;

    private CharacterController character;

    private Vector3 lastFrameMovementVector;
    public float lerpTime;
    private float lerpValue = 0;
    Vector3 gravVelocity;
    private bool headBobbing;
    private float headBobTime;
    private float handMoveTime;
    private float handMoveWidth;
    private Vector3 lastPos;
    private Vector3 handStandardPos;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = GameObject.FindGameObjectWithTag("Level").GetComponent<LevelInformation>().checkPointsInOrder[player.master.GetComponent<MenuManager>().checkPointIndex].transform.position;
        rigidBody = gameObject.GetComponent<Rigidbody>();
        mouseLook = gameObject.GetComponent<MouseLook>();

        character = GetComponent<CharacterController>();
        lastPos = transform.position;
        handStandardPos = playerHands.position;

    }
    void Update()
    {
        //Get Input
        vertical = Convert.ToInt32(player.binds.KeyAtBind("MoveForward", true)) - Convert.ToInt32(player.binds.KeyAtBind("MoveBackward", true));
        horizontal = Convert.ToInt32(player.binds.KeyAtBind("MoveRight", true)) - Convert.ToInt32(player.binds.KeyAtBind("MoveLeft", true));
        jumping = player.binds.KeyAtBind("Jump", false);

        UpdatePlayerMovement();
    }

    // Update is called once per frame

    public void UpdatePlayerMovement()
    {
        CoyoteTimeCheck();


        //Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, character.height / 2 + 0.3f, whatIsGround);
        isBonkHead = Physics.Raycast(transform.position, Vector3.up, character.height / 2 + 0.2f, whatIsGround); 

        //Combine force vectors
        Vector3 movementVector = (transform.forward * vertical + transform.right * horizontal).normalized * (1 - Convert.ToInt32(!isGrounded) * airMoveReduction);

        if ((isGrounded || coyoteTime) && jumping && !jump)
        {
            gravVelocity = Vector3.up * jumpForce + movementVector * moveSpeed / 4;
            jump = true;
            Invoke("JumpReset", 0.07f);
        }

        if (!isGrounded && !coyoteTime)
        {
            //This helps fix a bug where the character jitters when jumping next to objects
            character.stepOffset = 0.01f;
            character.slopeLimit = 45;
            if (isBonkHead && !noBonk)
            {
                gravVelocity = Vector3.down * 0.2f;
                noBonk = true;
                Invoke("ResetBonk", 0.1f);
            }
            gravVelocity += Vector3.down * gravity * Time.deltaTime;
        }
        //This helps fix a bug where the character jitters when jumping next to objects
        if (isGrounded)
        {
            character.stepOffset = 0.3f;
            character.slopeLimit = 45;
        }
        if (isGrounded && !jump)
        {
            gravVelocity = new Vector3(0, 0, 0);
        }

        if (!isGrounded && !isMantleing)
        {
            if (MantleCheck())
            {
                gravVelocity = Vector3.up * 10;
                isMantleing = true;
                player.MantleAnim(this);
            }
        }


        //Get lerp information & lerp
        if (movementVector != lastFrameMovementVector) lerpValue = 12;

        if (lerpValue - lerpTime * Time.deltaTime >= 0) lerpValue = lerpValue - lerpTime * Time.deltaTime;
        else lerpValue = 0;

        movementVector = Vector3.Lerp(movementVector, lastFrameMovementVector, lerpValue / 12);

        //move
        lastFrameMovementVector = movementVector;
        character.Move(movementVector * moveSpeed * Time.deltaTime);

        character.Move(gravVelocity * Time.deltaTime);
        Debug.Log(isGrounded + "     " + gravVelocity);

        if (Vector3.Distance(lastPos, transform.position) > 0.03 && isGrounded && !headBobbing)
        {
            headBobbing = true;

        }
        lastPos = transform.position;
     
        if (headBobbing == true && Time.timeScale != 0)
        {
            headBobTime += Time.deltaTime * 10;
            mouseLook.cam.transform.Translate(Vector3.down * Mathf.Sin(headBobTime) / 400);
            if (headBobTime > 6.282f)
            {
                headBobTime = 0;
                headBobbing = false;
            }

            handMoveTime += Time.deltaTime * 5;

            playerHands.Translate(Vector3.down * Mathf.Cos(handMoveTime * 2) / 3);
            playerHands.Translate(Vector3.right * Mathf.Sin(handMoveTime)/2);

            if (handMoveTime > 6.282f)
            {
                handMoveTime = 0;
            }

        }
        if (headBobbing == false && playerHands.position != handStandardPos)
        {
            playerHands.Translate((handStandardPos - playerHands.position) * Time.deltaTime * 5);
        }

    }
    private void JumpReset()
    {
        jump = false;
    }
    private void ResetBonk()
    {
        noBonk = false;    
    }

    private void CoyoteTimeCheck()
    {
        if (!isGrounded && wasGrounded)
        {
            coyoteTime = true;
            Invoke("ResetCoyote", 0.1f);
        }
        wasGrounded = isGrounded;
    }

    private void ResetCoyote()
    {
        coyoteTime = false;
    }

    private bool MantleCheck()
    {
        RaycastHit hit;
        Physics.Raycast(mantleCheck.position, -transform.up, out hit, 1.5f, whatIsGround);

        if (hit.point != new Vector3(0, 0, 0) && (Vector3.Angle(hit.normal, Vector3.up) <= 45f))
        {
            return true;
        }
        else return false;
    }
}
