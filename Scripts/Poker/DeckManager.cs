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

    void Start()
    {
        //Load all card assets from the Map Folder
        Card[] Cards = Resources.LoadAll<Card>("CardData");
        //add the loaded cards to the allCards list
        allCards.AddRange(Cards);
    }



    public void DrawCard(MapHandManager mapHandManager)
    {
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
