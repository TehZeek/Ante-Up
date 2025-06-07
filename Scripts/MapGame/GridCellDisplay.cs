using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(GridCell))]
public class GridCellDisplay : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Color highlightColor = Color.cyan;
    public Color posColor = Color.green;
    public Color negColor = Color.red;
    public Color invisColor = Color.clear;
    public Color legalColor = Color.green;
    public GridCell gridCell;
    public GameObject[] backgrounds;
    private bool setBackground = false;
    private GridManager gridManager;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        gridCell = GetComponent<GridCell>();
        gridManager = FindFirstObjectByType<GridManager>();
    }

    void Update()
    {
        if (!setBackground)
        {
            SetBackground();
        }
    }

    // When the mouse enters the collider area
    void OnMouseEnter()
    {
        int doors = 0;
        if (gridCell.upExit) { doors++; }
        if (gridCell.downExit) { doors++; }
        if (gridCell.leftExit) { doors++; }
        if (gridCell.rightExit) { doors++; }


        if (!GameManager.Instance.PlayingCard)
        {
            spriteRenderer.color = highlightColor;

        }
        else if (gridCell.cellFull || doors == 0)
        {
            spriteRenderer.color = negColor;
        }
        else
        {
            spriteRenderer.color = posColor;
        }


    }

    // When the mouse exits the collider area
    void OnMouseExit()
    {
        spriteRenderer.color = originalColor;
    }

    private void SetBackground()
    {
        if (!gridCell.destroyCard)
        {
            if (gridCell.gridIndex.x % 2 != 0)
            {
                backgrounds[0].SetActive(true);
            }
            if (gridCell.gridIndex.y % 2 != 0)
            {
                backgrounds[1].SetActive(true);
            }
            setBackground = true;
            }
    }

    public void SetSpecial(int whatItIs)
    {
        // it's a trap
        if (whatItIs == 1)
        {
            backgrounds[2].SetActive(true);
        }
        // it's a boon
        if (whatItIs == 2)
        {
            backgrounds[3].SetActive(true);
        }

        //it's the discard pile
        if (whatItIs == 3)
        {
            backgrounds[4].SetActive(true);
            spriteRenderer.color = invisColor;
            originalColor = invisColor;

        }

    }
    public void ShowLegal()
    {
        originalColor = legalColor;
        spriteRenderer.color = originalColor;

    }

}
