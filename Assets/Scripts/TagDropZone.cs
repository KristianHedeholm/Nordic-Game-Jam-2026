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

        // Snap tag into this zone and resize to fill it
        tag.transform.SetParent(transform, false);
        var tagRT = tag.GetComponent<RectTransform>();
        // Fill only the dark grey inner area — leave yellow banner + border fully visible
        // Top ~35% is the yellow label, sides/bottom have ~28px border
        tagRT.anchorMin = new Vector2(0, 0);
        tagRT.anchorMax = new Vector2(1, 0.58f); // stop below the yellow banner
        tagRT.offsetMin = new Vector2(32, 28);
        tagRT.offsetMax = new Vector2(-32, 0);

        // Hide the placeholder label — the tag itself shows the answer
        if (answerLabel != null)
            answerLabel.gameObject.SetActive(false);

        AudioManager.Instance?.PlayButtonClick();
        onAnswered?.Invoke(tag.value);
    }
}
