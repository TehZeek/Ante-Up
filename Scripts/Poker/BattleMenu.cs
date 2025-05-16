using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class BattleMenu : MonoBehaviour
{
    public int betSize = 2;
    public bool BetIsSet = false;
    public bool[] isAllIn = new bool[] { false, false, false, false };
    public bool AllInTrigger = false;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;

    //state images
    public Image BetNone;
    public Image BetBet;
    public Image BetCheck;
    public Image BetFold;
    public Image CallNone;
    public Image CallCall;
    public Image CallRaise;
    public Image CallFold;
    public Image AllIn;
    public GameObject AllInOver;
    public GameObject BetInAni;
    public List<GameObject> buttons;

    void Start()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        UpdateButtonDisplay(0);
    }

    public void NextPlayer()
    {
        allButtonOff();
        pokerTurnManager.TickTurn();
    }

    public void NextTurn()
    {
        betSize = 0;
        BetIsSet = false;
        for (int i = 0; i < 4; i++)
        {
            pokerChipManager.MeetTheBet[i] = 0;
        }
    }

    private void allButtonOff()
    {
        BetNone.gameObject.SetActive(false);
        BetBet.gameObject.SetActive(false);
        BetCheck.gameObject.SetActive(false);
        BetFold.gameObject.SetActive(false);
        CallNone.gameObject.SetActive(false);
        CallCall.gameObject.SetActive(false);
        CallRaise.gameObject.SetActive(false);
        CallFold.gameObject.SetActive(false);
        AllIn.gameObject.SetActive(false);
        AllInOver.gameObject.SetActive(false);
        BetInAni.gameObject.SetActive(false);
    }

    public void OptionOne()
    {
        if (BetIsSet) { CallChosen(); }
        else { BetChosen(); }
    }

    public void OptionTwo()
    {
        if (BetIsSet) { RaiseChosen(); }
        else { CheckChosen(); }
    }

    public void OptionThree()
    {
        FoldChosen();
    }

    public void OptionFour()
    {
        AllInChosen();
    }



    private void CallChosen()
    {
        int player = pokerTurnManager.turnOrder[2];
        int amountOwed = betSize - pokerChipManager.MeetTheBet[player];
        if (pokerChipManager.playerChips[player] > amountOwed)
        {
            pokerChipManager.BetToThePot(player, amountOwed);
            pokerTurnManager.HasChecked[player] = true;
        }
        else
        {
            amountOwed = pokerChipManager.playerChips[player];
            pokerChipManager.BetToThePot(player, amountOwed);
            pokerChipManager.playerChips[player] = 0;
            isAllIn[player] = true;
            Debug.Log("ALL IN");
        }
        Debug.Log("Call");
        NextPlayer();
        //make adjustments for an all in and fold button
    }

    private void BetChosen()
    {
        Debug.Log("Bet");
        //bet default until someone makes a bet
        //check if any player's chips can meet the bet? if not ALL IN
        //default BET to 1 chip.
        //set a max bet to monster's chips?
        //put to bet to the pot
        //space for player power
        //set has checked
        //turn off all other haschecked (make this a function that checks if a player is out)
        //TickTurn
    }

    private void RaiseChosen()
    {
        Debug.Log("Raise");
        //raise, same as bet minus calling power
    }


    private void CheckChosen()
    {
        int player = pokerTurnManager.turnOrder[2];
        pokerTurnManager.HasChecked[player] = true;
        Debug.Log("Check");
        NextPlayer();
    }
    private void FoldChosen()
    {
        Debug.Log("Fold");
        //fold set p1IsOut bool to true
        //add cards in hand to burn pile
        //fold powers
        //tickturn
    }
    private void AllInChosen()
    { 
        Debug.Log("All In");
        //ALL IN set isALLIN bool to true
        //As bet, but amount = current chips
        //must have less chips then the enemy
        // add in functionality to tickturn to move through the turns at end of round
    }




    private IEnumerator DelayButtonDisplay()
    {
        yield return new WaitForSeconds(0.5f); // 30 frames at 60 FPS

        if (AllInTrigger) { UpdateButtonDisplay(3); }
        else if (!BetIsSet) { UpdateButtonDisplay(1); }
        else { UpdateButtonDisplay(2); }
    }


    public void UpdateButtonDisplay(int displaySwitch)
    {
        allButtonOff();
        if (displaySwitch == 0)
        {
            BetInAni.gameObject.SetActive(true);
            StartCoroutine(DelayButtonDisplay());
        }

        if (displaySwitch == 1)
        {
            BetNone.gameObject.SetActive(true);
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(true);
            buttons[2].gameObject.SetActive(true);
            buttons[3].gameObject.SetActive(false);
        }
        if (displaySwitch == 11)
        {
            BetBet.gameObject.SetActive(true);
        }
        if (displaySwitch == 12)
        {
            BetCheck.gameObject.SetActive(true);
        }
        if (displaySwitch == 13)
        {
            BetFold.gameObject.SetActive(true);
        }
        if (displaySwitch == 2)
        {
            CallNone.gameObject.SetActive(true);
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(true);
            buttons[2].gameObject.SetActive(true);
            buttons[3].gameObject.SetActive(false);
        }
        if (displaySwitch == 21)
        {
            CallCall.gameObject.SetActive(true);
        }
        if (displaySwitch == 22)
        {
            CallRaise.gameObject.SetActive(true);
        }
        if (displaySwitch == 23)
        {
            CallFold.gameObject.SetActive(true);
        }
        if (displaySwitch == 3)
        {
            AllIn.gameObject.SetActive(true);
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(false);
            buttons[2].gameObject.SetActive(false);
            buttons[3].gameObject.SetActive(true);
        }
        if (displaySwitch == 31)
        {
            AllInOver.gameObject.SetActive(true);
        }
    }
    //set the states for the button menu -
    //where it's at for the turn
    //what options are availible









    //side bets? hit sidebet amount?
    // up to 2 needed

}
