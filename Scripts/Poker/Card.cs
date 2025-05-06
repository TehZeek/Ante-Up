using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZeekSpace
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public List<CardSuit> cardSuit;
        public List<CardRank> cardRank;
        public Sprite suitSprite;
        public Sprite rankSprite;
        public Sprite faceSprite;
        public Sprite numberSprite;
        public NewRank newRank;
        public NewSuit newSuit;


        public enum CardSuit
        {
            Spade,
            Diamond,
            Heart,
            Club,
            Wizards,
            Woe,
            Wrath,
            Wolves,
            Wyverns,
            Warlords,
            Widows,
            Wildlings,
            Whispers,
            Wraiths
        }

        public enum CardRank
        {
            Woe,
            Ace,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King
        }

        public enum NewSuit
        {
            Blank,
            Woe,
            Ace,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King
        }

        public enum NewRank
        {
            Blank,
            Woe,
            Ace,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King
        }

    }


}