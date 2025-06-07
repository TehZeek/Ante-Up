using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerActorManager : MonoBehaviour
{
    public List<BGCharacterActions> players = new List<BGCharacterActions>();
    public List<BGCharacterActions> enemies = new List<BGCharacterActions>(); // Support multiple enemies
    public float actionInterval = 4f;

    private WaitForSeconds waitInterval;
    public PokerTurnManager pokerTurnManager;
    public PokerChipManager pokerChipManager;

    private void Start()
    {
        waitInterval = new WaitForSeconds(actionInterval);
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();
        StartCoroutine(ActionLoop());
    }

    public void UpdateActorActivityBasedOnTurn()
    {
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        pokerChipManager = FindFirstObjectByType<PokerChipManager>();

        int currentTurn = pokerTurnManager.turnOrder[2]; // 0 = monster, 1–3 = players

        foreach (var actor in players)
        {
            if (actor == null) continue;

            int actorIndex = actor.characterIndex;
            bool isFolded = pokerTurnManager.IsOut[actorIndex] && !pokerTurnManager.isAllIn[actorIndex];
            bool isEliminated = !pokerTurnManager.isAllIn[actorIndex] && pokerChipManager.playerChips[actorIndex] <= 0;

            if (isEliminated)
            {
                actor.SetOutState();
                continue;
            }

            if (actorIndex == currentTurn)
            {
                actor.Pause(); // Actor is hidden behind UI
            }
            else
            {
                actor.Unpause();

                // Handle folding / idle state clearly
                if (isFolded)
                {
                    if (actor.CurrentState != BGCharacterActions.ActorState.Folding)
                    {
                        actor.EnterDefensivePose();
                    }
                }
                else
                {
                    // Only reset to Idle if not already performing something else
                    if (actor.CurrentState == BGCharacterActions.ActorState.Folding)
                    {
                        actor.SetState(BGCharacterActions.ActorState.Idle);
                    }
                }
            }

            SetTargeting(actor, currentTurn);
        }

        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.isOut)
            {
                enemy.Unpause();
            }
        }
    }

    private void SetTargeting(BGCharacterActions actor, int currentTurn)
    {
        if (currentTurn == 0)
        {
            actor.potentialTargets = null;
            return; // Monster idle mode
        }

        int actorIndex = actor.characterIndex;

        // All non-active players (background actors)
        bool isBackgroundActor = actorIndex != currentTurn;

        if (actorIndex == 0 && isBackgroundActor)
        {
            // Player 0 should target the other two background players
            List<Transform> validTargets = new();
            foreach (var a in players)
            {
                if (a != null && a.characterIndex != 0 && a.characterIndex != currentTurn && !a.isOut)
                    validTargets.Add(a.transform);
            }
            actor.potentialTargets = validTargets.ToArray();
        }
        else if (actorIndex != 0 && isBackgroundActor)
        {
            // All others target player 0
            var target = players.Find(p => p.characterIndex == 0 && !p.isOut);
            actor.potentialTargets = target != null ? new Transform[] { target.transform } : null;
        }
        else
        {
            // The active player doesn’t do anything in BG animation
            actor.potentialTargets = null;
        }
    }

    public void StartMonsterIdleMode()
    {
        int currentTurn = pokerTurnManager.turnOrder[2];

        if (currentTurn != 0) return; // Only do this during monster's turn

        foreach (var actor in players)
        {
            if (actor == null || actor.isOut) continue;

            actor.StopAllCoroutines();
            actor.StartCoroutine(actor.SlowIdleLoop());
        }
    }


    IEnumerator ActionLoop()
    {
        while (true)
        {
            yield return waitInterval;

            foreach (var actor in players)
            {
                if (actor == null || actor.isOut || actor.CurrentState != BGCharacterActions.ActorState.Idle)
                    continue;

                switch (Random.Range(0, 2))
                {
                    case 0:
                        StartCoroutine(AttackRandomEnemy(actor));
                        break;
                }
            }

            FacePlayersTowardMonster();
        }
    }

    IEnumerator AttackRandomEnemy(BGCharacterActions actor)
    {
        if (actor.potentialTargets == null || actor.potentialTargets.Length == 0)
            yield break;

        BGCharacterActions target = GetRandomValidTarget(actor.potentialTargets);

        if (actor == null || target == null || target.isOut)
            yield break;

        SetFacing(actor.transform, target.transform);
        actor.SetState(BGCharacterActions.ActorState.Attacking);

        yield return actor.PerformAttack();

        if (target.CurrentState == BGCharacterActions.ActorState.Idle && !target.isOut)
        {
            TriggerHurt(target);
        }

        yield return new WaitForSeconds(1f);

        if (!actor.isOut)
            actor.SetState(BGCharacterActions.ActorState.Idle);
    }

    public void KnockOut(BGCharacterActions actor)
    {
        if (actor != null)
            actor.SetOutState();
    }

    void TriggerHurt(BGCharacterActions actor)
    {
        actor?.TriggerHurt();
    }

    void SetFolded(BGCharacterActions actor)
    {
        actor?.EnterDefensivePose();
    }

    void FacePlayersTowardMonster()
    {
        int currentTurn = pokerTurnManager.turnOrder[2];
        if (currentTurn == 0) return;

        Transform monsterTransform = players[0].transform;

        foreach (var player in players)
        {
            if (player == null || player.isOut)
                continue;

            SetFacing(player.transform, monsterTransform);
        }
    }


    void SetFacing(Transform actor, Transform target)
    {
        if (actor == null || target == null) return;

        bool shouldFaceLeft = actor.position.x > target.position.x;
        Vector3 scale = actor.localScale;
        scale.x = Mathf.Abs(scale.x) * (shouldFaceLeft ? -1 : 1);
        actor.localScale = scale;
    }

    public BGCharacterActions GetRandomValidTarget(Transform[] potentialTargets)
{
    if (potentialTargets == null || potentialTargets.Length == 0) return null;

    List<BGCharacterActions> valid = new();
    foreach (Transform t in potentialTargets)
    {
        if (t == null) continue;
        BGCharacterActions bga = t.GetComponent<BGCharacterActions>();
        if (bga != null && !bga.isOut)
            valid.Add(bga);
    }

    return valid.Count > 0 ? valid[Random.Range(0, valid.Count)] : null;
}

    public void SetActors(List<BGCharacterActions> playerList, List<BGCharacterActions> enemyList)
    {
        players = playerList;
        enemies = enemyList;
    }
}