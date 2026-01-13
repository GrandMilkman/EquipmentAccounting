# README.md

## Build and Run Commands

```bash
# Start PostgreSQL database (requires Docker)
cd .. && docker-compose up -d

# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release
```

## Technology Stack

- .NET 9.0 Windows Forms application
- Entity Framework Core 9.0.3 with PostgreSQL database
- PostgreSQL 16 in Docker (docker-compose.yml in parent directory)
- Database connection: `Host=localhost;Port=5432;Database=equipment_db;Username=equipment_user;Password=equipment_pass`

## Architecture Overview

This is a TV channel content management application for managing rights owners, films, contacts, and TV schedules. The UI is in Russian.

### Data Layer
- `Data/AppDbContext.cs` - EF Core DbContext with DbSets for all entities
- Database is auto-created on first run with seed data in `Program.cs`
- Uses lazy loading proxies for navigation properties

### Models
- `User` - Authentication with Login, Password, RoleId (foreign key to Role)
- `Role` - Permissions system with granular access controls
- `RightsOwner` - Rights holder (e.g., Беларусьфильм, Paramount)
- `Film` - Film entity with title, age restriction, duration, file path, purchase date, rights expiration, show count
- `Contact` - Contact information for rights sellers
- `TvScheduleEntry` - TV program schedule entries

### Roles and Permissions
- **Администратор** - Full system access
- **Инженер видеомонтажа** - Creates rights owners, adds films with basic info (title, age, duration, file path)
- **Специалист** - Edits rights info (purchase date, expiration, show count), manages contacts
- **Руководитель** - View-only access

### Forms Architecture
- `LoginForm` - Entry point with logo, authenticates against Users table
- `MainForm` - MDI container with role-based menu navigation and header with logo
- `Forms/CRUD/CrudForm<T>` - Generic abstract base class providing standardized CRUD UI

### Menu Structure
- **Контент** - Rights owners list, opens films for each owner
- **Контакты** - Contact management
- **Программа** - TV schedule with show count tracking
- **Администрирование** - User and role management (admin only)
- **Справка** - About dialog

### Key Features
- Role-based access control (RBAC) with granular permissions
- TV schedule with automatic show count decrement when films are aired
- Films cannot be added to schedule if show count is 0 or rights have expired
- Color-coded display for expired rights and low show counts
- Replaceable TV channel logo (`logo.png` in output directory)

### Utils
- `InputDialog` - Reusable prompt dialog for user input
- `SessionManager` - Static class for managing current user session and permission checks

## Default Credentials
- Admin: login=`admin`, password=`admin`
- Video Editor: login=`editor`, password=`editor`
- Specialist: login=`specialist`, password=`specialist`
- Manager: login=`manager`, password=`manager`

## Logo
Place a `logo.png` file in the application directory (or bin/Debug/net9.0-windows/) to display the TV channel logo on the login and main forms.


# Диаграммы системы учёта контента телеканала

## 1. ER-диаграмма базы данных (Entity-Relationship Diagram)

```mermaid
erDiagram
    Roles ||--o{ Users : "имеет"
    Contacts ||--o{ RightsOwners : "связан с"
    RightsOwners ||--o{ Films : "владеет"
    Films ||--o{ TvScheduleEntries : "показывается в"

    Roles {
        int Id PK
        string Name UK
        string Description
        bool CanManageUsers
        bool CanManageRoles
        bool CanCreateRightsOwners
        bool CanEditRightsOwners
        bool CanDeleteRightsOwners
        bool CanCreateFilms
        bool CanEditFilmBasicInfo
        bool CanEditFilmRightsInfo
        bool CanDeleteFilms
        bool CanManageContacts
        bool CanManageSchedule
        bool CanViewContent
        bool CanViewSchedule
        bool CanViewContacts
    }

    Users {
        int Id PK
        string Login UK
        string Password
        int RoleId FK
    }

    Contacts {
        int Id PK
        string CompanyName
        string Phone
        string Email
        string Address
        string ContactPerson
        string Notes
    }

    RightsOwners {
        int Id PK
        string Name
        string Description
        datetime DateAdded
        int ContactId FK
    }

    Films {
        int Id PK
        string Title
        string AgeRestriction
        int Duration
        string FilePath
        datetime PurchaseDate
        datetime RightsExpirationDate
        int ShowCount
        datetime DateAdded
        int RightsOwnerId FK
    }

    TvScheduleEntries {
        int Id PK
        int FilmId FK
        datetime ScheduledDateTime
        bool IsAired
        string Notes
        datetime CreatedAt
    }
```

## 2. Диаграмма архитектуры приложения (Component Diagram)

```mermaid
flowchart TB
    subgraph PresentationLayer["Слой представления (Presentation Layer)"]
        LoginForm["LoginForm<br/>Форма авторизации"]
        MainForm["MainForm<br/>MDI-контейнер"]

        subgraph CrudForms["CRUD-формы"]
            RightsOwnersForm["RightsOwnersForm"]
            FilmsForm["FilmsForm"]
            ContactsForm["ContactsForm"]
            TvScheduleForm["TvScheduleForm"]
            UsersForm["UsersForm"]
            RolesForm["RolesForm"]
        end

        AboutForm["AboutForm"]
    end

    subgraph BusinessLogicLayer["Слой бизнес-логики (Business Logic Layer)"]
        SessionManager["SessionManager<br/>Управление сессией"]
        InputDialog["InputDialog<br/>Диалоги ввода"]
        CrudFormBase["CrudForm&lt;T&gt;<br/>Базовый класс CRUD"]
    end

    subgraph DataLayer["Слой данных (Data Layer)"]
        AppDbContext["AppDbContext<br/>Контекст БД"]

        subgraph Models["Модели данных"]
            User["User"]
            Role["Role"]
            RightsOwner["RightsOwner"]
            Film["Film"]
            Contact["Contact"]
            TvScheduleEntry["TvScheduleEntry"]
        end
    end

    subgraph Infrastructure["Инфраструктура"]
        PostgreSQL[("PostgreSQL 16<br/>Docker")]
        EFCore["Entity Framework Core 9.0"]
    end

    LoginForm --> SessionManager
    MainForm --> SessionManager
    CrudForms --> CrudFormBase
    CrudFormBase --> AppDbContext
    SessionManager --> User
    SessionManager --> Role
    AppDbContext --> Models
    AppDbContext --> EFCore
    EFCore --> PostgreSQL
```

## 3. Диаграмма классов моделей (Class Diagram)

```mermaid
classDiagram
    class User {
        +int Id
        +string Login
        +string Password
        +int RoleId
        +Role Role
    }

    class Role {
        +int Id
        +string Name
        +string Description
        +bool CanManageUsers
        +bool CanManageRoles
        +bool CanCreateRightsOwners
        +bool CanEditRightsOwners
        +bool CanDeleteRightsOwners
        +bool CanCreateFilms
        +bool CanEditFilmBasicInfo
        +bool CanEditFilmRightsInfo
        +bool CanDeleteFilms
        +bool CanManageContacts
        +bool CanManageSchedule
        +bool CanViewContent
        +bool CanViewSchedule
        +bool CanViewContacts
        +ICollection~User~ Users
    }

    class Contact {
        +int Id
        +string CompanyName
        +string Phone
        +string Email
        +string Address
        +string ContactPerson
        +string Notes
        +ICollection~RightsOwner~ RightsOwners
    }

    class RightsOwner {
        +int Id
        +string Name
        +string Description
        +DateTime DateAdded
        +int? ContactId
        +Contact Contact
        +ICollection~Film~ Films
    }

    class Film {
        +int Id
        +string Title
        +string AgeRestriction
        +int Duration
        +string FilePath
        +DateTime? PurchaseDate
        +DateTime? RightsExpirationDate
        +int ShowCount
        +DateTime DateAdded
        +int RightsOwnerId
        +RightsOwner RightsOwner
        +ICollection~TvScheduleEntry~ ScheduleEntries
        +bool HasValidRights
    }

    class TvScheduleEntry {
        +int Id
        +int FilmId
        +Film Film
        +DateTime ScheduledDateTime
        +bool IsAired
        +string Notes
        +DateTime CreatedAt
    }

    Role "1" --> "*" User : contains
    Contact "1" --> "*" RightsOwner : linked to
    RightsOwner "1" --> "*" Film : owns
    Film "1" --> "*" TvScheduleEntry : scheduled in
```

## 4. Диаграмма вариантов использования (Use Case Diagram)

```mermaid
flowchart TB
    subgraph Actors["                                          Актёры                                          "]
        direction LR
        Admin(("Администратор"))
        VideoEditor(("Инженер<br/>видеомонтажа"))
        Specialist(("Специалист"))
        Manager(("Руководитель"))
    end

    subgraph UseCases["                                                                    Варианты использования                                                                    "]
        direction LR

        subgraph Auth["Авторизация"]
            UC1["Вход в систему"]
        end

        subgraph Content["Управление контентом"]
            direction TB
            UC2["Просмотр<br/>правообладателей"]
            UC3["Создание<br/>правообладателя"]
            UC4["Редактирование<br/>правообладателя"]
            UC5["Удаление<br/>правообладателя"]
            UC6["Просмотр<br/>фильмов"]
            UC7["Создание<br/>фильма"]
            UC8["Редактирование<br/>базовой инфо"]
            UC9["Редактирование<br/>прав на показ"]
            UC10["Удаление<br/>фильма"]
        end

        subgraph Contacts["Контакты"]
            direction TB
            UC11["Просмотр<br/>контактов"]
            UC12["Управление<br/>контактами"]
        end

        subgraph Schedule["Телепрограмма"]
            direction TB
            UC13["Просмотр<br/>программы"]
            UC14["Управление<br/>программой"]
        end

        subgraph AdminPanel["Администрирование"]
            direction TB
            UC15["Управление<br/>пользователями"]
            UC16["Управление<br/>ролями"]
        end
    end

    %% Администратор - полный доступ
    Admin --> UC1
    Admin --> UC2 & UC3 & UC4 & UC5
    Admin --> UC6 & UC7 & UC8 & UC9 & UC10
    Admin --> UC11 & UC12
    Admin --> UC13 & UC14
    Admin --> UC15 & UC16

    %% Инженер видеомонтажа
    VideoEditor --> UC1
    VideoEditor --> UC2 & UC3 & UC4
    VideoEditor --> UC6 & UC7 & UC8
    VideoEditor --> UC13

    %% Специалист
    Specialist --> UC1
    Specialist --> UC2 & UC6 & UC9
    Specialist --> UC11 & UC12
    Specialist --> UC13

    %% Руководитель - только просмотр
    Manager --> UC1
    Manager --> UC2 & UC6
    Manager --> UC11
    Manager --> UC13
```

## 5. Диаграмма последовательности: Авторизация пользователя

```mermaid
sequenceDiagram
    participant U as Пользователь
    participant LF as LoginForm
    participant DB as AppDbContext
    participant SM as SessionManager
    participant MF as MainForm

    U->>LF: Ввод логина и пароля
    LF->>DB: Поиск пользователя по логину
    DB-->>LF: User с Role (Include)

    alt Пользователь найден и пароль верный
        LF->>SM: SetCurrentUser(user)
        SM-->>LF: OK
        LF->>MF: new MainForm()
        MF->>SM: Проверка прав доступа
        SM-->>MF: Права текущей роли
        MF-->>U: Отображение интерфейса по правам
    else Неверные данные
        LF-->>U: Сообщение об ошибке
    end
```

## 6. Диаграмма последовательности: Добавление фильма в телепрограмму

```mermaid
sequenceDiagram
    participant U as Пользователь
    participant TSF as TvScheduleForm
    participant DB as AppDbContext
    participant F as Film

    U->>TSF: Добавить запись в программу
    TSF->>DB: Получить список фильмов
    DB-->>TSF: List<Film>
    TSF-->>U: Диалог выбора фильма
    U->>TSF: Выбор фильма и времени

    TSF->>F: Проверка HasValidRights

    alt Права действительны (HasValidRights = true)
        TSF->>DB: Создать TvScheduleEntry
        DB-->>TSF: OK
        TSF-->>U: Запись добавлена
    else Права недействительны
        TSF-->>U: Ошибка: права истекли или ShowCount = 0
    end
```

## 7. Диаграмма последовательности: Обработка эфира

```mermaid
sequenceDiagram
    participant Timer as Timer (60 сек)
    participant TSF as TvScheduleForm
    participant DB as AppDbContext
    participant F as Film
    participant TSE as TvScheduleEntry

    Timer->>TSF: Tick (каждые 60 сек)
    TSF->>DB: Получить записи где ScheduledDateTime < Now и IsAired = false
    DB-->>TSF: List<TvScheduleEntry>

    loop Для каждой записи
        TSF->>TSE: IsAired = true
        TSF->>F: ShowCount = ShowCount - 1
        TSF->>DB: SaveChanges()
    end

    TSF->>TSF: LoadData() - обновить таблицу
```

## 8. Диаграмма состояний фильма

```mermaid
stateDiagram-v2
    [*] --> Создан: Добавление фильма

    Создан --> БезПрав: Права не заполнены
    Создан --> ПраваДействительны: Права заполнены

    БезПрав --> ПраваДействительны: Заполнение прав

    ПраваДействительны --> МалоПоказов: ShowCount <= 5
    ПраваДействительны --> ПраваИстекли: RightsExpirationDate < Today
    ПраваДействительны --> ПраваДействительны: Показ в эфире (ShowCount--)

    МалоПоказов --> ПоказыИсчерпаны: ShowCount = 0
    МалоПоказов --> МалоПоказов: Показ в эфире (ShowCount--)
    МалоПоказов --> ПраваИстекли: RightsExpirationDate < Today

    ПраваИстекли --> [*]: Удаление или продление
    ПоказыИсчерпаны --> [*]: Удаление или продление

    note right of ПраваДействительны: Зелёный цвет в UI
    note right of МалоПоказов: Жёлтый цвет (LightYellow)
    note right of ПраваИстекли: Красный цвет (LightCoral)
    note right of ПоказыИсчерпаны: Красный цвет (LightCoral)
```

## 9. Диаграмма развёртывания (Deployment Diagram)

```mermaid
flowchart TB
    subgraph ClientMachine["Клиентская машина (Windows)"]
        WinFormsApp["Windows Forms Application<br/>.NET 9.0"]
    end

    subgraph ServerDocker["Docker Environment"]
        PostgreSQLContainer["PostgreSQL 16<br/>Container"]
        PostgreSQLData[("equipment_db<br/>Database")]
    end

    WinFormsApp -->|"TCP/IP:5432<br/>Npgsql"| PostgreSQLContainer
    PostgreSQLContainer --> PostgreSQLData

    subgraph Configuration["Конфигурация подключения"]
        ConnString["Host=localhost<br/>Port=5432<br/>Database=equipment_db<br/>Username=equipment_user<br/>Password=equipment_pass"]
    end

    WinFormsApp -.-> ConnString
    ConnString -.-> PostgreSQLContainer
```

## 10. Диаграмма потоков данных (Data Flow Diagram)

```mermaid
flowchart LR
    subgraph External["Внешние сущности"]
        User((Пользователь))
    end

    subgraph Processes["Процессы"]
        P1[["1.0<br/>Авторизация"]]
        P2[["2.0<br/>Управление<br/>контентом"]]
        P3[["3.0<br/>Управление<br/>контактами"]]
        P4[["4.0<br/>Управление<br/>программой"]]
        P5[["5.0<br/>Администрирование"]]
    end

    subgraph DataStores["Хранилища данных"]
        D1[(Users)]
        D2[(Roles)]
        D3[(RightsOwners)]
        D4[(Films)]
        D5[(Contacts)]
        D6[(TvScheduleEntries)]
    end

    User -->|"Логин/Пароль"| P1
    P1 -->|"Проверка"| D1
    D1 -->|"Данные пользователя"| P1
    P1 -->|"Загрузка роли"| D2
    D2 -->|"Права доступа"| P1
    P1 -->|"Результат авторизации"| User

    User -->|"CRUD операции"| P2
    P2 <-->|"Правообладатели"| D3
    P2 <-->|"Фильмы"| D4
    P2 -->|"Данные контента"| User

    User -->|"CRUD операции"| P3
    P3 <-->|"Контакты"| D5
    P3 -->|"Данные контактов"| User

    User -->|"Управление расписанием"| P4
    P4 <-->|"Записи программы"| D6
    P4 -->|"Чтение фильмов"| D4
    P4 -->|"Обновление ShowCount"| D4
    P4 -->|"Телепрограмма"| User

    User -->|"Администрирование"| P5
    P5 <-->|"Пользователи"| D1
    P5 <-->|"Роли"| D2
    P5 -->|"Данные админки"| User
```

## 11. Матрица прав доступа по ролям

```mermaid
flowchart TB
    subgraph Legend["Условные обозначения"]
        Yes["Да - разрешено"]
        No["Нет - запрещено"]
    end

    subgraph RolesMatrix["Матрица прав доступа"]
        direction TB

        subgraph Admin["Администратор"]
            A1["Пользователи: Да"]
            A2["Роли: Да"]
            A3["Правообладатели: Полный доступ"]
            A4["Фильмы: Полный доступ"]
            A5["Контакты: Полный доступ"]
            A6["Программа: Полный доступ"]
        end

        subgraph Editor["Инженер видеомонтажа"]
            E1["Пользователи: Нет"]
            E2["Роли: Нет"]
            E3["Правообладатели: Создание, Редактирование"]
            E4["Фильмы: Создание, Базовая инфо"]
            E5["Контакты: Только просмотр"]
            E6["Программа: Только просмотр"]
        end

        subgraph Spec["Специалист"]
            S1["Пользователи: Нет"]
            S2["Роли: Нет"]
            S3["Правообладатели: Только просмотр"]
            S4["Фильмы: Права на показ"]
            S5["Контакты: Полный доступ"]
            S6["Программа: Только просмотр"]
        end

        subgraph Mgr["Руководитель"]
            M1["Пользователи: Нет"]
            M2["Роли: Нет"]
            M3["Правообладатели: Только просмотр"]
            M4["Фильмы: Только просмотр"]
            M5["Контакты: Только просмотр"]
            M6["Программа: Только просмотр"]
        end
    end
```
