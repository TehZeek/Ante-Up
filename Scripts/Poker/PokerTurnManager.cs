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
    //clean the below
    public bool[] WhoWon = new bool[] { false, false, false, false };
    private List<int> playersStillIn;
    private List<int> minimumRank;
    private List<int> p1handRanking;
    private List<int> p2handRanking;
    private List<int> p3handRanking;
    private List<int> monhandRanking;

 

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
        Debug.Log("Finished PokerTurnManager.Start()");
}

void Update()
    {

        while (stillThisTurn)
        {
            switch (turnOrder[1])
            {
                case 0:
                    TurnAssign();
                    break;
                case 1:
                    PocketCardDraw();
                    break;
                case 2:
                    TheFlop();
                    break;
                case 3:
                    TheTurn();
                    break;
                case 4:
                    TheRiver();
                    break;
                case 5:
                    BonusRound();
                    break;
                case 6:
                    Showdown();
                    break;
            }
        }
    }

    public void TickTurn()
    {
        // call after each Poker action, ticks through who's turn it is, then the round, then resets to another person in the hot seat.
        turnOrder[2]++;
        if (turnOrder[2] == 0 && IsOut[0]) { turnOrder[2]++; }
        if (turnOrder[2] == 1 && IsOut[1]) { turnOrder[2]++; }
        if (turnOrder[2] == 2 && IsOut[2]) { turnOrder[2]++; }
        if (turnOrder[2] == 3 && IsOut[3]) { turnOrder[2]=0; }
        if (turnOrder[2] > 3) { turnOrder[2] = 0; }

        if ((HasChecked[0] || IsOut[0]) && (HasChecked[1] || IsOut[1]) && (HasChecked[2] || IsOut[2]) && (HasChecked[3] || IsOut[3]))
        {
            turnOrder[1]++;
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
                turnOrder[0]++;
            turnOrder[2] = turnOrder[0];
            if (turnOrder[2] == 1 && IsOut[1]) { turnOrder[2]++; }
            if (turnOrder[2] == 2 && IsOut[2]) { turnOrder[2]++; }
            if (turnOrder[2] == 3 && IsOut[3]) 
            {
                turnOrder[3]++;
                Debug.Log("HAND OVER");
                //monster victory            
            }
            stillThisTurn = true;
        }
        Debug.Log("finished TickTurn");
        PlayerOptions();
    }

    private void TurnAssign()
    {
        stillThisTurn = false;
        int bigBlind = turnOrder[0] + 1; 
        if (bigBlind > 3) { bigBlind -= 4; }
        int smallBlind = turnOrder[0] + 2; 
        if (smallBlind > 3) { smallBlind -= 4; }
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
        stillThisTurn = false;
        pokerDrawPile.DealPocketCards();
        pokerTableCards.CompareHands();
        Debug.Log("Finished PocketCardDraw");
        PlayerOptions();
    }

    private void PlayerOptions()
    {
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
        stillThisTurn = false;
        pokerTableCards.ShowdownReveal();
        battleMenu.UpdateButtonDisplay(4);
        FindWhoWon();
    }

    private void FindWhoWon()
    {
        // see who is still in if the Monster folded
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
                    WhoWon[i] = true;
                }
            }
            return;
        }
        // see if players folded and Monster is in
        if ((IsOut[1] && !isAllIn[1])&& (IsOut[2] && !isAllIn[2])&& (IsOut[3] && !isAllIn[3]))
        {
            WhoWon[0] = true;
            return;
        }

        // compare Showdown winner
        // make a list of players still in
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
        p2handRanking.AddRange(pokerHandCompare.p3Rank);
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

        // make a list of still in players that beat the minimum hand
        for (int i = 0; i < playersStillIn.Count; i++)
        {
            //compare hands to the minimum hand and monster hand
            if (i == 1)
            {
                if (DidP1Win(p1handType, p1handRanking, minimumHandRank, minimumRank)) { WhoWon[1] = true; }
                else 
                { 
                    WhoWon[1] = false;
                    playersStillIn.Remove(1);

                }
            }
            if (i == 2)
            {
                if (DidP2Win(p2handType, p2handRanking, minimumHandRank, minimumRank)) { WhoWon[2] = true; }
                else 
                {
                    WhoWon[2] = false;
                    playersStillIn.Remove(2);
                }
            }
            if (i == 3)
            {
                if (DidP3Win(p3handType, p3handRanking, minimumHandRank, minimumRank)) { WhoWon[3] = true; }
                else
                {
                    WhoWon[3] = false;
                    playersStillIn.Remove(3);
                }
            }
        }
        int HowManyWon = 0;
        for (int i = 1; i < 4; i++)
        {
            if (WhoWon[i]) { HowManyWon++; }
        }
        if (HowManyWon == 0)
        {
            WhoWon[0] = true;
            playersStillIn.Clear();
            playersStillIn.Add(0);
            pokerChipManager.SplitThePot(playersStillIn);
        } 
        else
        {
            pokerChipManager.SplitThePot(playersStillIn);
        }
        ClearTurnVariables();

    }

    private bool DidP1Win(int p1handType, List<int> p1handRanking, int minimumHandRank, List<int> minimumRank)
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

    private bool DidP2Win(int p2handType, List<int> p2handRanking, int minimumHandRank, List<int> minimumRank)
    {
        if (p2handType > minimumHandRank)
        {
            return true;
        }
        else if (p2handType == minimumHandRank)
        {
            if (p2handRanking[0] > minimumRank[0])
            {
                return true;
            }
            else if (p2handRanking[0] == minimumRank[0])
            {
                if (p2handRanking[1] > minimumRank[1])
                {
                    return true;
                }
                else if (p2handRanking[1] == minimumRank[1])
                {
                    if (p2handRanking[2] > minimumRank[2])
                    {
                        return true;
                    }
                    else if (p2handRanking[2] == minimumRank[2])
                    {
                        if (p2handRanking[3] > minimumRank[3])
                        {
                            return true;
                        }
                        else if (p2handRanking[3] == minimumRank[3])
                        {
                            if (p2handRanking[4] >= minimumRank[4])
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

    private bool DidP3Win(int p3handType, List<int> p3handRanking, int minimumHandRank, List<int> minimumRank)
    {
        if (p3handType > minimumHandRank)
        {
            return true;
        }
        else if (p3handType == minimumHandRank)
        {
            if (p3handRanking[0] > minimumRank[0])
            {
                return true;
            }
            else if (p3handRanking[0] == minimumRank[0])
            {
                if (p3handRanking[1] > minimumRank[1])
                {
                    return true;
                }
                else if (p3handRanking[1] == minimumRank[1])
                {
                    if (p3handRanking[2] > minimumRank[2])
                    {
                        return true;
                    }
                    else if (p3handRanking[2] == minimumRank[2])
                    {
                        if (p3handRanking[3] > minimumRank[3])
                        {
                            return true;
                        }
                        else if (p3handRanking[3] == minimumRank[3])
                        {
                            if (p3handRanking[4] >= minimumRank[4])
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
    }



}
