using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;


public class PokerTurnManager : MonoBehaviour
{
    public int[] turnOrder = new int[]{1, 0, 1};
    private GameManager gameManager;
    private PokerChipManager pokerChipManager;
    private PokerDrawPile pokerDrawPile;
    private PokerTableCards pokerTableCards;
    private BattleMenu battleMenu;
    private BattleManager battleManager; 
    private PokerHandCompare pokerHandCompare;
    public bool[] IsOut = new bool[] { false, false, false, false };
    public bool[] HasChecked = new bool[] { false, false, false, false };
    public bool[] isAllIn = new bool[] { false, false, false, false };
    public bool HasARiver = true;
    public bool HasABonusRound = false;
    public bool stillThisTurn = false;
    private List<int> playersStillIn = new List<int>();
    private const int PlayerCount = 4;


    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        turnOrder[0] = gameManager.whichTurn;
        turnOrder[1] = 0;
        turnOrder[2] = turnOrder[0];
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        battleMenu = FindFirstObjectByType<BattleMenu>();
        Debug.Log("[Start] PokerTurnManager initialized. Starting turn: " + turnOrder[0]);
    }

    void Update()
    {

        if (stillThisTurn)
        {
            stillThisTurn = false; // Prevent reentry
            StartCoroutine(ProcessTurn());
        }
    }

    private IEnumerator ProcessTurn()
    {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("[Update] stillThisTurn is true. Current phase: " + turnOrder[1]);

            switch (turnOrder[1])
            {
                case 0:
                    Debug.Log("[Update] Entering TurnAssign()"); TurnAssign(); break;
                case 1:
                    Debug.Log("[Update] Entering PocketCardDraw()"); PocketCardDraw(); break;
                case 2: Debug.Log("[Update] Entering TheFlop()"); TheFlop(); break;

                case 3: Debug.Log("[Update] Entering TheTurn()"); TheTurn(); break;
                case 4: Debug.Log("[Update] Entering TheRiver()"); TheRiver(); break;

                case 5: Debug.Log("[Update] Entering BonusRound()"); BonusRound(); break;

                case 6: Debug.Log("[Update] Entering Showdown()"); Showdown(); break;
                default: Debug.LogWarning("[Update] Invalid turnOrder[1] value: " + turnOrder[1]); break;
            }
        yield return null;
    }

    public void TickTurn()
    {
        AdvanceTurnIndex();
        if (CheckForEndOfRound()) { return; }
        Debug.Log("[TickTurn] Ending. turnOrder: [" + turnOrder[0] + ", " + turnOrder[1] + ", " + turnOrder[2] + "]");
        PlayerOptions();
    }

    private void AdvanceTurnIndex()
    {
        battleMenu.AllInTrigger = false;
        Debug.Log("[TickTurn] Called. Current Player Turn Index: " + turnOrder[2]);

        do
        {
            turnOrder[2]++;
            if (turnOrder[2] >= PlayerCount)
            {
                turnOrder[2] = 0;
            }
        } while (IsOut[turnOrder[2]]);

        Debug.Log("[TickTurn] After incrementing, turnOrder[2]: " + turnOrder[2]);

        if ((HasChecked[0] || IsOut[0]) && (HasChecked[1] || IsOut[1]) && (HasChecked[2] || IsOut[2]) && (HasChecked[3] || IsOut[3])) 
        { 
            AdvanceRound();
        }
    }
    private void AdvanceRound()
    {
        if (CheckAllIn()) { return; }

        Debug.Log("[TickTurn] All players checked or are out. Advancing round.");
        battleMenu.BetIsSet = false;

        if (turnOrder[1] < 6) { turnOrder[1]++; } else { FindWhoWon(); }
       
        if (turnOrder[1] == 1) { battleMenu.BetIsSet = true; }

        if (turnOrder[1] == 4 && !HasARiver)
        {
            turnOrder[1]++;
        }
        if (turnOrder[1] == 5 && !HasABonusRound)
        {
            turnOrder[1]++;
        }

        for (int i = 0; i < PlayerCount; i++)
        {
            HasChecked[i] = false;
        }

        turnOrder[2] = turnOrder[0];


        if (turnOrder[2] == 1 && IsOut[1]) { turnOrder[2]++; }
        if (turnOrder[2] == 2 && IsOut[2]) { turnOrder[2]++; }
        if (turnOrder[2] == 3 && IsOut[3]) { turnOrder[2] = 0; }

        stillThisTurn = true;
    }
    private bool CheckAllIn()
    {
        if (isAllIn[0] || ((isAllIn[1] || IsOut[1]) && (isAllIn[2] || IsOut[2]) && (isAllIn[3] || IsOut[3])))
        {
            Debug.Log("ALL IN LOOP");
            stillThisTurn = false;
            AllInLoop();
            return true;
        }
        return false;
    }

    private bool CheckForEndOfRound()
    {
        if (IsOut[0] && !isAllIn[0])
        {
            FindWhoWon();
            return true;
        }
        else if (IsOut[1] && !isAllIn[1] && IsOut[2] && !isAllIn[2] && IsOut[3] && !isAllIn[3])
        {
            FindWhoWon();
            return true;
        }

        else
        {
            Debug.Log("Not ending the round early");
            return false; 
        }
    }

    private void TurnAssign()
    {
        Debug.Log("[TurnAssign] Assigning blinds.");

        stillThisTurn = false;
        int bigBlind = turnOrder[0] + 1; 
        if (bigBlind > 3) { bigBlind -= 4; }
        int smallBlind = turnOrder[0] + 2; 
        if (smallBlind > 3) { smallBlind -= 4; }
        Debug.Log("[TurnAssign] BigBlind: " + bigBlind + ", SmallBlind: " + smallBlind);

        pokerChipManager.BetToThePot(bigBlind, pokerChipManager.anteAmount);
        pokerChipManager.BetToThePot(smallBlind, 1);
        for (int i = 0; i < PlayerCount; i++)
        {
            HasChecked[i] = true;
        }
        Debug.Log("Finished TurnAssign");
        TickTurn();
    }


    private void PocketCardDraw()
    {
        Debug.Log("[PocketCardDraw] Dealing pocket cards.");
        stillThisTurn = false;
        pokerDrawPile.DealPocketCards();
        pokerTableCards.CompareHands();
        PlayerOptions();
    }

    private void PlayerOptions()
    {
        int chips = pokerChipManager.playerChips[turnOrder[2]];
        int chipsNeeded = pokerChipManager.BetSize + 1 - pokerChipManager.InThePot[turnOrder[2]];
        if (chips <= chipsNeeded) 
        {
            battleMenu.AllInTrigger = true;
        }

        Debug.Log("[PlayerOptions] Displaying player options.");
        battleMenu.UpdateButtonDisplay(0);
        battleManager.UpdateHUD();
    }

    private void TheFlop()
    {
        stillThisTurn = false;
        pokerDrawPile.DealFlopCards();
        pokerTableCards.CompareHands();
        PlayerOptions();
    }

    private void TheTurn()
    {
        stillThisTurn = false;
        pokerDrawPile.DealTurnCards();
        pokerTableCards.CompareHands();
        PlayerOptions();
    }

    private void TheRiver()
    {
        stillThisTurn = false;
        pokerDrawPile.DealRiverCards();
        pokerTableCards.CompareHands();
        PlayerOptions();
    }

    private void BonusRound()
    {
        stillThisTurn = false;
        pokerDrawPile.DealRiverCards();
        pokerTableCards.CompareHands();
        PlayerOptions();
    }

    private void Showdown()
    {
        Debug.Log("[Showdown] Executing showdown.");
        stillThisTurn = false;
        pokerTableCards.ShowdownReveal();
        battleMenu.UpdateButtonDisplay(4);
        FindWhoWon();
    }
    private void  AllInLoop()

    {
        if (!IsOut[1] || isAllIn[1] || !IsOut[2] || isAllIn[2] || !IsOut[3] || isAllIn[3])
        {
            // Call ShowdownReveal and wait 1 second
            pokerTableCards.ShowdownReveal();
            int cardsLeftToDeal = 5;
            if (HasABonusRound) { cardsLeftToDeal++; }
            if (!HasARiver) { cardsLeftToDeal--; }
            if (turnOrder[1] == 2 && cardsLeftToDeal >= 3) { cardsLeftToDeal -= 3; }
            if (turnOrder[1] == 3 && cardsLeftToDeal >= 1) { cardsLeftToDeal--; }
            if (turnOrder[1] == 4 && cardsLeftToDeal >= 1) { cardsLeftToDeal--; }
            if (turnOrder[1] == 5 && cardsLeftToDeal >= 1) { cardsLeftToDeal--; }

            if (cardsLeftToDeal > 0)
            {
                for (int i = 0; i < cardsLeftToDeal; i++)
                {
                    pokerDrawPile.DealRiverCards();
                }
            }
            Debug.Log("Compared Hands");
            FindWhoWon();

        }
        else { FindWhoWon(); }
    }

    private void FindWhoWon()
    {
        Debug.Log("[FindWhoWon] Determining the winner.");
        playersStillIn.Clear();

        if(RoundOverEarly())
        {
            return;
        }
        Debug.Log("We have a showdown!");
        int minimumHandRank = GetMonsterHand();
        int playerWinners = 0;
        Debug.Log("Monster Hand: " + minimumHandRank);
        for (int i = 1; i < PlayerCount; i++)
        {
            if (!IsOut[i] || isAllIn[i])
            {
                int p = GetPlayerHand(i);
                Debug.Log("Player "+i+" Hand: " + p);
                if (p > minimumHandRank)
                {
                    playerWinners++;
                    Debug.Log("Found player " + i + " Whos hand is a better rank");
                    playersStillIn.Add(i);
                }

                else if (p == minimumHandRank)
                {
                    Debug.Log("Player " + i + " hand rank tie!");

                    bool resolved = false;

                    for (int j = 0; j < 5; j++) // Compare up to 5 kicker cards
                    {
                        int tiebreakerBeat = GetMonsterRank(j);
                        int tiebreakerHave = GetPlayerRank(i, j);

                        if (tiebreakerHave > tiebreakerBeat)
                        {
                            playerWinners++;
                            playersStillIn.Add(i);
                            Debug.Log("Player " + i + " wins tie with higher kicker.");
                            resolved = true;
                            break; // Stop comparing
                        }
                        else if (tiebreakerHave < tiebreakerBeat)
                        {
                            Debug.Log("Player " + i + " loses tie with lower kicker.");
                            resolved = true;
                            break; // Stop comparing
                        }
                    }

                    if (!resolved)
                    {
                        // Complete tie after all 5 kickers
                        Debug.Log("Player " + i + " ties exactly with monster hand. Pot is split.");
                        playersStillIn.Add(i);
                        playersStillIn.Add(0);
                    }
                }
                else if (p < minimumHandRank)
                {
                    Debug.Log("Player " + i + " Does not win");
                }
            }
        }
        //check monster win
        if (playerWinners == 0)
        {
            playersStillIn.Add(0);
        }
        Debug.Log("[FindWhoWon] Players who won: " + string.Join(",", playersStillIn));
       
        pokerChipManager.SplitThePot(playersStillIn);
      
        ClearTurnVariables();
    }

    private bool RoundOverEarly()
    {
        //If the Monster Folded players still in split the pot
        if (IsOut[0] && !isAllIn[0])
        {
            for (int i = 1; i < PlayerCount; i++)
            {
                if (!IsOut[i] || isAllIn[i])
                {
                    playersStillIn.Add(i);
                    Debug.Log("[FindWhoWon] Monsters Folded, Players who won: " + string.Join(",", playersStillIn));
                    
                }
            }
            pokerChipManager.SplitThePot(playersStillIn);
            ClearTurnVariables();
            return true;
        }

        //If the players all folded
        if ((IsOut[1] && !isAllIn[1]) && (IsOut[2] && !isAllIn[2]) && (IsOut[3] && !isAllIn[3]))
        {
            playersStillIn.Add(0);
            Debug.Log("Players folded, Monsters get the pot");
            pokerChipManager.SplitThePot(playersStillIn);
            ClearTurnVariables();
            return true;
        }
        return false;
    }

    private int GetMonsterHand()
    {
        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
        HandTypes minimumHand = monsterManager.monster.minimumHand;
        int minimumHandRank = pokerHandCompare.allHandTypes.IndexOf(minimumHand);
        int monhandType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand);
        if (monhandType > minimumHandRank)
        {
            minimumHandRank = monhandType;

        }
        return minimumHandRank;
    }

    private int GetPlayerHand(int player)
    {
        if (player == 1)
        {
            return pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P1Hand);
        }
        else if (player == 2)
        {
            return pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P2Hand);
        }
        else
        {
            return pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P3Hand);
        }
    }
    private int GetPlayerRank(int player, int rank)
    {
        if (player == 1)
        {
            return pokerHandCompare.p1Rank[rank];
        }
        else if (player == 2)
        {
            return pokerHandCompare.p2Rank[rank];
        }
        else
        {
            return pokerHandCompare.p3Rank[rank];
        }
    }

      private int GetMonsterRank(int rank)
      {
        if (rank == 0)
        {
            MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
            int minRank = monsterManager.monster.minimumRank;
            int monRank = pokerHandCompare.monRank[rank];
            HandTypes minimumHand = monsterManager.monster.minimumHand;
            int minimumHandRank = pokerHandCompare.allHandTypes.IndexOf(minimumHand);
            int minType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand);
            if( minType > minimumHandRank)
            {
                return pokerHandCompare.monRank[rank];
            }
            else
            {
                return minRank;
            }
        }
        else 
        {
            return pokerHandCompare.monRank[rank];
        }

    }

    private void ClearTurnVariables()
    {
        Debug.Log("[ClearTurnVariables] Resetting for next hand.");

        stillThisTurn = false;
        playersStillIn.Clear();
        turnOrder[0]++;
        if (turnOrder[0] > 3) { turnOrder[0] -= PlayerCount; }
        turnOrder[1] = 0;
        turnOrder[2] = 0;
        pokerChipManager.potChips = 0;
        pokerChipManager.BetSize = 2;
        for (int i = 0; i < PlayerCount; i++)
        {
            IsOut[i] = false;
            if (pokerChipManager.playerChips[i] == 0) { IsOut[i] = true; }
            isAllIn[i] = false;
            HasChecked[i] = false;
            pokerChipManager.InThePot[i] = 0;
            battleManager.HUDs[i].GetComponent<HUD>().isBetter = false;
        }
        pokerChipManager.UpdateChipDisplay();
        battleManager.UpdateHUD();
        pokerDrawPile.Reshuffle();
        stillThisTurn = true;
        Debug.Log("[ClearTurnVariables] Variables reset complete.");
    }
}
