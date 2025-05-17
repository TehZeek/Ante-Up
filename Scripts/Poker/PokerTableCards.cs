using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PokerTableCards : MonoBehaviour
{
    public GameObject cardPrefab; //Assign Card Prefab in inspector
    public Transform p1Transform;
    public Transform p2Transform;
    public Transform p3Transform;
    public Transform monTransform;
    public Transform tableTransform;
    public Transform burnTransform;
    public float cardSpacing = 150f;
    public float verticalSpacing = 25f;
    public float fanSpread = -5f;
    private PokerDrawPile pokerDrawPile;
    public GameObject newCard;
    public List<GameObject> playerOnePocket = new List<GameObject>();
    public List<GameObject> playerTwoPocket = new List<GameObject>();
    public List<GameObject> playerThreePocket = new List<GameObject>();
    public List<GameObject> monsterPocket = new List<GameObject>();
    public List<GameObject> tableHand = new List<GameObject>();
    public List<GameObject> burnDeck = new List<GameObject>();
    // import this from PokerDrawPile public List<GameObject> cardsInHand = new List<GameObject>(); // hold the list of card objects in our hand

    void Start()
    {
        PokerDrawPile pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
    }

    public void Fold(int player)
    {
        if (player == 0)
        {
            for (int i = 0; i < monsterPocket.Count; i++)
            {
                burnDeck.Add(monsterPocket[i]);
            }
            monsterPocket.Clear();
            for (var i = monTransform.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(monTransform.transform.GetChild(i).gameObject);
            }
            //go to player[0] is out / end round
        }
        if (player == 1)
        {
            for (int i = 0; i < playerOnePocket.Count; i++)
            {
                burnDeck.Add(playerOnePocket[i]);
            }
            playerOnePocket.Clear();
            for (var i = p1Transform.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(p1Transform.transform.GetChild(i).gameObject);
            }
            //go to player[1] is out
        }
        if (player == 2)
        {
            for (int i = 0; i < playerTwoPocket.Count; i++)
            {
                burnDeck.Add(playerTwoPocket[i]);
            }
            playerTwoPocket.Clear();
            for (var i = p2Transform.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(p2Transform.transform.GetChild(i).gameObject);
            }
            //go to player[2] is out
        }
        if (player == 3)
        {
            for (int i = 0; i < playerThreePocket.Count; i++)
            {
                burnDeck.Add(playerThreePocket[i]);
            }
            playerThreePocket.Clear();
            for (var i = p3Transform.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(p3Transform.transform.GetChild(i).gameObject);
            }
            //go to player[3] is out
        }
        UpdateTableVisuals();
    }

    public void AddCardToPosition(Card cardData, int whichPlayer)
    {
        //make the card
        if (whichPlayer == 0)
        {
            GameObject newCard = Instantiate(cardPrefab, monTransform.position, Quaternion.identity, monTransform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            newCard.GetComponent<CardDisplay>().CardBack.gameObject.SetActive(true);
            monsterPocket.Add(newCard);
        }
        if (whichPlayer == 1)
        {
            GameObject newCard = Instantiate(cardPrefab, p1Transform.position, Quaternion.identity, p1Transform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            playerOnePocket.Add(newCard);

        }
        if (whichPlayer == 2)
        {
            GameObject newCard = Instantiate(cardPrefab, p2Transform.position, Quaternion.identity, p2Transform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            playerTwoPocket.Add(newCard);

        }
        if (whichPlayer == 3)
        {
            GameObject newCard = Instantiate(cardPrefab, p3Transform.position, Quaternion.identity, p3Transform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            playerThreePocket.Add(newCard);

        }
        if (whichPlayer >3 && whichPlayer < 9)
        {
            GameObject newCard = Instantiate(cardPrefab, tableTransform.position, Quaternion.identity, tableTransform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            tableHand.Add(newCard);

        }
        if (whichPlayer == 9)
        {
            GameObject newCard = Instantiate(cardPrefab, burnTransform.position, Quaternion.identity, burnTransform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            newCard.GetComponent<CardDisplay>().CardBack.gameObject.SetActive(true);
            burnDeck.Add(newCard);
        }
        UpdateTableVisuals();


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
        for (var i = burnTransform.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(burnTransform.transform.GetChild(i).gameObject);
        }
    }
    
    public void UpdateTableVisuals()
    {
        UpdateTV(monsterPocket);
        UpdateTV(playerOnePocket);
        UpdateTV(playerTwoPocket);
        UpdateTV(playerThreePocket);
        UpdateTV(tableHand);
    }

    public void UpdateTV(List<GameObject> Cards)
    {
        int cardCount = Cards.Count;
        if (cardCount == 1)
        {
            Cards[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            Cards[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            Cards[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));

            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);

            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);


            Cards[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);

        }

    }

    public void ShowdownReveal()
    {
        int cardCount = monsterPocket.Count;
            for (int i = 0; i < cardCount; i++)
        {
            monsterPocket[i].GetComponent<CardDisplay>().CardBack.gameObject.SetActive(false);
        }
    }

    public void CompareHands()
    {
        PokerHandCompare pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        int guy = 0;
        int guy2 = 1;
        int guy3 = 2;
        int guy4 = 3;

        pokerHandCompare.UpdateHandType(monsterPocket, tableHand, guy);
        pokerHandCompare.UpdateHandType(playerOnePocket, tableHand, guy2);
        pokerHandCompare.UpdateHandType(playerTwoPocket, tableHand, guy3);
        pokerHandCompare.UpdateHandType(playerThreePocket, tableHand, guy4);

    }

}


