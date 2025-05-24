using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour

{
    public List<Card> allCards = new List<Card>();
    private int currentIndex = 0;
    private GameManager gameManager;
    private ChipManager chipManager;
    private PokerDrawPile pokerDrawPile;

    void Start()
    {
        //Load all card assets from the Map Folder
        Card[] Cards = Resources.LoadAll<Card>("CardData");
        //add the loaded cards to the allCards list
        allCards.AddRange(Cards);
        gameManager = FindFirstObjectByType<GameManager>();
        pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
    }


    public void BattleSetup()
    {
        pokerDrawPile.MakeDrawPile(allCards);
    }


    public void DrawCard(MapHandManager mapHandManager)
    {
        // we're going to move this and it doesn't work anyways
        if (allCards.Count == 0 )
        {
            return;
        }
        else
        {
            Card nextCard = allCards[currentIndex];
            mapHandManager.AddCardToHand(nextCard);
            currentIndex = (currentIndex + 1) % allCards.Count;
        }
    }
}
