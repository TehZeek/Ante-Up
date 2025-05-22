using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;


//This class determines what hand you actually have, which will eventually be called for enemy logic and determining the winner.

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

    //different bools used to determine hand

    private bool hasPair = false;
    private bool hasTwoPair = false;
    // private bool hasHatTrick = false;
    private bool hasThree = false;
    private bool hasStraight = false;
    private bool hasFlush = false;
    private bool hasFour = false;
    //bool private hasTwoThree = false;
    //bool private hasFive = false;
    //bool private hasSix = false;
    // bool private hasPlunger = false;
    // bool private hasLongBoy = false;


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
        // this checks the card list for pairs
        var rankedGroups = currentHand.GroupBy(card => card.cardRank.First())
                                      .Select(group => new { Rank = group.Key, Count = group.Count() })
                                      .ToList();

        // Detect different types of duplicates
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
        //detect flush
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
        hasStraight = true;
        handRank[0] = 5;
    }
    if (rankValues.Contains((int)Card.CardRank.Ace) &&
        rankValues.Contains((int)Card.CardRank.King) &&
        rankValues.Contains((int)Card.CardRank.Queen) &&
        rankValues.Contains((int)Card.CardRank.Jack) &&
        rankValues.Contains((int)Card.CardRank.Ten))
    {
        hasStraight = true;
        handRank[0] = 13;
    }

    for (int i = 0; i <= rankValues.Count - 5; i++)
    {
        if (rankValues[i + 1] == rankValues[i] + 1 &&
            rankValues[i + 2] == rankValues[i] + 2 &&
            rankValues[i + 3] == rankValues[i] + 3 &&
            rankValues[i + 4] == rankValues[i] + 4)
        {
            hasStraight = true;
            handRank[0] = rankValues[i + 4];
            handRank[1] = 0;
        }
    }

    }

    private void UpdateHandToCompare(HandTypes bestHandType, List<int> bestHandRanks, List<Card> bestHandPlayed, int playerNumber)
    {
        bestHandRanks.Add(0);
        bestHandRanks.Add(0);
        bestHandRanks.Add(0);
        bestHandRanks.Add(0);
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
