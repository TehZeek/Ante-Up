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
    public Transform FadeInSpot;
    public List<GameObject> HUDs = new List<GameObject>();
    private GameManager gameManager;
    public List<Transform> hubTransform = new List<Transform>();
    private PokerTableCards pokerTableCards;
    private PokerTurnManager pokerTurnManager;



    void Start()
    {
        FadeIn = Instantiate(LoadScreenPrefab, FadeInSpot.position, Quaternion.identity, FadeInSpot);
        gameManager = FindFirstObjectByType<GameManager>();
        monsterManager.SetUpMonster();

        MakeHUDs();

        PokerChipManager pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerChipManager.StartChips();

        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        deckManager.BattleSetup();

        ShiftScene shiftScene = FindFirstObjectByType<ShiftScene>();
        shiftScene.BuildScenes();


        // build things: Scenes, draw deck
        // assign things: monster AI, chips, turn order



        // Raise Load Screen

        Debug.Log("Calling FadeSplash");

        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        battleMenu.UpdateButtonDisplay(0);

        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerTurnManager.stillThisTurn = true;

        FadeIn.GetComponent<FadeOutAndDestroy>().FadeSplash();
        //make the huds

        Debug.Log("We made " + HUDs.Count + " HUDs");

    }

    private void MakeHUDs()
    {
        GameObject newHUDMon = Instantiate(hubPrefab, hubTransform[0].position, Quaternion.identity, hubTransform[0]);
        newHUDMon.GetComponent<HUD>().monsterHud = monsterManager.monster;
        newHUDMon.GetComponent<HUD>().isCharacter = false;
        newHUDMon.GetComponent<HUD>().MakeHUD();
        HUDs.Add(newHUDMon);

        for (int i = 0; i < (gameManager.characters.Count); i++)
        {
            GameObject newHUD = Instantiate(hubPrefab, hubTransform[i + 1].position, Quaternion.identity, hubTransform[i + 1]);
            newHUD.GetComponent<HUD>().characterHud = gameManager.characters[i];
            newHUD.GetComponent<HUD>().isCharacter = true;
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



    




    // set up the monster
    // set up the battle/table
    // set up the monster logic

}
