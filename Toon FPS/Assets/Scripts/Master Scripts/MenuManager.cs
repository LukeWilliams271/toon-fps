using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public class SaveObject
    {
        public WeaponScript[] savedWeapons;
        public KeyCode[] savedBinds;
        public float savedMouseSens;
        public int savedLevelIndex;
        public int savedCheckPointIndex;
        public string savedLevelName;
    }

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public GameObject tabMenuUI;

    public int checkPointIndex;
    [HideInInspector]
    public bool gameIsPaused;
    [HideInInspector]
    public bool insideTabMenu;
    public AudioMixer masterGroup;
    public bool debugDisablePause;
    private bool inOptions;

    [HideInInspector]
    public KeyBinds binds;


    //[HideInInspector]
    public int currentLevel;
    public SaveObject saveData;
     
    // Start is called before the first frame update
    void Start()
    {
        currentLevel = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameIsPaused || insideTabMenu || SceneManager.GetActiveScene().buildIndex == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        masterGroup.SetFloat("masterPitch", Time.timeScale);
        currentLevel = SceneManager.GetActiveScene().buildIndex;
        if (binds.KeyAtBind("PauseGame", false) && !debugDisablePause && currentLevel != 0)
        {
            if (gameIsPaused)
            {
                if (inOptions)
                {
                    CloseOptions();
                }
                else
                {
                    ResumeGame();
                }
            }
            else if (!gameIsPaused)
            {
                if (insideTabMenu) CloseTabMenu();
                PauseGame();
            }
        } 
        if (binds.KeyAtBind("OpenFiles", false) && !gameIsPaused && SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (insideTabMenu)
            {
                CloseTabMenu();
            } 
            else if (!insideTabMenu)
            {
                OpenTabMenu();
            }
        }
        
    }

    public void LoadLevel(int number)
    {
        CloseOptions();
        ResumeGame();
        SceneManager.UnloadSceneAsync(currentLevel);
        SceneManager.LoadScene(number);
        checkPointIndex = 0;
    }
    public void LoadLevel(int number, int checkPointNumber)
    {
        CloseOptions();
        ResumeGame();
        SceneManager.UnloadSceneAsync(currentLevel);
        SceneManager.LoadScene(number);
        checkPointIndex = checkPointNumber;
    }

    public void LoadSave()
    {
        if (File.Exists(Application.dataPath + "/save.txt"))
        {
            string json = File.ReadAllText(Application.dataPath + "/save.txt");
            saveData = JsonUtility.FromJson<SaveObject>(json);

            GetComponent<WeaponManager>().weapons = saveData.savedWeapons;
            GetComponent<KeyBinds>().SetAllKeys(saveData.savedBinds);
            GetComponent<KeyBinds>().mouseSensitivity = saveData.savedMouseSens;
        }
        Debug.Log(saveData.savedLevelIndex);
        LoadLevel(saveData.savedLevelIndex, saveData.savedCheckPointIndex);
    }
    public void SaveAndExitToMain()
    {

        SaveObject saveObject = new SaveObject {
            savedWeapons = GetComponent<WeaponManager>().weapons,
            savedBinds = GetComponent<KeyBinds>().keyBinds.ToArray(),
            savedMouseSens = GetComponent<KeyBinds>().mouseSensitivity,
            savedCheckPointIndex = checkPointIndex,
            savedLevelIndex = currentLevel,
            savedLevelName = GetComponent<GameMaster>().levelInfo.levelName
        };
        string json = JsonUtility.ToJson(saveObject);
        File.WriteAllText(Application.dataPath + "/save.txt", json);
        LoadLevel(0);                                                                                                           
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }
    public void OpenOptions()
    {
        gameIsPaused = true;
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        inOptions = true;
        optionsMenuUI.GetComponent<SettingsManger>().ChangeTab(optionsMenuUI.GetComponent<SettingsManger>().activeTab);
    }
    public void CloseOptions()
    {
        gameIsPaused = true;
        inOptions = false;
        if (SceneManager.GetActiveScene().buildIndex != 0) pauseMenuUI.SetActive(true); 
        optionsMenuUI.SetActive(false);
    }
  
    public void OpenTabMenu()
    {
        insideTabMenu = true;
        Time.timeScale = 0f;
        tabMenuUI.SetActive(true);

    }
    public void CloseTabMenu()
    {
        insideTabMenu = false;
        Time.timeScale = 1f;
        tabMenuUI.SetActive(false);
        tabMenuUI.GetComponentInChildren<InventoryManager>().isReordering = false;
        tabMenuUI.GetComponentInChildren<InventoryManager>().selectedSlot = null;
    }
    
    public void UpdateLoadButtonText(GameObject loadButtonButton, GameObject loadButtonText)
    {
        if (File.Exists(Application.dataPath + "/save.txt"))
        {
            loadButtonButton.GetComponentInParent<Button>().interactable = true;
            string json = File.ReadAllText(Application.dataPath + "/save.txt");
            saveData = JsonUtility.FromJson<SaveObject>(json);
            loadButtonText.GetComponent<TMP_Text>().text = "LOAD LEVEL: " + saveData.savedLevelName;
        } else
        {
            loadButtonButton.GetComponentInParent<Button>().interactable = false;
            loadButtonText.GetComponent<TMP_Text>().text = "LOAD LEVEL";
        }
    }
}
