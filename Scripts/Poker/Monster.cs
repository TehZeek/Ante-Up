using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZeekSpace
{
    [CreateAssetMenu(fileName = "New Monster", menuName = "Monster")]
    public class Monster : ScriptableObject
    {
        public string monsterName;
        public int monsterChips;
        public Character character;
        public HandTypes minimumHand;
        public int minimumRank;
        public int pocketSize;
        public int monsterTier;
        public BetPower betPower;
        public MonsterAI monsterAI;
        public YouFoldPower youFoldPower;
        public IFoldPower iFoldPower;
        public PocketPower pocketPower;
        public FlopPower flopPower;
        public TurnPower turnPower;
        public RiverPower riverPower;
        public TurnRound turnRound;
        public AllIn allIn;
        public PassivePower passivePower;
        public string scared;
        public string confident;
        public string escape;
        public string steal;
        public string challenge;
        public bool willRun = false;



        public enum MonsterAI
        {
            Scared,
            Brave,
            Bluffs,
            Neutral,
            Random
        }

        public enum BetPower
        {
            None,
            RemoveTraps,
            DenyCard,
            ReclaimCard,
            RemoveToken,
            BigBet,
            DestroyCard,
            ReplaceCard,
            DisinfranchiseToken,
            RankDownCard,
            AddDanger,
            StealChip
        }

        public enum YouFoldPower
        {
            None,
            MinusOnePocket,
            AddAWoundCard,
            AddADanger,
            DenyCard,
            RemoveToken,
            ReclaimCard
        }

        public enum IFoldPower
        {
            None,
            PlusOnePocket,
            FleeWithPot,
            BurnAllCards,
            KeepPocket,
            StealChip
        }

        public enum PocketPower
        {
            None,
            ChargeUpAttack,
            RedrawPocket,
            RankDownToken,
            DisinfranchiseToken,
            BigAnte
        }

        public enum FlopPower
        {
            None,
            DenyCard,
            ReplaceCard,
            DisinfranchiseToken,
            RankDowntoken,
            ShufflePockets,
            DangerToken
        }
        public enum TurnPower
        {
            None,
            DenyCard,
            ReplaceCard,
            DisinfranchiseToken,
            RankDowntoken,
            ShufflePockets,
            ReclaimCard
        }
        public enum RiverPower
        {
            None,
            DenyCard,
            ReplaceCard,
            DisinfranchiseToken,
            RankDowntoken,
            ShufflePockets,
            ReclaimCard,
            DangerToken
        }
        public enum TurnRound
        {
            None,
            TipTheWaiter,
            BiggestCard,
            LongestBoy,
            BiggestAlliance,
            RegularPoker,
            GoosesWild,
            OnFire,
            BlindPocket,
            OopsAllPairs,
            GoFish,
            WorstHand,
            TensWild,
            HiOrLow,
            ExtraRound
        }
        public enum AllIn
        {
            None,
            DestroyAllWoundsDealDamage,
            RemoveAllTokens,
            DestroyCardFromDeck
        }

        public enum PassivePower
        {
            None,
            OnlyHurtByFaceCards,
            NotHurtByEvens,
            BlindPocket,
            BigAnte
        }

    }
}