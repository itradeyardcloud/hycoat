# 00-foundation/02-auth-system

## Feature ID
`00-foundation/02-auth-system`

## Feature Name
Authentication & Authorization System

## Dependencies
- `00-foundation/00-api-restructuring` — project structure, packages
- `00-foundation/01-database-schema` — AppUser, AppRole entities, Identity tables

## Business Context
HyCoat Systems has 7 departments with 3 access levels. The auth system provides:
- **JWT-based authentication** with refresh tokens (suited for PWA/mobile)
- **Role hierarchy:** Admin > Leader > Department User
- **Department-based claims** for fine-grained access control
- User management (Admin only)

---

## Database Entities
Uses entities from `01-database-schema`:
- `AppUser` (extends IdentityUser) — FullName, Department, IsActive, RefreshToken, RefreshTokenExpiryTime
- `AppRole` (extends IdentityRole) — Description

## API Endpoints

### AuthController (`/api/auth`)

| Method | Route | Description | Auth Required | Roles |
|---|---|---|---|---|
| POST | `/api/auth/login` | Login with email + password | No | — |
| POST | `/api/auth/refresh-token` | Get new access token using refresh token | No | — |
| POST | `/api/auth/logout` | Invalidate refresh token | Yes | Any |
| POST | `/api/auth/change-password` | Change own password | Yes | Any |

### UsersController (`/api/users`)

| Method | Route | Description | Auth Required | Roles |
|---|---|---|---|---|
| GET | `/api/users` | List all users (paginated) | Yes | Admin |
| GET | `/api/users/{id}` | Get user details | Yes | Admin |
| POST | `/api/users` | Create new user | Yes | Admin |
| PUT | `/api/users/{id}` | Update user | Yes | Admin |
| PUT | `/api/users/{id}/toggle-active` | Activate/deactivate user | Yes | Admin |
| GET | `/api/users/me` | Get current user profile | Yes | Any |
| PUT | `/api/users/me` | Update own profile (name, phone) | Yes | Any |
| GET | `/api/users/roles` | List available roles | Yes | Admin |
| GET | `/api/users/departments` | List departments | Yes | Admin |

### Request/Response DTOs

#### LoginRequest
```csharp
namespace HycoatApi.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

#### LoginResponse
```csharp
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public UserDto User { get; set; } = null!;
}
```

#### UserDto
```csharp
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
```

#### CreateUserRequest
```csharp
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;  // Sales, PPC, SCM, Production, QA, Purchase, Finance
    public string Role { get; set; } = string.Empty;         // Admin, Leader, User
    public string? PhoneNumber { get; set; }
}
```

#### RefreshTokenRequest
```csharp
public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

#### ChangePasswordRequest
```csharp
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
```

## Services

### AuthService
```
Services/AuthService.cs
```
Responsibilities:
1. **Login** — Validate credentials, generate JWT + refresh token, return tokens + user info
2. **RefreshToken** — Validate refresh token, generate new JWT, optionally rotate refresh token
3. **Logout** — Clear refresh token from database
4. **ChangePassword** — Validate current password, update to new
5. **GenerateJWT** — Create token with claims: UserId, Email, FullName, Role, Department
6. **GenerateRefreshToken** — Cryptographically random string, store in AppUser

### JWT Token Claims
```
Claims in the JWT:
- sub (Subject): User.Id
- email: User.Email
- name: User.FullName
- role: Role name (Admin / Leader / User)
- department: User.Department (Sales / PPC / SCM / Production / QA / Purchase / Finance)
- jti (JWT ID): unique token identifier
- exp: expiration time
- iss: "HycoatApi"
- aud: "HycoatApp"
```

## Program.cs Additions

Add Identity and JWT configuration:
```csharp
// Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Register AuthService
builder.Services.AddScoped<AuthService>();

// Add Authentication & Authorization middleware (in pipeline)
app.UseAuthentication();
app.UseAuthorization();
```

### Helpers/ClaimsHelper.cs
```csharp
namespace HycoatApi.Helpers;

public static class ClaimsHelper
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetEmail(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Email)?.Value;

    public static string? GetRole(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value;

    public static string? GetDepartment(this ClaimsPrincipal user)
        => user.FindFirst("department")?.Value;

    public static string? GetFullName(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Name)?.Value;

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.GetRole() == "Admin";

    public static bool IsLeader(this ClaimsPrincipal user)
        => user.GetRole() == "Leader" || user.IsAdmin();
}
```

## Business Rules

### Role Permissions Matrix

| Action | Admin | Leader | User |
|---|---|---|---|
| View all departments' data | ✅ | ✅ (read only) | ❌ |
| CRUD own department | ✅ | ✅ | ✅ |
| CRUD other departments | ✅ | ❌ | ❌ |
| User management | ✅ | ❌ | ❌ |
| View dashboards | ✅ (all) | ✅ (cross-dept) | ✅ (own dept) |
| View reports | ✅ (all) | ✅ (all) | ✅ (own dept) |
| System configuration | ✅ | ❌ | ❌ |

### Department Access
Each department user can only access modules belonging to their department:
- **Sales:** Inquiry, Quotation, PI, Work Order
- **PPC:** Production Work Orders, Production Schedule
- **SCM:** Material Inward, Dispatch
- **Production:** Pretreatment Logs, Production Logs
- **QA:** Incoming Inspection, In-Process Inspection, Final Inspection, Test Certificates
- **Purchase:** Powder Indent, Purchase Orders, GRN
- **Finance:** Invoices, Reports

> **Note:** Leaders and Admin can access all modules. Work Order is visible to all departments (read).

### Validation Rules
1. Email must be unique and valid format
2. Password: min 8 chars, at least 1 uppercase, 1 lowercase, 1 digit
3. Department must be one of: Sales, PPC, SCM, Production, QA, Purchase, Finance
4. Role must be one of: Admin, Leader, User
5. Deactivated users cannot log in
6. Refresh token rotation: old token invalidated when new one is issued

## Acceptance Criteria
1. `POST /api/auth/login` with valid credentials returns JWT access token + refresh token + user details
2. `POST /api/auth/login` with invalid credentials returns 401
3. `POST /api/auth/login` for deactivated user returns 401 with "Account is deactivated" message
4. Protected endpoints return 401 without token and 403 for insufficient role
5. JWT contains correct claims: sub, email, name, role, department
6. Access token expires in 15 minutes (configurable)
7. `POST /api/auth/refresh-token` with valid refresh token returns new access token
8. `POST /api/auth/refresh-token` with expired/invalid refresh token returns 401
9. `POST /api/auth/logout` clears the refresh token from database
10. `POST /api/auth/change-password` with correct current password updates password
11. `GET /api/users` (Admin only) lists all users with pagination
12. `POST /api/users` (Admin only) creates a new user with specified role and department
13. `PUT /api/users/{id}/toggle-active` toggles user active status
14. `GET /api/users/me` returns current user's profile
15. Seeded admin user (admin@hycoat.com / Admin@123) can log in after migration

## Reference
- **WORKFLOWS.md:** "Departments & Roles" table
- **README.md (requirements):** "Authentication & Authorization" section — role hierarchy, departments, auth flow
