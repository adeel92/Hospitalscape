using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTemp : MonoBehaviour
{
    public Animator animator;
    public readonly int UNFILL_HASH = Animator.StringToHash("Unfill");

    public void Unfill()
    {
        animator.SetTrigger(UNFILL_HASH);
    }
}
