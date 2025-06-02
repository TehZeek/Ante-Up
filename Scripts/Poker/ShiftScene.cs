using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.CompilerServices;

public class ShiftScene : MonoBehaviour
{
    public RectTransform imageTransform;
    public float moveTime = 1f;
    private Vector3 P1Position = new Vector3(0, 0, 0);
    private Vector3 P2Position = new Vector3(2264, 0, 0);
    private Vector3 P3Position = new Vector3(4537, 0, 0);
    private Vector3 P0Position = new Vector3(6809, 0, 0);
    private Vector3 P4Position = new Vector3(9100, 0, 0);
    private PokerTurnManager pokerTurnManager;
    public Material blurMaterial;
    public float blurFadeStartTime = 0.25f; // Start blur fade before movement ends
    private bool blurIsOn = false;
    public List<GameObject> Scenes = new List<GameObject>();
    public List<Transform> sceneTransform = new List<Transform>();
    public GameObject scenePrefab;
    private int whichScene;


    public void BuildScenes()
    {
        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        PokerTableCards pokerTableCards = FindFirstObjectByType<PokerTableCards>();
 
        blurMaterial.SetFloat("_BlurStrength", 0);
        for (int i = 0; i<4;i++)
        {
            //make a TEMP scene, add it to Scenes
            GameObject tempScene = Instantiate(scenePrefab, sceneTransform[i].position, Quaternion.identity, sceneTransform[i]);
            Scenes.Add(tempScene);
            RoomPerspective room = tempScene.GetComponent<RoomPerspective>();
            room.character = gameManager.characters[i];
            room.MakeRoom(i);
        }
        GameObject tempyScene = Instantiate(scenePrefab, sceneTransform[4].position, Quaternion.identity, sceneTransform[4]);
        Scenes.Add(tempyScene);
        Scenes[4].GetComponent<RoomPerspective>().character = gameManager.characters[1];
        Scenes[4].GetComponent<RoomPerspective>().MakeRoom(1);
        pokerTableCards.monTransform = Scenes[0].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.p1Transform = Scenes[4].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.p2Transform = Scenes[2].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.p3Transform = Scenes[3].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.remapHands();
    }

    public void ShiftTheScene()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        Vector3 targetPosition = imageTransform.anchoredPosition;

        if (pokerTurnManager != null)
        {
            for (int i = 0; i < 4; i++)
            {
                Scenes[i].GetComponent<RoomPerspective>().actorManager.UpdateActorActivityBasedOnTurn();

                if (i == 0)
                {
                    Scenes[i].GetComponent<RoomPerspective>().actorManager.StartMonsterIdleMode();

                }
                if (whichScene == 4)
                {
                    transform.position = P1Position;
                }
                blurIsOn = true;
                float targetBlurStrength = GetBlurStrengthByTag();

                // Smoothly increase blur strength
                StartCoroutine(AdjustBlur(targetBlurStrength, 0.25f));
                whichScene = pokerTurnManager.turnOrder[2];
                // Player 1 uses fake scene index 4
                if (whichScene == 1) { whichScene = 4; }
                switch (whichScene)
                {
                    case 0: targetPosition.x = P0Position.x; break;
                    case 1: targetPosition.x = P1Position.x; break;
                    case 2: targetPosition.x = P2Position.x; break;
                    case 3: targetPosition.x = P3Position.x; break;
                    case 4: targetPosition.x = P4Position.x; break;
                }

                // Move smoothly while starting blur fade-out early
                LeanTween.moveX(imageTransform, targetPosition.x, moveTime)
                    .setEase(LeanTweenType.easeOutQuad);
            }
        }
    }

    IEnumerator AdjustBlur(float targetBlur, float duration)
    {
        float startBlur = blurMaterial.GetFloat("_BlurStrength");
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            blurMaterial.SetFloat("_BlurStrength", Mathf.Lerp(startBlur, targetBlur, elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        blurMaterial.SetFloat("_BlurStrength", targetBlur); // Ensure final value is set
        if (blurIsOn) { StartCoroutine(BlurOff()); }
        blurIsOn = false;
    }

    IEnumerator BlurOff()
    {
        blurIsOn = false;
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(AdjustBlur(0, moveTime - blurFadeStartTime));
    }

    float GetBlurStrengthByTag()
    {
        switch (gameObject.tag)
        {
            case "Sprite": return 0.2f;
            case "Background": return 2f;
            case "Foreground": return 0.3f;
            default: return 0.3f;
        }
    }
}