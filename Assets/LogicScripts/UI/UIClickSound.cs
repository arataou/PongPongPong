using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour
{
    [SerializeField] AudioClip overrideClip;

    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(Play);
    }

    void Play()
    {
        if (AudioManager.Instance == null) return;
        var clip = overrideClip != null ? overrideClip : AudioManager.Instance.sfxClickUI;
        AudioManager.Instance.PlaySfx(clip);
    }
}
