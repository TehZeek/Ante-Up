using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class PokerTableCards : MonoBehaviour
{
    public GameObject cardPrefab; //Assign Card Prefab in inspector
    public Transform p1Transform;
    public Transform p2Transform;
    public Transform p3Transform;
    public Transform monTransform;
    public Transform tableTransform;
    public float cardSpacing = 75f;
    private PokerDrawPile pokerDrawPile;
    public GameObject newCard;
    public List<Card> playerOnePocket = new List<Card>();
    public List<Card> playerTwoPocket = new List<Card>();
    public List<Card> playerThreePocket = new List<Card>();
    public List<Card> monsterPocket = new List<Card>();
    public List<Card> tableHand = new List<Card>();
    public List<Card> burnDeck = new List<Card>();
    // import this from PokerDrawPile public List<GameObject> cardsInHand = new List<GameObject>(); // hold the list of card objects in our hand

    void Start()
    {
        PokerDrawPile pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
    }


    public void AddCardToPosition(Card cardData, int whichPlayer)
    {
        //make the card
        if (whichPlayer == 0)
        {
            GameObject newCard = Instantiate(cardPrefab, monTransform.position, Quaternion.identity, monTransform);
            monsterPocket.Add(cardData);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();

        }
        if (whichPlayer == 1)
        {
            GameObject newCard = Instantiate(cardPrefab, p1Transform.position, Quaternion.identity, p1Transform);
            playerOnePocket.Add(cardData);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        }
        if (whichPlayer == 2)
        {
            GameObject newCard = Instantiate(cardPrefab, p2Transform.position, Quaternion.identity, p2Transform);
            playerTwoPocket.Add(cardData);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        }
        if (whichPlayer == 3)
        {
            GameObject newCard = Instantiate(cardPrefab, p3Transform.position, Quaternion.identity, p3Transform);
            playerThreePocket.Add(cardData);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        }
        if (whichPlayer >3 && whichPlayer < 9)
        {
            GameObject newCard = Instantiate(cardPrefab, tableTransform.position, Quaternion.identity, tableTransform);
            tableHand.Add(cardData);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        }
        if (whichPlayer == 9)
        {
            burnDeck.Add(cardData);
        }

    }

    public void ClearTable()
    {
        for (var i = p1Transform.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(p1Transform.transform.GetChild(i).gameObject);
        }
        for (var i = p2Transform.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(p2Transform.transform.GetChild(i).gameObject);
        }
        for (var i = p3Transform.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(p3Transform.transform.GetChild(i).gameObject);
        }
        for (var i = monTransform.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(monTransform.transform.GetChild(i).gameObject);
        }
        for (var i = tableTransform.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(tableTransform.transform.GetChild(i).gameObject);
        }
    }
    
    public void UpdateTableVisuals()
    {
     //   updatetablevisuals(pokerDrawPile.monsterPocket.Count, pokerDrawPile.monsterPocket);
     //   updatetablevisuals(pokerDrawPile.playerOnePocket.Count, pokerDrawPile.playerOnePocket);
     //   updatetablevisuals(pokerDrawPile.playerTwoPocket.Count, pokerDrawPile.playerTwoPocket);
     //   updatetablevisuals(pokerDrawPile.playerThreePocket.Count, pokerDrawPile.playerThreePocket);
     //   updatetablevisuals(pokerDrawPile.tableHand.Count, pokerDrawPile.tableHand);
    }



}


