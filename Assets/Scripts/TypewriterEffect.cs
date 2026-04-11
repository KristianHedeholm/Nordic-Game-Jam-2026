using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Typewriter effect for TextMeshPro text.
/// Call TypeWrite() to animate text letter by letter.
/// Click/tap to skip to full text instantly.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    [Tooltip("Characters per second")]
    public float charsPerSecond = 40f;

    private TMP_Text tmp;
    private Coroutine current;
    private bool isTyping;
    private Action onComplete;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Start typing out the given text. Calls onComplete when done (or skipped).
    /// </summary>
    public void TypeWrite(string text, Action onDone = null, float? speed = null)
    {
        if (current != null) StopCoroutine(current);
        onComplete = onDone;
        float cps = speed ?? charsPerSecond;
        current = StartCoroutine(Routine(text, cps));
    }

    /// <summary>
    /// Skip to the end immediately.
    /// </summary>
    public void Skip()
    {
        if (!isTyping) return;
        if (current != null) StopCoroutine(current);
        isTyping = false;
        tmp.maxVisibleCharacters = tmp.textInfo.characterCount;
        onComplete?.Invoke();
        onComplete = null;
    }

    public bool IsTyping => isTyping;

    void Update()
    {
        // Click or any key to skip (new Input System)
        if (isTyping && (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                         Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame))
            Skip();
    }

    private IEnumerator Routine(string text, float cps)
    {
        isTyping = true;
        tmp.text = text;
        tmp.ForceMeshUpdate();
        tmp.maxVisibleCharacters = 0;

        int total = tmp.textInfo.characterCount;
        float delay = 1f / cps;
        float elapsed = 0f;

        while (tmp.maxVisibleCharacters < total)
        {
            elapsed += Time.deltaTime;
            int show = Mathf.FloorToInt(elapsed / delay);
            tmp.maxVisibleCharacters = Mathf.Min(show, total);

            // Slightly faster on punctuation pauses
            if (tmp.maxVisibleCharacters < total)
            {
                char c = text[Mathf.Min(tmp.maxVisibleCharacters, text.Length - 1)];
                if (c == '.' || c == '!' || c == '?')
                    yield return new WaitForSeconds(0.12f);
                else if (c == ',' || c == ';')
                    yield return new WaitForSeconds(0.05f);
            }

            yield return null;
        }

        tmp.maxVisibleCharacters = total;
        isTyping = false;
        onComplete?.Invoke();
        onComplete = null;
    }
}
