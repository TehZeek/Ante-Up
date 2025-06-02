using UnityEngine;
using System.Collections;
using ZeekSpace;
using System.Collections.Generic;
using UnityEngine.UI;
using static BGCharacterActions;

public class BGCharacterActions : MonoBehaviour
{
    public Character character;
    public RectTransform boundaryRect; // Assign in Inspector
    private Rect boundaryRectLocal;        // Will hold local-space rect
    private RectTransform myRectTransform; // This object's RectTransform
    private Vector3[] worldCorners = new Vector3[4];

    public float multiplier = 500f;
    public Transform[] potentialTargets;

    public int characterIndex; // used to access GameManager.character[index]
    public Image characterImage;

    private Vector3 originalPosition;
    private bool isHopping = false;
    private bool isPaused = false;

    public enum ActorState
    {
        Idle,
        Attacking,
        AttackEnd,
        Hurt,
        Folding,
        KnockedOut
    }

    public static List<CharacterPoseSet> characterPose = new List<CharacterPoseSet>();

    public class CharacterPoseSet
    {
        public Sprite idle;
        public Sprite attack;
        public Sprite attackEnd;
        public Sprite hurt;
        public Sprite defend;
        public Sprite outPose;
    }

    private ActorState currentState = ActorState.Idle;
    public ActorState CurrentState => currentState;

    public bool isOut = false;

    public enum AttackType { Melee, Ranged }
    public AttackType attackType = AttackType.Melee;

    public Transform firePoint;
    public GameObject projectilePrefab;
    private PokerTurnManager pokerTurnManager;
    private PokerChipManager pokerChipManager;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void setCharacter()
    {
         pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
         pokerChipManager = FindFirstObjectByType<PokerChipManager>();

        var poseSet = new CharacterPoseSet
        {
            idle = character.RoomSprite,
            attack = character.BattleSprite,
            attackEnd = character.AttackSprite,
            hurt = character.HurtSprite,
            defend = character.DefendSprite,
            outPose = character.DeadSprite,
        };

        while (characterPose.Count <= characterIndex)
        {
            characterPose.Add(null);
        }
        characterPose[characterIndex] = poseSet;

        projectilePrefab = character.RangedPrefab;
        if (character.isRanged)
            attackType = AttackType.Ranged;

        myRectTransform = GetComponent<RectTransform>();
        originalPosition = transform.localPosition;

        if (boundaryRect != null)
        {
            boundaryRect.GetWorldCorners(worldCorners);
        }
        UpdateBoundaryRectLocal();

        SetSpriteForState(ActorState.Idle);
        StartCoroutine(HopLoop());
        StartCoroutine(RandomBehaviorLoop());
    }

    void UpdateBoundaryRectLocal()
    {
        if (boundaryRect == null || myRectTransform == null) return;

        Vector3[] worldCorners = new Vector3[4];
        boundaryRect.GetWorldCorners(worldCorners);

        Vector3 localBL = myRectTransform.parent.InverseTransformPoint(worldCorners[0]);
        Vector3 localTR = myRectTransform.parent.InverseTransformPoint(worldCorners[2]);

        boundaryRectLocal = new Rect(localBL, localTR - localBL);
    }

    public void Pause()
    {
        isPaused = true;
        LeanTween.pause(gameObject);
        StopAllCoroutines();
    }

    public void Unpause()
    {
        if (isOut) return;
        isPaused = false;
        StartCoroutine(HopLoop());
        StartCoroutine(RandomBehaviorLoop());
    }

    public void SetState(ActorState newState)
    {
        currentState = newState;
        SetSpriteForState(newState);
    }

    private void SetSpriteForState(ActorState state)
    {
        if (characterImage == null || characterIndex >= 4) return;

        var poses = characterPose[characterIndex];
        switch (state)
        {
            case ActorState.Idle: characterImage.sprite = poses.idle; break;
            case ActorState.Attacking: characterImage.sprite = poses.attack; break;
            case ActorState.AttackEnd: characterImage.sprite = poses.attackEnd; break;
            case ActorState.Hurt: characterImage.sprite = poses.hurt; break;
            case ActorState.Folding: characterImage.sprite = poses.defend; break;
            case ActorState.KnockedOut: characterImage.sprite = poses.outPose; break;
        }
    }

    public IEnumerator PerformAttack()
    {
        if (potentialTargets == null || potentialTargets.Length == 0) yield break;

        Transform target = potentialTargets[Random.Range(0, potentialTargets.Length)];
        if (target == null) yield break;

        SetState(ActorState.Attacking);

        if (attackType == AttackType.Melee)
        {
            Debug.Log($"{name} performing melee attack.");
            yield return HopTowardAndAttack(target);
        }
        else if (attackType == AttackType.Ranged)
        {
            Debug.Log($"{name} performing ranged attack.");
            yield return new WaitForSeconds(0.3f);
            FireProjectile(target);
            yield return new WaitForSeconds(0.1f);
            SetState(ActorState.AttackEnd);
            yield return new WaitForSeconds(0.5f);
            SetState(ActorState.Idle);
        }

        yield return new WaitForSeconds(0.5f);

        if (!isOut && currentState != ActorState.KnockedOut)
            SetState(ActorState.Idle);
    }

    private IEnumerator HopTowardAndAttack(Transform target)
    {
        Vector3 original = transform.localPosition;
        Vector3 attackPos = Vector3.Lerp(original, target.localPosition, 0.6f);

        LeanTween.moveLocal(gameObject, attackPos, 0.4f).setEase(LeanTweenType.easeOutQuad);

        yield return new WaitForSeconds(0.2f);
        SetSpriteForState(ActorState.AttackEnd);

        yield return new WaitForSeconds(0.2f);
        yield return new WaitForSeconds(0.1f);

        LeanTween.moveLocal(gameObject, original, 0.4f).setEase(LeanTweenType.easeInQuad);

        yield return new WaitForSeconds(0.2f);
        SetSpriteForState(ActorState.Idle);
    }

    public void FireProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null || target == null)
        {
            Debug.LogWarning($"{name} cannot fire: Missing firePoint or projectile.");
            return;
        }
        Debug.Log($"{name} is firing projectile at {target.name}");

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation, firePoint.parent);
        proj.SetActive(true);


        if (proj == null)
        {
            Debug.LogError($"{name} tried to fire, but Instantiate returned null.");
        }
        else
        {
            Debug.Log($"{name} successfully instantiated a projectile at {firePoint.position}");
        }

        LeanTween.move(proj, target.position, 0.6f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => Destroy(proj));
    }

    IEnumerator HopLoop()
    {
        while (!isPaused)
        {
            float waitTime = Random.Range(2.5f, 6f);
            yield return new WaitForSeconds(waitTime);

            if (!isHopping && !isOut && currentState == ActorState.Idle)
            {
                RandomizeHop();
            }
        }
    }

    public void RandomizeHop()
    {
        if (myRectTransform == null) return;

        isHopping = true;

        float forwardDistance = Random.Range(-10f, 10f) * multiplier;
        float hopHeight = Random.Range(-2.5f, 10f) * multiplier;
        float hopTime = Random.Range(.25f, 1f);
        float returnTime = Random.Range(.4f, 1.5f);

        Vector3 hopTarget = originalPosition + new Vector3(forwardDistance, hopHeight, 0);
        UpdateBoundaryRectLocal();

        hopTarget.x = Mathf.Clamp(hopTarget.x, boundaryRectLocal.xMin, boundaryRectLocal.xMax);
        hopTarget.y = Mathf.Clamp(hopTarget.y, boundaryRectLocal.yMin, boundaryRectLocal.yMax);

        LeanTween.moveLocal(gameObject, hopTarget, hopTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
                LeanTween.moveLocal(gameObject, originalPosition, returnTime)
                    .setEase(LeanTweenType.easeInQuad)
                    .setOnComplete(() => isHopping = false)
            );
    }

    public IEnumerator SlowIdleLoop()
    {
        SetState(isOut ? ActorState.KnockedOut : ActorState.Attacking);

        while (true)
        {
            float wait = Random.Range(1.5f, 3.5f);
            yield return new WaitForSeconds(wait);

            if (isOut || currentState == ActorState.KnockedOut) yield break;

            SetState(currentState == ActorState.Folding ? ActorState.Folding : ActorState.Attacking);

            Vector3 original = transform.localPosition;
            float dx = Random.Range(-10f, 10f) * multiplier;
            float dy = Random.Range(-5f, 5f) * multiplier;

            Vector3 moveTarget = original + new Vector3(dx, dy, 0);
            UpdateBoundaryRectLocal();
            moveTarget.x = Mathf.Clamp(moveTarget.x, boundaryRectLocal.xMin, boundaryRectLocal.xMax);
            moveTarget.y = Mathf.Clamp(moveTarget.y, boundaryRectLocal.yMin, boundaryRectLocal.yMax);

            LeanTween.moveLocal(gameObject, moveTarget, 1.5f).setEase(LeanTweenType.easeInOutSine);
        }
    }

    public void SetOutState()
    {
        isOut = true;
        SetState(ActorState.KnockedOut);
        StopAllCoroutines();
        LeanTween.cancel(gameObject);
    }

    public void ResetOutState()
    {
        isOut = false;
        SetState(ActorState.Idle);
        StartCoroutine(HopLoop());
        StartCoroutine(RandomBehaviorLoop());
    }

    IEnumerator RandomBehaviorLoop()
    {
        while (true && !isPaused)
        {
            float delay = Random.Range(3f, 6f);
            yield return new WaitForSeconds(delay);

            if (currentState != ActorState.Idle || isOut) continue;

            int action = Random.Range(0, 3);

            switch (action)
            {
                case 0:
                    StartCoroutine(PerformAttack());
                    break;

                case 1:
                    TriggerHurt();
                    break;

                case 2:
                    //Only enter defensive pose if this actor is **actually folded**
                   if (pokerTurnManager != null && pokerTurnManager.IsOut[characterIndex] && !pokerTurnManager.isAllIn[characterIndex])
                    {
                       EnterDefensivePose();
                    }
                    break;
            }
        }
    }

    public void TriggerHurt()
    {
        if (currentState == ActorState.KnockedOut) return;
        SetState(ActorState.Hurt);
        StartCoroutine(ResetStateAfter(1f));
    }

    IEnumerator ResetStateAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!isOut && currentState != ActorState.KnockedOut)
            SetState(ActorState.Idle);
    }

    public void EnterDefensivePose()
    {
        if (currentState == ActorState.KnockedOut) return;
        SetState(ActorState.Folding);
    }
}
