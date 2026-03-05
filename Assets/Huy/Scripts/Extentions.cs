using UnityEngine;

public static class Extentions
{
    private static LayerMask layerMask = LayerMask.GetMask("Default", "Map", "Enemy");

    // Catching grounded or jumping
    public static bool Raycast(this Rigidbody2D rigidbody, Vector2 direction)
    {
        if (rigidbody == null)
            return false;

        if (rigidbody.bodyType == RigidbodyType2D.Kinematic)
            return false;

        float radius = 0.25f;
        float distance = 0.375f;

        RaycastHit2D hit = Physics2D.CircleCast(rigidbody.position, radius, direction, distance, layerMask);

        return hit.collider != null && hit.rigidbody != rigidbody;
    }

    // Smoother for jumping
    public static bool DotTest(this Transform transform, Transform other, Vector2 testDirection)
    {
        Vector2 direction = other.position - transform.position;
        return Vector2.Dot(direction.normalized, testDirection) > 0.25f;
    }
}
