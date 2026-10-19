
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
}
