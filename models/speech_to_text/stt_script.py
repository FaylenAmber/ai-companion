import sounddevice as sd
import numpy as np
import keyboard
import queue
import threading
from faster_whisper import WhisperModel

SAMPLE_RATE = 16000
CHANNELS = 1
BLOCK_SIZE = 1024
MODEL_NAME = "medium"
DEVICE = "cpu"
DTYPE = np.float32

print(f"Loading Whisper model: {MODEL_NAME} ...")
model = WhisperModel(MODEL_NAME, device=DEVICE)

audio_queue = queue.Queue()
recording = threading.Event()

def audio_callback(indata, frames, time, status):
    if status:
        print(status)
    if recording.is_set():
        audio_queue.put(indata.copy())

def record_loop():
    recorded = []
    while recording.is_set() or not audio_queue.empty():
        try:
            chunk = audio_queue.get(timeout=0.1)
            recorded.append(chunk)
        except queue.Empty:
            pass
    if recorded:
        audio_data = np.concatenate(recorded, axis=0)
        return audio_data
    return None

def transcribe_audio(audio_data: np.ndarray) -> str:
    audio_data = audio_data.reshape(-1)
    print("[Transcribe] Processing audio...")

    segments, info = model.transcribe(audio_data, beam_size=5, language="id")
    print(f"[Transcribe] Detected language: {info.language} (prob={info.language_probability:.2f})")

    full_text = ""
    for seg in segments:
        print(f"[{seg.start:.2f}s -> {seg.end:.2f}s] {seg.text}")
        full_text += seg.text + " "
    result = full_text.strip()
    print(f"[Result] {result}")
    return result

def stt():
    with sd.InputStream(samplerate=SAMPLE_RATE,
                        channels=CHANNELS,
                        blocksize=BLOCK_SIZE,
                        dtype=DTYPE,
                        callback=audio_callback):
        print("Press and hold Right Shift to record, release to stop and transcribe. (ESC to quit)")
        while True:
            if keyboard.is_pressed("right shift") and not recording.is_set():
                print("[Recording] Started...")
                recording.set()

            elif not keyboard.is_pressed("right shift") and recording.is_set():
                print("[Recording] Stopped.")
                recording.clear()
                audio_data = record_loop()
                if audio_data is not None:
                    text = transcribe_audio(audio_data)
                    yield text

            if keyboard.is_pressed("esc"):
                print("Exiting...")
                break

if __name__ == "__main__":
    for result in stt():
        print(f"[Main Received] {result}")