using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class PokerChipManager : MonoBehaviour
{
    // to add, a script for not having enough money to bet
    private GameManager gameManager;
    private BattleMenu battleMenu;
    private PokerTurnManager pokerTurnManager;

    public int[] playerChips = new int[] { 0, 0, 0, 0 };
    private int partyChips;
    public int potChips;
    public int[] InThePot = new int[] { 0, 0, 0, 0 };
    public TextMeshProUGUI p1ChipDisplay;
    public TextMeshProUGUI p2ChipDisplay;
    public TextMeshProUGUI p3ChipDisplay;
    public TextMeshProUGUI monChipDisplay;
    public TextMeshProUGUI potChipDisplay;
    public int anteAmount = 2;
    public int BetSize = 2;


    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        battleMenu = FindFirstObjectByType<BattleMenu>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        partyChips = gameManager.PartyChips;
        playerChips[0] = battleManager.monster.monsterChips;
        playerChips[1] = (int)Mathf.FloorToInt(partyChips / 3);
        playerChips[2] = (int)Mathf.Floor((partyChips - playerChips[1]) / 2);
        playerChips[3] = (int)(partyChips - playerChips[1] - playerChips[2]);
        UpdateChipDisplay();
    }

    public void UpdateChipDisplay()
    {
        p1ChipDisplay.text = playerChips[1].ToString();
        p2ChipDisplay.text = playerChips[2].ToString();
        p3ChipDisplay.text = playerChips[3].ToString();
        monChipDisplay.text = playerChips[0].ToString();
        potChipDisplay.text = potChips.ToString();
        //throw in those visual sprite chips here
    }

    public void BetToThePot(int player, int amount)
    {
            if (playerChips[player] > amount)
            {
                playerChips[player] -= amount;
                potChips += amount;
                InThePot[player] += amount;
            }
        else 
            {
            potChips += playerChips[player];
            InThePot[player] += playerChips[player];
            playerChips[player] = 0;
            NotEnoughChips(player);
            }

            UpdateChipDisplay();
    }

    private void NotEnoughChips(int player)
    {
        pokerTurnManager.isAllIn[player] = true;
        pokerTurnManager.IsOut[player] = true;
    }

    public void SplitThePot (List<int> players)
{
    int splitAmount = (int)Mathf.Floor(potChips / players.Count);
    int extraAmount = potChips - splitAmount;
    for (int i = 0; i < players.Count; i++)
        {
            playerChips[i] += splitAmount; 
        }
    if (extraAmount > 0 && players[0] != 0)
    {
        if (playerChips[1] < playerChips[2])
        { 
            if (playerChips[1] < playerChips[3])
            {
                    playerChips[1] += extraAmount;
            }
            else if (playerChips[3] < playerChips[2])
            {
                    playerChips[3] += extraAmount;
            }
        }
        else if (playerChips[2] < playerChips[3])
        {
                playerChips[2] += extraAmount;
        }
        else { playerChips[3] += extraAmount; }
    }
    UpdateChipDisplay();
}
    public void StealFromPot (int player, int amount)
{
        if (potChips >= amount)
        {
            playerChips[player] += amount;
            potChips -= amount;
        }
        else
        {
            playerChips[player] += potChips;
            potChips = 0;
        }
        UpdateChipDisplay();
    }

    public void StealFromOtherSide (int player, int stealsFrom, int amount)
    {
            if (playerChips[stealsFrom] >= amount)
                {
                playerChips[stealsFrom] -= amount;
                playerChips[player] += amount;
                }
                else
                {
                playerChips[player] += playerChips[stealsFrom];
                playerChips[stealsFrom] = 0;
                    // battleManager.PlayerDead(stealsFrom);
                }
        UpdateChipDisplay();
    }

    public void LosesChip(int player, int amount)
    {
            if (playerChips[player] > amount)
            {
            playerChips[player] -= amount;
            }
            else {
            playerChips[player] = 0;
                //battleManager.PlayerDead(player)();
                  }

        UpdateChipDisplay();
    }

    public void GainsChip(int player, int amount)
    {
        playerChips[player] += amount;
        UpdateChipDisplay();
    }










}
