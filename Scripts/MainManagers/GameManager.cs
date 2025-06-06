using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZeekSpace;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int partyChips = 50;
    public int enemyChips;
    private int difficulty = 5;
    public int[] playerChips = new int[] { 0, 0, 0, 0 };
    public OptionsManager optionsManager { get; private set; }
    public AudioMan audioMan { get; private set; }
    public DeckManager deckManager { get; private set; }
    public MapDeckManager mapDeckManager { get; private set; }
    public bool gameOver = false;
    public GameObject gameOverScreen;
    public bool startDungeonRun = false;
    public bool startBattle = false;
    public bool[] isDead = new bool[] { false, false, false, false };
    public bool[] isFolding = new bool[] { false, false, false, false };
    public bool PlayingCard = false;
    public int whichTurn = 1;
    public List<Character> characters = new List<Character>();
    public Monster monster;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SplitPartyChips()
    {
        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
        if (monsterManager == null)
        {
            Debug.LogError("MonsterManager not found!");
            playerChips[0] = 0;
        }
        else 
        {
            playerChips[0] = monsterManager.monster.monsterChips;
        }
        playerChips[1] = (int)Mathf.FloorToInt(partyChips / 3);
        playerChips[2] = (int)Mathf.Floor((partyChips - playerChips[1]) / 2);
        playerChips[3] = (int)(partyChips - playerChips[1] - playerChips[2]);
    }




    void Start()
    {
        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();

        // Retrieve the name of this scene.
        string sceneName = currentScene.name;

        // Retrieve the index of the scene in the project's build settings.
        int buildIndex = currentScene.buildIndex;

        // Check the scene name as a conditional.
        switch (buildIndex)
        {
            case 0:
                startDungeonRun = true;
                break;
            case 1:
                startBattle = true;
                break;
            case 2:
                break;
        }
        characters[0] = monster.character;
    }













private void InitializeManagers()
    {
        optionsManager = GetComponentInChildren<OptionsManager>();
        audioMan = GetComponentInChildren<AudioMan>();
        deckManager = GetComponentInChildren<DeckManager>();
        mapDeckManager = GetComponentInChildren<MapDeckManager>();

        if (optionsManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/OptionsManager");
            if (prefab == null)
            {
                Debug.Log($"OptionsManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                optionsManager = GetComponentInChildren<OptionsManager>();
            }
        }

        if (audioMan == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/AudioMan");
            if (prefab == null)
            {
                Debug.Log($"AudioMan prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                audioMan = GetComponentInChildren<AudioMan>();
            }
        }

        if (deckManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/DeckManager");
            if (prefab == null)
            {
                Debug.Log($"DeckManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                deckManager = GetComponentInChildren<DeckManager>();
            }
        }


        if (mapDeckManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/MapDeckManager");
            if (prefab == null)
            {
                Debug.Log($"MapDeckManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                mapDeckManager = GetComponentInChildren<MapDeckManager>();
            }
        }





    }

    public int Difficulty
    { get { return difficulty; } set { difficulty = value; } }

    public void GameOver()
    {
        gameOver = true;
        gameOverScreen.SetActive(true);
    }

    public void RestartGame()
    {
    }

}