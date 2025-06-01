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
    public PokerDrawPile pokerDrawPile;

    public void BattleSetup()
    {
        Card[] Cards = Resources.LoadAll<Card>("CardData");
        allCards.AddRange(Cards);
        Debug.Log("Trying to make draw pile with " + allCards.Count);
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
