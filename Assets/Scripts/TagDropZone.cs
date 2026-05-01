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

    private readonly Color CorrectColor = new Color(0.15f, 0.75f, 0.25f, 1f); 
    private readonly Color WrongColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    private readonly Color ActiveColor = new Color(1f, 1f, 1f, 1f);
    private readonly Color InactiveColor = new Color(1f, 1f, 1f, 0.35f);  

    void Awake()
    {
        bgImage = GetComponent<Image>();
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (bgImage)
        {
	        bgImage.color = active ? ActiveColor : InactiveColor;
        }
    }

    public void Reset()
    {
	    filled = false;
	    foreach (Transform child in transform)
	    {
		    if (child.GetComponent<DraggableTag>() != null)
		    {
			    Destroy(child.gameObject);
		    }
	    }
    }

    public void SetAnswerColor(bool isCorrect)
    {
	    if (bgImage)
	    {
		    bgImage.color = isCorrect ? CorrectColor : WrongColor;
	    }
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

        AudioManager.Instance?.PlayButtonClick();
        onAnswered?.Invoke(tag.TagValue);
    }
}
