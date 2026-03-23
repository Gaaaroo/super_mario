using UnityEngine;

public static class Extentions
{
    private static LayerMask layerMask = LayerMask.GetMask("Default", "Map", "Enemy");

    private static readonly RaycastHit2D[] CircleCastHits = new RaycastHit2D[16];

    private const float GroundCastMinDotDown = 0.85f;

    private const float MinFloorNormalUpDot = 0.55f;

    // Catching grounded or jumping
    public static bool Raycast(this Rigidbody2D rigidbody, Vector2 direction)
    {
        if (rigidbody == null)
            return false;

        if (rigidbody.bodyType == RigidbodyType2D.Kinematic)
            return false;

        float radius = 0.25f;
        float distance = 0.375f;

        Vector2 dir = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.down;

        int mask = layerMask.value;
        int count = Physics2D.CircleCastNonAlloc(
            rigidbody.position, radius, dir, CircleCastHits, distance, mask);

        bool checkFloorNormal = Vector2.Dot(dir, Vector2.down) >= GroundCastMinDotDown;

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = CircleCastHits[i];
            if (hit.collider == null || hit.rigidbody == rigidbody)
                continue;
            if (hit.collider.isTrigger)
                continue;

            if (checkFloorNormal)
            {
                if (hit.normal.sqrMagnitude < 1e-6f)
                    continue;
                if (Vector2.Dot(hit.normal, Vector2.up) < MinFloorNormalUpDot)
                    continue;
            }

            return true;
        }

        return false;
    }

    // Smoother for jumping
    public static bool DotTest(this Transform transform, Transform other, Vector2 testDirection)
    {
        Vector2 direction = other.position - transform.position;
        return Vector2.Dot(direction.normalized, testDirection) > 0.25f;
    }
}
