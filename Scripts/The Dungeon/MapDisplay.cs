using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;

public class MapDisplay : MonoBehaviour
{
    public MapCard mapData;
    public Image mapImage;
    public Image[] mapDoors;
    public Image mapIcon;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void UpdateMapDisplay()
    {
        if ((int)mapData.mapObject != 1)
        {
            mapIcon.gameObject.SetActive(true);
            mapIcon.sprite = mapData.mapIconSprite;
        }
        mapImage.sprite = mapData.mapTierSprite;


        for (int i = 0; i < mapData.mapExit.Count; i++)
        {
                mapDoors[(int)mapData.mapExit[i]].gameObject.SetActive(true);
                mapDoors[(int)mapData.mapExit[i]].sprite = mapData.mapExitSprite;
        }


    }
}
