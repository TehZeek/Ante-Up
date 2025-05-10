using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeekSpace;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardHighlight : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    public RectTransform parentRect;
    private Canvas canvas;
    private RectTransform canvasRectTranform;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    private Vector3 originalScale;
    private int currentState = 0;
    private Quaternion originalRotation;
    public Quaternion originalParentRotation;
    [SerializeField] private Vector3 originalPosition;

    private GridManager gridManager;

    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay = new Vector2(0f, 150f);
    [SerializeField] private Vector3 playPosition;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private float lerpFactor = 0.1f;



    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();
        gridManager = FindFirstObjectByType<GridManager>();

        if (canvas != null)
        {
            canvasRectTranform = canvas.GetComponent<RectTransform>();
        }
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;
        originalParentRotation = Quaternion.Euler(parentRect.localRotation.eulerAngles);
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
        }
    }

    private void HandleHoverState()
    {
        glowEffect.SetActive(true);
        parentRect.localRotation = Quaternion.Lerp(parentRect.localRotation, Quaternion.identity, lerpFactor);

        rectTransform.localScale = selectScale * originalScale;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1 ;

    }

    private void HandleDragState()
    {
        //set the card's rotation to zero
        
        rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);

    }




    private void TransitionToState0()
    {
        currentState = 0;
        rectTransform.localScale = originalScale;
        rectTransform.localPosition = originalPosition;
        rectTransform.localRotation = originalRotation; // Resets card rotation
        parentRect.localRotation = originalParentRotation; // Fix: Ensures parent rotation resets
        canvas.overrideSorting = false;
        glowEffect.SetActive(false);
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
        }
    }

}
