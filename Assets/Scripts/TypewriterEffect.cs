using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Typewriter effect for TextMeshPro. Types out text letter by letter.
/// Click or any key to skip to the end instantly.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    public float charsPerSecond = 22f;

    private TMP_Text tmp;
    private Coroutine current;
    private bool isTyping;
    private Action onComplete;

    void Awake() => tmp = GetComponent<TMP_Text>();

    public bool IsTyping => isTyping;

    public void TypeWrite(string text, Action onDone = null)
    {
        if (current != null) StopCoroutine(current);
        onComplete = onDone;
        current = StartCoroutine(Routine(text));
    }

    public void Skip()
    {
        if (!isTyping) return;
        if (current != null) { StopCoroutine(current); current = null; }
        isTyping = false;
        tmp.maxVisibleCharacters = int.MaxValue;
        var cb = onComplete;
        onComplete = null;
        cb?.Invoke();
    }

    void Update()
    {
        if (!isTyping) return;
        if (Mouse.current    != null && Mouse.current.leftButton.wasPressedThisFrame)    Skip();
        else if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) Skip();
    }

    private IEnumerator Routine(string text)
    {
        isTyping = true;
        tmp.text = text;
        tmp.ForceMeshUpdate();

        int total   = tmp.textInfo.characterCount;
        float delay = 1f / Mathf.Max(1f, charsPerSecond);
        tmp.maxVisibleCharacters = 0;

        for (int i = 0; i < total; i++)
        {
            tmp.maxVisibleCharacters = i + 1;

            // Pause after sentence-ending punctuation only
            char c = i < text.Length ? text[i] : '\0';
            float wait = delay;
            if (c == '.' || c == '!' || c == '?') wait += 0.18f;
            else if (c == ',') wait += 0.06f;

            yield return new WaitForSeconds(wait);
        }

        tmp.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        current  = null;
        var cb   = onComplete;
        onComplete = null;
        cb?.Invoke();
    }
}
