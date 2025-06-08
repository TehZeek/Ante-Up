using UnityEngine;
using System.Collections.Generic;

public class DialogueButtonSpawner : MonoBehaviour
{
    [Header("Dialogue Setup")]
    public GameObject dialoguePrefab; // Prefab with CharacterDialogueWindow
    public Transform parentCanvas;    // Parent canvas or UI container

    [Header("Message Settings")]
    [TextArea]
    public string dialogueText = "This is a test message!";
    public int positionIndex = 2;     // 0=TopLeft, 1=TopRight, 2=BottomLeft, 3=BottomRight
    public bool faceRight = false;    // Flip tail if true

    private Queue<CharacterDialogueWindow> activeDialogues = new Queue<CharacterDialogueWindow>();

    public void SpawnDialogue()
    {
        if (dialoguePrefab == null || parentCanvas == null)
        {
            Debug.LogWarning("Missing dialoguePrefab or parentCanvas.");
            return;
        }

        GameObject instance = Instantiate(dialoguePrefab, parentCanvas);
        CharacterDialogueWindow dialogue = instance.GetComponent<CharacterDialogueWindow>();

        if (dialogue != null)
        {
            dialogue.ShowDialogue(dialogueText, positionIndex, faceRight);

            // Add to queue
            activeDialogues.Enqueue(dialogue);

            // Trim if over 4
            if (activeDialogues.Count > 4)
            {
                CharacterDialogueWindow oldest = activeDialogues.Dequeue();
                if (oldest != null)
                {
                    oldest.HideDialogue();
                }
            }
        }
        else
        {
            Debug.LogError("The prefab does not have a CharacterDialogueWindow component.");
        }
    }

    public void HideDialogue()
    {
        // Remove oldest dialogue
        while (activeDialogues.Count > 0)
        {
            CharacterDialogueWindow oldest = activeDialogues.Dequeue();
            if (oldest != null)
            {
                oldest.HideDialogue();
                break;
            }
        }
    }
}