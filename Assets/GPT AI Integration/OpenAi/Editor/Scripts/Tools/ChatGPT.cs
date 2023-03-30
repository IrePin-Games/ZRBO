using OpenAi.Api.V1;
using OpenAi.Unity.V1;

using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace OpenAi.Examples
{
    public class ChatGPT : EditorWindow
    {
        private List<ChatMessageV1> messages = new List<ChatMessageV1>(); // Used as the chat history for context. If it exceeds the amount of tokens allowed, you'll get an error.
        private string _input = "Write your request here. For example:" + Environment.NewLine + "\"Explain what a Scriptable Object is and how to use it\" or" + Environment.NewLine + "\"Write a C# script that rotates a gameObject counter-clockwise over time\".";
        Vector2 scrollPos = Vector2.zero;
        private string _output;

        // Keep track of tokens and conversation cost
        private int _totalTokensUsed = 0;
        private float _totalConversationCost = 0.0f;

        private bool _showSettings = false;
        [Range(1, 32768), Tooltip("The maximum number of tokens to generate. Requests can use up to 4000 tokens shared between prompt and completion. (One token is roughly 4 characters for normal English text)")]
        public int max_tokens = 2048;
        [Range(0.0f, 1.0f), Tooltip("Controls randomness: Lowering results in less random completions. As the temperature approaches zero, the model will become deterministic and repetitive.")]
        public float temperature = 0.2f;
        [Range(0.0f, 1.0f), Tooltip("Controls diversity via nucleus sampling: 0.5 means half of all likelihood-weighted options are considered.")]
        public float top_p = 0.8f;
        [Tooltip("Where the API will stop generating further tokens. The returned text will not contain the stop sequence.")]
        public string stop;
        [Range(0.0f, 2.0f), Tooltip("How much to penalize new tokens based on their existing frequency in the text so far. Decreases the model's likelihood to repeat the same line verbatim.")]
        public float frequency_penalty = 0;
        [Range(0.0f, 2.0f), Tooltip("How much to penalize new tokens based on whether they appear in the text so far. Increases the model's likelihood to talk about new topics.")]
        public float presences_penalty = 0;
        [Tooltip("Prompts the AI to only respond with code. Use this if you plan on saving the output to a file.")]
        private bool _outputCodeOnly = false;
        [Tooltip("Prompts the AI to add comments to the code it writes to clarify what it's doing.")]
        private bool _addCommentsToCode = false;
        [Tooltip("The model to use for completion. See https://beta.openai.com/docs/models for a list of available models.")]
        private string model = "gpt-3.5-turbo";
        private string instructions = "I want you to act as a senior Unity game developer. Your task is to assist me in the development of a game using the Unity game engine. You will be responsible for providing assistance with game design, coding, and debugging. You should have experience in developing games with Unity, and be able to create a game with a high level of quality using the best clean coding practices. Here is your first task: ";
        private bool _showInstructionPrompt;
        private bool _saveMessageToMemory = true; // Only adds messages and resposnes to the AI's memory if this is true.

        [MenuItem("Tools/OpenAi/ChatGPT")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ChatGPT));
        }

        void OnGUI()
        {
            // Update GUI on main thread
            
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Code:", MonoScript.FromScriptableObject(this), typeof(ScriptableObject), false);
            GUI.enabled = true;

            SOAuthArgsV1 auth = AssetDatabase.LoadAssetAtPath<SOAuthArgsV1>("Assets/GPT AI Integration/OpenAi/Runtime/Config/DefaultAuthArgsV1.asset");
            OpenAiApiV1 api = new OpenAiApiV1(auth.ResolveAuth());

            GUIStyle chatGPTStyle = new GUIStyle(EditorStyles.textArea);
            chatGPTStyle.fontSize = 16;
            chatGPTStyle.wordWrap = true;
            chatGPTStyle.normal.textColor = new Color(0.737f, 0.749f, 0.780f, 1f);
            chatGPTStyle.padding = new RectOffset(10, 10, 10, 10);
            chatGPTStyle.margin = new RectOffset(0, 0, 5, 0);
            chatGPTStyle.normal.background = MakeTex(1, 1, new Color(0.267f, 0.271f, 0.325f, 1f));

            if (!string.IsNullOrEmpty(_output))
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                _output = EditorGUILayout.TextArea(_output.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\t", "\t"), chatGPTStyle, GUILayout.ExpandHeight(true));
                _output = Regex.Replace(_output, @"`{3}", "");
                EditorGUILayout.EndScrollView();
            }
            
            // Style for the input field
            GUIStyle inputStyle = new GUIStyle(EditorStyles.textField);
            inputStyle.normal.background = MakeTex(1, 1, new Color(0.204f, 0.208f, 0.255f, 1f));

            // Input field
            string previousInput = _input;
            _input = EditorGUILayout.TextArea(_input, inputStyle, GUILayout.MinHeight(80));
            inputStyle.wordWrap = true;

            // Check if the input has changed and update the token count if needed
            if (previousInput != _input)
            {
                // Force a repaint of the editor window
                Repaint();
            }

            _saveMessageToMemory = EditorGUILayout.Toggle("Save message to memory", _saveMessageToMemory);

            if (api != null && GUILayout.Button("Send"))
            {
                #pragma warning disable CS4014 // we don't need to await this, so we're just disbaling the warning message
                DoEditorTask(api);
                #pragma warning restore CS4014
            }
            
            // Calculate total tokens in messages array and update the token count
            int totalMessageTokens = CalculateTotalMessageTokens();
            int tokenCount = CountTokens(_input) + totalMessageTokens;
            EditorGUILayout.LabelField($"Estimated token cost: {tokenCount}");

            // Display the total tokens used below the input field

            EditorGUILayout.LabelField($"Estimated total tokens used: {_totalTokensUsed + totalMessageTokens}");

            // Calculate and display the total conversation cost
            _totalConversationCost = CalculateConversationCost(_totalTokensUsed, model);
            EditorGUILayout.LabelField($"Total conversation cost: ${_totalConversationCost:F5}");

            

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Save as C# file"))
            {
                // Use a regular expression to extract the class name from the output string
                Regex regex = new Regex(@"class\s+(\w+)");
                Match match = regex.Match(_output);
                string className = match.Groups[1].Value;

                // Prompt the user to enter a file name
                string defaultName = className + ".cs";
                string filePath = EditorUtility.SaveFilePanel("Save as C# file", "Assets/Scripts", defaultName, "cs");
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                // Create the Scripts folder if it doesn't exist
                string folderPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Create the C# file
                File.WriteAllText(filePath, _output);

                // Refresh the AssetDatabase to show the new file
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Settings"))
            {
                _showSettings = !_showSettings;
            }

            if (_showSettings)
            {
                model = EditorGUILayout.TextField("Model", model);
                max_tokens = EditorGUILayout.IntSlider("Max Tokens", max_tokens, 1, 4000);
                temperature = EditorGUILayout.Slider("Temperature", temperature, 0.0f, 1.0f);
                top_p = EditorGUILayout.Slider("Top P", top_p, 0.0f, 1.0f);
                stop = EditorGUILayout.TextField("Stop Sequence", stop);
                frequency_penalty = EditorGUILayout.Slider("Frequency Penalty", frequency_penalty, 0.0f, 2.0f);
                presences_penalty = EditorGUILayout.Slider("Presences Penalty", presences_penalty, 0.0f, 2.0f);
                _outputCodeOnly = EditorGUILayout.Toggle("Output Code Only", _outputCodeOnly);
                if (_outputCodeOnly)
                {
                    _addCommentsToCode = EditorGUILayout.Toggle("Add Comments to Code", _addCommentsToCode);
                } else {
                    temperature = 0.2f;
                    top_p = 0.8f;
                }
                if (GUILayout.Button("Instruction Prompt"))
                {
                    // Toggle the visibility of the TextArea
                    _showInstructionPrompt = !_showInstructionPrompt;
                }

                if (_showInstructionPrompt)
                {
                    // Display the TextArea
                    instructions = EditorGUILayout.TextArea(instructions, EditorStyles.textArea, GUILayout.MinHeight(80));
                }
            }
        }

        private async Task DoEditorTask(OpenAiApiV1 api)
        {
            _output = "AI is thinking...";

            if (_outputCodeOnly)
            {
                if (_addCommentsToCode)
                {
                    _input = _input + ". Only respond with Unity code in your response, no additional text or context, Also add comments to the code:";
                }
                else
                {
                    _input = _input + ". Only respond with Unity code in your response, no additional text or context:";
                }
                instructions = "// " + instructions;
                //model = "gpt-3.5-turbo";
                temperature = 0.0f;
                top_p = 1.0f;
            }
            ApiResult<CompletionV1> comp = null;
            ApiResult<ChatCompletionV1> chatComp = null;

            // The GPT models use a different format of request than others like Davinci, so if we're using a GPT model we need to call a different type of request
            if (!model.Contains("gpt"))
            {
                // Use this method when making a request to models other than GPT
                comp = await api.Engines.Engine(model).Completions.CreateCompletionAsync(
                    new CompletionRequestV1()
                    {
                        prompt = instructions + _input,
                        max_tokens = max_tokens,
                        temperature = temperature,
                        top_p = top_p,
                        stop = stop,
                        frequency_penalty = frequency_penalty,
                        presence_penalty = presences_penalty

                    }
                );
            } else {
                chatComp = await SendChatGPTRequest(_input);
            }
            _input = "Enter your next message";
            if ((comp?.IsSuccess ?? false) || (chatComp?.IsSuccess ?? false))
            {
                if (!model.Contains("gpt")){
                Debug.Log($"{comp.Result.choices[0].text}");
                } else {
                Debug.Log($"{chatComp.Result.choices[0].message.content}");
                }

                if (_outputCodeOnly)
                {
                    if (!model.Contains("gpt")){
                    _output = "\nusing UnityEngine;" + $"{comp.Result.choices[0].text}";
                    } else {
                    _output = $"{chatComp.Result.choices[0].message.content}";
                    }
                }
                else
                {
                    if (!model.Contains("gpt")){
                    _output = $"{comp.Result.choices[0].text}";
                    } else {
                    _output = $"{chatComp.Result.choices[0].message.content}";
                    }
                }
            }
            else
            {
                string errorMessage = $"ERROR: StatusCode={chatComp.HttpResponse.responseCode} - {chatComp.HttpResponse.error}";
                if (errorMessage.Contains("Unauthorized"))
                {
                    errorMessage += "\nThis error means your API key is incorrect or improperly setup. Make sure there are no <> symbols in your API key and you've followed the steps in the included ReadMe file.";
                }
                else if (errorMessage.Contains("Too Many Requests"))
                {
                    errorMessage += "\nThis error likely means your OpenAI account has no billing information setup. Verify you've setup billing info and haven't hit any account limits and try again. If that doesn't work, try increasing your 'Max Tokens' setting to a higher value.";
                }

                _output = errorMessage;
            }
        }

        // Use this method when making a request to the GPT3.5 or GPT4 models
        public async Task<ApiResult<ChatCompletionV1>> SendChatGPTRequest(string message)
        {
            SOAuthArgsV1 auth = AssetDatabase.LoadAssetAtPath<SOAuthArgsV1>("Assets/GPT AI Integration/OpenAi/Runtime/Config/DefaultAuthArgsV1.asset");
            OpenAiApiV1 api = new OpenAiApiV1(auth.ResolveAuth());

            

            // Add the user's message to the existing messages list
            messages.Add(new ChatMessageV1() { role = "user", content = message });

            // Check if the total tokens in the messages array exceed max_tokens
            while (CalculateTotalMessageTokens() > max_tokens)
            {
                // Remove the first item in the messages array
                messages.RemoveAt(0);
            }

            // Send OpenAI API request
            ApiResult<ChatCompletionV1> chatComp = await api.ChatCompletions
                .CreateChatCompletionAsync(
                    new ChatCompletionRequestV1()
                    {
                        model = model,
                        messages = messages.ToArray() // Pass the chat log to the API request
                    });

            // Add the assistant's response to the messages list only if _saveMessageToMemory is true
            if (_saveMessageToMemory)
            {
                messages.Add(new ChatMessageV1() { role = "assistant", content = chatComp.Result.choices[0].message.content });
            }

            // Remove the users message from the chat log if _saveMessageToMemory is false
            if (!_saveMessageToMemory)
            {
                    messages.RemoveAt(messages.Count - 1); // Remove the last item
            }

            // Update the total tokens used
            _totalTokensUsed = CalculateTotalMessageTokens();

            return chatComp;
        }

        // Create a solid color texture
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private int CountTokens(string text)
        {
            // You can use OpenAI's tiktoken Python library to get a precise token count,
            // but for simplicity, we'll use an approximation (1 token ≈ 4 characters)
            return text.Length / 4;
        }

        // Calculates the conversation cost
        private float CalculateConversationCost(int tokens, string model)
        {
            float costPerToken = 0.03f; // Default cost per token in USD

            if (model.Contains("gpt-3"))
            {
                costPerToken = 0.002f;
            }
            else if (model.Contains("gpt-4-32k"))
            {
                costPerToken = 0.06f;
            }

            return (tokens / 1000.0f) * costPerToken;
        }

        private int CalculateTotalMessageTokens()
        {
            // If chat log is empty, add the system instructions message
            if (messages.Count == 0)
            {
                messages.Add(new ChatMessageV1() { role = "system", content = instructions });
            }
            
            int totalTokens = 0;

            foreach (ChatMessageV1 message in messages)
            {
                totalTokens += CountTokens(message.content);
            }

            return totalTokens;
        }
    }
}