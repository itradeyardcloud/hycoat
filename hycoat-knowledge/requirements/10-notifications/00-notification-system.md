# 10-notifications/00-notification-system

## Feature ID
`10-notifications/00-notification-system`

## Feature Name
Notification System — In-App + PWA Push (API + UI)

## Dependencies
- `00-foundation/02-auth-system` — User identity for targeting
- `00-foundation/04-pwa-setup` — Service worker for push notifications
- `00-foundation/05-app-shell-layout` — Bell icon in AppBar
- `00-foundation/01-database-schema` — Notification entity

## Business Context
The ERP needs to notify users of key events across the order lifecycle. Notifications serve two purposes:
1. **Operational triggers** — Alert the next department when their action is needed (e.g., "Material received, PPC please schedule" or "QA approved, ready for dispatch")
2. **Alerts & reminders** — Low stock, overdue work orders, pending approvals

Delivery channels:
- **In-app bell** — Real-time via SignalR, badge count in AppBar
- **PWA push** — Browser push notifications for mobile/tablet users on the floor
- **Email** — For critical notifications (dispatch, discrepancy) — optional/future

**Workflow Reference:** WORKFLOWS.md → Workflow 9 — Customer Communication Events.

---

## Entity (from 01-database-schema)

### Notification
```
Models/Notification.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| UserId | string | FK → AppUser, Required | Target user |
| Title | string | Required, MaxLength(200) | |
| Message | string | Required, MaxLength(1000) | |
| Type | string | Required, MaxLength(30) | "Info", "Warning", "Alert", "Success" |
| Category | string | Required, MaxLength(50) | "MaterialInward", "QAApproval", "Dispatch", "LowStock", etc. |
| ReferenceId | int? | | Entity ID for navigation |
| ReferenceType | string? | MaxLength(50) | "WorkOrder", "MaterialInward", "FinalInspection", etc. |
| IsRead | bool | Default false | |
| ReadAt | DateTime? | | |
| CreatedAt | DateTime | | |

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/notifications` | All auth'd | List (current user) |
| GET | `/api/notifications/unread-count` | All auth'd | Badge count |
| PATCH | `/api/notifications/{id}/read` | All auth'd | Mark as read |
| POST | `/api/notifications/read-all` | All auth'd | Mark all as read |
| DELETE | `/api/notifications/{id}` | All auth'd | Delete (own only) |
| POST | `/api/notifications/subscribe` | All auth'd | Register push subscription |
| DELETE | `/api/notifications/subscribe` | All auth'd | Unregister push |

**Query Parameters:**
- `isRead` — true/false filter
- `category` — filter by category
- `page`, `pageSize`

---

## DTOs

### NotificationDto
```csharp
public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public string Category { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### PushSubscriptionDto
```csharp
public class PushSubscriptionDto
{
    public string Endpoint { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
}
```

---

## SignalR Hub

```csharp
// Hubs/NotificationHub.cs
public class NotificationHub : Hub
{
    // Client connects with JWT → auto-joins user-specific group
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }
}

// Service sends notification:
await _hubContext.Clients.Group($"user-{userId}")
    .SendAsync("ReceiveNotification", notificationDto);
```

**Client-side:**
```javascript
// services/signalr.js
const connection = new HubConnectionBuilder()
    .withUrl('/hubs/notifications', { accessTokenFactory: () => getToken() })
    .withAutomaticReconnect()
    .build();

connection.on('ReceiveNotification', (notification) => {
    // Update Zustand store → badge count + toast
    useNotificationStore.getState().addNotification(notification);
    toast(notification.title);
});
```

---

## Notification Triggers

| Event | Who Gets Notified | Category | Type |
|---|---|---|---|
| New Inquiry created | Sales team | Inquiry | Info |
| PI sent to customer | Sales Leader | Sales | Info |
| Work Order confirmed | PPC team, SCM team | WorkOrder | Success |
| Material received (Inward) | PPC team, QA team | MaterialInward | Info |
| Discrepancy found (Inward) | Sales team, Customer(future) | Discrepancy | Warning |
| Incoming inspection complete | PPC team | QAIncoming | Info |
| PWO created | Production team, QA team | PWO | Info |
| Production started | QA team | Production | Info |
| In-process inspection FAIL | Production team, PPC Leader | QAFailure | Alert |
| Final inspection — Approved | SCM team | QAApproval | Success |
| Final inspection — Rejected | Production team, PPC | QARejection | Alert |
| Dispatch completed | Sales team, Finance team | Dispatch | Success |
| Low powder stock | Purchase team | LowStock | Warning |
| Overdue Work Order | PPC Leader, Admin | Overdue | Alert |
| Invoice finalized | Finance Leader | Invoice | Info |

---

## Notification Service

```csharp
// Services/Notifications/INotificationService.cs
public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string message,
        string type, string category, int? referenceId = null,
        string? referenceType = null);
    Task NotifyRoleAsync(string role, string title, string message,
        string type, string category, int? referenceId = null,
        string? referenceType = null);
    Task NotifyDepartmentAsync(string department, string title, string message,
        string type, string category, int? referenceId = null,
        string? referenceType = null);
}
```

The service:
1. Creates Notification record in DB
2. Sends via SignalR to connected user(s)
3. Sends PWA push notification (if user has subscription)

Other services call `INotificationService` at appropriate points (e.g., after creating a MaterialInward, call `NotifyDepartmentAsync("PPC", ...)`).

---

## PWA Push Notifications

```csharp
// Services/Notifications/PushNotificationService.cs
// Uses WebPush NuGet package
public async Task SendPushAsync(string userId, string title, string body)
{
    var subscriptions = await _db.PushSubscriptions
        .Where(s => s.UserId == userId)
        .ToListAsync();

    foreach (var sub in subscriptions)
    {
        var pushSubscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
        var vapidDetails = new VapidDetails(_config["Vapid:Subject"],
            _config["Vapid:PublicKey"], _config["Vapid:PrivateKey"]);
        await _webPushClient.SendNotificationAsync(pushSubscription,
            JsonSerializer.Serialize(new { title, body }), vapidDetails);
    }
}
```

**Additional entity for push subscriptions:**
```
PushSubscription — UserId, Endpoint, P256dh, Auth, CreatedAt
```

---

## UI Components

### NotificationBell (in AppBar)

```
┌──────────────────────────────────────────────┐
│ [☰ HyCoat ERP]              [🔔 3] [Avatar] │
└──────────────────────────────────────────────┘
                                 │
                                 ▼ (click)
┌──────────────────────────────────────┐
│ Notifications          [Mark All Read]│
├──────────────────────────────────────┤
│ 🟡 Material received - WO-2025-018  │
│    PPC team, schedule production     │
│    2 min ago                         │
├──────────────────────────────────────┤
│ 🟢 QA Approved - WO-2025-017        │
│    Ready for dispatch                │
│    15 min ago                        │
├──────────────────────────────────────┤
│ 🔴 Low Stock: RAL 7035 Grey         │
│    Only 8.5 kg remaining             │
│    1 hr ago                          │
├──────────────────────────────────────┤
│ [View All Notifications →]           │
└──────────────────────────────────────┘
```

### NotificationsPage (`/notifications`)
Full-page list with filters (read/unread, category), pagination.
Click notification → navigate to referenced entity.

**Key behaviors:**
- Badge shows unread count (updated in real-time via SignalR)
- Clicking a notification marks it as read and navigates to the referenced entity
- Toast notification shown when new notification arrives
- "Mark All Read" clears the badge
- Push notification permission requested on first login (via PWA service worker)
- Notification type determines icon color: Info=blue, Warning=yellow, Alert=red, Success=green
- Navigation: `ReferenceType` + `ReferenceId` → route mapping (e.g., "WorkOrder" + 5 → `/sales/work-orders/5`)

---

## Business Rules
1. Notifications are per-user (not shared)
2. NotifyDepartmentAsync sends to all users in that department
3. NotifyRoleAsync sends to all users with that role
4. SignalR connection authenticated with JWT
5. Push subscriptions stored per user per device (multiple devices allowed)
6. Old notifications auto-cleaned after 90 days (background job or manual)
7. Notification click navigates to referenced entity
8. Maximum 500 unread notifications per user (oldest auto-read)

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/NotificationsController.cs` | CRUD + subscribe |
| `Hubs/NotificationHub.cs` | SignalR hub |
| `Services/Notifications/INotificationService.cs` + impl | Core service |
| `Services/Notifications/PushNotificationService.cs` | WebPush |
| `Models/PushSubscription.cs` | Entity |
| `DTOs/Notifications/NotificationDto.cs` | |
| `DTOs/Notifications/PushSubscriptionDto.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/components/notifications/NotificationBell.jsx` | AppBar bell + dropdown |
| `src/pages/notifications/NotificationsPage.jsx` | Full list |
| `src/hooks/useNotifications.js` | React Query hooks |
| `src/services/notificationService.js` | API calls |
| `src/services/signalr.js` | SignalR connection manager |
| `src/stores/notificationStore.js` | Zustand store for real-time state |
| `src/utils/notificationNavigator.js` | ReferenceType → route mapping |

### NuGet Packages
```xml
<PackageReference Include="WebPush" Version="2.*" />
```

### Config (appsettings.json)
```json
{
  "Vapid": {
    "Subject": "mailto:admin@hycoat.com",
    "PublicKey": "<generated>",
    "PrivateKey": "<generated>"
  }
}
```

## Acceptance Criteria
1. Bell icon shows unread count badge, updated in real-time
2. Dropdown shows last 10 notifications with type icons
3. Click notification → mark read + navigate to entity
4. "Mark All Read" works
5. Full notifications page with filters and pagination
6. SignalR delivers notifications in real-time
7. PWA push notification shown when app is in background
8. Push subscription register/unregister works
9. Notification triggers fire from other services (integration points documented)
10. Toast shown on new notification arrival

## Reference
- **WORKFLOWS.md:** Workflow 9 — Customer Communication Events
- **01-database-schema.md:** Notification entity (+ new PushSubscription)
