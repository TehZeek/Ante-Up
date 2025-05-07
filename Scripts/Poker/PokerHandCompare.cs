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
        var pairs = currentHand.GroupBy(card => card.cardRank.First())
                                .Where(group => group.Count() > 1)
                                .Select(group => new { Rank = group.Key, Count = group.Count() })
                                .ToList();

        foreach (var group in pairs)
        {
            Debug.Log($"Found {group.Count} of {group.Rank}");
        }
        //can use this with cardSuit for flushes, set to 5
        //then go through the list



    }











}
