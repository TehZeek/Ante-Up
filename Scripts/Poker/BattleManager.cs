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
        Bet, Call, Check, Fold, Flee
        }

    public MonsterChoice MonsterDecision()
    {
        //monster AI (Scared, Brave, Bluffs, Neutral, Random
        //look at current cards, money, what's on the table
        //pass a MonsterChoice enum
        PokerHandCompare pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        int HandScore;
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

        HandScore = ((MonsterHand -1) * 15) + MonsterRank;
        HandScore += BonusHandPoints();
        int temp = TablePoints();
        if (temp < HandScore)
        {
            HandScore -= temp;
        }

        int roundsLeft = RoundMultiplier();
        roundsLeft *= 5;
        HandScore += roundsLeft;

        //BET    CALL    CHECK    FOLD
        // subtract for each power enemy uses
        // add for each one we use, maybe
        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        if (HandScore < 20 && battleMenu.BetIsSet) 
        { 
            int bluffmaybe = Random.Range(1, 10);
            if (bluffmaybe < 3) { return MonsterChoice.Bet; }
            else { return MonsterChoice.Fold; } 
        }

        if (HandScore < 40) {
            int bluffmaybe = Random.Range(1, 10);
            if (bluffmaybe < 3) { return MonsterChoice.Bet; }
            else { return MonsterChoice.Check; }
        }

        if (HandScore > 40 && battleMenu.BetIsSet) { return MonsterChoice.Call; }
        if (HandScore > 60) { return MonsterChoice.Bet; }
        return MonsterChoice.Fold;
    }



    private int TablePoints()
    {
        int multiplier = RoundMultiplier();
        int bonusPoints = 0;
        List<Card> currentHand = new List<Card>();
        for (int i = 0; i < pokerTableCards.tableHand.Count; i++)
        {
            Card temp = pokerTableCards.tableHand[i].gameObject.GetComponent<CardDisplay>().cardData;
            currentHand.Add(temp);
        }

        var rankedGroups = currentHand.GroupBy(card => card.cardRank.First())
                                     .Select(group => new { Rank = group.Key, Count = group.Count() })
                                     .ToList();
        foreach (var group in rankedGroups)
        {
            if (group.Count == 2)
            {
                bonusPoints -= 5;
            }
            if (group.Count == 3)
            {
                bonusPoints -= 15;
            }
            if (group.Count == 4)
            {
                bonusPoints -= 50;
            }

        }
        var flush = currentHand.GroupBy(card => card.cardSuit.First())
                .Where(group => group.Count() > 2)
                .Select(group => new { Rank = group.Key, Count = group.Count() })
                                      .ToList();
        foreach (var group in flush)
        {
            if (group.Count == 2)
            {
                bonusPoints -= 1;
            }
            if (group.Count == 3)
            {
                bonusPoints -= 10;
            }
            if (group.Count == 4)
            {
                bonusPoints -= 25;
            }
            if (group.Count == 5)
            {
                bonusPoints -= 35;
            }
        }

        List<int> rankValues = currentHand
                .Select(card => (int)card.cardRank.First())
                .Distinct()  // Ensures only unique ranks are considered
                .OrderBy(rank => rank)
                .ToList();
        int howManyStraight = 0;
        for (int i = 0; i <= rankValues.Count - 2; i++)
        {
            if (rankValues[i + 1] == rankValues[i] + 1)
            {
                bonusPoints -= 2;
                howManyStraight++;
            }
            else if (rankValues[i + 1] == rankValues[i] + 2)
            {
                bonusPoints -= 1;
                howManyStraight++;
            }
            if (howManyStraight == 3)
            {
                bonusPoints-= 10;
            }
            if (howManyStraight >= 4)
            {
                bonusPoints -= 20;
            }
        }
        bonusPoints *= multiplier;
        return bonusPoints;

    }

    private int RoundMultiplier()
    {
        int roundDivider = 3;
        if (pokerTurnManager.HasABonusRound) { roundDivider++; }
        if (pokerTurnManager.HasARiver) { roundDivider++; }
        if (pokerTurnManager.turnOrder[1] <= roundDivider) { roundDivider -= pokerTurnManager.turnOrder[1]; }
        else { roundDivider = 0; }
        return roundDivider;
    }

    private int BonusHandPoints()
    {
        //make a multiplier based on how many rounds are left
        int multiplier = RoundMultiplier();
        int bonusPoints = 0;
            PokerHandCompare pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        List<Card> currentHand = new List<Card>();
        //get all the cards
        for (int i = 0; i < pokerTableCards.monsterPocket.Count; i++)
        {
            Card temp = pokerTableCards.monsterPocket[i].gameObject.GetComponent<CardDisplay>().cardData;
            currentHand.Add(temp);
        }
        for (int i = 0; i < pokerTableCards.tableHand.Count; i++)
        {
            Card temp = pokerTableCards.tableHand[i].gameObject.GetComponent<CardDisplay>().cardData;
            currentHand.Add(temp);
        }
        //check pairs
        var rankedGroups = currentHand.GroupBy(card => card.cardRank.First())
                                      .Select(group => new { Rank = group.Key, Count = group.Count() })
                                      .ToList();
        foreach (var group in rankedGroups)
        {
            if (group.Count == 2)
            {
                bonusPoints += 5;
            }
            if (group.Count == 3)
            {
                bonusPoints += 15;
            }
            if (group.Count == 4)
            {
                bonusPoints += 50;
            }

        }
        var flush = currentHand.GroupBy(card => card.cardSuit.First())
                .Where(group => group.Count() > 2)
                .Select(group => new { Rank = group.Key, Count = group.Count() })
                                      .ToList();
        foreach (var group in flush)
        {
            if (group.Count == 2)
            {
                bonusPoints += 1;
            }
            if (group.Count == 3)
            {
                bonusPoints += 8;
            }
            if (group.Count == 4)
            {
                bonusPoints += 20;
            }
            if (group.Count == 5)
            {
                bonusPoints += 35;
            }
        }

        List<int> rankValues = currentHand
                .Select(card => (int)card.cardRank.First())
                .Distinct()  // Ensures only unique ranks are considered
                .OrderBy(rank => rank)
                .ToList();
        int howManyStraight = 0;
        for (int i = 0; i <= rankValues.Count-2; i++)
        {
            if (rankValues[i + 1] == rankValues[i] + 1) 
            { 
                bonusPoints += 2;
                howManyStraight++;
            }
            else if (rankValues[i + 1] == rankValues[i] + 2)
            { 
                bonusPoints += 2;
                howManyStraight++;
            }
            if (howManyStraight == 3)
            {
                bonusPoints += 5;
            }
            if (howManyStraight >= 4)
            {
                bonusPoints += 10;
            }
        }
        bonusPoints *= multiplier;
        return bonusPoints;

    }




    // set up the monster
    // set up the battle/table
    // set up the monster logic

}
