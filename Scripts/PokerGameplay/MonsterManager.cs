using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ZeekSpace;

public class MonsterManager : MonoBehaviour
{
    private int bluffRange, foldRange, callRange, betRange;
    public Monster monster;

    private PokerChipManager chipManager;
    private BattleMenu battleMenu;
    private PokerTurnManager pokerTurnManager;
    private PokerHandCompare handCompare;
    private PokerTableCards tableCards;
    public HandTypes Flush;
    public HandTypes Straight;
    public HandTypes StraightFlush;
    public HandTypes RoyalFlush;

    public enum MonsterChoice { Bet, Call, Check, Fold, Flee }

    public void SetUpMonster()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        chipManager = FindFirstObjectByType<PokerChipManager>();
        battleMenu = FindFirstObjectByType<BattleMenu>();
        pokerTurnManager = FindFirstObjectByType<PokerTurnManager>();
        handCompare = FindFirstObjectByType<PokerHandCompare>();
        tableCards = FindFirstObjectByType<PokerTableCards>();

        monster = gameManager.monster;
        int aiRank = (int)monster.monsterAI;

        (bluffRange, foldRange, callRange, betRange) = aiRank switch
        {
            0 => (2, 30, 45, 70),  // scared
            1 => (4, 15, 35, 50),  // brave
            2 => (5, 30, 50, 75),  // bluffer
            _ => (3, 20, 40, 60)   // neutral
        };
    }

    public int MonsterDecision()
    {
        if (pokerTurnManager.isShowdownInProgress)
        {
            Debug.Log("MonsterDecision skipped: showdown in progress.");
            return (int)MonsterChoice.Check;
        }
        int score = EvaluateHandScore();

        int potSize = chipManager.potChips;
        int callAmount = Mathf.Max(0, chipManager.BetSize - chipManager.InThePot[0]);

        // Encourage big pots, penalize expensive calls
        score += Mathf.FloorToInt(potSize * 0.2f);
        score -= Mathf.FloorToInt(callAmount * 0.5f);

        Debug.Log($"Monster Hand Score (adjusted): {score}, Call Amount: {callAmount}");

        if (callAmount == 0)
        {
            if (score >= betRange)
            {
                Debug.Log("Monster chooses to Bet (strong hand, no penalty to stay in).");
                return (int)MonsterChoice.Bet;
            }
            else
            {
                Debug.Log("Monster checks (no call required).");
                return (int)MonsterChoice.Check;
            }
        }

        if (score < foldRange && battleMenu.BetIsSet)
            return TryBluffOr(MonsterChoice.Fold);

        if (score < callRange)
            return TryBluffOr(MonsterChoice.Check); // It's a weak hand, might bluff

        if (score > callRange && battleMenu.BetIsSet)
            return (int)MonsterChoice.Call;

        if (score > betRange)
            return (int)MonsterChoice.Bet;

        return (int)MonsterChoice.Fold;
    }

    private int TryBluffOr(MonsterChoice fallback)
    {
        int bluffRoll = Random.Range(1, 10);
        if (bluffRoll < bluffRange)
        {
            Debug.Log($"Monster is bluffing with roll {bluffRoll}");
            return (int)MonsterChoice.Bet;
        }
        return (int)fallback;
    }

    private int EvaluateHandScore()
    {
        var (handType, handRank) = GetMonsterHandInfo();
        int baseScore = ((handType - 1) * 15) + handRank;
        int bonus = GetBonusFromHandPotential();
        int penalty = GetTableThreatLevel();
        int roundFactor = RoundMultiplier() * 5;

        Debug.Log($"Base: {baseScore}, Bonus: {bonus}, Penalty: {penalty}, RoundFactor: {roundFactor}");

        return baseScore + bonus + roundFactor + penalty;
    }

    private (int, int) GetMonsterHandInfo()
    {
        int minType = handCompare.allHandTypes.IndexOf(monster.minimumHand);
        int curType = handCompare.allHandTypes.IndexOf(handCompare.MonHand);
        int type = Mathf.Max(minType, curType);
        int rank;
        if (type == minType && type != curType)
        {
            rank = monster.minimumRank;
        }
        else if (type != minType && type == curType)
        {
            rank = (handCompare.monRank.Count > 0) ? handCompare.monRank[0] : 0;
        }
        else
        {
            int monMin = monster.minimumRank;
            int monHas = (handCompare.monRank.Count > 0) ? handCompare.monRank[0] : 0;
            rank = Mathf.Max(monMin, monHas);
        }
            return (type, rank);
    }


    private int RoundMultiplier()
    {
        int rounds = 3;
        if (pokerTurnManager.HasABonusRound) rounds++;
        if (pokerTurnManager.HasARiver) rounds++;

        return Mathf.Max(0, rounds - pokerTurnManager.turnOrder[1]);
    }

    private int GetTableThreatLevel()
    {
        List<Card> tableOnly = tableCards.tableHand
            .Select(obj => obj.GetComponent<CardDisplay>().cardData)
            .ToList();

        int penalty = EvaluateGroups(tableOnly, penalty: true);

        bool flushThreat = tableOnly
            .GroupBy(c => c.cardSuit.First())
            .Any(g => g.Count() >= 3);          // three to a flush

        bool straightThreat = IsStraightPossible(tableOnly);

        if (flushThreat && MonsterHasFlush())
        {
            penalty = MonsterFlushIsNut()
                ? 0                                      // ❌ no fear, nut flush
                : Mathf.RoundToInt(penalty * 0.5f);      // 😌 half fear, has flush
        }

        if (straightThreat && MonsterHasStraight())
        {
            penalty = MonsterStraightIsNut()
                ? 0
                : Mathf.RoundToInt(penalty * 0.5f);
        }

        return penalty * (RoundMultiplier() + 2);
    }

    private int GetBonusFromHandPotential()
    {
        List<Card> hand = tableCards.monsterPocket
            .Concat(tableCards.tableHand)
            .Select(obj => obj.GetComponent<CardDisplay>().cardData)
            .ToList();

        int bonus = EvaluateGroups(hand, penalty: false);
        return bonus * RoundMultiplier();
    }

    private int EvaluateGroups(List<Card> cards, bool penalty)
    {
        int points = 0;
        var groupsByRank = cards.GroupBy(c => c.cardRank.First()).ToList();
        var groupsBySuit = cards.GroupBy(c => c.cardSuit.First()).ToList();

        foreach (var group in groupsByRank)
        {
            points += group.Count() switch
            {
                2 => penalty ? -5 : 5,
                3 => penalty ? -15 : 15,
                4 => penalty ? -50 : 50,
                _ => 0
            };
        }

        foreach (var group in groupsBySuit)
        {
            points += group.Count() switch
            {
                3 => penalty ? -10 : 8,
                4 => penalty ? -25 : 20,
                5 => penalty ? -35 : 35,
                _ => penalty ? -1 : 1
            };
        }

        List<int> ranks = cards.Select(c => (int)c.cardRank.First()).Distinct().OrderBy(r => r).ToList();
        int straightBonus = 0, streak = 0;

        for (int i = 0; i < ranks.Count - 1; i++)
        {
            if (ranks[i + 1] - ranks[i] <= 2)
            {
                straightBonus += penalty ? -2 : 2;
                streak++;
            }
        }

        if (streak >= 3)
            straightBonus += penalty ? -10 : 5;
        if (streak >= 4)
            straightBonus += penalty ? -10 : 5;

        return points + straightBonus;
    }

    private bool MonsterHasFlush()
    {
        // Anything ≥ Flush counts (Flush, Straight-Flush, Royal…)
        return handCompare.MonHand == Flush
            || handCompare.MonHand == StraightFlush
            || handCompare.MonHand == RoyalFlush;
    }

    private bool MonsterHasStraight()
    {
        // Straight-Flush & Royal-Flush are also straights
        return handCompare.MonHand == Straight
            || handCompare.MonHand == StraightFlush
            || handCompare.MonHand == RoyalFlush;
    }

    private bool IsStraightPossible(List<Card> cards)
    {
        // A 'loose' straight check – four sequential ranks within gaps ≤1
        var ranks = cards
            .Select(c => (int)c.cardRank.First())
            .Distinct()
            .OrderBy(r => r)
            .ToList();

        int consecutive = 1;
        for (int i = 1; i < ranks.Count; i++)
        {
            consecutive = (ranks[i] - ranks[i - 1] <= 1) ? consecutive + 1 : 1;
            if (consecutive >= 4)        // ≥4 means the river could finish it
                return true;
        }
        return false;
    }

    private bool MonsterFlushIsNut()
    {
        return MonsterHasFlush() && handCompare.monRank[0] >= 13;
    }

    private bool MonsterStraightIsNut()
    {
        return MonsterHasStraight() && handCompare.monRank[0] >= 13;
    }

}