using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOutAndDestroy : MonoBehaviour
{
    public float fadeDuration = 2f; // Duration of fade effect
    private Image image;
    private CanvasGroup canvasGroup; // Alternative for UI fading

    void Start()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (image != null)
        {
            StartCoroutine(FadeOutImage());
        }
        else if (canvasGroup != null)
        {
            StartCoroutine(FadeOutCanvasGroup());
        }
    }

    private IEnumerator FadeOutImage()
    {
        Color color = image.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            image.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Destroy after fade out
    }

    private IEnumerator FadeOutCanvasGroup()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}