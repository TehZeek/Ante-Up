using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterTalk : MonoBehaviour
{
    [Header("Wiggle Settings")]
    public float wiggleAmplitude = 10f;
    public float wiggleSpeed = 10f;
    public float wiggleDuration = 1f;

    [Header("Slide Settings")]
    public float slideSpeed = 1000f;
    public bool Side = true; // true = left, false = right

    private Vector2 onScreenPosition;
    private Vector2 offScreenPosition;

    private RectTransform rectTransform;
    private Coroutine wiggleRoutine;
    private Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (Side)
        {
            onScreenPosition = new Vector2(-658f, -50f);
            offScreenPosition = onScreenPosition + new Vector2(-1000f, 0f);
        }
        else
        {
            onScreenPosition = new Vector2(658f, -50f);
            offScreenPosition = onScreenPosition + new Vector2(1000f, 0f);
        }

        rectTransform.anchoredPosition = offScreenPosition;
        SetImageAlpha(0f); // Start fully transparent
    }

    public void EnterScene()
    {
        SetImageAlpha(1f); // Make visible
        StartCoroutine(SlideTo(onScreenPosition));
    }

    public void ExitScene()
    {
        StopAllCoroutines();
        StartCoroutine(SlideOutAndFade());
    }

    private IEnumerator SlideTo(Vector2 target)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, target) > 1f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                target,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }

    private IEnumerator SlideOutAndFade()
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, offScreenPosition) > 1f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                offScreenPosition,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        rectTransform.anchoredPosition = offScreenPosition;
        SetImageAlpha(0f); // Fade to invisible
    }

    private void SetImageAlpha(float alpha)
    {
        if (image != null)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }

    public void IsTalking()
    {
        if (wiggleRoutine != null)
            StopCoroutine(wiggleRoutine);

        wiggleRoutine = StartCoroutine(Wiggle());
    }

    private IEnumerator Wiggle()
    {
        float timer = 0f;

        while (timer < wiggleDuration)
        {
            float offset = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmplitude;
            rectTransform.anchoredPosition = onScreenPosition + new Vector2(0f, offset);
            timer += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = onScreenPosition;
        wiggleRoutine = null;
    }
}
