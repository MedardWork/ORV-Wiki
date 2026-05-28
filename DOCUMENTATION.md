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

## 2. Domain model (39 tables)

Full schema is documented in `~/Desktop/ORV Wiki/ORV_Wiki_Database_Specifikacia.md` and the dbml at `~/Desktop/ORV Wiki/ORV_Wiki_dbdiagram.dbml`. Quick summary:

### Entity groups

- **A. Beings (7)** — `Character`, `Constellation`, `Nebula`, `Dokkaebi`, `DemonKing`, `OuterGod`, `Worldline`
- **B. Power system (5)** — `Fable`, `Stigma`, `Attribute`, `Skill`, `Item`
- **C. Story & world (7)** — `Location`, `Scenario`, `Arc`, `Chapter`, `Event`, `Concept`, `Jump`
- **D. Community (7)** — `User`, `Role`, `Comment`, `CommentReaction`, `EditSuggestion`, `Bookmark`, `Notification`
- **E. Helpers (3)** — `Page`, `Media`, `Tag`
- **Pivots (10 distinct)** — `CharacterStigma`, `CharacterAttribute`, `CharacterSkill`, `CharacterItem`, `CharacterFable`, `CharacterConstellation`, `EventCharacter`, `ScenarioParticipant`, `ScenarioLocation`, `PageTag`. (`CommentReaction` doubles as the Comment↔User pivot.)

> **Timeline rework (May 2026):** `EventConnection` was removed as a pivot. The graph edges between worldlines are now first-class `Jump` rows (worldline-to-worldline, carrying an opaque `CharacterLabel`, optional `Description`, `LengthEstimate`, and `ArcId`). See migration `20260509205046_WorldlineJumpsRefactor` and the updated `/api/timeline` payload in §6.

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
  CharacterService.cs            ← read-only: ToDetailDto embeds relationships. Writes go through Content/ (generic)
  ICharacterRepository.cs
  ICharacterService.cs
  Dtos/                          ← CharacterDto, CharacterDetailDto, CharacterRelationDtos, CharacterListItemDto

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

Content/                         ← generalized content management for all 17 types
  ContentField.cs, ContentRelation.cs, ContentFieldKind.cs   ← editable-field / relation model
  ContentFields.cs, ContentRelations.cs, PageFields.cs       ← terse typed factories for descriptors
  IContentTypeDescriptor.cs, ContentTypeDescriptor.cs        ← per-type schema + persistence hooks
  Descriptors/{Entity}ContentDescriptor.cs (x17)             ← one declarative descriptor per content type
  IContentTypeRegistry.cs, ContentTypeRegistry.cs            ← resolves a descriptor by EntityType
  ContentDiff.cs, ContentSnapshot.cs                         ← parsed diff + raw read-back model
  IContentMutationService.cs, ContentMutationService.cs      ← validates + applies a diff (Create/Update/Delete)
  IEditorContentService.cs, EditorContentService.cs          ← direct editor writes; log an auto-approved suggestion
  Dtos/                          ← ContentTypeDescriptorDto, ContentWriteRequest, ContentWriteResult

EditSuggestions/
  EditSuggestionService.cs       ← submit/approve/reject for any type; approval applies the diff via ContentMutationService
  IEditSuggestionRepository.cs
  IEditSuggestionService.cs
  Dtos/                          ← EditSuggestionDto, CreateEditSuggestionRequest
  Validators/

Entities/                        ← 29 main entity classes + Pivots/ (10 pivot classes)
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
  Dtos/                          ← WorldlineNodeDto, EventNodeDto, JumpEdgeDto, TimelineDto

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
    20260429201044_InitialCreate.cs            ← 39 tables, 20 enum types, all FKs/CHECKs/UNIQUEs
    20260430154817_ReorderCharacterStatusEnum.cs ← reshuffles the `CharacterStatus` PostgreSQL enum
    20260509205046_WorldlineJumpsRefactor.cs   ← drops `EventConnection`, adds `Jump` (worldline→worldline edges)
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

Controllers/                     ← 29 controllers + ContentTypeRouting helper (content management via ContentController + ContentTypesController), see §6

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
- `CharacterDto` / `CharacterDetailDto` — `Title`, `ShortDescription`, `Biography`
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
- Invalidate: `EditorContentService` (the generic content write path — its `InvalidateCache` runs on Create/Update/Delete) and `EditSuggestionService.ApproveAsync` call `cache.Remove(PageCacheKeys.BySlug(slug))` after their `SaveChangesAsync`. `CharacterService` is read-only since Phase 12, so there is no per-type write code to invalidate from.

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
| `DbUpdateException`     | 409    | `ProblemDetails` (EF Core unique / FK violation on save) |
| `Exception` (catch-all) | 500    | `ProblemDetails` (message swallowed; logged via Serilog) |


The five custom exceptions live in `ORVWiki.Application/Common/Exceptions/`; `DbUpdateException` is EF Core's own — caught so a unique-index or FK violation on save reads as a correctable 409 instead of an opaque 500.

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

> **Embedding relationships:** to surface a pivot on an entity's detail page (as `Scenario`/`Location` do for `ScenarioLocation`), add `{Entity}Repository : PagedEntityRepository<TEntity>` overriding the `DetailQuery` hook with the eager-load `Include`s, then register it as a closed-type `IPagedEntityRepository<TEntity>` *after* the open generic in `AddInfrastructure()` (the closed registration wins). Single-entity reads then load the pivot; list reads stay on the lean `VisibleQuery`.

> **Naming gotchas:** `Attribute` clashes with `System.Attribute` (use `using AttributeEntity = ORVWiki.Application.Entities.Attribute;`). `Dokkaebi` clashes with its own namespace (use folder `Dokkaebis/` and alias `using DokkaebiEntity = ORVWiki.Application.Entities.Dokkaebi;`).

### 5.2 Content management — create / edit / delete

All 17 content types are editable through one generic, schema-driven engine — there is no per-type write code. The pieces live in `ORVWiki.Application/Content/`:

- **Descriptor** — `{Entity}ContentDescriptor : ContentTypeDescriptor<TEntity>` declares the type's editable `Fields` and `Relations` (pivot collections) with the `ContentFields` / `ContentRelations` / `PageFields` factories. Descriptors are stateless singletons, assembly-scanned into `IContentTypeRegistry`.
- **Engine** — `ContentMutationService` validates a `ContentDiff` (`{ fields, relations }`) against a descriptor and applies a Create / Update / Delete onto the `Page` + satellite entity + pivots in one transaction.
- **Editor writes** — `EditorContentService` (behind `POST/PUT/DELETE /api/content/{type}`) applies immediately and records an auto-approved `EditSuggestion`, so the suggestion table doubles as the wiki's change history.
- **Suggestions** — `EditSuggestionService.ApproveAsync` runs the same engine, so reader-submitted edits and new-page proposals apply identically.
- **Schema** — `GET /api/content-types` serves each descriptor as JSON (`ContentTypeDescriptorDto`); the frontend renders forms from it with no per-type code.

To expose a **new editable field** on an existing type, add one `ContentFields.*` entry to that type's descriptor — nothing else changes. To make a **brand-new entity** editable, give it a 1:1 `Page` + `IPagedEntity` (§5.1) and add a `{Entity}ContentDescriptor`; the registry, engine, API and frontend pick it up automatically. Cross-field rules go in the descriptor's optional `ValidateCrossFields` hook.

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
Open [https://localhost:7138](https://localhost:7138) and [http://localhost:5044](http://localhost:5044)

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


| Method | Path                             | Auth | Purpose                                                               |
| ------ | -------------------------------- | ---- | --------------------------------------------------------------------- |
| GET    | `/?page&pageSize&entityType&tag` | R    | Paginated visible pages; optional `entityType` / `tag` (slug) filters |
| GET    | `/{slug}`                        | R    | Single page (cached 5 min)                                            |

Every `PageDto` carries its `tags` — a list of `{ id, name, slug, color }`. The `?tag={slug}` filter narrows the list to pages bearing that tag (still spoiler-gated), which is how the frontend's tag-browse view is served.

### Tags (`/api/tags`)

| Method | Path | Auth | Purpose                                            |
| ------ | ---- | ---- | -------------------------------------------------- |
| GET    | `/`  | R    | All tags (`id`, `name`, `slug`, `color`), by name  |

Tags are a small unspoilerable lookup set, so the whole list is returned unpaginated and ungated. Browse a tag's pages via `GET /api/pages?tag={slug}`.


### Characters (`/api/characters`) — spoiler-gated


| Method | Path              | Auth | Purpose                                     |
| ------ | ----------------- | ---- | ------------------------------------------- |
| GET    | `/?page&pageSize` | R    | Paginated visible characters (list items)   |
| GET    | `/{id:long}`      | R    | One character + embedded relationship links |
| GET    | `/by-slug/{slug}` | R    | Same, by slug                               |

Create, edit and delete go through the generic content API (see **Content management** below); `CharactersController` itself is read-only.

**Character detail payload.** The two GET-detail endpoints return `CharacterDetailDto` — the character's own fields plus eleven arrays. Each pivot-derived entry is a navigable link (target `id` + `slug` + display name) carrying its join-row metadata:

- `stigmas`, `attributes`, `skills`, `items`, `fables` — owned powers/possessions (`level`, `acquiredChapter`, `lostChapter`, `isPrimary` as applicable).
- `constellations` — sponsoring/opposing constellations, typed by `CharacterConstellationRel` (`MainSponsor` / `Patron` / `Subscriber` / `Opposed`).
- `events`, `scenarios` — story participation, typed by `EventCharacterRole` / `ScenarioOutcome`.
- `deifiedConstellations`, `originatedFables` — constellations the character became and fables that originated from them (direct FK, not pivots).
- `tags` — the character page's tags (`id`, `name`, `slug`, `color`); each links to the tag's filtered page list.

Each relation is spoiler-gated on the **target's** `discoveryChapter`, so a reader never learns a character holds a skill/stigma whose page is still hidden. The list endpoint returns the flat `CharacterListItemDto` (no relationships).


### Read-only encyclopedic entities — spoiler-gated

All 16 entities below share the same three-endpoint shape via `PagedEntityReadService` (see §5.1). Every read DTO renders `Title`, `ShortDescription`, and the entity's narrative field(s) through `SpoilerService.RenderInline`.


| Method | Path                    | Auth | Purpose                                                    |
| ------ | ----------------------- | ---- | ---------------------------------------------------------- |
| GET    | `{base}/?page&pageSize` | R    | Paginated list of visible entities                         |
| GET    | `{base}/{id:long}`      | R    | Single entity by id (404 if hidden by spoiler gate)        |
| GET    | `{base}/by-slug/{slug}` | R    | Single entity by Page slug (404 if hidden by spoiler gate) |



| Entity          | Base path             | Narrative fields rendered                   |
| --------------- | --------------------- | ------------------------------------------- |
| `Arc`           | `/api/arcs`           | `Summary`                                   |
| `Attribute`     | `/api/attributes`     | `Effect`                                    |
| `Concept`       | `/api/concepts`       | `Definition`                                |
| `Constellation` | `/api/constellations` | `Description`                               |
| `DemonKing`     | `/api/demon-kings`    | `Description`                               |
| `Dokkaebi`      | `/api/dokkaebi`       | `Speciality`                                |
| `Event`         | `/api/events`         | `Title`, `Description`                      |
| `Fable`         | `/api/fables`         | `Title`, `Legend`                           |
| `Item`          | `/api/items`          | `Description`                               |
| `Location`      | `/api/locations`      | `Description`                               |
| `Nebula`        | `/api/nebulae`        | `Description`                               |
| `OuterGod`      | `/api/outer-gods`     | `Description`                               |
| `Scenario`      | `/api/scenarios`      | `Title`, `Conditions`, `Rewards`, `Penalty` |
| `Skill`         | `/api/skills`         | `Effect`                                    |
| `Stigma`        | `/api/stigmas`        | `Effect`                                    |
| `Worldline`     | `/api/worldlines`     | `Description`                               |


**Scenario and Location detail** additionally embed their `ScenarioLocation` links — `ScenarioDto.locations` lists the places a scenario plays out, `LocationDto.scenarios` lists the scenarios staged there, each spoiler-gated on the linked entity's `discoveryChapter`. The other 14 entities use the generic `PagedEntityRepository`; Scenario and Location register subclasses that override its `DetailQuery` hook (see §5.1) to eager-load the pivot for single-entity reads while list reads stay lean.

### Content management (`/api/content-types`, `/api/content`)

Every content type — Character and the 16 encyclopedic entities — is created, edited and deleted through one generic, schema-driven API (see §5.2).

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| GET    | `/api/content-types`           | R | Schema for all 17 types (fields, kinds, relations) |
| GET    | `/api/content-types/{type}`    | R | Schema for one type |
| GET    | `/api/content/{type}/{pageId}` | R | Raw (un-rendered) field + relation values, for an edit form |
| POST   | `/api/content/{type}`          | E | Create a page of that type |
| PUT    | `/api/content/{type}/{pageId}` | E | Update a page |
| DELETE | `/api/content/{type}/{pageId}` | E | Delete a page (cascades to its entity + pivots) |

`{type}` is the snake_case `EntityType` (`character`, `demon_king`, …). Editor writes apply immediately and are logged as auto-approved `EditSuggestion` rows. Body for POST/PUT: `{ changes: { fields, relations }, reason? }` — `fields` maps descriptor field names to values, `relations` carries `add` / `update` / `remove` ops on pivot collections.

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
| POST   | `/`                      | R    | Submit (`{ operation, entityType, pageId?, proposedChanges, reason? }`) |
| GET    | `/mine?page&pageSize`    | R    | My submissions                                  |
| GET    | `/?status&page&pageSize` | E    | Review queue                                    |
| GET    | `/{id:long}`             | E    | Detail                                          |
| POST   | `/{id:long}/approve`     | E    | Apply the change via ContentMutationService + notify submitter |
| POST   | `/{id:long}/reject`      | E    | Notify submitter                                |


**`operation`** is `Update` (edit an existing page) or `Create` (propose a new page) — readers may submit either; `Delete` is editor-only via the content API. **`proposedChanges`** is a `{ fields, relations }` diff: `fields` maps any descriptor field of the target type to its new value, `relations` carries `add` / `update` / `remove` ops on pivot collections. On approval the diff is applied by the same `ContentMutationService` the editor content API uses, so any field of any of the 17 types is handled — no longer Page-only. Editor content writes are recorded here as auto-approved rows, so this endpoint is also the wiki's change history.

### Notifications (`/api/notifications`)


| Method | Path              | Auth | Purpose                            |
| ------ | ----------------- | ---- | ---------------------------------- |
| GET    | `/?page&pageSize` | R    | List mine, newest first            |
| GET    | `/unread-count`   | R    | `{ count: N }`                     |
| POST   | `/{id:long}/read` | R    | Mark single read                   |
| POST   | `/read-all`       | R    | Bulk mark via `ExecuteUpdateAsync` |


### Timeline (`/api/timeline`)


| Method | Path                        | Auth | Purpose                                                                       |
| ------ | --------------------------- | ---- | ----------------------------------------------------------------------------- |
| GET    | `/?upToChapter&characterId` | R    | Full graph payload — worldlines (skeleton), filtered events, worldline jumps  |

**Payload shape:** `{ worldlines: WorldlineNodeDto[], events: EventNodeDto[], jumps: JumpEdgeDto[] }`. Every worldline is always returned so the renderer can draw lanes even when no event or jump matches the filter. Jumps are gated by their `Arc.ChapterStart` when present (or always shown when not arc-linked); they don't carry a chapter directly. `characterId` filter applies to events only — `Jump.CharacterLabel` is an opaque display string, not an FK.


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
    "Postgres": "Host=localhost;Port=5432;Database=orv_wiki;Username=medardduris;Password="
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

The committed default is the laptop's local Postgres (no password, trust auth). For the docker stack the connection string is overridden via the `ConnectionStrings__Postgres` env var in `docker-compose.yml`; for Railway it references `${{Postgres.PGHOST}}` / `…PGUSER` / `…PGPASSWORD` from the managed Postgres service. See §12 for the full docker / cloud wiring.

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

### No rate limiting

Anyone with valid credentials can spam `POST /api/comments`, `POST /api/edit-suggestions`, etc. Add `Microsoft.AspNetCore.RateLimiting` policies before production.

### CORS is permissive in Development, locked in non-Development

`Program.cs` registers a default CORS policy:

- **Development:** `SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()` — reflects whichever Origin the request brought (including `null` for `file://`), so local frontends opened from disk or any localhost port can call the API.
- **Non-Development:** `WithOrigins("https://claude.ai")…` — single origin allowlist.

**Caveat for the deployed stack:** because the Railway API runs with `ASPNETCORE_ENVIRONMENT=Development` (so seed data and OpenAPI also work), the production CORS is effectively permissive. If you ever flip the Railway env to `Production`, the GitHub Pages frontend will get blocked — either add `https://medardwork.github.io` to the production allowlist, or keep Development mode.

### Notification push is best-effort

If SignalR push fails (user disconnected), it's logged at Warning and swallowed. The notification row is persisted, so a refresh of `GET /api/notifications` will surface it. Acceptable given how SignalR reconnects work.

### Cache TTL only on slug lookups

Single-page reads benefit. List endpoints (`GET /api/pages`, `GET /api/characters`) hit the DB every call. Add output caching with `AddOutputCache()` if list calls become hot.

### `dotnet ef` requires PATH update

After `dotnet tool install --global dotnet-ef`, the binary lives at `~/.dotnet/tools/dotnet-ef`. Add `~/.dotnet/tools` to your `PATH` in `~/.zshrc` if not already.

### Default `dotnet new webapi` template files

Removed from `ORVWiki.API/`: `WeatherForecast.cs`, `Controllers/WeatherForecastController.cs`, `ORVWiki.API.http`. If you re-scaffold the project, delete them again.

---

## 10. The 12-phase roadmap (history)

Each phase shipped self-contained. The final solution is the union of all twelve.


| Phase                  | What it added                                                                                                                                                                                                                                                                                                                                                                  |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1 — Foundation         | 3-project solution, EF Core/Npgsql, all 39 tables + relations + constraints, snake_case naming, initial migration                                                                                                                                                                                                                                                            |
| 2 — Auth               | `User`/`Role` endpoints, JWT with `current_chapter` claim, BCrypt password hashing, 3 cumulative role policies, `DbInitializer` role seeding, exception middleware                                                                                                                                                                                                             |
| 3 — Core wiki CRUD     | `IRepository<T>` generic + `Repository<T>` base, `PageRepository` and `CharacterRepository` with spoiler-aware queries, services + DTOs + validators, `PagesController` + `CharactersController`, pagination types                                                                                                                                                             |
| 4 — Spoiler system     | `SpoilerService` with `IsRevealed`/`EnsureRevealed` (page gate) + `RenderInline` (inline `[spoiler ch=N]` parser → server-enforced segments), `Character.Biography` switched from raw string to `RenderedContent`                                                                                                                                                              |
| 5 — Community features | Comments (threading + spoiler filter on `chapter_at_post` + soft delete), `CommentReaction` toggle (per-type unique), `Bookmark` toggle, `EditSuggestion` workflow with diff apply + notifications, `Notification` CRUD, `ForbiddenException` (403) added                                                                                                                      |
| 6 — Timeline           | `TimelineService` returning `{Worldlines, Events, Jumps}` graph payload, optional `upToChapter` and `characterId` filters. Originally edges were `EventConnection` pivot rows; reworked in migration `WorldlineJumpsRefactor` (May 2026) to first-class `Jump` rows that go worldline-to-worldline.                                                                                |
| 7 — Polish             | Serilog structured logging + request logging, Scalar UI for OpenAPI with Bearer auth, `IMemoryCache` for popular pages with explicit invalidation, SignalR `NotificationsHub` + `INotificationPusher` abstraction, JWT-from-query for WebSocket upgrades, switched `EnqueueAsync` → `PublishAsync` (save-then-push semantics)                                                  |
| 8 — Universal spoilers | `IPagedEntity` marker + open-generic `PagedEntityRepository<T>` + abstract `PagedEntityReadService<T,TDto,TListItemDto>`. `Page.Title` and `Page.ShortDescription` switched from raw string to `RenderedContent`. 16 new read-only endpoints (`/api/arcs`, `/api/items`, `/api/stigmas`, …) — every visible string field across the wiki now respects `[spoiler ch=N]` markup. |
| 9 — Timeline rework    | `EventConnection` pivot dropped. New `Jump` entity (worldline-to-worldline edges with `CharacterLabel`, optional `Description` / `LengthEstimate` / `ArcId`). `TimelineDto` shape became `{Worldlines, Events, Jumps}`. Migration: `20260509205046_WorldlineJumpsRefactor`.                                                                                                       |
| 10 — Containerization  | Multi-stage Dockerfile for the API (.NET 10 publish → aspnet runtime, listens on `$PORT` for cloud hosts). Nginx-based frontend image with an envsubst template config so `PORT` / `API_HOST` / `API_PORT` are runtime-configurable. `docker-compose.yml` for local dev + `docker-compose.prod.yml` for handoff. Permissive-in-Dev CORS in `Program.cs` added in support.        |
| 11 — Cloud deployment  | API + managed Postgres on Railway, static frontend on GitHub Pages, multi-arch images (`amd64`+`arm64`) pushed to GHCR (`ghcr.io/medardwork/orv-api`, `…/orv-web`). `js/config.js` carries the per-deployment API URL; `core.js` picks the right default based on hostname; localStorage overrides honored except when stale localhost saves block production.                  |
| 12 — Content management | Descriptor-registry + generic mutation engine: Character and the 16 read-only types become fully editable through `/api/content`. `EditSuggestion` generalized to Create/Update/Delete on any field of any type; editor writes auto-logged as change history. Schema-driven editor UI, generalized suggest modal + review queue on the frontend. Character's bespoke write code removed. Migration: `20260517071124_GeneralizeEditSuggestions`. |

---

## 11. Quick reference for future Claude sessions

When picking this project back up, here's the minimum to know:

- **3 projects, dependencies API → Application ← Infrastructure**. Application has no ASP.NET/EF deps.
- **39 tables**, every encyclopedic entity has 1:1 with `Page` (which holds `slug`, `title`, `discovery_chapter` — the spoiler gate field).
- **Auth = JWT bearer**, role claim + `current_chapter` claim. Three policies in `Program.cs`.
- **Spoiler enforcement is server-side**: `WHERE discovery_chapter <= currentChapter` at SQL, and `[spoiler ch=N]` segments with `Content: null` for hidden ones. Every visible string in every encyclopedic DTO goes through `SpoilerService.RenderInline`.
- **Add a new read-only entity** = implement `IPagedEntity`, drop in a `PagedEntityReadService<T,TDto,TListItemDto>` subclass, register the service. The repo is open-generic — no per-entity registration. See §5.1.
- **Content is editable through one generic API** — `/api/content` + a per-type descriptor in `ORVWiki.Application/Content/Descriptors/`; no per-type write code. See §5.2.
- **Notifications** = inject `INotificationService`, call `PublishAsync(...)` after your own `SaveChanges` succeeds.
- `**Attribute` entity needs a `using AttributeEntity = ...;` alias** when referenced alongside `using System;`. `**Dokkaebi` entity needs a `using DokkaebiEntity = ...;` alias** when its module's namespace is in scope (folder is named `Dokkaebis/` to reduce the chance of collision).
- **Cache** lives only on `PageService.GetVisibleBySlugAsync`. Invalidate with `cache.Remove(PageCacheKeys.BySlug(slug))` after any write that changes Page fields.
- **All exceptions in `Application/Common/Exceptions/`** are auto-mapped to status codes by `ExceptionHandlingMiddleware`.
- **Migration command** uses `--project ORVWiki.Infrastructure --startup-project ORVWiki.API`.
- **Build clean, no warnings**, host starts on `dotnet run --project ORVWiki.API`.
- **Container / cloud deployment lives in §12** — three-service docker stack (db + api + web), multi-arch GHCR images, Railway for API + Postgres, GitHub Pages for the static frontend.

---

## 12. Docker & cloud deployment

The full stack runs in three containers and ships to two hosts.

### 12.1 Local stack — `docker compose`

Three services defined in `docker-compose.yml`:

| Service | Image | Role |
| --- | --- | --- |
| `db`   | `postgres:16-alpine` (pulled from Docker Hub) | Postgres with named volume `pgdata`, exposed on host `:5432` for direct connection from Rider/DBeaver |
| `api`  | built from `ORVWiki.API/Dockerfile`           | .NET 10 ASP.NET Core, listens on container `:8080`, host port `:5080` (macOS reserves `:5000` for Control Center) |
| `web`  | built from `docker/web/Dockerfile`            | Nginx serving `index.html` + `styles.css` + `js/`, reverse-proxies `/api/*`, `/hubs/*`, `/openapi`, `/scalar` to `api:8080` — gives the browser a single origin, no CORS |

Run:

```bash
docker compose up -d            # build (if needed) + start
docker compose up -d --build    # force rebuild after code changes
docker compose logs -f api      # tail API logs
docker compose stop             # stop (keep data volume)
docker compose down -v          # stop AND wipe pgdata volume
```

Then open **http://localhost:8080**. First boot runs EF migrations + seed (Arc01_ThreeWaysToSurvive + Backbone_WorldlineJumps) automatically because `ASPNETCORE_ENVIRONMENT=Development` is set in compose.

Defaults / overridable via `.env` next to the compose file (see `.env.example`):

```
POSTGRES_DB=orv_wiki
POSTGRES_USER=orv
POSTGRES_PASSWORD=orv
JWT_SIGNING_KEY=…at least 32 chars…
```

### 12.2 The two compose files

| File | When | Source of images |
| --- | --- | --- |
| `docker-compose.yml`      | Developing locally               | `build:` from source for `api` and `web` |
| `docker-compose.prod.yml` | Handing the stack to someone else | `image: ghcr.io/medardwork/orv-{api,web}:latest` — recipient doesn't need the source |

A grader gets one file and one command:

```bash
docker compose -f docker-compose.prod.yml up -d
```

### 12.3 Image registry — GHCR

Multi-arch images (`linux/amd64` + `linux/arm64`) are published at:

- `ghcr.io/medardwork/orv-api:latest`
- `ghcr.io/medardwork/orv-web:latest`

Both packages are **public** under https://github.com/MedardWork?tab=packages so no token is needed to pull them.

**Why multi-arch:** building on an Apple Silicon Mac produces `arm64` by default; Railway and most cloud hosts run `amd64`. A single-arch push would silently fail on Railway with "There was an error deploying from source." Always build both via `docker buildx`:

```bash
docker buildx create --name multiarch --driver docker-container --bootstrap --use   # one-time

docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t ghcr.io/medardwork/orv-api:latest \
  -f ORVWiki.API/Dockerfile \
  --push .

docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t ghcr.io/medardwork/orv-web:latest \
  -f docker/web/Dockerfile \
  --push .
```

To push you need a PAT with `write:packages`:

```bash
echo 'ghp_…' | docker login ghcr.io -u MedardWork --password-stdin
```

### 12.4 Image internals worth knowing

**API image (`ORVWiki.API/Dockerfile`)** is a two-stage build:

- `sdk:10.0` stage restores csproj files first (cached as long as deps don't change), then `dotnet publish -c Release`.
- `aspnet:10.0` runtime stage copies `/app/publish`, switches to the base image's pre-existing non-root `app` user, and exits via:
  ```dockerfile
  ENTRYPOINT ["/bin/sh", "-c", "exec dotnet ORVWiki.API.dll --urls=http://+:${PORT:-8080}"]
  ```
  Locally `PORT` is unset → binds 8080; on Railway `$PORT` is injected → binds whatever Railway picks. `exec` keeps PID 1 on dotnet so signal handling works.

**Web image (`docker/web/Dockerfile`)** is plain `nginx:1.27-alpine` + the static assets + a **template** config:

- `docker/web/nginx.conf.template` uses `${PORT}`, `${API_HOST}`, `${API_PORT}` placeholders.
- nginx's official entrypoint runs envsubst on files in `/etc/nginx/templates/*.template` at container start.
- `ENV NGINX_ENVSUBST_FILTER="^(PORT|API_HOST|API_PORT)$"` whitelists ONLY those three names so nginx's own `$host` / `$remote_addr` / `$connection_upgrade` aren't replaced with empty strings.
- Defaults `PORT=80 API_HOST=api API_PORT=8080` make local docker-compose work out of the box (where the api service is reachable as `api:8080`).
- `docker/web/connection_upgrade.conf` defines the `$connection_upgrade` map needed for SignalR WebSocket upgrades; it's NOT a template (sits in `/etc/nginx/conf.d/` directly).

### 12.5 Cloud topology

```
                        ┌─────────────────────────┐
   medardwork.github.io │  GitHub Pages (static)  │
    /ORV-Wiki/index.html│  index.html + js/ + css │
                        └────────────┬────────────┘
                                     │ fetch / WebSocket
                                     ▼
                        ┌─────────────────────────┐
                        │  Railway: orv-api       │
                        │  ghcr.io/.../orv-api    │
                        │  $PORT, ASPNETCORE_ENV=Dev │
                        └────────────┬────────────┘
                                     │ private network
                                     ▼
                        ┌─────────────────────────┐
                        │  Railway: Postgres      │
                        │  managed plugin, SSL    │
                        │  PGHOST / PGUSER / …    │
                        └─────────────────────────┘
```

The **web container is NOT deployed to the cloud** — GitHub Pages serves the static files directly. The nginx image only matters for the local docker stack.

### 12.6 Railway — API service env vars

Set on the `api` service in Railway:

| Variable | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` |
| `ConnectionStrings__Postgres` | `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}};SSL Mode=Require;Trust Server Certificate=true` |
| `Jwt__Issuer` | `orv-wiki` |
| `Jwt__Audience` | `orv-wiki-clients` |
| `Jwt__SigningKey` | ≥32-char random secret |
| `Jwt__AccessTokenMinutes` | `60` |

**Important quirks:**

- **Development is intentional.** Production mode disables the seed (`DbInitializer.InitializeAsync` is in an `if (IsDevelopment)` block), locks CORS to `https://claude.ai`, and activates `UseHttpsRedirection` which causes a `Failed to determine the https port` warning behind Railway's TLS termination.
- **Railway batches variable edits.** After adding/changing a variable you must click the **Deploy** button at the top — otherwise the container keeps the old values. `RAILWAY_ENVIRONMENT_NAME` is Railway's own metadata (`production` / `staging` label); it does NOT affect `ASPNETCORE_ENVIRONMENT`.
- **`ASPNETCORE_URLS` is not needed.** The image entrypoint already binds to `$PORT`. If both are set, the `--urls` cmd arg wins — same outcome either way.
- **Data Protection key warning is benign.** Keys live in `/home/app/.aspnet/DataProtection-Keys` (ephemeral). Only affects cookies/antiforgery, neither of which this app uses — JWT validation uses `Jwt:SigningKey` from config and survives container restarts.

### 12.7 Frontend — GitHub Pages

Pages serves from `main` / `/ (root)`. The entry is `index.html` (renamed from the original `orv-wiki-frontend.html` for Pages auto-discovery). The repo root has plenty of non-frontend files (Dockerfiles, csprojs); Pages just ignores them.

URL: **https://medardwork.github.io/ORV-Wiki/** (the trailing slash matters — without it relative asset paths break).

#### How the frontend picks its API base

Three-tier fallback in `js/core.js` (`State.apiBase` initializer):

1. **localStorage `orv.apiBase`** wins — unless it's empty, or it looks local (`localhost` / `127.0.0.1` / `0.0.0.0` / any bare `a.b.c.d` IP) AND the page itself isn't being served from a local origin. That carve-out exists because users who tested locally and saved `https://localhost:7138` would otherwise be stuck (the in-app Settings dialog is login-gated, creating a chicken-and-egg).
2. **Hostname-based default** otherwise:
   - `file://` → `https://localhost:7138` (Kestrel dev URL)
   - **local origin** (`localhost` / `127.0.0.1` / `0.0.0.0`, or a private-LAN IP — `192.168.x`, `10.x`, `172.16–31.x`) → `''` when served on port `8080` (docker-compose's nginx proxies `/api` and `/hubs` same-origin), otherwise `http://<hostname>:5044` (Kestrel's HTTP dev URL — covers `python -m http.server`, Live Server, or opening the site from another device on the LAN)
   - Anywhere else → `window.ORV_API_BASE` from `js/config.js`

`js/config.js` is loaded **before** `js/core.js` and holds one line:

```js
window.ORV_API_BASE = 'https://orv-api-production.up.railway.app';
```

Edit that single line when the API URL changes; push to `main` and GitHub Pages auto-rebuilds in 30-60 seconds.

#### Pages build pitfalls (already fixed, documented for future-you)

- **Broken submodule gitlink.** `actions/checkout` failed with `No url found for submodule path '.claude/worktrees/distracted-bouman-c3788e' in .gitmodules`. Fixed by `git rm --cached` on the gitlink and adding `.claude/worktrees/` (plus `bin/`, `obj/`, etc.) to `.gitignore`.
- **Cached 404.** After enabling Pages, browsers cache the "There isn't a GitHub Pages site here" page. Hard refresh or incognito to dodge.
- **CORS confusion.** If the API logs show `Hosting environment: Production`, the deployed CORS policy locks to `claude.ai` only and the Pages frontend can't reach it. Fix is just to set `ASPNETCORE_ENVIRONMENT=Development` and redeploy — see §12.6.

### 12.8 Update flow

| Change | What to do |
| --- | --- |
| .NET code | Rebuild + push `orv-api` multi-arch (§12.3), then **Redeploy** the api service in Railway |
| nginx config / static frontend (for local stack) | Rebuild + push `orv-web` multi-arch (§12.3) — only matters if you want others' local stacks to pick up the change |
| Frontend (for production) | `git push origin main` — GitHub Pages rebuilds automatically |
| DB schema | New EF migration as in §8; the api container runs `Database.MigrateAsync()` on startup in Dev mode, so a Railway redeploy applies it |
| Railway env vars | Edit in Variables tab → click the staged **Deploy** button (it's not automatic) |

### 12.9 Files map

```
/.dockerignore                       ← API context filter (frontend files left in for the web image)
/.env.example                        ← compose defaults (POSTGRES_*, JWT_SIGNING_KEY)
/docker-compose.yml                  ← local dev: build from source
/docker-compose.prod.yml             ← handoff: pull from GHCR
/ORVWiki.API/Dockerfile              ← multi-stage .NET 10 build
/docker/web/Dockerfile               ← nginx + static assets + envsubst template
/docker/web/nginx.conf.template      ← templated nginx server block (PORT/API_HOST/API_PORT)
/docker/web/connection_upgrade.conf  ← $connection_upgrade map for WebSocket proxying
/index.html                          ← entry HTML (formerly orv-wiki-frontend.html)
/js/config.js                        ← single source of the production API URL
/js/core.js                          ← hostname-aware State.apiBase initializer
```

