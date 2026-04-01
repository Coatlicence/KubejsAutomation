// Tests/TestBase.cs
using KubeAutomation.GenerationStrategies.Templates;

namespace KubeAutomation.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly string TempFolder;
        private readonly string _originalTemplatesFolder;

        protected TestBase()
        {
            // Сохраняем оригинальный путь
            _originalTemplatesFolder = TemplateManager.TemplatesFolder;

            // Создаём уникальную временную папку
            TempFolder = Path.Combine(Path.GetTempPath(), $"KubeTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(TempFolder);

            // Переключаем TemplateManager на тестовую папку
            TemplateManager.TemplatesFolder = TempFolder;
        }

        public abstract List<TestResult> RunAll();

        public void Dispose()
        {
            // Восстанавливаем оригинальный путь
            TemplateManager.TemplatesFolder = _originalTemplatesFolder;

            // Удаляем временные файлы
            if (Directory.Exists(TempFolder))
            {
                try { Directory.Delete(TempFolder, true); }
                catch { /* Игнорируем ошибки очистки */ }
            }
        }
    }
}