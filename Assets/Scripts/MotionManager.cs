using UnityEngine;
using System.Collections;

[System.Serializable]
public class IdleVariant
{
    public string triggerName;
    public AnimationClip clip;
}

public class MotionManager : MonoBehaviour
{
    [Header("Animator & Settings")]
    public Animator animator;
    public float minIdleDelay = 5f;
    public float maxIdleDelay = 10f;

    [Header("Idle Variants")]
    public IdleVariant[] variants;

    private int currentIndex = 0;
    private bool isPlayingVariant = false;
    private float timer;
    private float nextIdleTime;

    [Header("LipSync")]
    public LipSyncToBlendShape lipSync;
    private bool isSpeaking = false;

    void Start()
    {
        ResetIdleTimer();
    }

    private bool wasSpeaking = false;
    private Coroutine talkingCoroutine;

    void Update()
    {
        if (lipSync != null)
            isSpeaking = lipSync.IsSpeaking;

        if (isSpeaking && !wasSpeaking)
        {
            if (talkingCoroutine != null) StopCoroutine(talkingCoroutine);
            talkingCoroutine = StartCoroutine(HandleTalkingLoop());
        }

        if (!isSpeaking && wasSpeaking)
        {
            if (talkingCoroutine != null) StopCoroutine(talkingCoroutine);
            animator.SetTrigger("BaseIdle");
        }

        wasSpeaking = isSpeaking;

        if (!isSpeaking)
        {
            if (!isPlayingVariant)
            {
                timer += Time.deltaTime;
                if (timer >= nextIdleTime)
                {
                    StartCoroutine(PlayNextIdle());
                }
            }
        }

    }

    IEnumerator HandleTalkingLoop()
    {
        while (isSpeaking)
        {
            animator.SetTrigger("Talking");

            float talkingLength = GetAnimationLength("Talking"); 
            yield return new WaitForSeconds(talkingLength);

        }
    }
        
    float GetAnimationLength(string stateName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (var clip in ac.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }
        return 1f;
    }

    void ResetIdleTimer()
    {
        timer = 0f;
        nextIdleTime = Random.Range(minIdleDelay, maxIdleDelay);
    }

    IEnumerator PlayNextIdle()
    {
        if (variants.Length == 0)
            yield break;

        isPlayingVariant = true;

        IdleVariant selected = variants[currentIndex];

        animator.SetTrigger(selected.triggerName);

        currentIndex = (currentIndex + 1) % variants.Length;

        float duration = (selected.clip != null) ? selected.clip.length : 1f;
        yield return new WaitForSeconds(duration);

        animator.SetTrigger("BaseIdle");

        isPlayingVariant = false;
        ResetIdleTimer();
    }
}
