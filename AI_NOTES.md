# AI notes

- Использован GPT-5.6 Luna для end-to-end реализации.
- Архитектура сознательно оставлена компактной: Domain / Application / Infrastructure / API.
- CQRS, generic repositories и микросервисы не вводились, потому что задача маленькая.
- Основная продуктовая логика спроектирована до генерации кода: только Paid влияет на финансовые KPI.
- Refunded выбран как полностью исключаемый из финансовой аналитики статус; он остаётся виден в Recent Sales.
- Диапазон дат сделан [from,to), чтобы не зависеть от времени 23:59:59.
- Previous period имеет ту же длину, что и текущий период.
- Analytics API возвращает агрегаты; frontend не загружает 2–5 тысяч raw sales.
- Для period analytics добавлены составные индексы по date/status и manager/date/status.
- Seed фиксированным Random(20260924), поэтому после пересоздания базы данные повторяются.
- AI использовался для генерации большого объёма boilerplate и seed-кода.
- Важные SQL/EF выражения следует проверить через generated SQL на реальном запуске.
- В текущем окружении отсутствуют dotnet SDK и Docker, поэтому полный runtime smoke test здесь недоступен.
- Frontend должен показывать loading, API error и empty period вместо пустого белого экрана.
- Дальше production-улучшения: E2E, pre-aggregation, observability, secret management.
