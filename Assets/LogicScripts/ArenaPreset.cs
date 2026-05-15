using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PongPongPong/Arena Preset", fileName = "ArenaPreset")]
public class ArenaPreset : ScriptableObject
{
    [Header("Identity")]
    public string  displayName   = "新地图";
    public Sprite  previewSprite;

    [Header("Bounds (world units)")]
    public Vector2 boundsMin     = new Vector2(-8f, -4.5f);
    public Vector2 boundsMax     = new Vector2( 8f,  4.5f);

    [Header("Visuals")]
    public Color   borderColor   = new Color(1f, 0.3f, 0.3f, 0.55f);
    public Sprite  backgroundSprite;
    public Color   backgroundTint = Color.white;

    [Header("Obstacles")]
    [Tooltip("障碍物 Prefab 列表。SampleScene 启动时按此 Instantiate。")]
    public List<ObstacleEntry> obstacles = new List<ObstacleEntry>();

    [System.Serializable]
    public struct ObstacleEntry
    {
        public GameObject prefab;
        public Vector2    position;
        public float      rotation;
        public Vector2    scale;
    }
}