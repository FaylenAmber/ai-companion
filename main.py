import asyncio
import websockets
import json
import struct

from models.nlp_model.nlp_script import generate_response
from models.text_to_speech.tts_script import tts_xtts_v2
from models.emotion_classification.emotions_classify import estimate_emotion_segments
from models.speech_to_text.stt_script import stt

# Keep track of connected Unity clients
connected_clients = set()

INPUT_MODE = "text"  # default

# WebSocket handler for each connected client
async def handle_client(websocket, path):
    """
    Called whenever a Unity client connects.
    Adds the client to the `connected_clients` set.
    Removes the client when the connection is closed.
    """
    print(f"[SERVER] Unity connected: {websocket.remote_address}")
    connected_clients.add(websocket)
    try:
        # Wait until the client disconnects
        await websocket.wait_closed()
    finally:
        # Remove client from active connections
        connected_clients.remove(websocket)
        print(f"[SERVER] Unity disconnected: {websocket.remote_address}")

# Start the WebSocket server
async def start_websocket_server():
    """
    Opens a WebSocket server on port 8765.
    Unity clients can connect via ws://127.0.0.1:8765.
    """
    server = await websockets.serve(handle_client, "0.0.0.0", 8765)
    print("[SERVER] Listening on ws://127.0.0.1:8765")
    return server

# Send TTS audio and emotion data to Unity
async def send_audio_and_emotion(lines: list[dict], audio_bytes: bytes, subtitle: str):
    """
    Sends a packet to all connected Unity clients containing:
        - JSON (emotion lines + subtitle)
        - Audio bytes

    Packet structure:
        [json_length (4 bytes)] + [json_payload] + [audio_bytes]
    """
    if not connected_clients:
        print("[WARN] No Unity clients connected yet.")
        return

    # Build JSON payload
    json_obj = {
        "lines": lines,      # emotion-annotated text lines
        "subtitle": subtitle # subtitle text
    }

    json_str = json.dumps(json_obj).encode("utf-8")
    json_len = len(json_str)

    # Binary format: 4-byte JSON length + JSON + audio
    packet = struct.pack("<I", json_len) + json_str + audio_bytes

    # Broadcast to all connected clients
    for client in connected_clients.copy():
        try:
            await client.send(packet)
            print("[INFO] Audio & emotion lines sent to Unity.")
        except Exception as e:
            print(f"[ERROR] Failed to send to Unity: {e}")

def get_voice_input():
    """
    Runs the STT generator and returns the first transcription result.
    Exits immediately when ESC is pressed inside stt().
    """
    for text in stt():   # ambil hasil pertama
        return text
    print("ESC pressed, exiting input loop...")
    return "exit"

# Main user input loop
async def input_loop(default_speaker="speaker"):
    """
    Handles user interaction via terminal:
        1. Receive user input (question).
        2. Use NLP model to generate AI response.
        3. Convert AI response to audio via TTS.
        4. Run emotion classification on the response.
        5. Align emotions with TTS timestamps.
        6. Send results (audio + subtitle + emotions) to Unity.
    """
    loop = asyncio.get_running_loop()

    while True:
        # Blocking input -> run in executor to keep async loop alive
        if INPUT_MODE == "text":
            user_input = await loop.run_in_executor(None, input, "\n[EN] Insert Your Question (type 'exit' to quit):\n> ")
        else:  # voice mode
            user_input = await loop.run_in_executor(None, get_voice_input)

        if not user_input:
            continue
        if user_input.strip().lower() == "exit":
            break

        # Generate AI response
        ai_response = await loop.run_in_executor(None, generate_response, user_input)
        print(f"[AI Response]: {ai_response}")

        # Run TTS and emotion analysis in parallel
        tts_task = loop.run_in_executor(None, tts_xtts_v2, ai_response, default_speaker, "en")
        emotion_task = loop.run_in_executor(None, estimate_emotion_segments, ai_response)

        (audio_bytes, timestamps), emotion_lines = await asyncio.gather(tts_task, emotion_task)
        subtitle = ai_response

        # Merge emotion results with TTS timestamps
        for e in emotion_lines:
            t = next((t for t in timestamps if t['text'] == e['text']), None)
            if t:
                e['start'] = t['start']
                e['end'] = t['end']
                e['duration'] = t['end'] - t['start']
            else:
                e['start'] = 0
                e['end'] = 0
                e['duration'] = 0

        # Debug logs
        print("[Emotion]", emotion_lines)
        print("[Timestamps]", timestamps)

        # Send to Unity
        await send_audio_and_emotion(emotion_lines, audio_bytes, subtitle)

# Application entry point
async def main():
    """
    Application entry:
        - Starts the WebSocket server.
        - Starts the input loop for user interaction.
    """
    choose_mode()
    server = await start_websocket_server()
    try:
        await input_loop()
    finally:
        # Clean shutdown of server
        server.close()
        await server.wait_closed()
        print("[SERVER] Closed")

def choose_mode():
    global INPUT_MODE
    print("Choose Input Mode")
    print("1. Text")
    print("2. Voice")
    choice = input("Select mode: ").strip()
    if choice == "2":
        INPUT_MODE = "voice"
    else:
        INPUT_MODE = "text"
    print(f"[MODE] Using {INPUT_MODE.upper()} mode")

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n[SERVER] Stopped by user")
