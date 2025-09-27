from TTS.api import TTS
from pydub import AudioSegment
import io
import os
import tempfile
import soundfile as sf
import numpy as np

from utils.text_splitter import split_sentences

# Load the multilingual XTTS v2 model
MODEL_NAME = "tts_models/multilingual/multi-dataset/xtts_v2"
tts = TTS(model_name=MODEL_NAME, progress_bar=True, gpu=False)

# Dictionary of speakers (name -> reference audio path)
# Add more speakers if needed
speakers = {
    "speaker_name": "path/to/sample.wav",
}

def encode_wav(wav_array, sample_rate=24000):
    """
    Encode a NumPy array or list of float32 samples into a WAV buffer.
    
    Args:
        wav_array (np.ndarray or list): Raw audio waveform between -1.0 and 1.0
        sample_rate (int): Audio sampling rate
    
    Returns:
        BytesIO: Encoded WAV file buffer
    """
    buffer = io.BytesIO()

    # Ensure input is a NumPy array
    if isinstance(wav_array, list):
        wav_array = np.array(wav_array, dtype=np.float32)
    if not isinstance(wav_array, np.ndarray):
        raise TypeError("TTS output must be array/list, cannot encode.")

    # Convert float32 -> int16 PCM
    wav_int16 = np.int16(np.clip(wav_array, -1.0, 1.0) * 32767)

    # Write to memory buffer as WAV
    sf.write(buffer, wav_int16, samplerate=sample_rate, format="WAV", subtype="PCM_16")
    buffer.seek(0)
    return buffer


def tts_output(text, speaker="SET_CHOSEN_SPEAKER_HERE", language="en", memory_limit_mb=50,
                temperature=0.3, top_p=0.85, top_k=50, sample_rate=24000):
    """
    Generate speech audio from text using XTTS v2.
    
    Args:
        text (str): Input text to synthesize
        speaker (str): Speaker key defined in `speakers` dict
        language (str): Language code for synthesis
        memory_limit_mb (int): Soft memory usage limit (not enforced, only estimated)
        temperature (float): Sampling temperature for TTS
        top_p (float): Top-p nucleus sampling
        top_k (int): Top-k sampling
        sample_rate (int): Output audio sample rate
    
    Returns:
        tuple:
            - bytes: Final concatenated WAV audio as raw bytes
            - list[dict]: List of timestamps per sentence chunk
    """

    # Validate speaker
    if speaker not in speakers:
        raise ValueError(f"Speaker '{speaker}' not found. Available: {list(speakers.keys())}")

    speaker_wav = speakers[speaker]

    # Split text into smaller chunks to avoid model limits
    chunks = split_sentences(text)
    audio_segments = []
    timestamps = []
    start_time = 0.0
    total_size_mb = 0

    for chunk in chunks:
        # Rough memory estimate (heuristic)
        estimated_mb = len(chunk) * 0.088
        total_size_mb += estimated_mb

        try:
            # Generate raw waveform
            wav_array = tts.tts(
                text=chunk,
                speaker_wav=speaker_wav,
                language=language,
                temperature=temperature,
                top_p=top_p,
                top_k=top_k
            )
            buffer = encode_wav(wav_array, sample_rate=sample_rate)
            segment = AudioSegment.from_file(buffer, format="wav")

        except Exception as e:
            # Fallback: use file-based TTS if in-memory fails
            print(f"[WARN] tts() failed ({e}), fallback to tts_to_file()")
            with tempfile.NamedTemporaryFile(delete=False, suffix=".wav") as tmp:
                tts.tts_to_file(
                    text=chunk,
                    file_path=tmp.name,
                    speaker_wav=speaker_wav,
                    language=language,
                    temperature=temperature,
                    top_p=top_p,
                    top_k=top_k
                )
                segment = AudioSegment.from_wav(tmp.name)
                os.remove(tmp.name)

        # Track timestamps for each chunk
        end_time = start_time + len(segment) / 1000.0
        timestamps.append({
            "text": chunk,
            "start": start_time,
            "end": end_time,
            "duration": end_time - start_time
        })
        audio_segments.append(segment)
        start_time = end_time

    # Concatenate all segments into final audio
    final_audio = sum(audio_segments)
    final_audio = final_audio.set_channels(1).set_frame_rate(sample_rate)

    # Export audio to memory buffer
    output_buffer = io.BytesIO()
    final_audio.export(output_buffer, format="wav")

    return output_buffer.getvalue(), timestamps
