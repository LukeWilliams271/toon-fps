using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class KeyBinds : MonoBehaviour
{
    //[HideInInspector]
    //public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
    public List<string> keyNames;
    public List<KeyCode> keyBinds;

    [System.Serializable]
    public struct KeyTexts
    {
        public TMP_Text moveFoward;
        public TMP_Text moveBackward;
        public TMP_Text moveLeft;
        public TMP_Text moveRight;
        public TMP_Text jump;
        public TMP_Text interact;
        public TMP_Text fire;
        public TMP_Text altFire;
        public TMP_Text reload;
        public TMP_Text nextWeapon;
        public TMP_Text lastWeapon;
        public TMP_Text openFiles;
        public TMP_Text pauseGame;
        public TMP_Text weapon1;
        public TMP_Text weapon2;
        public TMP_Text weapon3;
        public TMP_Text weapon4;
        public TMP_Text weapon5;

    }
    public KeyTexts texts;

    [NamedArrayAttribute(new string[] { "MoveForward", "MoveBackward", "MoveLeft", "MoveRight", "Jump", "Interact", "Fire", "AltFire", "Reload", "NextWeapon", "LastWeapon", "OpenFiles", "PauseGame", "Weapon1", "Weapon2", "Weapon3", "Weapon4", "Weapon5" })]
    public GameObject[] keyButtonTexts;

    private GameObject currentKey;
    private bool rebinding;

    //More settings
    public float mouseSensitivity = 10;

    // Start is called before the first frame update
    void Awake()
    {
        //Get actual keys from save/load system
        //For now, add keys to the dictionary here to be able to edit them
        if (GetComponent<MenuManager>().saveData == null)
        {
            AddKey("MoveForward", KeyCode.W);
            AddKey("MoveBackward", KeyCode.S);
            AddKey("MoveLeft", KeyCode.A);
            AddKey("MoveRight", KeyCode.D);
            AddKey("Jump", KeyCode.Space);
            AddKey("Interact", KeyCode.F);
            AddKey("Fire", KeyCode.Mouse0);
            AddKey("AltFire", KeyCode.Mouse1);
            AddKey("Reload", KeyCode.R);
            AddKey("NextWeapon", KeyCode.Joystick8Button19);
            AddKey("LastWeapon", KeyCode.Joystick8Button18);
            AddKey("OpenFiles", KeyCode.Tab);
            AddKey("PauseGame", KeyCode.Escape);
            AddKey("Weapon1", KeyCode.Alpha1);
            AddKey("Weapon2", KeyCode.Alpha2);
            AddKey("Weapon3", KeyCode.Alpha3);
            AddKey("Weapon4", KeyCode.Alpha4);
            AddKey("Weapon5", KeyCode.Alpha5);
        }
        else
        {
            //keys = GetComponent<MenuManager>().saveData.savedBinds;
        }
        

       
            texts.moveFoward.text = GetText("MoveForward");
            texts.moveBackward.text = GetText("MoveBackward");
            texts.moveLeft.text = GetText("MoveLeft");
            texts.moveRight.text = GetText("MoveRight");
            texts.jump.text = GetText("Jump");
            texts.interact.text = GetText("Interact");
            texts.fire.text = GetText("Fire");
            texts.altFire.text = GetText("AltFire");
            texts.reload.text = GetText("Reload");
            texts.nextWeapon.text = GetText("NextWeapon");
            texts.lastWeapon.text = GetText("LastWeapon");
            texts.openFiles.text = GetText("OpenFiles");
            texts.pauseGame.text = GetText("PauseGame");
            texts.weapon1.text = GetText("Weapon1");
            texts.weapon2.text = GetText("Weapon2");
            texts.weapon3.text = GetText("Weapon3");
            texts.weapon4.text = GetText("Weapon4");
            texts.weapon5.text = GetText("Weapon5");

    }
    public void OnGUI()
    {
        if (currentKey != null)
        {
            currentKey.GetComponent<TMP_Text>().text = "Press any key";
            Event e = Event.current;
            if (e.isKey)
            {
                SetKey(currentKey, e.keyCode.ToString(), e.keyCode); 
            }
            else if (e.isMouse)
            {
                if (e.button == 0) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse0);
                if (e.button == 1) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse1);
                if (e.button == 2) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse2);
                if (e.button == 3) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse3);
                if (e.button == 4) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse4);
                if (e.button == 5) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse5);
                if (e.button == 6) SetKey(currentKey, "Mouse " + e.button.ToString(), KeyCode.Mouse6);

            }
            else if (e.isScrollWheel)
            {
                if (e.delta.y > 0)
                {
                    SetKey(currentKey, "Scroll Up", KeyCode.Joystick8Button19);
                }
                if (e.delta.y < 0)
                {
                    SetKey(currentKey, "Scroll Down", KeyCode.Joystick8Button18);
                }
                currentKey = null;
            }

        }  
    }
    public void RebindKey(GameObject clicked)
    {
        currentKey = clicked;
        rebinding = true;
    }
    private void SetKey(GameObject keyButton, string name, KeyCode key)
    {
        keyBinds[Array.IndexOf(keyNames.ToArray(), keyButton.name)] = key;
        keyButton.GetComponent<TMP_Text>().text = name;
        currentKey = null;
        Invoke("ResetBindingState", 0.1f);
    }

    //Returns the press state of the key given the bind name
    public bool KeyAtBind(string bindName, bool hold)
    {
        if (rebinding == false)
        {
            //Exeptions, primarly dealing with the mouse wheel stand-in keys
            if (keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)] == KeyCode.Joystick8Button19)
            {
                
                if (Input.mouseScrollDelta.y > 0.7f)
                {
                    return true;
                }
                else return false;
            }
            if (keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)] == KeyCode.Joystick8Button18)
            {

                if (Input.mouseScrollDelta.y < -0.7f)
                {
                    return true;
                }
                else return false;
            }

            //This is the return that all normal keys and mouse buttons will abide by
            if (hold) return Input.GetKey(keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)]);
            else return Input.GetKeyDown(keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)]);
        }
        else return false;
    }
    public string GetText(string bindName)
    {
        if (keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)] == KeyCode.Joystick8Button18) return "Scroll Down";
        if (keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)] == KeyCode.Joystick8Button19) return "Scroll Up";
        return keyBinds[Array.IndexOf(keyNames.ToArray(), bindName)].ToString();
    }
    private void AddKey(string name, KeyCode keyCode)
    {
        keyNames.Add(name);
        keyBinds.Add(keyCode);
    }
    public void SetAllKeys(KeyCode[] newKeyCodes)
    {
        for (int i = 0; i < keyBinds.Count; i++)
        {
            SetKey(keyButtonTexts[i], newKeyCodes[i].ToString(), newKeyCodes[i]);
        }
    }
    public void SetMouseSensitivity(float sens)
    {
        mouseSensitivity = sens; 
    }

    public void ResetBindingState()
    {
        rebinding = false;
    }
}

