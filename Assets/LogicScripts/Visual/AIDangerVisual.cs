using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class AIDangerVisual : MonoBehaviour
{
    const int Segments = 64;

    CircleCollider2D circle;
    LineRenderer warningGlow;
    LineRenderer warningLine;
    TextMeshPro label;

    void Awake()
    {
        BallVisualUtility.EnsureChildSpriteRenderer(gameObject);
        circle = GetComponent<CircleCollider2D>();
        warningGlow = CreateRing("AIDangerGlow", 1.2f, 0.11f, 2);
        warningLine = CreateRing("AIDangerLine", 1.08f, 0.035f, 4);
        CreateLabel();
    }

    void LateUpdate()
    {
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 5.8f);
        var glow = new Color(1f, 0.2f, 0.16f, Mathf.Lerp(0.12f, 0.36f, pulse));
        var line = new Color(1f, 0.36f, 0.3f, Mathf.Lerp(0.55f, 0.95f, pulse));
        SetRingColor(warningGlow, glow);
        SetRingColor(warningLine, line);
        if (warningGlow != null) warningGlow.transform.localScale = Vector3.one * (1f + 0.06f * pulse);
        if (label != null) label.color = new Color(1f, 0.38f, 0.32f, Mathf.Lerp(0.72f, 1f, pulse));
    }

    LineRenderer CreateRing(string name, float radiusMul, float width, int sorting)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = Segments;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = sorting;

        float baseR = circle != null ? circle.radius : 0.5f;
        float r = baseR * radiusMul;
        for (int i = 0; i < Segments; i++)
        {
            float a = (i / (float)Segments) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
        return lr;
    }

    void CreateLabel()
    {
        var go = new GameObject("AIDangerLabel");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.82f, 0f);
        label = go.AddComponent<TextMeshPro>();
        label.text = "AI";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 1.0f;
        label.fontStyle = FontStyles.Bold;
        label.sortingOrder = 7;
    }

    static void SetRingColor(LineRenderer lr, Color color)
    {
        if (lr == null) return;
        lr.startColor = color;
        lr.endColor = color;
    }
}
