
using UnityEngine;

public class PatinteroLineDetector : MonoBehaviour
{
    [SerializeField] private LayerMask lineLayer;
    [SerializeField] private float checkRadius = 0.12f;
    [SerializeField] private float groundCheckHeight = 0.1f;

    public bool IsOnLine(Vector3 worldPosition)
    {
        Vector3 checkPosition =
            worldPosition + Vector3.up * groundCheckHeight;

        return Physics.CheckSphere(
            checkPosition,
            checkRadius,
            lineLayer,
            QueryTriggerInteraction.Collide
        );
    }

    public float DistanceToLine(Vector3 worldPosition, float maxSearchDistance)
    {
        Vector3 checkPosition =
            worldPosition + Vector3.up * groundCheckHeight;

        Collider[] nearbyLines = Physics.OverlapSphere(
            checkPosition,
            maxSearchDistance,
            lineLayer,
            QueryTriggerInteraction.Collide
        );

        float closestDistance = float.PositiveInfinity;

        foreach (Collider line in nearbyLines)
        {
            if (line == null)
                continue;

            Vector3 closestPoint = line.ClosestPoint(checkPosition);

            float distance = Vector3.Distance(
                checkPosition,
                closestPoint
            );

            if (distance < closestDistance)
                closestDistance = distance;
        }

        return closestDistance;
    }

}
