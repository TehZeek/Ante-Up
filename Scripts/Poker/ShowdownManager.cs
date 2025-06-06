using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.SceneManagement;


public class ShowdownManager : MonoBehaviour
{
    [Header("Managers")]
    private PokerHandCompare pokerHandCompare;
    private GameManager gameManager;
    private BattleManager battleManager;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;
    private PokerTableCards pokerTableCards;
    public ShowdownCastManager CastManager;

    [Header("ChipManagement")]
    private int[] chipsLost;
    private int potSize;

    [Header("AllInCards")]
    private List<GameObject> table = new List<GameObject>();


    [Header("Display Texts")]
    public TextMeshProUGUI miniHand;
    public TextMeshProUGUI showdownText;
    public List<TextMeshProUGUI> AllInResults = new List<TextMeshProUGUI>();


    public void GetManagers()
    {
        pokerHandCompare = FindFirstObjectByType<PokerHandCompare>();
        gameManager = FindFirstObjectByType<GameManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        pokerTableCards = FindFirstObjectByType<PokerTableCards>();
    }

    public void ShowdownSetup()
    {
        CastManager.buildActors();
        chipsLost = pokerChipManager.InThePot;
        potSize = pokerChipManager.potChips;
        if (pokerHandCompare == null || gameManager == null || battleManager == null || pokerTurnManager == null || pokerChipManager == null)
            GetManagers();

        //check for the 4 showdown states
        if (pokerTurnManager.DidMonstersFold())
        {
            MonstersFoldShowdown();
            return;
        }
        else if (pokerTurnManager.DidPlayersFold())
        {
            PlayersFoldShowdown();
            return;
        }
        else if (pokerTurnManager.IsItAllInShowdown())
        {
            AllInShowdown();
            return;
        }

        else RegularShowdown();
    }

    //------------------------
    //Fold
    //------------------------

    private void MonstersFoldShowdown()
    {
        for (int i = 0; i < pokerTurnManager.playersStillIn.Count; i++)
        {
            CastManager.StartSpinning(pokerTurnManager.playersStillIn[i]);
        }
        FoldCheck();
        StartCoroutine(MonstersFoldContinued(pokerTurnManager.playersStillIn));
    }

    private IEnumerator MonstersFoldContinued(List<int> playersRemaining)
    {
        yield return new WaitForSeconds(0.5f);
        TextEffect("Monster Folds!", showdownText);
        StartCoroutine(PlayersWin());
    }

    private void PlayersFoldShowdown()
    {
        CastManager.StartSpinning(0);
        FoldCheck();
        StartCoroutine(PlayersFoldContinued());
    }

    private IEnumerator PlayersFoldContinued()
    {
        yield return new WaitForSeconds(0.5f);
        TextEffect("Party Folds!", showdownText);
        StartCoroutine (MonstersWin());
    }

    //------------------------
    //All In
    //------------------------

    private void AllInShowdown()
    {
        FoldCheck();
        table.Clear();
        string showHand = "Minimum Hand:\n";
        string minHand = pokerHandCompare.HandToString(4);
        TextEffect((showHand + minHand), miniHand);
        TextEffect("ALL IN\nShowdown!", showdownText);
        StartCoroutine(AllInThrowDown());
    }

    private IEnumerator AllInThrowDown()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < pokerTurnManager.playersStillIn.Count; i++)
        {
            CastManager.ReplacePocketCards(i);
            AllInCardDrama(i);
        }
        yield return new WaitForSeconds(1f);
        showdownText.gameObject.SetActive(false);
        int cardsToDraw = CardsToDraw();
        for (int i = 0; i < cardsToDraw; i++)
        {
            PlaceTableCardDramatically(i);
            yield return new WaitForSeconds(0.75f);
            for (int j = 0; j < 4; j++)
            {
                if (!pokerTurnManager.IsOut[j] || pokerTurnManager.isAllIn[j])
                {
                    AllInCardDrama(j);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        pokerTurnManager.playShowdown();
        if (pokerTurnManager.playersStillIn.Contains(0))
        {
            StartCoroutine(MonstersWinAllIn());
        }
        else
        {
            StartCoroutine(PlayersWinAllIn());
        }
    }

    private void AllInCardDrama(int player)
    {
        switch (player)
        {
            case 0: 
                pokerHandCompare.UpdateHandType(pokerTableCards.monsterPocket, table, 0); break;
            case 1:
                pokerHandCompare.UpdateHandType(pokerTableCards.playerOnePocket, table, 1); break;
            case 2:
                pokerHandCompare.UpdateHandType(pokerTableCards.playerTwoPocket, table, 2); break;
            case 3:
                pokerHandCompare.UpdateHandType(pokerTableCards.playerThreePocket, table, 3); break;
        }
        string hand = pokerHandCompare.HandToString(player);
        TextEffect(hand, AllInResults[player]);
        //make this bigger or smaller if ahead of or behind monster

        
        int comparison = 0;
        if (player == 0 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand))
        {
            miniHand.gameObject.SetActive(false);
        }
        if (player == 1 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P1Hand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P1Hand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand)) comparison++;
        if (player == 2 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P2Hand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P2Hand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand)) comparison++;
        if (player == 3 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P3Hand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P3Hand) > pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand)) comparison++;
        if (player == 1 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P1Hand) < pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) || pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P1Hand) < pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand)) comparison--;
        if (player == 2 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P2Hand) < pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) || pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P2Hand) < pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand)) comparison--;
        if (player == 3 && pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P3Hand) < pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MonHand) || pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.P3Hand) < pokerHandCompare.allHandTypes.IndexOf(pokerHandCompare.MinHand)) comparison--;

        CastManager.CardDrama(comparison, player);
    }

    private void PlaceTableCardDramatically(int whichCard)
    {
        PokerDrawPile pokerDrawPile = FindFirstObjectByType<PokerDrawPile>();
        if ((pokerTableCards.tableHand.Count == 0 && whichCard == 0) || whichCard >= pokerTableCards.tableHand.Count)
        {
            pokerDrawPile.DealCard(20 + whichCard);
            GameObject nextCard;
            nextCard = pokerTableCards.tableHand[whichCard];
            table.Add(nextCard);
        }
        else
        {
            var cardToDup = pokerTableCards.tableHand[whichCard].GetComponent<CardDisplay>().cardData;
            int noDup = pokerTableCards.tableHand.Count;
            pokerTableCards.AddCardToPosition(cardToDup, 20 + whichCard);
            table.Add(pokerTableCards.tableHand[whichCard]);
            pokerTableCards.tableHand[whichCard] = pokerTableCards.tableHand[noDup];
            pokerTableCards.tableHand.RemoveAt(noDup);
        }
    }

    private IEnumerator MonstersWinAllIn()
    {
        CastManager.StartSpinning(0);
        yield return new WaitForSeconds(0.5f);
        TextEffect("Monsters Win!", showdownText);

        yield return new WaitForSeconds(0.5f);
        CastManager.FlingAllInCardsOffScreen();
        StartCoroutine(MonstersWin());
    }

    private IEnumerator PlayersWinAllIn()
    {
        yield return new WaitForSeconds(0.5f);
        TextEffect("Players Win!", showdownText);
        yield return new WaitForSeconds(0.5f);
        CastManager.FlingAllInCardsOffScreen();
        StartCoroutine(PlayersWin());
    }

    //-----------------------------
    //Regular Showdown
    //-----------------------------
    private void RegularShowdown()
    {
        FoldCheck();
        TextEffect("Showdown!", showdownText);
        StartCoroutine(RegularShowdownContinued());
    }
    public IEnumerator RegularShowdownContinued()
    {
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < pokerTurnManager.playersStillIn.Count; i++)
        {
            CastManager.buildHands(i);
            yield return new WaitForSeconds(1.5f);
        }
        string showHand = "Minimum Hand:\n";
        string minHand = pokerHandCompare.HandToString(4);
        TextEffect((showHand + minHand), miniHand);
        yield return new WaitForSeconds(1f);
        TextEffect("Minimum Hand Check!", showdownText);
        yield return new WaitForSeconds(1f);
        HandTypes minimHand = gameManager.monster.minimumHand;
        int minimumHandRank = pokerHandCompare.allHandTypes.IndexOf(minimHand);
        int winners = pokerTurnManager.EvaluatePlayers(minimumHandRank);
        
        if (winners > 0)
        {
            for (int w = 1; w < 4; w++)
            {
                if (!pokerTurnManager.playersStillIn.Contains(w) && !pokerTurnManager.IsOut[w])
                {
                    //if players are still in, but some are out due to the minimum hand rule
                    CastManager.FinalCards[w].GetComponent<CardShowdown>().ThrowCardsOffscreen();
                    string defeatString = "Defeated";
                    CastManager.ActorTextEffect(defeatString, CastManager.ActorText[w]);
                    yield return new WaitForSeconds(0.5f);
                    StartCoroutine(CastManager.AttackTarget(0, w, chipsLost[w]));
                    yield return new WaitForSeconds(1.4f);
                }
            }
            TextEffect("Enemy Reveal!", showdownText);

            pokerTurnManager.playersStillIn.Clear();
            pokerTurnManager.playShowdown();
            StartCoroutine(CastManager.RevealCards());
            yield return new WaitForSeconds(2);

            if (pokerTurnManager.playersStillIn.Contains(0))
            {
                StartCoroutine(MonstersWinShowdown());
            }
            else
            {
                StartCoroutine(PlayersWinShowdown());
            }
        }
        else
        {
            StartCoroutine(MonstersWinShowdown());
        }
    }


private IEnumerator MonstersWinShowdown()
{
    CastManager.StartSpinning(0);
    yield return new WaitForSeconds(0.5f);
    TextEffect("Monsters Win!", showdownText);
    for (int g = 0; g < 4; g++)
    {
        CastManager.FinalCards[g].GetComponent<CardShowdown>().ThrowCardsOffscreen();
        yield return new WaitForSeconds(0.25f);
    }
    yield return new WaitForSeconds(0.5f);
        StartCoroutine (MonstersWin());
    }

    public IEnumerator PlayersWinShowdown()
{
    TextEffect("Players Win!", showdownText);
    yield return new WaitForSeconds(0.5f);
    for (int g = 0; g < 4; g++)
    {
        CastManager.FinalCards[g].GetComponent<CardShowdown>().ThrowCardsOffscreen();
        yield return new WaitForSeconds(0.25f);
    }

    for (int r = 0; r < pokerTurnManager.playersStillIn.Count; r++)
    {
        CastManager.StartSpinning(r);
        yield return new WaitForSeconds(0.5f);
    }
    PlayersWin();
}


//------------------------------
//Support Functions
//------------------------------

private IEnumerator PlayersWin()
    {
        int[] tempChips = SplitThePot(pokerTurnManager.playersStillIn);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < pokerTurnManager.playersStillIn.Count; i++)
        {
            StartCoroutine(CastManager.AttackTarget(pokerTurnManager.playersStillIn[i], 0, tempChips[i]));
        yield return new WaitForSeconds(0.7f);
        }
        yield return new WaitForSeconds(0.7f);
        for (int i = 0; i < pokerTurnManager.playersStillIn.Count; i++)
        {
            StartCoroutine(CastManager.UpdateChipCounter(tempChips[i], pokerTurnManager.playersStillIn[i]));
        }
        yield return new WaitForSeconds(0.1f * potSize);

        for (int j = 0; j < pokerTurnManager.playersStillIn.Count; j++)
        {
            CastManager.StopSpinning(pokerTurnManager.playersStillIn[j]);
        }
        WrapUpShowdown();
    }

    private IEnumerator MonstersWin()
    {
        yield return new WaitForSeconds(0.5f);

        for (int target = 1; target < 4; target++)
        {
            if (chipsLost[target] > 0)
            {
                StartCoroutine(CastManager.AttackTarget(0, target, chipsLost[target]));
                yield return new WaitForSeconds(1.4f);
            }
        }
        StartCoroutine(CastManager.UpdateChipCounter(potSize, 0));
        yield return new WaitForSeconds(0.1f * potSize);
        CastManager.StopSpinning(0);
        WrapUpShowdown();
    }

    public int CardsToDraw()
    {
        int cardRound = 4;
        if (pokerTurnManager.HasARiver) cardRound++;
        if (pokerTurnManager.HasABonusRound) cardRound++;
        return cardRound;
    }

    public int[] SplitThePot(List<int> playersRemaining)
    {
        int numPlayers = playersRemaining.Count;
        int baseShare = potSize / numPlayers;
        int remainder = potSize % numPlayers;

        int[] split = new int[4]; // assuming 4 players total

        // Distribute base shares
        foreach (int player in playersRemaining)
        {
            split[player] = baseShare;
        }

        // Find the player with the least current chips (from pokerChipManager)
        int minChips = int.MaxValue;
        int recipientOfRemainder = -1;

        foreach (int player in playersRemaining)
        {
            int playerChips = pokerChipManager.playerChips[player];
            if (playerChips < minChips)
            {
                minChips = playerChips;
                recipientOfRemainder = player;
            }
        }

        if (recipientOfRemainder != -1)
        {
            split[recipientOfRemainder] += remainder;
        }

        return split;
    }

    private void FoldCheck()
    {
        for (int i = 0; i < 4; i++)
        {
            if (gameManager.characters[i].isFolding)
                PlayerFold(i);
        }
    }

    public void PlayerFold(int player)
    {
        if (!gameManager.characters[player].isDead)
            CastManager.ActorFold(player);
        else
            CastManager.ActorDead(player);
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


//-----------------------------------
//End Showdown
//-----------------------------------

public void WrapUpShowdown()
{
    for (int i = 0; i < 4; i++)
    {
        if (pokerChipManager.playerChips[i] == 0)
        {
            gameManager.characters[i].isDead = true;
            CastManager.Actors[i].GetComponent<SpriteSpawner>().SetSprite(CastManager.Actors[i].GetComponent<SpriteSpawner>().DeadSprite);
            CastManager.Actors[i].GetComponent<SpriteSpawner>().originalSprite = CastManager.Actors[i].GetComponent<SpriteSpawner>().DeadSprite;
            CastManager.ActorTextEffect("Out!", CastManager.ActorText[i]);
        }
    }
    if (gameManager.characters[0].isDead)
    {
        MonsterOut();
        return;
    }
    if (gameManager.characters[1].isDead && gameManager.characters[2].isDead && gameManager.characters[3].isDead)
    {
        GameOver();
        return;
    }
    TextEffect("showdown over", showdownText);
    StartCoroutine(MonsterDecision());
}

public void MonsterOut()
{
    TextEffect("Monster Out!", showdownText);
    RewardScreen();
    //end poker battle, transition to reward screen
}
public void GameOver()
{
    TextEffect("Game Over!", showdownText);
    //end poker battle, transition to reward screen
}

private IEnumerator MonsterDecision()
{
    yield return new WaitForSeconds(2f);

    if (pokerChipManager.playerChips[0] <= Mathf.FloorToInt(gameManager.monster.monsterChips * 0.2f) ||
        (gameManager.monster.willRun && pokerChipManager.playerChips[0] <= Mathf.FloorToInt(gameManager.monster.monsterChips * 0.5f)))
    {
        TextEffect(gameManager.monster.escape, showdownText);
        yield return new WaitForSeconds(3f);
        RewardScreen();
    }
    else if (pokerChipManager.playerChips[0] >= Mathf.FloorToInt(gameManager.monster.monsterChips * 4f) || (gameManager.monster.willRun && pokerChipManager.playerChips[0] > gameManager.monster.monsterChips * 2))
    {
        TextEffect(gameManager.monster.steal, showdownText);
        yield return new WaitForSeconds(3f);
        BattleOver();
    }
    else if (pokerChipManager.playerChips[0] <= Mathf.FloorToInt(gameManager.monster.monsterChips * .6f))
    {
        TextEffect(gameManager.monster.scared, showdownText);
        gameManager.monster.willRun = true;
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    else if (pokerChipManager.playerChips[0] >= Mathf.FloorToInt(gameManager.monster.monsterChips * 2.5f))
    {
        TextEffect(gameManager.monster.confident, showdownText);
        gameManager.monster.willRun = true;
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    else
    {
        TextEffect(gameManager.monster.keepfighting, showdownText);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    StopAllCoroutines();
}

private void RewardScreen()
{
    TextEffect("You'll get a reward here", miniHand);
    BattleOver();
}
private void BattleOver()
{
    TextEffect("Battle Over!\n More to come!", showdownText);
}

}
