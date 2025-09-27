using UnityEngine;

public class IdleManager : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    void Update()
    {
        bool isTalking = audioSource.isPlaying;
        animator.SetBool("isTalking", isTalking);
    }
}
