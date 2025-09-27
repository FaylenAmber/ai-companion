using UnityEngine;
using static OVRLipSync;
using VRM;

public class LipSyncToBlendShape : MonoBehaviour
{
    public OVRLipSyncContext lipSyncContext;
    public VRMBlendShapeProxy blendShapeProxy;
    public Animator animator;

    private readonly string[] visemeToBlendshape =
        { "", "", "", "", "", "", "", "", "", "", "A", "E", "I", "O", "U" };
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
            string key = visemeToBlendshape[i];
            float weight = frame.Visemes[i];
            float smoothed = Mathf.Lerp(oldWeights[i], weight, 0.5f);
            oldWeights[i] = smoothed;

            if (!string.IsNullOrEmpty(key))
            {
                switch (key)
                {
                    case "A": valueA = smoothed; break;
                    case "E": valueE = smoothed; break;
                    case "I": valueI = smoothed; break;
                    case "O": valueO = smoothed; break;
                    case "U": valueU = smoothed; break;
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
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), valueA);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.E), valueE);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.I), valueI);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), valueO);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.U), valueU);

            blendShapeProxy.Apply();
        }
    }
}
