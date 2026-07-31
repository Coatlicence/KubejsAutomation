# Скрипт автоматической настройки проекта KubeAutomation
# Запускать в PowerShell с правами администратора (для установки компонентов)

Write-Host "=== Настройка проекта KubeAutomation ===" -ForegroundColor Cyan

# Проверка .NET SDK
Write-Host "`n[1/4] Проверка .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  Установлен .NET SDK версии: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  .NET SDK не найден!" -ForegroundColor Red
    Write-Host "  Скачайте и установите .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Проверка версии .NET
if (-not ($dotnetVersion -match "^8\.")) {
    Write-Host "  Внимание: Рекомендуется .NET 8.x, у вас версия $dotnetVersion" -ForegroundColor Yellow
}

# Восстановление NuGet пакетов
Write-Host "`n[2/4] Восстановление NuGet пакетов (включая WinUI 3)..." -ForegroundColor Yellow
dotnet restore --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Ошибка восстановления пакетов!" -ForegroundColor Red
    exit 1
}
Write-Host "  Пакеты успешно восстановлены" -ForegroundColor Green

# Сборка проекта
Write-Host "`n[3/4] Сборка проекта..." -ForegroundColor Yellow
dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Ошибка сборки! Проверьте наличие Windows SDK." -ForegroundColor Red
    Write-Host "  Установите компонент 'Разработка классических приложений Windows' в Visual Studio Installer" -ForegroundColor Yellow
    exit 1
}
Write-Host "  Сборка успешна" -ForegroundColor Green

# Проверка структуры
Write-Host "`n[4/4] Проверка структуры проекта..." -ForegroundColor Yellow
if (Test-Path ".nuget\packages") {
    Write-Host "  Локальная папка пакетов существует" -ForegroundColor Green
} else {
    Write-Host "  Создание локальной папки пакетов..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path ".nuget\packages" | Out-Null
}

Write-Host "`n=== Настройка завершена! ===" -ForegroundColor Green
Write-Host "`nТеперь вы можете:" -ForegroundColor Cyan
Write-Host "  1. Открыть KubeAutomation.sln в Visual Studio" -ForegroundColor White
Write-Host "  2. Или запустить: dotnet run --project KubeAutomation" -ForegroundColor White
Write-Host "`nЕсли возникли ошибки, см. README.md" -ForegroundColor Yellow
