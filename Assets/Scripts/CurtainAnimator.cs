using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Animates curtains opening from the centre — each side slides outward and bunches up.
/// </summary>
public class CurtainAnimator : MonoBehaviour
{
    public RectTransform curtainLeft;
    public RectTransform curtainRight;

    // How far each curtain slides out from centre (half the curtain width)
    public float openOffset = 480f;
    public float duration   = 1.4f;

    private Vector2 leftClosed;
    private Vector2 rightClosed;
    private Vector2 leftOpen;
    private Vector2 rightOpen;

    void Start()
    {
        if (curtainLeft)
        {
            leftClosed = curtainLeft.anchoredPosition;
            leftOpen   = leftClosed + new Vector2(-openOffset, 0);
        }
        if (curtainRight)
        {
            rightClosed = curtainRight.anchoredPosition;
            rightOpen   = rightClosed + new Vector2(openOffset, 0);
        }
    }

    public void OpenCurtains(Action onComplete)
    {
        StartCoroutine(AnimateOpen(onComplete));
    }

    public void CloseCurtains()
    {
        if (curtainLeft)  curtainLeft.anchoredPosition  = leftClosed;
        if (curtainRight) curtainRight.anchoredPosition = rightClosed;
    }

    private IEnumerator AnimateOpen(Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Ease out — fast start, slows as it reaches the folded position
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
            if (curtainLeft)  curtainLeft.anchoredPosition  = Vector2.Lerp(leftClosed,  leftOpen,  t);
            if (curtainRight) curtainRight.anchoredPosition = Vector2.Lerp(rightClosed, rightOpen, t);
            yield return null;
        }

        if (curtainLeft)  curtainLeft.anchoredPosition  = leftOpen;
        if (curtainRight) curtainRight.anchoredPosition = rightOpen;

        onComplete?.Invoke();
    }
}
