using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DiscardManager : MonoBehaviour
{
    [SerializeField] public List<MapCard> discardMaps = new List<MapCard>();
    public TextMeshProUGUI discardCount;
    public int discardCardsCount;

    void Awake()
    {
        UpdateDiscardCount();
    }

    private void UpdateDiscardCount()
    {
        discardCount.text = discardMaps.Count.ToString();
        discardCardsCount = discardMaps.Count;
    }

    public void AddToDiscard(MapCard card)
    {
        if (card != null)
        {
            discardMaps.Add(card);
            UpdateDiscardCount();
        }
    }

    public MapCard PullFromDiscard()
    {
        if (discardMaps.Count > 0)
        {
            MapCard cardToReturn = discardMaps[discardMaps.Count - 1];
            discardMaps.RemoveAt(discardMaps.Count - 1);
            UpdateDiscardCount();
            return cardToReturn;
        }
        else
        {
        return null;
        }
    }

    public bool PullSelectCardFromDiscard(MapCard card)
    {
        if (discardMaps.Count > 0 && discardMaps.Contains(card))
        {
            discardMaps.Remove(card);
            UpdateDiscardCount();
            return true;
        }
        else 
        {
            return false;
        }

    }

    public List<MapCard> PullAllFromDiscard()
    {
        if(discardMaps.Count > 0)
        {
            List<MapCard> cardsToReturn = new List<MapCard>(discardMaps);
            discardMaps.Clear();
            UpdateDiscardCount();
            return cardsToReturn;
        }
        else
        {
            return new List<MapCard>();
        }
    }
}
