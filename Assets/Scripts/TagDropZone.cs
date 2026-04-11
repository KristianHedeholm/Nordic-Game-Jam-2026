using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drop target for DraggableTag. Accepts one tag, locks it in, fires callback.
/// </summary>
public class TagDropZone : MonoBehaviour, IDropHandler
{
    [HideInInspector] public bool isActive;   // only accepts drops when active
    [HideInInspector] public string category;
    [HideInInspector] public System.Action<string> onAnswered;

    private bool filled;
    private Image bgImage;
    public TMP_Text answerLabel;  // text label shown inside the holder once filled

    void Awake()
    {
        bgImage = GetComponent<Image>();
    }

    public void SetActive(bool active)
    {
        isActive = active;
        // Highlight active zone, dim inactive
        if (bgImage) bgImage.color = active
            ? new Color(1f, 1f, 1f, 1f)       // full opacity when active
            : new Color(1f, 1f, 1f, 0.35f);   // dimmed when not yet active
    }

    public void OnDrop(PointerEventData e)
    {
        if (!isActive || filled) return;

        var tag = e.pointerDrag?.GetComponent<DraggableTag>();
        if (tag == null) return;

        filled = true;
        tag.Lock();

        // Snap tag into this zone
        tag.transform.SetParent(transform, false);
        var tagRT = tag.GetComponent<RectTransform>();
        tagRT.anchoredPosition = Vector2.zero;
        tagRT.sizeDelta = GetComponent<RectTransform>().sizeDelta;

        // Show answer text
        if (answerLabel != null)
            answerLabel.text = tag.value;

        AudioManager.Instance?.PlayButtonClick();
        if (UnityEngine.Random.value > 0.5f) AudioManager.Instance?.PlayCrowdCheerGood();
        else AudioManager.Instance?.PlayCrowdCheerBad();

        onAnswered?.Invoke(tag.value);
    }
}
