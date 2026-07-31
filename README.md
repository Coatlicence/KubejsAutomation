# KubeAutomation - WinUI 3 Приложение

## Требования для разработки

### Обязательные компоненты (устанавливаются автоматически при первом запуске Visual Studio или через скрипт)

1. **.NET 8.0 SDK** - [Скачать](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Windows App SDK** - Устанавливается через NuGet автоматически
3. **Windows SDK 10.0.19041.0** - Часть Visual Studio workload "Разработка классических приложений Windows"

### Установка на новом устройстве

#### Вариант 1: Автоматическая установка (рекомендуется)

1. Откройте терминал в папке проекта
2. Выполните команду:
   ```bash
   dotnet restore
   dotnet build
   ```

Пакеты NuGet (включая WinUI 3) будут загружены автоматически.

#### Вариант 2: Через Visual Studio

1. Откройте `KubeAutomation.sln` в Visual Studio 2022
2. Visual Studio автоматически восстановит все пакеты NuGet
3. Соберите проект (Ctrl+Shift+B)

#### Вариант 3: PowerShell скрипт установки

Запустите `setup.ps1` для автоматической установки всех зависимостей:

```powershell
.\setup.ps1
```

## Структура проекта

- `KubeAutomation/` - Основной проект WinUI 3 приложения
- `KubeAutomation.sln` - Решение Visual Studio
- `.nuget.config` - Конфигурация NuGet (пакеты сохраняются локально в проекте)
- `Directory.Build.props` - Общие настройки сборки

## Что такое WinUI 3?

WinUI 3 - это современная библиотека UI от Microsoft для создания нативных Windows приложений. В этом проекте используется **Windows App SDK**, который включает WinUI 3 и распространяется через NuGet.

## CI/CD Настройка

Проект настроен для автоматического восстановления зависимостей:

1. **NuGet пакеты в csproj**: Все зависимости указаны в файле проекта
2. **Локальная папка пакетов**: `.nuget/packages` для кэширования
3. **Self-contained режим**: Приложение может быть собрано со встроенными зависимостями

### Для CI/CD пайплайнов (GitHub Actions, Azure DevOps):

```yaml
steps:
  - uses: actions/checkout@v3
  - name: Setup .NET
    uses: actions/setup-dotnet@v3
    with:
      dotnet-version: '8.0.x'
  - name: Restore dependencies
    run: dotnet restore
  - name: Build
    run: dotnet build --configuration Release --no-restore
```

## Troubleshooting

### Ошибка "WinUI не существует в контексте"

1. Выполните `dotnet restore` в корне проекта
2. Перезагрузите Visual Studio
3. Проверьте, что установлен .NET 8.0 SDK: `dotnet --version`

### Ошибка сборки Windows SDK

Убедитесь, что в Visual Studio установлен компонент:
- "Разработка классических приложений Windows" (Windows Desktop Development)

### Проблемы с путями к проектам

Проект ссылается на внешний `LogExtractorLibrary`. Убедитесь, что он доступен по пути `../../LogExtractorLibrary/LogExtractorLibrary/LogExtractorLibrary.csproj` или удалите эту ссылку из csproj если она не нужна.
