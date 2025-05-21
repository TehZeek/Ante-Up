using UnityEngine;
using System.Collections;

public class HopMovement : MonoBehaviour
{
    public float forwardDistance = 2f; // How far forward it hops
    public float hopHeight = 1f; // How high it hops
    public float hopTime = 0.5f; // Time taken for the hop forward
    public float returnTime = 0.5f; // Time taken to return
    public float multiplier = 10000f;

    private Vector3 originalPosition;
    private Vector3 midHop;
    private bool ishopping = false;
    public enum CharacterType { battleSprite, silloette, hand };
    public CharacterType characterType;

    void Start()
    {
        originalPosition = transform.localPosition;
        StartCoroutine(HopLoop());
    }

    IEnumerator HopLoop()
    {
        while (true) // Runs infinitely unless stopped
        {
            float waitTime = Random.Range(1.5f, 4f); // Random wait time
            yield return new WaitForSeconds(waitTime); // Wait before hopping

            if (!ishopping) // Ensures no overlapping hops
            {
                randomizeHop();
            }
        }
    }

    public void randomizeHop()
    {
        ishopping = true;

        // Modify behavior based on character type
        switch (characterType)
        {
            case CharacterType.battleSprite:
                forwardDistance = Random.Range(-20f, 20f) * multiplier;
                hopHeight = Random.Range(-2.5f, 15f) * multiplier;
                hopTime = Random.Range(.25f, .75f);
                returnTime = Random.Range(0.4f, 1.0f);
                break;
            case CharacterType.silloette:
                forwardDistance = Random.Range(-3f, -0.5f) * multiplier;
                hopHeight = Random.Range(-1f, -3f) * multiplier;
                hopTime = Random.Range(1.5f, 4.0f);
                returnTime = Random.Range(1.5f, 4.0f);
                break;
            case CharacterType.hand:
                forwardDistance = Random.Range(-1.5f, 1.5f) * multiplier;
                hopHeight = Random.Range(-4f, -0.5f) * multiplier;
                hopTime = Random.Range(1.5f, 4.0f);
                returnTime = Random.Range(1.5f, 4.0f);
                break;
            default:
                forwardDistance = Random.Range(-20f, 20f) * multiplier;
                hopHeight = Random.Range(-2.5f, 15f) * multiplier;
                hopTime = Random.Range(.25f, .75f);
                returnTime = Random.Range(.4f, 1.0f);
                break;
        }

        Hop();
    }



    public void Hop()
    {
        // Move forward in an arc motion (hop)
        LeanTween.moveLocal(gameObject, originalPosition + new Vector3(forwardDistance, hopHeight, 0), hopTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(LandHop);
    }

    public void LandHop()
    {
        midHop = transform.localPosition;
        // Move forward in an arc motion (hop)
        LeanTween.moveLocal(gameObject, midHop + new Vector3(forwardDistance*0.75f, -hopHeight, 0), hopTime*0.8f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(ReturnToOriginalPosition);
    }

    private void ReturnToOriginalPosition()
    {
        // Move back to original position
        LeanTween.moveLocal(gameObject, originalPosition, returnTime).setEase(LeanTweenType.easeInQuad);
        ishopping = false;

    }
}