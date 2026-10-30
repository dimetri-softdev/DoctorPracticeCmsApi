# DoctorPracticeCmsApi

An ASP.NET Core API for managing a doctor's practice website and appointment requests. It uses SQL Server, EF Core migrations, ASP.NET Core Identity, JWT bearer authentication, and role-based authorization.

This starter manages public practice content and contact details for appointment follow-up. It is **not** an electronic health record: it does not collect diagnoses, symptoms, treatment notes, prescriptions, or clinical records. Appointment requests are requests for staff follow-up, not confirmed bookings. Protect appointment data and configure hosting, retention, access, and privacy controls for the laws and policies applicable to your practice before using real patient information.

## Requirements

- .NET 10 SDK
- SQL Server (SQL Server LocalDB is the default development connection on Windows)

## Configure and run locally

The development connection string is in `appsettings.Development.json`. To use another database, store its connection string in user secrets. Configure a private signing key and a practice administrator. Do not commit any secret values:

```powershell
dotnet user-secrets init --project .\DoctorPracticeCmsApi.csproj
dotnet user-secrets set "Jwt:Key" "<a random secret of at least 32 bytes>" --project .\DoctorPracticeCmsApi.csproj
dotnet user-secrets set "Jwt:Issuer" "DoctorPracticeCmsApi" --project .\DoctorPracticeCmsApi.csproj
dotnet user-secrets set "Jwt:Audience" "DoctorPracticeCmsApi.Client" --project .\DoctorPracticeCmsApi.csproj
dotnet user-secrets set "Admin:Email" "admin@example.com" --project .\DoctorPracticeCmsApi.csproj
dotnet user-secrets set "Admin:Password" "<a unique password of at least 12 characters>" --project .\DoctorPracticeCmsApi.csproj
```

Generate a signing key locally with:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

The application applies checked-in database migrations and ensures the Admin role exists at startup. When both administrator secrets are configured, it creates that administrator if needed. There is intentionally no public admin registration endpoint.

```powershell
dotnet restore .\DoctorPracticeCmsApi.slnx
dotnet run --project .\DoctorPracticeCmsApi.csproj
```

In Development, the OpenAPI document is available at `/openapi/v1.json`. Example requests are in `DoctorPracticeCmsApi.http`.

## Endpoints

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/login` | Public; administrator credentials only |
| GET | `/api/site` | Public; published website content aggregate |
| GET | `/api/pages`, `/api/pages/{slug}` | Public published content; Admin token can also view drafts |
| POST, PUT, DELETE | `/api/pages` | Admin |
| GET | `/api/services`, `/api/services/{slug}` | Public published services; Admin token can view drafts |
| POST, PUT, DELETE | `/api/services` | Admin |
| GET | `/api/faqs`, `/api/faqs/{id}` | Public published FAQs; Admin token can view drafts |
| POST, PUT, DELETE | `/api/faqs` | Admin |
| GET | `/api/doctors`, `/api/doctors/{id}` | Public published profiles; Admin token can view drafts |
| POST, PUT, DELETE | `/api/doctors` | Admin |
| POST | `/api/appointments` | Public, rate-limited appointment request |
| GET | `/api/appointments`, `/api/appointments/{id}` | Admin only |
| PATCH | `/api/appointments/{id}/status` | Admin only |

Appointment submissions collect only a name, email, phone number, and requested time; the requested time must be in the future and within the next year. The API does not promise a slot or automatically confirm requests. Login and public appointment submission are rate-limited. Admin login uses lockout after repeated failed passwords.

## Database migrations

Migrations live in `Data/Migrations`. To create a migration after changing data models:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.10
dotnet ef migrations add <MigrationName> --project .\DoctorPracticeCmsApi.csproj
```

The API applies checked-in migrations during startup. Set the production database connection string, JWT values, and administrator bootstrap credentials in the hosting service's protected configuration; never put production secrets in this repository.

## Tests

```powershell
dotnet test .\DoctorPracticeCmsApi.slnx
```

The xUnit integration tests use an isolated EF Core InMemory database.

## GitHub Actions

`.github/workflows/ci-cd.yml` runs tests and publishes the API on pushes and pull requests. A push to `main` deploys to Azure App Service only when the repository has both:

- Variable `AZURE_WEBAPP_NAME`: the App Service name.
- Secret `AZURE_WEBAPP_PUBLISH_PROFILE`: the App Service publish profile XML.

Configure the SQL Server connection string, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Admin__Email`, and `Admin__Password` in protected App Service configuration before deploying.
