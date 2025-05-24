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
    private Vector3 P1Position = new Vector3(-1188, 0, 0);
    private Vector3 P2Position = new Vector3(1068, 0, 0);
    private Vector3 P3Position = new Vector3(3333, 0, 0);
    private Vector3 P0Position = new Vector3(5610, 0, 0);
    private PokerTurnManager pokerTurnManager;
    public Material blurMaterial;
    public float blurFadeStartTime = 0.25f; // Start blur fade before movement ends
    private bool blurIsOn = false;
    public List<GameObject> Scenes = new List<GameObject>();
    public List<Transform> sceneTransform = new List<Transform>();
    public GameObject scenePrefab;

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
            if (i == 0)
            {
                Scenes[i].GetComponent<RoomPerspective>().monster = monsterManager.monster;
                Scenes[i].GetComponent<RoomPerspective>().isCharacter = false;
                Scenes[i].GetComponent<RoomPerspective>().MakeRoom(i);
            }
            else
            {
                Scenes[i].GetComponent<RoomPerspective>().character = gameManager.characters[i-1];
                Scenes[i].GetComponent<RoomPerspective>().isCharacter = true;
                Scenes[i].GetComponent<RoomPerspective>().MakeRoom(i);
            }
            
        }
        pokerTableCards.monTransform = Scenes[0].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.p1Transform = Scenes[1].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.p2Transform = Scenes[2].GetComponent<RoomPerspective>().handLocation;
        pokerTableCards.p3Transform = Scenes[3].GetComponent<RoomPerspective>().handLocation;
    }

    public void ShiftTheScene()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        Debug.Log("Shifting the scene!");
        Vector3 targetPosition = imageTransform.anchoredPosition;

        if (pokerTurnManager != null)
        {
            Debug.Log("PokerTurnManager is valid, starting shift");

            blurIsOn = true;
            float targetBlurStrength = GetBlurStrengthByTag();

            // Smoothly increase blur strength
            StartCoroutine(AdjustBlur(targetBlurStrength, 0.25f));

            switch (pokerTurnManager.turnOrder[2])
            {
                case 0: targetPosition.x = P0Position.x; break;
                case 1: targetPosition.x = P1Position.x; break;
                case 2: targetPosition.x = P2Position.x; break;
                case 3: targetPosition.x = P3Position.x; break;
            }

            // Move smoothly while starting blur fade-out early
            LeanTween.moveX(imageTransform, targetPosition.x, moveTime)
                .setEase(LeanTweenType.easeOutQuad);

        }
    }

    private IEnumerator AdjustBlur(float targetBlur, float duration)
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

    private IEnumerator BlurOff()
    {
        blurIsOn = false;
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(AdjustBlur(0, moveTime - blurFadeStartTime));
    }

    private float GetBlurStrengthByTag()
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