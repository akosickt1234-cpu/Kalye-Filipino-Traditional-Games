
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AnimationController : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float actionDuration = 1f;

    private bool actionActive;
    private bool walking;
    private bool running;
    private bool jumping;

    private Coroutine actionCoroutine;

    public void SetLocomotion(bool walking, bool running, bool jumping)
    {
        // Always remember the latest locomotion state.
        this.walking = walking;
        this.running = running;
        this.jumping = jumping;

        // Don't let locomotion override Grab or Throw.
        if (actionActive)
            return;

        ApplyLocomotion();
    }

    public void SetPickUp() // Q
    {
        PlayAction("isPickUp");
    }

    public void SetThrow() // E
    {
        PlayAction("isthrow");
    }

    private void PlayAction(string action)
    {
        // Prevent the previous action timer from interfering.
        if (actionCoroutine != null)
        {
            StopCoroutine(actionCoroutine);
        }

        actionActive = true;

        // Disable locomotion animations during the action.
        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);
        animator.SetBool("isJump", false);

        // Ensure only one action plays at a time.
        animator.SetBool("isPickUp", false);
        animator.SetBool("isthrow", false);

        animator.SetBool(action, true);

        actionCoroutine = StartCoroutine(EndAction(action));
    }

    private IEnumerator EndAction(string action)
    {
        yield return new WaitForSeconds(actionDuration);

        animator.SetBool(action, false);

        actionActive = false;
        actionCoroutine = null;

        // Return to the latest locomotion state.
        ApplyLocomotion();
    }

    private void ApplyLocomotion()
    {
        animator.SetBool("isWalk", walking);
        animator.SetBool("isRun", running);
        animator.SetBool("isJump", jumping);
    }
}
