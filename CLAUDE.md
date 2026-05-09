# hiking-backend — CLAUDE.md

## Project Overview

ASP.NET Core 8.0 REST API for the hiking journal app.
Handles posts (CRUD), photos, GPX files, gear lists, and tags.
Uses PostgreSQL via Supabase + Dapper for data access, Supabase Storage for files.

## Tech Stack

| Layer | Tool |
|---|---|
| Framework | ASP.NET Core 8.0 |
| Database | PostgreSQL (Supabase) |
| Data Access | Dapper 2.1.72 + Npgsql 10.0.2 |
| Object Mapping | Mapster 10.0.7 |
| Image Processing | SixLabors.ImageSharp 3.1.12 + ffmpeg |
| File Storage | Supabase Storage (S3-compatible) |
| API Docs | Swagger / Swashbuckle 6.6.2 |

## Dev Commands

```bash
dotnet build hiking-backend.sln
dotnet run --project hiking.WebApi/hiking.WebApi.csproj   # http://localhost:5000
dotnet watch run --project hiking.WebApi/hiking.WebApi.csproj
```

## Environment

Fill in `hiking.WebApi/appsettings.Development.json`:

```json
{
  "Supabase": {
    "Url": "https://<project>.supabase.co",
    "ServiceKey": "<service role JWT>",
    "ConnectionString": "Host=...;Database=postgres;Username=postgres;Password=..."
  }
}
```

**External dependency**: `ffmpeg` must be installed and in PATH (used for cover image compression).

## Project Structure

```
hiking-backend/
├── hiking.WebApi/                 # Entry point
│   ├── Controllers/               # PostsController, PhotosController, GpxController, GearsController, TagsController
│   ├── RequestModel/              # Request DTOs (CreatePostRequest, UpdatePostRequest, …)
│   └── Program.cs                 # DI registration, middleware, CORS
├── hiking.Service/                # Business logic
│   ├── Services/                  # PostService, PhotoService, GpxService, StorageService, TagService, GearService
│   ├── Commands/                  # CreatePostCommand, UpdatePostCommand
│   └── DTOs/                      # PostDetailDto, GearInputDto …
└── hiking.Repository/             # Data access
    ├── Repositories/              # PostRepository, PhotoRepository, GearRepository, TagRepository
    ├── Model/                     # PostModel, PhotoModel, GearModel, FileData
    └── supabase/migrations/       # SQL migration files (apply via Supabase Dashboard → SQL Editor)
```

## API Endpoints

| Method | Route | Body | Description |
|--------|-------|------|-------------|
| GET | `/api/posts` | — | List all posts |
| GET | `/api/posts/{id}` | — | Post detail (post + photos + gears) |
| POST | `/api/posts` | multipart/form-data | Create post |
| PUT | `/api/posts/{id}` | JSON | Update post |
| DELETE | `/api/posts/{id}` | — | Soft-delete post |
| DELETE | `/api/photos/{id}` | — | Delete a photo |
| POST | `/api/photos/{id}/cover` | multipart/form-data | Replace cover image |
| POST | `/api/photos/{id}/photos` | multipart/form-data | Add photos |
| POST | `/api/gpx/{id}` | multipart/form-data | Replace GPX file |
| GET | `/api/gears` | — | All gears (across all posts) |
| GET | `/api/tags` | — | All tags |
| POST | `/api/tags` | JSON | Create tag |
| GET | `/health` | — | DB connectivity check |

## Database Schema

### posts
| Column | Type | Notes |
|--------|------|-------|
| id | uuid PK | auto |
| title | text | not null |
| description | text | |
| cover_image | text | URL |
| compressed_cover_image | text | 20px-height thumbnail |
| gpx_file | text | URL |
| date_start | varchar | extracted from GPX or user input |
| date_end | varchar | |
| weather | text | |
| people_count | int | |
| tags | text[] | array of tag names |
| deleted_at | timestamptz | NULL = active (soft delete) |
| created_at | timestamptz | auto |

### photos
| Column | Type | Notes |
|--------|------|-------|
| id | uuid PK | |
| post_id | uuid FK | cascade delete |
| url | text | |
| created_at | timestamptz | auto |

### gears (裝備庫)
| Column | Type | Notes |
|--------|------|-------|
| id | uuid PK | |
| name | text | not null |
| weight | numeric | default weight |
| note | text | |
| category | varchar(20) | default '其他' |
| quantity | int | default 1 |
| brand | varchar(100) | |
| reference_url | text | |
| price | int | |
| added_at | text | |

### gears_mapping_post (裝備 ↔ 紀錄關聯)
| Column | Type | Notes |
|--------|------|-------|
| id | uuid PK | used as gear identifier by frontend |
| post_id | uuid FK | → posts.id, cascade delete |
| gear_id | uuid FK | → gears.id, cascade delete |
| weight | numeric | post-specific weight override |
| quantity | int | post-specific quantity |

### tags
| Column | Type | Notes |
|--------|------|-------|
| id | uuid PK | |
| name | text | unique |
| created_at | timestamptz | auto |

## Key Patterns

**Snake_case ↔ PascalCase auto-mapping** — Set once in `Program.cs`, no manual column aliases needed:
```csharp
DefaultTypeMap.MatchNamesWithUnderscores = true;
```

**Strict layering: Repository → Service → Controller**
- Repositories own all SQL (raw Dapper, never in services)
- Services own business logic and file orchestration
- Controllers are thin (validate → call service → return)

**File upload flow**
1. Controller receives `IFormFile` → wraps as `FileData` record
2. Service calls `StorageService.UploadAsync(bucket, path, fileData)`
3. StorageService POSTs to Supabase Storage REST API
4. Returns public URL → stored in DB column

**Image compression** (cover images only)
- ffmpeg spawned as subprocess, scales to 20px height
- Compressed URL stored in `compressed_cover_image`

**Soft delete** — `DELETE /api/posts/{id}` sets `deleted_at = NOW()`.
All list/detail queries filter `WHERE deleted_at IS NULL`.

**Bulk gear operations** — `PUT /api/posts/{id}` JSON body accepts:
```json
{
  "gearsToAdd":      [ { "name": "...", "weight": 0 } ],
  "gearsToUpdate":   [ { "id": "…",   "name": "...", "weight": 0 } ],
  "gearIdsToDelete": [ "uuid" ]
}
```
Applied in sequence inside `PostService.UpdatePostAsync`.

**PostgreSQL array queries**
```csharp
WHERE id = ANY(@Ids)   // pass IEnumerable<Guid> as parameter
```

**CORS** — Development allows `http://localhost:5173` (Vite). Configured in `Program.cs`.
