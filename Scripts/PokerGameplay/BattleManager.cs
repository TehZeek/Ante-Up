using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZeekSpace;
using System.Linq;

public class BattleManager : MonoBehaviour
{

    // SETUP: show sprite face off, turnOrder[0] IN DANGER, Special Rules, choose Support or flee
    // start the turn order
    //    => deal pocket cards
    //    => Compare hands
    //    => build battle menu
    //       => Decisions (fold, check, call, raise, bet)
    //              => Space to plug in powers
    //    => monster logic
    //       => Between character turns, check to see if the round is over (all in, fold one side or the other)
    //              => Space to plug in powers/effects
    //    => repeat with flop, turn, (river), (bonus)
    //    => Showdown scene. Sprites face off, throw down, chips divided.
    //    Check for end of battle or Return to Setup

    public MonsterManager monsterManager;
    public GameObject hubPrefab;
    public GameObject scenePrefab;
    public GameObject LoadScreenPrefab;
    public GameObject ActionScreen;
    public List<GameObject> HUDs = new List<GameObject>();
    private GameManager gameManager;
    public List<Transform> hubTransform = new List<Transform>();
    private PokerTableCards pokerTableCards;
    private PokerTurnManager pokerTurnManager;
    private ShiftScene shiftScene;


    void Start()
    {
        ActionScreen.GetComponent<ShowdownManager>().TextEffect("Loading...", ActionScreen.GetComponent<ShowdownManager>().showdownText);
        gameManager = FindFirstObjectByType<GameManager>();
        monsterManager.SetUpMonster();

        MakeHUDs();

        PokerChipManager pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerChipManager.StartChips();

        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        deckManager.BattleSetup();

        shiftScene = FindFirstObjectByType<ShiftScene>();
        Debug.Log("Leaving Battle Manager, onto ShiftScene");
        shiftScene.BuildScenes();

        // Raise Load Screen

        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        battleMenu.UpdateButtonDisplay(0);

        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerTurnManager.stillThisTurn = true;

        //before the below, we can do battle setup scene??
        StartCoroutine(LoadWaitThenShift());
    }

    private IEnumerator LoadWaitThenShift()
    {
        yield return new WaitForSeconds(2f);
        ActionScreen.GetComponent<ShowdownManager>().TextEffect("", ActionScreen.GetComponent<ShowdownManager>().showdownText);
        shiftScene.ShiftDown();
    }

    private void MakeHUDs()
    {
        for (int i = 0; i < (4); i++)
        {
            GameObject newHUD = Instantiate(hubPrefab, hubTransform[i].position, Quaternion.identity, hubTransform[i]);
            if (i != 0) { newHUD.GetComponent<HUD>().isCharacter = true; }
            newHUD.GetComponent<HUD>().characterHud = gameManager.characters[i];
            newHUD.GetComponent<HUD>().MakeHUD();
            HUDs.Add(newHUD);
        }
    }

    public void UpdateHUD()
    {
        HUDs[0].GetComponent<HUD>().RefreshHUD(pokerTableCards.monsterPocket, 0);
        HUDs[1].GetComponent<HUD>().RefreshHUD(pokerTableCards.playerOnePocket, 1);
        HUDs[2].GetComponent<HUD>().RefreshHUD(pokerTableCards.playerTwoPocket, 2);
        HUDs[3].GetComponent<HUD>().RefreshHUD(pokerTableCards.playerThreePocket, 3);
    }

    public void TurnOffHUD()
    {
        foreach (GameObject hudPanel in HUDs)
            HideUiObject(hudPanel);

        foreach (GameObject tableCard in pokerTableCards.tableHand)
            HideUiObject(tableCard);

        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        if (battleMenu != null)
            HideUiObject(battleMenu.gameObject);
    }

    private static void HideUiObject(GameObject uiObject)
    {
        if (uiObject == null) return;

        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;   // fully transparent
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowdownTime()
    {
        TurnOffHUD();
        shiftScene.ShowDownShift();
        ActionScreen.GetComponent<ShowdownManager>().GetManagers();
        ActionScreen.GetComponent<ShowdownManager>().ShowdownSetup();
    }

    public void ResetHand()
    {
        ActionScreen.GetComponent<ShowdownManager>().ResetShowdown();
        pokerTurnManager.ClearTurnVariables();
        pokerTableCards.ClearTable();
        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        battleMenu.UpdateButtonDisplay(0);
        PokerDrawPile pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        PokerHandCompare pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        PokerChipManager pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        for (int i=0; i<4; i++)
        {
            List<GameObject> temp = new List<GameObject>();
            HUDs[i].GetComponent<HUD>().RefreshHUD(temp,i);
            pokerHandCompare.ClearHandData(i);
        }
        pokerChipManager.UpdateChipDisplay();
        LoadWaitThenShift();
    }



}


