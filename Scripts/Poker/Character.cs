using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZeekSpace
{
    [CreateAssetMenu(fileName = "New Character", menuName = "Character")]
    public class Character : ScriptableObject
    {
        public string characterName;
        public Sprite HUDSprite;
        public GameObject battleSpritePrefab;
        public Sprite RoomSprite;
        public Sprite Silloette;
        public Sprite HandBottom;
        public Sprite HandTop;
        public Sprite BattleSprite;
        public Sprite AttackSprite;
        public Sprite HurtSprite;
        public Sprite DeadSprite;
        public GameObject RangedPrefab;
        public bool isOut;
        public bool isFolding;
        public bool isAllIn;
        public bool isBetting;
        public bool isRanged;
        public bool wonHand;
        public bool lostShowdown;
        public Weapon weapon;
        public Armor armor;
        public Trinket trinket;
        public PlayerSuit playerSuit;
        public PlayerRank playerRank;



        public enum Weapon
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

        public enum Armor
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

        public enum Trinket
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

        public enum PlayerRank
        {
            Woe,
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
            King,
            Ace,
        }

        public enum PlayerSuit
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

        
    }


}