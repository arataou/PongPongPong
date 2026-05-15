using UnityEngine;

public enum AIDifficulty { Easy = 0, Normal = 1, Hard = 2 }

public enum GameMode { PvP = 0, Coop = 1 }

public struct DifficultyProfile
{
    public float firstSpawnDelay;
    public float spawnInterval;
    public float speedMul;
    public float massMul;

    public static DifficultyProfile Get(AIDifficulty d)
    {
        switch (d)
        {
            case AIDifficulty.Easy:
                return new DifficultyProfile { firstSpawnDelay = 90f, spawnInterval = 15f, speedMul = 0.7f, massMul = 0.7f };
            case AIDifficulty.Hard:
                return new DifficultyProfile { firstSpawnDelay = 30f, spawnInterval = 5f,  speedMul = 1.4f, massMul = 1.3f };
            default:
                return new DifficultyProfile { firstSpawnDelay = 60f, spawnInterval = 10f, speedMul = 1.0f, massMul = 1.0f };
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
