using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZeekSpace;

public class BattleManager : MonoBehaviour
{
    public Monster monster;
    public GameObject hubPrefab;
    public List<GameObject> HUDs = new List<GameObject>();
    private GameManager gameManager;
    private PokerTableCards pokerTableCards;
    private PokerTurnManager pokerTurnManager;
    public List<Transform> hubTransform = new List<Transform>();

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();

        //make the huds
        GameObject newHUDMon = Instantiate(hubPrefab, hubTransform[0].position, Quaternion.identity, hubTransform[0]);
        newHUDMon.GetComponent<HUD>().monsterHud = monster;
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
        Debug.Log("We made " + HUDs.Count + " HUDs");
    }

    public void UpdateHUD()
    {
        HUDs[0].GetComponent<HUD>().RefreshHUD(pokerTableCards.monsterPocket, 0);
        HUDs[1].GetComponent<HUD>().RefreshHUD(pokerTableCards.playerOnePocket, 1);
        HUDs[2].GetComponent<HUD>().RefreshHUD(pokerTableCards.playerTwoPocket, 2);
        HUDs[3].GetComponent<HUD>().RefreshHUD(pokerTableCards.playerThreePocket, 3);
    }

    public enum MonsterChoice
        {
        Bet, Raise, Call, Check, Fold, AllIn, Flee
        }

    public MonsterChoice MonsterDecision()
    {
        //monster AI (Scared, Brave, Bluffs, Neutral, Random
        //look at current cards, money, what's on the table
        //pass a MonsterChoice enum
        PokerHandCompare pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        int HandScore;
        int PossibleHands;
        int MinimumHandType = pokerHandCompare.allHandTypes.IndexOf(monster.minimumHand);
        int CurrentHandType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand);
        int MonsterHand;
        int MonsterRank;
        if (MinimumHandType < CurrentHandType) 
        { 
            MonsterHand = CurrentHandType;
            MonsterRank = pokerHandCompare.monRank[0];
        }
        else if (MinimumHandType == CurrentHandType) 
            {
                if (monster.minimumRank > pokerHandCompare.monRank[0]) 
                {
                     MonsterHand = MinimumHandType;
                     MonsterRank = monster.minimumRank;
                }
                else
                {
                     MonsterHand = CurrentHandType;
                     MonsterRank = pokerHandCompare.monRank[0];
                }
            }
        else
        {
            MonsterHand = MinimumHandType;
            MonsterRank = monster.minimumRank;
        }

        HandScore = (MonsterHand * 15) + MonsterRank;

        if (pokerTurnManager.turnOrder[1]>1)
        {
            //lets see what potential thing there could be
        }



        return 0;
    }






    // set up the monster
    // set up the battle/table
    // set up the monster logic

}
