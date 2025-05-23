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
    public GameObject siloette;
    public GameObject handTop;
    public GameObject handBottom;
    public Transform handLocation;
    public Character character;
    public Monster monster;
    public bool isCharacter;
    private GameManager gameManager;
    private BattleManager battleManager;

    public void MakeRoom(int player)
    {
        if (isCharacter) 
        {
            siloette.GetComponent<Image>().sprite = character.Silloette;
            handTop.GetComponent<Image>().sprite = character.HandTop;
            handBottom.GetComponent<Image>().sprite = character.HandBottom;
        }
        else
        {
            siloette.GetComponent<Image>().sprite = monster.Silloette;
            handTop.GetComponent<Image>().sprite = monster.HandTop;
            handBottom.GetComponent<Image>().sprite = monster.HandBottom;
        }
        buildActorList(player);
    }

    private void buildActorList(int player)
    {
        gameManager = FindFirstObjectByType<GameManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        actorPrefab[0] = battleManager.monster.battleSpritePrefab;
        actorPrefab[1] = gameManager.characters[0].battleSpritePrefab;
        actorPrefab[2] = gameManager.characters[1].battleSpritePrefab;
        actorPrefab[3] = gameManager.characters[2].battleSpritePrefab;

        for (int i = 0; i < actorPrefab.Count; i++)
        {
            if (i == player) { actorPrefab[i].gameObject.SetActive(false); }
        }

    }

}
