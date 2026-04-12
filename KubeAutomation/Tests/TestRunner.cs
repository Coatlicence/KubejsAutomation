// Tests/TestRunner.cs
using KubeAutomation.Tests.Cases;

namespace KubeAutomation.Tests
{
    public static class TestRunner
    {
        /// <summary>
        /// Запускает все тесты
        /// </summary>
        public static void RunAll()
        {
            TestLogger.Init();

            var results = new List<TestResult>();

            // Запуск тестов
            results.AddRange(new RoundtripTests().RunAll());
            results.AddRange(new StandartRecipeTests().RunAll());
            results.AddRange(new RecipeRemovalTests().RunAll());

            // Подсчёт
            int passed = results.Count(r => r.Passed);
            int failed = results.Count - passed;

            // Вывод в консоль — ТОЛЬКО итог
            Console.WriteLine(TestLogger.Finalize(passed, failed));
            
        }

        /// <summary>
        /// Запускает выбранные тесты
        /// </summary>
        /// <param name="testCases"></param>
        public static void Run(List<TestBase> testCases)
        {
            if (testCases.Count <= 0)
                return;

            var results = new List<TestResult>();

            foreach (var test in testCases) 
            {
                results.AddRange(new RoundtripTests().RunAll());
            }

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