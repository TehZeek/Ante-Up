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
    private MonsterManager monsterManager;
    private BattleManager battleManager;

    public int[] playerChips = new int[] { 0, 0, 0, 0 };
    private int partyChips;
    public int potChips;
    public int[] InThePot = new int[] { 0, 0, 0, 0 };
    private List<TextMeshProUGUI> ChipDisplay = new List<TextMeshProUGUI>();
    public TextMeshProUGUI potChipDisplay;
    public int anteAmount = 2;
    public int BetSize = 2;


    public void StartChips()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        battleMenu = FindFirstObjectByType<BattleMenu>();
        battleManager = FindFirstObjectByType<BattleManager>();
        monsterManager = FindFirstObjectByType<MonsterManager>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        gameManager.SplitPartyChips();
        playerChips = gameManager.playerChips;
    }

    public void UpdateChipDisplay()
    {
        if (battleManager.HUDs.Count > 0)
        {
            ChipDisplay.Clear();
            for (int i = 0; i < battleManager.HUDs.Count; i++)
            {
                ChipDisplay.Add(battleManager.HUDs[i].GetComponent<HUD>().chipsDisplay.GetComponent<TextMeshProUGUI>());
            }
        }
        for (int i = 0; i < ChipDisplay.Count; i++)
        {
            battleManager.HUDs[i].GetComponent<HUD>().DisplayHUDChips(i);
        }

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
    Debug.Log($"Splitting pot of {potChips} between {players.Count} players.");
    int splitAmount = (int)Mathf.Floor(potChips / players.Count);
    int extraAmount = potChips - (splitAmount*players.Count);
    for (int i = 0; i < players.Count; i++)
        {
            playerChips[players[i]] += splitAmount;
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
