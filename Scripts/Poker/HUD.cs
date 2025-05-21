using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZeekSpace;

public class HUD : MonoBehaviour
{
    public Character characterHud;
    public Monster monsterHud;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;
    public bool isCharacter;
    public Image face;
    public List<GameObject> PocketCardIcons = new List<GameObject>();
    public List<GameObject> PocketCardHidden = new List<GameObject>();
    public List<GameObject> PocketNumbers = new List<GameObject>();
    public List<GameObject> PocketSuit = new List<GameObject>();
    public List<GameObject> StatusEffect = new List<GameObject>();
    public List<Sprite> statusEffects = new List<Sprite>();
    public GameObject chipsDisplay;
    public GameObject ChipImage;
    public bool isBetter = false;

    void Awake()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();

    }

    public void RefreshHUD(List<GameObject> pocketCards, int player)
    {
        ClearHUD();
        MakeHUDStatus(pocketCards.Count, player);
        MakeHUDCards(pocketCards, player);
        DisplayHUDChips(player);
    }

    private void MakeHUDStatus(int pocketCards, int player)
    {
        if (pocketCards == 0) { pocketCards = 4; }
        else {pocketCards--; }

        if (!pokerTurnManager.isAllIn[player] && pokerChipManager.playerChips[player] == 0)
        {
            StatusEffect[pocketCards].GetComponent<Image>().sprite = statusEffects[3];
            StatusEffect[pocketCards].SetActive(true);
        }

        else if (pokerTurnManager.isAllIn[player])
        {
            StatusEffect[pocketCards].GetComponent<Image>().sprite = statusEffects[2];
            StatusEffect[pocketCards].SetActive(true);
        }
        else if (pokerTurnManager.IsOut[player] && !pokerTurnManager.isAllIn[player])
        {
            StatusEffect[pocketCards].GetComponent<Image>().sprite = statusEffects[0];
            StatusEffect[pocketCards].SetActive(true);
        }

        else if (isBetter)
        {
            StatusEffect[pocketCards].GetComponent<Image>().sprite = statusEffects[1];
            StatusEffect[pocketCards].SetActive(true);
        }


        else
        {
            StatusEffect[pocketCards].SetActive(false);
        }

        //add in a "Needs X to call" to current player
    }

    public void DisplayHUDChips(int player)
    {
        if (chipsDisplay == null)
        {
            Debug.LogError($"chipsDisplay is null in HUD for player {player}");
        }
        if (ChipImage == null)
        {
            Debug.LogError("ChipImage is null in HUD");
        }
        if (pokerChipManager == null)
        {
            Debug.LogError("PokerChipManager is not found in HUD.");
        }
        if (player < 0 || player >= pokerChipManager.playerChips.Length)
        {
            Debug.LogError($"Invalid player index {player}. Array length: {pokerChipManager.playerChips.Length}");
            return;
        }



        if (pokerChipManager.playerChips[player] > 0 || pokerTurnManager.isAllIn[player])
        {
            ChipImage.SetActive(true);
            chipsDisplay.SetActive(true);
            chipsDisplay.GetComponent<TextMeshProUGUI>().text = pokerChipManager.playerChips[player].ToString();
            Debug.Log("Refreshed Chips");
        }
    }

    private void MakeHUDCards(List<GameObject> pocketCards, int player)
    {
        int numCards = pocketCards.Count;
        List<int> tempswap = new List<int>();
        for (int j = numCards-1; j >=0; j--)
        {
            tempswap.Add(j);
        }

        for (int i = 0; i < numCards; i++)
        {

            PocketCardIcons[i].SetActive(true);
            if (player == 0)
            {
                PocketCardHidden[i].SetActive(true);
            }
            
            PocketNumbers[i].SetActive(true);
            PocketNumbers[i].GetComponent<Image>().sprite = pocketCards[tempswap[i]].GetComponent<CardDisplay>().rankImage[0].sprite;
            PocketNumbers[i].GetComponent<Image>().color = pocketCards[tempswap[i]].GetComponent<CardDisplay>().rankImage[0].color;
            PocketSuit[i].SetActive(true);
            PocketSuit[i].GetComponent<Image>().sprite = pocketCards[tempswap[i]].GetComponent<CardDisplay>().cardData.suitSprite;
        }
        // can i do something here to reverse the order they are displayed?
        // 2 would go from 0, 1 to 1, 0
        // 3 from 0,1,2 to 2,1,0
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
            ChipImage.SetActive(false);
            chipsDisplay.SetActive(false);
    }

    public void MakeHUD()
    {
        if (isCharacter) { face.sprite = characterHud.HUDSprite; }
        else { face.sprite = monsterHud.HUDSprite; }
        ClearHUD();
    }

    private void TransitionSetup()
    {
        //we're going to build the scene to the left of the current screen
    }

    private void TransitionSlide()
    {
        //we're going to slide everything to the right
    }

    private void RemoveOldScene()
    {
        //we're going to remove the old scene
    }

}
