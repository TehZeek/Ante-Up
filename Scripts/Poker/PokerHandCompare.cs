using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class PokerHandCompare : MonoBehaviour
{
    private PokerTableCards pokerTableCards;
    public List<HandTypes> allHandTypes = new List<HandTypes>();
    public HandTypes P1Hand;
    public HandTypes P2Hand;
    public HandTypes P3Hand;
    public HandTypes MonHand;
    public HandTypes bestHandType;
    private List<Card> currentHand = new List<Card>();
    public List<Card> bestHand;
    public List<Card> p1Hand = new List<Card>();
    public List<Card> p2Hand = new List<Card>();
    public List<Card> p3Hand = new List<Card>();
    public List<Card> monHand = new List<Card>();
    public List<int> p1Rank;
    public List<int> p2Rank;
    public List<int> p3Rank;
    public List<int> monRank;
    public List<int> handRank;
    public List<string> cardRank = new List<string>
{    "Woe", "Two", "Three", "Four", "Five", "Six",
    "Seven", "Eight", "Nine", "Ten", "Jack", "Queen",
    "King", "Ace"};
    private bool hasPair = false;
    private bool hasTwoPair = false;
    private bool hasThree = false;
    private bool hasStraight = false;
    private bool hasFlush = false;
    private bool hasFour = false;

    void Start()
    {
        HandTypes[] hands = Resources.LoadAll<HandTypes>("HandRanks");
        allHandTypes.AddRange(hands);
        allHandTypes = allHandTypes.OrderBy(hand => hand.handRank).ToList();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
    }

    public void UpdateHandType(List<GameObject> pocket, List<GameObject> table, int player)
    {
        ClearHandData(player);
        ClearRankBools();

        //get the hand and table and combine them into a currentHand list
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

        PairCheck();
        FlushCheck();
        StraightCheck();

        // with bools turned on, we clean up the hand based on it's rank
        if (hasFour)
            {
                // Extract four of a kind plus the highest kicker
                var fourKindGroup = currentHand.GroupBy(card => card.cardRank.First())
                                               .Where(group => group.Count() == 4)
                                               .FirstOrDefault(); // Get the first match

                if (fourKindGroup != null)
                {
                    // Assign the rank of the Four of a Kind
                    handRank[0] = (int)fourKindGroup.Key;

                    // Extract the four cards
                    bestHand = fourKindGroup.ToList();

                    // Find the highest kicker not part of the four-of-a-kind
                    var kickerCard = currentHand.Except(bestHand)
                                                .OrderByDescending(card => card.cardRank.First())
                                                .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[1] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }
                }
                bestHandType = allHandTypes[8];
                UpdateHandToCompare(bestHandType, handRank, bestHand, player);
                return;
            }

        if (hasFlush)
        {
            // Isolate the flush cards
            int highestStraightFlushRank = 0;

            // Extract all flush-suited cards
            var flushCards = currentHand.GroupBy(card => card.cardSuit.First())
                                        .Where(group => group.Count() >= 5)
                                        .SelectMany(group => group)
                                        .OrderByDescending(card => card.cardRank.First())
                                        .ToList();

            // Convert flush card ranks to a sorted, distinct list
            List<int> flushRankValues = flushCards.Select(card => (int)card.cardRank.First())
                                                  .Distinct()
                                                  .OrderBy(rank => rank)
                                                  .ToList();

            // Check for standard Straight Flush
            for (int i = 0; i <= flushRankValues.Count - 5; i++)
            {
                if (flushRankValues[i + 1] == flushRankValues[i] + 1 &&
                    flushRankValues[i + 2] == flushRankValues[i] + 2 &&
                    flushRankValues[i + 3] == flushRankValues[i] + 3 &&
                    flushRankValues[i + 4] == flushRankValues[i] + 4)
                {
                    highestStraightFlushRank = flushRankValues[i + 4];

                    // Assign hand type based on rank (Royal Flush vs. Straight Flush)
                    bestHandType = (highestStraightFlushRank == 13) ? allHandTypes[10] : allHandTypes[9];
                    handRank[0] = highestStraightFlushRank;

                    break;
                }
            }

            // Check for Ace-low Straight Flush (Ace-2-3-4-5)
            if (highestStraightFlushRank == 0 &&
                flushRankValues.Contains((int)Card.CardRank.Ace) &&
                flushRankValues.Contains((int)Card.CardRank.Two) &&
                flushRankValues.Contains((int)Card.CardRank.Three) &&
                flushRankValues.Contains((int)Card.CardRank.Four) &&
                flushRankValues.Contains((int)Card.CardRank.Five))
            {
                highestStraightFlushRank = 5; // Highest card in an Ace-low straight flush
                bestHandType = allHandTypes[9]; // Assign as a Straight Flush
                handRank[0] = highestStraightFlushRank;

            }

            // If no Straight Flush was found, store the highest Flush card rank
            if (highestStraightFlushRank == 0)
            {
                handRank[0] = (int)flushCards.First().cardRank.First();
                bestHandType = allHandTypes[6]; // Regular Flush
            }

            handRank[1] = 0;
            UpdateHandToCompare(bestHandType, handRank, bestHand, player);
            return;
        }

        if (hasStraight)
            {
                // Convert ranks to a sorted, distinct list
                List<int> straightValues = currentHand
                    .Select(card => (int)card.cardRank.First())
                    .Distinct()
                    .OrderBy(rank => rank)
                    .ToList();

                bool foundStraight = false;

                // Check for standard five-card consecutive straights
                for (int i = straightValues.Count - 5; i >= 0; i--) // Reverse loop to get highest straight first
                {
                    if (straightValues[i] + 1 == straightValues[i + 1] &&
                        straightValues[i] + 2 == straightValues[i + 2] &&
                        straightValues[i] + 3 == straightValues[i + 3] &&
                        straightValues[i] + 4 == straightValues[i + 4])
                    {
                        handRank[0] = straightValues[i + 4]; // Save highest Straight card rank
                        bestHand = currentHand
                            .Where(card => straightValues.GetRange(i, 5).Contains((int)card.cardRank.First()))
                            .OrderByDescending(card => card.cardRank.First())
                            .Take(5)
                            .ToList();

                        
                        foundStraight = true;
                        break;
                    }
                }

                // Special case: Ace-low straight (Ace, 2, 3, 4, 5)
                if (!foundStraight &&
                    straightValues.Contains((int)Card.CardRank.Ace) &&
                    straightValues.Contains((int)Card.CardRank.Two) &&
                    straightValues.Contains((int)Card.CardRank.Three) &&
                    straightValues.Contains((int)Card.CardRank.Four) &&
                    straightValues.Contains((int)Card.CardRank.Five))
                {
                    handRank[0] = 5; // Highest card in an Ace-low straight
                    bestHand = currentHand
                        .Where(card => new List<int> { (int)Card.CardRank.Ace, (int)Card.CardRank.Two,
                                           (int)Card.CardRank.Three, (int)Card.CardRank.Four,
                                           (int)Card.CardRank.Five }.Contains((int)card.cardRank.First()))
                        .OrderByDescending(card => card.cardRank.First())
                        .Take(5)
                        .ToList();

                    


            }
            bestHandType = allHandTypes[5];
            UpdateHandToCompare(bestHandType, handRank, bestHand, player);
                return;
            }

            if (hasThree && hasPair)  //Full House
            {
                // Extract three of a kind 
                var threeKindGroup = currentHand.GroupBy(card => card.cardRank.First())
                                               .Where(group => group.Count() == 3)
                                               .FirstOrDefault(); // Get the first match

                if (threeKindGroup != null)
                {
                    // Assign the rank of the three of a Kind
                    handRank[0] = (int)threeKindGroup.Key;

                    // Extract the three cards
                    bestHand = threeKindGroup.ToList();
                }
                // Extract best pair 

                var lilpair = currentHand.GroupBy(card => card.cardRank.First())
                                   .Where(group => group.Count() == 2)
                                   .FirstOrDefault(); // Get the first match

                if (lilpair != null)
                {
                    // Assign the rank of the pair
                    handRank[1] = (int)lilpair.Key;

                    // Extract the two cards
                    bestHand = lilpair.ToList();
                }

                bestHandType = allHandTypes[7];
                UpdateHandToCompare(bestHandType, handRank, bestHand, player);
                return;
            }

            if (hasThree)
            {
                // Extract three of a kind plus the highest kicker
                var threeKindGroup = currentHand.GroupBy(card => card.cardRank.First())
                                               .Where(group => group.Count() == 3)
                                               .FirstOrDefault(); // Get the first match

                if (threeKindGroup != null)
                {
                    // Assign the rank of the three of a Kind
                    handRank[0] = (int)threeKindGroup.Key;

                    // Extract the three cards
                    bestHand = threeKindGroup.ToList();

                    // Find the highest kicker not part of the four-of-a-kind
                    var kickerCard = currentHand.Except(bestHand)
                                                .OrderByDescending(card => card.cardRank.First())
                                                .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[1] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }
                    // Find the second kicker not part of the four-of-a-kind
                    kickerCard = currentHand.Except(bestHand)
                                                .OrderByDescending(card => card.cardRank.First())
                                                .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[2] = (int)kickerCard.cardRank.First();

                        bestHand.Add(kickerCard); // Add the 2nd kicker to the best hand
                    }
                }
                bestHandType = allHandTypes[4];
                UpdateHandToCompare(bestHandType, handRank, bestHand, player);
                return;
            }

        if (hasTwoPair)
        {
            bestHand.Clear(); // Ensure the best hand list is empty

            // Get the two highest pairs
            var pairGroups = currentHand.GroupBy(card => card.cardRank.First())
                                        .Where(group => group.Count() == 2)
                                        .OrderByDescending(group => group.Key)
                                        .Take(2)
                                        .ToList();

            if (pairGroups.Count == 2)
            {
                handRank[0] = (int)pairGroups[0].Key; // First pair (higher)
                handRank[1] = (int)pairGroups[1].Key; // Second pair (lower)

                // Add both pairs to bestHand
                bestHand.AddRange(pairGroups[0]);
                bestHand.AddRange(pairGroups[1]);

                // Find highest kicker (not part of the pairs)
                var kickerCard = currentHand.Except(bestHand)
                                            .OrderByDescending(card => card.cardRank.First())
                                            .FirstOrDefault();

                if (kickerCard != null)
                {
                    handRank[2] = (int)kickerCard.cardRank.First(); // Store kicker rank
                    bestHand.Add(kickerCard); // Add kicker to best hand
                }
            }

            bestHandType = allHandTypes[3]; // Assign Two Pair hand type
            UpdateHandToCompare(bestHandType, handRank, bestHand, player);
            return;
        }


        if (hasPair)
            {
                //strip to the pair plus 3 highest
                // Extract pairs plus the highest kicker
                var onlyPair = currentHand.GroupBy(card => card.cardRank.First())
                                               .Where(group => group.Count() == 2)
                                               .FirstOrDefault(); // Get the first match

                if (onlyPair != null)
                {
                    // Assign the rank of the three of a Kind
                    handRank[0] = (int)onlyPair.Key;

                    // Extract the three cards
                    bestHand = onlyPair.ToList();

                    // Find the highest kicker not part of the four-of-a-kind
                    var kickerCard = currentHand.Except(bestHand)
                                                .OrderByDescending(card => card.cardRank.First())
                                                .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[1] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }
                    kickerCard = currentHand.Except(bestHand)
                               .OrderByDescending(card => card.cardRank.First())
                               .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[2] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }
                    kickerCard = currentHand.Except(bestHand)
                               .OrderByDescending(card => card.cardRank.First())
                               .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[3] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }

                }
                bestHandType = allHandTypes[2];
                UpdateHandToCompare(bestHandType, handRank, bestHand, player);
                return;
            }
        // Extract the five highest-ranked cards for HighCard
        bestHand = currentHand.OrderByDescending(card => card.cardRank.First())
                              .Take(5)
                              .ToList();

        // Assign ranks to handRank array for comparison
        for (int i = 0; i < bestHand.Count; i++)
        {
            handRank[i] = (int)bestHand[i].cardRank.First();
        }

        // Assign the High Card hand type
        bestHandType = allHandTypes[1];

        // Update the player's hand comparison
        UpdateHandToCompare(bestHandType, handRank, bestHand, player);
        return;

    }


    private void ClearHandData(int playNum)
    {
        currentHand.Clear();
        bestHand.Clear();
        bestHandType = allHandTypes[0];
        handRank.Clear();
        handRank.Add(0);
        handRank.Add(0);
        handRank.Add(0);
        handRank.Add(0);
        handRank.Add(0);

        if (playNum == 0)
        {
            monHand.Clear();
            monRank.Clear();
            MonHand = allHandTypes[0];
        }
        if (playNum == 1)
        {
            p1Hand.Clear();
            p1Rank.Clear();
            P1Hand = allHandTypes[0];
        }
        if (playNum == 2)
        {
            p2Hand.Clear();
            p2Rank.Clear();
            P2Hand = allHandTypes[0];
        }
        if (playNum == 3)
        {
            p3Hand.Clear();
            p3Rank.Clear();
            P3Hand = allHandTypes[0];
        }
    }

    private void ClearRankBools()
    {
     hasPair = false;
     hasTwoPair = false;
      hasThree = false;
      hasStraight = false;
      hasFlush = false;
      hasFour = false;
    }

    private void PairCheck()
    {
        var rankedGroups = currentHand.GroupBy(card => card.cardRank.First())
                                      .Select(group => new { Rank = group.Key, Count = group.Count() })
                                      .ToList();

        foreach (var group in rankedGroups)
        {
            if (group.Count == 2)
            {
                if (hasPair)
                { 
                    hasTwoPair = true;
                }
                hasPair = true;
            }
            if (group.Count == 3)
            {
                hasThree = true;
            }
            if (group.Count == 4)
            {
                hasFour = true;
            }
        }
    }

    private void FlushCheck()
    {
        var flush = currentHand.GroupBy(card => card.cardSuit.First())
                        .Where(group => group.Count() > 4)
                        .Select(group => new { Rank = group.Key, Count = group.Count() })
                        .ToList();

        foreach (var group in flush)
        {
            hasFlush = true;
        }
    }

    private void StraightCheck()
    {
        // Convert card ranks to ints and remove duplicates
        List<int> rankValues = currentHand
            .Select(card => (int)card.cardRank.First()) // Use .First() only if cardRank is a string like "A"
            .Distinct()
            .ToList();

        // Special case: Add '1' for Ace to handle Ace-low straight (A-2-3-4-5)
        if (rankValues.Contains((int)Card.CardRank.Ace))
        {
            rankValues.Add(1); // Ace-low value
        }
        rankValues.Sort();

        // Look for any 5-card sequence
        for (int i = 0; i <= rankValues.Count - 5; i++)
        {
            bool isStraight = true;
            for (int j = 1; j < 5; j++)
            {
                if (rankValues[i + j] != rankValues[i] + j)
                {
                    isStraight = false;
                    break;
                }
            }

            if (isStraight)
            {
                hasStraight = true;
                handRank[0] = rankValues[i + 4]; // Highest card in straight
                handRank[1] = 0;
                return;
            }
        }
    }

    private bool IsTwoPairHand(HandTypes hand) => new[] {
    allHandTypes[3], allHandTypes[7], allHandTypes[12],
    allHandTypes[17], allHandTypes[18], allHandTypes[22], allHandTypes[23]
    }.Contains(hand);

    private bool IsOfAKind(HandTypes hand) => new[] { allHandTypes[4], allHandTypes[8],
    allHandTypes[11], allHandTypes[13], allHandTypes[19], allHandTypes[24], allHandTypes[25]}.Contains(hand);

    private bool IsFlushOrStraight(HandTypes hand) => new[] { allHandTypes[5], allHandTypes[6],
    allHandTypes[9], allHandTypes[15], allHandTypes[16], allHandTypes[20]}.Contains(hand);

    private bool IsACher(HandTypes hand) => new[] { allHandTypes[10], allHandTypes[21], allHandTypes[0] }.Contains(hand);

    public string HandToString(int player)
    {
        HandTypes hand;
        List<int> rank = new List<int>();
        if (player == 0)
        {
            hand = MonHand;
            rank = monRank;
        }
        else if (player == 1)
        {
            hand = P1Hand;
            rank = p1Rank;
        }
        else if (player == 2)
        {
            hand = P2Hand;
            rank = p2Rank;
        }
        else if (player == 3)
        {
            hand = P3Hand;
            rank = p3Rank;
        }
        else if (player == 4)
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            hand = gameManager.monster.minimumHand;
            rank[0] = gameManager.monster.minimumRank;
            rank[1] = 2;
            rank[2] = 2;
            rank[3] = 3;
        }
        else { return "Nope"; }
        string rankString = rank.Count > 0 ? cardRank[rank[0]].ToString() : "??";
        string handString = hand.ToString();
        string rankString2 = cardRank[rank[1]].ToString();
        string rankString3 = cardRank[rank[2]].ToString();

        if (hand == allHandTypes[1])
            return $"{rankString} High";
        if (hand == allHandTypes[2])
            return $"{handString} of {rankString}'s";
        if (IsTwoPairHand(hand))
            return $"{handString}: {rankString}'s and {rankString2}'s";
        if (IsOfAKind(hand))
            return $"{handString}: {rankString}'s";
        if (IsFlushOrStraight(hand))
            return $"{handString}: {rankString} high";
        if (IsACher(hand))
            return $"{handString}";
        else if (hand == allHandTypes[17])
            return $"{handString}: {rankString}'s, {rankString2}'s and {rankString3}'s";
        else return "Error";
    }

    private List<Card> GetPlayerHand(int player)
    {
        return player switch
        {
            0 => monHand,
            1 => p1Hand,
            2 => p2Hand,
            3 => p3Hand,
            _ => throw new ArgumentOutOfRangeException(nameof(player))
        };
    }

    private void SetPlayerHand(int player, List<Card> hand, List<int> rank, HandTypes handType)
    {
        switch (player)
        {
            case 0:
                monHand = hand;
                monRank = rank;
                MonHand = handType;
                break;
            case 1:
                p1Hand = hand;
                p1Rank = rank;
                P1Hand = handType;
                break;
            case 2:
                p2Hand = hand;
                p2Rank = rank;
                P2Hand = handType;
                break;
            case 3:
                p3Hand = hand;
                p3Rank = rank;
                P3Hand = handType;
                break;
        }
    }

    private void UpdateHandToCompare(HandTypes bestHandType, List<int> bestHandRanks, List<Card> bestHandPlayed, int playerNumber)
    {
        while (bestHandRanks.Count < 5)
            bestHandRanks.Add(0);

        SetPlayerHand(playerNumber, bestHandPlayed.ToList(), bestHandRanks.ToList(), bestHandType);
    }

}
