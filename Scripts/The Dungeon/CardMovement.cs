using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private RectTransform canvasRectTranform;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    private Vector3 originalScale;
    private int currentState = 0;
    private Quaternion originalRotation;
    [SerializeField] private Vector3 originalPosition;
    private int siblingOrder;
    private GridManager gridManager;

    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay = new Vector2(0f, 150f);
    [SerializeField] private Vector3 playPosition;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject playArrow;
    [SerializeField] private float lerpFactor = 0.1f;



    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        siblingOrder = rectTransform.GetSiblingIndex();
        gridManager = FindFirstObjectByType<GridManager>();

        if (canvas != null)
        {
            canvasRectTranform = canvas.GetComponent<RectTransform>();
        }
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;
    }

    void Update()
    {

        switch (currentState)
        {
            case 1:
                HandleHoverState();
                break;
            case 2:
                HandleDragState();
                if (!Input.GetMouseButton(0)) //Check if mouse button is released
                {
                    TransitionToState0();
                }
                break;
            case 3:
                HandlePlayState();
                break;
        }
    }

    private void HandleHoverState()
    {
        glowEffect.SetActive(true);
        rectTransform.localScale = selectScale * originalScale;

    }

    private void HandleDragState()
    {
        //set the card's rotation to zero
        rectTransform.SetAsLastSibling();
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);

    }

    private void HandlePlayState()
    {
        if (!GameManager.Instance.PlayingCard)
        { 
            GameManager.Instance.PlayingCard = true;
        }

        rectTransform.localPosition = playPosition;
        rectTransform.localRotation = Quaternion.identity;

        // when you drag a card onto a grid spot, check and see if it has a collider.

        if (!Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.GetComponent<GridCell>())
            {
                GridCell cell = hit.collider.GetComponent<GridCell>();
                Vector2 targetPos = cell.gridIndex;

                int whatTypeOfCell = gridManager.AddObjectToGrid(GetComponent<MapDisplay>().mapData.prefab, GetComponent<MapDisplay>().mapData.mapCard, targetPos);
                MapHandManager mapHandManager = FindFirstObjectByType<MapHandManager>();
                DiscardManager discardManager = FindFirstObjectByType<DiscardManager>();


                    // play the map card, place it in the discard deck, show that it was played
                 if (whatTypeOfCell == 1)
                {
                    Debug.Log("Returned 1");

                    discardManager.AddToDiscard(GetComponent<MapDisplay>().mapData);
                    mapHandManager.cardsInHand.Remove(gameObject);
                    mapHandManager.UpdateMapHandVisuals();
                    Destroy(gameObject);
                }
                // burn the card, remove it from the deck
                else if (whatTypeOfCell == 2)
                {
                    Debug.Log("Returned 2");

                    mapHandManager.cardsInHand.Remove(gameObject);
                    mapHandManager.UpdateMapHandVisuals();
                    Destroy(gameObject);
                }
                // here is where we can add trap functionality
                else if (whatTypeOfCell == 3)
                {
                    Debug.Log("Returned 3");

                    discardManager.AddToDiscard(GetComponent<MapDisplay>().mapData);
                    mapHandManager.cardsInHand.Remove(gameObject);
                    mapHandManager.UpdateMapHandVisuals();
                    ChipManager chipManager = FindFirstObjectByType<ChipManager>();
                    chipManager.spendChips(10);
                    // some sort of boo effect or sound effect here
                    Destroy(gameObject);
                }
                // place a card on a boon functionality
                else if (whatTypeOfCell == 4)
                {
                    Debug.Log("Returned 4");

                    discardManager.AddToDiscard(GetComponent<MapDisplay>().mapData);
                    mapHandManager.cardsInHand.Remove(gameObject);
                    mapHandManager.UpdateMapHandVisuals();
                    ChipManager chipManager = FindFirstObjectByType<ChipManager>();
                    chipManager.winChips(5);
                    // some sort of yay effect or sound effect here

                    Destroy(gameObject);
                }
            }

            TransitionToState0();
        }

        if (Input.mousePosition.y < cardPlay.y)
        {
            currentState = 2;
            playArrow.SetActive(false);
            Cursor.visible = true;
        }
    }

    private void TransitionToState0()
    {
        currentState = 0;
        rectTransform.localScale = originalScale;
        rectTransform.localPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
        rectTransform.SetSiblingIndex(siblingOrder);
        glowEffect.SetActive(false);
        playArrow.SetActive(false);
        Cursor.visible = true;
        GameManager.Instance.PlayingCard = false;


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;
            playPosition = originalPosition + new Vector3(0f, 150f, 0f);
            currentState = 1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            TransitionToState0();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            currentState = 2;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentState == 2)
        {
            if (Input.mousePosition.y > cardPlay.y)
            {
                currentState = 3;
                playArrow.SetActive(true);
                Cursor.visible = false;
                rectTransform.localPosition = Vector3.Lerp(rectTransform.position, playPosition, lerpFactor);
            }
        }
    }

}
