using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using TMPro;
using System.Collections;
using VRM;
using EmotionData;

public class WebSocketAudioEmotionReceiver : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSourcePlayback;
    public AudioSource audioSourceLipSync;

    [Header("Controllers")]
    public LipSyncToBlendShape lipSync;
    public EmotionController emotionController;
    public TextMeshProUGUI subtitleText;

    private WebSocket websocket;
    private Coroutine emotionCoroutine;

    private async void Start()
    {
        websocket = new WebSocket("ws://127.0.0.1:8765");

        websocket.OnMessage += async (bytes) =>
        {
            await System.Threading.Tasks.Task.Yield();
            if (bytes.Length < 10) return;

            int jsonLen = BitConverter.ToInt32(bytes, 0);
            string json = Encoding.UTF8.GetString(bytes, 4, jsonLen);

            byte[] wavData = new byte[bytes.Length - 4 - jsonLen];
            Buffer.BlockCopy(bytes, 4 + jsonLen, wavData, 0, wavData.Length);

            EmotionPayloadList payloadList = JsonConvert.DeserializeObject<EmotionPayloadList>(json);

            AudioClip clip = WavUtility.ToAudioClip(wavData, 0, "TTS_Audio");
            if (clip == null)
            {
                Debug.LogError("[WebSocket] Failed to convert audio bytes to AudioClip.");
                return;
            }

            PlayAudioAndSetEmotions(clip, payloadList.lines, payloadList.subtitle);
        };

        await websocket.Connect();
        Debug.Log("[WebSocket] Connected to Python server.");
    }

    private void PlayAudioAndSetEmotions(AudioClip clip, EmotionPayload[] emotionLines, string subtitle)
    {
        Debug.Log($"[WebSocket] Received {emotionLines.Length} emotion lines.");
        foreach (var line in emotionLines)
        {
            Debug.Log($"[WebSocket] Emotion: {line.emotion}, Start: {line.start}, End: {line.end}, Duration: {line.duration}, Confidence: {line.confidence}, Text: \"{line.text}\"");
        }

        audioSourcePlayback.clip = clip;
        audioSourceLipSync.clip = clip;

        audioSourcePlayback.volume = 1f;
        audioSourcePlayback.Play();
        audioSourceLipSync.Play();

        if (lipSync != null)
            lipSync.BeginLipSync(audioSourceLipSync);

        if (emotionCoroutine != null)
            StopCoroutine(emotionCoroutine);

        emotionCoroutine = StartCoroutine(PlayEmotionSequence(emotionLines, clip.length, subtitle));
    }

    private IEnumerator PlayEmotionSequence(EmotionPayload[] lines, float totalDuration, string subtitle)
    {
        if (subtitleText != null)
            subtitleText.text = subtitle;

        for (int i = 0; i < lines.Length; i++)
        {
            EmotionPayload line = lines[i];

            float clampedStart = Mathf.Min(line.start, totalDuration);
            float clampedEnd = Mathf.Min(line.end, totalDuration);
            float clampedDuration = Mathf.Max(0, clampedEnd - clampedStart);

            float waitTime = i == 0
                ? clampedStart
                : clampedStart - Mathf.Min(lines[i - 1].end, totalDuration);

            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            if (lipSync != null)
                lipSync.NotifySegmentActive(clampedStart, clampedEnd);

            if (emotionController != null && clampedDuration > 0)
                emotionController.TriggerEmotion(line.emotion, clampedDuration, line.confidence);
        }

        if (emotionController != null && emotionController.blendShapeProxy != null)
        {
            emotionController.blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateUnknown("Idle"), 1.0f);
            emotionController.blendShapeProxy.Apply();
        }
    }

    private void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
            Debug.Log("[WebSocket] Connection closed.");
        }
    }
}
