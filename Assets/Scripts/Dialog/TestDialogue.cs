using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TestDialogue : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image Felyne;
    public Image Spirit;

    [Header("Typing & Audio")]
    public float typeSpeed = 0.05f;
    public AudioSource audioSource;
    public AudioClip typeSound;
    public int charsPerSound = 2;

    [Header("Dialogue Lines")]
    public DialogueLine[] lines;

    private int index = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentLine = "";
    private string currentSpeaker = "";

    public bool IsFinished { get; private set; } = false;

    public void StartDialogue(DialogueLine[] newLines)
    {
        lines = newLines;
        index = 0;
        IsFinished = false;

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLine;
                isTyping = false;
            }
            else
            {
                index++;
                if (index < lines.Length)
                {
                    ShowLine();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    void ShowLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);;

        DialogueLine line = lines[index];
        currentSpeaker = line.speakerName;
        currentLine = line.text;

        nameText.text = currentSpeaker;

        Felyne.color = new Color(1f, 1f, 1f, 0f);
        Spirit.color = new Color(1f, 1f, 1f, 0f);
        if (currentSpeaker == "Player")
        {
            Felyne.sprite = line.portrait;
            Felyne.color = Color.white;   
        }
        else
        {
            Spirit.sprite = line.portrait;
            Spirit.color = Color.white; 
        }

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }


    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        int charCount = 0;

        foreach (char c in line)
        {
            dialogueText.text += c;
            charCount++;

            if (audioSource && typeSound && charCount % charsPerSound == 0)
                audioSource.PlayOneShot(typeSound);

            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        IsFinished = true;
    }
}



