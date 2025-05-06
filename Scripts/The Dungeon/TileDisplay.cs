using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class TileDisplay : MonoBehaviour
{
    public MapCard mapData;
    public Image tileSprite;
    public Image[] tileDoor;
    public Image tileIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void UpdateTileDisplay()
    {
        if ((int)mapData.mapObject != 1)
        {
            tileIcon.gameObject.SetActive(true);
            tileIcon.sprite = mapData.tileIcon;
        }
        tileSprite.sprite = mapData.tileSprite;


        for (int i = 0; i < mapData.mapExit.Count; i++)
        {
            tileDoor[(int)mapData.mapExit[i]].gameObject.SetActive(true);
            tileDoor[(int)mapData.mapExit[i]].sprite = mapData.tileDoor;
        } 
    }
}
