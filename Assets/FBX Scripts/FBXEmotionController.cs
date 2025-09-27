using System.Collections;
using UnityEngine;
using FBX;

public class FBXEmotionController : MonoBehaviour
{
    [Header("References")]
    public FBXBlendShapeProxy blendShapeProxy;
    public LipSyncToFBXBlendShape lipSync;
    public AutoBlink autoBlink;

    private Coroutine currentEmotionCoroutine;
    private FBXBlendShapePreset currentEmotionPreset = FBXBlendShapePreset.Unknown;
    private float currentEmotionWeight = 0f;

    [Header("Allowed Presets")]
    public FBXBlendShapePreset[] allowedPresets =
    {
        FBXBlendShapePreset.Joy,
        FBXBlendShapePreset.Angry,
        FBXBlendShapePreset.Sad
    };

    [Header("Disable Blink on These Emotions")]
    public FBXBlendShapePreset[] noBlinkEmotions = { FBXBlendShapePreset.Angry };

    public void TriggerEmotion(FBXBlendShapePreset preset, float duration, float confidence)
    {
        Debug.Log($"[EmotionControllerFBX] TriggerEmotion → Emotion: {preset}, Duration: {duration}, Confidence: {confidence}");

        if (currentEmotionCoroutine != null)
            StopCoroutine(currentEmotionCoroutine);

        if (preset == FBXBlendShapePreset.Neutral)
        {
            currentEmotionCoroutine = StartCoroutine(FadeToNeutralCoroutine());
            return;
        }

        if (System.Array.IndexOf(allowedPresets, preset) == -1)
        {
            Debug.LogWarning($"[EmotionControllerFBX] Preset {preset} tidak ada dalam allowedPresets.");
            return;
        }

        bool disableBlink = System.Array.IndexOf(noBlinkEmotions, preset) != -1;
        if (autoBlink != null)
            autoBlink.SetBlinkEnabled(!disableBlink);

        currentEmotionCoroutine = StartCoroutine(LerpEmotionCoroutine(preset, duration));
    }

    private IEnumerator LerpEmotionCoroutine(FBXBlendShapePreset preset, float duration)
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
        FBXBlendShapePreset startPreset = currentEmotionPreset;

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            currentEmotionWeight = Mathf.Lerp(startWeight, 0f, t);

            if (startPreset != FBXBlendShapePreset.Unknown)
                blendShapeProxy.ImmediatelySetValue(startPreset, currentEmotionWeight);

            yield return null;
        }

        ResetEmotionBlendShapes();
        currentEmotionPreset = FBXBlendShapePreset.Unknown;
        currentEmotionWeight = 0f;

        if (autoBlink != null)
            autoBlink.SetBlinkEnabled(true);

        ApplyIdle();
    }

    private void ApplyIdle()
    {
        blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.Neutral, 1.0f);
    }

    private void ResetEmotionBlendShapes()
    {
        foreach (var preset in allowedPresets)
        {
            blendShapeProxy.ImmediatelySetValue(preset, 0f);
        }
    }

    private void LateUpdate()
    {
        foreach (var preset in allowedPresets)
        {
            blendShapeProxy.ImmediatelySetValue(preset, 0f);
        }

        if (currentEmotionPreset == FBXBlendShapePreset.Unknown)
        {
            blendShapeProxy.ImmediatelySetValue(FBXBlendShapePreset.Neutral, 1.0f);
        }
        else
        {
            blendShapeProxy.ImmediatelySetValue(currentEmotionPreset, currentEmotionWeight);
        }
    }
}
