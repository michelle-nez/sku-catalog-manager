# Getting started

From a fresh clone to a running app. Roughly ten minutes, most of it waiting for
NuGet.

## Prerequisites

| Requirement | Notes |
|---|---|
| Visual Studio 2026 | With the **ASP.NET and web development** workload |
| .NET 10 SDK | Installed with that workload; `dotnet --version` should report 10.x |
| SQL Server LocalDB | Ships with the Visual Studio workload. Any SQL Server instance works — only the connection string changes |

Visual Studio is not strictly required — the CLI steps below cover everything — but
the project is set up around it, and Package Manager Console is the smoother path for
migrations.

Check LocalDB is present:

```powershell
sqllocaldb info
```

`MSSQLLocalDB` should be listed. If the command is not found, LocalDB was not
installed with the workload — add **SQL Server Express LocalDB** through the Visual
Studio Installer under Individual components.

## 1. Clone and open

```bash
git clone https://github.com/michelle-nez/sku-catalog-manager.git
cd sku-catalog-manager
```

Open `SkuCatalogManager.sln`. Visual Studio restores the NuGet packages on load;
`SkuCatalog.Web` is already the startup project.

## 2. Add the connection string

**The app will not run until this is done.** `appsettings.json` ships the key with an
empty value so the shape is documented without a credential in the repository:

```json
"ConnectionStrings": {
  "CatalogDatabase": ""
}
```

The real value goes in **User Secrets**, which live outside the repository in your
Windows user profile and are never committed.

**In Visual Studio** — right-click `SkuCatalog.Web` → **Manage User Secrets**, then:

```json
{
  "ConnectionStrings": {
    "CatalogDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=SkuCatalogManager;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Or from the CLI**, run from the solution folder:

```bash
dotnet user-secrets set "ConnectionStrings:CatalogDatabase" "Server=(localdb)\MSSQLLocalDB;Database=SkuCatalogManager;Trusted_Connection=True;TrustServerCertificate=True" --project SkuCatalog.Web
```

`Trusted_Connection=True` means Windows authentication, so there is no password to
store anywhere. That is deliberate — the development setup has no credential to leak.

## 3. Create the database

Nothing creates the database automatically. There is no `Migrate()` or
`EnsureCreated()` call in `Program.cs`, so a fresh clone has no schema until the
migration is applied by hand.

**Package Manager Console** — set **Default project** to `SkuCatalog.Data`, and leave
the startup project as `SkuCatalog.Web`:

```powershell
Update-Database
```

**Or from the CLI**, naming both projects:

```bash
dotnet ef database update --project SkuCatalog.Data --startup-project SkuCatalog.Web
```

Both projects have to be named because the migrations live in `SkuCatalog.Data` while
the EF Core tools and the connection string live in `SkuCatalog.Web`.

If `dotnet ef` is not recognized:

```bash
dotnet tool install --global dotnet-ef
```

This creates two tables, both indexes, the foreign key, and inserts four seed
categories. Full detail in [database.md](database.md).

## 4. Run it

Press **F5**, or:

```bash
dotnet run --project SkuCatalog.Web
```

| Profile | URL |
|---|---|
| `http` | http://localhost:5187 |
| `https` | https://localhost:7185 (also serves 5187) |

Both profiles set `ASPNETCORE_ENVIRONMENT=Development`.

On first run over HTTPS the development certificate may need trusting:

```bash
dotnet dev-certs https --trust
```

## 5. Check it actually works

A fresh database has **four categories and no products**, so:

1. Open `/products` — it should show the empty state, "No products yet", not an error.
2. Click **Add product**. The category dropdown should list Wall Plates, Cables,
   Adapters and Accessories. If it is empty, the seed data did not apply — the
   migration probably did not run.
3. Save a product with a SKU, name, category, price and quantity. It appears in the list.
4. Add a second product **reusing the same SKU**. It should be rejected with
   "SKU 'X' is already in use." — that message means the database's unique index is
   doing its job.
5. Retire the first product. It disappears from the list, but the row is still there —
   soft delete, not a delete.

If all five behave, the app and the database are both wired up correctly.

## Current deployment state

**This application is not deployed anywhere, and has no deployment configuration.**

Verified in the repository: no publish profile (`.pubxml`), no `Dockerfile`, no
`.github/workflows`, no `appsettings.Production.json`. It runs on a developer machine
against LocalDB.

That is the whole truth of it today. The section below describes what deployment
*would* involve, and none of it is set up.

## Optional future deployment

**Nothing here is implemented.** It is recorded so the gap is explicit rather than
implied.

Because this is Blazor Server, three constraints apply that would not apply to a
static site:

- It needs a real .NET host that keeps a **live SignalR connection** open. Static
  hosts (GitHub Pages, Netlify) cannot run it at all.
- It needs a **reachable SQL Server**, not LocalDB. LocalDB is a developer-only
  instance and does not exist on a server.
- **Sticky sessions** matter if it is ever scaled to more than one instance, because
  each circuit is bound to the server that created it.

A minimal deployment would need:

1. A .NET 10 host — a hosting provider's IIS site, Azure App Service, or a container.
2. A SQL Server database on that host, and the connection string supplied through the
   host's own configuration or environment variables, **never in the repository**.
3. The migration applied against that database — either by running
   `dotnet ef database update` against it, or by generating a SQL script with
   `dotnet ef migrations script` and running it manually. Applying migrations
   automatically at startup is deliberately not done here; it would let a deploy alter
   a production schema without anyone choosing to.
4. `ASPNETCORE_ENVIRONMENT` set to `Production`, which is what switches on
   `UseExceptionHandler` and HSTS.

## Where to go next

| Document | Covers |
|---|---|
| [architecture.md](architecture.md) | Projects, layers, rendering model, page flow |
| [database.md](database.md) | Schema, entities, indexes, migrations, seed data, ER diagram |
