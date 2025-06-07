using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using ZeekSpace;

public class BGCharacterActions : MonoBehaviour
{
    [Header("Character Setup")]
    public Character character;                     // Your ScriptableObject or data class for sprites, etc.
    public int characterIndex;                      // Index into the static pose list
    public Image characterImage;                    // Assign in Inspector: the UI Image component to swap sprites

    [Header("Boundary Rect (for hopping)")]
    public RectTransform boundaryRect;              // Assign in Inspector: the UI container rectangle

    [Header("Hop Parameters")]
    public float maxXOffset = 20f;                  // ±X offset for “away” hop
    public float maxYOffset = 10f;                  // ±Y offset for “away” hop
    public float hopDurationMin = 0.8f;             // Min time for each hop
    public float hopDurationMax = 1.5f;             // Max time for each hop

    // These will be filled by PokerActorManager at runtime:
    [HideInInspector]
    public Transform[] potentialTargets;            // “Who this actor can attack”

    // (Optional) a little delay before an actor can act again:
    [HideInInspector]
    public float actionCooldownMin = 2f;
    [HideInInspector]
    public float actionCooldownMax = 5f;

    // Sprites for each state (Idle, Attacking, etc.). This static list is built at Start():
    public static List<CharacterPoseSet> characterPose = new List<CharacterPoseSet>();

    [System.Serializable]
    public class CharacterPoseSet
    {
        public Sprite idle;
        public Sprite attack;
        public Sprite attackEnd;
        public Sprite hurt;
        public Sprite defend;
        public Sprite outPose;
    }

    private Vector3 originalPosition;
    private Rect boundaryRectLocal;
    private RectTransform myRectTransform;
    private Vector3[] worldCorners = new Vector3[4];

    public enum ActorState
    {
        Idle,
        Attacking,
        AttackEnd,
        Hurt,
        Folding,
        KnockedOut
    }

    private ActorState currentState = ActorState.Idle;
    public ActorState CurrentState => currentState;

    public bool isOut = false;                      // true once this actor is knocked out

    public enum AttackType { Melee, Ranged }
    public AttackType attackType = AttackType.Melee;
    [HideInInspector]
    public GameObject projectilePrefab;             // For ranged attacks
    [HideInInspector]
    public Transform firePoint;                     // Where to spawn the projectile

    private bool isHopping = false;
    private bool isTakingAction = false;            // True while performing hop or attack

    private void Start()
    {
        // Cache original position & RectTransform
        originalPosition = transform.localPosition;
        myRectTransform = GetComponent<RectTransform>();

        // Build this actor’s pose‐set (once per characterIndex)
        BuildPoseSet();

        // Choose Idle sprite on startup
        SetSpriteForState(ActorState.Idle);

        // Compute boundary in local space
        if (boundaryRect != null)
            boundaryRect.GetWorldCorners(worldCorners);
        UpdateBoundaryRectLocal();

        // Determine ranged vs melee
        attackType = character.isRanged ? AttackType.Ranged : AttackType.Melee;
        projectilePrefab = character.RangedPrefab; // assign from Character asset
        firePoint = transform.Find("FirePoint");   // or assign in Inspector if you prefer
    }

    /// <summary>
    /// Called by PokerActorManager immediately after instantiating/spawning this actor.
    /// </summary>
    public void setCharacter()
    {
        // In case this was spawned at runtime:
        if (boundaryRect != null)
            boundaryRect.GetWorldCorners(worldCorners);
        UpdateBoundaryRectLocal();
        SetSpriteForState(ActorState.Idle);
    }

    /// <summary>
    /// Build the static pose-set list at index = characterIndex.
    /// </summary>
    private void BuildPoseSet()
    {
        var poseSet = new CharacterPoseSet
        {
            idle = character.RoomSprite,
            attack = character.BattleSprite,
            attackEnd = character.AttackSprite,
            hurt = character.HurtSprite,
            defend = character.DefendSprite,
            outPose = character.DeadSprite
        };

        while (characterPose.Count <= characterIndex)
            characterPose.Add(null);

        characterPose[characterIndex] = poseSet;
    }

    /// <summary>
    /// Recompute the local‐space boundary rectangle each time before clamping.
    /// </summary>
    private void UpdateBoundaryRectLocal()
    {
        if (boundaryRect == null || myRectTransform == null) return;
        boundaryRect.GetWorldCorners(worldCorners);
        Vector3 localBL = myRectTransform.parent.InverseTransformPoint(worldCorners[0]);
        Vector3 localTR = myRectTransform.parent.InverseTransformPoint(worldCorners[2]);
        boundaryRectLocal = new Rect(localBL, localTR - localBL);
    }

    /// <summary>
    /// Public method manager can call: hop toward a random target then hop away.
    /// </summary>
    public IEnumerator HopTowardThenAway()
    {
        isHopping = true;

        // STEP 1: Hop TOWARD chosen target
        if (potentialTargets != null && potentialTargets.Length > 0)
        {
            // Pick a random target
            Transform chosen = potentialTargets[Random.Range(0, potentialTargets.Length)];
            if (chosen != null && !isOut)
            {
                Vector3 currentPos = transform.localPosition;
                Vector3 targetLocal = chosen.localPosition;

                // Move halfway toward the target (0.5f). Change to 1f to go all the way.
                Vector3 approachLocal = Vector3.Lerp(currentPos, targetLocal, 0.5f);

                // Clamp inside boundary
                UpdateBoundaryRectLocal();
                approachLocal.x = Mathf.Clamp(approachLocal.x,
                                              boundaryRectLocal.xMin,
                                              boundaryRectLocal.xMax);
                approachLocal.y = Mathf.Clamp(approachLocal.y,
                                              boundaryRectLocal.yMin,
                                              boundaryRectLocal.yMax);

                float durationToward = Random.Range(hopDurationMin, hopDurationMax);
                LeanTween.moveLocal(gameObject, approachLocal, durationToward)
                         .setEase(LeanTweenType.easeOutQuad);

                yield return new WaitForSeconds(durationToward);
            }
        }

        // STEP 2: Hop AWAY in a random direction from the current position
        {
            Vector3 currentAfterApproach = transform.localPosition;
            float xOff = Random.Range(-maxXOffset, maxXOffset);
            float yOff = Random.Range(-maxYOffset, maxYOffset);
            Vector3 awayTarget = currentAfterApproach + new Vector3(xOff, yOff, 0f);

            // Clamp inside boundary again
            UpdateBoundaryRectLocal();
            awayTarget.x = Mathf.Clamp(awayTarget.x,
                                       boundaryRectLocal.xMin,
                                       boundaryRectLocal.xMax);
            awayTarget.y = Mathf.Clamp(awayTarget.y,
                                       boundaryRectLocal.yMin,
                                       boundaryRectLocal.yMax);

            float durationAway = Random.Range(hopDurationMin, hopDurationMax);
            LeanTween.moveLocal(gameObject, awayTarget, durationAway)
                     .setEase(LeanTweenType.easeInOutSine);

            yield return new WaitForSeconds(durationAway);
        }

        isHopping = false;
    }

    /// <summary>
    /// Public method manager can call: perform attack on a given target.
    /// </summary>
    public IEnumerator PerformAttack(Transform target)
    {
        if (isOut || target == null) yield break;

        // 1) Switch to Attacking sprite
        currentState = ActorState.Attacking;
        SetSpriteForState(ActorState.Attacking);
        yield return new WaitForSeconds(0.5f);

        // 2) Melee “hop‐and‐return” or Ranged fire
        if (attackType == AttackType.Melee)
        {
            // Hop out and back
            Vector3 original = transform.localPosition;
            Vector3 attackPos = Vector3.Lerp(original, target.localPosition, 0.6f);

            LeanTween.moveLocal(gameObject, attackPos, 0.4f)
                     .setEase(LeanTweenType.easeOutQuad);
            yield return new WaitForSeconds(0.2f);

            currentState = ActorState.AttackEnd;
            SetSpriteForState(ActorState.AttackEnd);
            yield return new WaitForSeconds(0.2f);

            LeanTween.moveLocal(gameObject, original, 0.4f)
                     .setEase(LeanTweenType.easeInQuad);
            yield return new WaitForSeconds(0.2f);
        }
        else // Ranged
        {
            yield return new WaitForSeconds(0.3f);
            FireProjectile(target);
            yield return new WaitForSeconds(0.1f);

            currentState = ActorState.AttackEnd;
            SetSpriteForState(ActorState.AttackEnd);
            yield return new WaitForSeconds(0.5f);
        }

        // 3) Return to Idle sprite
        currentState = ActorState.Idle;
        SetSpriteForState(ActorState.Idle);
    }

    /// <summary>
    /// Public method: spawn a projectile toward target.
    /// </summary>
    public void FireProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null || isOut) return;
        GameObject proj = Instantiate(projectilePrefab,
                                      firePoint.position,
                                      firePoint.rotation,
                                      firePoint.parent);
        LeanTween.move(proj, target.position, 0.6f)
                 .setEase(LeanTweenType.easeOutQuad)
                 .setOnComplete(() => Destroy(proj));
    }

    /// <summary>
    /// Public method: switch to Hurt sprite for a moment.
    /// </summary>
    public void TriggerHurt()
    {
        if (isOut) return;
        StartCoroutine(HurtCoroutine());
    }

    private IEnumerator HurtCoroutine()
    {
        currentState = ActorState.Hurt;
        SetSpriteForState(ActorState.Hurt);
        yield return new WaitForSeconds(0.5f);

        if (!isOut)
        {
            currentState = ActorState.Idle;
            SetSpriteForState(ActorState.Idle);
        }
    }

    /// <summary>
    /// Public method: switch to Folding pose (no movement).
    /// </summary>
    public void EnterDefensivePose()
    {
        if (currentState == ActorState.KnockedOut) return;
        currentState = ActorState.Folding;
        SetSpriteForState(ActorState.Folding);
    }

    /// <summary>
    /// Public method: mark this actor as “knocked out” and stop all actions.
    /// </summary>
    public void SetOutState()
    {
        if (isOut) return;
        isOut = true;
        currentState = ActorState.KnockedOut;
        SetSpriteForState(ActorState.KnockedOut);

        StopAllCoroutines();
        LeanTween.cancel(gameObject);
    }

    /// <summary>
    /// Change the UI Image’s sprite based on the current ActorState.
    /// </summary>
    private void SetSpriteForState(ActorState state)
    {
        if (characterImage == null || characterIndex >= characterPose.Count) return;
        Sprite newSprite = null;
        var poses = characterPose[characterIndex];

        switch (state)
        {
            case ActorState.Idle: newSprite = poses.idle; break;
            case ActorState.Attacking: newSprite = poses.attack; break;
            case ActorState.AttackEnd: newSprite = poses.attackEnd; break;
            case ActorState.Hurt: newSprite = poses.hurt; break;
            case ActorState.Folding: newSprite = poses.defend; break;
            case ActorState.KnockedOut: newSprite = poses.outPose; break;
        }

        if (newSprite != null)
        {
            characterImage.sprite = newSprite;
            characterImage.SetNativeSize();
        }
    }
}
