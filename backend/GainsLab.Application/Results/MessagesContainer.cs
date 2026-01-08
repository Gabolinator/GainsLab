using System.Collections.Generic;
using System.Collections.ObjectModel;
using GainsLab.Contracts.Interface;

namespace GainsLab.Application.Results;

public enum MessageType
{
    Info,
    Warning,
    Error
}

public sealed class MessagesContainer : IMessagesContainer
{
    private readonly List<string> _messages = new();
    private readonly List<string> _warnings = new();
    private readonly List<string> _errors = new();

    /// <summary>
    /// If true, adding the same message multiple times will keep duplicates.
    /// If false, duplicates are ignored per message type.
    /// </summary>
    public bool AllowDuplicates { get; }

    public MessagesContainer(bool allowDuplicates = true)
    {
        AllowDuplicates = allowDuplicates;
    }

    // --------------------
    // Add single
    // --------------------
    public void Add(MessageType type, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var target = GetList(type);
        if (AllowDuplicates || !target.Contains(message))
            target.Add(message);
    }

    public void AddInfo(string message) => Add(MessageType.Info, message);
    public void AddWarning(string message) => Add(MessageType.Warning, message);
    public void AddError(string message) => Add(MessageType.Error, message);

    // --------------------
    // Add range
    // --------------------
    public void AddRange(MessageType type, IEnumerable<string> messages)
    {
        if (messages is null) return;

        foreach (var msg in messages)
            Add(type, msg);
    }

    public void AddInfoRange(IEnumerable<string> messages) => AddRange(MessageType.Info, messages);
    public void AddWarningRange(IEnumerable<string> messages) => AddRange(MessageType.Warning, messages);
    public void AddErrorRange(IEnumerable<string> messages) => AddRange(MessageType.Error, messages);

    // --------------------
    // Retrieve
    // --------------------
    public IReadOnlyList<string> Get(MessageType type) => new ReadOnlyCollection<string>(GetList(type));
    public IReadOnlyList<string> GetAll()
        => new ReadOnlyCollection<string>(_messages.Concat(_warnings).Concat(_errors).ToList());

    public IReadOnlyList<string> Infos => new ReadOnlyCollection<string>(_messages);
    public IReadOnlyList<string> Warnings => new ReadOnlyCollection<string>(_warnings);
    public IReadOnlyList<string> Errors => new ReadOnlyCollection<string>(_errors);

    public int Count(MessageType type) => GetList(type).Count;
    public int TotalCount => _messages.Count + _warnings.Count + _errors.Count;

    public string? GetAt(MessageType type, int index)
    {
        var list = GetList(type);
        return (index < 0 || index >= list.Count) ? null : list[index];
    }

    // --------------------
    // Clear
    // --------------------
    public void Clear(MessageType type) => GetList(type).Clear();

    public void ClearAll()
    {
        _messages.Clear();
        _warnings.Clear();
        _errors.Clear();
    }

    // --------------------
    // Append/merge
    // --------------------
    public void Append(MessagesContainer other)
    {
        if (other is null) return;

        AddInfoRange(other.Infos);
        AddWarningRange(other.Warnings);
        AddErrorRange(other.Errors);
    }

    // --------------------
    // Convenience
    // --------------------
    public bool HasErrors => _errors.Count > 0;
    public bool HasWarnings => _warnings.Count > 0;
    public bool HasInfos => _messages.Count > 0;

    public override string ToString()
        => string.Join(" | ", GetAll());
    
    public string ToString(MessageType type)
        => string.Join(" | ", GetList(type));

    private List<string> GetList(MessageType type) => type switch
    {
        MessageType.Info => _messages,
        MessageType.Warning => _warnings,
        MessageType.Error => _errors,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown message type.")
    };
}
