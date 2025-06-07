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
    public bool isCharacter;
    private GameManager gameManager;
    public RectTransform Boundary;
    public PokerActorManager actorManager; // Assign via Inspector
    private List<BGCharacterActions> instantiatedActors = new List<BGCharacterActions>();

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
        List<BGCharacterActions> playerActors = new List<BGCharacterActions>();
        BGCharacterActions enemyActor = null;
        for (int i = 0; i < 4; i++)
        {
            if (i != player)
            {
                GameObject prefab = gameManager.characters[i].battleSpritePrefab;
                GameObject actorObj = Instantiate(prefab, actorPosition[i].position, Quaternion.identity, actorPosition[i]);

                BGCharacterActions actor = actorObj.GetComponent<BGCharacterActions>();
                actor.character = gameManager.characters[i];
                actor.characterIndex = i;
                actor.characterImage = actorObj.GetComponent<Image>();
                actor.boundaryRect = Boundary;
                actor.setCharacter();
                // initialize state/sprites
                playerActors.Add(actor);
            }
        }
            actorManager.SetActors(playerActors, new List<BGCharacterActions> { enemyActor });
    }

}
