# Практика 6

Простая студенческая работа на `Blazor WebAssembly` + `ASP.NET Core` + `Entity Framework Core` + `PostgreSQL`.

Что сделано:

- `PKS5.Client` - клиентская часть на Blazor WebAssembly;
- `PKS5.Server` - сервер с API `api/products`;
- `PKS5.Shared` - общая модель `Product`;
- вывод списка товаров;
- добавление товара;
- изменение товара;
- удаление товара с подтверждением;
- переключение светлой и темной темы;
- сохранение данных в PostgreSQL.

Методы API:

- `GET /api/products` - список товаров;
- `POST /api/products` - добавление товара;
- `PUT /api/products/{id}` - изменение товара;
- `DELETE /api/products/{id}` - удаление товара.

cd C:\Users\Mosen\Desktop\PKS\pks4
docker compose up -d postgres

cd C:\Users\Mosen\Desktop\PKS
.\.dotnet\dotnet.exe run --project .\pks5\PKS5.Server

http://localhost:5104