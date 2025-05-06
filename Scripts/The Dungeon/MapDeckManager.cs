using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class MapDeckManager : MonoBehaviour
{
    public List<MapCard> allMapCards = new List<MapCard>();
    private MapHandManager mapHandManager;
    private DrawPileManager drawPileManager;
    private ChipManager chipManager;
    private bool startDungeonRun = false;
    public int startingHandSize = 5;
    public int maxHandSize = 5;

    void Start()
    {
        MapCard[] cards = Resources.LoadAll<MapCard>("MapStart");
        allMapCards.AddRange(cards);
        startDungeonRun = true;
        Debug.Log("Started DeckManager" + startDungeonRun);
    }

    void Awake()
    {
        if (drawPileManager == null)
        {
            drawPileManager = FindFirstObjectByType<DrawPileManager>();
        }
        if (mapHandManager == null)
        {
            mapHandManager = FindFirstObjectByType<MapHandManager>();
        }
        if (chipManager == null)
        {
            chipManager = FindFirstObjectByType<ChipManager>();
        }
    }

    void Update()
    {
        if (startDungeonRun)
        {
            Debug.Log("Started Update" + startDungeonRun);
            startDungeonRun = false;
            Debug.Log("Starting Dungeon Setup" + startDungeonRun);
            DungeonSetup();
        }
    }

    public void DungeonSetup()
    {
        mapHandManager.DungeonSetup(maxHandSize);
        drawPileManager.MakeDrawPile(allMapCards);
        drawPileManager.DungeonSetup(startingHandSize, maxHandSize);
        chipManager.DungeonSetup();

        Debug.Log("Finished MapDeckManager Dungeon setup");
    }

}
