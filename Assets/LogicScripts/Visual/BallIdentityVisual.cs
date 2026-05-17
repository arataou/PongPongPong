using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerBase))]
[DisallowMultipleComponent]
public class BallIdentityVisual : MonoBehaviour
{
    const int Segments = 64;

    PlayerBase player;
    SpriteRenderer sprite;
    CircleCollider2D circle;
    LineRenderer outerGlow;
    LineRenderer innerLine;
    TextMeshPro label;

    void Awake()
    {
        player = GetComponent<PlayerBase>();
        circle = GetComponent<CircleCollider2D>();
        sprite = BallVisualUtility.EnsureChildSpriteRenderer(gameObject);

        outerGlow = CreateRing("PlayerIdentityGlow", 1.18f, 0.085f, 1);
        innerLine = CreateRing("PlayerIdentityLine", 1.08f, 0.026f, 3);
        CreateLabel();
    }

    void LateUpdate()
    {
        Color c = sprite != null ? sprite.color : Color.white;
        SetRingColor(outerGlow, new Color(c.r, c.g, c.b, 0.2f));
        SetRingColor(innerLine, new Color(c.r, c.g, c.b, 0.8f));

        float pulse = 1f + 0.035f * Mathf.Sin(Time.time * 4.5f + player.PlayerIndex);
        if (outerGlow != null) outerGlow.transform.localScale = Vector3.one * pulse;
        if (label != null)
        {
            label.text = $"P{player.PlayerIndex}";
            label.color = new Color(c.r, c.g, c.b, 0.92f);
        }
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
        var go = new GameObject("PlayerIdentityLabel");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.78f, 0f);
        label = go.AddComponent<TextMeshPro>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 1.1f;
        label.fontStyle = FontStyles.Bold;
        label.sortingOrder = 6;
        label.text = $"P{player.PlayerIndex}";
    }

    static void SetRingColor(LineRenderer lr, Color color)
    {
        if (lr == null) return;
        lr.startColor = color;
        lr.endColor = color;
    }
}
