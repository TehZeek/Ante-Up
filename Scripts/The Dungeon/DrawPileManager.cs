using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.CompilerServices;


public class DrawPileManager : MonoBehaviour
{
    public List<MapCard> drawPile = new List<MapCard>();
    private int currentIndex = 0;

    private MapHandManager mapHandManager;
    private DiscardManager discardManager;
    public TextMeshProUGUI drawPileCounter;
    private int maxHandSize;
    private bool justOnce = true;
    private GameManager gameManager;
    private ChipManager chipManager;
    public int suppliesCost = 1;
    public int suppliesCostCounter = 0;
    public TextMeshProUGUI suppliesCostDisplay;


    void Start()
    {
    }

    void Awake()
    {
        if (mapHandManager == null)
        {
            mapHandManager = FindFirstObjectByType<MapHandManager>();
        }
    }



    public void MakeDrawPile(List<MapCard> cardsToAdd)
    {
        if (justOnce)
        {
            justOnce = false;
            drawPile.AddRange(cardsToAdd);
            UpdateDrawPileCount();
            Utility.Shuffle(drawPile);
        }
        else { Debug.Log("Tried to add another deck"); }

        
    }

    public void DungeonSetup(int numberOfCardsToDraw, int setMaxHandSize)
    {
        maxHandSize = setMaxHandSize;
        for (int i = 0; i < numberOfCardsToDraw; i++)
        {
            MapCard nextCard = drawPile[currentIndex];
            DrawMapCard(mapHandManager);
            currentIndex = (currentIndex+1) % drawPile.Count;
        }

    }

    public void DrawMapCard(MapHandManager mapHandManager)
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager.PartyChips < suppliesCost) {return; }
        if (mapHandManager.cardsInHand.Count == maxHandSize) { return; }
        if (drawPile.Count == 0)
        {
        //    RefillDeckFromDiscard();
        }
        else
        {
            MapCard nextCard = drawPile[currentIndex];
            mapHandManager.AddMapCardToHand(nextCard);
            drawPile.RemoveAt(currentIndex);
            chipManager = FindFirstObjectByType<ChipManager>();
            chipManager.spendChips(suppliesCost);
            UpdateSuppliesCost();
            UpdateDrawPileCount();
            if (drawPile.Count > 0)
            {
                currentIndex = (currentIndex + 1) % drawPile.Count;
            }
            
        }
    }
    public void UpdateSuppliesCost()
    {
        suppliesCostCounter++;
        if (suppliesCostCounter ==3)
        {
            suppliesCostCounter = 0;
            suppliesCost++;
            if (suppliesCost > 1) { suppliesCostDisplay.text = new string("Supply Cost\n" + suppliesCost + " Chips"); }
            else { suppliesCostDisplay.text = new string("Supply Cost\n" + suppliesCost + " Chip"); }
        }
    }

    private void UpdateDrawPileCount()
    {
        drawPileCounter.text = drawPile.Count.ToString();
    }

    private void RefillDeckFromDiscard()
    {
        if (discardManager == null)
        {
            discardManager = FindFirstObjectByType<DiscardManager>();
        }
        if (discardManager != null && discardManager.discardCardsCount > 0)
        {
            drawPile = discardManager.PullAllFromDiscard();
            Utility.Shuffle(drawPile);
            currentIndex = 0;
        }


    }

}
