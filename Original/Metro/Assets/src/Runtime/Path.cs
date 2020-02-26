using Unity.Collections;
using Unity.Mathematics;

public static class Path
{
    public static void GetNextTarget(int currentIndex, float speed, float3 currentPosition, NativeArray<float3> path, out int targetIndex, out float3 targetPosition)
    {
        targetIndex = currentIndex;
        targetPosition = currentPosition;

        float distance = 0f;
        while (distance < speed)
        {
            targetIndex = (targetIndex + 1) % path.Length;
            targetPosition = path[targetIndex];
            distance = math.distance(targetPosition, currentPosition);
        }
    }
}
