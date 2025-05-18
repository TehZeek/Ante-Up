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
    public Character character;
    public Monster monster;
    public bool isCharacter;
    public Image face;
    public List<GameObject> PocketCardIcons = new List<GameObject>();
    public List<GameObject> PocketCardHidden = new List<GameObject>();
    public List<GameObject> PocketNumbers = new List<GameObject>();
    public List<GameObject> PocketSuit = new List<GameObject>();
    public List<GameObject> StatusEffect = new List<GameObject>();
    public List<Sprite> pocketNumbers = new List<Sprite>();
    public List<Sprite> pocketSuit = new List<Sprite>();
    public List<Sprite> statusEffects = new List<Sprite>();

    public void RefreshHUD(List<GameObject> pocketCards, int player)
    {
        //get the player stats - is all out, has bet, is out, had folded
        //get the pocket cards for the player
        //populate the pocketcardicons, numbers, suit with the appropriate sprites
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
