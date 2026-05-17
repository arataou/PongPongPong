using UnityEngine;

public static class BallVisualUtility
{
    public static SpriteRenderer EnsureChildSpriteRenderer(GameObject host)
    {
        if (host == null) return null;

        var rootRenderer = host.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = true;
            DisableLegacyChildVisual(host.transform);
            return rootRenderer;
        }

        return host.GetComponentInChildren<SpriteRenderer>(true);
    }

    public static Color AccentColor(GameObject host)
    {
        var sprite = host != null ? EnsureChildSpriteRenderer(host) : null;
        return sprite != null ? sprite.color : Color.white;
    }

    static void DisableLegacyChildVisual(Transform root)
    {
        if (root == null) return;

        var child = root.Find("BallVisual");
        if (child == null) return;

        var legacyRenderer = child.GetComponent<SpriteRenderer>();
        if (legacyRenderer != null)
            legacyRenderer.enabled = false;
    }
}
