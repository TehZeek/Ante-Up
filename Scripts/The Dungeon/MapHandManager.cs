using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class MapHandManager : MonoBehaviour
{
    public GameObject cardPrefab; //Assign Card Prefab in inspector
    public GameObject deckPrefab;
    public Transform handTransform; //the root of the hand position
    public float fanSpread = -5f; //how much the hand is spread out
    public float cardSpacing = 75f;
    public float verticalSpacing = 25f;
    public List<GameObject> cardsInHand = new List<GameObject>(); // hold the list of card objects in our hand
    public int maxHandSize;


    public void AddMapCardToHand(MapCard cardData)
    {
        if (cardsInHand.Count < maxHandSize)
        {
            //make the card
            GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
            cardsInHand.Add(newCard);


            //set the cardData of the instantiated card
            newCard.GetComponent<MapDisplay>().mapData = cardData;
            newCard.GetComponent<MapDisplay>().UpdateMapDisplay();

            UpdateMapHandVisuals();
        }
    }

    public void AddCardToHand(Card cardData)
    {
        if (cardsInHand.Count < maxHandSize)
        {
            //make the card
            GameObject newCard = Instantiate(deckPrefab, handTransform.position, Quaternion.identity, handTransform);
            cardsInHand.Add(newCard);


            //set the cardData of the instantiated card
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            UpdateMapHandVisuals();
        }
    }

    void Update()
    {
     //   UpdateMapHandVisuals();
    }

    public void DungeonSetup(int setMaxHandSize)
    {
        maxHandSize = setMaxHandSize;
    }

    public void UpdateMapHandVisuals()
    {
    int cardCount = cardsInHand.Count;

        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
         {
        float rotationAngle = (fanSpread * (i - (cardCount - 1)/2f));
        cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));

            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);

            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);


            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);

         }

    }

}
