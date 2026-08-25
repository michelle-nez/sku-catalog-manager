# SKU Catalog Manager

A small product catalog for e-commerce SKUs - add, find, correct, and retire the
products you list. Built to practice EF Core and SQL Server schema design.

![Product list](screenshots/product-list.png)

## Stack

- .NET 10, Blazor Server (interactive server rendering)
- Entity Framework Core 10
- SQL Server (LocalDB in development)

## What it does

- Products and categories in two related SQL Server tables
- Unique index on SKU, enforced by the database rather than the screen
- Soft delete - retiring a product hides it and keeps the record
- Server-side validation driven by data annotations
- Responsive layout - the table becomes stacked cards on a phone

## The schema

```
dbo.Categories
    Id              int, primary key, identity
    Name            nvarchar(60), not null

dbo.Products
    Id              int, primary key, identity
    Sku             nvarchar(40), not null, UNIQUE index
    Name            nvarchar(200), not null
    Price           decimal(18,2), not null
    Quantity        int, not null
    IsActive        bit, not null
    CreatedUtc      datetime2, not null
    CategoryId      int, not null, foreign key -> Categories.Id
```

`Price` is `decimal(18,2)`, not a floating point type - money has to be exact.
Categories are seeded by the migration so the dropdown is never empty on a fresh
database.

## Project layout

| Project | What it holds |
|---|---|
| `SkuCatalog.Web` | Blazor Server app - the screens |
| `SkuCatalog.Data` | Models, `CatalogDbContext`, and EF Core migrations |

The reference points one way only: Web references Data, never the reverse.

The context is registered with `AddDbContextFactory`, not `AddDbContext`. Blazor
Server components are long-lived and several can run at once, so each operation
gets its own short-lived context instead of sharing one.

## Running it locally

1. Open `SkuCatalogManager.sln` in Visual Studio 2026.
2. Right-click `SkuCatalog.Web` and choose **Manage User Secrets**.
3. Add a `ConnectionStrings:CatalogDatabase` value pointing at your local
   SQL Server LocalDB instance (`(localdb)\MSSQLLocalDB`), with the database
   named `SkuCatalogManager` and a trusted connection. See
   `appsettings.json` for the setting's shape.

4. In the Package Manager Console, set **Default project** to `SkuCatalog.Data`
   and run `Update-Database`.
5. Run the project and open `/products`.

The connection string lives in User Secrets only. `appsettings.json` keeps a blank
placeholder so the setting's shape is documented without a value in the repo.

## What I would do next

- Server-side paging once the catalog outgrows a single page
- Search and filter by category
- A view for retired products, with a way to bring one back
- Unit tests over the save logic

---

Self-directed portfolio project.
