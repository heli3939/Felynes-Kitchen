using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManage: MonoBehaviour
{
    [Header("Data (ScriptableObject)")]
    public DialogueData dialogueData;
    public DialogueLine[] lines;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public GameObject pressHint;

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.025f;
    public bool playOnStart = true;

    private DialogueLine[] activeLines;
    private int index = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullLine = "";

    public string speakerName;   
    public Sprite portrait;

    void Start()    
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (dialogueData != null && dialogueData.lines != null && dialogueData.lines.Length > 0)
            activeLines = dialogueData.lines;
        else
            activeLines = lines;

        if (playOnStart && activeLines != null && activeLines.Length > 0)
            StartDialogue();
    }

    void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                OnAdvance();
            }
        }
    }

    public void StartDialogue()
    {
        if (activeLines == null || activeLines.Length == 0) return;
        index = 0;
        dialoguePanel.SetActive(true);
        InputBlocker.Lock(true);
        ShowLine();
    }

    private void ShowLine()
    {
        DialogueLine line = activeLines[index];
        nameText.text = line.speakerName ?? "";
        if (line.portrait != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = true;
        }
        else
        {
            portraitImage.enabled = false;
        }
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.text));
    }
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        currentFullLine = line ?? "";
        dialogueText.text = "";
        if (pressHint != null) pressHint.SetActive(false);

        for (int i = 0; i < currentFullLine.Length; i++)
        {
            dialogueText.text += currentFullLine[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        if (pressHint != null) pressHint.SetActive(true);
    }

    private void OnAdvance()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullLine;
            isTyping = false;
            if (pressHint != null) pressHint.SetActive(true);
            return;
        }
        index++;
        if (index < activeLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        InputBlocker.Lock(false);
    }
}