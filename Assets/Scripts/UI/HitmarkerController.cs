using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitmarkerController : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Play()
    {
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
    }

    public void Stop()
    {
        animator.ResetTrigger("Hit");
    }
    
}
