using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.CompilerServices;

public class PokerDrawPile : MonoBehaviour
{
    public List<Card> drawPile = new List<Card>();
    private int currentIndex = 0;
    public List<Card> playerOnePocket = new List<Card>();
    public List<Card> playerTwoPocket = new List<Card>();
    public List<Card> playerThreePocket = new List<Card>();
    public List<Card> monsterPocket = new List<Card>();
    public List<Card> tableHand = new List<Card>();
    public List<Card> burnDeck = new List<Card>();
    public int playerOnePocketSize = 2;
    public int playerTwoPocketSize = 2;
    public int playerThreePocketSize = 2;
    public int monsterPocketSize = 2;
    private int tableFlopSize = 3;
    private int tableTurnSize = 1;
    private int tableRiverSize = 1;
    private List<int> pocketCards = new List<int>();
    // set up a bool for various extra turns and sizes

    private PokerTableCards pokerTableCards;
    private PokerBurnPile pokerBurnPile;
    public TextMeshProUGUI drawPileCounter;
    private GameManager gameManager;

    void Start()
    {

    }

    // find what passes this from the DeckManager
    public void MakeDrawPile(List<Card> cardsToAdd)
    {
        drawPile.AddRange(cardsToAdd);
    //    UpdateDrawPileCount();
        Utility.Shuffle(drawPile);
    }

    public void Shuffle(List<Card> cardsToAdd)
    {
        drawPile.Clear();
        playerOnePocket.Clear();
        playerTwoPocket.Clear();
        playerThreePocket.Clear();
        monsterPocket.Clear();
        tableHand.Clear();
        burnDeck.Clear();
        drawPile.AddRange(cardsToAdd);
        //    UpdateDrawPileCount();
        Utility.Shuffle(drawPile);
    }

    public void addPocketCards()
    {
        pocketCards.Clear(); // Ensure the list is empty before adding new values
        pocketCards.Add(monsterPocketSize);
        pocketCards.Add(playerOnePocketSize);
        pocketCards.Add(playerTwoPocketSize);
        pocketCards.Add(playerThreePocketSize);
        DealPocketCards();
    }




    public void DealPocketCards()
    {
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();

        for (int i = 0; i < pocketCards.Count; i++)
        {

            for (int j = 0; j < pocketCards[i]; j++)
            {
                Card nextCard = drawPile[currentIndex];
                if (i == 0) { DealCard(pokerTableCards, monsterPocket); }
                if (i == 1) { DealCard(pokerTableCards, playerOnePocket); }
                if (i == 2) { DealCard(pokerTableCards, playerTwoPocket); }
                if (i == 3) { DealCard(pokerTableCards, playerThreePocket); }
                currentIndex = (currentIndex + 1) % drawPile.Count;
            }
        }

    }

    public void DealFlopCards()
    {
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        for (int i = 0; i < tableFlopSize; i++)
        {
            Card nextCard = drawPile[currentIndex];
            DealCard(pokerTableCards, tableHand);
            currentIndex = (currentIndex + 1) % drawPile.Count;

        }

    }

    public void DealTurnCards()
    {
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        for (int i = 0; i < tableTurnSize; i++)
        {
            Card nextCard = drawPile[currentIndex];
            DealCard(pokerTableCards, tableHand);
            currentIndex = (currentIndex + 1) % drawPile.Count;
        }

    }

    public void DealRiverCards()
    {
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        for (int i = 0; i < tableRiverSize; i++)
        {
            Card nextCard = drawPile[currentIndex];
            DealCard(pokerTableCards, tableHand);
            currentIndex = (currentIndex + 1) % drawPile.Count;

        }

    }

    public void DealCard(PokerTableCards pokerTableCards, List<Card> cardPlacement)
    {
        Card nextCard = drawPile[currentIndex];
        pokerTableCards.AddCardToPosition(nextCard, cardPlacement);
        drawPile.RemoveAt(currentIndex);
    }

}
