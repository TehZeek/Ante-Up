using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;



public class GridManager : MonoBehaviour
{
    public int width = 9;
    public int height = 4;
    public GameObject gridCellPrefab;
    public List<GameObject> gridObjects = new List<GameObject>();
    public GameObject[,] gridCells;
    public GameObject obstacle;
    public GameObject startSpace;
    public MapCard obstacleMapCard;
    public MapCard startSpaceMapCard;
    public MapCard burnSpaceMapCard;
    private GameManager gameManager;
    public GridCell upperCell;
    public GridCell lowerCell;
    public GridCell leftCell;
    public GridCell rightCell;
    public int doorCount = 0;


    public void Awake()
    {
        CreateGrid();
    }


    public void CreateGrid()
    {
        
        gameManager = FindFirstObjectByType<GameManager>();

        gridCells = new GameObject[width+1, height];
        Vector2 centerOffset = new Vector2(width / 2.0f - 0.5f, height / 2.0f - 0.5f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                
                Vector2 gridPosition = new Vector2(x, y);
                Vector2 spawnPosition = (gridPosition - centerOffset)*24;

                GameObject gridCell = Instantiate(gridCellPrefab, spawnPosition, Quaternion.identity);
                gridCell.transform.SetParent(transform);

                gridCell.GetComponent<GridCell>().gridIndex = gridPosition;

                gridCells[x, y] = gridCell;
                
            }
        }
        //make the burn card
        
        Vector2 burnPosition = new Vector2(9, 0);
        Vector2 burnyPosition =new Vector2(200, -75);
        GameObject burnCell = Instantiate(gridCellPrefab, burnyPosition, Quaternion.identity);
        burnCell.transform.SetParent(transform);
        burnCell.GetComponent<GridCell>().gridIndex = burnPosition;
        burnCell.transform.localScale = new Vector3(125f, 125f, 1f);
        burnCell.GetComponent<GridCell>().destroyCard = true;
        burnCell.GetComponent<GridCellDisplay>().SetSpecial(3);
        gridCells[9, 0] = burnCell;
        
        PlaceStartSpace();
        AddExtraItemsToGridBasedOnDifficulty(gameManager.Difficulty);
        SetExitTriggers();

    }




    public void PlaceStartSpace()
    {

        Vector2 startPosition = new Vector2(4, 0);
        Debug.Log("Sending startspace " + startSpace.gameObject.name + " ssMapCard " + startSpaceMapCard.name + " start position " + startPosition);
        AddObjectToGrid(startSpace, startSpaceMapCard, startPosition);
        Debug.Log("sending start to add objects");
    }


    public int AddObjectToGrid(GameObject obj, MapCard mapCard, Vector2 gridPosition)
    {
        // we're going to check the place on the gridCells[,] to see if it has the right door connection, is trapped, boon, or the burn pile.
        // return 0 if you can't play
        // 1 for normal play
        // 2 for burn card
        // 3 for trapped space
        // 4 for boon space


        if (gridPosition.x == 9)
        { 
            return 2;
        }
        if (gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height)
        {
            GridCell cell = gridCells[(int)gridPosition.x, (int)gridPosition.y].GetComponent<GridCell>();
            if (cell.cellFull) { return 0; }
            else
            {
                Debug.Log("checking doors..."+cell+mapCard+gridPosition);
                if (!checkDoorsAroundTile(cell, mapCard, gridPosition)) { return 0; }
                else
                {
                    Debug.Log("doors checked");

                    GameObject newObj = Instantiate(obj, cell.GetComponent<Transform>().position, Quaternion.identity);
                    newObj.transform.SetParent(transform);
                    TileDisplay tdis = newObj.GetComponent<TileDisplay>();
                    tdis.mapData = mapCard;
                    tdis.UpdateTileDisplay();

                    gridObjects.Add(newObj);
                    cell.objectInCell = newObj;

                    int tileType = (int)tdis.mapData.mapObject;
                    cell.cellFull = true;

                    

                    // check the tile's exits and turn them on

                    for (int i = 0; i < tdis.mapData.mapExit.Count; i++)
                    {
                        int y = (int)tdis.mapData.mapExit[i];
                        if (y == 0) cell.upExit = true;
                        if (y == 1) cell.downExit = true;
                        if (y == 2) cell.leftExit = true;
                        if (y == 3) cell.rightExit = true;

                    }

                    // To come: check if there is an encounter that prevents movement or placement past it
                    SetExitTriggers();

                    if (doorCount == 0)
                    {
                        gameManager.GameOver();
                    }
                    Debug.Log(cell.isaTrap);
                    Debug.Log(cell.isaBoon);

                    //To come: trigger traps or boons
                    if (cell.isaTrap)
                    {
                        Debug.Log("logged trap");
                        return 3;
                    }
                    if (cell.isaBoon)
                    {
                        Debug.Log("Logged Boon");

                        return 4; 
                    }
                    return 1;
                }
            }
        }
        else return 0;
    }



    public bool checkDoorsAroundTile(GridCell cell, MapCard mapCard, Vector2 gridPosition)
    {
        Debug.Log(cell + " " + mapCard + " " + gridPosition);
        int y = (int)gridPosition.y;
        int x = (int)gridPosition.x;
        Debug.Log("checking the startCheck" + mapCard.name);
        int startCheck = (int)mapCard.mapObject;
        Debug.Log("startcheck is " + startCheck);
        //write in exception for start, trap, boon, obstacle
        if (startCheck == 0 || startCheck == 11 || startCheck == 9) { return true; }


        for (int i = 0; i < mapCard.mapExit.Count; i++)
        {
            int j = (int)mapCard.mapExit[i];
            if (j == 0 && y < height-1)
            { 
                 GridCell upperCell = gridCells[x, y + 1].GetComponent<GridCell>();
                 if (upperCell.downExit) 
                    {return true;}
            }
            if (j == 1 && y > 0)
            {
                GridCell lowerCell = gridCells[x, y - 1].GetComponent<GridCell>();
                if (lowerCell.upExit)
                { return true; }
            }
            if (j == 2 && x > 0)
            {
                GridCell leftCell = gridCells[x - 1, y].GetComponent<GridCell>();
                if (leftCell.rightExit) 
                { return true; }
            }
            if (j == 3 && x < width-1)
            {
                GridCell rightCell = gridCells[x + 1, y].GetComponent<GridCell>();
                if (rightCell.leftExit) 
              { return true; }
            }
        }
        return false;
    }

    // turns on the exit triggers on each cell that detect if a door can be laid
    // also counts the legal doors and updates the count

    public void SetExitTriggers()
    {
        int doorCheck;
        doorCount = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                doorCheck = 0;
                
                GridCell checkGrid = gridCells[x, y].GetComponent<GridCell>();
                if (y < height - 2)
                {
                    GridCell upper = gridCells[x, y+1].GetComponent<GridCell>();
                    if (upper.downExit) { checkGrid.upExit = true; doorCheck++; }
                }
                if (y > 0)
                {
                    GridCell lower = gridCells[x, y - 1].GetComponent<GridCell>();
                    if (lower.upExit) { checkGrid.downExit = true; doorCheck++; }
                }
                if (x < width - 2)
                {
                    GridCell right = gridCells[x + 1, y].GetComponent<GridCell>();
                    if (right.leftExit) { checkGrid.rightExit = true; doorCheck++; }
                }
                if (x > 0)
                {
                    GridCell left = gridCells[x - 1, y].GetComponent<GridCell>();
                    if (left.rightExit) { checkGrid.leftExit = true; doorCheck++; }
                }
                if (doorCheck > 0 && !checkGrid.cellFull) 
                {
                    doorCheck = 0;
                    doorCount++;
                    checkGrid.GetComponent<GridCellDisplay>().ShowLegal();
                }
                DoorManager doorManager = FindFirstObjectByType<DoorManager>();
                doorManager.UpdateDoorCount(doorCount);

            }
        }
    }




    public void AddObjectToRandomCell(GameObject obj, MapCard mapCard)
    {
        List<Vector2> availableCells = new List<Vector2>();

        // Find all empty cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = gridCells[x, y].GetComponent<GridCell>();
                if (!cell.cellFull)
                {
                    availableCells.Add(new Vector2(x, y));
                }
            }
        }

        // Choose a random cell
        if (availableCells.Count > 0)
        {
            Vector2 chosenCell = availableCells[UnityEngine.Random.Range(0, availableCells.Count)];
            AddObjectToGrid(obj,  mapCard, chosenCell);
        }
    }

    public void AddExtraItemsToGridBasedOnDifficulty(int diff)
    {

        int traps = Mathf.RoundToInt(diff / 2);
        for (int i=0; i<traps; i++)
        {
            TurnIconOnRandomCell(1);
        }
        int obstacles = diff;
        for (int i = 0; i < obstacles; i++)
        {
            AddObjectToRandomCell(obstacle, obstacleMapCard);
        }
        int boons = Mathf.RoundToInt(5 - diff/2);
        for (int i = 0; i < boons; i++)
        {
            TurnIconOnRandomCell(2);
        }
    }

    public void TurnIconOnRandomCell(int icontype)
    {
        List<Vector2> availableCells = new List<Vector2>();

        // Find all empty cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = gridCells[x, y].GetComponent<GridCell>();
                if (!cell.cellFull && !cell.isaTrap &&!cell.isaBoon)
                {
                    availableCells.Add(new Vector2(x, y));
                }
            }
        }

        // Choose a random cell
        if (availableCells.Count > 0)
        {
            Vector2 chosenCell = availableCells[UnityEngine.Random.Range(0, availableCells.Count)];
            GridCell cell = gridCells[(int)chosenCell.x, (int)chosenCell.y].GetComponent<GridCell>();
            if (icontype == 1)
            { 
                cell.isaTrap = true;
                cell.GetComponent<GridCellDisplay>().SetSpecial(icontype);
            }
            if (icontype == 2)
            { 
                cell.isaBoon = true;
                cell.GetComponent<GridCellDisplay>().SetSpecial(icontype);
            }

        }
    }




    public void DestroyGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridCells[x, y] != null)
                {
                    Destroy(gridCells[x, y]); // Destroy the GridCell GameObject
                    gridCells[x, y] = null; // Clear reference
                }
            }
        }

        // Destroy any additional grid-related objects

        {
            Destroy(gridCells[9, 0]);
            gridCells[9, 0] = null;
        }

        gridObjects.Clear(); // Empty the list of added objects
                             // Destroy all children of GridManager
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

    }



}
