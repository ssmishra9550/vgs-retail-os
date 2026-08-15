# ADR-005: Authentication Architecture (Membership Multi-Tenancy, ASP.NET Core Identity, JWT & Refresh Token Rotation)

## Status
Accepted

## Date
2026-08-15

## Context
VGS Retail OS is a multi-tenant retail operating system serving independent enterprise retailers across physical stores, online clients, mobile devices, and offline POS terminals. Designing a secure, scalable, multi-tenant authentication system requires balancing developer productivity, operational simplicity, and enterprise-grade security.

Without clear architectural decisions, authentication systems risk coupling domain logic to identity frameworks, leaking tenant data through spoofed request headers, exposing tokens to XSS/CSRF attacks in Single Page Applications (SPAs), or depending on brittle session state. This ADR establishes the mandatory authentication architecture for VGS Retail OS.

---

## Decision

### 1. Architectural Style & Canonical Dependency Flow
Authentication follows the approved **Modular Monolith Layered Architecture** with strict boundary encapsulation.

The canonical dependency and execution direction for the Auth module is:

```
[ API Layer ]         Controllers, HTTP DTOs, Routing, Versioning
      ↓
[ IBL Layer ]         Business Layer Interfaces (IAuthBL)
      ↓
[ BL Layer ]          Business Logic & Workflows (AuthBL)
      ↓
[ BO Layer ]          Pure Domain Business Objects (UserBO, RefreshTokenBO)
      ↓
[ IDAC Layer ]        Data Access Component Interfaces (IAuthDAC)
      ↓
[ DAC Layer ]         Persistence Implementation (AuthDAC, ApplicationUser)
      ↓
[ Database / Infra ]  PostgreSQL 17 (EF Core 10) / Redis 7
```

Canonical dependency direction:  
`API → IBL → BL → BO → IDAC → DAC → EF Core 10 → PostgreSQL 17`

---

### 2. BO vs. Identity / EF Entity Separation
Domain Business Objects (`BO`) and infrastructure/persistence entities are strictly decoupled:

- **Domain Business Objects (`BO`)**: Pure C# POCOs (e.g., `UserBO`, `RoleBO`, `RefreshTokenBO`). They must remain completely framework-independent and MUST NOT inherit from `IdentityUser`, `IdentityRole`, EF Core base classes, or ASP.NET Core types.
- **Persistence Entities (`DAC`)**: Identity and EF Core persistence types are confined entirely inside the `DAC` / persistence boundary:
  - `ApplicationUser : IdentityUser<Guid>`
  - `ApplicationRole : IdentityRole<Guid>`
  - `RefreshTokenEntity`
- **Mapping Boundary**: `AuthDAC` maps between `ApplicationUser` / `RefreshTokenEntity` and `UserBO`. `AuthBL` operates strictly on domain `UserBO`. `AuthController` maps `UserBO` to public API Request/Response DTOs.
- **Mandatory Equivalence Rule**:  
  $$\text{BO} \neq \text{Identity Entity} \neq \text{EF Entity} \neq \text{API DTO}$$

---

### 3. Membership-Oriented Multi-Tenancy
VGS Retail OS rejects a rigid, single-owner `Tenant -> Organization -> Store -> User` hierarchy in favor of a flexible **Membership Model**:

```
Tenant
  ├── Organizations
  │      └── Stores
  │
  └── Users

User
  ↓
TenantMembership
  ↓
OrganizationMembership
  ↓
StoreMembership
```

- **User Scopes**: A user may hold memberships across multiple tenants, organizations, or stores with varying roles and permission scopes.
- **DefaultStoreId**: `DefaultStoreId` represents a user operational default preference, **NOT** an authorization boundary.
- **Tenant Context Distinction**:
  - `Authentication`: Verifies user identity (Who are you?).
  - `Authorization`: Verifies user permissions for a requested action (What can you do?).
  - `Tenant Context`: Enforces database query boundary isolation via `ITenantContext` and EF Core Global Query Filters.
  - `Active Store Context`: Currently selected store for POS and inventory transactions.

---

### 4. TenantId Security Rules
- **Client Hint Rule**: A client-provided `TenantId` (via `X-Tenant-ID` header, host/subdomain, or request payload) is **ONLY a routing/lookup hint**. It MUST NEVER be treated as proof of authorization.
- **Required Authentication & Resolution Flow**:
  $$\text{Tenant Hint} \rightarrow \text{Resolve Tenant} \rightarrow \text{Find Candidate User} \rightarrow \text{Verify Credentials} \rightarrow \text{Verify Tenant Membership} \rightarrow \text{Verify Active Status} \rightarrow \text{Create Authenticated Context}$$
- **Server Enforcement**: For all authenticated requests, tenant authorization is validated against server-side membership records. Signed JWT claims contain `tenant_id`, but authorization handlers enforce valid tenant membership and scope server-side.

---

### 5. JWT Access-Token Strategy
- **Token Format**: Short-lived JSON Web Token (JWT), signed using RSA-256 (production) or HMAC-SHA256 (512-bit key for dev/staging).
- **Access Token Lifetime**: 15 minutes.
- **Claims Payload**:
  - `sub`: User ID (`Guid`)
  - `tenant_id`: Active Tenant ID (`Guid`)
  - `org_id`: Active Organization ID (`Guid`)
  - `store_id`: Currently Selected Store ID (`Guid`)
  - `jti`: Unique Token Identifier (`Guid`)
  - `sec_stamp`: Security Stamp (`Guid`)
  - `roles`: Active assigned roles list
  - `permissions`: Active permission codes list
- **Claim Staleness & Revocation**: Claims embedded in short-lived JWTs can become stale before token expiration. Security is maintained through short 15-minute TTLs combined with `SecurityStamp` validation. Any security-sensitive change (password reset, permission revocation, admin lock) updates `ApplicationUser.SecurityStamp`, instantly invalidating active sessions upon token refresh or security stamp checks.

---

### 6. Refresh-Token Strategy & Rotation (RTR)
- **Token Format**: 256-bit cryptographically secure random opaque token (`RandomNumberGenerator.GetBytes(64)`).
- **Lifetime**: 7-day sliding window.
- **Refresh Token Rotation (RTR)**: Every call to `/api/v1/auth/refresh` invalidates the presented refresh token and issues a new `(AccessToken, RefreshToken)` pair.
- **Token Family & Reuse Detection**: Each refresh token is assigned a `FamilyId`. If an already-consumed or invalid refresh token is presented (indicating a replay attack or token theft), the entire `FamilyId` is revoked immediately, forcing re-authentication across all associated devices.
- **Persistence**: Stored persistently in PostgreSQL 17 (`user_refresh_tokens` table) via EF Core 10 `AuthDAC`.

---

### 7. Session & Device Strategy
- **Multi-Device Support**: Users can hold concurrent active sessions across multiple devices (e.g., POS terminal 1, Manager tablet, Web portal).
- **Session Tracking**: Tracked via `FamilyId` associated with device metadata (`DeviceName`, `UserAgent`, `IpAddress`, `LastUsedAt`).
- **Remote Revocation**: Users or administrators can list active devices (`GET /api/v1/auth/sessions`) and terminate specific sessions or revoke all active sessions (`DELETE /api/v1/auth/sessions/{sessionId}`).

---

### 8. Database & Cache Responsibilities (PostgreSQL vs. Redis)
- **PostgreSQL 17 (System-of-Record)**: Authoritative source of truth for identity, users, credentials, refresh tokens, memberships, and security state.
- **Redis 7 (Acceleration & Revocation Layer)**: Ephemeral cache, revocation index, and rate-limiting support.
- **Redis Non-Dependence & Resiliency**:
  - Redis MUST NOT be the source of truth for identity data.
  - If Redis is unavailable, crashed, or flushed, authentication requests fall back gracefully to PostgreSQL 17 without identity data loss or application crashes.
  - Not every authentication request is hard-blocked by Redis.

---

### 9. Password Security Baseline
- **Hasher Implementation**: ASP.NET Core Identity standard `IPasswordHasher<ApplicationUser>` (PBKDF2 with HMAC-SHA512).
- **Rationale**: Built-in framework integration, mature battle-tested security implementation, automatic algorithm versioning and re-hashing support upon user login. Custom password hashing implementations are explicitly forbidden.
- **Account Lockout Policy**: 5 consecutive failed login attempts triggers a 15-minute account lockout (`LockoutEnd`).
- **Password Policy**: Minimum 12 characters, requiring uppercase, lowercase, numeric, and special characters.

---

### 10. Angular SPA Token Storage & CSRF Protection
- **Access Token**: Stored **in-memory only** inside Angular `AuthService` state. Never stored in `localStorage` or `sessionStorage` to eliminate XSS token theft.
- **Refresh Token**: Stored in an `HttpOnly; Secure; SameSite=Strict` cookie managed automatically by the browser.
- **CSRF Protection**: Cookie-authenticated state-changing operations (`POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`) enforce Anti-CSRF protection via custom request headers (`X-XSRF-TOKEN` / `X-Requested-With`) to block Cross-Site Request Forgery.

---

### 11. Authentication vs. Authorization Separation
- **Authentication**: Establishes WHO the user is and WHICH tenant/store context is active.
- **Authorization**: Evaluates WHAT the user is permitted to do within that context.
- **RBAC Readiness**: Prepares ASP.NET Core Policy-Based Authorization (`[Authorize(Policy = "Permission.Inventory.Read")]`) driven by claims baked into the access token.

---

### 12. Security Risks & Mitigations

| Risk | Mitigation |
| :--- | :--- |
| **Tenant Claim Spoofing** | Client `TenantId` hint is unverified. Server verifies user tenant membership during credential check. |
| **XSS Token Theft** | Access token stored in SPA memory; Refresh token locked in `HttpOnly` cookie. |
| **Refresh Token Replay** | Strict Refresh Token Rotation (RTR) + Token Family Revocation upon double-use. |
| **Brute-Force Attacks** | Redis-backed IP rate limiting + ASP.NET Core Identity account lockout (5 failed attempts). |

---

### 13. Deferred Authentication Capabilities
The following advanced capabilities are explicitly deferred from the initial baseline:
- Full RBAC management endpoints (`/api/v1/roles`, `/api/v1/permissions`) — *Scheduled for TASK-018*.
- Multi-Factor Authentication (MFA / TOTP) UI and verification flows.
- External OAuth2 / OIDC / Social identity providers (Google, Microsoft, SAML).
- Self-service password reset email delivery service.
- Biometric / POS hardware key authentication.

---

### 14. TASK-014 Implementation Scope
TASK-014 will implement the baseline authentication module:
1. **Auth Business Module Layout**: `backend/src/Modules/VGS.RetailOS.Modules.Auth/` following `API -> IBL -> BL -> BO -> IDAC -> DAC`.
2. **Entities & Schemas**: `ApplicationUser`, `ApplicationRole`, `RefreshTokenEntity` in `DAC`; `UserBO`, `RefreshTokenBO` in `BO`.
3. **Services**:
   - `IAuthBL` / `AuthBL`: Credential validation, login, refresh token rotation, revocation/logout.
   - `IAuthDAC` / `AuthDAC`: EF Core persistence operations against PostgreSQL 17.
   - `IJwtTokenGenerator` / `JwtTokenGenerator`: Signed JWT creation.
4. **Endpoints**:
   - `POST /api/v1/auth/login`
   - `POST /api/v1/auth/refresh`
   - `POST /api/v1/auth/logout`
   - `GET /api/v1/auth/me`
5. **Frontend Integration**: Angular 22 `AuthService`, `AuthGuard`, `AuthInterceptor`, and Login component UI.
6. **Tests**: Unit, Integration, API, and Security test coverage.

---

## Consequences

### Benefits
- **Clean Architecture & Separation**: Decouples domain logic (`UserBO`) from ASP.NET Core Identity and EF Core persistence classes (`ApplicationUser`).
- **XSS/CSRF Resilient**: In-memory access tokens and `HttpOnly; SameSite=Strict` cookies prevent script token theft.
- **Strict Multi-Tenant Safety**: Eliminates tenant spoofing by requiring server-side membership verification.
- **Resilient Infrastructure**: PostgreSQL 17 is the single system-of-record; Redis failures do not cause identity data loss.

### Trade-offs
- **Mapping Overhead**: Requires explicit mapping between API DTOs, `UserBO`, and `ApplicationUser`.
- **Short Token Lifetime Overhead**: 15-minute access token lifespan requires Angular SPA to seamlessly invoke refresh token rotation in the background.

---

## Related Documentation
- `docs/decisions/ADR-001-dotnet-version.md`
- `docs/decisions/ADR-002-angular-frontend-stack.md`
- `docs/decisions/ADR-003-database-data-access-strategy.md`
- `docs/decisions/ADR-004-vgs-coding-and-layered-module-architecture.md`
- `ARCHITECTURE.md`
- `PROJECT_RULES.md`
