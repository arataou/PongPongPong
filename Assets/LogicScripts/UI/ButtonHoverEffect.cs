using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler,  IPointerUpHandler
{
    [SerializeField] float hoverScale  = 1.08f;
    [SerializeField] float pressScale  = 0.95f;
    [SerializeField] float lerpSpeed   = 16f;

    Vector3 baseScale;
    Vector3 target;
    bool    hover;
    bool    pressed;

    void Awake()
    {
        baseScale = transform.localScale;
        target    = baseScale;
    }

    void OnDisable()
    {
        transform.localScale = baseScale;
        hover = pressed = false;
        target = baseScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, target,
            1f - Mathf.Exp(-lerpSpeed * Time.unscaledDeltaTime));
    }

    public void OnPointerEnter(PointerEventData e) { hover   = true;  Recalc(); }
    public void OnPointerExit (PointerEventData e) { hover   = false; Recalc(); }
    public void OnPointerDown (PointerEventData e) { pressed = true;  Recalc(); }
    public void OnPointerUp   (PointerEventData e) { pressed = false; Recalc(); }

    void Recalc()
    {
        float mul = pressed ? pressScale : (hover ? hoverScale : 1f);
        target = baseScale * mul;
    }
}
