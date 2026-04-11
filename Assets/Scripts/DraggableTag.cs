using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Makes a UI element draggable. Snaps back if not dropped on a valid DropZone.
/// </summary>
public class DraggableTag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public string value;
    [HideInInspector] public System.Action<string> onDropped; // called when successfully dropped

    private RectTransform rt;
    private Canvas rootCanvas;
    private Vector2 startPos;
    private Transform originalParent;
    private int originalSiblingIndex;
    private bool locked;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Lock() => locked = true;

    public void OnBeginDrag(PointerEventData e)
    {
        if (locked) return;
        startPos = rt.anchoredPosition;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Reparent to canvas root so it renders on top of everything
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        GetComponent<CanvasGroup>().blocksRaycasts = false;
        AudioManager.Instance?.PlayButtonClick();
    }

    public void OnDrag(PointerEventData e)
    {
        if (locked) return;
        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (locked) return;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        // If not handled by a drop zone, snap back
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        rt.anchoredPosition = startPos;
    }
}
