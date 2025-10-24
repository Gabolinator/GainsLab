using System;

namespace GainsLab.Models.Core.Descriptor;

// OUTDATED: Legacy CreationInfo records retained for later refactor.

// public record CreationInfo(string? CreatedBy, Identifier Identifier ,DateTime? CreatedAt, DateTime? UpdatedAt = null)
// {
//     public bool IsEmpty() => string.IsNullOrWhiteSpace(CreatedBy) && Identifier.IsEmpty();
//
//  
//
//     public int Id
//     {
//         get => Identifier.DbID ?? -1;
//
//         set => Identifier.DbID = value;
//     }
//     
//     public CreationInfo Copy()
//     {
//         return new CreationInfo(CreatedBy, Identifier, CreatedAt, UpdatedAt);
//     }
//     
//     public override string ToString()
//     {
//         var created = CreatedAt?.ToString("s") ?? "unknown";
//         var updated = UpdatedAt?.ToString("s") ?? "never";
//         return $"Created by: {CreatedBy ?? "unknown"}, At: {created}, Updated: {updated}"
//                + (Identifier != null ? $", ID: {Identifier}" : "");
//     }
// }
//
// public record EmptyCreationInfo() : CreationInfo(null, null, null)
// {
//     public override string ToString() => "CreationInfo: (empty)";
// }

