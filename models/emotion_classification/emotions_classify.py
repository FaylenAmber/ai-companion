from transformers import AutoTokenizer, AutoModelForSequenceClassification, pipeline
from utils.text_splitter import split_sentences

# Pretrained Emotions Classification model from Hugging Face
model_name = "SET_MODEL_NAME" # set the model name (see README.md for details)

# Load tokenizer and model
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForSequenceClassification.from_pretrained(model_name)

# Build a Hugging Face pipeline for text classification
# top_k=1 ensures only the most likely emotion is returned
sentiment_pipeline = pipeline(
    "text-classification",
    model=model,
    tokenizer=tokenizer,
    top_k=1
)

# Mapping from GoEmotions fine-grained labels → simplified VRM-style emotions
EMOTIONS_MAPPING = {
    # Joy group
    "admiration": "joy",
    "amusement": "joy",
    "approval": "joy",
    "caring": "joy",
    "desire": "joy",
    "excitement": "joy",
    "gratitude": "joy",
    "joy": "joy",
    "love": "joy",
    "optimism": "joy",
    "pride": "joy",
    "relief": "joy",

    # Angry group
    "anger": "angry",
    "annoyance": "angry",
    "disapproval": "angry",
    "disgust": "angry",

    # Sad group
    "fear": "sad",
    "nervousness": "sad",
    "remorse": "sad",
    "sadness": "sad",

    # Surprised group
    "confusion": "surprised",
    "curiosity": "surprised",
    "realization": "surprised",
    "surprise": "surprised",

    # Neutral / others
    "neutral": "neutral",
    "embarrassment": "shy",
    "grief": "sad"
}

def map_to_vrm_emotion(goemotions_label: str) -> str:
    """
    Map a GoEmotions label into a simplified VRM-compatible emotion.
    Defaults to 'neutral' if not found in mapping.
    """
    return EMOTIONS_MAPPING.get(goemotions_label, "neutral")

def detect_emotion(text: str):
    """
    Run emotion detection on a given text using the GoEmotions pipeline.
    Returns:
        (emotion_label, confidence_score)
    Example:
        ("joy", 0.85)
    """
    try:
        # Pipeline returns a nested list [[{label, score}]]
        result = sentiment_pipeline(text)[0][0]
        return result['label'].lower(), result['score']
    except Exception:
        # Fail-safe in case model inference errors
        return "unknown", 0.0

def estimate_emotion_segments(text: str):
    """
    Split the input text into sentences and estimate the emotion for each one.
    Returns a list of segments with structure:
        [
            {
                "text": <sentence>,
                "emotion": <mapped_emotion>,
                "confidence": <score 0-1>
            }, ...
        ]
    """
    sentences = split_sentences(text)
    segments = []

    for sentence in sentences:
        emotion, score = detect_emotion(sentence)
        vrm_emotion = map_to_vrm_emotion(emotion)
        segments.append({
            "text": sentence,
            "emotion": vrm_emotion,
            "confidence": round(score, 2)
        })

    return segments
