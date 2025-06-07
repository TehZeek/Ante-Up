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
    public RectTransform showdownTransform;
    public float moveTime = 1f;

    // Scene anchor positions (x only; anchoredPosition is Vector2)
    private readonly Vector3 P1Position = new Vector3(0, 0, 0);
    private readonly Vector3 P2Position = new Vector3(2264, 0, 0);
    private readonly Vector3 P3Position = new Vector3(4537, 0, 0);
    private readonly Vector3 P0Position = new Vector3(6809, 0, 0);
    private readonly Vector3 P4Position = new Vector3(9100, 0, 0);
    private readonly Vector3 ShowdownPosition = new Vector3(0, -1280, 0);

    private PokerTurnManager pokerTurnManager;
    public Material blurMaterial;
    public float blurFadeStartTime = 0.25f; // Start blur fade before movement ends
    private bool blurIsOn = false;

    public List<GameObject> Scenes = new List<GameObject>();
    public List<Transform> sceneTransform = new List<Transform>();
    public GameObject scenePrefab;

    private int whichScene;
    private bool GamePlay = false;

    // Cache last X position to figure out direction/skip‑animation cases
    private float _lastAnchorX = float.NaN;

    #region BUILD
    public void BuildScenes()
    {
        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        PokerTableCards pokerTableCards = FindFirstObjectByType<PokerTableCards>();

        blurMaterial.SetFloat("_BlurStrength", 0);
        for (int i = 0; i < 4; i++)
        {
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

        _lastAnchorX = imageTransform.anchoredPosition.x;
    }
    #endregion

    #region MAIN SHIFT
    public void ShiftTheScene()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        if (!GamePlay || pokerTurnManager == null) return;

        // Determine logical scene index
        whichScene = pokerTurnManager.turnOrder[2];
        if (whichScene == 1) whichScene = 4; // player 1 ≙ scene 4 visually

        // Translate to x coordinate
        float targetX = imageTransform.anchoredPosition.x;
        switch (whichScene)
        {
            case 0: targetX = P0Position.x; break;
            case 1: targetX = P1Position.x; break;
            case 2: targetX = P2Position.x; break;
            case 3: targetX = P3Position.x; break;
            case 4: targetX = P4Position.x; break;
        }

        // -------------------------------------------------
        // WRAP FIX: if coming FROM scene 4 (x = 9100) to ANY
        // other scene, first TELEPORT to scene 1 (x = 0) so
        // the ensuing animation always moves rightward.
        // -------------------------------------------------
        if (Mathf.Approximately(_lastAnchorX, P4Position.x) && !Mathf.Approximately(targetX, P4Position.x))
        {
            // instant snap from 4 → 1, no blur
            imageTransform.anchoredPosition = new Vector2(P1Position.x, imageTransform.anchoredPosition.y);
            _lastAnchorX = P1Position.x;
        }

        // Skip anim only for direct 1 ⇄ 4 jumps (handled above for 4→1)
        bool skipAnim =
            (Mathf.Approximately(_lastAnchorX, P1Position.x) && Mathf.Approximately(targetX, P4Position.x)) ||
            (Mathf.Approximately(_lastAnchorX, P4Position.x) && Mathf.Approximately(targetX, P1Position.x));

        // Blur only when animating
        if (!skipAnim)
        {
            blurIsOn = true;
            float targetBlurStrength = GetBlurStrengthByTag();
            StartCoroutine(AdjustBlur(targetBlurStrength, 0.25f));
        }
        else
        {
            blurMaterial.SetFloat("_BlurStrength", 0);
        }

        // Execute move
        if (skipAnim)
        {
            imageTransform.anchoredPosition = new Vector2(targetX, imageTransform.anchoredPosition.y);
        }
        else
        {
            LeanTween.moveX(imageTransform, targetX, moveTime).setEase(LeanTweenType.easeOutQuad);
        }

        _lastAnchorX = targetX; // remember for next shift

        // Actor updates
        for (int i = 0; i < 4; i++)
        {
            var manager = Scenes[i].GetComponent<RoomPerspective>().actorManager;
            manager.UpdateActorActivityBasedOnTurn();
            if (i == 0) manager.StartMonsterIdleMode();
        }
    }
    #endregion

    #region SHOWDOWN & V‑TRANSITIONS
    public void ShowDownShift()
    {
        GamePlay = false;
        blurIsOn = true;
        float targetBlurStrength = GetBlurStrengthByTag();

        LeanTween.moveY(imageTransform, ShowdownPosition.y, moveTime).setEase(LeanTweenType.easeOutQuad);
        LeanTween.moveY(showdownTransform, P0Position.y, moveTime).setEase(LeanTweenType.easeOutQuad);
    }

    public void ShiftDown()
    {
        GamePlay = true;
        ShiftTheScene();
        LeanTween.moveY(imageTransform, P0Position.y, moveTime).setEase(LeanTweenType.easeOutQuad);
        LeanTween.moveY(showdownTransform, ShowdownPosition.y, moveTime).setEase(LeanTweenType.easeOutQuad);
    }
    #endregion

    #region BLUR HELPERS
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
        blurMaterial.SetFloat("_BlurStrength", targetBlur);
        if (blurIsOn) { StartCoroutine(BlurOff()); }
        blurIsOn = false;
    }

    IEnumerator BlurOff()
    {
        blurIsOn = false;
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(AdjustBlur(0, moveTime - blurFadeStartTime));
    }
    #endregion

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
