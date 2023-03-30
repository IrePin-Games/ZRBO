using OpenAi.Json;

using System;
using System.Collections.Generic;

namespace OpenAi.Api.V1
{
    /// <summary>
    /// Object used when requesting a completion. <see href="https://beta.openai.com/docs/api-reference/create-completion"/>
    /// </summary>
    public class ChatCompletionRequestV1 : AModelV1
    {
        /// <summary>
        /// The messages to generate chat completions for, in the chat format.
        /// </summary>
        public ChatMessageV1[] messages;
        
        /// <summary>
        /// ID of the model to use. Currently, only gpt-3.5-turbo and gpt-3.5-turbo-0301 are supported.
        /// </summary>
        public string model;


        /// <inheritdoc />
        public override void FromJson(JsonObject json)
        {
            if (json.Type != EJsonType.Object) throw new OpenAiApiException("Deserialization failed, provided json is not an object");

            foreach(JsonObject obj in json.NestedValues)
            {
                switch (obj.Name) 
                {
                    case nameof(messages):
                        messages = ArrayFromJson<ChatMessageV1>(obj);
                        break;
                    case nameof(model):
                        model = obj.StringValue;
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override string ToJson()
        {
            JsonBuilder jb = new JsonBuilder();

            jb.StartObject();
            jb.AddArray(nameof(messages), messages);
            jb.Add(nameof(model), model);
            jb.EndObject();

            return jb.ToString();
        }
    }
}
