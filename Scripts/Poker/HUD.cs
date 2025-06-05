using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using ZeekSpace;

public class HUD : MonoBehaviour
{
    public Character characterHud;
    public bool isCharacter = false;

    [Header("UI Elements")]
    public Image face;
    public List<GameObject> PocketCardIcons = new List<GameObject>();
    public List<GameObject> PocketCardHidden = new List<GameObject>();
    public List<GameObject> PocketNumbers = new List<GameObject>();
    public List<GameObject> PocketSuit = new List<GameObject>();
    public List<GameObject> StatusEffect = new List<GameObject>();
    public List<Sprite> statusEffects = new List<Sprite>(); // 0: Folded, 1: Better, 2: All-In, 3: Out, 4: Needs To Call
    public GameObject chipsDisplay;
    public GameObject ChipImage;
    public GameObject MinHand;
    public bool isBetter = false;
    public List<TextMeshProUGUI> StatusTexts = new List<TextMeshProUGUI>(); // parallel to StatusEffect
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;

    void Awake()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
    }

    public void MakeHUD()
    {

        if (isCharacter)
        {
            face.sprite = characterHud.HUDSprite;
        }
        else
        {
            face.sprite = characterHud.HUDSprite;
            MinHand.SetActive(true);

            MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
            BattleManager battleManager = FindFirstObjectByType<BattleManager>();
            MinHand.GetComponent<TextMeshProUGUI>().text =
                "Minimum Hand: " +
                monsterManager.monster.minimumHand.handRank +
                " of " + monsterManager.monster.minimumRank + "'s";
        }

        ClearHUD();
    }

    public void RefreshHUD(List<GameObject> pocketCards, int player)
    {
        ClearHUD();
        MakeHUDStatus(pocketCards.Count, player);
        MakeHUDCards(pocketCards, player);
        DisplayHUDChips(player);
    }

    private void MakeHUDStatus(int pocketCardCount, int player)
    {
        int statusIndex = (pocketCardCount == 0) ? 4 : pocketCardCount - 1;

        Sprite statusSprite = null;
        bool showStatus = false;

        bool isCurrentTurn = (pokerTurnManager.turnOrder[2] == player);
        bool isOut = pokerTurnManager.IsOut[player];
        bool isAllIn = pokerTurnManager.isAllIn[player];
        bool hasChips = pokerChipManager.playerChips[player] > 0;

        int amountToCall = Mathf.Max(0, pokerChipManager.BetSize - pokerChipManager.InThePot[player]);

        if (isOut && !isAllIn)
        {
            statusSprite = statusEffects[0]; // Folded
            showStatus = true;
        }
        else if (isBetter)
        {
            statusSprite = statusEffects[1]; // Has better hand
            showStatus = true;
        }
        else if (isAllIn)
        {
            statusSprite = statusEffects[2]; // All-In
            showStatus = true;
        }
        else if (!hasChips)
        {
            statusSprite = statusEffects[3]; // Out (no chips)
            showStatus = true;
        }
        else if (isCurrentTurn && amountToCall > 0)
        {
            if (statusIndex < StatusTexts.Count)
            {
                StatusTexts[statusIndex].text = "To Call: " + amountToCall;
                StatusTexts[statusIndex].gameObject.SetActive(true);
            }
        }

        if (showStatus && statusSprite != null && statusIndex < StatusEffect.Count)
        {
            StatusEffect[statusIndex].GetComponent<Image>().sprite = statusSprite;
            StatusEffect[statusIndex].SetActive(true);
        }
    }

    public void DisplayHUDChips(int player)
    {
        if (player < 0 || player >= pokerChipManager.playerChips.Length)
        {
            Debug.LogError($"Invalid player index {player}.");
            return;
        }

        bool showChips = pokerChipManager.playerChips[player] > 0 || pokerTurnManager.isAllIn[player];

        ChipImage?.SetActive(showChips);
        chipsDisplay?.SetActive(showChips);

        if (showChips)
        {
            chipsDisplay.GetComponent<TextMeshProUGUI>().text = pokerChipManager.playerChips[player].ToString();
        }
    }

    private void MakeHUDCards(List<GameObject> pocketCards, int player)
    {
        int numCards = pocketCards.Count;

        for (int i = 0; i < numCards; i++)
        {
            int reversedIndex = numCards - 1 - i;
            GameObject card = pocketCards[reversedIndex];
            CardDisplay display = card.GetComponent<CardDisplay>();

            PocketCardIcons[i].SetActive(true);
            PocketCardHidden[i].SetActive(player == 0); // Only show hidden card for player 0 (character)
            PocketNumbers[i].SetActive(true);
            PocketSuit[i].SetActive(true);

            PocketNumbers[i].GetComponent<Image>().sprite = display.rankImage[0].sprite;
            PocketNumbers[i].GetComponent<Image>().color = display.rankImage[0].color;
            PocketSuit[i].GetComponent<Image>().sprite = display.cardData.suitSprite;
        }
    }

    public void ClearHUD()
    {
        for (int i = 0; i < PocketCardIcons.Count; i++)
        {
            PocketCardIcons[i].SetActive(false);
            PocketCardHidden[i].SetActive(false);
            PocketNumbers[i].SetActive(false);
            PocketSuit[i].SetActive(false);
            StatusEffect[i].SetActive(false);
        }
        for (int i = 0; i < StatusTexts.Count; i++)
        {
            StatusTexts[i].text = "";
            StatusTexts[i].gameObject.SetActive(false);
        }

        ChipImage?.SetActive(false);
        chipsDisplay?.SetActive(false);
    }
}