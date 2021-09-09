using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    private Transform playerBody;
    public Transform cam;

    private float xRotation = 0f;

    private float mouseX;
    private float mouseY;

    public KeyBinds binds;
    // Start is called before the first frame update
    void Awake()
    {
          
    }

    void Start()
    {
        playerBody = gameObject.transform;
        Cursor.lockState = CursorLockMode.Locked;
        binds = playerBody.GetComponent<HealthManagement>().master.GetComponent<KeyBinds>();
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * binds.mouseSensitivity * 100 * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * binds.mouseSensitivity * 100 * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.localEulerAngles = playerBody.localEulerAngles + new Vector3(0, mouseX, 0);
    }

}
