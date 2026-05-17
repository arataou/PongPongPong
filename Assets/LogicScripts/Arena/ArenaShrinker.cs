using UnityEngine;

public class ArenaShrinker : MonoBehaviour
{
    [SerializeField] float startDelay   = 30f;
    [SerializeField] float shrinkRate   = 0.05f;
    [SerializeField] float minHalfWidth = 3f;
    [SerializeField] float minHalfHeight = 2f;

    ArenaBounds  bounds;
    Vector2      initialMin;
    Vector2      initialMax;
    float        elapsed;

    void Start()
    {
        bounds = ArenaBounds.Instance;
        if (bounds == null) { enabled = false; return; }
        initialMin = bounds.Min;
        initialMax = bounds.Max;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (MatchManager.Instance != null &&
            MatchManager.Instance.CurrentOutcome != MatchManager.Outcome.Ongoing) return;

        elapsed += Time.deltaTime;
        if (elapsed < startDelay) return;

        float t = (elapsed - startDelay) * shrinkRate;
        Vector2 center = 0.5f * (initialMin + initialMax);
        Vector2 halfSize = 0.5f * (initialMax - initialMin);
        halfSize.x = Mathf.Max(minHalfWidth,  halfSize.x - t);
        halfSize.y = Mathf.Max(minHalfHeight, halfSize.y - t);

        var preset = ScriptableObject.CreateInstance<ArenaPreset>();
        preset.boundsMin = center - halfSize;
        preset.boundsMax = center + halfSize;
        preset.borderColor = new Color(1f, 0.3f, 0.3f, 0.55f);
        bounds.Apply(preset);
        Destroy(preset);
    }
}
