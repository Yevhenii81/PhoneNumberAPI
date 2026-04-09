PhoneNumberApi

REST API сервис для хранения и валидации телефонных номеров с использованием PostgreSQL
Построен на ASP.NET Core Web API (.NET 8)


================================================
Эндпоинт
================================================

POST /phonenumber/check_number

Принимает номер телефона с кодом страны и проверяет его наличие в базе данных

Что делает:
- Если номер уже есть в базе — возвращает 409 Conflict
- Если номер новый — сохраняет его и возвращает 201 Created
- Если номер невалидный — возвращает 400 Bad Request


================================================
Валидация
================================================

- Используется библиотека Google libphonenumber
- Номер обязан содержать код страны, например +1, +380, +44
- Поддерживаются американские и международные номера
- Перед сохранением номер приводится к формату E.164, например +12125551234
- Дубли исключены на уровне базы данных через уникальный индекс


================================================
Технологии
================================================

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- PostgreSQL через Npgsql
- PhoneNumbers (порт Google libphonenumber)


================================================
Конфигурация
================================================

В файле appsettings.json укажи строку подключения к своей базе данных:

    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=phone_db;Username=postgres;Password=ВАШ_ПАРОЛЬ"
      }
    }


================================================
Запуск
================================================

Требования:
- .NET 8 SDK
- PostgreSQL 18

Шаги:
1. Создём базу данных PostgreSQL с именем phone_db
2. Обновим строку подключения в appsettings.json
3. Применим миграции: dotnet ef database update
4. Запустим проект: dotnet run
5. Swagger UI откроется по адресу https://localhost:{port}/swagger


================================================
Примеры запросов и ответов
================================================

Запрос:

    POST /phonenumber/check_number
    {
      "phoneNumber": "+12125551234"
    }

Номер добавлен — 201 Created:

    {
      "message": "Phone number added successfully.",
      "number": "+12125551234",
      "region": "US"
    }

Номер уже существует — 409 Conflict:

    {
      "error": "Phone number already exists.",
      "number": "+12125551234",
      "region": "US"
    }

Невалидный номер — 400 Bad Request:

    {
      "error": "Invalid phone number format. Please include country code (e.g. +1, +380, +44)."
    }


================================================
Структура проекта
================================================

PhoneNumberApi/
    Controllers/
        PhoneNumberController.cs
    Data/
        AppDbContext.cs
    Models/
        PhoneNumber.cs
        CheckNumberRequest.cs
    Services/
        PhoneValidationService.cs
    Migrations/
    appsettings.json
    Program.cs
