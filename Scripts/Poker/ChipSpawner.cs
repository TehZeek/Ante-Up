using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using TMPro;

public class SpriteSpawner : MonoBehaviour
{
    public GameObject[] spritePrefabs; // Assign multiple sprite prefabs in the Inspector
    public GameObject projectilePrefab;
    public float speed = 400f; // Adjust movement speed
    public float rotationSpeed = 360f; // Degrees per second
    public Transform parentTransform; // Assign a UI Panel or Canvas element in the Inspector
    public float spawnDelay = 0.05f; // Delay between spawns

    public RectTransform actor; // Assign the Canvas object in the Inspector
    public List<Vector2> targetPosition = new List<Vector2>(); // Set the destination position
    public Sprite BattleSprite; // Assign the sprite to switch to midway
    public Sprite HurtSprite;
    public Sprite AttackSprite;
    public Sprite DeadSprite;
    public float leapDuration = 0.7f; // Total time for the leap
    public float arcHeight = 50f; // Height of the arc
    public Image screenFlashOverlay; // Assign a full-screen UI Image in the Inspector
    public float flashDuration = 0.2f; // Duration of the screen flash


    private Vector2 startPosition;
    private Image canvasImage;
    private Sprite originalSprite;
    public bool isRanged = false;

    void Start()
    {
        if (actor == null) return;

        startPosition = actor.anchoredPosition;
        canvasImage = actor.GetComponent<Image>();
        if (canvasImage != null) originalSprite = canvasImage.sprite;
        // pull the character or monster, apply the sprites etc from it so we can instantiate this
    }

    public void SpawnChipDamage(int count)
    {
        StartCoroutine(SpawnChipsWithDelay(count));
    }

    private IEnumerator SpawnChipsWithDelay(int count)
    {
        canvasImage.sprite = HurtSprite;
        SpriteSizing(HurtSprite);

        for (int i = 0; i < count; i++)
        {
            SpawnAndMoveChip();
            yield return new WaitForSeconds(spawnDelay);
        }
        canvasImage.sprite = originalSprite;
        SpriteSizing(originalSprite);
    }

    private void SpawnAndMoveChip()
    {
        if (spritePrefabs.Length == 0 || parentTransform == null) return;

        // Pick a random sprite prefab
        GameObject selectedPrefab = spritePrefabs[Random.Range(0, spritePrefabs.Length)];

        // Instantiate the sprite as a child of the specified parent
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

        // Destroy after a random time between 0.3 and 0.9 seconds
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
        // Change sprite to BattleSprite
        canvasImage.sprite = BattleSprite;
        SpriteSizing(BattleSprite);
        yield return new WaitForSeconds(0.7f);
        canvasImage.sprite = AttackSprite;
        SpriteSizing(AttackSprite);
        // Instantiate projectile
        if (projectilePrefab != null && targetPos < targetPosition.Count)
        {
            GameObject projectile = Instantiate(projectilePrefab, parentTransform);
            RectTransform projectileTransform = projectile.GetComponent<RectTransform>();
            if (projectileTransform == null) projectileTransform = projectile.AddComponent<RectTransform>();

            // Set projectile to start at the center of parentTransform
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

        // Restore original sprite
        canvasImage.sprite = originalSprite;
        SpriteSizing(originalSprite);
    }

    private IEnumerator LeapRoutine(int targetPos)
    {
        canvasImage.sprite = BattleSprite;
        SpriteSizing(BattleSprite);

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
                canvasImage.sprite = AttackSprite;
                SpriteSizing(AttackSprite);
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
                canvasImage.sprite = BattleSprite;
                SpriteSizing(BattleSprite);
            }
            yield return null;
        }
        // Restore original sprite
        canvasImage.sprite = originalSprite;
        SpriteSizing(originalSprite);

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
        // Change sprite to HurtSprite
        canvasImage.sprite = HurtSprite;
        SpriteSizing(HurtSprite);

        Color originalColor = screenFlashOverlay.color; // Store original color

        // Flash screen red twice
        for (int i = 0; i < 2; i++)
        {
            screenFlashOverlay.color = new Color(1, 0, 0, 0.5f); // Semi-transparent red
            yield return new WaitForSeconds(flashDuration);
            screenFlashOverlay.color = new Color(1, 0, 0, 0f); // Transparent
            yield return new WaitForSeconds(flashDuration);
        }

        // Change sprite to DeadSprite and adjust size
        canvasImage.sprite = DeadSprite;

        // Adjust size based on new sprite dimensions
        SpriteSizing(DeadSprite);
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