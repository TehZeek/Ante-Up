using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.CompilerServices;
public class RoomPerspective : MonoBehaviour
{
    public GameObject Background;
    public List<GameObject> actorPrefab = new List<GameObject>();
    public List<Transform> actorPosition = new List<Transform>();
    public GameObject siloette;
    public GameObject handTop;
    public GameObject handBottom;
    public Transform handLocation;
    public Character character;
    public Monster monster;
    public bool isCharacter;
    private GameManager gameManager;
    private MonsterManager monsterManager;

    public void MakeRoom(int player)
    {
            siloette.GetComponent<Image>().sprite = character.Silloette;
            handTop.GetComponent<Image>().sprite = character.HandTop;
            handBottom.GetComponent<Image>().sprite = character.HandBottom;
        buildActorList(player);
    }

    private void buildActorList(int player)
    {
        gameManager = FindFirstObjectByType<GameManager>();
        monsterManager = FindFirstObjectByType<MonsterManager>();
        for (int i=0; i<4; i++)
        {
            actorPrefab[i] = gameManager.characters[0].battleSpritePrefab;
        }

        for (int i = 0; i < actorPrefab.Count; i++)
        {
            if (i != player) 
            {
                Instantiate(actorPrefab[i], actorPosition[i].position, Quaternion.identity, actorPosition[i]);
            }
        }

    }

}
