using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PokerHandCompare : MonoBehaviour
{
    public List<HandTypes> allHandTypes = new List<HandTypes>();
    public HandTypes P1Hand;
    public HandTypes P2Hand;
    public HandTypes P3Hand;
    public HandTypes MonHand;
    private PokerTableCards pokerTableCards;
    public List<Card> currentHand = new List<Card>();


    void Start()
    {
        HandTypes[] hands = Resources.LoadAll<HandTypes>("HandRanks");
        allHandTypes.AddRange(hands);
        allHandTypes = allHandTypes.OrderBy(hand => hand.handRank).ToList();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
    }

    public void UpdateHandType(List<GameObject> pocket, List<GameObject> table, int player, int round)
    {
        bool hasStraight=false;
        bool hasPair = false;
        bool hasFlush = false;
        bool hasThree = false;
        bool hasTwoPair = false;
        bool hasThreePair = false;
        bool hasTwoThree = false;
        bool hasFour = false;
        bool hasFive = false;
        bool hasSix = false;
        bool hasRoyal = true;

        currentHand.Clear();
        for (int i = 0; i < pocket.Count; i++)
        {
            Card temp = pocket[i].gameObject.GetComponent<CardDisplay>().cardData; 
            currentHand.Add(temp);
        }
        for (int i = 0; i < table.Count; i++)
        {
            Card temp = table[i].gameObject.GetComponent<CardDisplay>().cardData;
            currentHand.Add(temp);
        }
        currentHand = currentHand.OrderBy(card => card.cardRank.First()).ToList();


        // this checks the card list for pairs
        var rankedGroups = currentHand.GroupBy(card => card.cardRank.First())
                                      .Select(group => new { Rank = group.Key, Count = group.Count() })
                                      .ToList();

        // Detect different types of duplicates
        foreach (var group in rankedGroups)
        {
            if (group.Count == 2)
            {
                Debug.Log($"Pair found: {group.Rank}");
                if (hasTwoPair) 
                    { hasThreePair = true; }
                if (hasPair) 
                    {hasTwoPair = true;}
                hasPair = true;
            }
            else if (group.Count == 3)
            {
                if(hasThree)
                { hasTwoThree = true; }
                Debug.Log($"Three of a Kind found: {group.Rank}");
            }
            else if (group.Count == 4)
            {
                hasFour = true;
                Debug.Log($"Four of a Kind found: {group.Rank}");
            }
            else if (group.Count == 5)
            {
                hasFive = true;
                Debug.Log($"Five of a Kind found: {group.Rank}");
            }
            else if (group.Count == 6)
            {
                hasSix = true;
                Debug.Log($"Six of a Kind found: {group.Rank}");
            }
        }

        //detect flush
        var flush = currentHand.GroupBy(card => card.cardSuit.First())
                        .Where(group => group.Count() > 4)
                        .Select(group => new { Rank = group.Key, Count = group.Count() })
                        .ToList();


       
        foreach (var group in flush)
        {
            Debug.Log($"Found {group.Count} of {group.Rank}");
            hasFlush = true;
        }


        //straight
        List<int> rankValues = currentHand
                        .Select(card => (int)card.cardRank.First())
                        .Distinct()  // Ensures only unique ranks are considered
                        .OrderBy(rank => rank)
                        .ToList();

        // Special case: Ace-low straight (Ace-2-3-4-5)
        if (rankValues.Contains((int)Card.CardRank.Ace) &&
            rankValues.Contains((int)Card.CardRank.King) &&
            rankValues.Contains((int)Card.CardRank.Queen) &&
            rankValues.Contains((int)Card.CardRank.Jack) &&
            rankValues.Contains((int)Card.CardRank.Ten))
        {
            Debug.Log("Ace-high straight found: Ace, K, A, J, 10");
            hasStraight = true;
            hasRoyal = true;
        }


        for (int i = 0; i <= rankValues.Count - 5; i++)
        {
            if (rankValues[i + 1] == rankValues[i] + 1 &&
                rankValues[i + 2] == rankValues[i] + 2 &&
                rankValues[i + 3] == rankValues[i] + 3 &&
                rankValues[i + 4] == rankValues[i] + 4)
            {
                Debug.Log($"Straight found: {currentHand[i].cardRank.First} to {currentHand[i + 4].cardRank}");
                hasStraight = true;
            }
        }


        //will need to update for wilds and weirdness and extra cards but hey 

        //detect hand type and populate the five cards being used


    }











}
