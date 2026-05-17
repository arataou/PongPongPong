using System.Collections.Generic;
using UnityEngine;

public class ArenaLoader : MonoBehaviour
{
    [SerializeField] ArenaBounds    arenaBounds;
    [SerializeField] SpriteRenderer backgroundRenderer;
    [SerializeField] Transform      obstacleParent;
    [Tooltip("没有从 MainMenu 进入时用的默认地图(直接 Play SampleScene 调试场景用)。")]
    [SerializeField] ArenaPreset    fallbackPreset;

    readonly List<GameObject> spawnedObstacles = new List<GameObject>();

    void Awake()
    {
        var preset = GameSession.Instance != null ? GameSession.Instance.SelectedArena : null;
        if (preset == null) preset = fallbackPreset;
        if (preset != null) ApplyPreset(preset);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameBgm();
    }

    void ApplyPreset(ArenaPreset preset)
    {
        if (arenaBounds == null) arenaBounds = ArenaBounds.Instance;
        if (arenaBounds != null) arenaBounds.Apply(preset);

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = preset.backgroundSprite;
            backgroundRenderer.color  = preset.backgroundTint;
            if (preset.backgroundSprite != null)
            {
                Vector2 size = preset.boundsMax - preset.boundsMin;
                backgroundRenderer.transform.position = (Vector3)(0.5f * (preset.boundsMin + preset.boundsMax));
                backgroundRenderer.drawMode = SpriteDrawMode.Sliced;
                backgroundRenderer.size     = size;
            }
        }

        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
            if (spawnedObstacles[i] != null) Destroy(spawnedObstacles[i]);
        spawnedObstacles.Clear();

        foreach (var entry in preset.obstacles)
        {
            if (entry.prefab == null) continue;
            Vector3 scale = entry.scale == Vector2.zero ? Vector3.one : new Vector3(entry.scale.x, entry.scale.y, 1f);
            var go = Instantiate(
                entry.prefab,
                new Vector3(entry.position.x, entry.position.y, 0f),
                Quaternion.Euler(0f, 0f, entry.rotation),
                obstacleParent);
            go.transform.localScale = scale;
            spawnedObstacles.Add(go);
        }
    }
}