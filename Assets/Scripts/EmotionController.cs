using System.Collections;
using UnityEngine;
using VRM;

public class EmotionController : MonoBehaviour
{
    public VRMBlendShapeProxy blendShapeProxy;
    public LipSyncToBlendShape lipSync;
    public AutoBlink autoBlink;

    private Coroutine currentEmotionCoroutine;
    private BlendShapePreset currentEmotionPreset = BlendShapePreset.Unknown;
    private float currentEmotionWeight = 0f;

    [Header("Allowed Presets")]
    private readonly BlendShapePreset[] allowedPresets =
    {
        BlendShapePreset.Joy,
        BlendShapePreset.Angry,
        BlendShapePreset.Sorrow
    };

    [Header("Disable Blink on These Emotions")]
    public BlendShapePreset[] noBlinkEmotions = { BlendShapePreset.Angry }; 

    public void TriggerEmotion(string emotion, float duration, float confidence)
    {
        Debug.Log($"[EmotionController] TriggerEmotion → Emotion: {emotion}, Duration: {duration}, Confidence: {confidence}");

        if (currentEmotionCoroutine != null)
            StopCoroutine(currentEmotionCoroutine);

        if (emotion.ToLower() == "neutral")
        {
            currentEmotionCoroutine = StartCoroutine(FadeToNeutralCoroutine());
            return;
        }

        BlendShapePreset preset = GetPresetFromEmotion(emotion);

        if (preset == BlendShapePreset.Unknown)
        {
            Debug.LogWarning($"[EmotionController] Emotion {emotion} tidak dikenali.");
            return;
        }

        if (System.Array.IndexOf(allowedPresets, preset) == -1)
        {
            Debug.LogWarning($"[EmotionController] Preset {preset} tidak ada dalam allowedPresets.");
            return;
        }

        bool disableBlink = System.Array.IndexOf(noBlinkEmotions, preset) != -1;
        if (autoBlink != null)
            autoBlink.SetBlinkEnabled(!disableBlink);

        currentEmotionCoroutine = StartCoroutine(LerpEmotionCoroutine(preset, duration));
    }

    private IEnumerator LerpEmotionCoroutine(BlendShapePreset preset, float duration)
    {
        currentEmotionPreset = preset;
        currentEmotionWeight = 1.0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        yield return FadeToNeutralCoroutine();
    }

    private IEnumerator FadeToNeutralCoroutine(float fadeTime = 0.6f)
    {
        float startWeight = currentEmotionWeight;
        BlendShapePreset startPreset = currentEmotionPreset;

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            currentEmotionWeight = Mathf.Lerp(startWeight, 0f, t);

            if (startPreset != BlendShapePreset.Unknown)
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(startPreset), currentEmotionWeight);

            yield return null;
        }

        ResetEmotionBlendShapes();
        currentEmotionPreset = BlendShapePreset.Unknown;
        currentEmotionWeight = 0f;

        if (autoBlink != null)
            autoBlink.SetBlinkEnabled(true);

        ApplyIdle();
    }

    private void ApplyIdle()
    {
        var idleKey = BlendShapeKey.CreateUnknown("Idle");
        blendShapeProxy.ImmediatelySetValue(idleKey, 1.0f);
        blendShapeProxy.Apply();
    }

    private void ResetEmotionBlendShapes()
    {
        foreach (var preset in allowedPresets)
        {
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0f);
        }
        blendShapeProxy.Apply();
    }

    private BlendShapePreset GetPresetFromEmotion(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "anger": return BlendShapePreset.Angry;
            case "joy": return BlendShapePreset.Joy;
            case "sad": return BlendShapePreset.Sorrow;
            default: return BlendShapePreset.Unknown;
        }
    }

    private void LateUpdate()
    {
        foreach (var preset in allowedPresets)
        {
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0f);
        }

        if (currentEmotionPreset == BlendShapePreset.Unknown)
        {
            var idleKey = BlendShapeKey.CreateUnknown("Idle");
            blendShapeProxy.ImmediatelySetValue(idleKey, 1.0f);
        }
        else
        {
            blendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(currentEmotionPreset),
                currentEmotionWeight
            );
        }

        blendShapeProxy.Apply();
    }
}
