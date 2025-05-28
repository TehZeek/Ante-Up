using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ZeekSpace;

using UnityEngine.UI;
using TMPro;

public class SpriteSpawner : MonoBehaviour
{
    [Header("Actor Setup")]
    public Character character;
    public RectTransform actor;
    public Sprite BattleSprite;
    public Sprite HurtSprite;
    public Sprite AttackSprite;
    public Sprite DeadSprite;
    public Sprite originalSprite;
    public bool isRanged = false;

    [Header("Projectiles")]
    public GameObject[] spritePrefabs;
    public GameObject projectilePrefab;
    public float speed = 400f;
    public float rotationSpeed = 360f; 
    public Transform parentTransform;
    public float spawnDelay = 0.05f;
    public List<Vector2> targetPosition = new List<Vector2>();

    [Header("Movement")]
    public float leapDuration = 0.7f;
    public float arcHeight = 50f;
    public Image screenFlashOverlay;
    public float flashDuration = 0.2f;
    private Vector2 startPosition;
    public Image canvasImage;
    

    public void BuildActor()
    {
        if (character != null)
        {
            Debug.Log("Building Character");
            canvasImage.sprite = character.RoomSprite;
            projectilePrefab = character.RangedPrefab;
            BattleSprite = character.BattleSprite;
            HurtSprite = character.HurtSprite;
            AttackSprite = character.AttackSprite;
            DeadSprite = character.DeadSprite;
            isRanged = character.isRanged;
            originalSprite = character.RoomSprite;
        }
        else { Debug.Log("no character"); }

            actor = GetComponent<RectTransform>();

        if (actor == null) return;

        startPosition = actor.anchoredPosition;
        SetSprite(originalSprite);
        // pull the character or monster, apply the sprites etc from it so we can instantiate this
    }

    void SetSprite(Sprite newSprite)
    {
        Debug.Log("New sprite " + newSprite.name);
        if (canvasImage == null || newSprite == null) { Debug.Log("no canvas image"); return; }
        canvasImage.sprite = newSprite;
        SpriteSizing(newSprite);
    }

    public void SpawnChipDamage(int count)
    {
        StartCoroutine(SpawnChipsWithDelay(count));
    }

    private IEnumerator SpawnChipsWithDelay(int count)
    {
        SetSprite(HurtSprite);
        for (int i = 0; i < count; i++)
        {
            SpawnAndMoveChip();
            yield return new WaitForSeconds(spawnDelay);
        }
        SetSprite(originalSprite);
    }

    private void SpawnAndMoveChip()
    {
        if (spritePrefabs.Length == 0 || parentTransform == null) return;

        GameObject selectedPrefab = spritePrefabs[Random.Range(0, spritePrefabs.Length)];
        GameObject newSprite = Instantiate(selectedPrefab, parentTransform);

        // Set the sprite's position to the center of the parent
        RectTransform rectTransform = newSprite.GetComponent<RectTransform>();
        if (rectTransform == null) rectTransform = newSprite.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        // Ensure the new sprite appears **behind** the parent
        newSprite.transform.SetSiblingIndex(0); // Moves it to the back of the hierarchy

        // Choose a random outward direction
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // Move outward using UI-friendly movement
        newSprite.AddComponent<SpriteMover>().Initialize(randomDirection, speed);
        // Apply spinning effect
        newSprite.AddComponent<Rotator>().rotationSpeed = rotationSpeed;
        Destroy(newSprite, Random.Range(0.3f, 0.9f));
    }

    public void StartLeap(int targetPos)
    {
        if (actor != null && !isRanged)
        {
            StartCoroutine(LeapRoutine(targetPos));
        }
        if (actor != null && isRanged)
        {
            StartCoroutine(RangeRoutine(targetPos));
        }
    }

    private IEnumerator RangeRoutine(int targetPos)
    {
        SetSprite(BattleSprite);
        yield return new WaitForSeconds(0.7f);
        SetSprite(AttackSprite);
        // Instantiate projectile
        if (projectilePrefab != null && targetPos < targetPosition.Count)
        {
            GameObject projectile = Instantiate(projectilePrefab, parentTransform);
            RectTransform projectileTransform = projectile.GetComponent<RectTransform>();
            if (projectileTransform == null) projectileTransform = projectile.AddComponent<RectTransform>();

            projectileTransform.anchoredPosition = Vector2.zero;
            Vector2 targetPosVec = targetPosition[targetPos];
            float travelTime = 0.6f; // Adjust as needed
            float elapsedTime = 0f;

            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / travelTime;
                projectileTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, targetPosVec, t);
                yield return null;

            }
            // Flash screen white on impact
            if (screenFlashOverlay != null)
            {
                StartCoroutine(FlashScreen());
            }

            // Destroy projectile after impact
            Destroy(projectile);
        }
        yield return new WaitForSeconds(0.35f);

        SetSprite(originalSprite);
    }

    private IEnumerator LeapRoutine(int targetPos)
    {
        SetSprite(BattleSprite);

        float elapsedTime = 0f;

        while (elapsedTime < leapDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / leapDuration;

            // Calculate an arc using a parabolic function
            float heightOffset = arcHeight * Mathf.Sin(t * Mathf.PI);
            Vector2 interpolatedPosition = Vector2.Lerp(startPosition, targetPosition[targetPos], t) + new Vector2(0, heightOffset);
            actor.anchoredPosition = interpolatedPosition;

            // Switch sprite midway
            if (t >= 0.55f && canvasImage != null && canvasImage.sprite != AttackSprite)
            {
                SetSprite(AttackSprite);
            }

            yield return null;
        }
        // FLASH SCREEN when it reaches the destination
        if (screenFlashOverlay != null)
        {
            StartCoroutine(FlashScreen());
        }
        //send chip damage to target

        // Reverse the leap back to the start position
        elapsedTime = 0f;
        while (elapsedTime < leapDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / leapDuration;
            float heightOffset = arcHeight * Mathf.Sin((1 - t) * Mathf.PI);
            Vector2 interpolatedPosition = Vector2.Lerp(targetPosition[targetPos], startPosition, t) + new Vector2(0, heightOffset);
            actor.anchoredPosition = interpolatedPosition;
            if (t >= 0.35f && canvasImage != null && canvasImage.sprite != BattleSprite)
            {
                SetSprite(BattleSprite);
            }
            yield return null;
        }
        SetSprite(originalSprite);
    }
    private IEnumerator FlashScreen()
    {
        float elapsedTime = 0f;
        Color originalColor = screenFlashOverlay.color; // Store original color
        Color flashColor = new Color(0, 0, 0, 0); // black screen flash

        // Fade in
        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            screenFlashOverlay.color = Color.Lerp(originalColor, flashColor, elapsedTime / (flashDuration / 2));
            yield return null;
        }

        elapsedTime = 0f;

        // Fade out
        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            screenFlashOverlay.color = Color.Lerp(flashColor, originalColor, elapsedTime / (flashDuration / 2));
            yield return null;
        }

        // Ensure it returns to the exact original color
        screenFlashOverlay.color = originalColor;
    }

    public void Defeated()
    {
        StartCoroutine(DefeatedRoutine());
    }

    private IEnumerator DefeatedRoutine()
    {
        SetSprite(HurtSprite);
        Color originalColor = screenFlashOverlay.color; // Store original color

        // Flash screen red twice
        for (int i = 0; i < 2; i++)
        {
            screenFlashOverlay.color = new Color(1, 0, 0, 0.5f); // Semi-transparent red
            yield return new WaitForSeconds(flashDuration);
            screenFlashOverlay.color = new Color(1, 0, 0, 0f); // Transparent
            yield return new WaitForSeconds(flashDuration);
        }

        SetSprite(DeadSprite);
        screenFlashOverlay.color = originalColor;

    }

    void SpriteSizing(Sprite changeSprite)
    {
        if (changeSprite != null)
        {
            RectTransform rectTransform = canvasImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (changeSprite == originalSprite)
                {
                    rectTransform.sizeDelta = new Vector2(75f, 75f);
                }
                else
                {
                    rectTransform.sizeDelta = new Vector2(changeSprite.texture.width, changeSprite.texture.height);
                }
            }

        }
    }



}

// Handles movement of the sprite
public class SpriteMover : MonoBehaviour
{
    private Vector2 direction;
    private float speed;

    public void Initialize(Vector2 moveDirection, float moveSpeed)
    {
        direction = moveDirection;
        speed = moveSpeed;
    }

    void Update()
    {
        GetComponent<RectTransform>().anchoredPosition += direction * speed * Time.deltaTime;
    }
}

// Rotator component to handle spinning
public class Rotator : MonoBehaviour
{
    public float rotationSpeed;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}