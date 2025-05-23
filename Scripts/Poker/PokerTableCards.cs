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
    private BattleManager battleManager;
    // import this from PokerDrawPile public List<GameObject> cardsInHand = new List<GameObject>(); // hold the list of card objects in our hand

    void Start()
    {
        pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        battleManager = FindFirstObjectByType<BattleManager>();
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
        Transform targetTransform = GetTargetTransform(whichPlayer);

        if (targetTransform != null)
        {
            Vector3 spawnPosition = GetRandomEdgePosition();

            GameObject newCard = Instantiate(cardPrefab, spawnPosition, Quaternion.identity, targetTransform);
            newCard.GetComponent<CardDisplay>().cardData = cardData;
            newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
            if (whichPlayer == 0 || whichPlayer == 9) newCard.GetComponent<CardDisplay>().CardBack.gameObject.SetActive(true);
            if (targetTransform == tableTransform)
            {
                LeanTween.move(newCard, targetTransform.position, 1f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() => StartCoroutine(DelayedTableUpdate()));
            }
            else
            {
                // Instantly move non-table cards
                newCard.transform.position = targetTransform.position;
                StartCoroutine(DelayedTableUpdate());
            }

            AddCardToList(whichPlayer, newCard);
        }
    }

    private IEnumerator DelayedTableUpdate()
    {
        yield return new WaitForSeconds(0.5f); // Wait 1 second before updating visuals
        UpdateTableVisuals();
    }


    private Vector3 GetRandomEdgePosition()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        switch (Random.Range(0, 4)) // Pick one of the four edges
        {
            case 0: return Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, screenWidth), 0, 10)); // Bottom edge
            case 1: return Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, screenWidth), screenHeight, 10)); // Top edge
            case 2: return Camera.main.ScreenToWorldPoint(new Vector3(0, Random.Range(0, screenHeight), 10)); // Left edge
            case 3: return Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, Random.Range(0, screenHeight), 10)); // Right edge
            default: return Vector3.zero;
        }
    }

    private Transform GetTargetTransform(int whichPlayer)
    {
        return whichPlayer switch
        {
            0 => monTransform,
            1 => p1Transform,
            2 => p2Transform,
            3 => p3Transform,
            > 3 and < 9 => tableTransform,
            9 => burnTransform,
            10 => tableTransform,
            _ => null
        };
    }

    private void AddCardToList(int whichPlayer, GameObject newCard)
    {
        if (whichPlayer == 0) { monsterPocket.Add(newCard); } 
        else if (whichPlayer == 1) { playerOnePocket.Add(newCard); }
        else if (whichPlayer == 2) { playerTwoPocket.Add(newCard); }
        else if (whichPlayer == 3) { playerThreePocket.Add(newCard); }
        else if ((whichPlayer > 3 && whichPlayer < 9) || whichPlayer == 10) { tableHand.Add(newCard); }
        else if (whichPlayer == 9) { burnDeck.Add(newCard); }
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
        battleManager.UpdateHUD();
    }

    public void UpdateTableVisuals()
    {
        battleManager.UpdateHUD();
        UpdateTV(monsterPocket);
        UpdateTV(playerOnePocket);
        UpdateTV(playerTwoPocket);
        UpdateTV(playerThreePocket);
        UpdateTable(tableHand);
    }

    public void UpdateTV(List<GameObject> Cards)
    {
        int cardCount = Cards.Count;
        if (cardCount == 1)
        {
            LeanTween.moveLocal(Cards[0], new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.rotateLocal(Cards[0], new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutQuad);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            Vector3 targetPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
            Vector3 targetRotation = new Vector3(0f, 0f, rotationAngle);

            // Animate position and rotation smoothly
            LeanTween.moveLocal(Cards[i], targetPosition, 0.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.rotateLocal(Cards[i], targetRotation, 0.5f).setEase(LeanTweenType.easeOutQuad);
        }
    }


    public void UpdateTable(List<GameObject> Cards)
    {
        int cardCount = Cards.Count;
        if (cardCount == 1)
        {
            LeanTween.moveLocal(Cards[0], new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.rotateLocal(Cards[0], new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutQuad);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 3f));
            float horizontalOffset = (cardSpacing * 3 * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);

            Vector3 targetPosition = new Vector3(horizontalOffset, 0f, 0f);
            Vector3 targetRotation = new Vector3(0f, 0f, rotationAngle);

            // Animate position and rotation smoothly
            LeanTween.moveLocal(Cards[i], targetPosition, 0.25f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.rotateLocal(Cards[i], targetRotation, 0.25f).setEase(LeanTweenType.easeOutQuad);
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


