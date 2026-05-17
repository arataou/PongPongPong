using UnityEngine;

public static class BallVisualUtility
{
    public static SpriteRenderer EnsureChildSpriteRenderer(GameObject host)
    {
        if (host == null) return null;

        var renderers = host.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].transform != host.transform)
                return renderers[i];
        }

        var rootRenderer = host.GetComponent<SpriteRenderer>();
        if (rootRenderer == null) return null;

        var child = new GameObject("BallVisual");
        child.transform.SetParent(host.transform, false);

        var visual = child.AddComponent<SpriteRenderer>();
        visual.sprite = rootRenderer.sprite;
        visual.color = rootRenderer.color;
        visual.sharedMaterial = rootRenderer.sharedMaterial;
        visual.sortingLayerID = rootRenderer.sortingLayerID;
        visual.sortingOrder = rootRenderer.sortingOrder;
        visual.flipX = rootRenderer.flipX;
        visual.flipY = rootRenderer.flipY;
        visual.drawMode = rootRenderer.drawMode;
        visual.size = rootRenderer.size;
        visual.maskInteraction = rootRenderer.maskInteraction;

        rootRenderer.enabled = false;
        return visual;
    }

    public static Color AccentColor(GameObject host)
    {
        var sprite = host != null ? host.GetComponentInChildren<SpriteRenderer>() : null;
        return sprite != null ? sprite.color : Color.white;
    }
}
