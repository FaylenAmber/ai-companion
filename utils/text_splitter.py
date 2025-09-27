import re

def clean_text(text: str) -> str:
    text = re.sub(r"(<3|[:;xX8][-^']?[)DpP(*/\\|])", "", text)
    
    emoji_pattern = re.compile(
        "[" 
        u"\U0001F600-\U0001F64F"
        u"\U0001F300-\U0001F5FF"
        u"\U0001F680-\U0001F6FF"
        u"\U0001F1E0-\U0001F1FF"
        u"\u2600-\u26FF"
        "]+", flags=re.UNICODE
    )
    text = emoji_pattern.sub("", text)

    text = re.sub(r"\s+", " ", text)
    text = re.sub(r"([!?\.])\1+", r"\1", text)

    return text.lower().strip()

def split_sentences(text: str) -> list[str]:
    text = clean_text(text)
    sentences = re.split(r"[.!?]+", text)
    return [s.strip() for s in sentences if s.strip()]
