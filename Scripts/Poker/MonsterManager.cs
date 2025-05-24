using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZeekSpace;
using System.Linq;


public class MonsterManager : MonoBehaviour
{
    private int bluffRange;
    private int foldRange;
    private int callRange;
    private int betRange;
    public Monster monster;

    public void SetUpMonster()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        monster = gameManager.monster;
        int aiRank = (int)monster.monsterAI;
        if (aiRank == 0)
        {
            //scared
            bluffRange = 2;
            foldRange = 30;
            callRange = 45;
            betRange = 70;
        }
        else if (aiRank == 1)
        {
            //brave
            bluffRange = 4;
            foldRange = 15;
            callRange = 35;
            betRange = 50;
        }
        else if (aiRank == 2)
        {
            //bluffer
            bluffRange = 5;
            foldRange = 30;
            callRange = 50;
            betRange = 75;
        }
        else
        {
            bluffRange = 3;
            foldRange = 20;
            callRange = 40;
            betRange = 60;
        }
    }

    public enum MonsterChoice
    {
        Bet, Call, Check, Fold, Flee
    }

    public int MonsterDecision()
    {
        //monster AI (Scared, Brave, Bluffs, Neutral, Random)
        //look at current cards, money, what's on the table
        //pass a MonsterChoice enum
        
        int HandScore;

        (int MonsterHand, int MonsterRank)= MonsterHandOrMinimumHand();
        
        HandScore = ((MonsterHand - 1) * 15) + MonsterRank;
        Debug.Log("Monster Hand Score, without potential: " + HandScore);
        HandScore += BonusHandPoints();
        int temp = TablePoints();
        HandScore += temp;

        int roundsLeft = RoundMultiplier();
        roundsLeft *= 5;
        Debug.Log("Rounds remaining adding: " + roundsLeft);

        HandScore += roundsLeft;

        //BET    CALL    CHECK    FOLD
        // subtract for each power enemy uses
        // add for each one we use, maybe
        BattleMenu battleMenu = FindFirstObjectByType<BattleMenu>();
        Debug.Log("Monster Hand Score: " + HandScore);
        if (HandScore < foldRange && battleMenu.BetIsSet)
        {
            int bluffmaybe = Random.Range(1, 10);
            if (bluffmaybe < bluffRange)
            {
                Debug.Log("Monster is Bluffing: " + bluffmaybe);
                return 0;
            }
            else { return 3; }
        }

        else if (HandScore < callRange)
        {
            int bluffmaybe = Random.Range(1, 10);
            if (bluffmaybe < bluffRange)
            {
                Debug.Log("Monster is Bluffing: " + bluffmaybe);
                return 0;
            }
            else { return 2; }
        }

        else if (HandScore > callRange && battleMenu.BetIsSet) { return 1; }
        else if (HandScore > betRange) { return 0; }

        else return 3;
    }

    public (int, int) MonsterHandOrMinimumHand()
    {
        PokerHandCompare pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        int MinimumHandType = pokerHandCompare.allHandTypes.IndexOf(monster.minimumHand);
        int CurrentHandType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand);
        int Type;
        int Rank;
        if (MinimumHandType < CurrentHandType)
        {
            Type = CurrentHandType;
            Rank = pokerHandCompare.monRank[0];
        }
        else if (MinimumHandType == CurrentHandType)
        {
            if (monster.minimumRank > pokerHandCompare.monRank[0])
            {
                Type = MinimumHandType;
                Rank = monster.minimumRank;
            }
            else
            {
                Type = CurrentHandType;
                Rank = pokerHandCompare.monRank[0];
            }
        }
        else
        {
            Type = MinimumHandType;
            Rank = monster.minimumRank;
        }
        return (Type, Rank);
    }

    private int TablePoints()
    {
        PokerTableCards pokerTableCards = FindFirstObjectByType<PokerTableCards>();

        int multiplier = RoundMultiplier();
        multiplier += 2;
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
                bonusPoints -= 10;
            }
            if (howManyStraight >= 4)
            {
                bonusPoints -= 20;
            }
        }
        bonusPoints *= multiplier;
        Debug.Log("Table potential removing: " + bonusPoints);
        return bonusPoints;

    }

    private int RoundMultiplier()
    {
        PokerTurnManager pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();

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
        PokerTableCards pokerTableCards = FindFirstObjectByType<PokerTableCards>();
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
        for (int i = 0; i <= rankValues.Count - 2; i++)
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
        Debug.Log("Hand potential adding: " + bonusPoints);

        return bonusPoints;

    }


}
