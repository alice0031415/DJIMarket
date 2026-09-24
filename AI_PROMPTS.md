# AI prompts

## 20:36 — ChatGPT / GPT-5.6 Luna
Спроектируй и реализуй целиком тестовое DJI-Market Sales Performance Dashboard за ограниченный timebox: .NET 10, ASP.NET Core, EF Core, PostgreSQL, React+TypeScript, Docker Compose. Нужны модель данных, migrations, reproducible seed, server-side analytics API, dashboard UI, loading/error/empty states, tests, README и AI journal. Старайся не переусложнять архитектуру и соблюдай business rules из ТЗ.

## 20:40 — ChatGPT / GPT-5.6 Luna
Реализуй backend: Manager, Customer, Category, Product, Sale, SaleItem; Paid/Cancelled/Refunded; KPI, ranking, trend, category/product analytics и recent sales. Revenue и profit считай на сервере, без загрузки сырых продаж во frontend. Диапазон дат делай [from,to), предыдущий период такой же длины.

## 20:45 — ChatGPT / GPT-5.6 Luna
Сделай reproducible seed с 15–25 менеджерами, 50–100 клиентами, десятками товаров и тысячами продаж за минимум 6 месяцев, с неравномерностью, сезонностью, крупными/мелкими сделками, Cancelled/Refunded и разной маржинальностью.

## 20:52 — ChatGPT / GPT-5.6 Luna
Сделай современный desktop SaaS dashboard 1440×900: 6 KPI, period presets + custom range, chart динамики, категории, ranking с Gross Profit/Average Check, top products, recent sales, loading/error/empty states и умеренные микро-анимационные ощущения.

## 21:05 — ChatGPT / GPT-5.6 Luna
Проверь edge cases из ТЗ: менеджер без продаж должен присутствовать в ranking, одинаковые значения должны давать стабильную сортировку, даты работают как [from,to), а Recent Sales должны показывать Paid/Cancelled/Refunded без поломки dashboard.

## 21:12 — ChatGPT / GPT-5.6 Luna
Доведи проект до сдаваемого состояния: проверь структуру, Docker-конфигурацию, TypeScript импорты/типы, README, тесты и git history; не добавляй сложную архитектуру без необходимости.

