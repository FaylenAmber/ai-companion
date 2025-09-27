using UnityEngine;
using System.Collections;
using VRM;

public class AutoBlink : MonoBehaviour
{
    public VRMBlendShapeProxy blendShapeProxy;

    [Header("Blink Settings")]
    public float minBlinkInterval = 2f;
    public float maxBlinkInterval = 5f;
    public float blinkDuration = 0.2f;

    private float nextBlinkTime;
    private bool isBlinking = false;
    private bool blinkEnabled = true;

    void Start()
    {
        ScheduleNextBlink();
    }

    void Update()
    {
        if (!blinkEnabled) return;

        if (Time.time >= nextBlinkTime && !isBlinking)
        {
            StartCoroutine(Blink());
        }
    }

    public void SetBlinkEnabled(bool enabled)
    {
        blinkEnabled = enabled;
        if (!enabled)
        {
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), 0f);
            blendShapeProxy.Apply();
        }
    }

    void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(minBlinkInterval, maxBlinkInterval);
    }

    IEnumerator Blink()
    {
        if (blendShapeProxy == null) yield break;

        isBlinking = true;

        float t = 0f;
        float duration = 0.1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float value = Mathf.Lerp(0f, 1f, t / duration);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), value);
            blendShapeProxy.Apply();
            yield return null;
        }

        yield return new WaitForSeconds(blinkDuration);

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float value = Mathf.Lerp(1f, 0f, t / duration);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), value);
            blendShapeProxy.Apply();
            yield return null;
        }

        isBlinking = false;
        ScheduleNextBlink();
    }
}
