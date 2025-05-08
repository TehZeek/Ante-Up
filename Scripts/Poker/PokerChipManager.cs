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
    private int p1Chips;
    private int p2Chips;
    private int p3Chips;
    private int partyChips;
    private int monChips;
    private int potChips;
    public TextMeshProUGUI p1ChipDisplay;
    public TextMeshProUGUI p2ChipDisplay;
    public TextMeshProUGUI p3ChipDisplay;
    public TextMeshProUGUI monChipDisplay;
    public TextMeshProUGUI potChipDisplay;


    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        partyChips = gameManager.PartyChips;
        monChips = battleManager.monster.monsterChips;

        p1Chips = (int)Mathf.FloorToInt(partyChips / 3);
        p2Chips = (int)Mathf.Floor((partyChips - p1Chips) / 2);
        p3Chips = (int)(partyChips - p1Chips - p2Chips);
        UpdateChipDisplay();
    }

    public void UpdateChipDisplay()
    {
        p1ChipDisplay.text = p1Chips.ToString();
        p2ChipDisplay.text = p2Chips.ToString();
        p3ChipDisplay.text = p3Chips.ToString();
        monChipDisplay.text = monChips.ToString();
    }

    public void BetToThePot(int player, int amount)
    {
        if(player == 0)
        {
            if (monChips > amount)
            {
                monChips -= amount;
                potChips += amount;
            }
            else { NotEnoughChips(player); }
        }
        if (player == 1)
        {
            if (p1Chips>amount)
            {
                p1Chips -= amount;
                potChips += amount;
            }
            else { NotEnoughChips(player); }
        }
        if (player == 2)
        {
            if (p2Chips > amount)
            {
                p2Chips -= amount;
                potChips += amount;
            }
            else { NotEnoughChips(player); }
        }
        if (player == 3)
        {
            if (p3Chips > amount)
            {
                p3Chips -= amount;
                potChips += amount;
            }
            else { NotEnoughChips(player); }
        }
        UpdateChipDisplay();
    }

    private void NotEnoughChips(int player)
    {
    //let's add some all in logic here, or game over, or just a denial of chips
    Debug.Log("Not enough Chips for Player " + player);
    }

    public void SplitThePot (List<int> players)
{
    int splitAmount = (int)Mathf.Floor(potChips / players.Count);
    int extraAmount = potChips - splitAmount;
    for (int i = 0; i < players.Count; i++)
    {
        if (players[i] == 0) { monChips += splitAmount; }
        if (players[i] == 1) { p1Chips += splitAmount; }
        if (players[i] == 2) { p2Chips += splitAmount; }
        if (players[i] == 3) { p3Chips += splitAmount; }
    }
    if (extraAmount > 0 && players[0] != 0)
    {
        if (p1Chips < p2Chips)
        { 
            if (p1Chips < p3Chips)
            {
                p1Chips += extraAmount;
            }
            else if (p3Chips < p2Chips)
            {
                p3Chips += extraAmount;
            }
        }
        else if (p2Chips < p3Chips)
        {
            p2Chips += extraAmount;
        }
        else { p3Chips += extraAmount; }
    }
    UpdateChipDisplay();
}
    public void StealFromPot (int player, int amount)
{
    if (player == 0)
    {
        if (potChips >= amount)
        {
            monChips += amount;
            potChips -= amount;
        }
        else
        {
            monChips += potChips;
            potChips = 0;
        }

    }
    if (player == 1)
    {
        if (potChips >= amount)
        {
            p1Chips += amount;
            potChips -= amount;
        }
        else
        {
            p1Chips += potChips;
            potChips = 0;
        }

    }
    if (player == 2)
    {
        if (potChips >= amount)
        {
            p2Chips += amount;
            potChips -= amount;
        }
        else
        {
            p2Chips += potChips;
            potChips = 0;
        }

    }
    if (player == 3)
    {
        if (potChips >= amount)
        {
            p3Chips += amount;
            potChips -= amount;
        }
        else
        {
            p3Chips += potChips;
            potChips = 0;
        }

    }
        UpdateChipDisplay();

    }

    public void StealFromOtherSide (int player, int stealsFrom, int amount)
    {
        if (player == 0)
        {
            if (stealsFrom == 1)
            {
                if (p1Chips >= amount)
                { 
                    p1Chips -= amount;
                    monChips += amount;
                }
                else
                {
                    monChips += p1Chips;
                    p1Chips = 0;
                    // battleManager.PlayerDead(1);
                }
            }
            if (stealsFrom == 2)
            {
                if (p2Chips >= amount)
                {
                    p2Chips -= amount;
                    monChips += amount;
                }
                else
                {
                    monChips += p2Chips;
                    p2Chips = 0;
                    // battleManager.PlayerDead(2);
                }
            }
            if (stealsFrom == 3)
            {
                if (p3Chips >= amount)
                {
                    p3Chips -= amount;
                    monChips += amount;
                }
                else
                {
                    monChips += p3Chips;
                    p3Chips = 0;
                    // battleManager.PlayerDead(3);
                }
            }
        }
        else if (player == 1)
        {
            if (monChips >= amount)
            {
                monChips -= amount;
                p1Chips += amount;
            } else
            {
                p1Chips += monChips;
                monChips = 0;
                //battleManager.MonsterDead();
            }
        }
        else if (player == 2)
        {
            if (monChips >= amount)
            {
                monChips -= amount;
                p2Chips += amount;
            }
            else
            {
                p2Chips += monChips;
                monChips = 0;
                //battleManager.MonsterDead();
            }
        }
        else if (player == 3)
        {
            if (monChips >= amount)
            {
                monChips -= amount;
                p3Chips += amount;
            }
            else
            {
                p3Chips += monChips;
                monChips = 0;
                //battleManager.MonsterDead();
            }
        }
        UpdateChipDisplay();
    }

    public void LosesChip(int player, int amount)
    {
        if (player == 0)
        {
            if (monChips > amount)
            {
                monChips -= amount;
            }
            else {
                monChips = 0;
                //battleManager.MonsterDead();
                  }
        }
        if (player == 1)
        {
            if (p1Chips > amount)
            {
                p1Chips -= amount;
            }
            else {
                p1Chips = 0;
                // battleManager.PlayerDead(1);
            }
        }
        if (player == 2)
        {
            if (p2Chips > amount)
            {
                p2Chips -= amount;
            }
            else {
                p2Chips = 0;
                // battleManager.PlayerDead(2);
            }
        }
        if (player == 3)
        {
            if (p3Chips > amount)
            {
                p3Chips -= amount;
            }
            else {
                p3Chips = 0;
                // battleManager.PlayerDead(3);

            }
        }
        UpdateChipDisplay();
    }

    public void GainsChip(int player, int amount)
    {
        if (player == 0)
        {
                monChips += amount;
        }
        if (player == 1)
        {
            p1Chips += amount;
        }
        if (player == 2)
        {
            p2Chips += amount;
        }
        if (player == 3)
        {
            p3Chips += amount;
        }
        UpdateChipDisplay();
    }










}
