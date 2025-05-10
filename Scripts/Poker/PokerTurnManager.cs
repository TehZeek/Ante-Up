using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PokerTurnManager : MonoBehaviour
{
    private int[] turnOrder = new int[]{1, 0, 1};
    private GameManager gameManager;
    private PokerChipManager pokerChipManager;
    private PokerDrawPile pokerDrawPile;
    private PokerTableCards pokerTableCards;
    private int turnTicker = 0;
    public bool p1IsOut = false;
    public bool p2IsOut = false;
    public bool p3IsOut = false;
    public bool monIsOut = false;
    public bool monHasChecked = false;
    public bool p1HasChecked = false;
    public bool p2HasChecked = false;
    public bool p3HasChecked = false;

    public bool HasARiver = true;
    public bool HasABonusRound = false;
    private bool stillThisTurn = true;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        turnOrder[0] = gameManager.whichTurn;
        turnOrder[1] = 0;
        turnOrder[2] = turnOrder[0];
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
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
        if (turnOrder[2] == 1)
        {
            if (p1IsOut) { turnOrder[2]++; }
        }
        if (turnOrder[2] == 2)
        {
            if (p2IsOut) { turnOrder[2]++; }
        }
        if (turnOrder[2] == 3)
        {
            if (p3IsOut) { turnOrder[2]++; }
        }

        if (turnOrder[2] == 4)
        {
            turnOrder[2] = 0;
        }

        if ((monHasChecked || monIsOut) && (p1HasChecked || p1IsOut) && (p2HasChecked || p2IsOut) && (p3HasChecked || p3IsOut))
        {
            
            turnOrder[1]++;
                monHasChecked = false;
            p1HasChecked = false;
            p2HasChecked = false;
            p3HasChecked = false;
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
            stillThisTurn = true;
        }
    }

    private void TurnAssign()
    {
        stillThisTurn = false;
        int bigBlind = turnOrder[0];
        int smallBlind = turnOrder[0] + 1;
        if (turnOrder[0] == 3) { smallBlind = 0; }
        Debug.Log("It is player " + bigBlind + "'s turn, and player " + smallBlind + "with the small ante");
        pokerChipManager.BetToThePot(bigBlind, pokerChipManager.anteAmount);
        pokerChipManager.BetToThePot(smallBlind, 1);
        monHasChecked = true;
        p1HasChecked = true;
        p2HasChecked = true;
        p3HasChecked = true;
        TickTurn();
    }


    private void PocketCardDraw()
    {
        stillThisTurn = false;
        pokerDrawPile.DealPocketCards();
        pokerTableCards.CompareHands();
        PlayerOptions();
    }

    private void PlayerOptions()
    {
        // raise, check/call, fold, ALL IN options
        // TickTurn();
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
        //calculate the winner
        //split the pot pokerChipManager.SplitThePot(List<int> players)
    }
}
