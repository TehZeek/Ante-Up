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
    private GameManager gameManager;
    private ChipManager chipManager;
    public int startingHandSize = 5;
    public int maxHandSize = 5;

    void Start()
    {
        MapCard[] cards = Resources.LoadAll<MapCard>("MapStart");
        allMapCards.AddRange(cards);
        gameManager = FindFirstObjectByType<GameManager>();

    }

    void Update()
    {
        if (gameManager.startDungeonRun)
        {
            gameManager.startDungeonRun = false;
            DungeonSetup();
        }
    }

    public void DungeonSetup()
    {
        drawPileManager = FindFirstObjectByType<DrawPileManager>();
        mapHandManager = FindFirstObjectByType<MapHandManager>();
        chipManager = FindFirstObjectByType<ChipManager>();

        mapHandManager.DungeonSetup(maxHandSize);
        drawPileManager.MakeDrawPile(allMapCards);
        drawPileManager.DungeonSetup(startingHandSize, maxHandSize);
        chipManager.DungeonSetup();
    }

}
