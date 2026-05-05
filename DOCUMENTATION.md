# ORV Wiki Backend — Full Documentation

**Project:** Omniscient Reader's Viewpoint dedicated wiki backend
**Author:** Medard Duriš
**Stack:** .NET 10 · ASP.NET Core · EF Core 10 · PostgreSQL 16+ · Npgsql · Serilog · SignalR · Scalar/OpenAPI

This document is the single source of truth for understanding the codebase. It is structured so future sessions (human or LLM) can skip re-deriving context.

---

## 1. Architecture at a glance

### Solution layout

```
ORV_Wiki_Backend.sln
├── ORVWiki.API              ← ASP.NET Core host (controllers, DI, middleware, SignalR hub)
├── ORVWiki.Application      ← entities, enums, DTOs, service interfaces + impls, validators
└── ORVWiki.Infrastructure   ← EF Core DbContext, repositories, migrations, Npgsql, BCrypt, JWT
```

### Project reference graph

```
API ──▶ Application
API ──▶ Infrastructure ──▶ Application
```

Application depends on **nothing** from the other two. Infrastructure references Application (to implement its interfaces and reference its entities). API references both (composition root).

### Why this split

- **Application** is the inner core: domain types and use cases. Has no ASP.NET, EF Core, or DB dependencies (except a few abstractions like `IMemoryCache` and `ILogger<T>` from `Microsoft.Extensions.`* — these are stable contracts, not infrastructure).
- **Infrastructure** is the outer adapters: how persistence, password hashing, and JWT signing actually happen.
- **API** is the composition root + HTTP surface: wires DI, defines endpoints, exposes the hub.

### Tech stack (versions pinned in `.csproj` files)


| Concern           | Library                                                                                          |
| ----------------- | ------------------------------------------------------------------------------------------------ |
| ORM               | `Microsoft.EntityFrameworkCore` 10.0.7                                                           |
| Postgres provider | `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1                                                   |
| snake_case naming | `EFCore.NamingConventions` 10.0.1                                                                |
| Password hashing  | `BCrypt.Net-Next` 4.1.0                                                                          |
| JWT               | `System.IdentityModel.Tokens.Jwt` 8.0.1 + `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 |
| Validation        | `FluentValidation` 12.1.1 (+ DI extensions)                                                      |
| Logging           | `Serilog.AspNetCore` 10.0.0                                                                      |
| API docs          | `Microsoft.AspNetCore.OpenApi` 10.0.1 + `Scalar.AspNetCore` 2.14.8                               |
| Real-time         | `Microsoft.AspNetCore.SignalR` (built into `Microsoft.AspNetCore.App`)                           |
| Memory cache      | `Microsoft.Extensions.Caching.Memory` (built in)                                                 |


---

## 2. Domain model (40 tables)

Full schema is documented in `~/Desktop/ORV Wiki/ORV_Wiki_Database_Specifikacia.md` and the dbml at `~/Desktop/ORV Wiki/ORV_Wiki_dbdiagram.dbml`. Quick summary:

### Entity groups

- **A. Beings (7)** — `Character`, `Constellation`, `Nebula`, `Dokkaebi`, `DemonKing`, `OuterGod`, `Worldline`
- **B. Power system (5)** — `Fable`, `Stigma`, `Attribute`, `Skill`, `Item`
- **C. Story & world (6)** — `Location`, `Scenario`, `Arc`, `Chapter`, `Event`, `Concept`
- **D. Community (7)** — `User`, `Role`, `Comment`, `CommentReaction`, `EditSuggestion`, `Bookmark`, `Notification`
- **E. Helpers (3)** — `Page`, `Media`, `Tag`
- **Pivots (11 distinct)** — `CharacterStigma`, `CharacterAttribute`, `CharacterSkill`, `CharacterItem`, `CharacterFable`, `CharacterConstellation`, `EventCharacter`, `ScenarioParticipant`, `ScenarioLocation`, `PageTag`, `EventConnection`. (`CommentReaction` doubles as the Comment↔User pivot.)

### The `Page` pattern

Every encyclopedic entity (17 of them) has a 1:1 with `Page`. Page centralizes:


| Field               | Purpose                                                                    |
| ------------------- | -------------------------------------------------------------------------- |
| `slug`              | URL identifier (unique)                                                    |
| `title`             | Display name                                                               |
| `entity_type`       | enum discriminator                                                         |
| `discovery_chapter` | spoiler gate (page only visible if `discovery_chapter <= current_chapter`) |
| `spoiler_map`       | jsonb (legacy, unused since Phase 4 inline parser took over)               |
| `view_count`        | counter                                                                    |
| timestamps          | `created_at`, `updated_at`                                                 |


Then each entity table holds its specific fields and `page_id` (FK, unique → enforces 1:1).

### Naming convention

C# entities are PascalCase. EF Core's `UseSnakeCaseNamingConvention()` translates to snake_case at the database layer:

- `Character` → `characters` (pluralised by EF default)
- `PageId` → `page_id`
- `CharacterStatus.MainSponsor` enum → `main_sponsor` PostgreSQL enum value

### `Attribute` naming gotcha

`Attribute` clashes with `System.Attribute`. The entity is named `Attribute` but `AppDbContext` and `IAppDbContext` use a `using AttributeEntity = ORVWiki.Application.Entities.Attribute;` alias to disambiguate. **If you add code that references the Attribute entity, expect to need the same alias.**

---

## 3. Layer-by-layer file map

### `ORVWiki.Application/`

```
Auth/
  AuthService.cs                 ← register, login, get user, update chapter/role
  IAuthService.cs
  IPasswordHasher.cs             ← contract (impl in Infrastructure)
  IJwtTokenGenerator.cs          ← contract (impl in Infrastructure)
  Roles.cs                       ← string constants: "admin", "editor", "reader"
  Dtos/                          ← register/login requests, AuthResponse, UserDto
  Validators/                    ← FluentValidation rules

Bookmarks/
  BookmarkService.cs             ← toggle + list mine
  IBookmarkRepository.cs
  IBookmarkService.cs
  Dtos/BookmarkDto.cs

Characters/
  CharacterService.cs            ← full CRUD; ToDto renders Page.Title, Page.ShortDescription, Biography + cache invalidation
  ICharacterRepository.cs
  ICharacterService.cs
  Dtos/                          ← CharacterDto, CharacterListItemDto, CreateCharacterRequest, UpdateCharacterRequest
  Validators/

Comments/
  CommentService.cs              ← list (spoiler-filtered), create, soft-delete, toggle reaction
  ICommentRepository.cs          ← also handles CommentReaction methods
  ICommentService.cs
  Dtos/                          ← CommentDto, CreateCommentRequest, ReactionSummary
  Validators/

Common/
  IAppDbContext.cs               ← exposes all DbSets (Application uses this, Infrastructure implements)
  IRepository.cs                 ← generic CRUD contract
  IPagedEntity.cs                ← marker interface for entities with a 1:1 Page (Id, PageId, Page)
  IPagedEntityRepository.cs      ← generic read-only repo contract (visible-by-slug/id, list visible)
  PagedEntityReadService.cs      ← abstract base service: 404s + pagination, subclasses override ToDto/ToListItem
  PaginationParams.cs            ← clamped Page/PageSize
  PaginatedResult.cs             ← Items + Total + Page + PageSize
  Exceptions/
    AuthException.cs             → 401
    ForbiddenException.cs        → 403
    NotFoundException.cs         → 404
    ConflictException.cs         → 409
    ValidationException.cs       → 400 (carries field-level errors)

EditSuggestions/
  EditSuggestionService.cs       ← submit/approve/reject + diff apply on Page + notify submitter
  IEditSuggestionRepository.cs
  IEditSuggestionService.cs
  Dtos/                          ← EditSuggestionDto, CreateEditSuggestionRequest
  Validators/

Entities/                        ← 28 main entity classes + Pivots/ (11 pivot classes)
Enums/Enums.cs                   ← all 20 PostgreSQL enums in one file

Notifications/
  NotificationService.cs         ← list mine, count unread, mark read, PublishAsync (save + push)
  INotificationRepository.cs
  INotificationService.cs
  INotificationPusher.cs         ← contract (SignalR impl in API)
  Dtos/NotificationDto.cs

Pages/
  PageCacheKeys.cs               ← single source of cache key strings
  PageService.cs                 ← cached slug lookup + spoiler gate, list visible; ToDto renders Title and ShortDescription
  IPageRepository.cs
  IPageService.cs
  Dtos/PageDto.cs

Spoilers/
  SpoilerService.cs              ← page-level gate + inline [spoiler ch=N] parser
  ISpoilerService.cs
  Dtos/Segment.cs                ← {Type, Content, RevealChapter}
  Dtos/RenderedContent.cs        ← wraps List<Segment>, exposes HasHiddenContent

Timeline/
  TimelineService.cs             ← cross-aggregate query against IAppDbContext directly
  ITimelineService.cs
  Dtos/                          ← WorldlineNodeDto, EventNodeDto, EventConnectionEdgeDto, TimelineDto

# Read-only spoiler-rendered modules for the 16 remaining encyclopedic entities.
# Each folder follows the same shape — one service that extends
# PagedEntityReadService<TEntity, TDto, TListItemDto> plus its DTOs.
Arcs/, Attributes/, Concepts/, Constellations/, DemonKings/, Dokkaebis/, Events/,
Fables/, Items/, Locations/, Nebulae/, OuterGods/, Scenarios/, Skills/, Stigmas/, Worldlines/
  {Entity}Service.cs             ← overrides ToDto (renders narrative fields) + ToListItem + EntityName
  Dtos/{Entity}Dto.cs            ← full read DTO; Title/ShortDescription/narrative fields are RenderedContent
  Dtos/{Entity}ListItemDto.cs    ← small list payload (id, slug, name + key facets, discoveryChapter)

DependencyInjection.cs           ← AddApplication() — registers all services + validators + TimeProvider
```

> **Namespace gotcha:** the `Dokkaebi` entity collides with a sibling namespace if you call the module `Dokkaebi`. The folder is named `Dokkaebis/` (plural) and the service file uses `using DokkaebiEntity = ORVWiki.Application.Entities.Dokkaebi;`. The HTTP route is still `/api/dokkaebi` (singular Korean plural).

### `ORVWiki.Infrastructure/`

```
Auth/
  BcryptPasswordHasher.cs        ← BCrypt with work factor 12
  JwtSettings.cs                 ← bound to "Jwt" config section
  JwtTokenGenerator.cs           ← HMAC-SHA256, claims include current_chapter

Persistence/
  AppDbContext.cs                ← implements IAppDbContext, all entity configs in OnModelCreating
  DbInitializer.cs               ← Database.MigrateAsync() + role seeding (Development only)
  Migrations/
    20260429201044_InitialCreate.cs   ← 39 tables, 20 enum types, all FKs/CHECKs/UNIQUEs
    AppDbContextModelSnapshot.cs
  Repositories/
    Repository.cs                ← generic Repository<T> base
    PagedEntityRepository.cs     ← open-generic IPagedEntityRepository<T> impl (Include Page + spoiler gate at SQL)
    PageRepository.cs            ← GetVisibleBySlugAsync, GetBySlugAsync, ListVisibleAsync
    CharacterRepository.cs       ← spoiler-aware queries with Include(Page)
    CommentRepository.cs         ← also covers CommentReaction operations
    BookmarkRepository.cs
    EditSuggestionRepository.cs
    NotificationRepository.cs    ← uses ExecuteUpdateAsync for bulk read-marking

DependencyInjection.cs           ← AddInfrastructure() — Npgsql data source with enum mappings, DbContext, all repos, JWT/BCrypt
```

### `ORVWiki.API/`

```
Auth/
  AuthPolicies.cs                ← policy name constants matching Roles
  CurrentUser.cs                 ← static helpers: GetId(claims), GetCurrentChapter(claims)

Controllers/                     ← 26 controllers (10 original + 16 read-only encyclopedic), see §6

Middleware/
  ExceptionHandlingMiddleware.cs ← maps app exceptions → ProblemDetails / ValidationProblemDetails

OpenApi/
  BearerSecuritySchemeTransformer.cs  ← injects Bearer scheme into OpenAPI doc for Scalar

Realtime/
  NotificationsHub.cs            ← SignalR hub at /hubs/notifications (Reader+ policy)
  SignalRNotificationPusher.cs   ← INotificationPusher impl over IHubContext

Program.cs                       ← composition root: Serilog, OpenAPI/Scalar, JWT, SignalR, MemoryCache, Application+Infrastructure DI, middleware pipeline
appsettings.json                 ← ConnectionStrings, Jwt, Serilog
```

---

## 4. Cross-cutting systems

### 4.1 Authentication & authorization

**Login flow:**

1. `POST /api/auth/register` → creates user with role `reader`, returns JWT
2. `POST /api/auth/login` → validates credentials with BCrypt, returns JWT
3. JWT signed with HMAC-SHA256, 60-minute expiry (`Jwt:AccessTokenMinutes`)
4. Claims included: `sub`, `jti`, `email`, `unique_name`, `name`, `email`, `role`, `**current_chapter`** (custom)

**Authorization:**

- 3 cumulative policies in `Program.cs`:
  - `admin` → requires `admin` role
  - `editor` → requires `editor` OR `admin`
  - `reader` → requires `reader` OR `editor` OR `admin`
- Policy names match role names (defined in `AuthPolicies.cs` and `Roles.cs`).
- Apply via `[Authorize(Policy = AuthPolicies.Reader)]` on controllers/actions.

**Default roles** seeded by `DbInitializer` on first Development startup: `admin`, `editor`, `reader`.

### 4.2 The spoiler system (Phase 4)

Two distinct concerns, both in `SpoilerService`:

**A. Page-level chapter gate** — repository layer applies `WHERE discovery_chapter <= currentChapter` in SQL. If the row doesn't return, the controller surfaces a 404. `ISpoilerService.IsRevealed(int, int)` and `EnsureRevealed(int, int, string)` are convenience helpers for service code that holds an entity in hand.

**B. Inline `[spoiler ch=N]…[/spoiler]` parser** — `SpoilerService.RenderInline(text, currentChapter)` returns `RenderedContent { Segments }`. Each `Segment` is one of:

- text segment: `{ Type: "text", Content: "...", RevealChapter: null }`
- revealed spoiler: `{ Type: "spoiler", Content: "...", RevealChapter: N }` (when `currentChapter >= N`)
- hidden spoiler: `{ Type: "spoiler", Content: null, RevealChapter: N }` (when `currentChapter < N`)

**Server-enforced:** hidden segments have `Content: null` — the text **never reaches the client** if they haven't unlocked the chapter. This is stricter than Reddit's client-side blackout.

**Where `RenderInline` is wired** (every visible string field that may contain `[spoiler ch=N]` markup):

- `PageDto` — `Title`, `ShortDescription`
- `CharacterDto` — `Title`, `ShortDescription`, `Biography`
- All 16 other encyclopedic DTOs (`ConstellationDto`, `ItemDto`, `StigmaDto`, `ScenarioDto`, …) — `Title`, `ShortDescription`, plus the entity's narrative field(s):
  - **Narrative fields:** `Arc.Summary`, `Attribute.Effect`, `Concept.Definition`, `Constellation.Description`, `DemonKing.Description`, `Dokkaebi.Speciality`, `Event.Title` + `Event.Description`, `Fable.Title` + `Fable.Legend`, `Item.Description`, `Location.Description`, `Nebula.Description`, `OuterGod.Description`, `Scenario.Title` + `Conditions` + `Rewards` + `Penalty`, `Skill.Effect`, `Stigma.Effect`, `Worldline.Description`

Subclasses of `PagedEntityReadService` apply `Spoilers.RenderInline(...)` inside their `ToDto` override, so every read endpoint that returns one of these DTOs is automatically spoiler-safe. List endpoints return small `*ListItemDto` payloads with raw strings (slug, name) — those are summary fields not expected to contain inline markup.

To unlock spoilers, users `PATCH /api/users/me/current-chapter` with their reading progress; on next request the JWT claim is refreshed (currently they need to re-login to refresh — see §9 known limitations).

### 4.3 Comment spoiler filter

Same pattern, different field: `WHERE chapter_at_post <= currentChapter`. Implemented in `CommentRepository.ListVisibleByPageAsync`. Soft-deleted comments are kept in the table but their `Body` and `Username` are nulled in the DTO so thread structure survives.

### 4.4 Caching (Phase 7)

**Strategy:** cache `Page` rows by slug for 5 minutes, apply spoiler gate after the cache hit (so one cached row serves every reader regardless of `current_chapter`).

**Where:**

- Read: `PageService.GetVisibleBySlugAsync` wraps `IPageRepository.GetBySlugAsync` in `IMemoryCache.GetOrCreateAsync`.
- Invalidate: `CharacterService.UpdateAsync` / `DeleteAsync` and `EditSuggestionService.ApproveAsync` call `cache.Remove(PageCacheKeys.BySlug(slug))` after their `SaveChangesAsync`.

Cache keys live in `ORVWiki.Application/Pages/PageCacheKeys.cs` so writers and readers can't drift.

**Not cached:** list endpoints (too many filter combinations, low hit rate), entity-specific reads (e.g. `GET /api/characters/{id}`).

### 4.5 Real-time notifications (Phase 7)

**Hub:** `/hubs/notifications`, `[Authorize(Policy = AuthPolicies.Reader)]`. The default `IUserIdProvider` reads `ClaimTypes.NameIdentifier` from the JWT, so we can use `Clients.User(userId)` without manual group bookkeeping.

**JWT over WebSocket:** browsers can't set Authorization headers on WS upgrade, so `Program.cs` `JwtBearerEvents.OnMessageReceived` reads `?access_token=…` from the query string for `/hubs/`* paths.

**Push semantics — `INotificationService.PublishAsync`:**

1. Adds `Notification` row, calls its **own** `SaveChangesAsync` (decoupled from caller's transaction).
2. Calls `INotificationPusher.PushToUserAsync` to fire the SignalR event named `"notification"`.
3. Push failures are caught and logged but never fail the request — the row is durable and the user will see it on next fetch.

**Calling pattern** (from `CommentService` and `EditSuggestionService`): always call `PublishAsync` **after** the originating action's own `SaveChangesAsync` succeeds, so notifications never go out for actions that didn't commit.

Client subscribes:

```js
const conn = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/notifications", { accessTokenFactory: () => myJwt })
  .build();
conn.on("notification", (dto) => { /* dto matches NotificationDto */ });
await conn.start();
```

### 4.6 Exception handling

`ExceptionHandlingMiddleware` is the first middleware after Serilog request logging. It maps:


| Exception               | Status | Body                                                     |
| ----------------------- | ------ | -------------------------------------------------------- |
| `ValidationException`   | 400    | `ValidationProblemDetails` with field errors             |
| `AuthException`         | 401    | `ProblemDetails`                                         |
| `ForbiddenException`    | 403    | `ProblemDetails`                                         |
| `NotFoundException`     | 404    | `ProblemDetails`                                         |
| `ConflictException`     | 409    | `ProblemDetails`                                         |
| `Exception` (catch-all) | 500    | `ProblemDetails` (message swallowed; logged via Serilog) |


All five custom exceptions live in `ORVWiki.Application/Common/Exceptions/`.

### 4.7 Logging

Serilog is wired in `Program.cs` with `builder.Host.UseSerilog(...)`. Reads sinks and minimum levels from `appsettings.json` `Serilog` section; current config writes to console with structured JSON-style properties. `app.UseSerilogRequestLogging()` adds one summary line per request (method, path, status, elapsed).

To add a file sink later: install `Serilog.Sinks.File` (already transitively available via `Serilog.AspNetCore`), then add a `WriteTo` entry in `appsettings.json`.

---

## 5. Generic patterns to follow when extending

### 5.1 Adding read-only spoiler-rendered access for a new encyclopedic entity

This is the **fast path**. All 16 non-Character encyclopedic entities use it; mirror them.

**Prereq:** the entity has a 1:1 with `Page` and implements `IPagedEntity` (just `: IPagedEntity` on the class declaration — `Id`, `PageId`, `Page` already match).

1. **Application** — under `ORVWiki.Application/{Entities}/` (plural folder name):
  - `Dtos/{Entity}Dto.cs` — full read DTO. `Title` and `ShortDescription` are `RenderedContent`. Each narrative field is `RenderedContent`. Structural fields (enums, IDs, dates) are passthrough.
  - `Dtos/{Entity}ListItemDto.cs` — small list payload with raw strings.
  - `{Entity}Service.cs` — extends `PagedEntityReadService<TEntity, TDto, TListItemDto>(repository, spoilers)`. Override three members:
    - `EntityName` → string used in 404 messages
    - `ToDto(entity, currentChapter)` → run narrative fields through `Spoilers.RenderInline(...)`
    - `ToListItem(entity)` → raw-string projection for list views
2. **API** — `{Entities}Controller` with `[Route("api/{entities}")]`, `[Authorize(Policy = AuthPolicies.Reader)]`, three endpoints (`GET /`, `GET /{id:long}`, `GET /by-slug/{slug}`). Mirror any of the 16 existing read-only controllers (e.g., `ItemsController`).
3. **DI** — `services.AddScoped<{Entity}Service>();` in `AddApplication()`. The repository is registered once as an open generic (`AddScoped(typeof(IPagedEntityRepository<>), typeof(PagedEntityRepository<>))`) — no per-entity registration needed.

> **Naming gotchas:** `Attribute` clashes with `System.Attribute` (use `using AttributeEntity = ORVWiki.Application.Entities.Attribute;`). `Dokkaebi` clashes with its own namespace (use folder `Dokkaebis/` and alias `using DokkaebiEntity = ORVWiki.Application.Entities.Dokkaebi;`).

### 5.2 Adding full CRUD for an encyclopedic entity (e.g., write API)

`Character` is the template — it predates the generic read path and keeps its own `ICharacterRepository`/`CharacterService` because it needs Create/Update/Delete with cache invalidation, validators, and slug uniqueness checks.

1. **Application** — under `ORVWiki.Application/{Entities}/`:
  - `Dtos/` — `Create{Entity}Request`, `Update{Entity}Request` (in addition to the read DTOs from §5.1).
  - `I{Entity}Repository : IRepository<TEntity>` — copy `ICharacterRepository` shape (`GetWithPageByIdAsync`, `SlugExistsAsync`, etc.).
  - `I{Entity}Service` and `{Entity}Service` — copy `CharacterService`. The service can still apply spoiler rendering on read paths via injected `ISpoilerService`, or delegate read methods to a `PagedEntityReadService` subclass and only own the writes.
  - `Validators/` — slug, title, discoveryChapter, entity-specific rules. `FluentValidation` auto-discovered from the assembly.
2. **Infrastructure** — `{Entity}Repository : Repository<TEntity>, I{Entity}Repository`. Follow `CharacterRepository`: eager-load `Page`, apply spoiler gate at SQL.
3. **API** — controller mirroring `CharactersController`. Reader for GET, Editor for POST/PUT, Admin for DELETE.
4. **DI** — register both interfaces in `AddApplication()` and `AddInfrastructure()`.
5. **Cache invalidation** — if write methods can change Page metadata (title, slug, discoveryChapter, shortDescription), call `cache.Remove(PageCacheKeys.BySlug(slug))` after `SaveChanges` and inject `IMemoryCache`.

### 5.3 Adding a new endpoint that needs the user's spoiler chapter

```csharp
var currentChapter = CurrentUser.GetCurrentChapter(User);  // 0 if claim missing
var dto = await someService.SomeMethodAsync(..., currentChapter, ct);
```

`CurrentUser.GetId(User)` for the user id. Both helpers are in `ORVWiki.API/Auth/CurrentUser.cs`.

### 5.4 Adding a notification type

1. Add a value to `NotificationType` enum (`Application/Enums/Enums.cs`). **Will need a new EF migration** because PostgreSQL ENUMs require explicit `ALTER TYPE … ADD VALUE`.
2. Call `notifications.PublishAsync(targetUserId, NotificationType.NewType, payload, ct)` from the originating service after its `SaveChanges`.
3. Frontend listens on the existing `"notification"` SignalR event — no hub change needed.

### 5.5 Adding a new exception type → status

1. Define `class FooException(string m) : Exception(m)` in `Application/Common/Exceptions/`.
2. Add a `catch (FooException ex)` block in `ExceptionHandlingMiddleware.Invoke` that calls `WriteProblem(context, statusCode, ex.Message)`.
3. Add the status to the `ReasonPhrases` switch in the same file.

---

## 6. Complete API surface

All routes are under `/api/...` unless noted. **Auth column:** `—` = anonymous, `R` = Reader, `E` = Editor, `A` = Admin.
Open https://localhost:7138 and http://localhost:5044

### Auth (`/api/auth`)


| Method | Path        | Auth | Purpose                                         |
| ------ | ----------- | ---- | ----------------------------------------------- |
| POST   | `/register` | —    | Create user + return JWT (default role: reader) |
| POST   | `/login`    | —    | Email-or-username login, returns JWT            |
| GET    | `/me`       | R    | Current user details from JWT                   |


### Users (`/api/users`)


| Method | Path                  | Auth | Purpose                 |
| ------ | --------------------- | ---- | ----------------------- |
| PATCH  | `/me/current-chapter` | R    | Update reading progress |
| GET    | `/{id:long}`          | A    | Admin lookup            |
| PATCH  | `/{id:long}/role`     | A    | Change user's role      |


### Roles (`/api/roles`)


| Method | Path | Auth | Purpose        |
| ------ | ---- | ---- | -------------- |
| GET    | `/`  | A    | List all roles |


### Pages (`/api/pages`) — spoiler-gated


| Method | Path                         | Auth | Purpose                                       |
| ------ | ---------------------------- | ---- | --------------------------------------------- |
| GET    | `/?page&pageSize&entityType` | R    | Paginated visible pages, optional type filter |
| GET    | `/{slug}`                    | R    | Single page (cached 5 min)                    |


### Characters (`/api/characters`) — spoiler-gated


| Method | Path              | Auth | Purpose                                  |
| ------ | ----------------- | ---- | ---------------------------------------- |
| GET    | `/?page&pageSize` | R    | Paginated visible characters             |
| GET    | `/{id:long}`      | R    | Single character with rendered biography |
| GET    | `/by-slug/{slug}` | R    | Same, by slug                            |
| POST   | `/`               | E    | Create character + its page              |
| PUT    | `/{id:long}`      | E    | Replace character + page metadata        |
| DELETE | `/{id:long}`      | A    | Cascade delete via Page FK               |


### Read-only encyclopedic entities — spoiler-gated

All 16 entities below share the same three-endpoint shape via `PagedEntityReadService` (see §5.1). Every read DTO renders `Title`, `ShortDescription`, and the entity's narrative field(s) through `SpoilerService.RenderInline`.


| Method | Path                                  | Auth | Purpose                                                     |
| ------ | ------------------------------------- | ---- | ----------------------------------------------------------- |
| GET    | `{base}/?page&pageSize`               | R    | Paginated list of visible entities                          |
| GET    | `{base}/{id:long}`                    | R    | Single entity by id (404 if hidden by spoiler gate)         |
| GET    | `{base}/by-slug/{slug}`               | R    | Single entity by Page slug (404 if hidden by spoiler gate)  |


| Entity         | Base path             | Narrative fields rendered                                    |
| -------------- | --------------------- | ------------------------------------------------------------ |
| `Arc`          | `/api/arcs`           | `Summary`                                                    |
| `Attribute`    | `/api/attributes`     | `Effect`                                                     |
| `Concept`      | `/api/concepts`       | `Definition`                                                 |
| `Constellation`| `/api/constellations` | `Description`                                                |
| `DemonKing`    | `/api/demon-kings`    | `Description`                                                |
| `Dokkaebi`     | `/api/dokkaebi`       | `Speciality`                                                 |
| `Event`        | `/api/events`         | `Title`, `Description`                                       |
| `Fable`        | `/api/fables`         | `Title`, `Legend`                                            |
| `Item`         | `/api/items`          | `Description`                                                |
| `Location`     | `/api/locations`      | `Description`                                                |
| `Nebula`       | `/api/nebulae`        | `Description`                                                |
| `OuterGod`     | `/api/outer-gods`     | `Description`                                                |
| `Scenario`     | `/api/scenarios`      | `Title`, `Conditions`, `Rewards`, `Penalty`                  |
| `Skill`        | `/api/skills`         | `Effect`                                                     |
| `Stigma`       | `/api/stigmas`        | `Effect`                                                     |
| `Worldline`    | `/api/worldlines`     | `Description`                                                |


**Write endpoints (POST/PUT/DELETE) are not yet exposed for these 16.** Add them by mirroring `CharactersController` per §5.2 when a write workflow is needed.

### Comments (`/api/comments`)


| Method | Path                          | Auth        | Purpose                                             |
| ------ | ----------------------------- | ----------- | --------------------------------------------------- |
| GET    | `/?pageId={id}`               | R           | Comments on a page (filtered by `chapter_at_post`)  |
| POST   | `/`                           | R           | Create top-level or reply (notifies parent author)  |
| DELETE | `/{id:long}`                  | R (own) / E | Soft delete                                         |
| POST   | `/{id:long}/reactions/{type}` | R           | Toggle reaction (like/dislike/heart/laugh/sad/star) |


### Bookmarks (`/api/bookmarks`)


| Method | Path                    | Auth | Purpose                                |
| ------ | ----------------------- | ---- | -------------------------------------- |
| GET    | `/?page&pageSize`       | R    | List my bookmarks                      |
| POST   | `/toggle/{pageId:long}` | R    | Toggle, returns `{ bookmarked: bool }` |


### Edit Suggestions (`/api/edit-suggestions`)


| Method | Path                     | Auth | Purpose                                         |
| ------ | ------------------------ | ---- | ----------------------------------------------- |
| POST   | `/`                      | R    | Submit (`{ pageId, proposedChanges, reason? }`) |
| GET    | `/mine?page&pageSize`    | R    | My submissions                                  |
| GET    | `/?status&page&pageSize` | E    | Review queue                                    |
| GET    | `/{id:long}`             | E    | Detail                                          |
| POST   | `/{id:long}/approve`     | E    | Apply diff to Page + notify submitter           |
| POST   | `/{id:long}/reject`      | E    | Notify submitter                                |


**Diff format** (JSON object). Currently auto-applied keys on `Page`: `title` (string), `shortDescription` (string|null), `discoveryChapter` (int). Other keys are stored verbatim and ignored on apply — meant as flags for the editor to apply manually.

### Notifications (`/api/notifications`)


| Method | Path              | Auth | Purpose                            |
| ------ | ----------------- | ---- | ---------------------------------- |
| GET    | `/?page&pageSize` | R    | List mine, newest first            |
| GET    | `/unread-count`   | R    | `{ count: N }`                     |
| POST   | `/{id:long}/read` | R    | Mark single read                   |
| POST   | `/read-all`       | R    | Bulk mark via `ExecuteUpdateAsync` |


### Timeline (`/api/timeline`)


| Method | Path                        | Auth | Purpose                                                                         |
| ------ | --------------------------- | ---- | ------------------------------------------------------------------------------- |
| GET    | `/?upToChapter&characterId` | R    | Full graph payload — worldlines (skeleton), filtered events, pruned connections |


Note: per spec the timeline is **inherently spoiler-rich**, hence opt-in (no automatic `current_chapter` filter). Use `?upToChapter=N` to self-impose a limit.

### Real-time


| Endpoint                          | Auth | Description                                                                                                   |
| --------------------------------- | ---- | ------------------------------------------------------------------------------------------------------------- |
| `/hubs/notifications` (WebSocket) | R    | SignalR hub. Pass JWT via `?access_token=`. Listen for `"notification"` event with `NotificationDto` payload. |


### Dev-only docs


| Endpoint           | Description                                       |
| ------------------ | ------------------------------------------------- |
| `/openapi/v1.json` | Generated OpenAPI 3.x document                    |
| `/scalar/v1`       | Scalar API docs UI (with "Authorize" → paste JWT) |


---

## 7. Configuration

### `appsettings.json` keys

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=orv_wiki;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Issuer": "orv-wiki",
    "Audience": "orv-wiki-clients",
    "SigningKey": "REPLACE_WITH_AT_LEAST_32_CHARS_LONG_SECRET_KEY_FOR_HMAC_SHA256",
    "AccessTokenMinutes": 60
  },
  "Serilog": { /* MinimumLevel, WriteTo, Enrich */ }
}
```

**Before deploying:**

- ⚠️ Replace `Jwt:SigningKey` with a strong ≥32-char random secret. Use User Secrets or env vars in production.
- ⚠️ Replace the connection string password.
- Consider rotating to managed secrets (Azure Key Vault, AWS Secrets Manager, etc.).

### Environment variables override

Standard ASP.NET Core configuration: `ConnectionStrings__Postgres`, `Jwt__SigningKey`, etc.

---

## 8. Running locally

### One-time setup

```bash
# Postgres at localhost:5432 with database "orv_wiki" (any superuser will do)
createdb orv_wiki

# install EF tooling once
dotnet tool install --global dotnet-ef
```

### Run

```bash
cd ORV_Wiki_Backend
dotnet run --project ORVWiki.API
```

In Development:

- `DbInitializer` runs `Database.MigrateAsync()` then seeds the three default roles.
- API listens on the URLs in `Properties/launchSettings.json`.
- OpenAPI: `https://localhost:<port>/openapi/v1.json`
- Scalar UI: `https://localhost:<port>/scalar/v1`

### Manual migration commands

```bash
# Generate a new migration
dotnet ef migrations add <Name> --project ORVWiki.Infrastructure --startup-project ORVWiki.API --output-dir Persistence/Migrations

# Apply migrations to the configured DB
dotnet ef database update --project ORVWiki.Infrastructure --startup-project ORVWiki.API
```

### Quick smoke test (end-to-end)

```bash
# Register
curl -X POST http://localhost:<port>/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"email":"a@b.c","username":"alice","password":"hunter2hunter2"}'
# → returns { accessToken, expiresAt, user }

# Use the token
TOKEN=...
curl http://localhost:<port>/api/auth/me -H "Authorization: Bearer $TOKEN"

# Set reading progress (so spoilers below ch=50 reveal)
curl -X PATCH http://localhost:<port>/api/users/me/current-chapter \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"currentChapter":50}'
# Note: the JWT current_chapter claim is set at login time; you'll need
# to re-login for it to take effect on subsequent requests. See §9.
```

---

## 9. Known limitations / sharp edges

### `current_chapter` claim is set at login

The JWT carries `current_chapter` as a claim, set when `JwtTokenGenerator.Generate(user)` runs (login or register). After `PATCH /api/users/me/current-chapter`, the **DB row is updated but the active token is stale**. The user must re-login to get a fresh token. Acceptable for MVP since tokens expire in 60 minutes anyway. To fix: issue refresh tokens and re-mint on chapter change, or read `current_chapter` from DB per request (loses stateless JWT benefits).

### Only `Character` has full CRUD

The other 16 encyclopedic entities (`Constellation`, `Nebula`, `Stigma`, etc.) expose **read-only** spoiler-rendered endpoints via `PagedEntityReadService` (see §5.1). Adding POST/PUT/DELETE means mirroring `CharactersController` per §5.2 — copy the validators, the slug-uniqueness check, and the `cache.Remove(PageCacheKeys.BySlug(...))` invalidation calls. Cookie-cutter extension, not new design.

### EditSuggestion diff applies only Page-level fields

`ApplyDiffToPage` handles `title`, `shortDescription`, `discoveryChapter`. Diffs targeting entity-specific fields (`biography`, `effect`, etc.) are stored but not applied — the editor must apply them manually. To extend: add per-entity-type appliers, dispatched on `Page.EntityType`.

### No rate limiting

Anyone with valid credentials can spam `POST /api/comments`, `POST /api/edit-suggestions`, etc. Add `Microsoft.AspNetCore.RateLimiting` policies before production.

### No CORS configuration

If you'll serve a separate frontend, add `AddCors` + `UseCors` in `Program.cs` with the frontend origin allowlist. Currently any browser on a different origin will be blocked.

### Notification push is best-effort

If SignalR push fails (user disconnected), it's logged at Warning and swallowed. The notification row is persisted, so a refresh of `GET /api/notifications` will surface it. Acceptable given how SignalR reconnects work.

### Cache TTL only on slug lookups

Single-page reads benefit. List endpoints (`GET /api/pages`, `GET /api/characters`) hit the DB every call. Add output caching with `AddOutputCache()` if list calls become hot.

### `dotnet ef` requires PATH update

After `dotnet tool install --global dotnet-ef`, the binary lives at `~/.dotnet/tools/dotnet-ef`. Add `~/.dotnet/tools` to your `PATH` in `~/.zshrc` if not already.

### Default `dotnet new webapi` template files

Removed from `ORVWiki.API/`: `WeatherForecast.cs`, `Controllers/WeatherForecastController.cs`, `ORVWiki.API.http`. If you re-scaffold the project, delete them again.

---

## 10. The 8-phase roadmap (history)

Each phase shipped self-contained. The final solution is the union of all eight.


| Phase                  | What it added                                                                                                                                                                                                                                                                                                                 |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1 — Foundation         | 3-project solution, EF Core/Npgsql, all 40 entities + relations + constraints, snake_case naming, initial migration                                                                                                                                                                                                           |
| 2 — Auth               | `User`/`Role` endpoints, JWT with `current_chapter` claim, BCrypt password hashing, 3 cumulative role policies, `DbInitializer` role seeding, exception middleware                                                                                                                                                            |
| 3 — Core wiki CRUD     | `IRepository<T>` generic + `Repository<T>` base, `PageRepository` and `CharacterRepository` with spoiler-aware queries, services + DTOs + validators, `PagesController` + `CharactersController`, pagination types                                                                                                            |
| 4 — Spoiler system     | `SpoilerService` with `IsRevealed`/`EnsureRevealed` (page gate) + `RenderInline` (inline `[spoiler ch=N]` parser → server-enforced segments), `Character.Biography` switched from raw string to `RenderedContent`                                                                                                             |
| 5 — Community features | Comments (threading + spoiler filter on `chapter_at_post` + soft delete), `CommentReaction` toggle (per-type unique), `Bookmark` toggle, `EditSuggestion` workflow with diff apply + notifications, `Notification` CRUD, `ForbiddenException` (403) added                                                                     |
| 6 — Timeline           | `TimelineService` returning `{Worldlines, Events, Connections}` graph payload, optional `upToChapter` and `characterId` filters, dead-edge pruning                                                                                                                                                                            |
| 7 — Polish             | Serilog structured logging + request logging, Scalar UI for OpenAPI with Bearer auth, `IMemoryCache` for popular pages with explicit invalidation, SignalR `NotificationsHub` + `INotificationPusher` abstraction, JWT-from-query for WebSocket upgrades, switched `EnqueueAsync` → `PublishAsync` (save-then-push semantics) |
| 8 — Universal spoilers | `IPagedEntity` marker + open-generic `PagedEntityRepository<T>` + abstract `PagedEntityReadService<T,TDto,TListItemDto>`. `Page.Title` and `Page.ShortDescription` switched from raw string to `RenderedContent`. 16 new read-only endpoints (`/api/arcs`, `/api/items`, `/api/stigmas`, …) — every visible string field across the wiki now respects `[spoiler ch=N]` markup. |


---

## 11. Quick reference for future Claude sessions

When picking this project back up, here's the minimum to know:

- **3 projects, dependencies API → Application ← Infrastructure**. Application has no ASP.NET/EF deps.
- **40 tables**, every encyclopedic entity has 1:1 with `Page` (which holds `slug`, `title`, `discovery_chapter` — the spoiler gate field).
- **Auth = JWT bearer**, role claim + `current_chapter` claim. Three policies in `Program.cs`.
- **Spoiler enforcement is server-side**: `WHERE discovery_chapter <= currentChapter` at SQL, and `[spoiler ch=N]` segments with `Content: null` for hidden ones. Every visible string in every encyclopedic DTO goes through `SpoilerService.RenderInline`.
- **Add a new read-only entity** = implement `IPagedEntity`, drop in a `PagedEntityReadService<T,TDto,TListItemDto>` subclass, register the service. The repo is open-generic — no per-entity registration. See §5.1.
- **Add full CRUD** = mirror Character (Service + Repository + Controller + DTOs + Validators) and register in both DI extensions. See §5.2.
- **Notifications** = inject `INotificationService`, call `PublishAsync(...)` after your own `SaveChanges` succeeds.
- **`Attribute` entity needs a `using AttributeEntity = ...;` alias** when referenced alongside `using System;`. **`Dokkaebi` entity needs a `using DokkaebiEntity = ...;` alias** when its module's namespace is in scope (folder is named `Dokkaebis/` to reduce the chance of collision).
- **Cache** lives only on `PageService.GetVisibleBySlugAsync`. Invalidate with `cache.Remove(PageCacheKeys.BySlug(slug))` after any write that changes Page fields.
- **All exceptions in `Application/Common/Exceptions/`** are auto-mapped to status codes by `ExceptionHandlingMiddleware`.
- **Migration command** uses `--project ORVWiki.Infrastructure --startup-project ORVWiki.API`.
- **Build clean, no warnings**, host starts on `dotnet run --project ORVWiki.API`.

