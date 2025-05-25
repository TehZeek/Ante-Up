using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOutAndDestroy : MonoBehaviour
{
    public float fadeDuration = 3f; // Duration of fade effect
    public Image image;


    public void FadeSplash()
    {
        Debug.Log("FadeSplash() called");
            StartCoroutine(FadeOutImage());
    }

    private IEnumerator FadeOutImage()
    {
        Debug.Log("FadeOutImage() called");
        Color color = image.color;
        yield return new WaitForSeconds(2f);
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
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        Debug.Log("FadeOutCanvasGroup() called");
        yield return new WaitForSeconds(2f);
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