using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZeekSpace

{
    [CreateAssetMenu(fileName = "New MapTile", menuName = "MapTile")]
    public class MapCard : ScriptableObject
    {
        public string mapCardName;
        public MapRank mapRank;
        public List<MapExit> mapExit;
        public MapObject mapObject;
        public Sprite mapTierSprite;
        public Sprite mapExitSprite;
        public Sprite mapIconSprite;
        public Sprite tileSprite;
        public Sprite tileDoor;
        public Sprite tileIcon;
        public GameObject prefab;
        public MapCard mapCard;

        public enum MapRank
        {
            Tier1,
            Tier2,
            Tier3,
            Tier4,
            Dragon
        }

        public enum MapExit
        {
            Up,
            Down,
            Left,
            Right
        }

        public enum MapObject
        {
            Start,
            Clear,
            Encounter,
            Treasure,
            Chips,
            Trap,
            Special,
            Shop,
            Dragon,
            Burn,
            Smith,
            Obstacle
        }

    }



}
