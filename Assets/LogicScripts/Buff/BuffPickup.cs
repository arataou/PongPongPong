using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    public PickupBuff buffType;

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerBase>();
        if (player == null) return;
        player.ApplyPickupBuff(buffType);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxBuffPickup);
        Destroy(gameObject);
    }
}
