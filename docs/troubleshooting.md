# Troubleshooting

Problems you can actually hit with this application, what causes each one, and how to
confirm the fix. Grouped by when they happen.

Almost every first-run problem is one of two things: **the connection string is not
set**, or **the migration has not been applied**.

## Setup and startup

### The app builds and starts, but any page touching data fails

Usually the connection string is missing. `appsettings.json` ships
`"CatalogDatabase": ""`, and nothing supplies a value until you set a User Secret.

Confirm what the app is actually reading:

```bash
dotnet user-secrets list --project SkuCatalog.Web
```

Expect a line beginning `ConnectionStrings:CatalogDatabase = Server=(localdb)\...`.
Nothing listed means no secret is set — see
[getting-started.md](getting-started.md#2-add-the-connection-string).

### "Cannot open database 'SkuCatalogManager' requested by the login"

The connection string is fine; the database does not exist yet. Nothing in this app
creates it — there is no `Migrate()` or `EnsureCreated()` call in `Program.cs`.

```bash
dotnet ef database update --project SkuCatalog.Data --startup-project SkuCatalog.Web
```

### I edited the connection string in appsettings.json and nothing changed

**User Secrets override `appsettings.json`.** In Development the secret wins, so the
file edit is ignored. Change the secret instead, and leave the committed file empty —
a real value there would get committed.

### "Unable to create an object of type 'CatalogDbContext'"

The EF Core tools are running against the wrong startup project. Migrations live in
`SkuCatalog.Data`, but the tools and the connection string live in `SkuCatalog.Web`,
so both projects must be named:

```bash
dotnet ef database update --project SkuCatalog.Data --startup-project SkuCatalog.Web
```

In Package Manager Console: **Default project** = `SkuCatalog.Data`, **solution
startup project** = `SkuCatalog.Web`. Setting only one of the two is the usual cause.

### "dotnet ef does not exist" / not recognized

```bash
dotnet tool install --global dotnet-ef
```

Then reopen the terminal so the tools path is picked up.

### `sqllocaldb info` is empty or the command is not found

LocalDB was not installed with the Visual Studio workload. Add **SQL Server Express
LocalDB** through the Visual Studio Installer → Individual components. `MSSQLLocalDB`
should then appear.

### The app will not start — "cannot run a class library"

`SkuCatalog.Data` has been set as the startup project. It is a class library and has
no entry point. Right-click **`SkuCatalog.Web`** → **Set as Startup Project**.

### Port already in use

The launch profiles bind 5187 (http) and 7185 (https). Another instance is usually
still running — check for a stray `dotnet` process, or a previous debug session that
did not shut down. Changing the port means editing
`SkuCatalog.Web/Properties/launchSettings.json`.

### HTTPS certificate warnings on first run

```bash
dotnet dev-certs https --trust
```

### Missing NuGet packages after cloning

Restore explicitly, then rebuild:

```bash
dotnet restore
dotnet build
```

If restore fails, the .NET 10 SDK may be missing — `dotnet --version` should report
10.x.

## Data and EF Core

### The category dropdown is empty

Seed data has not been applied. The four categories are inserted by the
`InitialCreate` migration, not at runtime, so an empty dropdown almost always means
the migration did not run — or ran against a different database than the app is using.

Compare the database name in your secret against the one you migrated. Both should be
`SkuCatalogManager`.

### "SKU 'X' is already in use" — but that SKU is not in the list

**This is expected behavior, not a bug.** Retiring a product is a soft delete: the row
stays, keeps its SKU, and the unique index still covers it. The list only shows
`IsActive = true` rows, so the conflicting product is invisible while still holding
the SKU.

Confirm it directly:

```sql
SELECT Id, Sku, Name, IsActive FROM Products WHERE Sku = 'X';
```

An `IsActive = 0` row is the culprit. Fully explained in
[database.md](database.md#one-consequence-worth-knowing).

### Every save failure says "SKU is already in use"

**A known issue in the code, not a configuration problem.** `ProductEdit.SaveAsync`
catches `DbUpdateException` and reports all of them as a duplicate SKU. A timeout, a
dropped connection or a foreign key violation all produce the same misleading message.

If the SQL above shows no conflicting row, look at the real exception — set a
breakpoint in the `catch`, or check the console output. Listed under
[architecture.md → Recommendations](architecture.md#recommendations).

### Editing a seeded category name does nothing

Seed data is applied through `HasData`, which is part of the model, not a runtime
insert. Changing a name in `OnModelCreating` requires a **new migration** to generate
the `UpdateData` statement:

```bash
dotnet ef migrations add RenameCategory --project SkuCatalog.Data --startup-project SkuCatalog.Web
dotnet ef database update --project SkuCatalog.Data --startup-project SkuCatalog.Web
```

### "A second operation was started on this context instance"

This should not happen here — the app uses `AddDbContextFactory` and each operation
opens its own context precisely to avoid it. If it appears, something has been changed
to `AddDbContext`, or a context is being held across `await` boundaries instead of
created inside the method. The correct pattern is:

```csharp
await using var db = await DbFactory.CreateDbContextAsync();
```

### Two people edited the same product and one change vanished

Expected with the current schema. There is no concurrency token, so the last save
wins. Noted under [architecture.md → Recommendations](architecture.md#recommendations).

## UI and runtime

### The page heading is hidden under the app bar

A `pt-*` class has been added to `MudMainContent`. That overrides the padding
MudBlazor uses to clear the fixed app bar, so content slides underneath. Put spacing
on the inner `MudContainer` instead. There is a comment in `MainLayout.razor` about
this, because it has been hit before.

### MudBlazor components render unstyled, or dialogs and snackbars never appear

One of the required pieces is missing. All of these must be present:

- `builder.Services.AddMudServices()` in `Program.cs`
- `@using MudBlazor` in `Components/_Imports.razor`
- `MudBlazor.min.css` and `MudBlazor.min.js` linked in `App.razor`
- `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider` and
  `MudSnackbarProvider` at the top of `MainLayout.razor`

Missing providers is the usual one: components render, but anything that overlays
silently does nothing.

### "Rejoining the server..." keeps appearing

The Blazor Server circuit dropped. This is normal after the app restarts during
debugging — reload the page. If it happens repeatedly while idle, something is
interrupting the WebSocket connection: a proxy, a VPN, or an aggressive firewall.

### The error page is plain and the headings are not red

**A known cosmetic issue.** `Error.razor` styles its headings with `text-danger`, a
Bootstrap class, and Bootstrap is not linked in `App.razor` — the class resolves to
nothing. `Error.razor` and `NotFound.razor` are both still stock template pages in an
otherwise MudBlazor app. See
[architecture.md → Recommendations](architecture.md#recommendations).

### An unknown product id shows a blank form

It should not — `ProductEdit` redirects to `/products` when the id matches no row. If
a blank form appears instead, check that `BlazorDisableThrowNavigationException` is
still `true` in `SkuCatalog.Web.csproj`; the redirect happens inside a lifecycle
method and depends on it.

### I get a detailed exception page in one environment and a plain one in another

Working as designed. The developer exception page is Development only; Production uses
`/Error` and shows nothing sensitive. Controlled by `ASPNETCORE_ENVIRONMENT` — see
[configuration.md](configuration.md#environments).

## Things people look for that are not here

- **Swagger / OpenAPI** — there is none. This is a Blazor Server app with no HTTP API,
  no controllers and no minimal-API endpoints. `/swagger` will 404, correctly.
- **A login page** — there is no authentication. Every page is public by design.
- **A retired products screen** — not built. Retired rows are only visible in SQL.

## Still stuck?

Work through the five checks at the end of
[getting-started.md](getting-started.md#5-check-it-actually-works). They isolate the
failure to setup, database, or application in a couple of minutes.
