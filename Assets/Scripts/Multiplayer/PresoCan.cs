
using Unity.Netcode;
using UnityEngine;

public class PresoCan : NetworkBehaviour
{
    private Rigidbody rb;
    private bool hasBeenKnockedDown;
    public bool HasBeenKnockedDown => hasBeenKnockedDown;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Remember the can's original scene position and rotation.
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || hasBeenKnockedDown)
            return;

        Tsinelas tsinelas =
            collision.gameObject.GetComponent<Tsinelas>();

        if (tsinelas == null)
            tsinelas =
                collision.gameObject.GetComponentInParent<Tsinelas>();

        if (tsinelas == null)
            return;

        hasBeenKnockedDown = true;

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager != null)
            manager.AwardCanKnocked(tsinelas.ThrowerClientId);

        Debug.Log("The can was knocked down!");

        // The can falls using normal physics.
        if (rb != null)
            rb.isKinematic = false;
    }

    public void ResetCan()
    {
        if (!IsServer)
            return;

        // Stop physics before restoring the starting pose.
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetPositionAndRotation(
            startingPosition,
            startingRotation
        );

        if (rb != null)
        {
            rb.position = startingPosition;
            rb.rotation = startingRotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();

        hasBeenKnockedDown = false;

        if (rb != null)
            rb.isKinematic = false;

        Debug.Log("Can reset to its starting position!");
    }
}
