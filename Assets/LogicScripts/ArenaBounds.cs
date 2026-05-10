using UnityEngine;

public class ArenaBounds : MonoBehaviour
{
    public static ArenaBounds Instance { get; private set; }

    [SerializeField] Vector2 min = new Vector2(-8f, -4.5f);
    [SerializeField] Vector2 max = new Vector2( 8f,  4.5f);

    public Vector2 Min => min;
    public Vector2 Max => max;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (min + max) * 0.5f;
        Vector3 size   = max - min;
        Gizmos.DrawWireCube(center, size);
    }
}
