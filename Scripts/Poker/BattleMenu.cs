using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BattleMenu : MonoBehaviour
{
    public bool BetIsSet = true;
    public bool AllInTrigger = false;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;
    private PokerTableCards pokerTableCards;

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
    public Image AllInCheck;
    public Image AllInFold;
    public Image FoldAllIn;
    public Image FoldAllInFold;
    public GameObject AllInOver;
    public GameObject FoldAllInOver;
    public GameObject BetInAni;
    public List<GameObject> buttons;
    public bool isAnimating = false;
    public bool PlayerHasConfirmed = false;
    public Button confirmButton;

    void Start()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        confirmButton.gameObject.SetActive(false); // Hide button initially
        confirmButton.onClick.AddListener(PlayerConfirmed);
        Debug.Log("Finished Battle Menu start");
        UpdateButtonDisplay(0);
    }

    public void ShowConfirmButton(bool show)
    {
        confirmButton.gameObject.SetActive(show);
        PlayerHasConfirmed = false; // Reset confirmation flag
    }

    public void PlayerConfirmed()
    {
        PlayerHasConfirmed = true;
        ShowConfirmButton(false); // Hide the button after confirmation
        Debug.Log("Player confirmed. Proceeding.");
    }

    public void NextPlayer()
    {
        allButtonOff();
        pokerTurnManager.TickTurn();
    }

    public void NextTurn()
    {
        pokerChipManager.BetSize = 0;
        BetIsSet = true;
        for (int i = 0; i < 4; i++)
        {
            pokerChipManager.InThePot[i] = 0;
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
        AllInCheck.gameObject.SetActive(false); 
        AllInFold.gameObject.SetActive(false);
        FoldAllIn.gameObject.SetActive(false);
        FoldAllInFold.gameObject.SetActive(false);
        FoldAllInOver.gameObject.SetActive(false);
    }

    public void OptionOne()
    {
        if (AllInTrigger && !BetIsSet) { AllInChosen(); }
        else if (AllInTrigger && BetIsSet) { return; }
        else if (BetIsSet) { CallChosen(); }
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



    private void CallChosen()
    {
        int player = pokerTurnManager.turnOrder[2];
        int amountOwed = pokerChipManager.BetSize - pokerChipManager.InThePot[player];
        pokerChipManager.BetToThePot(player, amountOwed);
        pokerTurnManager.HasChecked[player] = true;
        NextPlayer();
        //make adjustments for an all in and fold button
    }

    private void BetChosen()
    {
        if (!BetIsSet)
        {
            //BetPower();
        }
        BetIsSet = true;
            for (int i = 0; i < 4; i++)
            {
                pokerTurnManager.HasChecked[i] = false;
            }
        pokerTurnManager.HasChecked[pokerTurnManager.turnOrder[2]] = true;
        pokerChipManager.BetToThePot(pokerTurnManager.turnOrder[2], (1 + pokerChipManager.BetSize - pokerChipManager.InThePot[pokerTurnManager.turnOrder[2]]));
        pokerChipManager.BetSize += 1;
        NextPlayer();
        
        //set a max bet to monster's chips?
        //space for player power
    }

    private void RaiseChosen()
    {
        BetChosen();
    }


    private void CheckChosen()
    {
        int player = pokerTurnManager.turnOrder[2];
        pokerTurnManager.HasChecked[player] = true;
        NextPlayer();
    }
    private void FoldChosen()
    {
        int player = pokerTurnManager.turnOrder[2];
        pokerTurnManager.IsOut[player] = true;
        pokerTableCards.Fold(player);
        //fold powers
        NextPlayer();
    }

    private void AllInChosen()
    { 
        Debug.Log("All In special stuff to go here");
        BetChosen();
        //ALL IN set isALLIN bool to true
        //As bet, but amount = current chips
        //must have less chips then the enemy
        // add in functionality to tickturn to move through the turns at end of round
    }




    private IEnumerator DelayButtonDisplay()
    {
        yield return new WaitForSeconds(0.5f); // 30 frames at 60 FPS
        isAnimating = false;
        Debug.Log("Finished button animation");
        if (AllInTrigger && !BetIsSet) { UpdateButtonDisplay(3); }
        else if (AllInTrigger && BetIsSet) { UpdateButtonDisplay(5); }
        else if (!BetIsSet) { UpdateButtonDisplay(1); }
        else { UpdateButtonDisplay(2); }
    }


    public void UpdateButtonDisplay(int displaySwitch)
    {
        allButtonOff();
        if (displaySwitch == 0)
        {
            isAnimating = true;
            BetInAni.gameObject.SetActive(true);
            StartCoroutine(DelayButtonDisplay());
        }

        if (displaySwitch == 1)
        {
            BetNone.gameObject.SetActive(true);
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(true);
            buttons[2].gameObject.SetActive(true);
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
        }
        if (displaySwitch == 21)
        {
            Debug.Log("turned on call rollover");
            CallCall.gameObject.SetActive(true);
        }
        if (displaySwitch == 22)
        {
            Debug.Log("turned on raise rollover");
            CallRaise.gameObject.SetActive(true);
        }
        if (displaySwitch == 23)
        {
            Debug.Log("turned on Fold rollover");
            CallFold.gameObject.SetActive(true);
        }
        if (displaySwitch == 3)
        {
            AllIn.gameObject.SetActive(true);
        }
        if (displaySwitch == 31)
        {
            AllInOver.gameObject.SetActive(true);
        }
        if (displaySwitch == 32)
        {
            AllInCheck.gameObject.SetActive(true);
        }
        if (displaySwitch == 32)
        {
            AllInFold.gameObject.SetActive(true);
        }
        if (displaySwitch == 4)
        {
            allButtonOff();
        }
        if (displaySwitch == 5)
        {
            FoldAllIn.gameObject.SetActive(true);
        }
        if (displaySwitch == 52)
        {
            FoldAllInOver.gameObject.SetActive(true);
        }
        if (displaySwitch == 53)
        {
            FoldAllInFold.gameObject.SetActive(true);
        }
    }

    //side bets? hit sidebet amount?
    // up to 2 needed

}
