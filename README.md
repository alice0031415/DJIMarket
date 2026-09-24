# DJI-Market — Sales Performance Dashboard

Desktop B2B/SaaS dashboard для анализа продаж менеджеров.

## Stack

- Frontend: React, TypeScript, Ant Design, TanStack Query, Recharts
- Backend: C#, .NET 10, ASP.NET Core, EF Core 10, REST
- Data: PostgreSQL 16
- Run: Docker Compose

## Run

```bash
docker compose up --build
```

Открыть: http://localhost:5173

Swagger: http://localhost:8080/swagger

## Business rules

- `Paid` — единственный статус, который участвует в Revenue, Gross Profit, Margin, Average Check и ranking.
- `Cancelled` — исключается из аналитики.
- `Refunded` — трактуется как полностью возвращённая продажа и исключается из текущей финансовой аналитики; при этом она отображается в Recent Sales.
- Revenue = `sum(SalePrice * Quantity)` по Paid.
- Cost = `sum(CostPrice * Quantity)` по Paid.
- Gross Profit = Revenue − Cost.
- Margin = Gross Profit / Revenue; при Revenue = 0 значение равно 0.
- Average Check = Revenue / количество Paid sales; при 0 продаж значение равно 0.
- API принимает диапазон `[from, to)`, то есть `to` является эксклюзивной датой.
- Previous period имеет ту же длину, что и текущий, и начинается непосредственно перед ним.
- Ranking: Gross Profit либо Average Check.

## API

`GET /api/dashboard?from=2026-08-26&to=2026-09-25&rankBy=profit`

Backend выполняет фильтрацию и агрегирование; frontend не загружает несколько тысяч raw sales.

## Data model

Manager → Sale → SaleItem → Product → Category; Sale также ссылается на Customer.

Индексы добавлены под периодную аналитику и часто используемые join/filter paths:

- `sales (SoldAt, Status)`
- `sales (ManagerId, SoldAt, Status)`
- `sales (CustomerId)`
- `sale_items (SaleId)`
- `sale_items (ProductId)`
- `products (CategoryId, IsActive)`

## Seed

Seed запускается автоматически после migration. Используется фиксированный `Random(20260924)`, reference period: 2026-01-01 … 2026-09-24. Данные намеренно неравномерные: есть разные менеджеры, чеки, маржинальность, сезонность, отмены/возвраты и крупные сделки.

## Tests

Backend tests покрывают основные вычислительные правила: margin, average check и period-over-period change.

```bash
dotnet test backend/tests/DJIMarket.Tests/DJIMarket.Tests.csproj
```

## What is intentionally out of scope

Auth, mobile layout, admin panel, Kubernetes, cloud deployment, microservices.

## What I would improve next in production

- server-side pagination/filtering for Recent Sales;
- pre-aggregated daily/monthly facts for much larger datasets;
- cancellation/refund reason dimensions;
- richer manager comparison and drill-down pages;
- OpenTelemetry + structured audit/diagnostics;
- API contract tests and Playwright E2E;
- secrets via environment/secret store instead of compose literals.

## 8-hour prioritization

Core business result, server-side analytics, polished desktop UX and one-command startup were prioritized over infrastructure and feature breadth.
