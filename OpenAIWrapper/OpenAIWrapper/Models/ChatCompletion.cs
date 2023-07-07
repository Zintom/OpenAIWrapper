﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zintom.OpenAIWrapper.Models;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Represents a models response for a given chat conversation.
/// </summary>
public sealed class ChatCompletion
{
    /// <summary>
    /// The ID of this completion.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    /// <summary>
    /// The unix time when this completion was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// The model used to generate this completion.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice>? Choices { get; set; }
}

public sealed class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public sealed class Choice
{
    /// <summary>
    /// The message in this response.
    /// </summary>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    /// <summary>
    /// The message in this response (when streaming), see <see cref="ChatGPT.GetStreamingChatCompletion(Message[], Action{ChatCompletion?}, ChatGPT.ChatCompletionOptions?, FunctionDefinition[])">GetStreamingChatCompletion</see>.
    /// </summary>
    [JsonPropertyName("delta")]
    public Message? Delta { get; set; }

    /// <summary>
    /// One of: '<i>stop</i>', '<i>length</i>', '<i>function_call</i>', '<i>content_filter</i>', or '<i>null</i>'.
    /// <para/>
    /// <i>stop</i>: API returned complete model output.
    /// <para/>
    /// <i>length</i>: Incomplete model output due to max_tokens parameter or token limit.
    /// <para/>
    /// <i>function_call</i>: The model decided to call a function.
    /// <para/>
    /// <i>content_filter</i>: Omitted content due to a flag from the content filters.
    /// <para/>
    /// <i>null</i>: API response still in progress or incomplete.
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary>
    /// The index of this response in the array of choices.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }
}

/// <summary>
/// A message as a part of a conversation.
/// </summary>
public sealed class Message
{
    /// <summary>
    /// The role of the author of this message. One of '<i>system</i>', '<i>user</i>', '<i>assistant</i>', or '<i>function</i>'.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// The contents of the message. <c>Content</c> is required for all messages except assistant messages with function calls.
    /// </summary>
    [JsonPropertyName("content"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    /// <summary>
    /// The name of the author of this message. Name is required if role is 'function', and it should be the name of the function whose response is in the content.
    /// May contain a-z, A-Z, 0-9, and underscores, with a maximum length of 64 characters.
    /// </summary>
    [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>
    /// The name and arguments of a function that should be called, as generated by the model.
    /// </summary>
    [JsonPropertyName("function_call"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FunctionCall? FunctionCall { get; set; }
}

/// <summary>
/// This is the object returned in a <see cref="ChatCompletion"/> if the model wants to call a function.
/// </summary>
[JsonConverter(typeof(FunctionCallJsonConverter))]
public sealed class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    public List<ArgumentDefinition> Arguments { get; set; } = new();

    public struct ArgumentDefinition
    {
        /// <summary>
        /// The name of the argument.
        /// </summary>
        public required string Name;

        /// <summary>
        /// The type of the argument.
        /// </summary>
        public required ArgumentType Type;

        /// <summary>
        /// The actual argument itself, as an object.
        /// </summary>
        public required object? Value;
    }

    public enum ArgumentType
    {
        /// <summary>
        /// A double precision number.
        /// </summary>
        Number,

        /// <summary>
        /// A string.
        /// </summary>
        String
    }
}

public sealed class FunctionCallJsonConverter : JsonConverter<FunctionCall>
{
    public override FunctionCall? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        FunctionCall functionCall = new();

        // Read PropertyName "name"
        if (!reader.Read() ||
            reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != "name")
        {
            throw new JsonException("PropertyName 'name' expected.");
        }

        // Get 'name' value.
        if (!reader.Read() ||
            reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        functionCall.Name = reader.GetString() ?? "";

        // Read PropertyName "arguments"
        if (!reader.Read() ||
            reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != "arguments")
        {
            throw new JsonException("PropertyName 'arguments' expected.");
        }

        // Get 'arguments' value.
        if (!reader.Read() ||
            reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        string arguments = reader.GetString() ?? "";

        // "arguments" is supplied as it's own JSON object, which this reader cannot read into, so we need to deserialize that separately.

        if (arguments.Length > 0)
        {
            Span<byte> argumentsUtf8Bytes = stackalloc byte[Encoding.UTF8.GetMaxByteCount(arguments.Length)];
            argumentsUtf8Bytes = argumentsUtf8Bytes[..Encoding.UTF8.GetBytes(arguments, argumentsUtf8Bytes)];

            InternalReadArgumentsObject(new Utf8JsonReader(argumentsUtf8Bytes), functionCall);
        }

        // Read EndObject
        if (!reader.Read() ||
            reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException($"Expected {JsonTokenType.EndObject}");

        return functionCall;
    }

    private static void InternalReadArgumentsObject(Utf8JsonReader reader, FunctionCall functionCall)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject ||
                reader.TokenType == JsonTokenType.EndObject)
                continue;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string argName = reader.GetString() ?? "";
                if (!reader.Read())
                {
                    throw new JsonException("Expected argument value.");
                }

                object? argValue = null;
                FunctionCall.ArgumentType argType = 0;

                if (reader.TokenType == JsonTokenType.Number)
                {
                    argValue = reader.GetDouble();
                    argType = FunctionCall.ArgumentType.Number;
                }
                if (reader.TokenType == JsonTokenType.String)
                {
                    argValue = reader.GetString() ?? "";
                    argType = FunctionCall.ArgumentType.String;
                }

                functionCall.Arguments.Add(new FunctionCall.ArgumentDefinition() { Name = argName, Type = argType, Value = argValue });
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, FunctionCall value, JsonSerializerOptions options)
    {
        throw new InvalidOperationException($"Writing a {nameof(FunctionCall)} is not relevant in the ChatCompletions API, you only need to provide the function result.");

        /*
        writer.WriteStartObject();

        writer.WriteString("name", value.Name);

        using (var argumentWriterStream = new MemoryStream())
        using (var argumentWriter = new Utf8JsonWriter(argumentWriterStream, new JsonWriterOptions() { Indented = false }))
        {
            argumentWriter.WriteStartObject();
            foreach (var argument in value.Arguments)
            {
                if (argument.Type == FunctionCall.ArgumentType.Number)
                {
                    argumentWriter.WriteNumber(argument.Name, (double)(argument.Value ?? 0));
                }
                if (argument.Type == FunctionCall.ArgumentType.String)
                {
                    argumentWriter.WriteString(argument.Name, argument.Value?.ToString() ?? "");
                }
            }

            argumentWriter.WriteEndObject();
            argumentWriter.Flush();

            using (var reader = new StreamReader(argumentWriterStream, Encoding.UTF8, leaveOpen: true))
            {
                argumentWriterStream.Position = 0;
                writer.WritePropertyName("arguments");
                writer.WriteRawValue("\"" + reader.ReadToEnd().Replace("\"", "\\\"") + "\"", true);
            }
        }
        writer.WriteEndObject();
        */
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member