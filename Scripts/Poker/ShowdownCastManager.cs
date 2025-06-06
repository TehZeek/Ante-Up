using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class ShowdownCastManager : MonoBehaviour
{
    public List<TextMeshProUGUI> chipDisplay = new List<TextMeshProUGUI>();
    public List<GameObject> Actors = new List<GameObject>();
    public List<TextMeshProUGUI> ActorText = new List<TextMeshProUGUI>();
    public List<GameObject> chipIcon = new List<GameObject>();
    private Dictionary<int, Coroutine> spinningCoroutines = new Dictionary<int, Coroutine>();
    private Dictionary<int, Quaternion> originalRotations = new Dictionary<int, Quaternion>();
    private PokerChipManager pokerChipManager;
    private PokerTurnManager pokerTurnManager;
    private PokerHandCompare pokerHandCompare;
    private GameManager gameManager; 
    public List<RectTransform> chipsTargets = new List<RectTransform>();

    [Header("AllInCards")]
    public List<GameObject> AllInPocket0 = new List<GameObject>();
    public List<GameObject> AllInPocket1 = new List<GameObject>();
    public List<GameObject> AllInPocket2 = new List<GameObject>();
    public List<GameObject> AllInPocket3 = new List<GameObject>();
    public List<GameObject> FinalCards = new List<GameObject>();
    public List<RectTransform> AllInTable = new List<RectTransform>();



    public void buildActors()
    {
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        gameManager = FindFirstObjectByType<GameManager>();

        for (int i = 0; i < 4; i++)
        {
            Actors[i].SetActive(true);
            Actors[i].GetComponent<SpriteSpawner>().character = gameManager.characters[i];
            Actors[i].GetComponent<SpriteSpawner>().BuildActor();
            chipDisplay[i].gameObject.SetActive(true);
            chipDisplay[i].text = gameManager.playerChips[i].ToString();
        }
    }

    public void ActorFold(int player)
    {
        Actors[player].GetComponent<SpriteSpawner>().SetSprite(Actors[player].GetComponent<SpriteSpawner>().DefendSprite);
        Actors[player].GetComponent<SpriteSpawner>().originalSprite = Actors[player].GetComponent<SpriteSpawner>().DefendSprite;
        ActorTextEffect("Folded!", ActorText[player]);
    }

    public void ActorDead(int player)
    {
        Actors[player].GetComponent<SpriteSpawner>().SetSprite(Actors[player].GetComponent<SpriteSpawner>().DeadSprite);
        Actors[player].GetComponent<SpriteSpawner>().originalSprite = Actors[player].GetComponent<SpriteSpawner>().DeadSprite;
        ActorTextEffect("Out!", ActorText[player]);
    }

    public void ActorTextEffect(string newText, TextMeshProUGUI textToChange)
    {
        textToChange.text = newText;
        textToChange.gameObject.SetActive(true);
        StartCoroutine(ZoomOutEffect(textToChange));
    }

    private IEnumerator ZoomOutEffect(TextMeshProUGUI textToChange)
    {
        textToChange.transform.localScale = new Vector3(5f, 5f, 5f); // Start scale (larger than normal)
        float duration = 0.25f; // Duration of the effect
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            textToChange.transform.localScale = Vector3.Lerp(new Vector3(5f, 5f, 5f), Vector3.one, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textToChange.transform.localScale = Vector3.one; // Ensure final scale is normal
    }

    public void StartSpinning(int index)
    {
        if (index < 0 || index >= chipIcon.Count)
        {
            Debug.LogWarning("Invalid index for chipIcon.");
            return;
        }

        if (spinningCoroutines.ContainsKey(index))
        {
            Debug.Log("Chip is already spinning.");
            return;
        }

        originalRotations[index] = chipIcon[index].transform.rotation; // Store original rotation
        spinningCoroutines[index] = StartCoroutine(SpinChip(chipIcon[index]));
    }

    public void StopSpinning(int index)
    {
        if (spinningCoroutines.ContainsKey(index))
        {
            StartCoroutine(BrakeEffect(chipIcon[index], spinningCoroutines[index], originalRotations[index]));
            spinningCoroutines.Remove(index);
        }
    }

    private IEnumerator SpinChip(GameObject chip)
    {
        float speed = 0f;
        float acceleration = 100f;

        while (true)
        {
            speed += acceleration * Time.deltaTime;
            chip.transform.Rotate(Vector3.forward * speed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator BrakeEffect(GameObject chip, Coroutine spinCoroutine, Quaternion originalRotation)
    {
        StopCoroutine(spinCoroutine);
        float speed = 300f;
        float deceleration = 50f;
        Quaternion startRotation = chip.transform.rotation;

        float elapsedTime = 0f;
        float duration = speed / deceleration; // Calculate how long the braking should take

        while (elapsedTime < duration)
        {
            speed = Mathf.Lerp(speed, 0f, elapsedTime / duration); // Smooth deceleration
            chip.transform.Rotate(Vector3.forward * speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        chip.transform.rotation = originalRotation; // Reset to original rotation
    }

    public IEnumerator AttackTarget(int attacker, int defender, int ChipsLost)
    {
        Actors[attacker].GetComponent<SpriteSpawner>().StartLeap(defender);
        yield return new WaitForSeconds(0.7f);
        Actors[defender].GetComponent<SpriteSpawner>().SpawnChipDamage(ChipsLost, attacker);
    }

    public IEnumerator UpdateChipCounter(int chips, int player)
    {
        for (int i = 0; i < chips; i++)
        {
            pokerChipManager.playerChips[player]++;
            ActorTextEffect(pokerChipManager.playerChips[player].ToString(), chipDisplay[player]);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ReplacePocketCards(int player)
    {
        Debug.Log("Replacing Pocket Cards " + player);
        PokerTableCards tableCards = FindFirstObjectByType<PokerTableCards>();
        if (player == 0) ReplacePocket(AllInPocket0, tableCards.monsterPocket);
        if (player == 1) ReplacePocket(AllInPocket1, tableCards.playerOnePocket);
        if (player == 2) ReplacePocket(AllInPocket2, tableCards.playerTwoPocket);
        if (player == 3) ReplacePocket(AllInPocket3, tableCards.playerThreePocket);
    }

    private void ReplacePocket(List<GameObject> NewPocket, List<GameObject> OldPocket)
    {
        for (int i = 0; i < OldPocket.Count; i++)
        {
            var theCard = OldPocket[i].GetComponent<CardDisplay>().cardData;
            NewPocket[i].SetActive(true);
            NewPocket[i].GetComponent<CardDisplay>().cardData = theCard;
        }
    }

    public void CardDrama(int comparison, int player)
    {
        List<GameObject> playerPocket = player switch
        {
            0 => AllInPocket0,
            1 => AllInPocket1,
            2 => AllInPocket2,
            3 => AllInPocket3,
            _ => null
        };

        if (playerPocket == null) return;
        Vector3 targetScale = Vector3.one;
        if (comparison > 0) targetScale *= 1.1f;
        if (comparison < 0) targetScale *= 0.8f;

        if (player != 0)
        {
            foreach (GameObject cardObject in playerPocket)
                if (cardObject != null)
                {
                    StartCoroutine(TweenCardScale(cardObject, targetScale));
                }
        }

    }


    private IEnumerator TweenCardScale(GameObject cardObject,
                                   Vector3 targetScale,
                                   float duration = 0.25f)
    {
        if (cardObject == null) yield break;

        Vector3 startScale = cardObject.transform.localScale;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float s = t / duration;
            cardObject.transform.localScale = Vector3.Lerp(startScale, targetScale, s);
            yield return null;
        }
        cardObject.transform.localScale = targetScale;   // ensure exact
    }

    public void FlingAllInCardsOffScreen()
    {
        // Collect every pocket card and every table card into one list.
        List<GameObject> allCards = new List<GameObject>();

        allCards.AddRange(AllInPocket0);
        allCards.AddRange(AllInPocket1);
        allCards.AddRange(AllInPocket2);
        allCards.AddRange(AllInPocket3);

        foreach (RectTransform tableSlot in AllInTable)
            if (tableSlot != null)
                allCards.Add(tableSlot.gameObject);

        // Start a launch coroutine for each card.
        foreach (GameObject cardObject in allCards)
            StartCoroutine(LaunchAndDestroyCard(cardObject));

    }
    private IEnumerator LaunchAndDestroyCard(GameObject cardObject)
    {
        if (cardObject == null) yield break;

        // --- configuration ----------------------------------------------------
        float flightTime = 0.6f;           // seconds
        float scaleMultiplier = 1.6f;           // how large the card grows
        float offscreenRadius = 1600f;          // distance to fly off-screen
                                                // ----------------------------------------------------------------------

        // Cache the original transform data.
        Vector3 startScale = cardObject.transform.localScale;
        Vector3 targetScale = startScale * scaleMultiplier;
        Vector3 startPosition = cardObject.transform.position;

        // Pick a random direction to fly off the screen.
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 targetPosition = startPosition +
                                  new Vector3(randomDirection.x, randomDirection.y, 0f) *
                                  offscreenRadius;

        // Simple tween over flightTime seconds.
        for (float elapsed = 0f; elapsed < flightTime; elapsed += Time.deltaTime)
        {
            float progress = elapsed / flightTime;

            // Easing (ease-out quad) for a bit of flair.
            float eased = 1f - Mathf.Pow(1f - progress, 2f);

            cardObject.transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
            cardObject.transform.localScale = Vector3.Lerp(startScale, targetScale, eased);

            yield return null;
        }

        // Ensure final transform values are exact.
        cardObject.transform.position = targetPosition;
        cardObject.transform.localScale = targetScale;

        Destroy(cardObject);
    }

    public IEnumerator RevealCards()
    {
        for (int q = 0; q < 5; q++)
        {
            FinalCards[0].GetComponent<CardShowdown>().showdownCards[q].GetComponent<CardDisplay>().CardBack.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1f);
        FinalCards[0].GetComponent<CardShowdown>().handText.GetComponent<TextMeshProUGUI>().text = pokerHandCompare.HandToString(0);
        yield return new WaitForSeconds(1f);
    }


    public void buildHands(int player)
    {
        if (player != 0) FinalCards[player].GetComponent<CardShowdown>().handText.text = pokerHandCompare.HandToString(player);
        else FinalCards[0].GetComponent<CardShowdown>().handText.text = "?????";

        for (int j = 0; j < 5; j++)
        {
            List<Card> currentHand = GetHandForPlayer(player);
            if (j < currentHand.Count)
            {
                FinalCards[player].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().cardData = currentHand[j];
                FinalCards[player].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().UpdateCardDisplay();
                if (player == 0)
                {
                    FinalCards[player].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().CardBack.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning($"[buildHands] Player {player}'s hand has fewer than {j + 1} cards.");
            }
        }
        FinalCards[player].GetComponent<CardShowdown>().handText.GetComponent<TextMeshProUGUI>().text = pokerHandCompare.HandToString(player);
        FinalCards[player].GetComponent<CardShowdown>().EnterScreen();
    }

    private List<Card> GetHandForPlayer(int player)
    {
        return player switch
        {
            0 => pokerHandCompare.monHand,
            1 => pokerHandCompare.p1Hand,
            2 => pokerHandCompare.p2Hand,
            3 => pokerHandCompare.p3Hand,
            _ => new List<Card>()
        };
    }

}
