using Microsoft.Agents.Core.Models;
using System.Collections.Concurrent;

namespace HelloWorldTeamsAgent.Services;

public sealed class ConversationReferenceStore()
{
    private readonly ConcurrentDictionary<string, ConversationReference> _cache = [];

    public void Save(string conversationId, ConversationReference reference)
    {
        _cache.AddOrUpdate(conversationId, reference, (key, oldValue) => reference);
    }

    public ConversationReference? Get(string conversationId)
    {
        _cache.TryGetValue(conversationId, out var reference);
        return reference;
    }

    public List<ConversationReference> GetActiveConversations()
    {
        return _cache.Values.ToList();
    }

    public void Remove(string conversationId)
    {
        _cache.TryRemove(conversationId, out _);
    }
}