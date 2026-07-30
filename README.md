<div align="center">

# StudyMate

**Кроссплатформенное приложение для учёбы: конспекты, рисование, напоминания и научный калькулятор — в одном месте.**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/.NET_MAUI-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![SkiaSharp](https://img.shields.io/badge/SkiaSharp-2.88-FF6B6B?style=flat-square)](https://github.com/mono/SkiaSharp)
[![Platforms](https://img.shields.io/badge/platforms-Android_|_iOS_|_Windows_|_macOS-informational?style=flat-square)](#поддерживаемые-платформы)

</div>

---

## О проекте

**StudyMate** — мобильное и десктопное приложение-помощник для студентов, написанное на **.NET MAUI**.
Вместо того чтобы держать отдельно блокнот, графический редактор, будильник и калькулятор, StudyMate
объединяет всё это в единое рабочее пространство: заметка может одновременно содержать
форматированный текст и рисунок от руки, к ней можно поставить напоминание, а рядом всегда
доступен научный калькулятор.

Проект разработан в рамках учебного курса НИУ ВШЭ и следует архитектуре **MVVM**
с внедрением зависимостей через встроенный DI-контейнер MAUI.

---

## Возможности

### Тетради и заметки
- Создание, редактирование, копирование и удаление заметок
- **Комбинированный редактор**: текст и графика в одной заметке
- Автоматическое сохранение в `notes.json` + резервные копии (`notes_backup.json`)
- Предпросмотр содержимого, относительные даты («5 мин назад»), статусы «Изменено / Сохранено»
- **Экспорт** заметки в `PNG`, `TXT` и `JSON`, **импорт** из `JSON`

### Графический редактор
Реализован на **SkiaSharp** с полноценным стеком действий:

| Инструменты | Возможности |
|---|---|
| Перо, Ластик | Undo / Redo без ограничений |
| Линия, Прямоугольник, Квадрат | Настройка толщины и цвета обводки |
| Эллипс, Круг, Треугольник | Режим заливки фигур |
| | Масштабирование холста (zoom) |
| | Сохранение холста в PNG |

### Текстовый редактор
Собственный контрол `RichTextEditor` с форматированием:
- **Жирный**, *курсив*, выбор шрифта и размера
- Цвет текста и фона, выравнивание (лево / центр / право / по ширине)
- Междустрочный интервал, красная строка, отступы
- Создание, открытие и сохранение документов

### Напоминания
- Планирование напоминаний с точным временем срабатывания
- **Нативные уведомления** на каждой платформе (Android / iOS / Windows / fallback)
- Запрос разрешений, отмена и редактирование напоминаний
- Автоматический расчёт статуса: «Через 30 мин» → «Активно» → «Выполнено»
- Хранение в `reminders.json`

### Научный калькулятор
- Арифметика с приоритетом операций и скобками
- Тригонометрия: `sin`, `cos`, `tan`, `cot`
- Степени и корни: `x²`, `x³`, `xʸ`, `√`, `∛`
- Логарифмы `ln`, `log`, факториал `x!`, проценты, константы `π` и `e`
- История вычислений (последние 50 записей), копирование выражения и результата
- Валидация выражений и понятные сообщения об ошибках
- Режим **матричного калькулятора** на базе `MathNet.Numerics` *(в разработке)*

---

## Архитектура

Приложение построено по паттерну **MVVM** с чистым разделением слоёв.

```
StudyMateProject/
├── Models/                   # Доменные модели
│   ├── Notes/                #   Note (INotifyPropertyChanged)
│   ├── Reminders/            #   Reminder + вычисляемые статусы
│   ├── Calculator/           #   CalculatorModel + история
│   ├── Drawing/              #   DrawingTool (enum инструментов)
│   └── TextEditor/           #   TextDocument, TextFormatting, TextAlignment
│
├── Services/                 # Бизнес-логика (интерфейс + реализация)
│   ├── NotesService/         #   CRUD, экспорт/импорт, бэкапы
│   ├── DrawingServices/      #   Отрисовка Skia, undo/redo, touch-обработка
│   ├── TextEditorServices/   #   Форматирование и работа с документами
│   ├── CalculatorServices/   #   Парсинг и вычисление выражений
│   └── NotificationServices/ #   Уведомления + локальное хранилище
│       ├── AndroidNotificationService.cs
│       ├── IOSNotificationService.cs
│       ├── WindowsNotificationService.cs
│       └── DefaultNotificationService.cs
│
├── ViewModels/               # Связующий слой (Commands, биндинги)
│   ├── CalculatorViewModel.cs
│   ├── CombinedEditorViewModel.cs
│   ├── DrawingPageViewModel.cs
│   └── TextEditorViewModel.cs
│
├── Views/                    # XAML-страницы
│   ├── NotesListPage         #   Список тетрадей
│   ├── CombinedEditorPage    #   Текст + графика
│   ├── DrawingPage           #   Графический редактор
│   ├── CalculatorPage        #   Калькулятор
│   └── ReminderPage / AddReminderPage / EditReminderPage
│
├── Controls/                 # Кастомные контролы (RichTextEditor)
├── Converters/               # IValueConverter для XAML-биндингов
├── Platforms/                # Платформенный код (Android, iOS, Windows, macOS, Tizen)
└── Resources/                # Стили, шрифты, изображения, иконки, splash
```

**Ключевые принципы:**
- Каждый сервис скрыт за интерфейсом (`INoteService`, `IDrawingService`, …) и регистрируется в DI-контейнере в [MauiProgram.cs](MauiProgram.cs)
- Платформенные реализации уведомлений подключаются через условную компиляцию — код ViewModel остаётся общим
- Модели реализуют `INotifyPropertyChanged`, UI обновляется реактивно
- Навигация — через **Shell** с кастомным Flyout-меню ([AppShell.xaml](AppShell.xaml))

---

## Технологии

| Технология | Назначение |
|---|---|
| **.NET 8** | Целевая платформа |
| **.NET MAUI** | Кроссплатформенный UI-фреймворк |
| **SkiaSharp 2.88** | 2D-графика и графический редактор |
| **CommunityToolkit.Maui 7.0** | Расширенные UI-компоненты и хелперы |
| **MathNet.Numerics 5.0** | Матричные и численные вычисления |
| **Newtonsoft.Json 13.0** | Сериализация заметок и напоминаний |

---

## Поддерживаемые платформы

| Платформа | Минимальная версия |
|---|---|
| Android | 5.0 (API 21) |
| iOS | 11.0 |
| Windows | 10 (build 17763) |
| macOS (Mac Catalyst) | 13.1 |

---

## Запуск проекта

### Требования
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Рабочая нагрузка MAUI: `dotnet workload install maui`
- Для Windows — Visual Studio 2022 (17.8+) с компонентом *.NET Multi-platform App UI development*
- Для iOS/macOS — Xcode 15+

### Клонирование и сборка

```bash
git clone https://github.com/CHIPUKAN/StudyMateProject.git
cd StudyMateProject

# Восстановление зависимостей
dotnet restore

# Запуск на Windows
dotnet build -t:Run -f net8.0-windows10.0.19041.0

# Запуск на Android (подключённое устройство или эмулятор)
dotnet build -t:Run -f net8.0-android

# Запуск на macOS
dotnet build -t:Run -f net8.0-maccatalyst
```

Либо просто откройте `StudyMateProject.sln` в Visual Studio, выберите целевую платформу и нажмите **F5**.

---

## Хранение данных

Все данные хранятся локально на устройстве, в `FileSystem.AppDataDirectory`:

| Файл | Содержимое |
|---|---|
| `notes.json` | Все заметки пользователя |
| `notes_backup.json` | Резервная копия заметок |
| `reminders.json` | Напоминания |

Экспортированные файлы (`PNG`, `TXT`, `JSON`) сохраняются в системные папки **Изображения** и **Документы**.

---

## Планы развития

- [ ] Завершить матричный калькулятор (определитель, обратная матрица, СЛАУ)
- [ ] Облачная синхронизация заметок между устройствами
- [ ] Поиск и теги для заметок
- [ ] Тёмная тема
- [ ] Распознавание рукописного текста
- [ ] Покрытие юнит-тестами сервисного слоя

---

## Автор

Учебный проект НИУ ВШЭ, 2 курс.
