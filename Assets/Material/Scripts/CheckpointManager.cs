using UnityEngine;

// Lưu checkpoint xuyên qua khi reload scene
public static class CheckpointManager
{
    private static bool hasCheckpoint;
    private static Vector2 checkpointPosition;

    public static bool HasCheckpoint => hasCheckpoint;

    public static Vector2 GetSpawnPosition(Vector2 defaultPosition)
    {
        return hasCheckpoint ? checkpointPosition : defaultPosition;
    }

    public static void SetCheckpoint(Vector2 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
    }

    public static void ClearCheckpoint()
    {
        hasCheckpoint = false;
    }
}

