PhoneNumberAPI

REST API сервис для хранения и валидации телефонных номеров с использованием PostgreSQL.

Построен на ASP.NET Core Web API (.NET 8).

Используется в проекте JobParser для автоматической проверки и дедупликации телефонов из вакансий.

Основной функционал

Валидация телефонов для JobParser

PhoneNumberAPI выполняет централизованную валидацию всех телефонных номеров:

JobParser извлекает телефоны из объявлений

Отправляет их в PhoneNumberAPI для проверки

API валидирует формат номера (через Google libphonenumber)

Проверяет дубликаты в базе данных

Возвращает статус:

201 Created — номер новый, сохранён в БД (объявление добавляется в CSV)

409 Conflict — номер уже существует (объявление отклоняется как дубликат)

400 Bad Request — номер невалидный (объявление отклоняется)

Это позволяет избежать дублирования лидов между запусками парсера и обеспечить чистоту базы контактов.

Эндпоинты

POST /PhoneNumber/check_number

Принимает номер телефона с кодом страны и проверяет его наличие в базе данных.

Что делает:

Если номер уже есть в базе — возвращает 409 Conflict

Если номер новый — сохраняет его и возвращает 201 Created

Если номер невалидный — возвращает 400 Bad Request

GET /PhoneNumber/all

Возвращает список всех сохранённых номеров.

GET /PhoneNumber/stats

Возвращает статистику:

Общее количество номеров

Количество добавленных за сегодня

Информация о базе данных

Валидация

Используется библиотека Google libphonenumber (PhoneNumbers)

Номер обязан содержать код страны, например: +1, +380, +44

Поддерживаются американские и международные номера

Перед сохранением номер приводится к формату E.164, например: +12125551234

Дубли исключены на уровне базы данных через уникальный индекс

Фильтрация невалидных номеров

API автоматически отклоняет:

Номера без кода страны

Слишком короткие номера (< 7 цифр)

Слишком длинные номера (> 15 цифр)

Некорректные комбинации цифр

Технологии

ASP.NET Core Web API (.NET 8)

Entity Framework Core

PostgreSQL через Npgsql

PhoneNumbers (порт Google libphonenumber)

Swagger/OpenAPI

Конфигурация

В файле appsettings.json укажите строку подключения к своей базе данных:

JSON

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=phone_db;Username=postgres;Password=ВАШ_ПАРОЛЬ"
  }
}

Запуск

Требования:

.NET 8 SDK

PostgreSQL 16 или выше

Шаги:

Создайте базу данных PostgreSQL с именем phone_db:

SQL

CREATE DATABASE phone_db;

Обновите строку подключения в appsettings.json

Примените миграции:

Bash

dotnet ef database update

Запустите проект:

Bash

dotnet run

Swagger UI откроется по адресу: https://localhost:7249/swagger

Примеры запросов и ответов

Проверка номера

Запрос:

http

POST /PhoneNumber/check_number

Content-Type: application/json

{
  "phoneNumber": "+12125551234"
}

Номер добавлен — 201 Created:

JSON

{
  "message": "Phone number added successfully.",
  "number": "+12125551234",
  "region": "US"
}

Номер уже существует — 409 Conflict:

JSON

{
  "error": "Phone number already exists.",
  "number": "+12125551234",
  "region": "US"
}

Невалидный номер — 400 Bad Request:

JSON

{
  "error": "Invalid phone number format. Please include country code (e.g. +1, +380, +44)."
}

Получение всех номеров

Запрос:

Bash

curl http://localhost:7249/PhoneNumber/all

Ответ:

JSON

[
  {
    "id": 1,
    "number": "+12125551234",
    "createdAt": "2026-01-15T20:45:00Z"
  },
  {
    "id": 2,
    "number": "+380671234567",
    "createdAt": "2026-01-15T21:30:00Z"
  }
]

Статистика

Запрос:

Bash

curl http://localhost:7249/PhoneNumber/stats
Ответ:

JSON

{
  "totalPhones": 1523,
  "addedToday": 45,
  "database": "phone_db",
  "timestamp": "2026-01-15T22:00:00Z"
}

Структура проекта

text

PhoneNumberAPI/

├── Controllers/

│   └── PhoneNumberController.cs    (HTTP эндпоинты)

├── Data/

│   └── AppDbContext.cs              (контекст Entity Framework)

├── Models/

│   ├── PhoneNumber.cs               (модель телефона)

│   └── CheckNumberRequest.cs        (DTO запроса)

├── Services/

│   └── PhoneValidationService.cs    (валидация через libphonenumber)

├── Migrations/                      (миграции БД)

├── appsettings.json                 (конфигурация)

├── Program.cs                       (точка входа)

└── README.md

Интеграция с JobParser

PhoneNumberAPI автоматически используется в JobParser для проверки дубликатов телефонов.

Настройка в JobParser:

В файле appsettings.json JobParser укажите:

JSON

{
  "ParserSettings": {
    "UseExternalPhoneApi": true,
    "PhoneCheckApiUrl": "http://localhost:7249/PhoneNumber/check_number"
  }
}

Как это работает:

JobParser парсит вакансию и извлекает телефоны

Отправляет каждый номер в PhoneNumberAPI

API проверяет формат и дубликаты

Возвращает статус:

201 → номер новый → вакансия сохраняется

409 → дубликат → вакансия отклоняется

400 → невалидный → вакансия отклоняется

Это обеспечивает автоматическую дедупликацию лидов между запусками парсера.

Структура БД

Таблица phone_numbers:

Поле	Тип	Описание

Id	INT	Автоинкремент, первичный ключ

Number	VARCHAR	Телефон в формате E.164, уникальный индекс

CreatedAt	TIMESTAMP	Дата добавления

Индексы:

Уникальный индекс на поле Number — предотвращает дубликаты на уровне БД

Индекс на поле CreatedAt — ускоряет запросы статистики

Логирование

API использует встроенное логирование ASP.NET Core:

text

[2026-01-15 20:45:37] [INF] Phone number added: +12125551234

[2026-01-15 20:46:12] [WRN] Duplicate phone number: +12125551234

[2026-01-15 20:47:05] [ERR] Invalid phone number: 1234567

Уровень логирования настраивается в appsettings.json:

JSON

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}

Производительность

Проверка номера: ~10-50 мс

Валидация формата: ~5 мс (Google libphonenumber)

Запрос в БД: ~5-10 мс (благодаря индексу на Number)

Поддерживаемая нагрузка: до 1000 запросов/сек на среднем сервере

Безопасность

Все подключения к БД используют параметризованные запросы (защита от SQL-инъекций)

Валидация входных данных через встроенные механизмы ASP.NET Core

Уникальный индекс в БД предотвращает race conditions при одновременных запросах

Типовые проблемы

PhoneNumberAPI недоступен

Убедитесь, что API запущен:

Bash

dotnet run

Проверьте доступность:

Bash

curl http://localhost:7249/PhoneNumber/stats

Ошибка подключения к БД

Проверьте:

PostgreSQL запущен

База данных phone_db существует

Строка подключения в appsettings.json правильная

Миграции применены: dotnet ef database update

Все номера возвращают 400

Проверьте формат номера — он должен включать код страны:

Правильно: +12125551234

Неправильно: 2125551234

Связанные проекты
JobParser — парсер вакансий с автоматической проверкой телефонов через PhoneNumberAPI
