using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card cardData;
    public Image cardBG;
    public Image cardBG2;
    public Image cardBG3;
    public Image cardBG4;
    public Image[] pipImages;
    public Image[] rankImage;
    public Image[] suitImage;
    public Image faceImage;
    public Image CardBack;
    private Color[] cardColors = {
        new Color(0f, 0f, 0f), //Spade
        new Color(0.60f, 0.15f, 0.15f), //Diamond
        new Color(0.60f, 0.15f, 0.15f), //Heart
        new Color(0f, 0f, 0f), //Club
    };

    void Start()
    {
        UpdateCardDisplay();
    }

    public int GetRandomNumber()
    {
        return Random.Range(0, 4); // Upper bound is exclusive, so this returns 0, 1, 2, or 3.
    }


    public void UpdateCardDisplay()
    {
        cardBG.gameObject.SetActive(false);
        cardBG2.gameObject.SetActive(false);
        cardBG3.gameObject.SetActive(false);
        cardBG4.gameObject.SetActive(false);



        int whichBG = GetRandomNumber();
        if (whichBG == 0)
        {
            cardBG.gameObject.SetActive(true);
        }
        else if (whichBG == 1)
        {
            cardBG2.gameObject.SetActive(true);
        }
        else if (whichBG == 2)
        {
            cardBG3.gameObject.SetActive(true);
        }
        else if (whichBG == 3)
        {
            cardBG4.gameObject.SetActive(true);
        }
        // pull the face image, pips, rank image, and suit images
        rankImage[0].color = cardColors[(int)cardData.cardSuit[0]];
        rankImage[1].color = cardColors[(int)cardData.cardSuit[0]];

        rankImage[0].sprite = cardData.numberSprite;
        rankImage[1].sprite = cardData.numberSprite;
        suitImage[0].sprite = cardData.suitSprite;
        suitImage[1].sprite = cardData.suitSprite;

        // find the card's rank
        // if the card rank is between 2 and 10
        // turn off the faceSprite
        // turn on the appropriate pips
        // set the rankSprite for each pip


        if ((int)cardData.cardRank[0] > 0 && (int)cardData.cardRank[0] < 10)
        {
            faceImage.gameObject.SetActive(false);

            for (int i = 0; i < pipImages.Length; i++)
            {
                pipImages[i].sprite = cardData.rankSprite;
            }

            PipDisplay((int)cardData.cardRank[0]);
        }
        else 
        {
            faceImage.sprite = cardData.faceSprite;
        }
    }

    private void PipDisplay(int theRank)
    {
        Debug.Log(theRank);
        if (theRank == 1)
        {
            pipImages[0].gameObject.SetActive(true);
            pipImages[2].gameObject.SetActive(true);
        }
        if (theRank == 2)
        {
            pipImages[0].gameObject.SetActive(true);
            pipImages[1].gameObject.SetActive(true);
            pipImages[2].gameObject.SetActive(true);
        }
        if (theRank == 3)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
        }
        if (theRank == 4)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[1].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
        }

        if (theRank == 5)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[11].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[12].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
        }
        if (theRank == 6)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[11].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[12].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
            pipImages[0].gameObject.SetActive(true);
        }


        if (theRank == 7)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[11].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[12].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
            pipImages[0].gameObject.SetActive(true);
            pipImages[2].gameObject.SetActive(true);
        }
        if (theRank == 8)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[4].gameObject.SetActive(true);
            pipImages[5].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[8].gameObject.SetActive(true);
            pipImages[9].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
            pipImages[1].gameObject.SetActive(true);
        }
        if (theRank == 9)
        {
            pipImages[3].gameObject.SetActive(true);
            pipImages[4].gameObject.SetActive(true);
            pipImages[5].gameObject.SetActive(true);
            pipImages[6].gameObject.SetActive(true);
            pipImages[7].gameObject.SetActive(true);
            pipImages[8].gameObject.SetActive(true);
            pipImages[9].gameObject.SetActive(true);
            pipImages[10].gameObject.SetActive(true);
            pipImages[0].gameObject.SetActive(true);
            pipImages[2].gameObject.SetActive(true);
        }
    }

}

// 3   7
//   0
// 4   8
// 11 1 12
// 5   9
//   2
// 6   10