using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GainsLab.Core.Models.Core.Utilities;

namespace GainsLab.Infrastructure.DB.Outbox;


/// <summary>
/// Enumerates the mutation types captured in the outbox table.
/// </summary>
public enum ChangeType { Insert, Update, Delete }


/// <summary>
/// Represents a single outbox record queued for upstream dispatch.
/// </summary>
public class OutboxChangeDto
{
    [Key]
    public long Id { get; set; } // identity
    public string Entity { get; set; } = default!;  // e.g., "User" based on enum EntityType
    public Guid EntityGuid { get; set; }
    public ChangeType ChangeType { get; set; }
    public string PayloadJson { get; set; } = default!; // full entity snapshot or minimal delta
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Sent { get; set; }
    
    public override string ToString()
    {
        return $"{nameof(OutboxChangeDto)} - " +
               $"ID : {Id} | " +
               $"Entity Type {Entity} |"+
               $"EntityGuid :{EntityGuid} |"+
               $"ChangeType :{ChangeType} ";
    }

    /// <summary>
    /// Performs lightweight validation to ensure an outbox change can be dispatched safely.
    /// </summary>
    public bool IsValid() => TryValidate(out _);

    /// <summary>
    /// Validates the current record and returns the failure reason when invalid.
    /// </summary>
    public bool TryValidate(out string? failureReason)
    {
        if (EntityGuid == Guid.Empty)
        {
            failureReason = "EntityGuid is empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Entity))
        {
            failureReason = "Entity name is missing.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(PayloadJson))
        {
            failureReason = "Payload is empty.";
            return false;
        }

        try
        {
            CoreUtilities.Logger.Log(nameof(OutboxChangeDto), $"Validating payload : {PayloadJson}");
            
            using var doc = JsonDocument.Parse(PayloadJson);
            var root = doc.RootElement;

            // if (!TryReadInt32(root, "Iid", out var iid))
            // {
            //     failureReason = "Payload is missing an Iid value.";
            //     return false;
            // }
            //
            // var requiresPersistedId = ChangeType is ChangeType.Update or ChangeType.Delete;
            // if (requiresPersistedId && iid <= 0)
            // {
            //     failureReason = "Payload is missing a positive Iid for update/delete changes.";
            //     return false;
            // }
            //
            if (!TryReadGuid(root, "GUID", out var payloadGuid))
            {
                failureReason = "Payload is missing a valid GUID.";
                return false;
            }

            if (payloadGuid != EntityGuid)
            {
                failureReason = "Payload guid does not match the outbox EntityGuid.";
                return false;
            }
        }
        catch (JsonException)
        {
            failureReason = "PayloadJson is not valid JSON.";
            return false;
        }

        failureReason = null;
        return true;
    }

    public int RankOf()
    {
        this.TryResolveEntityType(out var type);
        return type.RankOf();
    }

    private static bool TryReadInt32(JsonElement root, string propertyName, out int value)
    {
        value = 0;
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out value))
        {
            return false;
        }

        return true;
    }

    private static bool TryReadGuid(JsonElement root, string propertyName, out Guid guid)
    {
        guid = Guid.Empty;
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var raw = property.GetString();
        return Guid.TryParse(raw, out guid) && guid != Guid.Empty;
    }
}
