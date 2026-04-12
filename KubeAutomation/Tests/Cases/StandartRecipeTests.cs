using System;
using System.Collections.Generic;
using System.Globalization;
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.Tests.Cases
{
    public class StandartRecipeTests : TestBase
    {
        private static ItemComponent MakeItem(string itemId, int amount = 1)
        {
            return new ItemComponent(itemId, amount);
        }

        public override List<TestResult> RunAll()
        {
            var results = new List<TestResult>();

            TestLogger.Write("=================================================");
            TestLogger.Write("StandartRecipeTests: Запуск тестов генератора");
            TestLogger.Write("=================================================");
            TestLogger.Write("");

            TestLogger.Write("[GROUP] Валидация: Null и пустые параметры");
            results.Add(TestValidate_ExpectsError("smelting", null, MakeItem("in"), null, "outputItem не может быть null"));
            results.Add(TestValidate_ExpectsError("smelting", MakeItem("out"), null, null, "inputItem не может быть null"));
            results.Add(TestValidate_ExpectsError("", MakeItem("out"), MakeItem("in"), null, "block не может быть пустым"));
            results.Add(TestValidate_ExpectsError("   ", MakeItem("out"), MakeItem("in"), null, "block не может быть пустым"));
            results.Add(TestValidate_ExpectsError("invalid", MakeItem("out"), MakeItem("in"), null, "Blocks не содержит"));

            TestLogger.Write("[GROUP] Валидация: Граничные значения Output Amount");
            results.Add(TestValidate_OutputAmount(0, false, "Ниже минимума"));
            results.Add(TestValidate_OutputAmount(1, true, "Минимум (граница)"));
            results.Add(TestValidate_OutputAmount(32, true, "Середина диапазона"));
            results.Add(TestValidate_OutputAmount(64, true, "Максимум (граница)"));
            results.Add(TestValidate_OutputAmount(65, false, "Выше максимума"));

            TestLogger.Write("[GROUP] Валидация: Input Amount должен быть 1");
            results.Add(TestValidate_InputAmount(0, false, "Ноль"));
            results.Add(TestValidate_InputAmount(1, true, "Единственное допустимое"));
            results.Add(TestValidate_InputAmount(2, false, "Больше единицы"));
            results.Add(TestValidate_InputAmount(64, false, "Максимум"));

            TestLogger.Write("[GROUP] Валидация: Experience границы [0, 100]");
            results.Add(TestValidate_Experience(-1f, false, "Отрицательный"));
            results.Add(TestValidate_Experience(0f, true, "Ноль (граница)"));
            results.Add(TestValidate_Experience(0.5f, true, "Дробное"));
            results.Add(TestValidate_Experience(50f, true, "Середина"));
            results.Add(TestValidate_Experience(100f, true, "Максимум (граница)"));
            results.Add(TestValidate_Experience(101f, false, "Выше максимума"));

            TestLogger.Write("[GROUP] Валидация: Experience только для smoking");
            results.Add(TestValidate_ExperienceForBlock("smoking", 50f, true, "smoking + exp = OK"));
            results.Add(TestValidate_ExperienceForBlock("smelting", 50f, false, "smelting + exp = Error"));
            results.Add(TestValidate_ExperienceForBlock("blasting", 50f, false, "blasting + exp = Error"));
            results.Add(TestValidate_ExperienceForBlock("stonecutting", 50f, false, "stonecutting + exp = Error"));

            TestLogger.Write("[GROUP] Валидация: Уникальность ID предметов");
            results.Add(TestValidate_SameItemId("minecraft:iron", "minecraft:iron", false, "Одинаковые ID"));
            results.Add(TestValidate_SameItemId("minecraft:iron", "minecraft:ore", true, "Разные ID"));

            TestLogger.Write("[GROUP] Генерация: Все типы блоков");
            results.Add(TestGenerate_AllBlockTypes("smelting", "minecraft:iron", "minecraft:ore"));
            results.Add(TestGenerate_AllBlockTypes("blasting", "minecraft:gold", "minecraft:ore"));
            results.Add(TestGenerate_AllBlockTypes("smoking", "minecraft:cooked_beef", "minecraft:beef"));
            results.Add(TestGenerate_AllBlockTypes("stonecutting", "minecraft:stone_slab", "minecraft:stone"));

            TestLogger.Write("[GROUP] Генерация: Experience для smoking");
            results.Add(TestGenerate_SmokingWithXp(0f, ".xp(0)"));
            results.Add(TestGenerate_SmokingWithXp(0.5f, ".xp(0.5)"));
            results.Add(TestGenerate_SmokingWithXp(1f, ".xp(1)"));
            results.Add(TestGenerate_SmokingWithXp(100f, ".xp(100)"));

            TestLogger.Write("[GROUP] Форматирование: Культура и отступы");
            results.Add(TestGenerate_UsesInvariantCulture(0.75f, ".xp(0.75)"));

            TestLogger.Write("");
            TestLogger.Write("=================================================");
            TestLogger.Write($"StandartRecipeTests: {results.Count} тестов завершено");
            TestLogger.Write($"   Пройдено: {results.Count(r => r.Passed)}");
            TestLogger.Write($"   Провалено: {results.Count(r => !r.Passed)}");
            TestLogger.Write("=================================================");
            TestLogger.Write("");

            return results;
        }

        private TestResult TestValidate_ExpectsError(string block, ItemComponent output, ItemComponent input, float? exp, string expectedError)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Validate: {expectedError}";
            TestLogger.Write($"  [TEST] {testName}");

            try
            {
                var errors = MinecraftStandartRecipesGenerator.Validate(block, output, input, exp);
                var found = errors.Exists(e => e.Contains(expectedError));

                if (found)
                {
                    TestLogger.Write($"    PASS: Ошибка обнаружена");
                    sw.Stop();
                    return TestResult.Pass(testName, sw.Elapsed);
                }
                else
                {
                    TestLogger.Write($"    FAIL: Ошибка не обнаружена. Errors: {string.Join("; ", errors)}");
                    sw.Stop();
                    return TestResult.Fail(testName, $"Ожидалась ошибка '{expectedError}'", sw.Elapsed);
                }
            }
            catch (Exception ex)
            {
                TestLogger.Write($"    FAIL: Исключение: {ex.Message}");
                sw.Stop();
                return TestResult.Fail(testName, ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestValidate_OutputAmount(int amount, bool shouldBeValid, string description)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Validate: OutputAmount={amount} ({description})";
            TestLogger.Write($"  [TEST] {testName}");

            var output = MakeItem("minecraft:iron", amount);
            var input = MakeItem("minecraft:ore", 1);
            var errors = MinecraftStandartRecipesGenerator.Validate("smelting", output, input, null);
            var isValid = errors.Count == 0;

            if (isValid == shouldBeValid)
            {
                TestLogger.Write($"    PASS: Результат совпадает с ожидаемым");
                sw.Stop();
                return TestResult.Pass(testName, sw.Elapsed);
            }
            else
            {
                TestLogger.Write($"    FAIL: Ожидалось {(shouldBeValid ? "OK" : "Error")}, получено {(isValid ? "OK" : "Error")}");
                TestLogger.Write($"    Errors: {string.Join("; ", errors)}");
                sw.Stop();
                return TestResult.Fail(testName, $"Несоответствие результата", sw.Elapsed);
            }
        }

        private TestResult TestValidate_InputAmount(int amount, bool shouldBeValid, string description)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Validate: InputAmount={amount} ({description})";
            TestLogger.Write($"  [TEST] {testName}");

            var output = MakeItem("minecraft:iron", 1);
            var input = MakeItem("minecraft:ore", amount);
            var errors = MinecraftStandartRecipesGenerator.Validate("smelting", output, input, null);
            var isValid = errors.Count == 0;

            if (isValid == shouldBeValid)
            {
                TestLogger.Write($"    PASS: Результат совпадает с ожидаемым");
                sw.Stop();
                return TestResult.Pass(testName, sw.Elapsed);
            }
            else
            {
                TestLogger.Write($"    FAIL: Ожидалось {(shouldBeValid ? "OK" : "Error")}, получено {(isValid ? "OK" : "Error")}");
                TestLogger.Write($"    Errors: {string.Join("; ", errors)}");
                sw.Stop();
                return TestResult.Fail(testName, $"Несоответствие результата", sw.Elapsed);
            }
        }

        private TestResult TestValidate_Experience(float exp, bool shouldBeValid, string description)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Validate: Experience={exp} ({description})";
            TestLogger.Write($"  [TEST] {testName}");

            var output = MakeItem("minecraft:cooked_beef", 1);
            var input = MakeItem("minecraft:beef", 1);
            var errors = MinecraftStandartRecipesGenerator.Validate("smoking", output, input, exp);
            var isValid = errors.Count == 0;

            if (isValid == shouldBeValid)
            {
                TestLogger.Write($"    PASS: Результат совпадает с ожидаемым");
                sw.Stop();
                return TestResult.Pass(testName, sw.Elapsed);
            }
            else
            {
                TestLogger.Write($"    FAIL: Ожидалось {(shouldBeValid ? "OK" : "Error")}, получено {(isValid ? "OK" : "Error")}");
                TestLogger.Write($"    Errors: {string.Join("; ", errors)}");
                sw.Stop();
                return TestResult.Fail(testName, $"Несоответствие результата", sw.Elapsed);
            }
        }

        private TestResult TestValidate_ExperienceForBlock(string block, float exp, bool shouldBeValid, string description)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Validate: {description}";
            TestLogger.Write($"  [TEST] {testName}");

            var output = MakeItem("minecraft:iron", 1);
            var input = MakeItem("minecraft:ore", 1);
            var errors = MinecraftStandartRecipesGenerator.Validate(block, output, input, exp);
            var isValid = errors.Count == 0;

            if (isValid == shouldBeValid)
            {
                TestLogger.Write($"    PASS: Результат совпадает с ожидаемым");
                sw.Stop();
                return TestResult.Pass(testName, sw.Elapsed);
            }
            else
            {
                TestLogger.Write($"    FAIL: Ожидалось {(shouldBeValid ? "OK" : "Error")}, получено {(isValid ? "OK" : "Error")}");
                TestLogger.Write($"    Errors: {string.Join("; ", errors)}");
                sw.Stop();
                return TestResult.Fail(testName, $"Несоответствие результата", sw.Elapsed);
            }
        }

        private TestResult TestValidate_SameItemId(string outId, string inId, bool shouldBeValid, string description)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Validate: {description}";
            TestLogger.Write($"  [TEST] {testName}");

            var output = MakeItem(outId, 1);
            var input = MakeItem(inId, 1);
            var errors = MinecraftStandartRecipesGenerator.Validate("smelting", output, input, null);
            var isValid = errors.Count == 0;

            if (isValid == shouldBeValid)
            {
                TestLogger.Write($"    PASS: Результат совпадает с ожидаемым");
                sw.Stop();
                return TestResult.Pass(testName, sw.Elapsed);
            }
            else
            {
                TestLogger.Write($"    FAIL: Ожидалось {(shouldBeValid ? "OK" : "Error")}, получено {(isValid ? "OK" : "Error")}");
                TestLogger.Write($"    Errors: {string.Join("; ", errors)}");
                sw.Stop();
                return TestResult.Fail(testName, $"Несоответствие результата", sw.Elapsed);
            }
        }

        private TestResult TestGenerate_AllBlockTypes(string block, string outId, string inId)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Generate: {block}";
            TestLogger.Write($"  [TEST] {testName}");

            try
            {
                var output = MakeItem(outId, 1);
                var input = MakeItem(inId, 1);
                var result = MinecraftStandartRecipesGenerator.Generate(block, output, input, null);
                var expected = $"    event.{block}('{outId}', '{inId}')";

                if (result.Trim() == expected.Trim())
                {
                    TestLogger.Write($"    PASS: {result}");
                    sw.Stop();
                    return TestResult.Pass(testName, sw.Elapsed);
                }
                else
                {
                    TestLogger.Write($"    FAIL: Ожидалось \"{expected}\", получено \"{result}\"");
                    sw.Stop();
                    return TestResult.Fail(testName, $"Несоответствие кода", sw.Elapsed);
                }
            }
            catch (Exception ex)
            {
                TestLogger.Write($"    FAIL: Исключение: {ex.Message}");
                sw.Stop();
                return TestResult.Fail(testName, ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestGenerate_SmokingWithXp(float exp, string expectedXp)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Generate: Smoking exp={exp}";
            TestLogger.Write($"  [TEST] {testName}");

            try
            {
                var output = MakeItem("minecraft:cooked_beef", 1);
                var input = MakeItem("minecraft:beef", 1);
                var result = MinecraftStandartRecipesGenerator.Generate("smoking", output, input, exp);

                if (result.Contains(expectedXp))
                {
                    TestLogger.Write($"    PASS: Содержит {expectedXp}");
                    sw.Stop();
                    return TestResult.Pass(testName, sw.Elapsed);
                }
                else
                {
                    TestLogger.Write($"    FAIL: Не содержит {expectedXp}. Результат: {result}");
                    sw.Stop();
                    return TestResult.Fail(testName, $"Отсутствует {expectedXp}", sw.Elapsed);
                }
            }
            catch (Exception ex)
            {
                TestLogger.Write($"    FAIL: Исключение: {ex.Message}");
                sw.Stop();
                return TestResult.Fail(testName, ex.Message, sw.Elapsed);
            }
        }


        private TestResult TestGenerate_UsesInvariantCulture(float exp, string expectedFormat)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string testName = $"Generate: InvariantCulture exp={exp}";
            TestLogger.Write($"  [TEST] {testName}");

            try
            {
                var output = MakeItem("minecraft:cooked_beef", 1);
                var input = MakeItem("minecraft:beef", 1);
                var result = MinecraftStandartRecipesGenerator.Generate("smoking", output, input, exp);

                if (result.Contains(expectedFormat))
                {
                    TestLogger.Write($"    PASS: Используется точка (InvariantCulture)");
                    sw.Stop();
                    return TestResult.Pass(testName, sw.Elapsed);
                }
                else if (result.Contains(expectedFormat.Replace('.', ',')))
                {
                    TestLogger.Write($"    FAIL: Используется запятая (русская культура)! Результат: {result}");
                    sw.Stop();
                    return TestResult.Fail(testName, $"Неверный формат числа", sw.Elapsed);
                }
                else
                {
                    TestLogger.Write($"    FAIL: Неожиданный формат. Результат: {result}");
                    sw.Stop();
                    return TestResult.Fail(testName, $"Неожиданный формат", sw.Elapsed);
                }
            }
            catch (Exception ex)
            {
                TestLogger.Write($"    FAIL: Исключение: {ex.Message}");
                sw.Stop();
                return TestResult.Fail(testName, ex.Message, sw.Elapsed);
            }
        }

    }
}