using UnityEngine;

[DisallowMultipleComponent]
public class BoundaryDangerVisual : MonoBehaviour
{
    const int LineCount = 4;

    [SerializeField] float warningDistance = 1.15f;
    [SerializeField] Color warningColor = new Color(1f, 0.32f, 0.18f, 1f);
    [SerializeField] float lineWidth = 0.045f;

    readonly LineRenderer[] lines = new LineRenderer[LineCount];

    void Awake()
    {
        for (int i = 0; i < LineCount; i++)
            lines[i] = CreateLine($"BoundaryWarning_{i}");
    }

    void LateUpdate()
    {
        var bounds = ArenaBounds.Instance;
        if (bounds == null)
        {
            SetAlpha(0f);
            return;
        }

        UpdatePositions(bounds);
        float nearest = float.MaxValue;
        var players = MatchManager.AlivePlayers;
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (p == null) continue;
            Vector2 pos = p.transform.position;
            nearest = Mathf.Min(nearest, pos.x - bounds.Min.x);
            nearest = Mathf.Min(nearest, bounds.Max.x - pos.x);
            nearest = Mathf.Min(nearest, pos.y - bounds.Min.y);
            nearest = Mathf.Min(nearest, bounds.Max.y - pos.y);
        }

        float k = Mathf.Clamp01(1f - nearest / warningDistance);
        float pulse = 0.65f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
        SetAlpha(k * pulse * 0.72f);
    }

    LineRenderer CreateLine(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCapVertices = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 2;
        lr.enabled = false;
        return lr;
    }

    void UpdatePositions(ArenaBounds b)
    {
        Vector2 min = b.Min;
        Vector2 max = b.Max;
        lines[0].SetPosition(0, new Vector3(min.x, max.y, 0f));
        lines[0].SetPosition(1, new Vector3(max.x, max.y, 0f));
        lines[1].SetPosition(0, new Vector3(min.x, min.y, 0f));
        lines[1].SetPosition(1, new Vector3(max.x, min.y, 0f));
        lines[2].SetPosition(0, new Vector3(min.x, min.y, 0f));
        lines[2].SetPosition(1, new Vector3(min.x, max.y, 0f));
        lines[3].SetPosition(0, new Vector3(max.x, min.y, 0f));
        lines[3].SetPosition(1, new Vector3(max.x, max.y, 0f));
    }

    void SetAlpha(float alpha)
    {
        Color c = warningColor;
        c.a = alpha;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null) continue;
            lines[i].enabled = alpha > 0.02f;
            lines[i].startColor = c;
            lines[i].endColor = c;
        }
    }
}
