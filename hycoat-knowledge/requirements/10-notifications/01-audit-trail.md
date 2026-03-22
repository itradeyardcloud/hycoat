# 10-notifications/01-audit-trail

## Feature ID
`10-notifications/01-audit-trail`

## Feature Name
Audit Trail & File Management (API + UI)

## Dependencies
- `00-foundation/00-api-restructuring` — Middleware pipeline
- `00-foundation/01-database-schema` — AuditLog, FileAttachment entities
- `00-foundation/02-auth-system` — User identity

## Business Context
For regulatory compliance and operational accountability, HyCoat needs:
1. **Audit Trail** — Automatic logging of all create/update/delete operations with old and new values, user identity, and timestamps. This answers "who changed what, when?"
2. **File Management** — Centralized file upload/download service for drawings, photos, PDFs. Files are referenced by many entities (drawings on sections, photos on inward/production, PDFs on invoices/TCs).

---

## Entities

### AuditLog
```
Models/AuditLog.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | long | PK | |
| EntityName | string | Required, MaxLength(100) | e.g., "WorkOrder" |
| EntityId | string | Required, MaxLength(50) | PK of changed entity |
| Action | string | Required, MaxLength(10) | "Create", "Update", "Delete" |
| UserId | string? | FK → AppUser | Who made the change |
| UserName | string? | MaxLength(100) | Snapshot |
| Timestamp | DateTime | Required | |
| OldValues | string? | | JSON of previous state |
| NewValues | string? | | JSON of new state |
| ChangedColumns | string? | | Comma-separated column names |
| IpAddress | string? | MaxLength(45) | |

### FileAttachment
```
Models/FileAttachment.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| FileName | string | Required, MaxLength(300) | Original filename |
| StoredFileName | string | Required, MaxLength(300) | GUID-based on disk |
| ContentType | string | Required, MaxLength(100) | MIME type |
| FileSizeBytes | long | | |
| EntityType | string | Required, MaxLength(100) | "SectionProfile", "MaterialInward", etc. |
| EntityId | int | Required | FK to owning entity |
| Category | string? | MaxLength(50) | "Drawing", "Photo", "PDF", "Document" |
| UploadedByUserId | string? | FK → AppUser | |
| UploadedAt | DateTime | | |

---

## Audit Trail Implementation

### EF Core SaveChanges Interceptor

```csharp
// Data/AuditInterceptor.cs
public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        var context = eventData.Context as AppDbContext;
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog) // Don't audit the audit log
            .ToList();

        foreach (var entry in entries)
        {
            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKeyValue(entry),
                Action = entry.State.ToString(),
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
            };

            if (entry.State == EntityState.Modified)
            {
                var changedProps = entry.Properties
                    .Where(p => p.IsModified)
                    .ToList();
                auditLog.OldValues = JsonSerializer.Serialize(
                    changedProps.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                auditLog.NewValues = JsonSerializer.Serialize(
                    changedProps.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                auditLog.ChangedColumns = string.Join(",",
                    changedProps.Select(p => p.Metadata.Name));
            }
            else if (entry.State == EntityState.Added)
            {
                auditLog.NewValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else if (entry.State == EntityState.Deleted)
            {
                auditLog.OldValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
            }

            context.AuditLogs.Add(auditLog);
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
```

Register in Program.cs:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(new AuditInterceptor()));
```

---

## File Management Service

```csharp
// Services/Files/IFileService.cs
public interface IFileService
{
    Task<FileAttachment> UploadAsync(IFormFile file, string entityType, int entityId, string? category = null);
    Task<(Stream stream, string contentType, string fileName)> DownloadAsync(int fileId);
    Task DeleteAsync(int fileId);
    Task<List<FileAttachmentDto>> GetByEntityAsync(string entityType, int entityId);
}
```

**Storage:**
- Files stored in `wwwroot/uploads/{entityType}/{entityId}/`
- Filename: `{Guid}{extension}` (prevents collisions)
- Original filename preserved in DB for display
- Max file size: 10MB (configurable)
- Allowed types: images (jpg/png/webp), PDF, Excel (xlsx), drawings (dwg/dxf/pdf)

---

## API Endpoints

### Audit Trail
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/audit-logs` | Admin | List all audit logs |
| GET | `/api/audit-logs/entity/{entityName}/{entityId}` | Admin, Leader | History for specific entity |

**Query Parameters:**
- `entityName`, `userId`, `action`
- `dateFrom`, `dateTo`
- `search` — entity name, user name
- `page`, `pageSize`

### File Management
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/files/upload` | All auth'd | Upload file |
| GET | `/api/files/{id}` | All auth'd | Download file |
| DELETE | `/api/files/{id}` | Owner or Admin | Delete file |
| GET | `/api/files/entity/{entityType}/{entityId}` | All auth'd | List files for entity |

---

## DTOs

### AuditLogDto
```csharp
public class AuditLogDto
{
    public long Id { get; set; }
    public string EntityName { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object?>? OldValues { get; set; }
    public Dictionary<string, object?>? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
}
```

### FileAttachmentDto
```csharp
public class FileAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Category { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime UploadedAt { get; set; }
    public string DownloadUrl { get; set; }
}
```

### UploadFileDto (form data)
```
file: IFormFile (multipart)
entityType: string
entityId: int
category: string? (optional)
```

---

## UI Pages

### AuditLogPage (`/admin/audit-logs`) — Admin only

```
┌──────────────────────────────────────────────────────────────┐
│ PageHeader: "Audit Trail"                                    │
│ Filters: [Entity ▼] [User ▼] [Action ▼] [From] [To]        │
├──────────────────────────────────────────────────────────────┤
│ Timestamp        │ User    │ Action │ Entity      │ ID      │
│ 2025-06-16 14:32 │ Ramesh  │ Update │ WorkOrder   │ 18      │
│ 2025-06-16 14:30 │ Priya   │ Create │ Invoice     │ 42      │
│ 2025-06-16 14:28 │ System  │ Update │ PowderStock │ 3       │
├──────────────────────────────────────────────────────────────┤
│ Expand row → Change details:                                 │
│ ┌────────────────┬──────────────┬──────────────┐             │
│ │ Field          │ Old Value    │ New Value    │             │
│ │ Status         │ InProgress   │ Completed    │             │
│ │ CompletedDate  │ null         │ 2025-06-16   │             │
│ └────────────────┴──────────────┴──────────────┘             │
└──────────────────────────────────────────────────────────────┘
```

### EntityAuditPanel (reusable component)
Shown on entity detail pages (e.g., Work Order detail → "History" tab):

```
┌──────────────────────────────────────────────────┐
│ CHANGE HISTORY                                   │
│ 📝 16 Jun 14:32 — Ramesh updated Status          │
│    InProgress → Completed                        │
│ ➕ 12 Jun 09:15 — Priya created this Work Order  │
│ 📝 12 Jun 10:00 — Priya updated CustomerRef      │
│    null → "PO-2025-088"                          │
└──────────────────────────────────────────────────┘
```

### FileAttachmentsPanel (reusable component)
Shown on entity forms/detail pages:

```
┌──────────────────────────────────────────────────┐
│ ATTACHMENTS                     [📎 Upload File] │
│ ┌────────────────────────────────────────┐       │
│ │ 📄 profile-6063T5.pdf  (Drawing)      │ [⬇][🗑]│
│ │ 📷 inward-photo-1.jpg  (Photo)        │ [⬇][🗑]│
│ │ 📄 test-certificate.pdf (PDF)         │ [⬇][🗑]│
│ └────────────────────────────────────────┘       │
└──────────────────────────────────────────────────┘
```

**Key behaviors:**
- Audit logs auto-generated on every SaveChanges (no manual intervention)
- Expandable rows show field-by-field old/new value comparison
- EntityAuditPanel usable on any entity detail page
- File upload with drag-and-drop or click to browse
- Download files with original filename
- File preview for images (lightbox) and PDFs (inline viewer)
- Only file owner or Admin can delete files

---

## Business Rules
1. Audit logging is automatic via EF Core interceptor — no code changes needed in services
2. AuditLog entity itself is not audited (prevents recursion)
3. Soft-deleted entities are logged as "Delete" action
4. File storage uses GUID-based names to prevent path traversal
5. File size limit: 10MB per file (configurable)
6. Allowed file types validated server-side (whitelist approach)
7. Files linked to entities via EntityType + EntityId (polymorphic)
8. Audit logs retained indefinitely (or per retention policy)
9. Only Admin can view the full audit log page
10. Entity-specific history visible to users who can access that entity

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Data/AuditInterceptor.cs` | EF Core SaveChanges interceptor |
| `Controllers/AuditLogsController.cs` | Read-only endpoints |
| `Controllers/FilesController.cs` | Upload/download/delete/list |
| `Services/Audit/IAuditLogService.cs` + impl | Query audit logs |
| `Services/Files/IFileService.cs` + impl | File upload/download/delete |
| `Services/Auth/ICurrentUserService.cs` + impl | Get current user from HttpContext |
| `Models/AuditLog.cs` | Entity |
| `Models/FileAttachment.cs` | Entity |
| `DTOs/Audit/AuditLogDto.cs` | |
| `DTOs/Files/FileAttachmentDto.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/admin/AuditLogPage.jsx` | Full audit log viewer |
| `src/components/shared/EntityAuditPanel.jsx` | Reusable history panel |
| `src/components/shared/FileAttachmentsPanel.jsx` | Reusable file manager |
| `src/hooks/useAuditLogs.js` | React Query hooks |
| `src/hooks/useFiles.js` | React Query hooks |
| `src/services/auditLogService.js` | API calls |
| `src/services/fileService.js` | API calls |

## Acceptance Criteria
1. All create/update/delete operations automatically logged
2. Audit log shows entity name, action, user, timestamp, old/new values
3. Expandable row shows field-by-field change diff
4. Entity-specific history panel reusable across detail pages
5. File upload works (drag-and-drop + browse)
6. Files linked to entities (EntityType + EntityId)
7. Download preserves original filename
8. Image preview in lightbox, PDF preview inline
9. Only Admin sees full audit log page
10. File type and size validation (server-side whitelist)
11. GUID-based storage names prevent path traversal

## Reference
- **01-database-schema.md:** AuditLog, FileAttachment entities
