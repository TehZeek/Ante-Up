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
    public PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;
    private BattleManager battleManager; 
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
    public Image CallFoldNone;
    public Image CallFoldCall;
    public Image CallFoldFold;
    public GameObject AllInOver;
    public GameObject FoldAllInOver;
    public GameObject BetInAni;
    public List<GameObject> buttons;
    public bool isAnimating = false;
    public Image betWords;
    public TextMeshProUGUI bettingWords;
    public bool StillLoadingTurn = false;
    private GameManager gameManager;

    void Start()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        gameManager = FindFirstObjectByType<GameManager>();
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
        CallFoldNone.gameObject.SetActive(false);
        CallFoldCall.gameObject.SetActive(false);
        CallFoldFold.gameObject.SetActive(false);
        betWords.gameObject.SetActive(false);
    }

    public void OptionOne()
    {
        if (!StillLoadingTurn)
        {
            StillLoadingTurn = true;
            if (AllInTrigger && !BetIsSet) { AllInChosen(); }
            else if (AllInTrigger && BetIsSet) { return; }
            else if (BetIsSet) { RaiseChosen(); }
            else { BetChosen(); }
        }
    }

    public void OptionTwo()
    {
        if (!StillLoadingTurn)
        {
            StillLoadingTurn = true;
            if (pokerTurnManager.isAllIn[0]) { CallChosen(); }
            else if (BetIsSet) { CallChosen(); }
            else { CheckChosen(); }
        }
    }

    public void OptionThree()
    {
        if (!StillLoadingTurn)
        {
            StillLoadingTurn = true;
            FoldChosen();
        }
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
                battleManager.HUDs[i].GetComponent<HUD>().isBetter = false;
            }
        pokerTurnManager.HasChecked[pokerTurnManager.turnOrder[2]] = true;
        pokerChipManager.BetToThePot(pokerTurnManager.turnOrder[2], (1 + pokerChipManager.BetSize - pokerChipManager.InThePot[pokerTurnManager.turnOrder[2]]));
        pokerChipManager.BetSize += 1;
        battleManager.HUDs[pokerTurnManager.turnOrder[2]].GetComponent<HUD>().isBetter = true;
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
        Debug.Log("FOLD BUTTON CALLED " + player);
        pokerTurnManager.IsOut[player] = true;
        pokerTableCards.Fold(player);
        gameManager.characters[player].isFolding = true;
        //fold powers
        NextPlayer();
    }

    private void AllInChosen()
    { 
        BetChosen();
        //ALL IN set isALLIN bool to true
        //As bet, but amount = current chips
        //must have less chips then the enemy
        // add in functionality to tickturn to move through the turns at end of round
    }


    private IEnumerator DelayEnemyTurn()
    {
        isAnimating = true;

        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();

        if (pokerTurnManager.isShowdownInProgress)
        {
            Debug.Log("DelayEnemyTurn canceled: showdown in progress.");
            yield break;
        }

        ShiftScene shiftScene = FindFirstObjectByType<ShiftScene>();
        shiftScene.ShiftTheScene();
        yield return new WaitForSeconds(0.5f); // 30 frames at 60 FPS
        betWords.gameObject.SetActive(true);
        bettingWords.text = "...";
        yield return new WaitForSeconds(2f);
        int monsterChoice = monsterManager.MonsterDecision();
        if (monsterChoice == 0) { 
            bettingWords.text = "I'll BET!";
            yield return new WaitForSeconds(2f);
            BetChosen();
        }
        if (monsterChoice == 1) { 
            bettingWords.text = "I Call";
            yield return new WaitForSeconds(2f);
            CallChosen();
        }
        if (monsterChoice == 2) { 
            bettingWords.text = "Hmm...  Check.";
            yield return new WaitForSeconds(2f);
            CheckChosen();
        }
        if (monsterChoice == 3) { 
            bettingWords.text = "...I Fold";
            yield return new WaitForSeconds(2f); 
            FoldChosen();
        }
        if (monsterChoice == 4) { Debug.Log("Monster is Fleeing"); }
        isAnimating = false;
    }

    private IEnumerator DelayButtonDisplay()
    {
        ShiftScene shiftScene = FindFirstObjectByType<ShiftScene>();
        shiftScene.ShiftTheScene();
        yield return new WaitForSeconds(0.5f);
        BetInAni.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        isAnimating = false;
        if (AllInTrigger && !BetIsSet) { UpdateButtonDisplay(3); }
        else if (AllInTrigger && BetIsSet) { UpdateButtonDisplay(5); }
        else if (pokerTurnManager.isAllIn[0]) { UpdateButtonDisplay(6); }
        else if (!BetIsSet) { UpdateButtonDisplay(1); }
        else { UpdateButtonDisplay(2); }
        StillLoadingTurn = false;
}


    public void UpdateButtonDisplay(int displaySwitch)
    {
        allButtonOff();
        if (pokerTurnManager.turnOrder[2]==0)
        {
            StillLoadingTurn = true;
            StartCoroutine(DelayEnemyTurn());
            return;
        }
        if (displaySwitch == 0)
        {
            StillLoadingTurn = true;
            isAnimating = true;
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
            CallRaise.gameObject.SetActive(true);
        }
        if (displaySwitch == 22)
        {
            CallCall.gameObject.SetActive(true);
        }
        if (displaySwitch == 23)
        {
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
            ShiftScene shiftScene = FindFirstObjectByType<ShiftScene>();
            shiftScene.ShiftTheScene();
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
        if (displaySwitch == 6)
        {
            CallFoldNone.gameObject.SetActive(true);
        }
        if (displaySwitch == 62)
        {
            CallFoldCall.gameObject.SetActive(true);
        }
        if (displaySwitch == 63)
        {
            CallFoldFold.gameObject.SetActive(true);
        }
    }

    //side bets? hit sidebet amount?
    // up to 2 needed

}
