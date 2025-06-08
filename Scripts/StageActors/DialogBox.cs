using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CharacterDialogueWindow : MonoBehaviour
{
    [Header("References")]
    public GameObject dialogueWindowObject;           // Root UI container
    public TextMeshProUGUI dialogueText;
    public RectTransform dialogueWindow;              // Background panel (resizable)

    [Header("Settings")]
    public float characterDelay = 0.05f;
    public float initialDelay = 0.2f;
    private Vector2 padding = new Vector2(40f, 20f);
    private Vector2 offsetFromCorner = new Vector2(200f, 200f); // Offset from each corner
    public Image tailImage; // Assign in the inspector
    private Vector3 tailOriginalLocalPos;
    private Vector3 textOriginalLocalPos;


    private Coroutine typingRoutine;

    void Awake()
    {
        if (tailImage != null)
            tailOriginalLocalPos = tailImage.rectTransform.localPosition;

        if (dialogueText != null)
            textOriginalLocalPos = dialogueText.rectTransform.localPosition;
    }

    public void ShowDialogue(string message, int positionIndex, bool faceRight)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        positionIndex = Mathf.Clamp(positionIndex, 0, 3);
        DialogueAnchorPosition anchorPosition = (DialogueAnchorPosition)positionIndex;

        dialogueWindowObject.SetActive(true);
        PositionDialogue(anchorPosition);

        if (tailImage != null)
        {
            Vector3 tailScale = tailImage.rectTransform.localScale;
            tailScale.x = faceRight ? -1f : 1f;
            tailImage.rectTransform.localScale = tailScale;
        }

        // Set the full message for sizing purposes
        dialogueText.text = message;
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);

        typingRoutine = StartCoroutine(ResizeThenType(message));
    }

    private IEnumerator TypeTextAfterResize(string message)
    {
        yield return new WaitForSeconds(initialDelay); // Optional: wait before typing
        yield return null; // Wait 1 frame for layout to settle

        for (int i = 0; i < message.Length; i++)
        {
            dialogueText.text += message[i];
            yield return new WaitForSeconds(characterDelay);
        }
    }

    public enum DialogueAnchorPosition
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    private void PositionDialogue(DialogueAnchorPosition anchor)
    {
        Vector2 anchoredPosition = Vector2.zero;

        float x = offsetFromCorner.x;
        float y = offsetFromCorner.y;

        switch (anchor)
        {
            case DialogueAnchorPosition.TopLeft:
                anchoredPosition = new Vector2(-x, y);
                break;
            case DialogueAnchorPosition.TopRight:
                anchoredPosition = new Vector2(x, y);
                break;
            case DialogueAnchorPosition.BottomLeft:
                anchoredPosition = new Vector2(-x, -y);
                break;
            case DialogueAnchorPosition.BottomRight:
                anchoredPosition = new Vector2(x, -y);
                break;
        }

        dialogueWindow.anchoredPosition = anchoredPosition;
    }

    private IEnumerator StartDialogueWithDelay(string message)
    {
        yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(TypeTextAndResize(message));
    }

    private IEnumerator TypeTextAndResize(string message)
    {
        for (int i = 0; i < message.Length; i++)
        {
            dialogueText.text += message[i];
            StartCoroutine(ResizeThenType(message));
            yield return new WaitForSeconds(characterDelay);
        }

        ResizeWindowToText();
    }

    private IEnumerator ResizeThenType(string message)
    {
        // Wait 1 frame so layout system settles after SetText
        yield return null;

        ResizeWindowToText(); // Now layout should return correct bounds

        dialogueText.text = ""; // Clear before typing
        yield return new WaitForSeconds(initialDelay); // Optional pause before typewriter

        for (int i = 0; i < message.Length; i++)
        {
            dialogueText.text += message[i];
            yield return new WaitForSeconds(characterDelay);
        }
    }

    private void ResizeWindowToText()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);
        Vector2 textSize = dialogueText.textBounds.size;
        dialogueWindow.sizeDelta = textSize + padding;
    }

    public void HideDialogue()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        Destroy(gameObject); // Or use SetActive(false) if pooling
    }
}