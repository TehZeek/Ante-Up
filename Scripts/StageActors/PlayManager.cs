using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayManager : MonoBehaviour
{
    [Header("Dialogue Setup")]
    public GameObject dialoguePrefab;
    public Transform parentCanvas;

    [Header("Message Settings")]
    [TextArea]
    public string dialogueText = "This is a test message!";
    public int positionIndex = 2; // 0=TopLeft, 1=TopRight, 2=BottomLeft, 3=BottomRight
    public bool faceRight = false;

    [Header("Actor Setup")]
    public List<GameObject> Actor = new List<GameObject>();

    private GameManager gameManager;
    //private Script script;
    private Queue<CharacterDialogueWindow> activeDialogues = new Queue<CharacterDialogueWindow>();

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        GenerateActors();
    }

    private void GenerateActors()
    {
        for (int i = 0; i < Actor.Count; i++)
        {
            if (Actor[i] != null && gameManager.characters.Count > i)
            {
                Image img = Actor[i].GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = gameManager.characters[i].Silloette;
                }
            }
        }
    }

    public void SetActorCharacter(int index)
    {
        if (index < 0 || index >= Actor.Count || index >= gameManager.characters.Count)
            return;

        Image img = Actor[index].GetComponent<Image>();
        if (img != null)
        {
            img.sprite = gameManager.characters[index].Silloette;
        }
    }

    public void EnterActor(int index)
    {
        if (index < 0 || index >= Actor.Count) return;

        var actorScript = Actor[index].GetComponent<PlotCharacter>();
        if (actorScript != null)
            StartCoroutine(EnterRoutine(actorScript));
    }

    private IEnumerator EnterRoutine(PlotCharacter actor)
    {
        actor.EnterScene();
        yield return new WaitForSeconds(0.5f);
    }

    public void ExitActor(int index)
    {
        if (index < 0 || index >= Actor.Count) return;

        var actorScript = Actor[index].GetComponent<PlotCharacter>();
        if (actorScript != null)
            StartCoroutine(ExitRoutine(actorScript));
    }

    private IEnumerator ExitRoutine(PlotCharacter actor)
    {
        actor.ExitScene();
        yield return new WaitForSeconds(0.5f);
    }

    public void ActorTalk(int index, float delay)
    {
        if (index < 0 || index >= Actor.Count) return;

        var actorScript = Actor[index].GetComponent<PlotCharacter>();
        if (actorScript != null)
            StartCoroutine(TalkRoutine(actorScript, delay));
    }

    private IEnumerator TalkRoutine(PlotCharacter actor, float delay)
    {
        actor.IsTalking(delay);
        yield return new WaitForSeconds(delay);
    }

    public void SpawnDialogue(string message, int positionIndex, bool faceRight, int actorIndex)
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
            dialogue.ShowDialogue(message, positionIndex, faceRight);

            // ✨ Trigger the corresponding actor's talking animation
            float duration = dialogue.GetTypingDuration(dialogueText);
            if (actorIndex >= 0 && actorIndex < Actor.Count)
            {
                var actorScript = Actor[actorIndex].GetComponent<PlotCharacter>();
                if (actorScript != null)
                {
                    Debug.Log("Talking for " + duration + " seconds (message length: " + dialogueText.Length + ")");

                    actorScript.IsTalking(duration);
                }
            }

            activeDialogues.Enqueue(dialogue);

            if (activeDialogues.Count > 4)
            {
                CharacterDialogueWindow oldest = activeDialogues.Dequeue();
                if (oldest != null)
                    oldest.HideDialogue();
            }
        }
        else
        {
            Debug.LogError("The prefab does not have a CharacterDialogueWindow component.");
        }
    }

    public void HideDialogue()
    {
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
