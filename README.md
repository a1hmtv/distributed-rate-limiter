![Distributed Rate Limiter](https://capsule-render.vercel.app/api?type=waving&color=0:FFF0F5,50:F0B5CF,100:D77EA5&height=260&section=header&text=distributed%20rate%20limiter&fontSize=49&fontColor=672344&fontAlignY=38&desc=EVENT-DRIVEN%20BACKEND%20SYSTEM&descSize=17&descAlignY=60)

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-963B67?style=for-the-badge">
  <img alt=".NET 9" src="https://img.shields.io/badge/.NET_9-AC4D77?style=for-the-badge&logo=dotnet&logoColor=white">
  <img alt="gRPC" src="https://img.shields.io/badge/gRPC-B74D70?style=for-the-badge">
  <img alt="Kafka" src="https://img.shields.io/badge/Apache_Kafka-672344?style=for-the-badge&logo=apachekafka&logoColor=white">
  <img alt="Redis" src="https://img.shields.io/badge/Redis-963B67?style=for-the-badge&logo=redis&logoColor=white">
  <img alt="MongoDB" src="https://img.shields.io/badge/MongoDB-AC4D77?style=for-the-badge&logo=mongodb&logoColor=white">
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-B74D70?style=for-the-badge&logo=postgresql&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Docker-672344?style=for-the-badge&logo=docker&logoColor=white">
</p>

## ✧ О проекте

Распределённая backend-система на C# для управления пользовательскими данными и ограничения частоты обращений к API. Правила доступа по частоте запросов хранятся отдельно от пользовательских данных, обновляются в работающем приложении и применяются к событиям о запросах пользователей.

Проект демонстрирует событийную архитектуру, синхронное взаимодействие через gRPC, асинхронную обработку событий Kafka, работу с несколькими хранилищами и использование фоновых сервисов .NET.

## ✧ Функциональные возможности

- CRUD-операции с пользователями, поиск и проверка уникальности логина.
- Управление лимитами запросов в минуту для отдельных методов API.
- Начальная загрузка правил в потокобезопасный кеш и обновление через MongoDB Change Streams.
- Подсчёт событий по пользователю и методу API в скользящем временном окне.
- Временное ограничение доступа при превышении лимита с использованием Redis TTL.
- Проверка идентификатора пользователя и блокировки в gRPC-interceptors.
- Консольный генератор событий с динамическим управлением заданиями и частотой отправки.

## ✧ Архитектура

| Компонент | Ответственность |
|---|---|
| [UserService](UserService/) | Пользовательские данные, gRPC API, проверка блокировки перед обработкой запроса |
| [RateLimiter.Writer](RateLimiter.Writer/) | Управление правилами RPM и запись в MongoDB |
| [RateLimiter.Reader](RateLimiter.Reader/) | Кеш правил, обработка событий, подсчёт запросов и установка блокировок |
| [UserRequestsKafkaGenerator](UserRequestsKafkaGenerator/) | Генерация событий для демонстрации нагрузки |

### Обработка запроса

Генератор моделирует поток обращений к API. В текущей версии UserService самостоятельно не публикует события запросов в Kafka.

## ✧ Технологический стек

| Направление | Инструменты |
|---|---|
| Язык и платформа | C#, .NET 9, ASP.NET Core |
| RPC | gRPC, Protocol Buffers |
| Реляционная БД | PostgreSQL, Npgsql, Dapper, SQL-функции и индексы |
| Документная БД | MongoDB.Driver, Change Streams |
| События | Apache Kafka, Confluent.Kafka |
| Временное состояние | Redis, StackExchange.Redis, MemoryCache |
| Маппинг и валидация | Mapperly, FluentValidation |
| Инфраструктура | Docker, Docker Compose |

## ✧ Инженерные решения

- Разделение прикладной, доменной, инфраструктурной логики и уровня представления.
- Repository Pattern, Dependency Injection, разделение gRPC-, доменных и DB-моделей.
- Асинхронная потоковая загрузка правил через `IAsyncEnumerable`.
- Фоновые обработчики, cancellation tokens и потокобезопасный кеш.
- Скользящее окно учёта событий и блокировки с ограниченным временем жизни.
- SQL-функции, индексы и soft delete для пользовательских данных; soft delete для правил.

## ✧ Запуск локально

Потребуются Docker с Compose и .NET 9 SDK для запуска генератора вне контейнеров. Все опубликованные порты привязаны к localhost; конфигурация предназначена только для локальной разработки.

```bash
cp .env.example .env
# При необходимости задайте локальный пароль PostgreSQL в .env.
docker compose config --quiet
docker compose up --build -d
docker compose ps -a
```

Сервисы `mongodb-init` и `kafka-init` выполняют одноразовую инициализацию и при успехе завершаются с кодом 0. При ошибках используйте `docker compose logs <service>`.

| Сервис | Адрес на хосте |
|---|---|
| User Service (gRPC) | localhost:8080 |
| Writer (gRPC) | localhost:5001 |
| Reader (gRPC) | localhost:5002 |
| Kafka | localhost:9092 |
| PostgreSQL | localhost:5433 |
| MongoDB | localhost:27018 (для диагностики, directConnection=true) |
| Redis | localhost:6379 |

Запуск генератора в отдельном терминале:

```bash
dotnet run --project UserRequestsKafkaGenerator/UserRequestsKafkaGenerator.csproj
```

В интерактивной консоли доступны `add`, `update`, `remove`, `list`, `help`, `exit`. Переменные `KAFKA_BOOTSTRAP_SERVERS` и `KAFKA_TOPIC` позволяют изменить подключение и топик.

## ✧ API и проверка работы

Контракты доступны в исходных `.proto` файлах. Для ручной проверки можно использовать grpcurl (без server reflection, с явным указанием контракта) или gRPC-клиент IDE.

| API | Методы |
|---|---|
| [UserGrpc](UserService/Presentation/Protos/user.proto) | CreateUser, GetUserById, GetUserByName, UpdateUser, DeleteUser |
| [Writer](RateLimiter.Writer/Protos/writer-service.proto) | CreateLimit, GetLimitByRoute, UpdateLimit, DeleteLimit |
| [Reader](RateLimiter.Reader/Protos/reader-service.proto) | Ping, GetAllLimits |

Пример создания правила для `GetUserById` (на чистой MongoDB):

```bash
grpcurl -plaintext -import-path RateLimiter.Writer/Protos \
  -proto writer-service.proto \
  -d '{"route":"GetUserById","requestsPerMinute":6}' \
  localhost:5001 writer.Writer/CreateLimit
```

В уже настроенной базе используйте `UpdateLimit` для существующего правила. Проверьте, что Reader получил правило через `GetAllLimits` (контракт `reader-service.proto`).

Пример gRPC-запроса к UserService с демонстрационным идентификатором:

```bash
grpcurl -plaintext -import-path UserService/Presentation/Protos \
  -proto user.proto -H 'user_id: 42' -d '{"id":1}' \
  localhost:8080 users.UserGrpc/GetUserById
```

Для успешного чтения сначала создайте тестового пользователя и используйте возвращённый ID. Запускайте генератор, чтобы проверить появление блокировки:

```text
add demo 42 GetUserById 18
list
```

При превышении настроенного лимита ожидается gRPC-ошибка `ResourceExhausted`. Снимите нагрузку командой `remove demo`; после истечения TTL доступ должен восстановиться. Отсутствующий либо некорректный `user_id` должен приводить к `Unauthenticated`. Это сценарии ручной проверки, а не результаты выполненных автоматических тестов.

Остановить инфраструктуру:

```bash
docker compose down
```

## ✧ Статус и ограничения

- Исходный проект разработан в учебных целях в команде. Описание отражает систему целиком, а не индивидуальный вклад одного автора.
- В архиве нет отдельных проектов автоматических тестов. Сборку и интеграционный запуск подготовленной версии необходимо проверить локально (`dotnet build RateLimiter.sln` и сценарии выше).
- События о нагрузке генерируются отдельно, лимитирование асинхронное; возможна задержка между превышением лимита и блокировкой.
- `user_id` — доверенный демонстрационный заголовок, не полноценная аутентификация. Пароли в исходной доменной модели хранятся и передаются без безопасного хеширования; использовать только вымышленные данные. Writer не защищён авторизацией.
- Счётчики запросов хранятся в памяти одного Reader; нет распределённых счётчиков, гарантированного восстановления состояния, дедупликации событий и полноценного восстановления Change Stream после сбоя. Между загрузкой кеша и открытием подписки возможен пропуск изменений.
- Конфигурация инфраструктуры ориентирована на локальное окружение, а не production.

---

*Командный учебный проект по разработке распределённых backend-систем.*

![Footer](https://capsule-render.vercel.app/api?type=waving&color=0:FFF0F5,50:F0B5CF,100:D77EA5&height=110&section=footer&text=%E2%99%A1&fontSize=38&fontColor=672344&fontAlign=50&fontAlignY=72)
