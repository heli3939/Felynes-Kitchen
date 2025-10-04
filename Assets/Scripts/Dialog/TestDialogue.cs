using UnityEngine;
using TMPro;
using System.Collections;

public class TestDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private string[] lines = {
        "Hi! Welcome",
        "I hope you can enjoy your time here",
        "Good luck! Let's start!"
    };

    private int index = 0;
    private bool isTyping = false;
    private string currentLine = "";
    private Coroutine typingCoroutine;

    public float typeSpeed = 0.1f;

    public AudioSource audioSource;
    public AudioClip typeSound;
    public int charsPerSound = 2; // play sound every 2 characters

    void Start()
    {
        dialoguePanel.SetActive(true);
        ShowLine();
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
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
                    dialoguePanel.SetActive(false);
                }
            }
        }
    }

    void ShowLine()
    {
        currentLine = lines[index];
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        int charCount = 0;

        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            charCount++;

            if (audioSource != null && typeSound != null && charCount % charsPerSound == 0)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }
}

