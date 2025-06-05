using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;


public class ActionScreen : MonoBehaviour
{
    public float fadeDuration = 3f; // Duration of fade effect
    public Image image;
    public List<GameObject> Actors = new List<GameObject>();
    public List<Vector2> SpawnPosition = new List<Vector2>();
    public TextMeshProUGUI showdownText;
    public List<TextMeshProUGUI> chipDisplay = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> folded = new List<TextMeshProUGUI>();
    public List<GameObject> chipIcon = new List<GameObject>();
    private Dictionary<int, Coroutine> spinningCoroutines = new Dictionary<int, Coroutine>();
    private Dictionary<int, Quaternion> originalRotations = new Dictionary<int, Quaternion>();
    public List<GameObject> FinalCards = new List<GameObject>();
    public TextMeshProUGUI miniHand;
    private PokerHandCompare pokerHandCompare;
    private GameManager gameManager;
    private BattleManager battleManager;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;
    private PokerTableCards pokerTableCards;
    public List<RectTransform> chipsTargets = new List<RectTransform>();
    private List<bool> IsDead = new List<bool>() { false, false, false, false };
    private int totalSum;
    public List<GameObject> AllInPocket0 = new List<GameObject>();
    public List<GameObject> AllInPocket1 = new List<GameObject>();
    public List<GameObject> AllInPocket2 = new List<GameObject>();
    public List<GameObject> AllInPocket3 = new List<GameObject>();
    public List<GameObject> AllInTable = new List<GameObject>();


    public void GetManagers()
    {
        pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        gameManager = FindFirstObjectByType<GameManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
    }

    public void UpdateChipCounter(int player)
    {
        pokerChipManager.playerChips[player]++;
        TextEffect(pokerChipManager.playerChips[player].ToString(), chipDisplay[player]);
    }

    public void TextEffect(string newText, TextMeshProUGUI textToChange)
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

    private void buildActors()
    {
        if (gameManager == null) { Debug.Log("no game manager"); return; }
        if (Actors == null || Actors.Count < 4)
        {
            Debug.LogError("Actors list is null or does not have enough elements.");
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("Building " + i);
            Actors[i].SetActive(true);
            Actors[i].GetComponent<SpriteSpawner>().character = gameManager.characters[i];
            Actors[i].GetComponent<SpriteSpawner>().BuildActor();
            chipDisplay[i].gameObject.SetActive(true);
            chipDisplay[i].text = gameManager.playerChips[i].ToString();
        }
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

    public void PlayerFold(int player)
    {
        if (!IsDead[player])
        {
            Actors[player].GetComponent<SpriteSpawner>().SetSprite(Actors[player].GetComponent<SpriteSpawner>().DefendSprite);
            Actors[player].GetComponent<SpriteSpawner>().originalSprite = Actors[player].GetComponent<SpriteSpawner>().DefendSprite;
            TextEffect("Folded!", folded[player]);
        }
        else
        {
            Actors[player].GetComponent<SpriteSpawner>().SetSprite(Actors[player].GetComponent<SpriteSpawner>().DeadSprite);
            Actors[player].GetComponent<SpriteSpawner>().originalSprite = Actors[player].GetComponent<SpriteSpawner>().DeadSprite;
            TextEffect("Out!", folded[player]);
        }
    }


    public void PlayersFoldShowdown(List<int> ChipsLost)
    {
        for (int i = 1; i < 4; i++)
        { 
            PlayerFold(i);
        }
        StartSpinning(0);
        StartCoroutine(PlayersFoldContinued(ChipsLost));
    }
    private IEnumerator PlayersFoldContinued(List<int> ChipsLost)
    {
        yield return new WaitForSeconds(0.5f);
        TextEffect("Party Folds!", showdownText);
        yield return new WaitForSeconds(0.5f);
        for (int j = 1; j < 4; j++)
        {
            if (ChipsLost[j] > 0 && !IsDead[j])
            {
                Actors[0].GetComponent<SpriteSpawner>().StartLeap(j);
                yield return new WaitForSeconds(0.7f);
                List<int> target = new List<int>() { 0 };
                Actors[j].GetComponent<SpriteSpawner>().SpawnChipDamage(ChipsLost[j], target);
                yield return new WaitForSeconds(0.7f);
            }
            if (ChipsLost[0] > 0) { 
                for (int m = 0; m < ChipsLost[0]; m++)
                {
                    UpdateChipCounter(0);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        yield return new WaitForSeconds(2f);
        StopSpinning(0);
        WrapUpShowdown();
    }

    public void MonstersFoldShowdown(List<int> playersRemaining, List<int> ChipsLost)
    {
        for (int i = 0; i < 4; i++)
        {
            if (!playersRemaining.Contains(i))
            {
                PlayerFold(i);
            }
        }
        for (int j = 0; j < playersRemaining.Count; j++)
        {
            StartSpinning(playersRemaining[j]);
        }
        StartCoroutine(MonstersFoldContinued(playersRemaining, ChipsLost));
    }

    private IEnumerator MonstersFoldContinued(List<int> playersRemaining, List<int> ChipsLost)
    {
        yield return new WaitForSeconds(0.5f);
        TextEffect("Monster Folds!", showdownText);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < playersRemaining.Count; i++)
            {
                Actors[playersRemaining[i]].GetComponent<SpriteSpawner>().StartLeap(0);
            }
        yield return new WaitForSeconds(0.7f);
        for (int l = 0; l < ChipsLost.Count; l++)
        {
            totalSum+= ChipsLost[l];
        }

        Actors[0].GetComponent<SpriteSpawner>().SpawnChipDamage(totalSum, playersRemaining);
        totalSum = 0;
        yield return new WaitForSeconds(2.0f);
        for (int j = 0; j < playersRemaining.Count; j++)
        {
            StopSpinning(playersRemaining[j]);
        }
        WrapUpShowdown();
    }

    public void AllInShowdown(List<int> ChipsLost)
    {
        int cardRound = 3;
        if (pokerTurnManager.HasARiver) cardRound++;
        if (pokerTurnManager.HasABonusRound) cardRound++;
        if (pokerTurnManager.turnOrder[1] >= cardRound)
        {
            RegularShowdown(ChipsLost);
            return;
        }
        int cardsLeftToPlay = (cardRound - pokerTurnManager.turnOrder[1] - 1);
        string showHand = "Minimum Hand:\n";
        string minHand = pokerHandCompare.HandToString(4);
        TextEffect((showHand + minHand), miniHand);
        TextEffect("ALL IN\nShowdown!", showdownText);
        StartCoroutine(AllInThrowDown(ChipsLost));
    }

    private IEnumerator AllInThrowDown(List<int> chipsLost)
    {
        const float revealDelay = 1.5f;

        if (pokerTableCards == null) GetManagers();   // safety net
        yield return new WaitForSeconds(revealDelay);

        // Parallel lists:   seat-index → { all-in pocket, table pocket }
        var allInPockets = new List<List<GameObject>> { AllInPocket0, AllInPocket1, AllInPocket2, AllInPocket3 };
        var tablePockets = new List<List<GameObject>>
    {
        pokerTableCards.monsterPocket,
        pokerTableCards.playerOnePocket,
        pokerTableCards.playerTwoPocket,
        pokerTableCards.playerThreePocket
    };

        for (int seat = 0; seat < 4; seat++)
        {
            bool isFolded = pokerTurnManager.IsOut[seat] && !pokerTurnManager.isAllIn[seat];

            if (isFolded)
                PlayerFold(seat);
            else
                ReplacePocket(allInPockets[seat], tablePockets[seat]);

            yield return new WaitForSeconds(revealDelay);
        }
    }

    private static void ReplacePocket(
        List<GameObject> allInPocketCards,
        List<GameObject> destinationPocketSlots)
    {
        for (int slotIndex = 0; slotIndex < destinationPocketSlots.Count; slotIndex++)
        {
            bool replacementCardExists =
                slotIndex < allInPocketCards.Count && allInPocketCards[slotIndex] != null;

            if (replacementCardExists)
            {
                CardDisplay replacementCardDisplay =
                    allInPocketCards[slotIndex].GetComponent<CardDisplay>();
                CardDisplay destinationCardDisplay =
                    destinationPocketSlots[slotIndex].GetComponent<CardDisplay>();

                if (replacementCardDisplay != null && destinationCardDisplay != null)
                {
                    destinationCardDisplay.cardData = replacementCardDisplay.cardData;
                    destinationCardDisplay.UpdateCardDisplay();
                }

                destinationPocketSlots[slotIndex].SetActive(true);
                destinationPocketSlots[slotIndex].GetComponent<CardDisplay>().UpdateCardDisplay();
            }
            else
            {
                if (destinationPocketSlots[slotIndex] != null)
                {
                    destinationPocketSlots[slotIndex].SetActive(false);
                }
            }
        }
    }


    public void RegularShowdown(List<int> ChipsLost)
    {
        //setup
        TextEffect("Showdown!", showdownText);
        StartCoroutine(RegularShowdownContinued(ChipsLost));
    }
    public IEnumerator RegularShowdownContinued(List<int> ChipsLost)
    {
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < 4; i++)
            {
                if (pokerTurnManager.IsOut[i] && !pokerTurnManager.isAllIn[i])
                {
                    PlayerFold(i);
                }
                else 
                {
                    buildHands(i);
                }
                yield return new WaitForSeconds(1.5f);
            }
        string showHand = "Minimum Hand:\n";
        string minHand = pokerHandCompare.HandToString(4);

        TextEffect((showHand + minHand), miniHand);

        yield return new WaitForSeconds(1.5f);
        HandTypes minimHand = gameManager.monster.minimumHand;
        int minimumHandRank = pokerHandCompare.allHandTypes.IndexOf(minimHand);
        int winners = pokerTurnManager.EvaluatePlayers(minimumHandRank);
        if (winners>0)
        {
            for (int w = 1; w < 4; w++)
            {
                if (!pokerTurnManager.playersStillIn.Contains(w) && !pokerTurnManager.IsOut[w])
                {
                    //if players are still in, but some are out due to the minimum hand rule
                    FinalCards[w].GetComponent<CardShowdown>().ThrowCardsOffscreen();
                    string defeatString = "Defeated";
                    TextEffect(defeatString, folded[w]);
                    yield return new WaitForSeconds(0.5f);
                    Actors[0].GetComponent<SpriteSpawner>().StartLeap(w);
                    yield return new WaitForSeconds(0.7f);
                    List<int> target = new List<int>() { 0 };
                    Actors[w].GetComponent<SpriteSpawner>().SpawnChipDamage(0, target);
                    yield return new WaitForSeconds(0.7f);
                }
            }
            pokerTurnManager.playersStillIn.Clear();
            pokerTurnManager.playShowdown();

            for (int q = 0; q < 5; q++)
            {
                FinalCards[0].GetComponent<CardShowdown>().showdownCards[q].GetComponent<CardDisplay>().CardBack.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(1f);
            FinalCards[0].GetComponent<CardShowdown>().handText.GetComponent<TextMeshProUGUI>().text = pokerHandCompare.HandToString(0);
            yield return new WaitForSeconds(1f);

            if (pokerTurnManager.playersStillIn.Contains(0))
            {
                StartCoroutine(MonstersWinShowdown(ChipsLost));
            }
            else
            {
                TextEffect("Players Win!", showdownText);
                yield return new WaitForSeconds(0.5f);

                for (int r = 0; r < pokerTurnManager.playersStillIn.Count; r++)
                {
                    StartSpinning(r);
                    yield return new WaitForSeconds(0.5f);
                }
                for (int i = 0; i < pokerTurnManager.playersStillIn.Count; i++)
                {
                    Actors[pokerTurnManager.playersStillIn[i]].GetComponent<SpriteSpawner>().StartLeap(0);
                }
                yield return new WaitForSeconds(0.7f);
                for (int l = 0; l < ChipsLost.Count; l++)
                {
                    totalSum += ChipsLost[l];
                }

                Actors[0].GetComponent<SpriteSpawner>().SpawnChipDamage(totalSum, pokerTurnManager.playersStillIn);
                totalSum = 0;
                yield return new WaitForSeconds(2.0f);
                for (int j = 0; j < pokerTurnManager.playersStillIn.Count; j++)
                {
                    StopSpinning(pokerTurnManager.playersStillIn[j]);
                }
            }
        }
        else
        {
            StartCoroutine(MonstersWinShowdown(ChipsLost));
        }
        WrapUpShowdown();
    }

    private IEnumerator MonstersWinShowdown(List<int> ChipsLost)
    {
        StartSpinning(0);
        yield return new WaitForSeconds(0.5f);
        TextEffect("Monsters Win!", showdownText);
        for (int g = 0; g < 4; g++)
        {
            FinalCards[g].GetComponent<CardShowdown>().ThrowCardsOffscreen();
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.5f);
        for (int j = 1; j < 4; j++)
        {
            if (ChipsLost[j] > 0 && !IsDead[j])
            {
                Actors[0].GetComponent<SpriteSpawner>().StartLeap(j);
                yield return new WaitForSeconds(0.7f);
                List<int> target = new List<int>() { 0 };
                Actors[j].GetComponent<SpriteSpawner>().SpawnChipDamage(ChipsLost[j], target);
                yield return new WaitForSeconds(0.7f);
            }
            if (ChipsLost[0] > 0)
            {
                for (int m = 0; m < ChipsLost[0]; m++)
                {
                    UpdateChipCounter(0);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        yield return new WaitForSeconds(2f);
        StopSpinning(0);
        WrapUpShowdown();
    }


    public void ShowdownSetup()
    {
        buildActors();
        if (pokerHandCompare == null || gameManager == null || battleManager == null || pokerTurnManager == null || pokerChipManager == null)
        {
            Debug.Log("Can't load a showdown due to managers");
            return;
        }
        List<int> chipsLost = new List<int>();
        for (int i = 0; i<4; i++)
        {
            chipsLost.Add(pokerChipManager.InThePot[0]);
            if (pokerChipManager.playerChips[i] == 0 && pokerTurnManager.IsOut[i] && !pokerTurnManager.isAllIn[i]) { IsDead[i] = true; }
        }
        if (pokerTurnManager.DidMonstersFold())
        {
            MonstersFoldShowdown(pokerTurnManager.playersStillIn, chipsLost);
            return;
        }
        if (pokerTurnManager.DidPlayersFold())
        {
            PlayersFoldShowdown(chipsLost);
            return;
        }
        if (pokerTurnManager.isAllIn[0] || ((pokerTurnManager.isAllIn[1] || pokerTurnManager.isAllIn[2] || pokerTurnManager.isAllIn[3]) && (pokerTurnManager.IsOut[1] && pokerTurnManager.IsOut[2] && pokerTurnManager.IsOut[3])))
        {
            AllInShowdown(chipsLost);
            return;
        }
        RegularShowdown(chipsLost);
    }

    public void WrapUpShowdown()
    {
        TextEffect("showdown over", showdownText);
        //  pokerTurnManager.ClearTurnVariables();
      //  pokerTableCards.ClearTable();
        //players fall down if dead
        //game over check
        //round over check
        //set back up
    }
}