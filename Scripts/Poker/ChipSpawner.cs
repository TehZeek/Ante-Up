using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using ZeekSpace;

public class SpriteSpawner : MonoBehaviour
{
    [Header("Actor Setup")]
    public Character character;
    public RectTransform actor;
    public Sprite BattleSprite;
    public Sprite HurtSprite;
    public Sprite DefendSprite;
    public Sprite AttackSprite;
    public Sprite DeadSprite;
    public Sprite originalSprite;
    public bool isRanged = false;

    [Header("Projectiles")]
    public GameObject[] spritePrefabs;
    public GameObject projectilePrefab;
    public float speed = 1800f;
    public float rotationSpeed = 360f;
    public Transform parentTransform;
    public float spawnDelay = 0.05f;
    public List<Vector2> targetPosition = new List<Vector2>();
    public List<RectTransform> chipTargets = new List<RectTransform>();
    private List<GameObject> spawnedSprites = new List<GameObject>();
    private Queue<GameObject> chipPool = new Queue<GameObject>();

    [Header("Movement")]
    public float leapDuration = 0.7f;
    public float arcHeight = 50f;
    public Image screenFlashOverlay;
    public float flashDuration = 0.2f;
    private Vector2 startPosition;
    public Image canvasImage;
    private ActionScreen actionScreen;

    public void BuildActor()
    {
        if (character != null)
        {
            actionScreen = FindFirstObjectByType<ActionScreen>();
            canvasImage.sprite = character.RoomSprite;
            projectilePrefab = character.RangedPrefab;
            BattleSprite = character.BattleSprite;
            HurtSprite = character.HurtSprite;
            AttackSprite = character.AttackSprite;
            DeadSprite = character.DeadSprite;
            isRanged = character.isRanged;
            originalSprite = character.RoomSprite;
            chipTargets = actionScreen.chipsTargets;
            DefendSprite = character.DefendSprite;
        }

        actor = GetComponent<RectTransform>();
        if (actor == null) return;

        startPosition = actor.anchoredPosition;
        SetSprite(originalSprite);
    }

    public void SetSprite(Sprite newSprite)
    {
        if (canvasImage == null || newSprite == null) return;
        canvasImage.sprite = newSprite;
        SpriteSizing(newSprite);
    }

    public void SpawnChipDamageTemp()
    {
        int count = 15;
        List<int> target = new List<int>() {0, 2};
        SpawnChipDamage(count, target);
    }

    public void SpawnChipDamage(int count, List<int> target)
    {
        StartCoroutine(SpawnChipsWithDelay(count, target));
    }

    private IEnumerator SpawnChipsWithDelay(int count, List<int> target)
    {
        List<int> chipsPerTarget = new List<int>() { 0, 0, 0, 0 };
        int targets = target.Count;
        int initialChips = count / targets;
        int extraChips = count - (initialChips * targets);
        for (int i = 0; i < target.Count; i++)
        {
            chipsPerTarget[target[i]] += initialChips;
        }
        int randomIndex = UnityEngine.Random.Range(0, target.Count);
        chipsPerTarget[target[randomIndex]] += extraChips;

        SetSprite(HurtSprite);
        for (int i = 0; i < target.Count; i++)
        {
            int targetIndex = target[i];
            for (int j = 0; j < chipsPerTarget[targetIndex]; j++)
            {
                GameObject newSprite = SpawnChip(targetIndex);
                spawnedSprites.Add(newSprite);
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        SetSprite(originalSprite);

       // MoveChipsByUserInput(chipsPerTarget);
    }

    private GameObject SpawnChip(int target)
    {
        if (spritePrefabs.Length == 0 || parentTransform == null) return null;

        GameObject chip;
        if (chipPool.Count > 0)
        {
            chip = chipPool.Dequeue();
            chip.SetActive(true);
        }
        else
        {
            GameObject selectedPrefab = spritePrefabs[UnityEngine.Random.Range(0, spritePrefabs.Length)];
            chip = Instantiate(selectedPrefab, parentTransform);
        }

        RectTransform rectTransform = chip.GetComponent<RectTransform>() ?? chip.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        chip.transform.SetSiblingIndex(0);

        Vector3 randomDirection = new Vector3(UnityEngine.Random.insideUnitCircle.x, UnityEngine.Random.insideUnitCircle.y, 0f).normalized;
        RectTransform theTarget = chipTargets[target];

        SpriteMover mover = chip.GetComponent<SpriteMover>() ?? chip.AddComponent<SpriteMover>();
        mover.Initialize(randomDirection, speed, ReturnChip, theTarget);

        Rotator rotator = chip.GetComponent<Rotator>() ?? chip.AddComponent<Rotator>();
        rotator.rotationSpeed = rotationSpeed;
        StartCoroutine(chipDisplayDelay(target));

        return chip;
    }
    private IEnumerator chipDisplayDelay(int player)
    {
        yield return new WaitForSeconds(2f);
        actionScreen.UpdateChipCounter(player);
    }

    private void ReturnChip(GameObject chip)
    {
        chip.SetActive(false);
        chipPool.Enqueue(chip);
    }

    public void StartLeap(int targetPos)
    {
        if (actor != null && !isRanged)
        {
            StartCoroutine(LeapRoutine(targetPos));
        }
        else if (actor != null && isRanged)
        {
            StartCoroutine(RangeRoutine(targetPos));
        }
    }

    private IEnumerator RangeRoutine(int targetPos)
    {
        SetSprite(BattleSprite);
        yield return new WaitForSeconds(0.7f);
        SetSprite(AttackSprite);

        if (projectilePrefab != null && targetPos < targetPosition.Count)
        {
            GameObject projectile = Instantiate(projectilePrefab, parentTransform);
            RectTransform projectileTransform = projectile.GetComponent<RectTransform>() ?? projectile.AddComponent<RectTransform>();
            projectileTransform.anchoredPosition = Vector2.zero;

            Vector2 targetPosVec = targetPosition[targetPos];
            float travelTime = 0.6f;
            float elapsedTime = 0f;

            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / travelTime;
                projectileTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, targetPosVec, t);
                yield return null;
            }

            if (screenFlashOverlay != null)
            {
                StartCoroutine(FlashScreen());
            }
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
            float heightOffset = arcHeight * Mathf.Sin(t * Mathf.PI);
            Vector2 interpolatedPosition = Vector2.Lerp(startPosition, targetPosition[targetPos], t) + new Vector2(0, heightOffset);
            actor.anchoredPosition = interpolatedPosition;

            if (t >= 0.55f && canvasImage != null && canvasImage.sprite != AttackSprite)
            {
                SetSprite(AttackSprite);
            }
            yield return null;
        }

        if (screenFlashOverlay != null)
        {
            StartCoroutine(FlashScreen());
        }

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
        Color originalColor = screenFlashOverlay.color;
        Color flashColor = new Color(0, 0, 0, 0);

        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            screenFlashOverlay.color = Color.Lerp(originalColor, flashColor, elapsedTime / (flashDuration / 2));
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < flashDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            screenFlashOverlay.color = Color.Lerp(flashColor, originalColor, elapsedTime / (flashDuration / 2));
            yield return null;
        }

        screenFlashOverlay.color = originalColor;
    }

    public void Defeated()
    {
        StartCoroutine(DefeatedRoutine());
    }

    private IEnumerator DefeatedRoutine()
    {
        SetSprite(HurtSprite);
        Color originalColor = screenFlashOverlay.color;

        for (int i = 0; i < 2; i++)
        {
            screenFlashOverlay.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(flashDuration);
            screenFlashOverlay.color = new Color(1, 0, 0, 0f);
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
                rectTransform.sizeDelta = (changeSprite == originalSprite)
                    ? new Vector2(originalSprite.texture.width, originalSprite.texture.height)
                    : new Vector2(changeSprite.texture.width, changeSprite.texture.height);
            }
        }
    }
}

public class SpriteMover : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private bool isMovingToTarget = false;
    private Action<GameObject> returnToPoolCallback;
    private float freeMovementTime = 0.75f; // First movement duration
    private RectTransform chipTarget;
    private bool hasReturned;

    public void Initialize(Vector3 moveDirection, float moveSpeed, Action<GameObject> onReturnToPool, RectTransform target)
    {
        direction = moveDirection;
        speed = moveSpeed*=1.5f;
        returnToPoolCallback = onReturnToPool;
        chipTarget = target;
        hasReturned = false;

        StartCoroutine(MoveSequence()); // Start movement logic
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (isMovingToTarget)
        {
            float distance = Vector3.Distance(transform.position, chipTarget.position);
            if (distance < 30f) // <-- Adjust this threshold to taste
            {
                ReturnToPoolOrDestroy();
            }
        }
    }

    private IEnumerator MoveSequence()
    {
        yield return new WaitForSeconds(freeMovementTime); // Move freely for 1 second
        isMovingToTarget = true;
        speed *= 1.5f;
        Vector3 worldTargetPos = chipTarget.position;  // world position of the target
        Vector3 worldCurrentPos = GetComponent<RectTransform>().position;
        direction = (worldTargetPos - worldCurrentPos).normalized;
        yield return new WaitForSeconds(freeMovementTime*2);
        ReturnToPoolOrDestroy();
    }

    private void ReturnToPoolOrDestroy()
    {
        if (hasReturned) return;
        hasReturned = true;

        isMovingToTarget = false;
        if (returnToPoolCallback != null)
        {
            returnToPoolCallback.Invoke(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


public class Rotator : MonoBehaviour
{
    public float rotationSpeed;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
