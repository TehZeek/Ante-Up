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
    public GameObject FadeIn;
    public GameObject ActionScreenScene;
    public List<GameObject> HUDs = new List<GameObject>();
    private GameManager gameManager;
    public List<Transform> hubTransform = new List<Transform>();
    private PokerTableCards pokerTableCards;
    private PokerTurnManager pokerTurnManager;
    private ShiftScene shiftScene;


    void Start()
    {
        FadeIn.GetComponent<ActionScreen>().TextEffect("Loading...", FadeIn.GetComponent<ActionScreen>().showdownText);
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
        FadeIn.GetComponent<ActionScreen>().TextEffect("", FadeIn.GetComponent<ActionScreen>().showdownText);
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

    public void TurnOnHUD()
    {
        foreach (GameObject hudPanel in HUDs)
            ShowUiObject(hudPanel);

        foreach (GameObject tableCard in pokerTableCards.tableHand)
            ShowUiObject(tableCard);

        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        if (battleMenu != null)
            ShowUiObject(battleMenu.gameObject);
    }

    private static void ShowUiObject(GameObject uiObject)
    {
        if (uiObject == null) return;

        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;   // fully opaque
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void ShowdownTime()
    {
        TurnOffHUD();
        shiftScene.ShowDownShift();
        ActionScreenScene.GetComponent<ActionScreen>().GetManagers();
        ActionScreenScene.GetComponent<ActionScreen>().ShowdownSetup();
    }

        /// <summary>
/// Clears all showdown-only state and prepares every system for the next hand.
/// Must be called exactly once after the pot is settled and BEFORE new cards
/// are dealt.
/// </summary>
public void CleanUpShowdown()
    {
        //---------------------------------------------------------------------
        // 1️⃣  Update persistent data that carries across hands
        //---------------------------------------------------------------------
        // – player chip stacks were already modified during damage / reward
        //   animations, so nothing to do here.
        // – propagate "IsDead" flags to the master list in GameManager
        PokerDrawPile pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        pokerDrawPile.Reshuffle();
        for (int seat = 0; seat < 4; seat++)
            gameManager.characters[seat].isDead = FadeIn.GetComponent<ActionScreen>().IsDead[seat];
        //---------------------------------------------------------------------
        // 2️⃣  Reset PokerTurnManager to its pre-hand defaults
        //---------------------------------------------------------------------
        pokerTurnManager.turnOrder[0]++;                   // monster → next encounter
        pokerTurnManager.turnOrder[1] = 0;                // round counter (pre-flop)
        pokerTurnManager.turnOrder[2] = 0;                // action counter
        pokerTurnManager.playersStillIn.Clear();

        for (int i = 0; i<4; i++)
        {
            if (gameManager.characters[i].isDead) pokerTurnManager.IsOut[i] = true;
            else pokerTurnManager.IsOut[i] = false;
            pokerTurnManager.isAllIn[i] = false;
            pokerTurnManager.HasChecked[i] = false;
        }

        pokerTurnManager.isShowdownInProgress = false;

        //---------------------------------------------------------------------
        // 3️⃣  Wipe the table and pocket visuals
        //---------------------------------------------------------------------
        pokerTableCards.ClearTable();                       // destroys community card GOs
        pokerTableCards.showdownLayoutActive = false;       // restore normal spacing

        //---------------------------------------------------------------------
        // 4️⃣  Reset the ActionScreen & HUD
        //---------------------------------------------------------------------
        FadeIn.GetComponent<ActionScreen>().FlingAllInCardsOffScreen();  // any late survivors
        FadeIn.GetComponent<ActionScreen>().ClearShowdownUI();           // hides results text, etc.


        //---------------------------------------------------------------------
        // 5️⃣  Tell DeckManager to reshuffle and deal a fresh hand
        //---------------------------------------------------------------------

        TurnOnHUD();
        for (int i = 0; i < 4; i++) HUDs[i].GetComponent<HUD>().ClearHUD();
        pokerTurnManager.TurnAssign();
        StartCoroutine(LoadWaitThenShift());
    }



}


