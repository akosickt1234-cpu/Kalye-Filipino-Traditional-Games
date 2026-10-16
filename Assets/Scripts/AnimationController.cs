using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetLocomotion(bool walking, bool running, bool jumping)
    {
        animator.SetBool("isWalk", walking);
        animator.SetBool("isRun", running);
        animator.SetBool("isJump", jumping);
    }

    public void SetPickUp()// Q
    {
        animator.SetBool("isPickUp", true);
        StartCoroutine(Disable("isPickUp"));
    }

    public void SetThrow()// E
    {
        animator.SetBool("isthrow", true);
        StartCoroutine( Disable("isthrow"));
    }

    IEnumerator Disable(string anim)
    {
        yield return new WaitForSeconds(1);
        animator.SetBool(anim, false);
    }
}
