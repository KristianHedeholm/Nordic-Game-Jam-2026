using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Animates the curtains sliding open on reveal.
/// </summary>
public class CurtainAnimator : MonoBehaviour
{
    public RectTransform curtainLeft;
    public RectTransform curtainRight;

    private Vector2 leftClosed;
    private Vector2 rightClosed;

    void Start()
    {
        if (curtainLeft)  leftClosed  = curtainLeft.anchoredPosition;
        if (curtainRight) rightClosed = curtainRight.anchoredPosition;
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
        float duration = 1.2f;
        float elapsed  = 0f;

        Vector2 leftTarget  = leftClosed  + new Vector2(-900, 0);
        Vector2 rightTarget = rightClosed + new Vector2(900, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            if (curtainLeft)  curtainLeft.anchoredPosition  = Vector2.Lerp(leftClosed,  leftTarget,  t);
            if (curtainRight) curtainRight.anchoredPosition = Vector2.Lerp(rightClosed, rightTarget, t);
            yield return null;
        }

        onComplete?.Invoke();
    }
}
