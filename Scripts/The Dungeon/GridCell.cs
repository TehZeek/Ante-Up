using UnityEngine;

public class GridCell : MonoBehaviour
{

    public Vector2 gridIndex;
    public bool cellFull = false;
    public bool upExit = false;
    public bool downExit = false;
    public bool leftExit = false;
    public bool rightExit = false;
    public bool blocksMovement = false;
    public bool destroyCard = false;
    public bool isaTrap = false;
    public bool isaBoon = false;

    public GameObject objectInCell;

}