using UnityEngine;
using System.Collections.Generic;
using ZeekSpace;
using System.Linq;
using System;

public class PokerHandCompare : MonoBehaviour
{
    private PokerTableCards pokerTableCards;
    public List<HandTypes> allHandTypes = new List<HandTypes>();
    public HandTypes P1Hand, P2Hand, P3Hand, MonHand, bestHandType, MinHand;
    public List<Card> bestHand = new();
    public List<Card> p1Hand = new(), p2Hand = new(), p3Hand = new(), monHand = new();
    public List<int> p1Rank = new(), p2Rank = new(), p3Rank = new(), monRank = new(), handRank = new();

    private List<Card> currentHand = new();

    public List<string> cardRank = new() {
        "Woe", "Two", "Three", "Four", "Five", "Six",
        "Seven", "Eight", "Nine", "Ten", "Jack", "Queen",
        "King", "Ace"
    };

    void Start()
    {
        allHandTypes = Resources.LoadAll<HandTypes>("HandRanks").OrderBy(h => h.handRank).ToList();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        MinHand = gameManager.monster.minimumHand;
    }

    public void UpdateHandType(List<GameObject> pocket, List<GameObject> table, int player)
    {
        ClearHandData(player);
        var combined = new List<GameObject>(pocket);
        combined.AddRange(table);

        for (int i = 0; i<combined.Count; i++)
        {
            Card tempCard = combined[i].GetComponent<CardDisplay>().cardData;
            currentHand.Add(tempCard);
        }
        Debug.Log("Found Cards: " + currentHand.Count);
        currentHand = currentHand.OrderByDescending(card => card.cardRank.First()).ToList();
        

        // Evaluate hands in descending order of strength
        if (TryEvaluateStraightFlush() || TryEvaluateFourOfAKind() || TryEvaluateFullHouse() || TryEvaluateFlush() || TryEvaluateStraight()
            || TryEvaluateThreeOfAKind() || TryEvaluateTwoPair() || TryEvaluatePair())
        {
            UpdateHandToCompare(bestHandType, handRank, bestHand, player);
            return;
        }

        // High card fallback
        bestHand = currentHand.Take(5).ToList();
        handRank = bestHand.Select(card => (int)card.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[1];
        Debug.Log("Sending #bestHand, #handRank, HandType "+bestHand.Count+handRank.Count+bestHandType.name);

        UpdateHandToCompare(bestHandType, handRank, bestHand, player);
        Debug.Log($"[PostUpdateHandToCompare] Player {player} has {pocket.Count} pocket cards and {table.Count} table cards.");

    }

    private bool TryEvaluateStraightFlush()
    {
        // Group by suit and filter to flush-eligible groups (5+ cards)
        var flushGroups = currentHand
            .GroupBy(c => c.cardSuit.First())
            .Where(g => g.Count() >= 5);

        foreach (var group in flushGroups)
        {
            var suitedCards = group.OrderByDescending(c => c.cardRank.First()).ToList();
            var ranks = suitedCards.Select(c => (int)c.cardRank.First()).Distinct().OrderByDescending(r => r).ToList();

            if (ranks.Contains(13)) ranks.Insert(0, 0); // Ace-low support (Ace = 13, Ace-low = 0)

            for (int i = 0; i <= ranks.Count - 5; i++)
            {
                var seq = ranks.Skip(i).Take(5).ToList();
                if (seq.Zip(seq.Skip(1), (a, b) => a - b == 1).All(b => b))
                {
                    bestHand = suitedCards.Where(c => seq.Contains((int)c.cardRank.First()))
                                          .OrderByDescending(c => c.cardRank.First())
                                          .Take(5)
                                          .ToList();

                    handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
                    while (handRank.Count < 5) handRank.Add(0);

                    bestHandType = seq.Max() == 13
                        ? allHandTypes[10] // Royal Flush
                        : allHandTypes[9];  // Straight Flush

                    return true;
                }
            }
        }

        return false;
    }

    private bool TryEvaluateFourOfAKind()
    {
        var group = currentHand.GroupBy(c => c.cardRank.First()).FirstOrDefault(g => g.Count() == 4);
        if (group == null) return false;

        bestHand = group.ToList();
        bestHand.AddRange(currentHand.Except(bestHand).Take(1));

        handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[8];
        return true;
    }

    private bool TryEvaluateFlush()
    {
        var flushGroup = currentHand.GroupBy(c => c.cardSuit.First()).FirstOrDefault(g => g.Count() >= 5);
        if (flushGroup == null) return false;

        bestHand = flushGroup.OrderByDescending(c => c.cardRank.First()).Take(5).ToList();
        handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[6];
        return true;
    }

    private bool TryEvaluateStraight()
    {
        var ranks = currentHand.Select(c => (int)c.cardRank.First()).Distinct().OrderByDescending(r => r).ToList();
        if (ranks.Contains(13)) ranks.Insert(0, 0); // Ace low straight

        for (int i = 0; i <= ranks.Count - 5; i++)
        {
            var seq = ranks.Skip(i).Take(5).ToList();
            if (seq.Zip(seq.Skip(1), (a, b) => a - b == 1).All(b => b))
            {
                bestHand = currentHand.Where(c => seq.Contains((int)c.cardRank.First()))
                                      .OrderByDescending(c => c.cardRank.First())
                                      .Take(5).ToList();
                handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
                while (handRank.Count < 5) handRank.Add(0);
                bestHandType = allHandTypes[5];
                return true;
            }
        }
        return false;
    }

    private bool TryEvaluateFullHouse()
    {
        var three = currentHand.GroupBy(c => c.cardRank.First()).Where(g => g.Count() >= 3)
                                .OrderByDescending(g => g.Key).FirstOrDefault();
        var pair = currentHand.GroupBy(c => c.cardRank.First()).Where(g => g.Count() >= 2 && g.Key != three?.Key)
                               .OrderByDescending(g => g.Key).FirstOrDefault();

        if (three == null || pair == null) return false;
        bestHand = three.Take(3).Concat(pair.Take(2)).ToList();
        handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[7];
        return true;
    }

    private bool TryEvaluateThreeOfAKind()
    {
        var three = currentHand.GroupBy(c => c.cardRank.First()).Where(g => g.Count() == 3)
                                .OrderByDescending(g => g.Key).FirstOrDefault();
        if (three == null) return false;

        bestHand = three.ToList();
        bestHand.AddRange(currentHand.Except(bestHand).Take(2));
        handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[4];
        return true;
    }

    private bool TryEvaluateTwoPair()
    {
        var pairs = currentHand.GroupBy(c => c.cardRank.First()).Where(g => g.Count() >= 2)
                                .OrderByDescending(g => g.Key).Take(2).ToList();
        if (pairs.Count < 2) return false;

        bestHand = pairs.SelectMany(g => g.Take(2)).ToList();
        bestHand.AddRange(currentHand.Except(bestHand).Take(1));
        handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[3];
        return true;
    }

    private bool TryEvaluatePair()
    {
        var pair = currentHand.GroupBy(c => c.cardRank.First()).Where(g => g.Count() == 2)
                               .OrderByDescending(g => g.Key).FirstOrDefault();
        if (pair == null) return false;

        bestHand = pair.ToList();
        bestHand.AddRange(currentHand.Except(bestHand).Take(3));
        handRank = bestHand.Select(c => (int)c.cardRank.First()).ToList();
        while (handRank.Count < 5) handRank.Add(0);
        bestHandType = allHandTypes[2];
        return true;
    }

    private void ClearHandData(int player)
    {
        currentHand.Clear();
        bestHand.Clear();
        handRank.Clear();

        if (player == 0) { monHand.Clear(); monRank.Clear(); MonHand = allHandTypes[0]; }
        if (player == 1) { p1Hand.Clear(); p1Rank.Clear(); P1Hand = allHandTypes[0]; }
        if (player == 2) { p2Hand.Clear(); p2Rank.Clear(); P2Hand = allHandTypes[0]; }
        if (player == 3) { p3Hand.Clear(); p3Rank.Clear(); P3Hand = allHandTypes[0]; }
    }

    private void UpdateHandToCompare(HandTypes handType, List<int> rank, List<Card> cards, int player)
    {
        while (rank.Count < 5) rank.Add(0);
        switch (player)
        {
            case 0: monHand = cards.ToList(); monRank = rank.ToList(); MonHand = handType; break;
            case 1: p1Hand = cards.ToList(); p1Rank = rank.ToList(); P1Hand = handType; break;
            case 2: p2Hand = cards.ToList(); p2Rank = rank.ToList(); P2Hand = handType; break;
            case 3: p3Hand = cards.ToList(); p3Rank = rank.ToList(); P3Hand = handType; break;
        }

    }

    public string HandToString(int player)
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        HandTypes MinHand = gameManager.monster.minimumHand;
        List<int> MinRank = new List<int>() { (gameManager.monster.minimumRank - 1) };
        var hand = player switch
        {
            0 => MonHand,
            1 => P1Hand,
            2 => P2Hand,
            3 => P3Hand,
            4 => MinHand,
            _ => allHandTypes[0]
        };

        var rank = player switch
        {
            0 => monRank,
            1 => p1Rank,
            2 => p2Rank,
            3 => p3Rank,
            4 => MinRank,
            _ => new List<int>()
        };

        if (hand == allHandTypes[1]) return $"{cardRank[rank[0]]} High";
        if (hand == allHandTypes[2]) return $"Pair of {cardRank[rank[0]]}s";
        if (hand == allHandTypes[3]) return $"Two Pair: {cardRank[rank[0]]}s and {cardRank[rank[2]]}s";
        if (hand == allHandTypes[4]) return $"Three of a Kind: {cardRank[rank[0]]}s";
        if (hand == allHandTypes[5]) return $"Straight to {cardRank[rank[0]]}";
        if (hand == allHandTypes[6]) return $"Flush: {cardRank[rank[0]]} high";
        if (hand == allHandTypes[7]) return $"Full House: {cardRank[rank[0]]}s over {cardRank[rank[3]]}s";
        if (hand == allHandTypes[8]) return $"Four of a Kind: {cardRank[rank[0]]}s";
        if (hand == allHandTypes[9]) return $"Straight Flush to: {cardRank[rank[0]]}";
        if (hand == allHandTypes[10]) return $"Royal Flush";

        return $"Unknown";
    }

}
