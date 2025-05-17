using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PokerTurnManager : MonoBehaviour
{
    public int[] turnOrder = new int[]{1, 0, 1};
    private GameManager gameManager;
    private PokerChipManager pokerChipManager;
    private PokerDrawPile pokerDrawPile;
    private PokerTableCards pokerTableCards;
    private BattleMenu battleMenu;
    private PokerHandCompare pokerHandCompare;
    public bool[] IsOut = new bool[] { false, false, false, false };
    public bool[] HasChecked = new bool[] { false, false, false, false };
    public bool[] isAllIn = new bool[] { false, false, false, false };
    public bool HasARiver = true;
    public bool HasABonusRound = false;
    private bool stillThisTurn = true;
    public bool[] WhoWon = new bool[] { false, false, false, false };
    private List<int> playersStillIn = new List<int>();
    private List<int> minimumRank = new List<int>();
    private List<int> p1handRanking = new List<int>();
    private List<int> p2handRanking = new List<int>();
    private List<int> p3handRanking = new List<int>();
    private List<int> monhandRanking = new List<int>();

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
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

        while (stillThisTurn)
        {
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
        }
    }

    public void TickTurn()
    {
        Debug.Log("[TickTurn] Called. Current Player Turn Index: " + turnOrder[2]);
        turnOrder[2]++;
        for (int i = 0; i < 4; i++)
        {
            if (turnOrder[2] == i && IsOut[i]) { turnOrder[2]++; }
        }
        if (turnOrder[2] > 3) { turnOrder[2] = 0; }
        Debug.Log("[TickTurn] After incrementing, turnOrder[2]: " + turnOrder[2]);

        if (IsOut[0] && !isAllIn[0])
        {
            FindWhoWon();
        }

        if ((HasChecked[0] || IsOut[0]) && (HasChecked[1] || IsOut[1]) && (HasChecked[2] || IsOut[2]) && (HasChecked[3] || IsOut[3]))
        {
            Debug.Log("[TickTurn] All players checked or are out. Advancing round.");

            if (turnOrder[1] < 6) { turnOrder[1]++;}
            battleMenu.BetIsSet = false;
            if (turnOrder[1] == 1) { battleMenu.BetIsSet = true; }
            for (int i = 0; i < 4; i++)
            {
                HasChecked[i] = false;
            }
            turnOrder[2] = turnOrder[0];

            if (turnOrder[1] == 4 && !HasARiver)
            {
                turnOrder[1]++;
             }
            if (turnOrder[1] == 5 && !HasABonusRound)
            {
                turnOrder[1]++;
            }
            if (turnOrder[1] == 7)
            {
                Debug.Log("[TickTurn] Starting new hand.");
                turnOrder[0]++;
                turnOrder[1] = 0;
                turnOrder[2] = turnOrder[0];
            }
            if (turnOrder[2] == 1 && IsOut[1]) { turnOrder[2]++; }
            if (turnOrder[2] == 2 && IsOut[2]) { turnOrder[2]++; }
            if (turnOrder[2] == 3 && IsOut[3]) 
            {
                turnOrder[3]++;
                Debug.Log("[TickTurn] All players out. HAND OVER.");
                FindWhoWon();         
            }
            stillThisTurn = true;
        }
        Debug.Log("[TickTurn] Ending. turnOrder: [" + turnOrder[0] + ", " + turnOrder[1] + ", " + turnOrder[2] + "]");
        PlayerOptions();
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
        for (int i = 0; i < 4; i++)
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
        Debug.Log("[PlayerOptions] Displaying player options.");
        battleMenu.UpdateButtonDisplay(0);
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

    private void FindWhoWon()
    {
        Debug.Log("[FindWhoWon] Determining the winner.");

        for (int i = 0; i < 4; i++)
        {
            WhoWon[i] = false;
        }
        
        if (IsOut[0] && !isAllIn[0])
        {
            for (int i = 1; i < 4; i++)
            {
                if (!IsOut[i] || isAllIn[i])
                {
                    playersStillIn.Add(i);
                    pokerChipManager.SplitThePot(playersStillIn);
                    ClearTurnVariables();
                }
            }
            return;
        }

        if ((IsOut[1] && !isAllIn[1])&& (IsOut[2] && !isAllIn[2])&& (IsOut[3] && !isAllIn[3]))
        {
            WhoWon[0] = true;
            playersStillIn.Clear();
            playersStillIn.Add(0);
            Debug.Log("Monsters get the pot");
            pokerChipManager.SplitThePot(playersStillIn);
            ClearTurnVariables();
            return;
        }

        for (int i = 1; i < 4; i++)
        {
            if (!IsOut[i] || isAllIn[i])
            {playersStillIn.Add(i); }
        }
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        HandTypes minimumHand = battleManager.monster.minimumHand;
        minimumRank.Add(battleManager.monster.minimumRank);

        int minimumHandRank = pokerHandCompare.allHandTypes.IndexOf(minimumHand);
        int p1handType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P1Hand);
        int p2handType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P2Hand);
        int p3handType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P3Hand);
        int monhandType = pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand);
        p1handRanking.AddRange(pokerHandCompare.p1Rank);
        p2handRanking.AddRange(pokerHandCompare.p2Rank);
        p3handRanking.AddRange(pokerHandCompare.p3Rank);
        monhandRanking.AddRange(pokerHandCompare.monRank);


        if (monhandType > minimumHandRank)
        {
            minimumHandRank = monhandType;
            minimumRank.Clear();
            minimumRank.AddRange(monhandRanking);
        }
        if (monhandType == minimumHandRank)
        {
            if (monhandRanking[0] >= minimumRank[0])
            {
                minimumRank.Clear();
                minimumRank.AddRange(monhandRanking);
            }
        }
        Debug.Log("[FindWhoWon] Minimum hand rank: " + minimumHandRank + " of " + minimumRank[0]);

        List<int> toRemove = new List<int>();

        for (int i = 0; i < playersStillIn.Count; i++)
        {

            if (i == 1)
            {
                if (DidPlayerWin(p1handType, p1handRanking, minimumHandRank, minimumRank)) { WhoWon[1] = true; }
                else 
                { 
                    WhoWon[1] = false;
                    toRemove.Add(1);

                }
            }
            if (i == 2)
            {
                if (DidPlayerWin(p2handType, p2handRanking, minimumHandRank, minimumRank)) { WhoWon[2] = true; }
                else 
                {
                    WhoWon[2] = false;
                    toRemove.Add(2);
                }
            }
            if (i == 3)
            {
                if (DidPlayerWin(p3handType, p3handRanking, minimumHandRank, minimumRank)) { WhoWon[3] = true; }
                else
                {
                    WhoWon[3] = false;
                    toRemove.Add(3);
                }
            }
        }
        playersStillIn = playersStillIn.Except(toRemove).ToList();
        Debug.Log("[FindWhoWon] Players still in: " + string.Join(",", playersStillIn));


        int HowManyWon = playersStillIn.Count;
        if (HowManyWon == 0)
        {
            WhoWon[0] = true;
            playersStillIn.Clear();
            playersStillIn.Add(0);
            Debug.Log("Monsters get the pot");

            pokerChipManager.SplitThePot(playersStillIn);
        } 
        else
        {
            Debug.Log("Players get the pot");
            pokerChipManager.SplitThePot(playersStillIn);
        }

        ClearTurnVariables();
    }

    private bool DidPlayerWin(int p1handType, List<int> p1handRanking, int minimumHandRank, List<int> minimumRank)
    {
        if (p1handType > minimumHandRank)
        {
            return true;
        }
        else if (p1handType == minimumHandRank)
        {
            if (p1handRanking[0] > minimumRank[0])
            {
                return true;
            }
            else if (p1handRanking[0] == minimumRank[0])
            {
                if (p1handRanking[1] > minimumRank[1])
                {
                    return true;
                }
                else if (p1handRanking[1] == minimumRank[1])
                {
                    if (p1handRanking[2] > minimumRank[2])
                    {
                        return true;
                    }
                    else if (p1handRanking[2] == minimumRank[2])
                    {
                        if (p1handRanking[3] > minimumRank[3])
                        {
                            return true;
                        }
                        else if (p1handRanking[3] == minimumRank[3])
                        {
                            if (p1handRanking[4] >= minimumRank[4])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

    return false;
    }


    private void ClearTurnVariables()
    {
        Debug.Log("[ClearTurnVariables] Resetting for next hand.");

        stillThisTurn = false;
        minimumRank.Clear();
        playersStillIn.Clear();
        p1handRanking.Clear();
        p2handRanking.Clear();
        p3handRanking.Clear();
        monhandRanking.Clear();
        turnOrder[0]++;
        turnOrder[1] = 0;
        turnOrder[2] = 0;
        pokerChipManager.potChips = 0;
        pokerChipManager.BetSize = 2;
        for (int i = 0; i < 4; i++)
        {
            WhoWon[i] = false;
            IsOut[i] = false;
            if (pokerChipManager.playerChips[i] == 0) { IsOut[i] = true; }
            isAllIn[i] = false;
            HasChecked[i] = false;
            pokerChipManager.InThePot[i] = 0;
        }
        pokerChipManager.UpdateChipDisplay();
        pokerDrawPile.Reshuffle();
        stillThisTurn = true;
        Debug.Log("[ClearTurnVariables] Variables reset complete.");

    }
}
