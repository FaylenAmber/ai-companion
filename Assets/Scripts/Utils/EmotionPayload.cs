using System;
using UnityEngine;

namespace EmotionData
{
    [Serializable]
    public class EmotionPayload
    {
        public string emotion;
        public string text;
        public float confidence;
        public float start;
        public float end;
        public float duration;
    }

    [Serializable]
    public class EmotionPayloadList
    {
        public EmotionPayload[] lines;
        public string subtitle;
    }
}
