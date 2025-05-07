using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZeekSpace
{
    [CreateAssetMenu(fileName = "New Hand", menuName = "Hand")]
    public class HandTypes : ScriptableObject
    {
        public int requiresCards;
        public HandRank handRank;
        public bool requiresFlush;
        public bool requiresStraight;
        public bool requiresPair;

        public enum HandRank
        {
            Woe,
            AceHigh,
            Pair,
            TwoPair,
            ThreeOfAKind,
            Straight,
            Flush,
            FullHouse,
            FourOfAKind,
            StraightFlush,
            RoyalFlush,
            FiveOfAKind,
            FlushHouse,
            FlushFives,
            HatTrick,
            LongBoy,
            PlungerFlush,
            DomesticSixPack,
            FullerHouse,
            SixOfAKind,
            WellDressedLongBoy,
            RoyalLine,
            ImportedSixPack,
            FlusherFullerHouse,
            SixyFlush,
            SevenOfAKind
                
        }


    }


}