using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    public PickupBuff buffType;

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerBase>();
        if (player == null) return;
        player.ApplyPickupBuff(buffType);
        Destroy(gameObject);
    }
}
