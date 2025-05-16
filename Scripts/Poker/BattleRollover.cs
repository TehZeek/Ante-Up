using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BattleRollover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler

{
    public int ButtonNum;
    public BattleMenu battleMenu;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleMenu.AllInTrigger)
        {
            battleMenu.UpdateButtonDisplay(ButtonNum);
                return;
        }
        if (!battleMenu.BetIsSet)
        {
            battleMenu.UpdateButtonDisplay(ButtonNum);
        }
        else
        {
            battleMenu.UpdateButtonDisplay(ButtonNum+10);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleMenu.AllInTrigger)
        {
            battleMenu.UpdateButtonDisplay(3);
            return;
        }
        if (!battleMenu.BetIsSet)
        {
            battleMenu.UpdateButtonDisplay(1);
        }
        else
        {
            battleMenu.UpdateButtonDisplay(2);
        }
    }
}
