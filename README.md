# Практическая работа №8 — JWT (по примеру из методички)

Решение повторяет порядок методички: `PasswordHasher<User>` для регистрации, проверка пароля, `GenerateJwtToken()` в `AuthController`, настройка JWT в `Program.cs` и `[Authorize]` на `TasksController`. Вместо `List<User>` из обучающего примера используется **MS SQL Server**, как требуется в конце задания.

## Структура

- `Controllers/AuthController.cs` — регистрация, вход, метод генерации JWT.
- `Controllers/TasksController.cs` — CRUD задач с проверками владельца и роли.
- `Models/User.cs`, `Models/Task.cs`, `Models/Pr8Context.cs` — EF Core модели и контекст.
- `Database/01_create_tables.sql` — SQL таблицы `Users` и `Tasks`.
- `Program.cs` — EF Core, PasswordHasher, JWT и создание администратора.

**Нет** папок `DTO`, `Data`, сервисов и отдельного класса создания администратора. `Pr8Context` нужен EF Core для подключения к базе и обычно генерируется при Database-First.

## 1. Создать таблицы

На `sql.ects` (в базе, к которой есть доступ) выполните `Database/01_create_tables.sql`. Важно: база в задании не названа, нужно указать именно свою, не `master`.

## 2. Настроить секреты (в папке проекта)

```powershell
dotnet restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Server=sql.ects;Database=ВАША_БАЗА;User Id=student_00;Password=student_00;Encrypt=True;TrustServerCertificate=True;'
dotnet user-secrets set "Jwt:Key" "ReplaceThisWithARandomSecretOfAtLeast32Bytes"
```

Замените `ВАША_БАЗА` и JWT-ключ. `TrustServerCertificate=True` — только для учебного сервера. `UserSecretsId` уже указан в `.csproj`, поэтому `dotnet user-secrets init` повторять не требуется. Пароль администратора — `admin123` / `P@s$w0rd` — задаётся в `Program.cs`, где он хешируется перед сохранением.

## 3. Database-First

В методичке требуется **генерация моделей из базы**, а не миграции. Для выполнения этого требования после создания таблиц и настройки подключения выполните:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.33
dotnet ef dbcontext scaffold "Server=sql.ects;Database=ВАША_БАЗА;User Id=student_00;Password=student_00;Encrypt=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --context Pr8Context --context-dir Models --output-dir Models --namespace pr8_TRPO.Models --context-namespace pr8_TRPO.Models --table dbo.Users --table dbo.Tasks --no-onconfiguring --force
```

В комплекте есть модели, соответствующие схеме скрипта, чтобы был виден код до подключения к серверу. **Они не были фактически сгенерированы из учебной БД** — нужно выполнить эту команду у себя, чтобы выполнить требование Database-First. После генерации модели/контекст будут заменены кодом EF Core. Это ожидаемо.

## 4. Запустить

```powershell
dotnet build
dotnet run --launch-profile https
```

В Development доступен Swagger (`/swagger`). Примеры всех запросов есть в `pr8_TRPO.http`. Для защищённых запросов передавайте заголовок `Authorization: Bearer <JWT>`.

## Маршруты

| HTTP | Адрес | Кто может |
|---|---|---|
| POST | `/api/v1/auth/register` | Все; роль `User`, при дубликате 409 |
| POST | `/api/v1/auth/login` | Все; возвращает `access_token` или 401 |
| GET | `/api/v1/tasks` | Авторизованный; только свои задачи |
| GET | `/api/v1/tasks/{id}` | Владелец / Admin; 403 или 404 |
| POST | `/api/v1/tasks` | Авторизованный; 201, `OwnerLogin` из JWT |
| PUT | `/api/v1/tasks/{id}` | Владелец / Admin; 200 |
| DELETE | `/api/v1/tasks/{id}` | Владелец / Admin; 204 |
| GET | `/api/v1/tasks/all` | Только Admin; иначе 403 |

Без JWT защищённые методы возвращают 401. В текущей среде нет .NET SDK и подключения к `sql.ects`, поэтому фактическую сборку и интеграционные тесты нужно выполнить на своей машине.
