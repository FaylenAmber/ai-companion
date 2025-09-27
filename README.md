# **AI Companion (2D/3D Model Integration)**

This project is a **modular AI Assistant** designed for real-time interactive communication with AI-driven characters.\
It integrates multiple core features:

- **Natural Language Processing (NLP)** – understanding and generating natural conversations.
- **Emotion Classification** – detecting emotional tone from speech or text to reflect expressions on the model.
- **Speech Recognition (STT)** – converting voice input into text.
- **Text-to-Speech (TTS)** – generating natural AI voice responses.
- **WebSocket Server for Unity** – enabling seamless communication between the AI backend and Unity-based clients.

<br>

## **⚠️ Disclaimer**

- This project is for **research and personal use**.
- Check each model’s **license and usage restrictions** before deploying commercially.
- Some models may have additional usage policies.
- This repository does not redistribute any third-party model weights.  
- All external models are downloaded from their official sources (e.g., Hugging Face) under their respective licenses.

<br>

## **✨ Features**

- **Emotion Classification** → Maps fine-grained [GoEmotions](https://github.com/google-research/google-research/tree/master/goemotions) labels to simplified VRM-style emotions.
- **NLP Response (Qwen)** → Role-play AI assistant with customizable personality.
- **Speech Recognition** → Real-time transcription using [Faster-Whisper](https://github.com/guillaumekln/faster-whisper).
- **Text-to-Speech** → Multilingual expressive voice synthesis via [XTTS v2](https://github.com/coqui-ai/TTS).
- **Unity Integration** → WebSocket server to stream audio + subtitles + emotion metadata.

<br>

## **🛠 Installation**

Clone the repository:

```
git clone https://github.com/faylenamber/ai-companion.git
cd ai-companion
```


Install dependencies:
```
pip install -r requirements.txt
```

You may also need:

- [FFmpeg](https://ffmpeg.org/) (for audio processing)
- [PyTorch](https://pytorch.org/get-started/locally/) (for Qwen, Whisper, and XTTS v2)

<br>

## **🚀 Usage**

**1. Emotion Classification**

Detect emotions from text using a Hugging Face model:

Python models\emotion\_classification\emotions\_classify.py

👉 Example Models:

- [logasanjeev/emotions-analyzer-bert](https://huggingface.co/logasanjeev/emotions-analyzer-bert)


⚠️ Note:

You must set the model name manually in **line 5** of emotions\_classify.py.

Default (placeholder):
```python
model_name = "SET_MODEL_NAME"
```

Change it to the model you want to use, for example:
```python
model_name = "logasanjeev/emotions-analyzer-bert"
```

<br>

**2. NLP Response**

Interactive chat with Qwen model:

python models/nlp/nlp\_model.py

👉 Example Models:

- [Qwen/Qwen1.5-4B-Chat](https://huggingface.co/Qwen/Qwen1.5-4B-Chat)
- [Qwen/Qwen2.5-0.5B-Instruct](https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct)

⚠️ Note:

You must set the model name manually in **line 14** of nlp\_model.py.

Default (placeholder):
```python
model_name = "SET_MODEL_NAME"
```

Change it to the model you want to use, for example:
```python
model_name = "Qwen/Qwen1.5-4B-Chat"
```

<br>

**3. Speech Recognition (Whisper)**

Record speech and transcribe (press Right Shift to record, Esc to quit):

python models/stt/whisper\_transcript.py

👉 Example Models:

- [openai/whisper-tiny](https://huggingface.co/openai/whisper-tiny)
- [openai/whisper-medium](https://huggingface.co/openai/whisper-medium)
- [guillaumekln/faster-whisper-medium](https://github.com/guillaumekln/faster-whisper)

<br>

**4. Text-to-Speech (XTTS v2)**

Synthesize voice with multilingual XTTS v2:

👉 Example Models:

- [tts_models/multilingual/multi-dataset/xtts_v2](https://huggingface.co/coqui/XTTS-v2)

**⚠️** Note:

You must set the speaker\_name manually in **line 49** of xtts\_v2.py.

It depand on your speaker’s set up in **line 17** of xtts\_v2:

```python
speakers = {
    "speaker_name": "path/to/sample.wav",
}
```

Default (placeholder):

```python
def tts_output(text, speaker="SET_CHOSEN_SPEAKER_HERE", language="en", memory_limit_mb=50, temperature=0.3, top_p=0.85, top_k=50, sample_rate=24000):
```

<br>

Change "SET\_CHOSEN\_SPEAKER\_HERE" to the speaker you want to choose, for example:

```python
def tts_output(text, speaker="speaker_name", language="en", memory_limit_mb=50, temperature=0.3, top_p=0.85, top_k=50, sample_rate=24000):
```

<br>

**5. Unity Integration**

Run the WebSocket server:

`
python main.py
`

Unity connects to:

`
ws://127.0.0.1:8765
`

Data sent includes:

- Audio (WAV)
- Emotion lines
- Subtitles

<br>

## **📑 Models and Licenses**

|**Model / Library**|**Description**|**License**|
| :- | :- | :- |
|**logasanjeev/emotions-analyzer-bert**|Fine-grained emotion dataset & classification|[MIT](https://huggingface.co/logasanjeev/emotions-analyzer-bert/blob/main/LICENSE)|
|**Hugging Face Transformers**|NLP & model loading framework|[Apache 2.0](https://github.com/huggingface/transformers/blob/main/LICENSE)|
|**Qwen Models**|LLMs for dialogue|[Tongyi Qianwen License](https://huggingface.co/Qwen)|
|**Whisper (OpenAI)**|Speech recognition model|[MIT](https://github.com/openai/whisper/blob/main/LICENSE)|
|**Faster-Whisper (CTranslate2)**|Optimized Whisper inference|[MIT](https://github.com/SYSTRAN/faster-whisper/blob/master/LICENSE)|
|**XTTS v2 (Coqui TTS)**|Multilingual expressive TTS|[CPML](https://huggingface.co/coqui/XTTS-v2/blob/main/LICENSE.txt)|
|**GoEmotions Dataset (Google Research)**|Fine-grained emotion labels dataset used for mapping|[Apache 2.0](https://github.com/google-research/google-research/blob/master/LICENSE)|

---

## 📜 License & Attribution

This repository contains **original code** licensed under the [MIT License](./LICENSE).

It makes use of external models and libraries. In particular:

- **Qwen Models**  
  - Source: [Qwen on Hugging Face](https://huggingface.co/Qwen)  
  - License: Tongyi Qianwen License  
  - Note: Usage may be subject to restrictions. Please review Qwen’s license terms for commercial use.

- **Emotion Classification**

   - This project uses emotion labels inspired by the [GoEmotions dataset](https://github.com/google-research/google-research/tree/master/goemotions) (Apache 2.0 License).  
   - The fine-grained GoEmotions labels are mapped into simplified VRM-style emotions for avatar animation.  
   - Mapping logic is original to this project, but derived from the GoEmotions label set.

- **XTTS-v2**  
  - Source: [Coqui XTTS-v2 on Hugging Face](https://huggingface.co/coqui/XTTS-v2)  
  - Codebase: [coqui-ai/TTS](https://github.com/coqui-ai/TTS)  
  - License: [Coqui Public Model License (CPML)](https://huggingface.co/coqui/XTTS-v2/blob/main/LICENSE.txt)  
  - Note: You may use this project for research and personal purposes. For any **commercial use**, please check Coqui’s licensing terms.

Please ensure compliance with the licenses of all third-party models/libraries before redistributing or deploying this project.


<br>

## **🎤 Using with VTuber Model (VTube Studio)**

You can integrate this project with **VTube Studio** for real-time lip-sync on your VTuber avatar.\
The general setup works as follows:

1. **Enable LipSync in VTube Studio**
   1. In VTube Studio settings, go to *Audio LipSync* and enable it.
   2. This feature makes your VTuber model’s mouth move according to incoming audio input.
1. **Install VB-CABLE (Virtual Audio Device)**
   1. Download: [VB-CABLE Official Website](https://vb-audio.com/Cable/)
   1. VB-CABLE acts as a virtual microphone: audio output from Python → VB-CABLE → VTube Studio input.
1. **Configure Audio Routing**
   1. In your system’s audio settings:
      1. **Output device** → set to VB-CABLE
      1. **Input device (microphone in VTube Studio)** → set to VB-CABLE
   1. This way, the TTS audio generated by the code is routed directly into VTube Studio’s LipSync system.
1. **Run the AI Assistant**
   1. Start the Python project to generate speech.
   1. The output audio will now drive the mouth movement of your VTube Studio avatar.

👉 Reference:
- [VTube Studio Official Website](https://denchisoft.com/)
- [VB-CABLE (Virtual Audio Cable)](https://vb-audio.com/Cable/)

<br>

## **🎤 Using with 3D Models (VRM/FBX) in Unity**

For 3D avatars such as **VRM** or **FBX**, the setup can be achieved inside **Unity** with the help of external plugins and audio routing from Python. The workflow relies on generating speech audio from Python (e.g., via TTS), and then using Unity with lip-sync plugins to animate the model’s mouth movement.

**Unity Environment**

1. **Unity version:** `2022.3.6f1` (recommended LTS build for stability)

<br>

**Plugins Used**

1. **OVR LipSync (Oculus LipSync for Unity)**
   1. Provides real-time viseme and phoneme detection for accurate mouth movement.
   1. Official page: [Oculus LipSync for Unity](https://developer.oculus.com/downloads/package/oculus-lipsync-unity/)
1. **UniVRM**
   1. An open-source Unity plugin for handling VRM models (loading, rendering, rigging).
   1. GitHub repository: [UniVRM](https://github.com/vrm-c/UniVRM)

<br>

**Workflow Overview**

1. **Import the plugins** into Unity project:
   1. Drag and drop the OVR LipSync Unity package.
   1. Import UniVRM (via .unitypackage or directly from GitHub release).
1. **Prepare the 3D model (VRM/FBX):**
   1. For VRM: use UniVRM to load and configure humanoid rig.
   1. For FBX: ensure it’s humanoid-rigged and blendshapes are defined for lip movements.

After importing the required plugins (**OVR LipSync** and **UniVRM**) and preparing your VRM/FBX model, the next step is to create two new GameObjects in the **Hierarchy** to handle lip-sync processing and audio playback.

<br>

**1. LipSync Player GameObject**

This object is responsible for processing lip-sync data from the audio and applying it to the VRM/FBX model’s blendshapes.

**Steps:**

1. In the **Hierarchy**, create a new empty GameObject → rename it to LipSyncPlayer.
1. Add the following components:
   1. **OVR Lip Sync Context**
      1. This will automatically add an **OVRLipSync** script and an **Audio Source**.
      1. In the **OVR Lip Sync Context** component:
         1. Assign the attached **Audio Source** into its field.
      1. The **Audio Source** component itself can remain with default settings (no changes required).
   1. **Lip Sync To VRM/FBX Blend Shape**
      1. This component maps viseme outputs to the model’s blendshapes.
      1. Fill in the fields according to your model’s blendshape settings (e.g., A, I, U, E, O).
   1. **Emotion Controller**
      1. This allows emotion states (happy, sad, angry, neutral, etc.) to be layered on top of lip-sync.
      1. Fill in the fields as needed depending on your avatar’s available blendshapes.

<br>

**2. Playback GameObject**

This object handles audio playback and receiving real-time audio data (from Python or external input).

**Steps:**

1. In the **Hierarchy**, create another empty GameObject → rename it to Playback.
1. Add the following components:
   1. **Audio Source** (manually add)
      1. Leave the fields empty; this component will simply serve as the output for played audio.
   1. **Web Socket Audio Emotion Receiver**
      1. This component receives streamed audio data and emotion metadata (from your Python backend, for example).
      1. Fill in the fields (e.g., WebSocket server URL, emotion mapping) according to your setup.

At this point, you now have:

- **LipSyncPlayer** → processes lip-sync (OVR Lip Sync Context + blendshape + emotion).
- **Playback** → handles incoming audio data and plays it back in the Unity scene.

And now you can test the setup:

1. Start the **Companion** by running `main.py`.
1. Once it is ready, return to Unity and press **`Play`** in the scene.

