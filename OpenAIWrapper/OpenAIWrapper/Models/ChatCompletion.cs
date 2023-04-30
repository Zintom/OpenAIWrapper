﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zintom.OpenAIWrapper.Models;

public sealed class ChatCompletion
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

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
    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    [JsonPropertyName("delta")]
    public Message? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

/// <summary>
/// A message as a part of a conversation.
/// </summary>
public sealed class Message
{
    /// <summary>
    /// The role of the author of this message. One of '<i>system</i>', '<i>user</i>', or '<i>assistant</i>'.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// The contents of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}