// Tests/Cases/RoundtripTests.cs
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.Templates;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KubeAutomation.Tests.Cases
{
    public class RoundtripTests : TestBase
    {
        public List<TestResult> RunAll()
        {
            var results = new List<TestResult>
            {
                // Тесты выполняются в изолированной папке благодаря TestBase
                TestGetValueDirect(),
                TestSimpleRecipeRoundtrip(),
                TestComplexRecipeRoundtrip(),
                TestMetadataPreservation(),
                TestTemplateModeRequiredFields()
            };

            return results;
        }

        private TestResult TestGetValueDirect()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Прямая проверка GetValue");

                var config = new CustomRecipeConfig();
                config.CreateProperty("pattern", JsonType.Array);
                config.AddToArray("pattern", "AAA");
                config.AddToArray("pattern", "ABA");

                // Проверяем GetValue напрямую
                var val0 = config.GetValue("pattern[0]");
                var val1 = config.GetValue("pattern[1]");

                TestLogger.Write($"[DEBUG] pattern[0] = '{val0}' (type: {val0?.GetType()?.Name})");
                TestLogger.Write($"[DEBUG] pattern[1] = '{val1}' (type: {val1?.GetType()?.Name})");

                if (val0 as string != "AAA" || val1 as string != "ABA")
                    throw new Exception($"GetValue вернул неверные значения: '{val0}', '{val1}'");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("GetValue: Прямая проверка", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("GetValue: Прямая проверка", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestSimpleRecipeRoundtrip()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Roundtrip: Простой рецепт (РЕЖИМ РЕЦЕПТОВ)");

                // 1. Создаём конфиг со всеми значениями
                var original = new CustomRecipeConfig();
                original.CreateProperty("type", JsonType.String);
                original.SetValue("type", "minecraft:crafting_shaped");

                original.CreateProperty("pattern", JsonType.Array);
                original.AddToArray("pattern", "AAA");
                original.AddToArray("pattern", "ABA");

                string originalJson = original.ToJson();
                TestLogger.Write($"[DATA] Original JSON:\n{originalJson}");

                // 2. 🔧 Сохраняем как РЕЦЕПТ (не шаблон!)
                var saved = TemplateManager.SaveRecipe("test_simple_recipe", original, "test");
                TestLogger.Write($"[SAVE] SaveRecipe result: {saved}");
                if (!saved) throw new Exception("SaveRecipe failed");

                // 3. Загружаем обратно (LoadTemplate работает для обоих режимов)
                var result = TemplateManager.LoadTemplate("test_simple_recipe", "test");
                TestLogger.Write($"[LOAD] Success: {result.Success}, Error: {result.ErrorMessage}");
                if (!result.Success) throw new Exception(result.ErrorMessage);

                // 4. Сравниваем
                string loadedJson = result.Config!.ToJson();
                TestLogger.Write($"[DATA] Loaded JSON:\n{loadedJson}");

                if (!JsonSemanticEquals(originalJson, loadedJson))
                    throw new Exception("JSON mismatch after roundtrip");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass | Duration: {stopwatch.ElapsedMilliseconds}ms\n");
                return TestResult.Pass("Roundtrip: Простой рецепт (Recipe Mode)", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | Error: {ex.Message}\n");
                return TestResult.Fail("Roundtrip: Простой рецепт (Recipe Mode)", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestComplexRecipeRoundtrip()
        {
            TestLogger.Write("[TEST] Start: Roundtrip: Сложный рецепт");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // 1. Создаём конфиг со сложной вложенной структурой
                TestLogger.Write("[STEP 1] Создание исходного конфига...");
                var original = new CustomRecipeConfig();
                original.CreateProperty("type", JsonType.String);
                original.SetValue("type", "create:sequenced_assembly");
                original.CreateProperty("loops", JsonType.Int);
                original.SetValue("loops", 3);

                // Массив sequence с 3 шагами
                original.CreateArray("sequence");
                for (int i = 0; i < 3; i++)
                {
                    var step = original.AddObjectToArray("sequence");
                    original.CreateProperty($"{step}.type", JsonType.String);
                    original.SetValue($"{step}.type", "create:pressing");

                    // Вложенный массив ingredients внутри каждого шага
                    original.CreateArray($"{step}.ingredients");
                    var ing = original.AddObjectToArray($"{step}.ingredients");
                    original.CreateProperty($"{ing}.item", JsonType.String);
                    original.SetValue($"{ing}.item", $"mod:intermediate_{i}");
                }

                string originalJson = original.ToJson();
                TestLogger.Write($"[DATA] Original JSON:\n{originalJson}");

                // 2. Сохраняем как шаблон
                TestLogger.Write("[STEP 2] Сохранение шаблона (SaveTemplate)...");
                var saved = TemplateManager.SaveTemplate("test_complex", original, "create");
                TestLogger.Write($"[SAVE] Result: {saved}");
                if (!saved) throw new Exception("Не удалось сохранить шаблон");

                // Отладка: что попало в файл (извлекаем структуру)
                var paths = original.GetPathList();
                TestLogger.Write($"[DEBUG] Extracted {paths.Count} paths for saving:");
                foreach (var p in paths.Take(10)) // Первые 10, чтобы не засорять лог
                {
                    var val = original.GetValue(p);
                    TestLogger.Write($"  [PATH] {p} = '{val}'");
                }
                if (paths.Count > 10) TestLogger.Write($"  ... и ещё {paths.Count - 10} путей");

                // 3. Загружаем обратно
                TestLogger.Write("[STEP 3] Загрузка шаблона (LoadTemplate)...");
                var result = TemplateManager.LoadTemplate("test_complex", "create");
                TestLogger.Write($"[LOAD] Success: {result.Success}, Error: {result.ErrorMessage ?? "none"}");

                if (!result.Success) throw new Exception(result.ErrorMessage);
                if (result.Config == null) throw new Exception("Config is null after load");

                // Отладка: что загрузилось
                if (result.Paths != null)
                {
                    TestLogger.Write($"[DEBUG] Loaded {result.Paths.Count} paths:");
                    foreach (var p in result.Paths.Take(10))
                    {
                        TestLogger.Write($"  [PATH] {p.Path} = '{p.Value}' (Type: {p.FieldType})");
                    }
                }

                // 4. Сравниваем
                string loadedJson = result.Config.ToJson();
                TestLogger.Write($"[DATA] Loaded JSON:\n{loadedJson}");

                if (!JsonSemanticEquals(originalJson, loadedJson))
                {
                    TestLogger.Write("[ERROR] JSON mismatch details:");
                    TestLogger.Write($"  Original length: {originalJson.Length}");
                    TestLogger.Write($"  Loaded length: {loadedJson.Length}");

                    // Простая проверка на наличие ключевых строк
                    if (!loadedJson.Contains("mod:intermediate_0"))
                        TestLogger.Write("  Missing: 'mod:intermediate_0'");
                    if (!loadedJson.Contains("sequence"))
                        TestLogger.Write("  Missing: 'sequence' array");

                    throw new Exception("JSON mismatch after roundtrip (см. лог выше)");
                }

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass | Duration: {stopwatch.ElapsedMilliseconds}ms\n");
                return TestResult.Pass("Roundtrip: Сложный рецепт", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | Error: {ex.Message}");
                TestLogger.Write($"[STACK] {ex.StackTrace}\n");
                return TestResult.Fail("Roundtrip: Сложный рецепт", ex.Message, stopwatch.Elapsed);
            }
        }


        private TestResult TestMetadataPreservation()
        {
            TestLogger.Write("[TEST] Start: Метаданные: Сохранение");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // 1. Подготовка данных
                TestLogger.Write("[STEP 1] Создание конфига и метаданных...");

                var config = new CustomRecipeConfig();
                config.CreateProperty("type", JsonType.String);
                config.SetValue("type", "test:type");

                // Данные для метаданных
                const string testName = "test_meta";
                const string testCategory = "test";
                const string testDescription = "Тестовое описание";
                const string testAuthor = "TestAuthor";

                TestLogger.Write($"[META] Name: '{testName}', Category: '{testCategory}'");
                TestLogger.Write($"[META] Description: '{testDescription}', Author: '{testAuthor}'");

                // 2. Сохранение шаблона
                TestLogger.Write("[STEP 2] Сохранение шаблона (SaveTemplate)...");

                var saved = TemplateManager.SaveTemplate(
                    testName,
                    config,
                    testCategory,
                    testDescription,
                    testAuthor
                );

                TestLogger.Write($"[SAVE] Result: {saved}");
                if (!saved) throw new Exception("SaveTemplate вернул false");

                // 🔧 Проверка: файл действительно создан
                var expectedPath = Path.Combine(TemplateManager.TemplatesFolder, testCategory, $"{testName}.json");
                var fileExists = File.Exists(expectedPath);
                TestLogger.Write($"[CHECK] Файл создан: {fileExists}, Путь: {expectedPath}");

                if (!fileExists) throw new Exception($"Файл шаблона не создан: {expectedPath}");

                // 🔧 Читаем сырой JSON для отладки
                var rawJson = File.ReadAllText(expectedPath);
                TestLogger.Write($"[DATA] Raw saved JSON (first 500 chars):\n{rawJson.Substring(0, Math.Min(500, rawJson.Length))}");

                // 3. Загрузка шаблона
                TestLogger.Write("[STEP 3] Загрузка шаблона (LoadTemplate)...");

                var result = TemplateManager.LoadTemplate(testName, testCategory);

                TestLogger.Write($"[LOAD] Success: {result.Success}");
                TestLogger.Write($"[LOAD] ErrorMessage: '{result.ErrorMessage ?? "none"}'");

                if (!result.Success) throw new Exception($"LoadTemplate failed: {result.ErrorMessage}");
                if (result.Config == null) throw new Exception("Config is null after load");
                if (result.Metadata == null) throw new Exception("Metadata is null after load");

                // 4. Проверка метаданных с детальным логом
                TestLogger.Write("[STEP 4] Валидация метаданных...");

                var meta = result.Metadata;

                TestLogger.Write($"[META LOADED] Name: '{meta.Name}', Category: '{meta.Category}'");
                TestLogger.Write($"[META LOADED] Description: '{meta.Description}', Author: '{meta.Author}'");

                // Проверка по полям с подробными ошибками
                if (meta.Author != testAuthor)
                {
                    TestLogger.Write($"[ERROR] Author mismatch: expected '{testAuthor}', got '{meta.Author}'");
                    throw new Exception($"Автор не сохранился: ожидалось '{testAuthor}', получено '{meta.Author}'");
                }
                TestLogger.Write($"[OK] Author: '{meta.Author}'");

                if (meta.Description != testDescription)
                {
                    TestLogger.Write($"[ERROR] Description mismatch: expected '{testDescription}', got '{meta.Description}'");
                    throw new Exception($"Описание не сохранилось: ожидалось '{testDescription}', получено '{meta.Description}'");
                }
                TestLogger.Write($"[OK] Description: '{meta.Description}'");

                if (meta.Category != testCategory)
                {
                    TestLogger.Write($"[ERROR] Category mismatch: expected '{testCategory}', got '{meta.Category}'");
                    throw new Exception($"Категория не сохранилась: ожидалось '{testCategory}', получено '{meta.Category}'");
                }
                TestLogger.Write($"[OK] Category: '{meta.Category}'");

                if (meta.Name != testName)
                {
                    TestLogger.Write($"[ERROR] Name mismatch: expected '{testName}', got '{meta.Name}'");
                    throw new Exception($"Имя не сохранилось: ожидалось '{testName}', получено '{meta.Name}'");
                }
                TestLogger.Write($"[OK] Name: '{meta.Name}'");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass | Duration: {stopwatch.ElapsedMilliseconds}ms\n");
                return TestResult.Pass("Метаданные: Сохранение", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | Error: {ex.Message}");
                TestLogger.Write($"[STACK] {ex.StackTrace}\n");
                return TestResult.Fail("Метаданные: Сохранение", ex.Message, stopwatch.Elapsed);
            }
        }


        private TestResult TestTemplateModeRequiredFields()
        {
            TestLogger.Write("[TEST] Start: Режим 1 (Шаблоны): Проверка очистки Required полей");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // 1. Создаём конфиг с ПОЛНЫМИ данными (как готовый рецепт)
                TestLogger.Write("[STEP 1] Создание конфига с полными данными...");

                var original = new CustomRecipeConfig();

                // Fixed поле (тип рецепта)
                original.CreateProperty("type", JsonType.String);
                original.SetValue("type", "create:sequenced_assembly");

                // Default поле (параметры)
                original.CreateProperty("loops", JsonType.Int);
                original.SetValue("loops", 3);

                // Required поля (конкретные предметы — должны очиститься в шаблоне)
                original.CreateProperty("ingredient", JsonType.Object);
                original.CreateProperty("ingredient.item", JsonType.String);
                original.SetValue("ingredient.item", "minecraft:diamond");  // ← Должно стать null/пусто

                original.CreateArray("results");
                var res0 = original.AddObjectToArray("results");
                original.CreateProperty($"{res0}.id", JsonType.String);
                original.SetValue($"{res0}.id", "create:transmitter");  // ← Должно стать null/пусто

                original.CreateProperty("transitionalItem", JsonType.Object);
                original.CreateProperty("transitionalItem.item", JsonType.String);
                original.SetValue("transitionalItem.item", "create:incomplete_transmitter");  // ← Должно стать null/пусто

                string originalJson = original.ToJson();
                TestLogger.Write($"[DATA] Original JSON (полные данные):\n{originalJson}");

                // 2. Сохраняем как ШАБЛОН (не рецепт!)
                TestLogger.Write("[STEP 2] Сохранение как ШАБЛОН (SaveTemplate)...");

                var saved = TemplateManager.SaveTemplate("test_template_mode", original, "create", "Тест шаблона", "Tester");
                TestLogger.Write($"[SAVE] Result: {saved}");
                if (!saved) throw new Exception("SaveTemplate вернул false");

                // Проверка: читаем сырой файл, чтобы увидеть классификацию
                var expectedPath = Path.Combine(TemplateManager.TemplatesFolder, "create", "test_template_mode.json");
                var rawJson = File.ReadAllText(expectedPath);
                TestLogger.Write($"[DATA] Raw template JSON (структура с классификацией):\n{rawJson.Substring(0, Math.Min(1000, rawJson.Length))}");

                // 3. Загружаем шаблон обратно
                TestLogger.Write("[STEP 3] Загрузка шаблона (LoadTemplate)...");

                var result = TemplateManager.LoadTemplate("test_template_mode", "create");
                TestLogger.Write($"[LOAD] Success: {result.Success}, Error: {result.ErrorMessage ?? "none"}");

                if (!result.Success) throw new Exception(result.ErrorMessage);
                if (result.Config == null) throw new Exception("Config is null");
                if (result.Paths == null) throw new Exception("Paths is null");

                // Логируем загруженные пути с их типами полей
                TestLogger.Write($"[DEBUG] Loaded {result.Paths.Count} paths с классификацией:");
                foreach (var p in result.Paths)
                {
                    var val = result.Config.GetValue(p.Path);
                    var valStr = val?.ToString() ?? "null";
                    TestLogger.Write($"  [PATH] {p.Path} = '{valStr}' (FieldType: {p.FieldType})");
                }

                // 4. Проверка: Fixed/Default сохранились, Required очистились
                TestLogger.Write("[STEP 4] Валидация классификации полей...");

                var config = result.Config;
                bool hasErrors = false;

                // Проверка Fixed: type должен сохраниться
                var typeValue = config.GetValue("type")?.ToString();
                TestLogger.Write($"[CHECK] type = '{typeValue}' (ожидалось: 'create:sequenced_assembly')");
                if (typeValue != "create:sequenced_assembly")
                {
                    TestLogger.Write($"[ERROR] Fixed поле 'type' не сохранилось!");
                    hasErrors = true;
                }
                else
                {
                    TestLogger.Write($"[OK] Fixed поле 'type' сохранилось");
                }

                // Проверка Default: loops должен сохраниться
                var loopsValue = config.GetValue("loops")?.ToString();
                TestLogger.Write($"[CHECK] loops = '{loopsValue}' (ожидалось: '3')");
                if (loopsValue != "3")
                {
                    TestLogger.Write($"[ERROR] Default поле 'loops' не сохранилось!");
                    hasErrors = true;
                }
                else
                {
                    TestLogger.Write($"[OK] Default поле 'loops' сохранилось");
                }

                // Проверка Required: ingredient.item должен быть null/пустым
                var ingredientItemValue = config.GetValue("ingredient.item")?.ToString();
                TestLogger.Write($"[CHECK] ingredient.item = '{ingredientItemValue}' (ожидалось: null или '')");
                if (!string.IsNullOrEmpty(ingredientItemValue))
                {
                    TestLogger.Write($"[ERROR] Required поле 'ingredient.item' НЕ очистилось! Значение: '{ingredientItemValue}'");
                    hasErrors = true;
                }
                else
                {
                    TestLogger.Write($"[OK] Required поле 'ingredient.item' очищено");
                }

                // Проверка Required: results[0].id должен быть null/пустым
                var resultsIdValue = config.GetValue("results[0].id")?.ToString();
                TestLogger.Write($"[CHECK] results[0].id = '{resultsIdValue}' (ожидалось: null или '')");
                if (!string.IsNullOrEmpty(resultsIdValue))
                {
                    TestLogger.Write($"[ERROR] Required поле 'results[0].id' НЕ очистилось! Значение: '{resultsIdValue}'");
                    hasErrors = true;
                }
                else
                {
                    TestLogger.Write($"[OK] Required поле 'results[0].id' очищено");
                }

                // Проверка Required: transitionalItem.item должен быть null/пустым
                var transitionalValue = config.GetValue("transitionalItem.item")?.ToString();
                TestLogger.Write($"[CHECK] transitionalItem.item = '{transitionalValue}' (ожидалось: null или '')");
                if (!string.IsNullOrEmpty(transitionalValue))
                {
                    TestLogger.Write($"[ERROR] Required поле 'transitionalItem.item' НЕ очистилось! Значение: '{transitionalValue}'");
                    hasErrors = true;
                }
                else
                {
                    TestLogger.Write($"[OK] Required поле 'transitionalItem.item' очищено");
                }

                // 5. Итог
                if (hasErrors)
                {
                    stopwatch.Stop();
                    TestLogger.Write($"[TEST] Fail | Required поля не очистились");
                    return TestResult.Fail("Режим 1 (Шаблоны): Проверка Required полей", "Required поля не были очищены при загрузке шаблона", stopwatch.Elapsed);
                }

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass | Duration: {stopwatch.ElapsedMilliseconds}ms\n");
                return TestResult.Pass("Режим 1 (Шаблоны): Проверка Required полей", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | Error: {ex.Message}");
                TestLogger.Write($"[STACK] {ex.StackTrace}\n");
                return TestResult.Fail("Режим 1 (Шаблоны): Проверка Required полей", ex.Message, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Семантическое сравнение JSON (игнорирует порядок свойств и пробелы)
        /// </summary>
        private bool JsonSemanticEquals(string json1, string json2)
        {
            try
            {
                var node1 = JsonNode.Parse(json1);
                var node2 = JsonNode.Parse(json2);

                // Сравниваем через нормализованную сериализацию
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = null // сохраняем имена как есть
                };

                return node1?.ToJsonString(options) == node2?.ToJsonString(options);
            }
            catch
            {
                // Если парсинг упал — сравниваем как строки (на крайний случай)
                return json1 == json2;
            }
        }
    }
}