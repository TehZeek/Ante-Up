using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PokerRoom : MonoBehaviour
{
    private List<GameObject> HUDs = new List<GameObject>();


    void Start()
    {
        //load the Poker Room, our variables, characters, hud, monster, monsterhud, managers.  Pull the character list
        //from the GameManager, make hand prefabs
    }
    
    public void TransitionSetup()
    {
        //we're going to build the scene to the left of the current screen
        //set which 3 characters are puppets, where they stand in relation to siloette player
        // set silloette player, pull out the hands
    }

    private void TransitionSlide()
    {
        //we're going to slide characters, silloettes, BG to the right (not the Hud items) when the turn order changes
        // put various pauses
    }

    private void RemoveOldScene()
    {
        //we're going to remove the old scene from the cache
    }
}
