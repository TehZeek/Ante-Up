using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    void Awake()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        blurMaterial.SetFloat("_BlurStrength", 0);
    }

    public void ShiftTheScene()
    {
        Vector3 targetPosition = imageTransform.anchoredPosition;

        if (pokerTurnManager != null)
        {
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