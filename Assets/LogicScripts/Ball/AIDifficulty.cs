using UnityEngine;

public enum AIDifficulty { Easy = 0, Normal = 1, Hard = 2 }

public enum GameMode { PvP = 0, Coop = 1 }

public struct DifficultyProfile
{
    public float firstSpawnDelay;
    public float spawnInterval;
    public float speedMul;
    public float massMul;
    // COOP 模式下,buff 生成间隔 / 状态轮盘切换间隔统一乘这个 (1=不变, <1=更紧)。PvP 不应用。
    public float coopPacingMul;

    public static DifficultyProfile Get(AIDifficulty d)
    {
        switch (d)
        {
            case AIDifficulty.Easy:
                return new DifficultyProfile { firstSpawnDelay = 90f, spawnInterval = 15f, speedMul = 0.7f, massMul = 0.7f, coopPacingMul = 1.4f };
            case AIDifficulty.Hard:
                return new DifficultyProfile { firstSpawnDelay = 30f, spawnInterval = 5f,  speedMul = 1.4f, massMul = 1.3f, coopPacingMul = 0.55f };
            default:
                return new DifficultyProfile { firstSpawnDelay = 60f, spawnInterval = 10f, speedMul = 1.0f, massMul = 1.0f, coopPacingMul = 1.0f };
        }
    }

    public static string Label(AIDifficulty d)
    {
        switch (d)
        {
            case AIDifficulty.Easy:   return "菜";
            case AIDifficulty.Hard:   return "变态";
            default:                  return "普通";
        }
    }
}
