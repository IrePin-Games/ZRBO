using OpenAi.Api.V1;
using OpenAi.Unity.V1;

using System.Threading.Tasks;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace OpenAi.Examples
{
    public class PromptGenerator : EditorWindow
    {
        private string _input = "Senior Unity Developer";
        Vector2 scrollPos = Vector2.zero;
        private string _output;

        public int max_tokens = 1024;
        public float temperature = 0.7f;
        public float top_p = 1.0f;
        public string stop;
        public float frequency_penalty = 0;
        public float presences_penalty = 0;
        public string model = "gpt-3.5-turbo";
        private string instructions = "I want you to act as a prompt generator. Firstly, I will give you a title like this: “Act as a Senior Unity Game Developer”. The output I expect from you should look like what I put here in quotes: “I want you to act as a senior Unity game developer. Your task is to assist me in the development of a game using the Unity game engine. You will be responsible for providing assistance with game design, coding, and debugging. You should have experience in developing games with Unity, and be able to create a game with a high level of quality using the best clean coding practices. Here is your first task: ” (You should adapt the sample prompt according to the title I give. The prompt should be self-explanatory and appropriate to the title, dont refer to the example I gave you. in the quotes, only make a response structred in a similar way, based off of the title I give you). ";
        private bool _showInstructionPrompt;

        [MenuItem("Tools/OpenAi/Prompt Generator")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(PromptGenerator));
        }

        async void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Code:", MonoScript.FromScriptableObject(this), typeof(ScriptableObject), false);
            GUI.enabled = true;

            SOAuthArgsV1 auth = AssetDatabase.LoadAssetAtPath<SOAuthArgsV1>("Assets/GPT AI Integration/OpenAi/Runtime/Config/DefaultAuthArgsV1.asset");
            OpenAiApiV1 api = new OpenAiApiV1(auth.ResolveAuth());
            if (!string.IsNullOrEmpty(_output))
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                _output = EditorGUILayout.TextArea(_output.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\t", "\t"), GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();                
            }
            
            EditorGUILayout.LabelField("Act as a... ");
            _input = EditorGUILayout.TextField(_input, EditorStyles.textField);
            EditorStyles.textField.wordWrap = true;

            if (api != null && GUILayout.Button("Generate Instruction Prompt"))
            {
                Debug.Log("Performing Completion in Editor Time using the following input:");
                await DoEditorTask(api);
            }

        }

        private async Task DoEditorTask(OpenAiApiV1 api)
        {
            ApiResult<ChatCompletionV1> comp = null;
            _output = "Generating your prompt...";
            comp = await SendChatGPTRequest(_input);
            if (comp.IsSuccess)
            {
                _output = $"{comp.Result.choices[0].message.content}" + " Respond only as if you were this character.";
            }
            else
            {
                _output = $"ERROR: StatusCode={comp.HttpResponse.responseCode} - {comp.HttpResponse.error}";
            }
        }

        public async Task<ApiResult<ChatCompletionV1>> SendChatGPTRequest(string message)
        {
            SOAuthArgsV1 auth = AssetDatabase.LoadAssetAtPath<SOAuthArgsV1>("Assets/GPT AI Integration/OpenAi/Runtime/Config/DefaultAuthArgsV1.asset");
            OpenAiApiV1 api = new OpenAiApiV1(auth.ResolveAuth());
            ApiResult<ChatCompletionV1> comp = await api.ChatCompletions
                .CreateChatCompletionAsync(
                    new ChatCompletionRequestV1()
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]
                        {
                            new ChatMessageV1()
                            {
                                role = "system",
                                content = instructions
                            },
                            new ChatMessageV1()
                            {
                                role = "user",
                                content = message
                            }
                        }
                    });

            return comp;
        }
    }
}