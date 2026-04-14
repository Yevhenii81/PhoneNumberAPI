# PhoneNumberAPI

REST API сервис для хранения и валидации телефонных номеров с использованием PostgreSQL

Построен на ASP.NET Core Web API (.NET 8)

Используется в проекте JobParser для автоматической проверки и дедупликации телефонов из вакансий

---

## Основной функционал

### Валидация телефонов для JobParser

PhoneNumberAPI выполняет централизованную валидацию всех телефонных номеров:

- JobParser извлекает телефоны из объявлений  
- Отправляет их в PhoneNumberAPI для проверки  
- API валидирует формат номера (через Google libphonenumber)  
- Проверяет дубликаты в базе данных  

Возвращает статус:

- 201 Created — номер новый, сохранён в БД (объявление добавляется в CSV)  
- 409 Conflict — номер уже существует (объявление отклоняется как дубликат)  
- 400 Bad Request — номер невалидный (объявление отклоняется)  

Это позволяет избежать дублирования лидов между запусками парсера и обеспечить чистоту базы контактов

---

## Эндпоинты

### POST /PhoneNumber/check_number

Принимает номер телефона с кодом страны и проверяет его наличие в базе данных.

Что делает:

- Если номер уже есть в базе — возвращает 409 Conflict  
- Если номер новый — сохраняет его и возвращает 201 Created  
- Если номер невалидный — возвращает 400 Bad Request  

---

### GET /PhoneNumber/all

Возвращает список всех сохранённых номеров.

---

### GET /PhoneNumber/stats

Возвращает статистику:

- Общее количество номеров  
- Количество добавленных за сегодня  
- Информация о базе данных  

---

## Валидация

Используется библиотека Google libphonenumber (PhoneNumbers).

- Номер обязан содержать код страны (например: +1, +380, +44)  
- Поддерживаются международные номера  
- Перед сохранением номер приводится к формату E.164 (например: +12125551234)  
- Дубли исключены через уникальный индекс в базе данных  

---

## Фильтрация невалидных номеров

API автоматически отклоняет:

- Номера без кода страны  
- Слишком короткие номера (< 7 цифр)  
- Слишком длинные номера (> 15 цифр)  
- Некорректные комбинации цифр  

---

## Технологии

- ASP.NET Core Web API (.NET 8)  
- Entity Framework Core  
- PostgreSQL (Npgsql)  
- PhoneNumbers (Google libphonenumber)  
- Swagger / OpenAPI  
- Docker & Docker Compose  

---

## Конфигурация

В файле appsettings.json:

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=phone_db;Username=postgres;Password=ВАШ_ПАРОЛЬ"
  }
}

---

## Запуск

### Требования

- .NET 8 SDK  
- PostgreSQL 16+  

---

### Вариант 1: Локальный запуск

1. Создать БД:

CREATE DATABASE phone_db;

2. Обновить строку подключения

3. Применить миграции:

dotnet ef database update

4. Запустить:

dotnet run

Swagger:  
https://localhost:7249/swagger

---

### Вариант 2: Docker Compose

Что включено:

- PostgreSQL  
- PhoneNumberAPI  
- Health checks  
- Persistent storage  

Запуск:

docker-compose up -d

Остановка:

docker-compose down

Удаление данных:

docker-compose down -v

---

### Проверка

docker-compose ps  
docker-compose logs phonenumberapi  
curl http://localhost:7249/PhoneNumber/stats  

---

## Примеры запросов

### Проверка номера

POST /PhoneNumber/check_number  
Content-Type: application/json  

{
  "phoneNumber": "+12125551234"
}

---

### Ответы

201 Created

{
  "message": "Phone number added successfully.",
  "number": "+12125551234",
  "region": "US"
}

409 Conflict

{
  "error": "Phone number already exists."
}

400 Bad Request

{
  "error": "Invalid phone number format."
}

---

## Структура проекта

PhoneNumberAPI/  
├── Controllers/  
├── Data/  
├── Models/  
├── Services/  
├── Migrations/  
├── appsettings.json  
├── Dockerfile  
├── docker-compose.yml  
├── Program.cs  
└── README.md  

---

## Интеграция с JobParser

Настройка:

{
  "ParserSettings": {
    "UseExternalPhoneApi": true,
    "PhoneCheckApiUrl": "http://localhost:7249/PhoneNumber/check_number"
  }
}

---

### Как работает

- JobParser извлекает телефоны  
- Отправляет в API  
- API проверяет  
- Возвращает:

  - 201 → номер новый → вакансия сохраняется  
  - 409 → дубликат → вакансия отклоняется  
  - 400 → невалидный → вакансия отклоняется  

Это обеспечивает автоматическую дедупликацию лидов между запусками парсера
