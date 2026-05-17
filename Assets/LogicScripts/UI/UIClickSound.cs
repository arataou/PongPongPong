using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour, IPointerDownHandler, ISubmitHandler
{
    [SerializeField] AudioClip overrideClip;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public static void InstallAll()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            if (btn == null) continue;
            if (btn.GetComponent<UIClickSound>() == null)
                btn.gameObject.AddComponent<UIClickSound>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Play();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Play();
    }

    void Play()
    {
        if (AudioManager.Instance == null) return;
        if (button != null && !button.interactable) return;
        var clip = overrideClip != null ? overrideClip : AudioManager.Instance.sfxClickUI;
        AudioManager.Instance.PlaySfx(clip, 0.75f);
    }
}
