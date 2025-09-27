using UnityEngine;
using static OVRLipSync;
using FBX;

public class LipSyncToFBXBlendShape : MonoBehaviour
{
    public OVRLipSyncContext lipSyncContext;
    public FBXBlendShapeProxy blendShapeProxy;
    public Animator animator;

    private readonly FBXBlendShapePreset[] visemeToPreset =
    {
        FBXBlendShapePreset.Unknown, // 0
        FBXBlendShapePreset.Unknown, // 1
        FBXBlendShapePreset.Unknown, // 2
        FBXBlendShapePreset.Unknown, // 3
        FBXBlendShapePreset.Unknown, // 4
        FBXBlendShapePreset.Unknown, // 5
        FBXBlendShapePreset.Unknown, // 6
        FBXBlendShapePreset.Unknown, // 7
        FBXBlendShapePreset.Unknown, // 8
        FBXBlendShapePreset.Unknown, // 9
        FBXBlendShapePreset.A,       // 10
        FBXBlendShapePreset.E,       // 11
        FBXBlendShapePreset.I,       // 12
        FBXBlendShapePreset.O,       // 13
        FBXBlendShapePreset.U        // 14
    };

    private float[] oldWeights = new float[15];
    public bool IsSpeaking { get; private set; }
    private float valueA, valueE, valueI, valueO, valueU;

    private float currentBlend = 0f;
    private float smoothVelocity;
    public float smoothTime = 0.2f;

    private float segmentEndTime = -1f;
    private AudioSource currentAudio;

    public void BeginLipSync(AudioSource source)
    {
        lipSyncContext.audioSource = source;
        lipSyncContext.ResetContext();
        currentAudio = source;
    }

    public void NotifySegmentActive(float start, float end)
    {
        if (currentAudio != null)
        {
            float worldEnd = Time.time + (end - currentAudio.time);
            segmentEndTime = Mathf.Max(segmentEndTime, worldEnd);
        }
    }

    void Update()
    {
        if (lipSyncContext == null || blendShapeProxy == null) return;

        var frame = lipSyncContext.GetCurrentPhonemeFrame();
        bool speakingNow = false;

        for (int i = 10; i <= 14; i++) // A, E, I, O, U
        {
            float weight = frame.Visemes[i];
            float smoothed = Mathf.Lerp(oldWeights[i], weight, 0.5f);
            oldWeights[i] = smoothed;

            var preset = visemeToPreset[i];
            if (preset != FBXBlendShapePreset.Unknown)
            {
                switch (preset)
                {
                    case FBXBlendShapePreset.A: valueA = smoothed; break;
                    case FBXBlendShapePreset.E: valueE = smoothed; break;
                    case FBXBlendShapePreset.I: valueI = smoothed; break;
                    case FBXBlendShapePreset.O: valueO = smoothed; break;
                    case FBXBlendShapePreset.U: valueU = smoothed; break;
                }

                if (smoothed > 0.01f) speakingNow = true;
            }
        }

        bool inSegment = (segmentEndTime > Time.time);
        IsSpeaking = speakingNow || inSegment;

        float target = IsSpeaking ? 1f : 0f;
        currentBlend = Mathf.SmoothDamp(currentBlend, target, ref smoothVelocity, smoothTime);

        if (animator != null)
            animator.SetFloat("talkBlend", currentBlend);
    }

    void LateUpdate()
    {
        if (blendShapeProxy == null) return;

        if (IsSpeaking)
        {
            blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.A, valueA);
            blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.E, valueE);
            blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.I, valueI);
            blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.O, valueO);
            blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.U, valueU);
        }
    }
}
