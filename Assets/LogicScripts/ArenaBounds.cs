using UnityEngine;

public class ArenaBounds : MonoBehaviour
{
    public static ArenaBounds Instance { get; private set; }

    [SerializeField] Vector2 min = new Vector2(-8f, -4.5f);
    [SerializeField] Vector2 max = new Vector2( 8f,  4.5f);

    [Header("Runtime Border")]
    [SerializeField] bool  showBorder   = true;
    [SerializeField] Color borderColor  = new Color(1f, 0.3f, 0.3f, 0.55f);
    [SerializeField] float borderWidth  = 0.08f;
    [SerializeField] int   borderSortingOrder = 0;

    public Vector2 Min => min;
    public Vector2 Max => max;

    LineRenderer lr;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (showBorder) SetupBorder();
    }

    void SetupBorder()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();

        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = borderColor;
        lr.endColor      = borderColor;
        lr.startWidth    = borderWidth;
        lr.endWidth      = borderWidth;
        lr.loop          = true;
        lr.useWorldSpace = true;
        lr.numCapVertices    = 0;
        lr.numCornerVertices = 0;
        lr.sortingOrder  = borderSortingOrder;
        lr.positionCount = 4;
        lr.SetPosition(0, new Vector3(min.x, min.y, 0));
        lr.SetPosition(1, new Vector3(max.x, min.y, 0));
        lr.SetPosition(2, new Vector3(max.x, max.y, 0));
        lr.SetPosition(3, new Vector3(min.x, max.y, 0));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (min + max) * 0.5f;
        Vector3 size   = max - min;
        Gizmos.DrawWireCube(center, size);
    }
}
