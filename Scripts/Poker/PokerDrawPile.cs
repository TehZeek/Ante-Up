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
    public List<Card> tempDrawPile = new List<Card>();
    private int currentIndex = 0;
    public List<int> PocketSize = new List<int>();
    public int playerOnePocketSize = 2;
    public int playerTwoPocketSize = 2;
    public int playerThreePocketSize = 2;
    public int monsterPocketSize = 2;
    private int tableFlopSize = 3;
    private int tableTurnSize = 1;
    private int tableRiverSize = 1;
    //private List<int> pocketCards = new List<int>();
    // set up a bool for various extra turns and sizes

    private PokerTableCards pokerTableCards;
    private PokerBurnPile pokerBurnPile;
    private PokerChipManager pokerChipManager;
    private GameManager gameManager;
    private MonsterManager monsterManager;

    void Start()
    {
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
        monsterManager = FindFirstObjectByType<MonsterManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        monsterPocketSize = monsterManager.monster.pocketSize;
    }

    // find what passes this from the DeckManager
    public void MakeDrawPile(List<Card> cardsToAdd)
    {
        Debug.Log("Making Draw Pile with "+cardsToAdd.Count);
        drawPile.AddRange(cardsToAdd);
        Debug.Log("Draw Pile Made: "+drawPile.Count);
        tempDrawPile.AddRange(drawPile);
        Debug.Log("temp Draw Pile made with "+tempDrawPile.Count);
        //    UpdateDrawPileCount();
        Utility.Shuffle(drawPile);
        Debug.Log("Draw Pile Shuffled");
    }

    public void Reshuffle()
    {
        drawPile.Clear();
        pokerTableCards.playerOnePocket.Clear();
        pokerTableCards.playerTwoPocket.Clear();
        pokerTableCards.playerThreePocket.Clear();
        pokerTableCards.monsterPocket.Clear();
        pokerTableCards.tableHand.Clear();
        pokerTableCards.burnDeck.Clear();
        pokerTableCards.ClearTable();
        drawPile.AddRange(tempDrawPile);
        Utility.Shuffle(drawPile);
    }

    public void DealPocketCards()
    {
            for (int i = 0; i < monsterPocketSize; i++)
            {
                DealCard(0);
                currentIndex = (currentIndex + 1) % drawPile.Count;
            }


        for (int i = 0; i < playerOnePocketSize; i++)
        {
            DealCard(1);
            currentIndex = (currentIndex + 1) % drawPile.Count;
        }
        for (int i = 0; i < playerTwoPocketSize; i++)
        {
            DealCard(2);
            currentIndex = (currentIndex + 1) % drawPile.Count;
        }
        for (int i = 0; i < playerThreePocketSize; i++)
        {
            DealCard(3);
            currentIndex = (currentIndex + 1) % drawPile.Count;
        }

    }





    public void BurnCard()
    {
        DealCard(9);
        currentIndex = (currentIndex + 1) % drawPile.Count;

    }

    public void DealFlopCards()
    {
        StartCoroutine(DealFlopWithDelay());
    }

    private IEnumerator DealFlopWithDelay()
    {
        BurnCard();

        for (int i = 0; i < tableFlopSize; i++)
        {
            int nextcard = (i + 4);
            DealCard(nextcard);
            currentIndex = (currentIndex + 1) % drawPile.Count;

            yield return new WaitForSeconds(0.5f); // Wait 0.5 seconds before dealing the next card
        }
    }



    public void DealTurnCards()
    {
        BurnCard();
        for (int i = 0; i < tableTurnSize; i++)
        {
            DealCard(7);
            currentIndex = (currentIndex + 1) % drawPile.Count;
        }

    }

    public void DealRiverCards()
    {
        BurnCard();
        for (int i = 0; i < tableRiverSize; i++)
        {
            DealCard(8);
            currentIndex = (currentIndex + 1) % drawPile.Count;

        }
    }

    public void DealBonusCards()
    {
        BurnCard();
        for (int i = 0; i < tableRiverSize; i++)
        {
            DealCard(10);
            currentIndex = (currentIndex + 1) % drawPile.Count;
        }

    }

    public void DealCard(int cardPlacement)
    {
        Card nextCard = drawPile[currentIndex];
        pokerTableCards.AddCardToPosition(nextCard, cardPlacement);
        drawPile.RemoveAt(currentIndex);
    }



}
