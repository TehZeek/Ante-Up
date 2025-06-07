using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class CardShowdown : MonoBehaviour
{
    public List<GameObject> showdownCards = new List<GameObject>();
    public HandTypes hand;
    public TextMeshProUGUI handText;
    public List<Vector2> firstDestination = new List<Vector2>();
    public List<Vector2> secondDestination = new List<Vector2>();
    public float moveSpeed = 600f; // Adjust the speed as needed
    private Coroutine bounceRoutine;
    public Vector2 startOffset = new Vector2(550, 0);
    public Vector3 handTextOffset = new Vector3(135f, 0, 0);


    // Moves the cards from the right of the screen to their first destination
    public void EnterScreen()
    {
        StartCoroutine(EnterScreenRoutine());
    }

    IEnumerator MoveHandTextToCard(GameObject targetCard, Action onArrive = null)
    {
        StartBounce();
        float followSpeed = 500f;
        Vector3 targetPos = targetCard.transform.localPosition + handTextOffset;

        while (Vector3.Distance(handText.rectTransform.localPosition, targetPos) > 1f)
        {
            handText.rectTransform.localPosition = Vector3.MoveTowards(
                handText.rectTransform.localPosition,
                targetPos,
                followSpeed * Time.deltaTime
            );
            targetPos = targetCard.transform.localPosition + handTextOffset; // In case the card is still moving
            yield return null;
        }

        handText.rectTransform.localPosition = targetPos; // Final snap
        StopBounce();
        onArrive?.Invoke();
        
    }

    void StartBounce()
    {
        if (bounceRoutine != null)
            StopCoroutine(bounceRoutine);
        bounceRoutine = StartCoroutine(AnimateHandTextBounce());
    }

    void StopBounce()
    {
        if (bounceRoutine != null)
        {
            StopCoroutine(bounceRoutine);
            bounceRoutine = null;
        }

        // Reset character positions
        TMP_Text textComponent = handText;
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Vector3[] vertices = textInfo.meshInfo[i].vertices;
            Vector3[] originalVertices = textInfo.meshInfo[i].mesh.vertices;

            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] = originalVertices[j]; // Reset vertex offset
            }

            textInfo.meshInfo[i].mesh.vertices = vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    IEnumerator AnimateHandTextBounce()
    {
        TMP_Text textComponent = handText;
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        float bounceAmplitude = 6f;
        float bounceFrequency = 24f;

        while (true)
        {
            textComponent.ForceMeshUpdate();
            textInfo = textComponent.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 offset = new Vector3(0, Mathf.Sin(Time.time * bounceFrequency + i * 0.2f) * bounceAmplitude, 0);

                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;
            }

            // Push the changes to the mesh
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }
    }



    IEnumerator EnterScreenRoutine()
    {
        for (int i = 0; i < showdownCards.Count; i++)
        {
            showdownCards[i].SetActive(true);
            showdownCards[i].transform.rotation = Quaternion.identity;
            showdownCards[i].transform.localPosition = startOffset;

            StartCoroutine(MoveCard(showdownCards[i], firstDestination[i]));

            yield return new WaitForSeconds(0.35f); // Delay before the next card starts
        }
        handText.rectTransform.localPosition = startOffset;
        StartCoroutine(MoveHandTextToCard(showdownCards[showdownCards.Count - 1], MoveToSecondScreen));

    }

    public void MoveToSecondScreen()
    {
        for (int i = 0; i < showdownCards.Count; i++)
        {
            showdownCards[i].transform.localPosition = firstDestination[i];
        }
        StartCoroutine(MoveCardsToTempPositions());
        StartCoroutine(MoveHandTextToCard(showdownCards[showdownCards.Count - 1]));
    }

    IEnumerator briefPause()
    {
        for (int i = (showdownCards.Count-1); i >-1; i--)
        {
            StartCoroutine(MoveCard(showdownCards[i], secondDestination[i]));
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(MoveHandTextToCard(showdownCards[showdownCards.Count - 1]));
    }

    IEnumerator FollowCardDuringExit(GameObject targetCard)
    {
        while (targetCard.activeSelf)
        {
            handText.rectTransform.localPosition = targetCard.transform.localPosition + new Vector3(100f, 0, 0);
            yield return null;
        }
    }

    // Coroutine to smoothly move the card
    IEnumerator MoveCard(GameObject card, Vector2 targetPosition, Action onComplete = null)
    {
        float speed = moveSpeed;
        float maxFastMoveDuration = 1f;
        float maxSlowMoveDuration = .5f; 
        float timer = 0f;

        while (Vector2.Distance(card.transform.localPosition, targetPosition) > 5f && timer < maxFastMoveDuration)
        {
            card.transform.localPosition = Vector2.MoveTowards(card.transform.localPosition, targetPosition, speed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (Vector2.Distance(card.transform.localPosition, targetPosition) > 0.1f && timer < maxSlowMoveDuration)
        {
            card.transform.localPosition = Vector2.Lerp(card.transform.localPosition, targetPosition, 0.2f);
            timer += Time.deltaTime;
            yield return null;
        }

        card.transform.localPosition = targetPosition;

        // Bounce
        Vector2 bounceUp = targetPosition + new Vector2(0, 15f);
        Vector2 bounceDown = targetPosition;
        float bounceDuration = 0.15f;

        timer = 0f;
        while (timer < bounceDuration)
        {
            card.transform.localPosition = Vector2.Lerp(targetPosition, bounceUp, timer / bounceDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < bounceDuration)
        {
            card.transform.localPosition = Vector2.Lerp(bounceUp, bounceDown, timer / bounceDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        card.transform.localPosition = targetPosition;

        // Signal completion
        onComplete?.Invoke();
    }

    IEnumerator MoveCardsToTempPositions()
    {
        int cardsCompleted = 0;

        for (int i = 0; i < showdownCards.Count; i++)
        {
            int index = i; // Avoid closure issue
            StartCoroutine(MoveCard(showdownCards[index], secondDestination[0], () =>
            {
                cardsCompleted++;
            }));
        }

        // Wait until all cards finish moving
        while (cardsCompleted < showdownCards.Count)
        {
            yield return null;
        }
        StartCoroutine(MoveHandTextToCard(showdownCards[showdownCards.Count - 1]));
        StartCoroutine(briefPause());
    }

    public void ThrowCardsOffscreen()
    {
        foreach (GameObject card in showdownCards)
        {
            StartCoroutine(ThrowCardRoutine(card));
        }
    }

    IEnumerator ThrowCardRoutine(GameObject card)
    {
        Vector2 startPos = card.transform.localPosition;
        float angle = UnityEngine.Random.Range(45f, 135f);
        float force = UnityEngine.Random.Range(800f, 1200f);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector2 velocity = direction * force;
        float rotationSpeed = UnityEngine.Random.Range(720f, 1440f);
        float duration = 1.5f;
        float time = 0f;

        bool isFinalCard = card == showdownCards[showdownCards.Count - 1];

        while (time < duration)
        {
            float gravity = -2000f;
            velocity += Vector2.up * gravity * Time.deltaTime;
            card.transform.localPosition += (Vector3)(velocity * Time.deltaTime);
            card.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            if (isFinalCard)
            {
                handText.rectTransform.localPosition = card.transform.localPosition + new Vector3(100f, 0, 0);
            }

            time += Time.deltaTime;
            yield return null;
        }

        card.SetActive(false);
    }



}
