using System;
using System.Collections.Generic;
namespace Emby.UserData.Models {
 public enum FieldType { Text, TextArea, Date, Number, Boolean, Select }
 public sealed class FieldDefinition { public string Id { get; set; } = Guid.NewGuid().ToString("N"); public string Name { get; set; } public FieldType Type { get; set; } = FieldType.Text; public bool Required { get; set; } public bool IsSeparator { get; set; } public bool DisableUserOnDate { get; set; } public bool IsActive { get; set; } = true; public int Order { get; set; } public List<string> Options { get; set; } = new List<string>(); }
 public sealed class UserRecord { public string UserId { get; set; } public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>(); }
 public sealed class StoreDocument { public int FormatVersion { get; set; } = 1; public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>(); public Dictionary<string, UserRecord> Users { get; set; } = new Dictionary<string, UserRecord>(); public System.Collections.Generic.HashSet<string> AutoDisabledUserIds { get; set; } = new System.Collections.Generic.HashSet<string>(); }
}
namespace Emby.UserData.Models {
 public sealed class BackupDocument { public int FormatVersion { get; set; } = 1; public string Scope { get; set; } public System.DateTime ExportedAtUtc { get; set; } = System.DateTime.UtcNow; public System.Collections.Generic.List<FieldDefinition> Fields { get; set; } = new System.Collections.Generic.List<FieldDefinition>(); public System.Collections.Generic.Dictionary<string, UserRecord> Users { get; set; } }
}
