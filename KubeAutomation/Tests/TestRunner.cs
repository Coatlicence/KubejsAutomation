// Tests/TestRunner.cs
using KubeAutomation.Tests.Cases;

namespace KubeAutomation.Tests
{
    public static class TestRunner
    {
        public static void RunAll()
        {
            TestLogger.Init();

            var results = new List<TestResult>();

            // Запуск тестов
            results.AddRange(new RoundtripTests().RunAll());
            // results.AddRange(new ParserTests().RunAll());

            // Подсчёт
            int passed = results.Count(r => r.Passed);
            int failed = results.Count - passed;

            // Вывод в консоль — ТОЛЬКО итог
            Console.WriteLine(TestLogger.Finalize(passed, failed));
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}