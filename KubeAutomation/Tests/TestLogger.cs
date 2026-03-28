// Tests/TestLogger.cs
namespace KubeAutomation.Tests
{
    public static class TestLogger
    {
        private static readonly string LogPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "logs", $"test_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        // Вызвать один раз в начале тестов
        public static void Init()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"\n=== ЗАПУСК: {DateTime.Now} ===\n");
        }

        // Писать всё, что нужно (данные, JSON, этапы, ошибки)
        public static void Write(string message)
        {
            File.AppendAllText(LogPath, message + "\n");
        }

        // Вызвать в конце — вернёт строку для консоли
        public static string Finalize(int passed, int failed)
        {
            File.AppendAllText(LogPath, $"=== ИТОГ: {passed} пройдено, {failed} провалено ===\n");
            return $"YES {passed} | NO {failed} | Лог: {Path.GetFileName(LogPath)}";
        }
    }
}