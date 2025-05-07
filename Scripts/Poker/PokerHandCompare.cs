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
    public HandTypes bestHandType;
    private PokerTableCards pokerTableCards;
    public List<Card> currentHand = new List<Card>();
    public List<Card> p1Hand = new List<Card>();
    public List<Card> p2Hand = new List<Card>();
    public List<Card> p3Hand = new List<Card>();
    public List<Card> monHand = new List<Card>();
    public List<int> p1Rank;
    public List<int> p2Rank;
    public List<int> p3Rank;
    public List<int> monRank;
    public List<int> handRank;
    public List<Card> bestHand;

    void Start()
    {
        HandTypes[] hands = Resources.LoadAll<HandTypes>("HandRanks");
        allHandTypes.AddRange(hands);
        allHandTypes = allHandTypes.OrderBy(hand => hand.handRank).ToList();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
    }



    public void UpdateHandType(List<GameObject> pocket, List<GameObject> table, int player)
    {
        bool hasStraight = false;
        bool hasPair = false;
        bool hasFlush = false;
        bool hasThree = false;
        bool hasTwoPair = false;
        bool hasThreePair = false;
        bool hasTwoThree = false;
        bool hasFour = false;
        bool hasFive = false;
        bool hasSix = false;
        bool hasRoyal = false;
        bool hasFuller = false;
        bool hasFull = false;
        bool hasPlunger = false;
        bool hasLongBoy = false;
        bool hasLowStraight = false;

        currentHand.Clear();
        bestHand.Clear();
        bestHandType = null;
        handRank.Clear();
        handRank.Add(0);
        handRank.Add(0);
        handRank.Add(0);
        handRank.Add(0);
        handRank.Add(0);

        if (player == 0)
        {
            monHand.Clear();
            monRank.Clear();
        }
        if (player == 1)
        {
            p1Hand.Clear();
            p1Rank.Clear();
        }
        if (player == 2)
        {
            p2Hand.Clear();
            p1Rank.Clear();
        }
        if (player == 3)
        {
            p3Hand.Clear();
            p3Rank.Clear();
        }

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
                { hasTwoPair = true; }
                hasPair = true;
            }
            if (group.Count == 3)
            {
                if (hasThree)
                { hasTwoThree = true; }
                if (hasTwoPair)
                { hasFull = true; }
                hasThree = true;
                Debug.Log($"Three of a Kind found: {group.Rank}");
            }
            if (group.Count == 4)
            {
                hasFour = true;
                if (hasTwoPair)
                { hasFuller = true; }
                Debug.Log($"Four of a Kind found: {group.Rank}");
            }
            if (group.Count == 5)
            {
                hasFive = true;
                Debug.Log($"Five of a Kind found: {group.Rank}");
            }
            if (group.Count == 6)
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
        var plungerFlush = currentHand.GroupBy(card => card.cardSuit.First())
                        .Where(group => group.Count() > 5)
                        .Select(group => new { Rank = group.Key, Count = group.Count() })
                        .ToList();


        foreach (var group in flush)
        {
            Debug.Log($"Found {group.Count} of {group.Rank}");
            hasFlush = true;
        }
        foreach (var group in plungerFlush)
        {
            Debug.Log($"Found {group.Count} of {group.Rank}");
            hasPlunger = true;
        }
        //add detection for 6 card flush, wild cards, temporary wild cards, defect cards


        //straight
        List<int> rankValues = currentHand
                        .Select(card => (int)card.cardRank.First())
                        .Distinct()  // Ensures only unique ranks are considered
                        .OrderBy(rank => rank)
                        .ToList();

        // Special case: Ace-low straight (Ace-2-3-4-5)
        if (rankValues.Contains((int)Card.CardRank.Ace) &&
            rankValues.Contains((int)Card.CardRank.Two) &&
            rankValues.Contains((int)Card.CardRank.Three) &&
            rankValues.Contains((int)Card.CardRank.Four) &&
            rankValues.Contains((int)Card.CardRank.Five))
        {
            Debug.Log("Ace-low straight found: Ace, 2, 3, 4, 5");
            hasStraight = true;
            hasLowStraight = true;
            handRank[0] = 5;
        }
        if (rankValues.Contains((int)Card.CardRank.Ace) &&
            rankValues.Contains((int)Card.CardRank.King) &&
            rankValues.Contains((int)Card.CardRank.Queen) &&
            rankValues.Contains((int)Card.CardRank.Jack) &&
            rankValues.Contains((int)Card.CardRank.Ten))
        {
            Debug.Log("Ace-high straight found: Ace, K, Q, J, 10");
            hasStraight = true;
            hasRoyal = true;
            handRank[0] = 13;
        }

        for (int i = 0; i <= rankValues.Count - 5; i++)
        {
            if (rankValues[i + 1] == rankValues[i] + 1 &&
                rankValues[i + 2] == rankValues[i] + 2 &&
                rankValues[i + 3] == rankValues[i] + 3 &&
                rankValues[i + 4] == rankValues[i] + 4)
            {
                Debug.Log($"Straight found: {currentHand[i].cardRank.First()} to {currentHand[i + 4].cardRank}");
                hasStraight = true;
                handRank[0] = rankValues[i + 4];
                handRank[1] = 0;
            }
        }
        //check for a LongBoy
        if (hasStraight)
        {
            if (rankValues.Contains((int)Card.CardRank.Ace) &&
                 rankValues.Contains((int)Card.CardRank.Two) &&
                 rankValues.Contains((int)Card.CardRank.Three) &&
                 rankValues.Contains((int)Card.CardRank.Four) &&
                 rankValues.Contains((int)Card.CardRank.Five) &&
                 rankValues.Contains((int)Card.CardRank.Six))
            {
                Debug.Log("Ace-low LongBoy found: Ace, 2, 3, 4, 5");
                hasLongBoy = true;
                handRank[0] = 5;
            }
            else if (rankValues.Contains((int)Card.CardRank.Ace) &&
                rankValues.Contains((int)Card.CardRank.King) &&
                rankValues.Contains((int)Card.CardRank.Queen) &&
                rankValues.Contains((int)Card.CardRank.Jack) &&
                rankValues.Contains((int)Card.CardRank.Ten) &&
                rankValues.Contains((int)Card.CardRank.Nine))
            {
                Debug.Log("Ace-high LongBoy found: Ace, K, Q, J, 10");
                hasLongBoy = true;
                hasRoyal = true;
                handRank[0] = 13;
            }

            else
            {
                for (int i = 0; i <= rankValues.Count - 5; i++)
                {
                    if (rankValues[i + 1] == rankValues[i] + 1 &&
                        rankValues[i + 2] == rankValues[i] + 2 &&
                        rankValues[i + 3] == rankValues[i] + 3 &&
                        rankValues[i + 4] == rankValues[i] + 4 &&
                        rankValues[i + 5] == rankValues[i] + 5)
                    {
                        Debug.Log($"LongBoy found: {currentHand[i].cardRank.First()} to {currentHand[i + 4].cardRank}");
                        hasStraight = true;
                        handRank[0] = rankValues[i + 5];
                        handRank[1] = 0;
                    }
                }
            }


            //will need to update for wilds and weirdness and extra cards but hey 

            //detect hand type and populate the five cards being used

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
                int highestStraightFlushRank = 0;
                int highestFlushRank = 0;

                // Extract the five highest suited cards
                var flushCards = currentHand.GroupBy(card => card.cardSuit.First())
                                            .Where(group => group.Count() >= 5)
                                            .SelectMany(group => group)
                                            .OrderByDescending(card => card.cardRank.First())
                                            .ToList();

                // Check for a Straight Flush within Flush cards
                List<int> flushRankValues = flushCards.Select(card => (int)card.cardRank.First()).Distinct().OrderBy(rank => rank).ToList();

                for (int i = 0; i <= flushRankValues.Count - 5; i++)
                {
                    if (flushRankValues[i + 1] == flushRankValues[i] + 1 &&
                        flushRankValues[i + 2] == flushRankValues[i] + 2 &&
                        flushRankValues[i + 3] == flushRankValues[i] + 3 &&
                        flushRankValues[i + 4] == flushRankValues[i] + 4)
                    {
                        // Straight Flush found, store the highest card rank
                        highestStraightFlushRank = flushRankValues[i + 4];
                        if (highestStraightFlushRank == 13)
                        {
                            bestHandType = allHandTypes[10];
                            handRank[0] = 13;
                        }
                        else { bestHandType = allHandTypes[9]; }
                        handRank[0] = highestStraightFlushRank;

                        Debug.Log($"Straight Flush found! Highest card: {highestStraightFlushRank}");
                        break;
                    }
                }

                // If no Straight Flush was found, store the highest Flush card rank
                if (highestStraightFlushRank == 0)
                {
                    handRank[0] = (int)flushCards.First().cardRank.First();
                    bestHandType = allHandTypes[6];
                    Debug.Log($"Flush found! Highest card: {highestFlushRank}");
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

                        Debug.Log($"Straight found: {bestHand.First().cardRank.First()} to {bestHand.Last().cardRank.First()}");
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

                    Debug.Log("Ace-low straight found: Ace, 2, 3, 4, 5");
                }
                UpdateHandToCompare(bestHandType, handRank, bestHand, player);
                return;
            }

            if (hasThree && hasTwoPair)  //Full House

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
                // Extract pairs plus the highest kicker
                var firstPair = currentHand.GroupBy(card => card.cardRank.First())
                                               .Where(group => group.Count() == 2)
                                               .Skip(1)
                                               .FirstOrDefault(); // Get the first match

                if (firstPair != null)
                {
                    // Assign the rank of the three of a Kind
                    handRank[0] = (int)firstPair.Key;

                    // Extract the three cards
                    bestHand = firstPair.ToList();

                    var secondpair = currentHand.GroupBy(card => card.cardRank.First())
                                   .Where(group => group.Count() == 2)
                                   .FirstOrDefault(); // Get the first match
                    handRank[1] = (int)secondpair.Key;

                    // Extract the three cards
                    bestHand = secondpair.ToList();


                    // Find the highest kicker not part of the four-of-a-kind
                    var kickerCard = currentHand.Except(bestHand)
                                                .OrderByDescending(card => card.cardRank.First())
                                                .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[2] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }

                }
                bestHandType = allHandTypes[3];
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

            
            
                //strip to the pair plus 3 highest
                // Extract pairs plus the highest kicker
                var pairGroup = currentHand.GroupBy(card => card.cardRank.First())
                                               .Where(group => group.Count() == 1)
                                               .FirstOrDefault(); // Get the first match

                if (pairGroup != null)
                {
                    // Assign the rank of the three of a Kind
                    handRank[0] = (int)pairGroup.Key;

                    // Extract the three cards
                    bestHand = pairGroup.ToList();

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

                    kickerCard = currentHand.Except(bestHand)
                     .OrderByDescending(card => card.cardRank.First())
                     .FirstOrDefault();

                    if (kickerCard != null)
                    {
                        handRank[4] = (int)kickerCard.cardRank.First();
                        bestHand.Add(kickerCard); // Add the kicker to the best hand
                    }

                }
                bestHandType = allHandTypes[1];
            UpdateHandToCompare(bestHandType, handRank, bestHand, player);
            return;
        }

    }





    private void UpdateHandToCompare(HandTypes bestHandType, List<int> bestHandRanks, List<Card> bestHandPlayed, int playerNumber)
    {
        if (playerNumber == 0)
        {
            monHand = bestHandPlayed.ToList();
            monRank = bestHandRanks.ToList();
            MonHand = bestHandType;
        }
        else if (playerNumber == 1)
        {
            p1Hand = bestHandPlayed.ToList();
            p1Rank = bestHandRanks.ToList();
            P1Hand = bestHandType;
        }
        else if (playerNumber == 2)
        {
            p2Hand = bestHandPlayed.ToList();
            p2Rank = bestHandRanks.ToList();
            P2Hand = bestHandType;
        }
        else if (playerNumber == 3)
        {
            p3Hand = bestHandPlayed.ToList();
            p3Rank = bestHandRanks.ToList();
            P3Hand = bestHandType;
        }
    }








}
