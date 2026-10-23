using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class MenuButtonAppearance : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] TMP_Text label;
    [SerializeField] UnityEngine.UI.Image surface;
    [SerializeField] GameObject cursor;

    static readonly Color Ink = new Color(0.15f, 0.17f, 0.16f, 1f);
    static readonly Color LightInk = new Color(0.98f, 0.98f, 0.95f, 1f);
    static readonly Color Highlight = new Color(0.18f, 0.20f, 0.19f, 0.62f);
    bool chosen, hovered, focused;

    public void Configure(TMP_Text text, UnityEngine.UI.Image image, GameObject marker)
    {
        label = text;
        surface = image;
        cursor = marker;
        Refresh();
    }

    void OnEnable()
    {
        focused = EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == gameObject;
        Refresh();
    }

    void OnDisable() { hovered = focused = false; }

    public void SetChosen(bool value) { chosen = value; Refresh(); }

    void Refresh()
    {
        bool on = chosen || hovered || focused;
        if (surface != null) surface.color = on ? Highlight : Color.clear;
        if (label != null) label.color = on ? LightInk : Ink;
        if (cursor != null) cursor.SetActive(on);
    }

    public void OnPointerEnter(PointerEventData data) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData data) { hovered = false; Refresh(); }
    public void OnSelect(BaseEventData data) { focused = true; Refresh(); }
    public void OnDeselect(BaseEventData data) { focused = false; Refresh(); }

    public static void Paint(UnityEngine.UI.Button button, bool selected,
        Color selectedColor, Color unselectedColor)
    {
        if (button == null) return;
        var appearance = button.GetComponent<MenuButtonAppearance>();
        if (appearance != null) appearance.SetChosen(selected);
        else
        {
            var image = button.GetComponent<UnityEngine.UI.Image>();
            if (image != null) image.color = selected ? selectedColor : unselectedColor;
        }
    }
}
