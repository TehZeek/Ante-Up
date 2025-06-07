using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using TMPro;

public class DoorManager : MonoBehaviour
{
    public TextMeshProUGUI doorCounter;
public void UpdateDoorCount(int doorsLeft)
    {
        doorCounter.text = (" "+doorsLeft+ "");
    }
}
