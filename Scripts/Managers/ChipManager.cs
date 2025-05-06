using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChipManager : MonoBehaviour
{
    private GameManager gameManager;
    public TextMeshProUGUI chipDisplay;
    private int chips;
    public int startingChips = 50;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }
    
    public void spendChips(int chipsSpent)
    {
        chips = gameManager.PartyChips;
        chips -= chipsSpent;
        UpdateChipsDisplay();
    }

    public void winChips(int chipsGained)
    {
        chips = gameManager.PartyChips;
        chips += chipsGained;
        UpdateChipsDisplay();
    }

    public void DungeonSetup()
    {
        chips = startingChips;
        UpdateChipsDisplay();
        DrawPileManager dpman = FindFirstObjectByType<DrawPileManager>();
        dpman.suppliesCost = 1;
        dpman.suppliesCostCounter = 0;
        dpman.suppliesCostDisplay.text = new string("Supply Cost\n 1 Chip");
    }

    public void UpdateChipsDisplay()
    {
        chipDisplay.text = chips.ToString();
        gameManager.PartyChips = chips;

    }
}
