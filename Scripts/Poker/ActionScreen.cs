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
    public TextMeshProUGUI minimumHand;
    private PokerHandCompare pokerHandCompare;
    private GameManager gameManager;
    private BattleManager battleManager;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;
    public List<RectTransform> chipsTargets = new List<RectTransform>();
    private List<bool> IsDead = new List<bool>() { false, false, false, false };

    void Start()
    {
        pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        gameManager = FindFirstObjectByType<GameManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
    }

    public void FadeSplash()
    {
        Debug.Log("FadeSplash() called");
            StartCoroutine(FadeOutImage());
    }

    private IEnumerator FadeOutImage()
    {
        Debug.Log("FadeOutImage() called");
        Color color = image.color;
        yield return new WaitForSeconds(2f);
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            image.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Destroy after fade out
    }

    private IEnumerator FadeOutCanvasGroup()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        Debug.Log("FadeOutCanvasGroup() called");
        yield return new WaitForSeconds(2f);
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
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
        float duration = 0.35f; // Duration of the effect
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
            if (player == 0)
                {
                    FinalCards[j].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().cardData = pokerHandCompare.monHand[j];
                    FinalCards[j].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().CardBack.gameObject.SetActive(true);
                }
            if (player == 1)
                {
                    FinalCards[j].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().cardData = pokerHandCompare.p1Hand[j];
                }
            if (player == 2)
                {
                    FinalCards[j].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().cardData = pokerHandCompare.p2Hand[j];
                }
            if (player == 3)
                {
                    FinalCards[j].GetComponent<CardShowdown>().showdownCards[j].GetComponent<CardDisplay>().cardData = pokerHandCompare.p3Hand[j];
                }
        }
    }

    public void PlayerFold(int player)
    {
        if (!IsDead[player])
        {
            Actors[player].GetComponent<SpriteSpawner>().SetSprite(Actors[player].GetComponent<SpriteSpawner>().HurtSprite);
            Actors[player].GetComponent<SpriteSpawner>().originalSprite = Actors[player].GetComponent<SpriteSpawner>().HurtSprite;
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
        TextEffect("Party Folds!", minimumHand);
        yield return new WaitForSeconds(0.5f);
        for (int j = 1; j < 4; j++)
            if (ChipsLost[j] > 0 && !IsDead[j])
            {
                Actors[0].GetComponent<SpriteSpawner>().StartLeap(j);
                yield return new WaitForSeconds(0.7f);
                List<int> target = new List<int>() { 0 };
                Actors[j].GetComponent<SpriteSpawner>().SpawnChipDamage(ChipsLost[j], target);
                yield return new WaitForSeconds(0.7f);
            }
        yield return new WaitForSeconds(2f);
        StopSpinning(0);
        WrapUpShowdown();
    }

    public void MonstersFoldShowdown(List<int> playersRemaining, List<int> ChipsLost)
    {

    }

    public void AllInShowdown()
    {

    }

    public void RegularShowdown()
    {

    }

    public void trialShowdownSetup()
    {
        List<int> chipsLost = new List<int>() {0,8,8,0};
        IsDead[3] = true;
        PlayersFoldShowdown(chipsLost);
        //something to test the other functions in the scene
    }

    public void ShowdownSetup()
    {
        buildActors();
        if (pokerHandCompare == null || gameManager == null || battleManager == null || pokerTurnManager == null || pokerChipManager == null)
        {
            trialShowdownSetup();
            return;
        }
        List<int> chipsLost = new List<int>();
        for (int i = 0; i<4; i++)
        {
            chipsLost.Add(pokerChipManager.InThePot[0]);
            if (pokerChipManager.playerChips[i] == 0 && pokerTurnManager.IsOut[i]) { IsDead[i] = true; }
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
        for (int i = 0; i < 4; i++)
            {
                if (!pokerTurnManager.IsOut[i] || pokerTurnManager.isAllIn[i])
                {
                    buildHands(i);
                }
                else
                {
                    PlayerFold(i);
                }
            }



        //pokerTurnManager  is out (fold), is all in
        // gameManager is dead with 0 chips and not all in
        //pokerHandCompare  pokerHandCompare.allHandTypes, .p1Hand
        //HUD[0].MinHand for minimum hand text
        //need to write a function in hand compare to make a string of the hand type and send it over
        //pokerTurnManager grab the winner(s)

        // show who folded, turn on folded text
        //monster attacks
        //bring in player 1 - delay - 2 - delay - 3 - delay
        //turn on minimum hand.  Explode player hands that did not beat it
        //monster attacks
        //show monster hand
        // if players lose, explose player hands, monster attacks.
        // if monster, explode monster hand, players attack
        //chips rain from those attacked, get sucked into chip counter of winner
        //player won X chips text

        // game over check to come

        // +1 chip for each additional player to win

        // if game over, dead sprite.  Game over text
        // if not, reset to next round.
    }
    public void WrapUpShowdown()
    {

    }
}