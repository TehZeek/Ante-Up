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
    public List<RectTransform> allInTransform = new List<RectTransform>();
    public float cardSpacing = 150f;
    public float verticalSpacing = 25f;
    public float fanSpread = -5f;
    private PokerDrawPile pokerDrawPile;
    public List<GameObject> playerOnePocket = new List<GameObject>();
    public List<GameObject> playerTwoPocket = new List<GameObject>();
    public List<GameObject> playerThreePocket = new List<GameObject>();
    public List<GameObject> monsterPocket = new List<GameObject>();
    public List<GameObject> tableHand = new List<GameObject>();
    public List<GameObject> burnDeck = new List<GameObject>();
    private BattleManager battleManager;
    public  Dictionary<int, (List<GameObject> pocket, Transform transform)> playerMap;
    public bool showdownLayoutActive = false;
    [SerializeField] private float spawnZDepth = 10f;
    // import this from PokerDrawPile public List<GameObject> cardsInHand = new List<GameObject>(); // hold the list of card objects in our hand

    void Start()
    {
        pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        battleManager = FindFirstObjectByType<BattleManager>();
    }

    public void remapHands()
    {
        playerMap = new Dictionary<int, (List<GameObject>, Transform)>
    {
        { 0, (monsterPocket, monTransform) },
        { 1, (playerOnePocket, p1Transform) },
        { 2, (playerTwoPocket, p2Transform) },
        { 3, (playerThreePocket, p3Transform) }
    };
    }

    public void Fold(int player)
    {
        Debug.Log("HAS FOLDED");
        if (playerMap == null) remapHands();
        if (!playerMap.ContainsKey(player)) return;
        var (pocket, transform) = playerMap[player];
        burnDeck.AddRange(pocket);
        pocket.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        UpdateTableVisuals();
    }

    public void AddCardToPosition(Card cardData, int whichPlayer)
    {
        Transform targetTransform = GetTargetTransform(whichPlayer);
        if (whichPlayer >= 20) showdownLayoutActive = true;
        else showdownLayoutActive = false;

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
        Vector3 pos = Random.Range(0, 4) switch
        {
            0 => new Vector3(Random.Range(0, Screen.width), 0, spawnZDepth),
            1 => new Vector3(Random.Range(0, Screen.width), Screen.height, spawnZDepth),
            2 => new Vector3(0, Random.Range(0, Screen.height), spawnZDepth),
            3 => new Vector3(Screen.width, Random.Range(0, Screen.height), spawnZDepth),
            _ => Vector3.zero
        };
        return Camera.main.ScreenToWorldPoint(pos);
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
            20 => allInTransform[0],
            21 => allInTransform[1],
            22 => allInTransform[2],
            23 => allInTransform[3],
            24 => allInTransform[4],
            25 => allInTransform[5],
            _ => null
        };
    }

    private void AddCardToList(int whichPlayer, GameObject newCard)
    {
        if (whichPlayer == 0) { monsterPocket.Add(newCard); } 
        else if (whichPlayer == 1) { playerOnePocket.Add(newCard); }
        else if (whichPlayer == 2) { playerTwoPocket.Add(newCard); }
        else if (whichPlayer == 3) { playerThreePocket.Add(newCard); }
        else if ((whichPlayer > 3 && whichPlayer < 9) || whichPlayer == 10 || whichPlayer >= 20) { tableHand.Add(newCard); }
        else if (whichPlayer == 9) { burnDeck.Add(newCard); }
    }

    public void ClearTable()
    {
        foreach (var entry in playerMap.Values)
        {
            ClearTransformChildren(entry.transform);
        }

        ClearTransformChildren(tableTransform);
        ClearTransformChildren(burnTransform);

        battleManager.UpdateHUD();
    }

    private void ClearTransformChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void UpdateTableVisuals()
    {
        battleManager.UpdateHUD();
        UpdateCardLayout(monsterPocket, cardSpacing, 3f, 2f);
        UpdateCardLayout(playerOnePocket, cardSpacing, 3f, 2f);
        UpdateCardLayout(playerTwoPocket, cardSpacing, 3f, 2f);
        UpdateCardLayout(playerThreePocket, cardSpacing, 3f, 2f);
        if (!showdownLayoutActive) UpdateCardLayout(tableHand, cardSpacing * 3, 3f, 0.25f);
        else UpdateCardLayout(tableHand, cardSpacing/2, 2f, 0.25f);
    }

    private void UpdateCardLayout(List<GameObject> cards, float spacing, float fanMultiplier, float duration = 0.5f)
    {
        int count = cards.Count;
        if (count == 1)
        {
            LeanTween.moveLocal(cards[0], Vector3.zero, duration).setEase(LeanTweenType.easeOutQuad);
            LeanTween.rotateLocal(cards[0], Vector3.zero, duration).setEase(LeanTweenType.easeOutQuad);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = fanSpread * (i - (count - 1) / fanMultiplier);
            float x = spacing * (i - (count - 1) / 2f);
            float y = verticalSpacing * (1 - Mathf.Pow(2f * i / (count - 1) - 1f, 2));

            Vector3 pos = new Vector3(x, y, 0);
            Vector3 rot = new Vector3(0, 0, angle);

            LeanTween.moveLocal(cards[i], pos, duration).setEase(LeanTweenType.easeOutQuad);
            LeanTween.rotateLocal(cards[i], rot, duration).setEase(LeanTweenType.easeOutQuad);
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
        pokerHandCompare.UpdateHandType(monsterPocket, tableHand, 0);
        pokerHandCompare.UpdateHandType(playerOnePocket, tableHand, 1);
        pokerHandCompare.UpdateHandType(playerTwoPocket, tableHand, 2);
        pokerHandCompare.UpdateHandType(playerThreePocket, tableHand, 3);
    }



}


