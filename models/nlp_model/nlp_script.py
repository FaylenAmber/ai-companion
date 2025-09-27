from transformers import AutoTokenizer, AutoModelForCausalLM
import torch
import re

def init_qwen_model():
    """
    Initialize model and tokenizer.
    By default, it loads the 4B Chat variant on CPU with float16 precision.

    Note:
        - Change the model path if you want to use another variant.
        - Example model selection is also shown in README.md.
    """
    model_name = "SET_MODEL_NAME"  # Replace with actual model name (see README.md)

    tokenizer = AutoTokenizer.from_pretrained(model_name)
    model = AutoModelForCausalLM.from_pretrained(
        model_name,
        device_map="cpu",            # Change to "auto" if you have GPU support
        torch_dtype=torch.float16    # Use float16 for memory efficiency
    )

    print("Model loaded successfully.")
    return tokenizer, model

def clean_response(text: str) -> str:
    """
    Clean model output by removing unwanted special tokens.

    Args:
        text (str): Raw model output

    Returns:
        str: Cleaned text response
    """
    text = re.sub(r"<\|.*?\|>", "", text)
    text = text.replace("|", "")
    return text.strip()

def generate_response(tokenizer, model, prompt: str) -> str:
    """
    Generate a response from the Qwen model given a user prompt.
    Uses a predefined system instruction to role-play as 'Lucy',
    a cheerful and cute assistant.

    Args:
        tokenizer: Hugging Face tokenizer
        model: Hugging Face CausalLM model
        prompt (str): User input text

    Returns:
        str: Model's generated response
    """
    chat_prompt = (
        "<|im_start|>system\n"
        "You are Yuki, a sweet and cute female assistant who loves to joke and often "
        "uses adorable emoticons like (>w<), (≧◡≦), etc. Respond with a lighthearted, "
        "expressive, and cheerful tone.<|im_end|>\n"
        "<|im_start|>user\n"
        f"{prompt}<|im_end|>\n"
        "<|im_start|>assistant\n"
    )

    # Tokenize input prompt
    inputs = tokenizer(chat_prompt, return_tensors="pt").to("cpu")

    # Generate response with sampling
    with torch.no_grad():
        outputs = model.generate(
            **inputs,
            max_new_tokens=200,
            do_sample=True,
            temperature=0.8,
            top_k=50,
            top_p=0.95,
            pad_token_id=tokenizer.eos_token_id,
        )

    # Decode output and extract assistant response
    full_text = tokenizer.decode(outputs[0], skip_special_tokens=False)
    if "<|im_start|>assistant" in full_text:
        response = full_text.split("<|im_start|>assistant")[-1]
    else:
        response = full_text

    return clean_response(response)

def main():
    """
    Run an interactive CLI loop for chatting with the Qwen model.
    Type 'exit' to quit the program.
    """
    tokenizer, model = init_qwen_model()

    while True:
        prompt = input("\nEnter your prompt (type 'exit' to quit):\n> ")
        if prompt.lower() == "exit":
            break

        response = generate_response(tokenizer, model, prompt)
        print(f"\nAI Response:\n{response}")

if __name__ == "__main__":
    main()
