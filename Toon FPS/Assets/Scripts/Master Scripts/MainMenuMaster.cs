using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMaster : MonoBehaviour
{
    //This whole script just call functions from the gameMaster's MenuManager script because of some fuckery

    private MenuManager daddy;  // lol
    public GameObject loadLevelText;
    public GameObject loadLevelButton;

    void Awake()
    {
        daddy = GameObject.Find("GameMaster").GetComponent<MenuManager>();
        daddy.UpdateLoadButtonText(loadLevelButton, loadLevelText);
    }

    public void LoadLevel(int levelToLoad)
    {

        daddy.LoadLevel(levelToLoad, 0);
    }
    public void OpenOptions()
    {
        daddy.OpenOptions();
    }
    public void Quit()
    {
        daddy.Quit();
    }
    public void LoadSave()
    {
        daddy.LoadSave();
    }
}
